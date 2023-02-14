﻿using Data.Json.Interfaces;
using System.Diagnostics;
using System.Globalization;

namespace Data.Json.Utils.ConnectionString;

public class JsonConnectionString : IConnectionStringProperties
{
    public string? ConnectionString { get; private set; }
    public string? DataSource { get; private set; }
    public bool Formatted { get; private set; } = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns>Null if connection string was parsed successfully and contained all required properties.  Otherewise, an error message.</returns>
    public void Parse(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        DbConnectionStringBuilder connectionStringBuilder = new();
        connectionStringBuilder.ConnectionString = connectionString;

        //DataSource
        if (TryGetValue(connectionStringBuilder, JsonConnectionStringKeywords.DataSource, out object? dataSource))
        {
            if (dataSource is not string sDataSource)
                throw new ArgumentException($"Invalid connection string: {nameof(JsonConnectionStringKeywords.DataSource)} was not a string.", nameof(connectionString));

            DataSource = sDataSource;
        }
        else
        {
            throw new ArgumentException($"Invalid connection string: {nameof(JsonConnectionStringKeywords.DataSource)} is a required connection string name/value pair.", nameof(connectionString));
        }

        foreach (KeyValuePair<string, object> keyValuePair in connectionStringBuilder)
        {
            //Any required keywords (like DataSource) have already been addressed above.
            if (IsKeyword(keyValuePair.Key, JsonConnectionStringKeywords.DataSource))
                continue;


            //Formatted
            if (IsKeyword(keyValuePair.Key, JsonConnectionStringKeywords.Formatted))
            {
                if (keyValuePair.Value == null)
                    throw new ArgumentException($"Invalid connection string: {nameof(JsonConnectionStringKeywords.Formatted)} was null.", nameof(connectionString));

                var bFormatted = ConvertToBoolean(keyValuePair.Value);
                if (!bFormatted.HasValue)
                    throw new ArgumentException($"Invalid connection string: {nameof(JsonConnectionStringKeywords.Formatted)} was not a boolean value.", nameof(connectionString));

                Formatted = bFormatted.Value;
                continue;
            }

            throw new ArgumentException($"Invalid connection string: Unknown keyword '{keyValuePair.Key}'", nameof(connectionString));
        }

        ConnectionString = connectionString;
    }


    private static bool? ConvertToBoolean(object value)
    {
        Debug.Assert(null != value, "ConvertToBoolean(null)");
        string? svalue = value as string;
        if (null != svalue)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(svalue, "true") || StringComparer.OrdinalIgnoreCase.Equals(svalue, "yes"))
                return true;
            else if (StringComparer.OrdinalIgnoreCase.Equals(svalue, "false") || StringComparer.OrdinalIgnoreCase.Equals(svalue, "no"))
                return false;
            else
            {
                string tmp = svalue.Trim();  // Remove leading & trailing white space.
                if (StringComparer.OrdinalIgnoreCase.Equals(tmp, "true") || StringComparer.OrdinalIgnoreCase.Equals(tmp, "yes"))
                    return true;
                else if (StringComparer.OrdinalIgnoreCase.Equals(tmp, "false") || StringComparer.OrdinalIgnoreCase.Equals(tmp, "no"))
                    return false;
            }
            return bool.Parse(svalue);
        }
        try
        {
            return ((IConvertible)value).ToBoolean(CultureInfo.InvariantCulture);
        }
        catch (InvalidCastException)
        {
            return null;
        }
    }

    private bool TryGetValue(DbConnectionStringBuilder connectionStringBuilder, JsonConnectionStringKeywords connectionStringKeyword, out object? value)
    {
        if (connectionStringBuilder.TryGetValue(connectionStringKeyword.ToString(), out value))
            return true;

        var aliases = GetAliases(connectionStringKeyword);
        foreach(var alias in aliases)
        {
            if (connectionStringBuilder.TryGetValue(alias, out value))
                return true;
        }

        return false;
    }

    private static bool IsKeyword(string keyword, JsonConnectionStringKeywords connectionStringKeyword) 
    {
        if (string.Compare(keyword, connectionStringKeyword.ToString(), true) == 0)
            return true;

        foreach (var aliasKeyword in GetAliases(connectionStringKeyword))
        {
            if (string.Compare(keyword, aliasKeyword, true) == 0)
                return true;
        }

        return false;
    }

    private static IEnumerable<string> GetAliases(JsonConnectionStringKeywords keyword)
    {
        var enumType = typeof(JsonConnectionStringKeywords);
        var memberInfos = enumType.GetMember(keyword.ToString());

        var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
        if (enumValueMemberInfo == null)
            yield break;

        var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(AliasAttribute), false);
        foreach(AliasAttribute valueAttribute in valueAttributes ) 
        {
            yield return valueAttribute.Name;
        }
    }
}
