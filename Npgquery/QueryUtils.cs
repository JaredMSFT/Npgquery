using System.Text;
using System.Text.Json;

namespace NpgqueryLib;

/// <summary>
/// Utility methods for working with PostgreSQL queries
/// </summary>
public static class QueryUtils
{
    /// <summary>
    /// Extract table names from a PostgreSQL query
    /// </summary>
    /// <param name="query">The SQL query</param>
    /// <returns>List of table names found in the query</returns>
    public static List<string> ExtractTableNames(string query)
    {
        var result = Npgquery.QuickParse(query);
        if (result.IsError || string.IsNullOrEmpty(result.ParseTree))
        {
            return new List<string>();
        }

        try
        {
            var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ExtractTablesFromJson(JsonDocument.Parse(result.ParseTree).RootElement, tables);
            return tables.ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Check if two queries have the same structure (same fingerprint)
    /// </summary>
    /// <param name="query1">First query</param>
    /// <param name="query2">Second query</param>
    /// <returns>True if queries have the same structure</returns>
    public static bool HaveSameStructure(string query1, string query2)
    {
        var fp1 = Npgquery.QuickFingerprint(query1);
        var fp2 = Npgquery.QuickFingerprint(query2);

        return fp1.IsSuccess && fp2.IsSuccess && 
               string.Equals(fp1.Fingerprint, fp2.Fingerprint, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get query type (SELECT, INSERT, UPDATE, DELETE, etc.)
    /// </summary>
    /// <param name="query">The SQL query</param>
    /// <returns>Query type or null if cannot be determined</returns>
    public static string? GetQueryType(string query)
    {
        var result = Npgquery.QuickParse(query);
        if (result.IsError || string.IsNullOrEmpty(result.ParseTree))
        {
            return null;
        }

        try
        {
            var doc = JsonDocument.Parse(result.ParseTree);
            return ExtractQueryTypeFromJson(doc.RootElement);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clean and format a PostgreSQL query
    /// </summary>
    /// <param name="query">The SQL query to clean</param>
    /// <returns>Cleaned and formatted query</returns>
    public static string CleanQuery(string query)
    {
        var normalized = Npgquery.QuickNormalize(query);
        return normalized.IsSuccess && !string.IsNullOrEmpty(normalized.NormalizedQuery) 
            ? normalized.NormalizedQuery 
            : query.Trim();
    }

    /// <summary>
    /// Validate multiple queries and return validation results
    /// </summary>
    /// <param name="queries">Queries to validate</param>
    /// <returns>Dictionary with query as key and validation result as value</returns>
    public static Dictionary<string, bool> ValidateQueries(IEnumerable<string> queries)
    {
        using var parser = new Npgquery();
        return queries.ToDictionary(q => q, parser.IsValid);
    }

    /// <summary>
    /// Get detailed error information for invalid queries
    /// </summary>
    /// <param name="queries">Queries to check</param>
    /// <returns>Dictionary with query as key and error message as value (null if valid)</returns>
    public static Dictionary<string, string?> GetQueryErrors(IEnumerable<string> queries)
    {
        using var parser = new Npgquery();
        return queries.ToDictionary(q => q, parser.GetError);
    }

    /// <summary>
    /// Split a multi-statement SQL string into individual statements
    /// </summary>
    /// <param name="sqlText">SQL text containing multiple statements</param>
    /// <returns>List of individual SQL statements</returns>
    public static List<string> SplitStatements(string sqlText)
    {
        var result = Npgquery.QuickSplit(sqlText);
        if (result.IsError || result.Statements == null)
        {
            return new List<string>();
        }

        return result.Statements
            .Where(s => !string.IsNullOrWhiteSpace(s.Statement))
            .Select(s => s.Statement!)
            .ToList();
    }

    /// <summary>
    /// Get all tokens from a PostgreSQL query
    /// </summary>
    /// <param name="query">The SQL query to tokenize</param>
    /// <returns>List of tokens</returns>
    public static List<SqlToken> GetTokens(string query)
    {
        var ret = new List<SqlToken>();
        var result = Npgquery.QuickScan(query);
        if (!result.IsSuccess || result.Tokens == null)
        {
            return new List<SqlToken>();
        }

        return ret;
    }
/*
    /// <summary>
    /// Get all token strings from a PostgreSQL query
    /// </summary>
    /// <param name="query">The SQL query to tokenize</param>
    /// <returns>List of tokens</returns>
    public static List<string> GetTokens(string query) {
        var result = Npgquery.QuickScan(query);
        if (!result.IsSuccess || result.Tokens == null) {
            return new List<string>();
        }

        return result.Tokens.ToList();
    }
*/
    /// <summary>
    /// Get all keywords from a PostgreSQL query
    /// </summary>
    /// <param name="query">The SQL query</param>
    /// <returns>List of SQL keywords found in the query</returns>
    public static List<string> GetKeywords(string query)
    {
        var tokens = GetTokens(query);
        return tokens
            .Where(t => !string.IsNullOrEmpty(t.KeywordKind))
            .Select(t => t.KeywordKind!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Convert AST back to SQL query
    /// </summary>
    /// <param name="parseTree">The AST JSON string</param>
    /// <returns>Deparsed SQL query or null if failed</returns>
    public static string? AstToSql(string parseTree)
    {
        var result = Npgquery.QuickDeparse(parseTree);
        return result.IsSuccess ? result.Query : null;
    }

    /// <summary>
    /// Round-trip test: parse a query and deparse it back
    /// </summary>
    /// <param name="query">Original SQL query</param>
    /// <returns>Tuple with success flag and the round-trip result</returns>
    public static (bool Success, string? RoundTripQuery) RoundTripTest(string query)
    {
        var parseResult = Npgquery.QuickParse(query);
        if (parseResult.IsError || string.IsNullOrEmpty(parseResult.ParseTree))
        {
            return (false, null);
        }

        var deparseResult = Npgquery.QuickDeparse(parseResult.ParseTree);
        if (deparseResult.IsError)
        {
            return (false, null);
        }

        return (true, deparseResult.Query);
    }

    /// <summary>
    /// Check if PL/pgSQL code is valid
    /// </summary>
    /// <param name="plpgsqlCode">The PL/pgSQL code to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidPlpgsql(string plpgsqlCode)
    {
        var result = Npgquery.QuickParsePlpgsql(plpgsqlCode);
        return result.IsSuccess;
    }

    /// <summary>
    /// Count the number of statements in a SQL string
    /// </summary>
    /// <param name="sqlText">SQL text that may contain multiple statements</param>
    /// <returns>Number of valid SQL statements</returns>
    public static int CountStatements(string sqlText)
    {
        var result = Npgquery.QuickSplit(sqlText);
        return result.IsSuccess && result.Statements != null 
            ? result.Statements.Count(s => !string.IsNullOrWhiteSpace(s.Statement))
            : 0;
    }

    /// <summary>
    /// Normalize multiple statements individually
    /// </summary>
    /// <param name="sqlText">SQL text containing multiple statements</param>
    /// <returns>Dictionary with original statement as key and normalized version as value</returns>
    public static Dictionary<string, string> NormalizeStatements(string sqlText)
    {
        var statements = SplitStatements(sqlText);
        var result = new Dictionary<string, string>();

        foreach (var statement in statements)
        {
            var normalized = CleanQuery(statement);
            result[statement] = normalized;
        }

        return result;
    }

    private static void ExtractTablesFromJson(JsonElement element, HashSet<string> tables)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals("relname", StringComparison.OrdinalIgnoreCase) && 
                    property.Value.ValueKind == JsonValueKind.String)
                {
                    var tableName = property.Value.GetString();
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tables.Add(tableName);
                    }
                }
                else if (property.Value.ValueKind == JsonValueKind.Object || 
                         property.Value.ValueKind == JsonValueKind.Array)
                {
                    ExtractTablesFromJson(property.Value, tables);
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractTablesFromJson(item, tables);
            }
        }
    }

    private static string? ExtractQueryTypeFromJson(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.EndsWith("Stmt", StringComparison.OrdinalIgnoreCase))
                {
                    return property.Name.Replace("Stmt", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant();
                }
                
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    var result = ExtractQueryTypeFromJson(property.Value);
                    if (result != null) return result;
                }
                else if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        var result = ExtractQueryTypeFromJson(item);
                        if (result != null) return result;
                    }
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var result = ExtractQueryTypeFromJson(item);
                if (result != null) return result;
            }
        }

        return null;
    }
}