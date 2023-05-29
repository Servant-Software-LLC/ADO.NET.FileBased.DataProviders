﻿namespace Data.Common.FileIO;

public abstract class FileWriter
{
    protected readonly IFileCommand fileCommand;
    protected readonly FileQuery fileQuery;
    protected readonly FileReader fileReader;
    protected readonly IFileTransaction? fileTransaction;
    protected IDataSetWriter? dataSetWriter { get; set; }

    public FileWriter(IFileConnection fileConnection,
                      IFileCommand fileCommand,
                      FileQuery fileQuery)
    {
        this.fileCommand = fileCommand;
        this.fileQuery = fileQuery;
        fileReader = fileConnection.FileReader;
        fileTransaction = fileCommand.FileTransaction;
    }

    public abstract int Execute();    
    internal static ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

    public bool IsTransaction
        => fileTransaction != null;
    public bool IsTransactedLater
        => fileTransaction != null && fileTransaction?.TransactionDone == false;

    public virtual bool Save()
    {
        if (IsTransactedLater)
        {
            return true;
        }
        dataSetWriter!.WriteDataSet(fileReader.DataSet!);
      
        return true;
    }
  
}
