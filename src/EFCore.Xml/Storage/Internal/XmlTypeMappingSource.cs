﻿using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Xml.Storage.Internal;

public class XmlTypeMappingSource : RelationalTypeMappingSource
{
    public XmlTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        // You can add more mappings or customize existing ones as needed.
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;

        if (storeTypeName != null)
        {
            switch (storeTypeName.ToLowerInvariant())
            {
                case "text":
                    return new StringTypeMapping("TEXT", System.Data.DbType.String);
            }
        }

        if (clrType != null)
        {
                return new StringTypeMapping("TEXT", System.Data.DbType.String);
        }

        return base.FindMapping(mappingInfo);
    }
}
