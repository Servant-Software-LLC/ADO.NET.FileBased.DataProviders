﻿using System.Reflection;
using Xunit;
using Data.Common.Extension;
using EFCore.Common.Tests;
using EFCore.Xml.Tests.Utils;
using EFCore.Xml.Design.Internal;

namespace EFCore.Xml.Tests.FileAsDatabase;

public class XmlScaffoldingTests
{
    [Fact]
    public void ValidateScaffolding()
    {
        var sandboxId = $"{GetType().FullName}.{MethodBase.GetCurrentMethod()!.Name}";
        var connectionString = ConnectionStrings.Instance.FileAsDB.Sandbox("Sandbox", sandboxId);
        ScaffoldingTests.ValidateScaffolding(connectionString,  new XmlDesignTimeServices());
    }

}
