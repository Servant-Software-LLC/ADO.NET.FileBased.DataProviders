﻿using Data.Json.Tests.FileAsDatabase;
using Data.Tests.Common;
using System.Data;
using System.Data.JsonClient;
using System.Reflection;
using Xunit;

namespace Data.Json.Tests.FolderAsDatabase;

public partial class JsonDataAdapterTests
{
    [Fact]
    public void DataAdapter_ShouldFillTheDataSet()
    {
        DataAdapterTests.DataAdapter_ShouldFillTheDataSet(
               () => new JsonConnection(ConnectionStrings.Instance.
               FolderAsDBConnectionString));
    }

    [Fact]
    public void Adapter_ShouldReturnData()
    {
        DataAdapterTests.Adapter_ShouldReturnData(
               () => new JsonConnection(ConnectionStrings.Instance.
               FolderAsDBConnectionString));
    }

    [Fact]
    public void DataAdapter_ShouldFillTheDataSet_WithFilter()
    {
        DataAdapterTests.DataAdapter_ShouldFillTheDataSet_WithFilter(
                       () => new JsonConnection(ConnectionStrings.Instance.
                       FolderAsDBConnectionString));
    }

    [Fact]
    public void Adapter_ShouldFillDatasetWithInnerJoinFromFolderAsDB()
    {
        DataAdapterTests.Adapter_ShouldFillDatasetWithInnerJoin(
                () => new JsonConnection(ConnectionStrings.Instance.eComFolderDBConnectionString));
    }

    [Fact]
    public void Adapter_ShouldReadDataWithSelectedColumns()
    {
        DataAdapterTests.Adapter_ShouldReadDataWithSelectedColumns(
                     () => new JsonConnection(ConnectionStrings.Instance.
                     FolderAsDBConnectionString));
    }

    [Fact]
    public void Update_DataAdapter_Should_Update_Existing_Row()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        DataAdapterTests.Update_DataAdapter_Should_Update_Existing_Row(
            () => new JsonConnection(ConnectionStrings.Instance.FolderAsDBConnectionString.Sandbox("Sandbox", sandboxId))
        );
    }

    [Fact]
    public void FillSchema_ShouldReturnDataTableWithAllColumns()
    {
        DataAdapterTests.FillSchema_ShouldReturnDataTableWithAllColumns(
             () => new JsonConnection(ConnectionStrings.Instance.
             FolderAsDBConnectionString));
    }

    [Fact]
    public void FillSchema_ShouldThrowInvalidOperationException_WhenSelectCommandIsNull()
    {
        DataAdapterTests.FillSchema_ShouldThrowInvalidOperationException_WhenSelectCommandIsNull(
              () => new JsonDataAdapter());
    }

    [Fact]
    public void FillSchema_ShouldThrowInvalidOperationException_WhenSelectCommandConnectionIsNull()
    {
        DataAdapterTests.FillSchema_ShouldThrowInvalidOperationException_WhenSelectCommandConnectionIsNull(
             () => new JsonConnection(ConnectionStrings.Instance.
             FolderAsDBConnectionString));
    }

    [Fact]
    public void CreateAdapter_ShouldThrowArgumentException_WhenSelectCommandTextIsNullOrEmpty()
    {
        DataAdapterTests.CreateAdapter_ShouldThrowArgumentException_WhenSelectCommandTextIsNullOrEmpty(
               () => new JsonConnection(ConnectionStrings.Instance.
               FolderAsDBConnectionString));
    }

    [Fact]
    public void GetFillParameters_ShouldReturnCorrectParametersForQueryWithoutParameters()
    {
        DataAdapterTests.GetFillParameters_ShouldReturnCorrectParametersForQueryWithoutParameters(
              () => new JsonConnection(ConnectionStrings.Instance.
              FolderAsDBConnectionString));
    }

    [Fact]
    public void GetFillParameters_ShouldReturnCorrectParametersForQueryWithParameters()
    {
        DataAdapterTests.GetFillParameters_ShouldReturnCorrectParametersForQueryWithParameters(
               () => new JsonConnection(ConnectionStrings.Instance.
               FolderAsDBConnectionString));
    }

    [Fact]
    public void GetFillParameters_ShouldReturnEmptyParametersForNonSelectQuery()
    {
        DataAdapterTests.GetFillParameters_ShouldReturnEmptyParametersForNonSelectQuery(
               () => new JsonConnection(ConnectionStrings.Instance.
               FolderAsDBConnectionString));
    }
}