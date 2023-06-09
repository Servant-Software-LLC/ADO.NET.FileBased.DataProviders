﻿using Microsoft.Extensions.Logging;

namespace Data.Common.FileIO.Read;

internal class FileEnumerator : IEnumerator<object?[]>
{
    private readonly ILogger log;
    private readonly DataView workingDataView;  // We cannot use the DefaultView of the DataTable, because workingResultset may be a pointer directly to
                                                // one of the tables (i.e. not created from the columns/joins of the SELECT query) and many JsonDataReader/JsonEnumerator
                                                // may be instantiated with different filters on them.
    private object?[] currentRow = Array.Empty<object>();
    private bool endOfResultset;

    public FileEnumerator(IEnumerable<string> resultSetColumnNames, DataTable workingResultset, Filter? filter, ILogger log)
    {
        if (workingResultset == null) 
            throw new ArgumentNullException(nameof(workingResultset));

        this.log = log;

        log.LogDebug($"FileEnumerator creating. Filter? {filter != null}");
        workingDataView = new DataView(workingResultset);
        if (filter != null)
        {
            try
            {
                var sFilter = filter.Evaluate();
                log.LogDebug($"Filter: {sFilter}");
                workingDataView.RowFilter = sFilter;
            }
            catch(Exception ex)
            {
                log.LogError($"Filter could not be applied to DataView.  Error: {ex}");
            }
        }

        Columns.AddRange(resultSetColumnNames);
        if (Columns.FirstOrDefault()?.Trim() == "*" && Columns != null)
        {
            Columns.Clear();
            foreach (DataColumn column in workingResultset.Columns)
            {
                Columns.Add(column.ColumnName);
            }
        }

        log.LogDebug($"FileEnumerator created.");
    }

    public object?[] Current => currentRow;
    object IEnumerator.Current => Current;
    public int CurrentIndex { get; private set; } = -1;
    public List<string> Columns { get; } = new List<string>();
    public int FieldCount => Columns.Count;

    public bool MoreRowsAvailable => workingDataView.Count > CurrentIndex;

    public bool HasRows => workingDataView.Count > 0;


    public bool MoveNext()
    {
        log.LogDebug($"FileEnumerator.MoveNext() called.  endofResultset = {endOfResultset}");

        if (endOfResultset)
            return false;

        CurrentIndex++;

        if (MoreRowsAvailable)
        {
            var row = workingDataView[CurrentIndex].Row;
            if (Columns?.FirstOrDefault()?.Trim() != "*")
            {
                currentRow = new object?[Columns!.Count];
                for (int i = 0; i < Columns?.Count; i++)
                {
                    currentRow[i] = row[Columns[i]];
                }
            }
            else
            {
                currentRow = row.ItemArray;
            }
            return true;
        }

        log.LogDebug($"End of resultset reached.");
        endOfResultset = true;
        return false;
    }

    public bool MoveNextInitial()
    {
        var res = MoveNext();
        Reset();
        return res;
    }

    public void Reset()
    {
        CurrentIndex = -1;
        //TableEnumerator.Reset();
    }

    public void Dispose()
    {
        //TableEnumerator?.Reset();
    }

    public string GetName(int i) => Columns[i];
    public int GetOrdinal(string name) => Columns.IndexOf(name);
    
    public Type GetType(int i)
    {
        var name = GetName(i);
        return workingDataView.Table!.Columns[name]!.DataType;
    }

}
