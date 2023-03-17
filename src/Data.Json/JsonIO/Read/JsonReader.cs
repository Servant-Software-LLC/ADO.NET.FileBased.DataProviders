﻿namespace Data.Json.JsonIO.Read;
public class JsonReader : FileReader
{
    public JsonReader(FileConnection jsonConnection) 
        : 
        base(jsonConnection)
    {
    }
    private JsonDocument Read(string path)
    {
        //ThrowHelper.ThrowIfInvalidPath(path);
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            return JsonDocument.Parse(stream);
        }
    }
    protected override void ReadFromFolder(IEnumerable<string> tableNames)
    {

        foreach (var name in tableNames)
        {
            var path = fileConnection.GetTablePath(name);
            var doc = Read(path);
            var element = doc.RootElement;
            Json.JsonException.ThrowHelper.ThrowIfInvalidJson(element, fileConnection);
            var dataTable = CreateNewDataTable(element);
            dataTable.TableName = name;
            Fill(dataTable, element);
            DataSet!.Tables.Add(dataTable);
            doc.Dispose();
        }

    }
    protected override void UpdateFromFolder(string tableName)
    {
        var path = fileConnection.GetTablePath(tableName);
        var doc = Read(path);
        var element = doc.RootElement;
        Json.JsonException.ThrowHelper.ThrowIfInvalidJson(element, fileConnection);
        var dataTable = DataSet!.Tables[tableName];
        if (dataTable == null)
        {
            dataTable = CreateNewDataTable(element);
            dataTable.TableName = tableName;
            DataSet!.Tables.Add(dataTable);
        }
        dataTable!.Clear();
        Fill(dataTable, element);
        doc.Dispose();

    }
    #region File Read Update
    protected override void ReadFromFile()
    {
        var doc = Read(fileConnection.Database);
        var element = doc.RootElement;
        Json.JsonException.ThrowHelper.ThrowIfInvalidJson(element, fileConnection);
        var dataBaseEnumerator = element.EnumerateObject();
        DataSet = new DataSet();
        foreach (var item in dataBaseEnumerator)
        {
            var dataTable = CreateNewDataTable(item.Value);
            dataTable.TableName = item.Name;
            Fill(dataTable, item.Value);
            DataSet.Tables.Add(dataTable);
        }
        doc.Dispose();

    }
    protected override void UpdateFromFile()
    {
        DataSet!.Clear();

        var doc = Read(fileConnection.Database);
        var element = doc.RootElement;
        Json.JsonException.ThrowHelper.ThrowIfInvalidJson(element, fileConnection);
        foreach (DataTable item in DataSet.Tables)
        {
            var jsonElement = element.GetProperty(item.TableName);
            Fill(item, jsonElement);
        }
        doc.Dispose();

    }
    #endregion


    DataTable CreateNewDataTable(JsonElement jsonElement)
    {
        DataTable dataTable = new DataTable();
        foreach (var col in GetFields(jsonElement))
        {
            dataTable.Columns.Add(col.name, col.type);
        }

        return dataTable;
    }
 
    public IEnumerable<(string name, Type type)> GetFields(JsonElement table)
    {
        var maxFieldElement = table.EnumerateArray().MaxBy(x =>
        {
            return x.EnumerateObject().Count();
        });
        var enumerator = maxFieldElement.EnumerateObject();
        return enumerator.Select(x => (x.Name, x.Value.ValueKind.GetClrFieldType()));
    }
    internal void Fill(DataTable dataTable, JsonElement jsonElement)
    {
        //fill datatables
        foreach (var row in jsonElement.EnumerateArray())
        {
            var newRow = dataTable.NewRow();
            foreach (var field in row.EnumerateObject())
            {
                var val = field.Value.GetValue();
                if (val != null)
                    newRow[field.Name] = val;
            }
            dataTable.Rows.Add(newRow);
        }

    }
   
}
