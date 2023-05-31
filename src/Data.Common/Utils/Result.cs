﻿using Data.Common.FileIO.Write;

namespace Data.Common.Utils;

internal class Result
{
    public DataTable WorkingResultSet { get; }
    public FileEnumerator FileEnumerator { get; }

    public object?[]? CurrentDataRow { get; private set; }
    public string Statement { get; }

    public Result(FileStatement fileStatement, FileReader fileReader, Func<FileStatement, FileWriter> createWriter, Result previousWriteResult)
    {
        Statement = fileStatement.Statement;

        //If SELECT statement
        if (fileStatement is FileSelect fileSelect)
        {
            //Normal SELECT query with a FROM <table> clause
            if (!string.IsNullOrEmpty(fileStatement.TableName))
            {
                WorkingResultSet = fileReader.ReadFile(fileStatement, true);
            }
            else //SELECT query with no FROM clause
            {
                WorkingResultSet = GetSingleRowTable(fileSelect, previousWriteResult);
            }

            if (WorkingResultSet == null)
                throw new ArgumentNullException(nameof(WorkingResultSet));

            var filter = fileStatement!.Filter;
            if (filter is FuncFilter funcFilter)
            {
                if (previousWriteResult is null)
                    throw new ArgumentNullException(nameof(previousWriteResult), $"Cannot evaluate WHERE clause { funcFilter } because it contains a built-in function that depends on a previous SQL statement being either an INSERT, UPDATE or DELETE statement.");

                funcFilter.EvaluateFunction(previousWriteResult);
            }


            FileEnumerator = new FileEnumerator(fileStatement.GetColumnNames(), WorkingResultSet, filter);

            RecordsAffected = -1;
            return;
        }

        //If INSERT, UPDATE, or DELETE statement
        var fileWriter = createWriter(fileStatement);

        RecordsAffected = fileWriter.Execute();

        if (fileWriter is FileInsertWriter fileInsertWriter)
            LastInsertIdentity = fileInsertWriter.LastInsertIdentity;
    }

    public int FieldCount => FileEnumerator == null ? 0 : FileEnumerator.FieldCount;

    public bool IsClosed => FileEnumerator == null || !FileEnumerator.MoreRowsAvailable;

    public bool HasRows => FileEnumerator != null && FileEnumerator.HasRows;

    public int RecordsAffected { get; private set; }
    public object? LastInsertIdentity { get; private set; }

    public bool Read()
    {
        if (FileEnumerator == null)
            return false;

        if (FileEnumerator.MoveNext())
        {
            CurrentDataRow = FileEnumerator.Current;
            return true;
        }

        return false;
    }

    private DataTable GetSingleRowTable(FileSelect fileSelect, Result previousWriteResult)
    {
        var dataTable = new DataTable();
        var columns = fileSelect.GetColumnNames();

        foreach (var column in columns)
        {
            dataTable.Columns.Add(column);
        }

        var newRow = dataTable.NewRow();
        foreach (var column in columns)
        {
            //NOTE: This isn't the proper way to do this, but limited on time at the moment in trying to get the EF Core Providers working.
            //      We're assuming the column name is the name of the function.
            var value = BuiltinFunction.EvaluateFunction(column, previousWriteResult);
            if (value != null)
            {
                newRow[column] = value;
            }
        }
        dataTable.Rows.Add(newRow);

        return dataTable;
    }
}
