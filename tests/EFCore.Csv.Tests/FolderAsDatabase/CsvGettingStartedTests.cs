﻿using Data.Common.Extension;
using EFCore.Csv.Tests.Utils;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Xunit;
using GettingStarted = EFCore.Common.Tests.GettingStartedTests<EFCore.Csv.Tests.Models.BloggingContext>;

namespace EFCore.Csv.Tests.FolderAsDatabase;

/// <summary>
/// Unit tests created based off of the Getting Started page in EF Core.  REF: https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
/// </summary>
public class CsvGettingStartedTests
{
    [Fact]
    public void Create_AddBlog()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        string connectionString = ConnectionStrings.Instance.gettingStartedFolderDB.Sandbox("Sandbox", sandboxId);
        GettingStarted.Create_AddBlog(connectionString);
    }

    [Fact]
    public void Read_FirstBlog()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        GettingStarted.Read_FirstBlog(ConnectionStrings.Instance.gettingStartedFolderDB.Sandbox("Sandbox", sandboxId));
    }

    [Fact]
    public void Update_UpdateBlogAddPost()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        GettingStarted.Update_UpdateBlogAddPost(ConnectionStrings.Instance.gettingStartedFolderDB.Sandbox("Sandbox", sandboxId));
    }

    [Fact]
    public void Delete_DeleteBlog()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        GettingStarted.Delete_DeleteBlog(ConnectionStrings.Instance.gettingStartedFolderDB.Sandbox("Sandbox", sandboxId).AddLogging(LogLevel.Debug));
    }

}
