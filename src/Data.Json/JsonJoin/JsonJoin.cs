﻿namespace Data.Json.JsonJoin
{

    public class DataTableJoin
    {
        private readonly IEnumerable<Join> dataTableInnerJoins;
        private readonly string mainTable;

        public DataTableJoin(IEnumerable<Join> dataTableInnerJoins, string mainTable)
        {
            this.dataTableInnerJoins = dataTableInnerJoins;
            this.mainTable = mainTable;
        }

        public DataTable Join(DataSet database)
        {
            var resultTable = new DataTable();
            foreach (DataTable item in database.Tables)
            {
                foreach (DataColumn col in item.Columns)
                {
                    if (resultTable.Columns[col.ColumnName] != null)
                    {
                        continue;
                    }
                    resultTable.Columns.Add(col.ColumnName, col.DataType);
                }
            }

            foreach (DataRow sourceRow in database.Tables[mainTable]!.Rows)
            {
                var rows = new List<DataRow>();
                foreach (var dataTableInnerJoin in dataTableInnerJoins)
                {
                    JoinRows(sourceRow,
                             resultTable,
                             dataTableInnerJoin,
                             database,
                             rows);
                }
                foreach (var item in rows)
                {
                    resultTable.Rows.Add(item);
                }
            }
            return resultTable;
        }
        public List<DataRow> JoinRows(DataRow sourceRow,
                                      DataTable resultTable,
                                      Join dataTableInnerJoin,
                                      DataSet database,
                                      List<DataRow> dataRows)
        {
            var sourceColumnVal = sourceRow[dataTableInnerJoin.SourceColumn];
            var dataTableToJoin = database.Tables[dataTableInnerJoin.TableName];

            var filter = new SimpleFilter(dataTableInnerJoin.JoinColumn, dataTableInnerJoin.Operation, sourceColumnVal);
            dataTableToJoin!.DefaultView.RowFilter = filter.Evaluate();


            foreach (DataRowView row in dataTableToJoin.DefaultView)
            {
                var resultRow = resultTable.NewRow();
                if (dataTableInnerJoin.InnerJoin.Count > 0)
                {
                    foreach (var innerjion in dataTableInnerJoin.InnerJoin)
                    {
                        var rows = new List<DataRow>();
                        var otherRows = JoinRows(row.Row,
                            resultTable,
                            innerjion,
                            database,
                            rows);
                        foreach (var item in otherRows)
                        {
                            AddRow(item, sourceRow.Table, sourceRow);
                            AddRow(item, dataTableToJoin!, row.Row);
                        }
                        dataRows.AddRange(rows);
                    }
                    continue;
                }
                AddRow(resultRow, sourceRow.Table, sourceRow);
                AddRow(resultRow, dataTableToJoin!, row.Row);
                dataRows.Add(resultRow);
            }

            return dataRows;
        }

        private static void AddRow(DataRow sourceRow, DataTable dataTableToJoin, DataRow row)
        {
            foreach (DataColumn col in dataTableToJoin.Columns)
            {
                sourceRow[col.ColumnName] = row[col.ColumnName];
            }
        }

    }
  



    public class Join
    {
        public string TableName { get; internal set; }
        public string JoinColumn { get; internal set; }
        public string Operation { get; } = "=";
        public string SourceColumn { get; internal set; }
        public IList<Join> InnerJoin { get; }

        public Join(string tableName, string joinColumn, string sourceColumn)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException($"'{nameof(tableName)}' cannot be null or empty.", nameof(tableName));
            }

            if (string.IsNullOrEmpty(joinColumn))
            {
                throw new ArgumentException($"'{nameof(joinColumn)}' cannot be null or empty.", nameof(joinColumn));
            }

            if (string.IsNullOrEmpty(sourceColumn))
            {
                throw new ArgumentException($"'{nameof(sourceColumn)}' cannot be null or empty.", nameof(sourceColumn));
            }

            InnerJoin = new List<Join>();
            TableName = tableName;
            JoinColumn = joinColumn;
            SourceColumn = sourceColumn;
        }

    }
}
