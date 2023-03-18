﻿using Data.Common.Parsing;
using Irony.Parsing;
namespace Data.Common.FileQuery;

public abstract class FileQuery
{
    protected readonly ParseTreeNode node;
    private readonly FileCommand jsonCommand;

    protected FileQuery(ParseTreeNode node, FileCommand jsonCommand)
    {
        this.node = node;
        this.jsonCommand = jsonCommand;
        Filter = GetFilters();
        TableName = GetTable();
    }

    public string TableName { get; }
    public Filter? Filter { get; }

    public abstract string GetTable();
    public abstract IEnumerable<string> GetColumnNames();
    public virtual Filter? GetFilters()
    {
        var whereClause = node
            .ChildNodes
            .FirstOrDefault(item => item.Term.Name == "whereClauseOpt");
        if (whereClause?.ChildNodes.Count <= 0 || whereClause == null)
        {
            return null;
        }
        return ExtractFilter(whereClause!.ChildNodes[1].ChildNodes!);
    }

    protected Filter? ExtractFilter(ParseTreeNodeList x)
    {
        Filter? mainFilter = null;
        foreach (var item in x)
        {
            if (item.Term.Name == "binExpr")
            {
                mainFilter = ExtractFilter(item.ChildNodes);
            }
            else if (item.Term.Name == "Id")
            {
                var field = x[0].ChildNodes[0].Token.ValueString;
                if (x[0].ChildNodes.Count > 1)
                    field += "." + x[0].ChildNodes[1].Token.ValueString;

                var op = x[1].ChildNodes[0].Token.ValueString;
                //check if the query is parameterized
                object? value = GetValue(x);

                mainFilter = new SimpleFilter(field, op, value!);
                break;
            }
            else if (item.Term.Name == "binOp")
            {
                var next = x[2];
                var filter2 = ExtractFilter(next.ChildNodes);

                var op = item.ChildNodes[0].Token.ValueString;
                if (op.ToLower() == "and")
                {
                    if (next.Term.Name == "binExpr")
                        mainFilter = Filter.And(mainFilter!, filter2!);
                    else
                        mainFilter = Filter.AndAlso(mainFilter!, filter2!);
                }
                else
                {
                    if (next.Term.Name == "binExpr")
                        mainFilter = Filter.Or(mainFilter!, filter2!);
                    else
                        mainFilter = Filter.OrAlso(mainFilter!, filter2!);
                }
                break;
            }
        }
        return mainFilter;
    }

    protected object? GetValue(ParseTreeNodeList x)
    {
        object? value = string.Empty;
        if (x[0].Term.Name=="Parameter")
        {
            value = GetValue(x[0].ChildNodes);

        }
        else if (x[2].Term.Name.StartsWith("Unnamed"))
        {
            value = GetValue(x[2].ChildNodes);
        }
        else
            value = x[2].Token.Value;
        return value;

        object? GetValue(ParseTreeNodeList x)
        {
            object? value;
            string paramName = GetParamName(x);
            if (!jsonCommand.Parameters.Contains(paramName))
            {
                throw new InvalidOperationException($"Must declare the scalar variable \"@{paramName}\"");
            }
            var parameter = jsonCommand.Parameters[paramName].Convert<IDbDataParameter>();
            value = parameter.Value;
            return value;

             string GetParamName(ParseTreeNodeList x)
            {
                if (x[0].ChildNodes.Count==0)
                {
                    return x[0].Token.ValueString;
                }
                return x[0].ChildNodes[0].Token.ValueString;
            }
        }
    }

    public static FileQuery Create(FileCommand jsonCommand)
    {
        var parser = new Parser(new SqlGrammar());
        var parseTree = parser.Parse(jsonCommand.CommandText);
        if (parseTree.HasErrors())
        {
            ThrowHelper.ThrowSyntaxtErrorException(string.Join(Environment.NewLine, parseTree.ParserMessages));
        }
        var mainNode = parseTree.Root.ChildNodes[0];
        switch (mainNode.Term.Name)
        {
            case "insertStmt":
                return new FileInsertQuery(mainNode,
                                           jsonCommand);
            case "deleteStmt":
                return new FileDeleteQuery(mainNode, jsonCommand);
            case "updateStmt":
                return new FileUpdateQuery(mainNode, jsonCommand);
            case "selectStmt":
                return new FileSelectQuery(mainNode, jsonCommand);
        }

        throw ThrowHelper.GetQueryNotSupportedException();
    }

}
