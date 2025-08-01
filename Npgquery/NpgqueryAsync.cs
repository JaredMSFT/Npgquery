using System.Text.Json;
using NpgqueryLib.Native;

namespace NpgqueryLib;

/// <summary>
/// Async extensions for PostgreSQL query parsing
/// </summary>
public static class NpgqueryAsync
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };

    /// <summary>
    /// Asynchronously parse a PostgreSQL query into an Abstract Syntax Tree (AST)
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="query">The SQL query to parse</param>
    /// <param name="options">Parse options (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parse result containing the AST or error information</returns>
    public static Task<ParseResult> ParseAsync(this Npgquery parser, string query, 
        ParseOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => parser.Parse(query, options), cancellationToken);
    }

    /// <summary>
    /// Asynchronously normalize a PostgreSQL query
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="query">The SQL query to normalize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Normalize result containing the normalized query or error information</returns>
    public static Task<NormalizeResult> NormalizeAsync(this Npgquery parser, string query, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => parser.Normalize(query), cancellationToken);
    }

    /// <summary>
    /// Asynchronously generate a fingerprint for a PostgreSQL query
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="query">The SQL query to fingerprint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fingerprint result containing the fingerprint hash or error information</returns>
    public static Task<FingerprintResult> FingerprintAsync(this Npgquery parser, string query, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => parser.Fingerprint(query), cancellationToken);
    }

    /// <summary>
    /// Asynchronously parse a PostgreSQL query and return the AST as a strongly-typed object
    /// </summary>
    /// <typeparam name="T">The type to deserialize the AST to</typeparam>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="query">The SQL query to parse</param>
    /// <param name="options">Parse options (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parsed AST as the specified type, or null if parsing failed</returns>
    public static Task<T?> ParseAsAsync<T>(this Npgquery parser, string query, 
        ParseOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        return Task.Run(() => parser.ParseAs<T>(query, options), cancellationToken);
    }

    /// <summary>
    /// Asynchronously validate that a PostgreSQL query has valid syntax
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="query">The SQL query to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the query is valid, false otherwise</returns>
    public static Task<bool> IsValidAsync(this Npgquery parser, string query, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => parser.IsValid(query), cancellationToken);
    }

    /// <summary>
    /// Process multiple queries in parallel
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="queries">The SQL queries to parse</param>
    /// <param name="options">Parse options (optional)</param>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of parse results</returns>
    public static async Task<ParseResult[]> ParseManyAsync(this Npgquery parser, IEnumerable<string> queries,
        ParseOptions? options = null, int maxDegreeOfParallelism = 4, 
        CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
        var tasks = queries.Select(async query =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await parser.ParseAsync(query, options, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Static async method for quick one-off parsing
    /// </summary>
    /// <param name="query">The SQL query to parse</param>
    /// <param name="options">Parse options (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parse result</returns>
    public static async Task<ParseResult> QuickParseAsync(string query, ParseOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        using var parser = new Npgquery();
        return await parser.ParseAsync(query, options, cancellationToken);
    }

    /// <summary>
    /// Static async method for quick one-off normalization
    /// </summary>
    /// <param name="query">The SQL query to normalize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Normalize result</returns>
    public static async Task<NormalizeResult> QuickNormalizeAsync(string query, 
        CancellationToken cancellationToken = default)
    {
        using var parser = new Npgquery();
        return await parser.NormalizeAsync(query, cancellationToken);
    }

    /// <summary>
    /// Static async method for quick one-off fingerprinting
    /// </summary>
    /// <param name="query">The SQL query to fingerprint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fingerprint result</returns>
    public static async Task<FingerprintResult> QuickFingerprintAsync(string query, 
        CancellationToken cancellationToken = default)
    {
        using var parser = new Npgquery();
        return await parser.FingerprintAsync(query, cancellationToken);
    }

    /// <summary>
    /// Asynchronously deparse a PostgreSQL AST back to SQL
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="parseTree">The AST JSON string to deparse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deparse result containing the SQL query or error information</returns>
    public static Task<DeparseResult> DeparseAsync(this Npgquery parser, JsonDocument parseTree, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => parser.Deparse(parseTree), cancellationToken);
    }

    /// <summary>
    /// Asynchronously split multiple PostgreSQL statements
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="query">The SQL string containing multiple statements</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Split result containing individual statements or error information</returns>
    public static Task<SplitResult> SplitAsync(this Npgquery parser, string query, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => parser.Split(query), cancellationToken);
    }

    /// <summary>
    /// Asynchronously scan/tokenize a PostgreSQL query
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="query">The SQL query to scan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan result containing tokens or error information</returns>
    public static Task<ScanResult> ScanAsync(this Npgquery parser, string query, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => parser.Scan(query), cancellationToken);
    }

    /// <summary>
    /// Asynchronously parse PL/pgSQL code
    /// </summary>
    /// <param name="parser">The Npgquery instance</param>
    /// <param name="plpgsqlCode">The PL/pgSQL code to parse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PL/pgSQL parse result containing the AST or error information</returns>
    public static Task<PlpgsqlParseResult> ParsePlpgsqlAsync(this Npgquery parser, string plpgsqlCode, 
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => parser.ParsePlpgsql(plpgsqlCode), cancellationToken);
    }

    /// <summary>
    /// Static async method for quick one-off deparsing
    /// </summary>
    /// <param name="parseTree">The AST JSON to deparse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deparse result</returns>
    public static async Task<DeparseResult> QuickDeparseAsync(JsonDocument parseTree, 
        CancellationToken cancellationToken = default)
    {
        using var parser = new Npgquery();
        return await parser.DeparseAsync(parseTree, cancellationToken);
    }

    /// <summary>
    /// Static async method for quick one-off splitting
    /// </summary>
    /// <param name="query">The SQL string to split</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Split result</returns>
    public static async Task<SplitResult> QuickSplitAsync(string query, 
        CancellationToken cancellationToken = default)
    {
        using var parser = new Npgquery();
        return await parser.SplitAsync(query, cancellationToken);
    }

    /// <summary>
    /// Static async method for quick one-off scanning
    /// </summary>
    /// <param name="query">The SQL query to scan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan result</returns>
    public static async Task<ScanResult> QuickScanAsync(string query, 
        CancellationToken cancellationToken = default)
    {
        using var parser = new Npgquery();
        return await parser.ScanAsync(query, cancellationToken);
    }

    /// <summary>
    /// Static async method for quick one-off PL/pgSQL parsing
    /// </summary>
    /// <param name="plpgsqlCode">The PL/pgSQL code to parse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PL/pgSQL parse result</returns>
    public static async Task<PlpgsqlParseResult> QuickParsePlpgsqlAsync(string plpgsqlCode, 
        CancellationToken cancellationToken = default)
    {
        using var parser = new Npgquery();
        return await parser.ParsePlpgsqlAsync(plpgsqlCode, cancellationToken);
    }
}