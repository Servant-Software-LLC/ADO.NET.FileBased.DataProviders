﻿using System.Data.JsonClient;

namespace Data.Json.JsonIO.Write;

internal class JsonUpdate : Common.FileIO.Write.FileUpdateWriter
{
    public JsonUpdate(Common.FileStatements.FileUpdate fileStatement, FileConnection<JsonParameter> FileConnection, FileCommand<JsonParameter> FileCommand) 
        : base(fileStatement, FileConnection, FileCommand)
    {
        dataSetWriter = new JsonDataSetWriter(FileConnection, fileStatement);
    }
}
