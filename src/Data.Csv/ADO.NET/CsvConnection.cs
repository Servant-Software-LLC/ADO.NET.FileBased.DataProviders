﻿using Data.Common.Utils.ConnectionString;
using Data.Csv.CsvIO;
using System.Data.Common;

namespace System.Data.CsvClient;


/// <summary>
/// Represents a connection for CSV operations.
/// </summary>
/// <inheritdoc/>
public class CsvConnection : FileConnection<CsvParameter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvConnection"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    public CsvConnection(string connectionString)
        : this(new FileConnectionString(connectionString)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvConnection"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    public CsvConnection(FileConnectionString connectionString)
        : base(connectionString)
    {
        FileReader = !AdminMode ? new CsvReader(this) : null;
    }

    /// <inheritdoc />
    public override string FileExtension => "csv";

    //public override bool DataTypeAlwaysString => true;

    /// <inheritdoc />
    public override void Open()
    {
        base.Open();

        EnsureNotFileAsDatabase();
    }

    /// <inheritdoc />
    public override void ChangeDatabase(string connString)
    {
        base.ChangeDatabase(connString);
        EnsureNotFileAsDatabase();
    }

    /// <inheritdoc/>
    private void EnsureNotFileAsDatabase()
    {
        if (PathType == PathType.File)
        {
            throw new InvalidOperationException("File as database is not supported in Csv Provider");

        }
    }

#if NET7_0_OR_GREATER    // .NET 7 implementation that supports covariant return types

    /// <inheritdoc/>
    public override CsvTransaction BeginTransaction() => new(this, default);

    /// <inheritdoc/>
    public override CsvTransaction BeginTransaction(IsolationLevel il) => BeginTransaction();

    /// <inheritdoc/>
    protected override CsvTransaction BeginDbTransaction(IsolationLevel il) => BeginTransaction(il);

    /// <inheritdoc/>
    public override CsvDataAdapter CreateDataAdapter(string query) => new(query, this);

    /// <inheritdoc/>
    public override CsvCommand CreateCommand() => new(this);

    /// <inheritdoc/>
    public override CsvCommand CreateCommand(string cmdText) => new(cmdText, this);

#else       // .NET Standard 2.0 compliant implementation.

    /// <inheritdoc/>
    public override FileTransaction<CsvParameter> BeginTransaction() => new CsvTransaction(this, default);

    /// <inheritdoc/>
    public override FileTransaction<CsvParameter> BeginTransaction(IsolationLevel il) => BeginTransaction();

    /// <inheritdoc/>
    protected override DbTransaction BeginDbTransaction(IsolationLevel il) => BeginTransaction(il);

    /// <inheritdoc/>
    public override FileDataAdapter<CsvParameter> CreateDataAdapter(string query) => new CsvDataAdapter(query, this);

    /// <inheritdoc/>
    public override FileCommand<CsvParameter> CreateCommand() => new CsvCommand(this);

    /// <inheritdoc/>
    public override FileCommand<CsvParameter> CreateCommand(string cmdText) => new CsvCommand(cmdText, this);

#endif

    /// <inheritdoc/>
    protected override void CreateFileAsDatabase(string databaseFileName) =>
        File.WriteAllText(databaseFileName, string.Empty);

    /// <inheritdoc/>
    protected override Func<FileStatement, IDataSetWriter> CreateDataSetWriter => fileStatement => new CsvDataSetWriter(this, fileStatement);
}
