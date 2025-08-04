using Google.Protobuf;
using NpgqueryLib.Native;
using PgQuery;
using System.Text.Json;
using static NpgqueryLib.Native.NativeMethods;

namespace NpgqueryLib;

/// <summary>
/// Main PostgreSQL query parser class providing parsing, normalization, and fingerprinting functionality
/// </summary>
public sealed class Npgquery : IDisposable {
    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };

    private bool _disposed;

    /// <summary>
    /// Parse a PostgreSQL query into an Abstract Syntax Tree (AST)
    /// </summary>
    /// <param name="query">The SQL query to parse</param>
    /// <param name="options">Parse options (optional)</param>
    /// <returns>Parse result containing the AST or error information</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed</exception>
    public ParseResult Parse(string query, ParseOptions? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Npgquery));
        
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        
        NativeMethods.PgQueryResult nativeResult = default;
        try
        {
            // Call native parse function
            var inputBytes = NativeMethods.StringToUtf8Bytes(query);
            nativeResult = NativeMethods.pg_query_parse(inputBytes);
            
            // Check if there's an error using MarshalError
            if (nativeResult.error != IntPtr.Zero)
            {
                var error = NativeMethods.MarshalError(nativeResult.error);
                string errorMessage = "Parse error";
                
                if (error?.message != IntPtr.Zero)
                {
                    errorMessage = NativeMethods.PtrToString(error.Value.message) ?? "Parse error";
                }
                
                return new ParseResult 
                { 
                    Query = query,
                    Error = errorMessage,
                    ParseTree = null
                };
            }
            
            // If no error, get the parse tree
            if (nativeResult.tree != IntPtr.Zero)
            {
                var parseTreeJson = NativeMethods.PtrToString(nativeResult.tree);
                
                if (!string.IsNullOrEmpty(parseTreeJson))
                {
                    // Deserialize the parse tree JSON
                    var parseTree = JsonDocument.Parse(parseTreeJson);
                    
                    return new ParseResult 
                    { 
                        Query = query,
                        ParseTree = parseTree,
                        Error = null
                    };
                }
            }
            
            // Fallback if no parse tree and no error
            return new ParseResult 
            { 
                Query = query,
                Error = "Failed to parse query: no result from parser"
            };
        }
        finally
        {
            // Always free the native result
            NativeMethods.pg_query_free_parse_result(nativeResult);
        }
    }

    /// <summary>
    /// Normalize a PostgreSQL query by removing comments and standardizing formatting
    /// </summary>
    /// <param name="query">The SQL query to normalize</param>
    /// <returns>Normalize result containing the normalized query or error information</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed</exception>
    public NormalizeResult Normalize(string query) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        try {
            var inputBytes = NativeMethods.StringToUtf8Bytes(query);
            var result = NativeMethods.pg_query_normalize(inputBytes);

            try {
                var normalizedQuery = NativeMethods.PtrToString(result.normalized_query);
                
                // Use MarshalError for proper error handling
                string? error = null;
                if (result.error != IntPtr.Zero)
                {
                    var errorStruct = NativeMethods.MarshalError(result.error);
                    if (errorStruct?.message != IntPtr.Zero)
                    {
                        error = NativeMethods.PtrToString(errorStruct.Value.message);
                    }
                }

                return new NormalizeResult {
                    Query = query,
                    NormalizedQuery = normalizedQuery,
                    Error = error
                };
            }
            finally {
                NativeMethods.pg_query_free_normalize_result(result);
            }
        }
        catch (Exception ex) {
            return new NormalizeResult {
                Query = query,
                Error = $"Native library error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Generate a fingerprint for a PostgreSQL query for similarity comparison
    /// </summary>
    /// <param name="query">The SQL query to fingerprint</param>
    /// <returns>Fingerprint result containing the fingerprint hash or error information</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed</exception>
    public FingerprintResult Fingerprint(string query) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        try {
            var inputBytes = NativeMethods.StringToUtf8Bytes(query);
            var result = NativeMethods.pg_query_fingerprint(inputBytes);

            try {
                var fingerprint = NativeMethods.PtrToString(result.fingerprint_str);
                
                // Use MarshalError for proper error handling
                string? error = null;
                if (result.error != IntPtr.Zero)
                {
                    var errorStruct = NativeMethods.MarshalError(result.error);
                    if (errorStruct?.message != IntPtr.Zero)
                    {
                        error = NativeMethods.PtrToString(errorStruct.Value.message);
                    }
                }

                return new FingerprintResult {
                    Query = query,
                    Fingerprint = fingerprint,
                    Error = error
                };
            }
            finally {
                NativeMethods.pg_query_free_fingerprint_result(result);
            }
        }
        catch (Exception ex) {
            return new FingerprintResult {
                Query = query,
                Error = $"Native library error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Parse a PostgreSQL query and return the AST as a strongly-typed object
    /// </summary>
    /// <typeparam name="T">The type to deserialize the AST to</typeparam>
    /// <param name="query">The SQL query to parse</param>
    /// <param name="options">Parse options (optional)</param>
    /// <returns>Parsed AST as the specified type, or null if parsing failed</returns>
    public T? ParseAs<T>(string query, ParseOptions? options = null) where T : class {
        var result = Parse(query, options);
        if (result.IsError || result.ParseTree is null) {
            return null;
        }

        try {
            return result.ParseTree as T;
        }
        catch {
            return null;
        }
    }

    /// <summary>
    /// Validate that a PostgreSQL query has valid syntax
    /// </summary>
    /// <param name="query">The SQL query to validate</param>
    /// <returns>True if the query is valid, false otherwise</returns>
    public bool IsValid(string query) {
        var result = Parse(query);
        return result.IsSuccess;
    }

    /// <summary>
    /// Get the error message for an invalid query
    /// </summary>
    /// <param name="query">The SQL query to check</param>
    /// <returns>Error message if invalid, null if valid</returns>
    public string? GetError(string query) {
        var result = Parse(query);
        return result.Error;
    }

    /// <summary>
    /// Dispose of the Npgquery instance
    /// </summary>
    public void Dispose() {
        _disposed = true;
    }

    /// <summary>
    /// Static factory method for quick one-off parsing
    /// </summary>
    /// <param name="query">The SQL query to parse</param>
    /// <param name="options">Parse options (optional)</param>
    /// <returns>Parse result</returns>
    public static ParseResult QuickParse(string query, ParseOptions? options = null) {
        using var parser = new Npgquery();
        return parser.Parse(query, options);
    }

    /// <summary>
    /// Static factory method for quick one-off normalization
    /// </summary>
    /// <param name="query">The SQL query to normalize</param>
    /// <returns>Normalize result</returns>
    public static NormalizeResult QuickNormalize(string query) {
        using var parser = new Npgquery();
        return parser.Normalize(query);
    }

    /// <summary>
    /// Static factory method for quick one-off fingerprinting
    /// </summary>
    /// <param name="query">The SQL query to fingerprint</param>
    /// <returns>Fingerprint result</returns>
    public static FingerprintResult QuickFingerprint(string query) {
        using var parser = new Npgquery();
        return parser.Fingerprint(query);
    }

    /// <summary>
    /// Split a string containing multiple PostgreSQL statements
    /// </summary>
    /// <param name="query">The SQL string containing multiple statements</param>
    /// <returns>Split result containing individual statements or error information</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed</exception>
    public SplitResult Split(string query) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        try {
            var inputBytes = NativeMethods.StringToUtf8Bytes(query);
            var result = NativeMethods.pg_query_split_with_parser(inputBytes);

            try {
                var stmts = NativeMethods.MarshalSplitStmts(result);
                
                // Use MarshalError for proper error handling
                string? error = null;
                if (result.error != IntPtr.Zero)
                {
                    var errorStruct = NativeMethods.MarshalError(result.error);
                    if (errorStruct?.message != IntPtr.Zero)
                    {
                        error = NativeMethods.PtrToString(errorStruct.Value.message);
                    }
                }

                var statements = new List<SqlStatement>();
                foreach (var stmt in stmts) {

                    statements.Add(new SqlStatement {
                        Location = stmt.stmt_location,
                        Length = stmt.stmt_len,
                        Statement = query.Substring(stmt.stmt_location, stmt.stmt_len)
                    });

                }

                return new SplitResult {
                    Query = query,
                    Statements = statements.ToArray(),
                    Error = error
                };
            }
            finally {
                NativeMethods.pg_query_free_split_result(result);
            }
        }
        catch (Exception ex) {
            return new SplitResult {
                Query = query,
                Error = $"Native library error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Scan/tokenize a PostgreSQL query
    /// </summary>
    /// <param name="query">The SQL query to scan</param>
    /// <returns>Scan result containing tokens or error information</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed</exception>
    public ScanResult Scan(string query) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        try {
            var inputBytes = NativeMethods.StringToUtf8Bytes(query);
            var result = NativeMethods.pg_query_scan(inputBytes);

            try {
                var processed = NativeMethods.ProcessScanResult(result, query);
                
                return new ScanResult {
                    Query = query,
                    Version = processed.Version,
                    Tokens = processed.Tokens,
                    Error = processed.Error,
                    Stderr = processed.Stderr
                };
            }
            finally {
                NativeMethods.pg_query_free_scan_result(result);
            }
        }
        catch (Exception ex) {
            return new ScanResult {
                Query = query,
                Error = $"Native library error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Scan/tokenize a PostgreSQL query with enhanced protobuf support
    /// </summary>
    /// <param name="query">The SQL query to scan</param>
    /// <returns>Enhanced scan result with protobuf data</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed</exception>
    public EnhancedScanResult ScanWithProtobuf(string query) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        try {
            var inputBytes = NativeMethods.StringToUtf8Bytes(query);
            var result = NativeMethods.pg_query_scan(inputBytes);

            try {
                var processed = NativeMethods.ProcessScanResult(result, query);
                
                // Also extract the raw protobuf data if available
                PgQuery.ScanResult? protobufResult = null;
                if (result.pbuf.data != IntPtr.Zero && result.pbuf.len != UIntPtr.Zero) {
                    try {
                        var protobufData = ProtobufHelper.ExtractProtobufData(result.pbuf);
                        protobufResult = PgQuery.ScanResult.Parser.ParseFrom(protobufData);
                    }
                    catch {
                        // Ignore protobuf parsing errors for the enhanced result
                    }
                }
                
                return new EnhancedScanResult {
                    Query = query,
                    Version = processed.Version,
                    Tokens = processed.Tokens,
                    Error = processed.Error,
                    Stderr = processed.Stderr,
                    ProtobufScanResult = protobufResult
                };
            }
            finally {
                NativeMethods.pg_query_free_scan_result(result);
            }
        }
        catch (Exception ex) {
            return new EnhancedScanResult {
                Query = query,
                Error = $"Native library error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Parse PL/pgSQL code into an Abstract Syntax Tree (AST)
    /// </summary>
    /// <param name="plpgsqlCode">The PL/pgSQL code to parse</param>
    /// <returns>Parse result containing the AST or error information</returns>
    /// <exception cref="ArgumentNullException">Thrown when plpgsqlCode is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed</exception>
    public PlpgsqlParseResult ParsePlpgsql(string plpgsqlCode) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(plpgsqlCode);

        try {
            var inputBytes = NativeMethods.StringToUtf8Bytes(plpgsqlCode);
            var result = NativeMethods.pg_query_parse_plpgsql(inputBytes);

            try {
                var parseTree = NativeMethods.PtrToString(result.tree);
                
                // Use MarshalError for proper error handling
                string? error = null;
                if (result.error != IntPtr.Zero)
                {
                    var errorStruct = NativeMethods.MarshalError(result.error);
                    if (errorStruct?.message != IntPtr.Zero)
                    {
                        error = NativeMethods.PtrToString(errorStruct.Value.message);
                    }
                }

                return new PlpgsqlParseResult {
                    Query = plpgsqlCode,
                    ParseTree = parseTree,
                    Error = error
                };
            }
            finally {
                NativeMethods.pg_query_free_plpgsql_parse_result(result);
            }
        }
        catch (Exception ex) {
            return new PlpgsqlParseResult {
                Query = plpgsqlCode,
                Error = $"Native library error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Static factory method for quick one-off deparsing
    /// </summary>
    /// <param name="parseTree">The AST JSON to deparse</param>
    /// <returns>Deparse result</returns>
    public static DeparseResult QuickDeparse(JsonDocument parseTree) {
        using var parser = new Npgquery();
        return parser.Deparse(parseTree);
    }

    /// <summary>
    /// Static factory method for quick one-off splitting
    /// </summary>
    /// <param name="query">The SQL string to split</param>
    /// <returns>Split result</returns>
    public static SplitResult QuickSplit(string query) {
        using var parser = new Npgquery();
        return parser.Split(query);
    }

    /// <summary>
    /// Static factory method for quick one-off scanning
    /// </summary>
    /// <param name="query">The SQL query to scan</param>
    /// <returns>Scan result</returns>
    public static ScanResult QuickScan(string query) {
        using var parser = new Npgquery();
        return parser.Scan(query);
    }

    /// <summary>
    /// Static factory method for quick one-off PL/pgSQL parsing
    /// </summary>
    /// <param name="plpgsqlCode">The PL/pgSQL code to parse</param>
    /// <returns>PL/pgSQL parse result</returns>
    public static PlpgsqlParseResult QuickParsePlpgsql(string plpgsqlCode) {
        using var parser = new Npgquery();
        return parser.ParsePlpgsql(plpgsqlCode);
    }

    /// <summary>
    /// Static factory method for quick one-off enhanced scanning with protobuf
    /// </summary>
    /// <param name="query">The SQL query to scan</param>
    /// <returns>Enhanced scan result</returns>
    public static EnhancedScanResult QuickScanWithProtobuf(string query) {
        using var parser = new Npgquery();
        return parser.ScanWithProtobuf(query);
    }

    /// <summary>
    /// Deparse a PostgreSQL AST back to SQL
    /// </summary>
    /// <param name="parseTree">The AST JSON document to deparse</param>
    /// <returns>Deparse result containing the SQL query or error information</returns>
    /// <exception cref="ArgumentNullException">Thrown when parseTree is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed</exception>
    public DeparseResult Deparse(JsonDocument parseTree) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(parseTree);

        try {
            // Convert JsonDocument to Protobuf ParseResult
            var json = parseTree.RootElement.GetRawText();
            var protoParseResult = NpgqueryLib.Protobuf.ProtobufAstHelper.ParseResultFromJson(json);
            var protoBytes = protoParseResult.ToByteArray();
            var protoStruct = NativeMethods.AllocPgQueryProtobuf(protoBytes);

            try {
                var deparseResult = NativeMethods.pg_query_deparse_protobuf(protoStruct);
                try {
                    if (deparseResult.error != IntPtr.Zero) {
                        var errorStruct = NativeMethods.MarshalError(deparseResult.error);
                        var errorMessage = errorStruct?.message != IntPtr.Zero
                            ? NativeMethods.PtrToString(errorStruct.Value.message)
                            : "Deparse error";
                        return new DeparseResult {
                            Ast = parseTree.RootElement.ToString(),
                            Error = errorMessage
                        };
                    }
                    return new DeparseResult {
                        Ast = parseTree.RootElement.ToString(),
                        Query = NativeMethods.PtrToString(deparseResult.query)
                    };
                }
                finally {
                    NativeMethods.pg_query_free_deparse_result(deparseResult);
                }
            }
            finally {
                NativeMethods.FreePgQueryProtobuf(protoStruct);
            }
        }
        catch (Exception ex) {
            return new DeparseResult {
                Ast = parseTree.RootElement.ToString(),
                Error = $"Native library error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Parse a PostgreSQL query into protobuf format (for use with Deparse)
    /// </summary>
    /// <param name="query">The SQL query to parse</param>
    /// <returns>Parse result with protobuf data</returns>
    public ProtobufParseResult ParseProtobuf(string query) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        try {
            var inputBytes = NativeMethods.StringToUtf8Bytes(query);
            var result = NativeMethods.pg_query_parse_protobuf(inputBytes);

            try {
                if (result.error != IntPtr.Zero) {
                    var errorStruct = NativeMethods.MarshalError(result.error);
                    var errorMessage = errorStruct?.message != IntPtr.Zero
                        ? NativeMethods.PtrToString(errorStruct.Value.message)
                        : "Parse error";

                    return new ProtobufParseResult {
                        Query = query,
                        Error = errorMessage
                    };
                }

                // Keep the parse_tree for deparsing
                return new ProtobufParseResult {
                    Query = query,
                    ParseTree = result.parse_tree,
                    NativeResult = result
                };
            }
            catch {
                // Make sure to free on error
                NativeMethods.pg_query_free_protobuf_parse_result(result);
                throw;
            }
        }
        catch (Exception ex) {
            return new ProtobufParseResult {
                Query = query,
                Error = $"Native library error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Deparse a PostgreSQL protobuf parse result back to SQL
    /// </summary>
    /// <param name="parseResult">The protobuf parse result from ParseProtobuf</param>
    /// <returns>Deparse result containing the SQL query or error information</returns>
    public DeparseResult DeparseProtobuf(ProtobufParseResult parseResult) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(parseResult);

        if (parseResult.IsError || parseResult.ParseTree == null) {
            return new DeparseResult {
                Ast = "",
                Error = "Cannot deparse an error result or null parse tree"
            };
        }

        try {
            // Fix: Use .Value to get the PgQueryProtobuf from the nullable type
            var deparseResult = NativeMethods.pg_query_deparse_protobuf(parseResult.ParseTree.Value);

            try {
                if (deparseResult.error != IntPtr.Zero) {
                    var errorStruct = NativeMethods.MarshalError(deparseResult.error);
                    var errorMessage = errorStruct?.message != IntPtr.Zero
                        ? NativeMethods.PtrToString(errorStruct.Value.message)
                        : "Deparse error";

                    return new DeparseResult {
                        Ast = "",
                        Error = errorMessage
                    };
                }

                return new DeparseResult {
                    Ast = "", // We don't have JSON AST in this flow
                    Query = NativeMethods.PtrToString(deparseResult.query)
                };
            }
            finally {
                NativeMethods.pg_query_free_deparse_result(deparseResult);
            }
        }
        catch (Exception ex) {
            return new DeparseResult {
                Ast = "",
                Error = $"Native library error: {ex.Message}"
            };
        }
        finally {
            // Free the protobuf parse result
            if (parseResult.NativeResult != null) {
                NativeMethods.pg_query_free_protobuf_parse_result(parseResult.NativeResult.Value);
            }
        }
    }
}
