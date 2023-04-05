using Data.Json.Tests.FileAsDatabase;
using System.Data.JsonClient;
using Xunit;

namespace Data.Json.Tests.FolderAsDatabase;

/// <summary>
/// Tests that exercise the <see cref="JsonDataReader"/> class.
/// </summary>
public class JsonDataReaderTests
{
    [Fact]
    public void Reader_ShouldReadData()
    {
        DataReaderTests.Reader_ShouldReadData(() =>
        new JsonConnection(ConnectionStrings.Instance
        .FolderAsDBConnectionString));
    }

    [Fact]
    public void Reader_ShouldReturnData()
    {
        DataReaderTests.Reader_ShouldReturnData(() =>
       new JsonConnection(ConnectionStrings.Instance
       .FolderAsDBConnectionString));
    }

    [Fact]
    public void Reader_ShouldReturnData_WithFilter()
    {
        DataReaderTests.Reader_ShouldReturnData_WithFilter(() =>
       new JsonConnection(ConnectionStrings.Instance
       .FolderAsDBConnectionString));
    }

    [Fact]
    public void Reader_ShouldReadDataWithInnerJoin()
    {
        DataReaderTests.Reader_ShouldReadDataWithInnerJoin(() =>
       new JsonConnection(ConnectionStrings.Instance
       .eComFileDBConnectionString));
    }

    [Fact]
    public void Reader_ShouldReadDataWithSelectedColumns()
    {
        DataReaderTests.Reader_ShouldReadDataWithSelectedColumns(() =>
     new JsonConnection(ConnectionStrings.Instance
     .FolderAsDBConnectionString));
    }
}