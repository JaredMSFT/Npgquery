using System.Text.Json.Serialization;

namespace NpgqueryLib;

/// <summary>
/// Represents the result of parsing a PostgreSQL query
/// </summary>
public sealed record ParseResult
{
    /// <summary>
    /// The parsed query as a JSON string representing the parse tree
    /// </summary>
    [JsonPropertyName("parse_tree")]
    public string? ParseTree { get; init; }

    /// <summary>
    /// Any error that occurred during parsing
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The original query that was parsed
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether parsing was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Indicates whether parsing failed
    /// </summary>
    [JsonIgnore]
    public bool IsError => !IsSuccess;
}

/// <summary>
/// Represents the result of normalizing a PostgreSQL query
/// </summary>
public sealed record NormalizeResult
{
    /// <summary>
    /// The normalized query string
    /// </summary>
    [JsonPropertyName("normalized_query")]
    public string? NormalizedQuery { get; init; }

    /// <summary>
    /// Any error that occurred during normalization
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The original query that was normalized
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether normalization was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Indicates whether normalization failed
    /// </summary>
    [JsonIgnore]
    public bool IsError => !IsSuccess;
}

/// <summary>
/// Represents the result of fingerprinting a PostgreSQL query
/// </summary>
public sealed record FingerprintResult
{
    /// <summary>
    /// The fingerprint hash of the query
    /// </summary>
    [JsonPropertyName("fingerprint")]
    public string? Fingerprint { get; init; }

    /// <summary>
    /// Any error that occurred during fingerprinting
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The original query that was fingerprinted
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether fingerprinting was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Indicates whether fingerprinting failed
    /// </summary>
    [JsonIgnore]
    public bool IsError => !IsSuccess;
}

/// <summary>
/// Options for parsing PostgreSQL queries
/// </summary>
public sealed record ParseOptions
{
    /// <summary>
    /// Whether to include location information in the parse tree
    /// </summary>
    public bool IncludeLocations { get; init; } = false;

    /// <summary>
    /// The PostgreSQL version to use for parsing (default is latest)
    /// </summary>
    public int PostgreSqlVersion { get; init; } = 160000; // PostgreSQL 16

    /// <summary>
    /// Default parse options
    /// </summary>
    public static readonly ParseOptions Default = new();
}

/// <summary>
/// Represents the result of deparsing a PostgreSQL AST back to SQL
/// </summary>
public sealed record DeparseResult
{
    /// <summary>
    /// The deparsed SQL query
    /// </summary>
    [JsonPropertyName("query")]
    public string? Query { get; init; }

    /// <summary>
    /// Any error that occurred during deparsing
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The original AST that was deparsed
    /// </summary>
    [JsonPropertyName("ast")]
    public string Ast { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether deparsing was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Indicates whether deparsing failed
    /// </summary>
    [JsonIgnore]
    public bool IsError => !IsSuccess;
}

/// <summary>
/// Represents a single SQL statement from splitting
/// </summary>
public sealed record SqlStatement
{
    /// <summary>
    /// The SQL statement text
    /// </summary>
    [JsonPropertyName("stmt")]
    public string? Statement { get; init; }

    /// <summary>
    /// Starting position in the original text
    /// </summary>
    [JsonPropertyName("stmt_location")]
    public int Location { get; init; }

    /// <summary>
    /// Length of the statement
    /// </summary>
    [JsonPropertyName("stmt_len")]
    public int Length { get; init; }
}

/// <summary>
/// Represents the result of splitting multiple PostgreSQL statements
/// </summary>
public sealed record SplitResult
{
    /// <summary>
    /// The individual SQL statements
    /// </summary>
    [JsonPropertyName("stmts")]
    public SqlStatement[]? Statements { get; init; }

    /// <summary>
    /// Any error that occurred during splitting
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The original query that was split
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether splitting was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Indicates whether splitting failed
    /// </summary>
    [JsonIgnore]
    public bool IsError => !IsSuccess;
}

/// <summary>
/// Represents a single token from scanning
/// </summary>
public sealed record SqlToken
{
    /// <summary>
    /// Token type
    /// </summary>
    [JsonPropertyName("token")]
    public int Token { get; init; }

    /// <summary>
    /// Token keyword (if applicable)
    /// </summary>
    [JsonPropertyName("keyword_kind")]
    public string? KeywordKind { get; init; }

    /// <summary>
    /// Starting position in the original text
    /// </summary>
    [JsonPropertyName("start")]
    public int Start { get; init; }

    /// <summary>
    /// Ending position in the original text
    /// </summary>
    [JsonPropertyName("end")]
    public int End { get; init; }
}

/// <summary>
/// Represents the result of scanning/tokenizing a PostgreSQL query
/// </summary>
public sealed record ScanResult
{
    /// <summary>
    /// The individual tokens
    /// </summary>
    [JsonPropertyName("tokens")]
    public SqlToken[]? Tokens { get; init; }

    /// <summary>
    /// Any error that occurred during scanning
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The original query that was scanned
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether scanning was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Indicates whether scanning failed
    /// </summary>
    [JsonIgnore]
    public bool IsError => !IsSuccess;
}

/// <summary>
/// Represents the result of parsing PL/pgSQL code
/// </summary>
public sealed record PlpgsqlParseResult
{
    /// <summary>
    /// The parsed PL/pgSQL as a JSON string representing the parse tree
    /// </summary>
    [JsonPropertyName("parse_tree")]
    public string? ParseTree { get; init; }

    /// <summary>
    /// Any error that occurred during parsing
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The original PL/pgSQL code that was parsed
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether parsing was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Indicates whether parsing failed
    /// </summary>
    [JsonIgnore]
    public bool IsError => !IsSuccess;
}