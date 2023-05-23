﻿namespace System.Data.FileClient;

public abstract class FileDataAdapter<TFileParameter> : IDataAdapter, IDisposable
    where TFileParameter : FileParameter<TFileParameter>, new()
{
    private DataRow? lastDataRowChanged;
    private FileConnection<TFileParameter>? connection;

    public FileDataAdapter()
    {
    }

    public FileDataAdapter(string query, FileConnection<TFileParameter> connection)
    {
        if (string.IsNullOrEmpty(query))
        {
            throw new ArgumentException($"'{nameof(query)}' cannot be null or empty.", nameof(query));
        }
        this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        SelectCommand = connection.CreateCommand();
        SelectCommand.Connection = connection;
        SelectCommand.CommandText = query;
    }

    public FileDataAdapter(FileCommand<TFileParameter> selectCommand)
    {
        SelectCommand = selectCommand ?? throw new ArgumentNullException(nameof(selectCommand));

        if (SelectCommand.Connection == null)
            throw new ArgumentNullException(nameof(connection));

        if (SelectCommand.Connection is not FileConnection<TFileParameter> selectConnection)
            throw new ArgumentException($"{nameof(SelectCommand)}.{nameof(SelectCommand.Connection)} is not a {nameof(FileConnection<TFileParameter>)}");

        if (selectConnection.AdminMode)
            throw new ArgumentException($"The {GetType()} cannot be used with an admin connection.");

        connection = selectConnection;

        if (string.IsNullOrEmpty(SelectCommand.CommandText))
        {
            throw new ArgumentException($"'{nameof(FileCommand<TFileParameter>.CommandText)}' cannot be null or empty.", nameof(SelectCommand.CommandText));
        }
    }

    public IDbCommand? SelectCommand { get; set; }
    public IDbCommand? UpdateCommand { get; set; }

    public MissingMappingAction MissingMappingAction { get; set; }
    public MissingSchemaAction MissingSchemaAction { get; set; }
    public ITableMappingCollection TableMappings { get; }

    public int Fill(DataSet dataSet)
    {
        if (connection == null)
            throw new InvalidOperationException($"A connection cannot be inferred when calling the {nameof(Fill)} method");

        if (connection.AdminMode)
            throw new ArgumentException($"The {GetType()} cannot be used with an admin connection.");

        if (SelectCommand == null)
            throw new InvalidOperationException($"{nameof(SelectCommand)} is not set.");

        if (SelectCommand.Connection == null)
            throw new InvalidOperationException($"{nameof(SelectCommand.Connection)} property on {nameof(SelectCommand)} is not set.");

        if (string.IsNullOrEmpty(SelectCommand.CommandText))
            throw new InvalidOperationException($"{nameof(SelectCommand.CommandText)} property on {nameof(SelectCommand)} is not set.");

        var selectQuery = FileQuery<TFileParameter>.Create((FileCommand<TFileParameter>)SelectCommand);
        var fileReader = connection.FileReader;
        var dataTable = fileReader.ReadFile(selectQuery, true);
        dataTable = GetTable(dataTable, selectQuery);

        var cols = GetColumns(dataTable, selectQuery);
        dataTable.Columns
            .Cast<DataColumn>()
            .Where(column => !cols.Contains(column.ColumnName))
            .Select(column => column.ColumnName)
            .ToList()
            .ForEach(dataTable.Columns.Remove);

        dataTable.RowChanged += DataTable_RowChanged;

        dataTable.TableName = "Table";
        dataSet.Tables.Clear();
        dataSet.Tables.Add(dataTable);
        return dataTable.Rows.Count;
    }

    public DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
    {
        if (SelectCommand == null)
            throw new InvalidOperationException($"{nameof(SelectCommand)} is not set.");

        if (SelectCommand.Connection == null)
            throw new InvalidOperationException($"{nameof(SelectCommand.Connection)} property on {nameof(SelectCommand)} is not set.");

        if (string.IsNullOrEmpty(SelectCommand.CommandText))
            throw new InvalidOperationException($"{nameof(SelectCommand.CommandText)} property on {nameof(SelectCommand)} is not set.");

        connection = (FileConnection<TFileParameter>)SelectCommand.Connection;

        if (connection.AdminMode)
            throw new ArgumentException($"The {GetType()} cannot be used with an admin connection.");

        var selectQuery = FileQuery<TFileParameter>.Create((FileCommand<TFileParameter>)SelectCommand);
        var fileReader = connection.FileReader;
        var dataTable = fileReader.ReadFile(selectQuery, true);
        var cols = GetColumns(dataTable, selectQuery);
        dataTable.Columns
            .Cast<DataColumn>()
            .Where(column => !cols.Contains(column.ColumnName))
            .Select(column => column.ColumnName)
            .ToList()
            .ForEach(dataTable.Columns.Remove);

        if (dataSet.Tables["Table"] != null)
        {
            dataSet.Tables.Remove("Table");
        }

        dataTable.Rows.Clear();
        return new DataTable[] { dataTable };
    }

    public IDataParameter[] GetFillParameters()
    {
        IDataParameter[]? value = null;
        if (SelectCommand != null)
        {
            IDataParameterCollection parameters = SelectCommand.Parameters;
            if (parameters != null)
            {
                value = new IDataParameter[parameters.Count];
                parameters.CopyTo(value, 0);
            }
        }
        if (value == null)
        {
            value = Array.Empty<IDataParameter>();
        }
        return value;
    }

    public int Update(DataSet dataSet)
    {
        // dataSet.Tables.Clear();
        if (dataSet == null)
            throw new ArgumentNullException($"{nameof(dataSet)} cannot be null");

        if (UpdateCommand == null)
            throw new InvalidOperationException($"Update requires a valid UpdateCommand when passed DataRow collection with modified rows.");

        if (UpdateCommand.Connection == null)
            throw new InvalidOperationException($"Update requires the UpdateCommand to have a connection object. The Connection property of the UpdateCommand has not been initialized.");

        if (string.IsNullOrEmpty(UpdateCommand.CommandText))
            throw new InvalidOperationException($"{nameof(UpdateCommand.CommandText)} property on {nameof(UpdateCommand)} is not set.");

        connection = (FileConnection<TFileParameter>)UpdateCommand!.Connection!;

        if (connection.AdminMode)
            throw new ArgumentException($"The {GetType()} cannot be used with an admin connection.");

        var query = FileQuery<TFileParameter>.Create((FileCommand<TFileParameter>)UpdateCommand);
        if (query is not FileUpdateQuery<TFileParameter> updateQuery)
        {
            throw new QueryNotSupportedException("This query is not yet supported via DataAdapter");
        }

        //check if source column is set and has parameters
        if (UpdateCommand.Parameters.Count > 0)
        {
            foreach (IDbDataParameter parameter in UpdateCommand.Parameters)
            {
                if (!string.IsNullOrEmpty(parameter.SourceColumn)
                    &&
                    lastDataRowChanged != null
                    &&
                    lastDataRowChanged.Table.Columns.Contains(parameter.SourceColumn))
                {
                    parameter.Value = lastDataRowChanged[parameter.SourceColumn];
                }
            }
        }

        var updater = CreateWriter(query);
        return updater.Execute();
    }

    protected abstract FileWriter<TFileParameter> CreateWriter(FileQuery<TFileParameter> fileQuery);

    private void DataTable_RowChanged(object sender, DataRowChangeEventArgs e)
    {
        if (e.Action == DataRowAction.Change)
        {
            lastDataRowChanged = e.Row;
        }
    }

    private DataTable GetTable(DataTable dataTable, FileQuery<TFileParameter> query)
    {
        var filters = query.GetFilters();
        var view = new DataView(dataTable);
        if (filters != null)
        {
            view.RowFilter = filters.Evaluate();
        }
        return view.ToTable();
    }

    private IEnumerable<string> GetColumns(DataTable dataTable, FileQuery<TFileParameter> query)
    {
        var cols = query.GetColumnNames()
            .ToList();

        if (cols!.FirstOrDefault()?.Trim() == "*" && cols != null)
        {
            cols.Clear();
            foreach (DataColumn column in dataTable.Columns)
            {
                cols.Add(column.ColumnName);
            }
        }

        return cols!;
    }

    public void Dispose()
    {
    }
}