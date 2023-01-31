﻿using Data.Json.Enum;

namespace Data.Json.Extension
{
    public static class ExtensionMethods
    {
        public static TTarget Convert<TTarget>(this object source)
        {
            return (TTarget)source;
        }
        public static TTarget Convert<TTarget>(this string source)
        {
          return (TTarget)System.Convert.ChangeType(source, typeof(TTarget));
        }
        public static Type GetClrFieldType(this JsonValueKind kind) => kind switch
        {
            JsonValueKind.String => typeof(string),
            JsonValueKind.Number => typeof(decimal),
            JsonValueKind.True => typeof(bool),
            JsonValueKind.False => typeof(bool),
            JsonValueKind.Null => typeof(string),
            JsonValueKind.Array => typeof(string),
            JsonValueKind.Object => typeof(string),
            _ => throw new NotSupportedException("Unsupported JSON value kind: " + kind)
        };

        public static string GetClrDataTypeName(this JsonValueKind kind) => kind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => "decimal",
            JsonValueKind.True => "bool",
            JsonValueKind.False => "bool",
            JsonValueKind.Null => "null",
            JsonValueKind.Array => "string",
            JsonValueKind.Object => "string",
            _ => throw new NotSupportedException("Unsupported JSON value kind: " + kind)
        };
        public static object? GetValue(this JsonElement row, string propName)
        {
            var jsonElement = row.EnumerateObject().First(x => x.Name == propName).Value;
            return GetValue(jsonElement);
        }
        public static object? GetValue(this JsonElement jsonElement)
        {
            var kind = jsonElement.ValueKind;
            return kind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.GetDecimal(),
                JsonValueKind.Undefined => string.Empty,
                JsonValueKind.Array => jsonElement.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Object => jsonElement.GetRawText(),
                JsonValueKind.Null => null,
                _ => throw new NotImplementedException()
            };

        }

        public static DataTable ToDataTable(this JsonDocument dataTable)
        {
            return null;
        }
       public static PathType GetPathType(this JsonConnection jsonConnection)
        {
            if (File.Exists(jsonConnection.ConnectionString))
            {
                return PathType.File;
            }
            else if (Directory.Exists(jsonConnection.ConnectionString))
            {
                return PathType.Directory;
            }
                return PathType.None;
        }
    }
}
