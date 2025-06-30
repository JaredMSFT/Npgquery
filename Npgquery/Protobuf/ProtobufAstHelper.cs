using Google.Protobuf;
using PgQuery;
using System.Text.Json;

namespace NpgqueryLib.Protobuf;

/// <summary>
/// Utilities for working with protobuf-generated PostgreSQL AST structures
/// </summary>
public static class ProtobufAstHelper
{
    /// <summary>
    /// Convert a protobuf ParseResult to JSON string
    /// </summary>
    /// <param name="parseResult">The protobuf ParseResult</param>
    /// <param name="formatted">Whether to format the JSON with indentation</param>
    /// <returns>JSON representation of the parse tree</returns>
    public static string ToJson(PgQuery.ParseResult parseResult, bool formatted = false)
    {
        var jsonFormatter = new JsonFormatter(JsonFormatter.Settings.Default.WithFormatDefaultValues(true));
        var json = jsonFormatter.Format(parseResult);
        
        if (formatted)
        {
            var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
        }
        
        return json;
    }

    /// <summary>
    /// Convert a protobuf ScanResult to JSON string
    /// </summary>
    /// <param name="scanResult">The protobuf ScanResult</param>
    /// <param name="formatted">Whether to format the JSON with indentation</param>
    /// <returns>JSON representation of the scan result</returns>
    public static string ToJson(PgQuery.ScanResult scanResult, bool formatted = false)
    {
        var jsonFormatter = new JsonFormatter(JsonFormatter.Settings.Default.WithFormatDefaultValues(true));
        var json = jsonFormatter.Format(scanResult);
        
        if (formatted)
        {
            var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
        }
        
        return json;
    }

    /// <summary>
    /// Parse JSON string back to ParseResult
    /// </summary>
    /// <param name="json">JSON representation of parse tree</param>
    /// <returns>Protobuf ParseResult object</returns>
    public static PgQuery.ParseResult ParseResultFromJson(string json)
    {
        var parser = new JsonParser(JsonParser.Settings.Default);
        return parser.Parse<PgQuery.ParseResult>(json);
    }

    /// <summary>
    /// Parse JSON string back to ScanResult
    /// </summary>
    /// <param name="json">JSON representation of scan result</param>
    /// <returns>Protobuf ScanResult object</returns>
    public static PgQuery.ScanResult ScanResultFromJson(string json)
    {
        var parser = new JsonParser(JsonParser.Settings.Default);
        return parser.Parse<PgQuery.ScanResult>(json);
    }

    /// <summary>
    /// Extract all SELECT statements from a ParseResult
    /// </summary>
    /// <param name="parseResult">The parse result to search</param>
    /// <returns>Collection of SelectStmt objects</returns>
    public static IEnumerable<SelectStmt> ExtractSelectStatements(PgQuery.ParseResult parseResult)
    {
        foreach (var stmt in parseResult.Stmts)
        {
            if (stmt.Stmt?.SelectStmt != null)
            {
                yield return stmt.Stmt.SelectStmt;
            }
        }
    }

    /// <summary>
    /// Extract all table names from a ParseResult
    /// </summary>
    /// <param name="parseResult">The parse result to search</param>
    /// <returns>Collection of table names</returns>
    public static IEnumerable<string> ExtractTableNames(PgQuery.ParseResult parseResult)
    {
        var tableNames = new HashSet<string>();
        
        foreach (var stmt in parseResult.Stmts)
        {
            ExtractTableNamesFromNode(stmt.Stmt, tableNames);
        }
        
        return tableNames;
    }

    /// <summary>
    /// Recursively extract table names from a node
    /// </summary>
    /// <param name="node">Node to search</param>
    /// <param name="tableNames">Set to collect table names</param>
    private static void ExtractTableNamesFromNode(Node node, HashSet<string> tableNames)
    {
        // This is a simplified implementation - a full implementation would need to
        // traverse all possible node types that could contain table references
        switch (node.NodeCase)
        {
            case Node.NodeOneofCase.RangeVar:
                if (!string.IsNullOrEmpty(node.RangeVar.Relname))
                {
                    tableNames.Add(node.RangeVar.Relname);
                }
                break;
                
            case Node.NodeOneofCase.SelectStmt:
                var selectStmt = node.SelectStmt;
                foreach (var fromItem in selectStmt.FromClause)
                {
                    ExtractTableNamesFromNode(fromItem, tableNames);
                }
                break;
                
            case Node.NodeOneofCase.RawStmt:
                if (node.RawStmt.Stmt != null)
                {
                    ExtractTableNamesFromNode(node.RawStmt.Stmt, tableNames);
                }
                break;
        }
    }

    /// <summary>
    /// Get statement type from a RawStmt
    /// </summary>
    /// <param name="rawStmt">The statement to analyze</param>
    /// <returns>Statement type as string</returns>
    public static string GetStatementType(RawStmt rawStmt)
    {
        if (rawStmt.Stmt == null) return "UNKNOWN";
        
        return rawStmt.Stmt.NodeCase switch
        {
            Node.NodeOneofCase.SelectStmt => "SELECT",
            Node.NodeOneofCase.InsertStmt => "INSERT", 
            Node.NodeOneofCase.UpdateStmt => "UPDATE",
            Node.NodeOneofCase.DeleteStmt => "DELETE",
            Node.NodeOneofCase.CreateStmt => "CREATE",
            Node.NodeOneofCase.AlterTableStmt => "ALTER",
            Node.NodeOneofCase.DropStmt => "DROP",
            Node.NodeOneofCase.MergeStmt => "MERGE",
            Node.NodeOneofCase.CallStmt => "CALL",
            Node.NodeOneofCase.DoStmt => "DO",
            _ => rawStmt.Stmt.NodeCase.ToString().Replace("Stmt", "").ToUpperInvariant()
        };
    }

    /// <summary>
    /// Count the number of statements in a ParseResult
    /// </summary>
    /// <param name="parseResult">The parse result</param>
    /// <returns>Number of statements</returns>
    public static int CountStatements(PgQuery.ParseResult parseResult)
    {
        return parseResult.Stmts.Count;
    }

    /// <summary>
    /// Check if a ParseResult contains any DDL statements
    /// </summary>
    /// <param name="parseResult">The parse result to check</param>
    /// <returns>True if contains DDL statements</returns>
    public static bool ContainsDdlStatements(PgQuery.ParseResult parseResult)
    {
        return parseResult.Stmts.Any(stmt => IsDdlStatement(stmt));
    }

    /// <summary>
    /// Check if a statement is a DDL statement
    /// </summary>
    /// <param name="rawStmt">The statement to check</param>
    /// <returns>True if it's a DDL statement</returns>
    private static bool IsDdlStatement(RawStmt rawStmt)
    {
        if (rawStmt.Stmt == null) return false;
        
        return rawStmt.Stmt.NodeCase switch
        {
            Node.NodeOneofCase.CreateStmt => true,
            Node.NodeOneofCase.AlterTableStmt => true,
            Node.NodeOneofCase.DropStmt => true,
            Node.NodeOneofCase.CreateSchemaStmt => true,
            Node.NodeOneofCase.CreateSeqStmt => true,
            Node.NodeOneofCase.CreateFunctionStmt => true,
            Node.NodeOneofCase.CreateTrigStmt => true,
            Node.NodeOneofCase.IndexStmt => true,
            _ => false
        };
    }
}