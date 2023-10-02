using Data.Common.Extension;
using Data.Tests.Common;
using System.Data.CsvClient;
using System.Reflection;
using Xunit;

namespace Data.Csv.Tests.FolderAsDatabase;

/// <summary>
/// These tests indirectly exercise the <see cref="JsonIO.Write.JsonInsert"/> class via calls to <see cref="CsvCommand.ExecuteNonQuery" />.
/// </summary>
public class CsvInsertTests
{
    [Fact]
    public void Insert_ShouldInsertData()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        InsertTests.Insert_ShouldInsertData(() => new CsvConnection(ConnectionStrings.Instance.FolderAsDB.Sandbox("Sandbox", sandboxId)));
    }

    [Fact]
    public void Insert_ShouldInsertNullData()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        InsertTests.Insert_ShouldInsertNullData(() => new CsvConnection(ConnectionStrings.Instance.FolderAsDB.Sandbox("Sandbox", sandboxId)), true);
    }

    [Fact]
    public void Insert_JsonShouldBeFormatted()
    {
        InsertTests.Insert_ShouldBeFormatted(() =>
        new CsvConnection(ConnectionStrings.Instance
        .FolderAsDB.AddFormatted(true)));
    }

    [Fact]
    public void Insert_IndentityColumn_NoLastRow()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        InsertTests.Insert_IndentityColumn_NoLastRow(
            () => new CsvConnection(ConnectionStrings.Instance.gettingStartedFolderDB.Sandbox("Sandbox", sandboxId)),
        true);
    }

    [Fact]
    public void Insert_IndentityColumn_LastRow()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        InsertTests.Insert_IndentityColumn_LastRow_Decimal(
            () => new CsvConnection(ConnectionStrings.Instance.FolderAsDB.Sandbox("Sandbox", sandboxId)),
        true);
    }
}