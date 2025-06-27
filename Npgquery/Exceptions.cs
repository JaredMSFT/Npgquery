namespace NpgqueryLib;

/// <summary>
/// Base exception for all Npgquery-related errors
/// </summary>
public abstract class NpgqueryException : Exception
{
    /// <summary>
    /// The query that caused the exception (if available)
    /// </summary>
    public string? Query { get; }

    protected NpgqueryException(string message, string? query = null) : base(message)
    {
        Query = query;
    }

    protected NpgqueryException(string message, Exception innerException, string? query = null) 
        : base(message, innerException)
    {
        Query = query;
    }
}

/// <summary>
/// Exception thrown when a PostgreSQL query cannot be parsed
/// </summary>
public sealed class ParseException : NpgqueryException
{
    /// <summary>
    /// The specific parse error message from libpg_query
    /// </summary>
    public string ParseError { get; }

    public ParseException(string parseError, string? query = null) 
        : base($"Failed to parse PostgreSQL query: {parseError}", query)
    {
        ParseError = parseError;
    }

    public ParseException(string parseError, Exception innerException, string? query = null) 
        : base($"Failed to parse PostgreSQL query: {parseError}", innerException, query)
    {
        ParseError = parseError;
    }
}

/// <summary>
/// Exception thrown when the native libpg_query library cannot be loaded or accessed
/// </summary>
public sealed class NativeLibraryException : NpgqueryException
{
    public NativeLibraryException(string message) : base(message)
    {
    }

    public NativeLibraryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when query normalization fails
/// </summary>
public sealed class NormalizationException : NpgqueryException
{
    /// <summary>
    /// The specific normalization error message
    /// </summary>
    public string NormalizationError { get; }

    public NormalizationException(string normalizationError, string? query = null)
        : base($"Failed to normalize PostgreSQL query: {normalizationError}", query)
    {
        NormalizationError = normalizationError;
    }

    public NormalizationException(string normalizationError, Exception innerException, string? query = null)
        : base($"Failed to normalize PostgreSQL query: {normalizationError}", innerException, query)
    {
        NormalizationError = normalizationError;
    }
}

/// <summary>
/// Exception thrown when query fingerprinting fails
/// </summary>
public sealed class FingerprintException : NpgqueryException
{
    /// <summary>
    /// The specific fingerprinting error message
    /// </summary>
    public string FingerprintError { get; }

    public FingerprintException(string fingerprintError, string? query = null)
        : base($"Failed to fingerprint PostgreSQL query: {fingerprintError}", query)
    {
        FingerprintError = fingerprintError;
    }

    public FingerprintException(string fingerprintError, Exception innerException, string? query = null)
        : base($"Failed to fingerprint PostgreSQL query: {fingerprintError}", innerException, query)
    {
        FingerprintError = fingerprintError;
    }
}

/// <summary>
/// Exception thrown when AST deparsing fails
/// </summary>
public sealed class DeparseException : NpgqueryException
{
    /// <summary>
    /// The specific deparsing error message
    /// </summary>
    public string DeparseError { get; }

    public DeparseException(string deparseError, string? ast = null)
        : base($"Failed to deparse PostgreSQL AST: {deparseError}", ast)
    {
        DeparseError = deparseError;
    }

    public DeparseException(string deparseError, Exception innerException, string? ast = null)
        : base($"Failed to deparse PostgreSQL AST: {deparseError}", innerException, ast)
    {
        DeparseError = deparseError;
    }
}

/// <summary>
/// Exception thrown when query splitting fails
/// </summary>
public sealed class SplitException : NpgqueryException
{
    /// <summary>
    /// The specific splitting error message
    /// </summary>
    public string SplitError { get; }

    public SplitException(string splitError, string? query = null)
        : base($"Failed to split PostgreSQL query: {splitError}", query)
    {
        SplitError = splitError;
    }

    public SplitException(string splitError, Exception innerException, string? query = null)
        : base($"Failed to split PostgreSQL query: {splitError}", innerException, query)
    {
        SplitError = splitError;
    }
}

/// <summary>
/// Exception thrown when query scanning/tokenization fails
/// </summary>
public sealed class ScanException : NpgqueryException
{
    /// <summary>
    /// The specific scanning error message
    /// </summary>
    public string ScanError { get; }

    public ScanException(string scanError, string? query = null)
        : base($"Failed to scan PostgreSQL query: {scanError}", query)
    {
        ScanError = scanError;
    }

    public ScanException(string scanError, Exception innerException, string? query = null)
        : base($"Failed to scan PostgreSQL query: {scanError}", innerException, query)
    {
        ScanError = scanError;
    }
}

/// <summary>
/// Exception thrown when PL/pgSQL parsing fails
/// </summary>
public sealed class PlpgsqlParseException : NpgqueryException
{
    /// <summary>
    /// The specific PL/pgSQL parsing error message
    /// </summary>
    public string PlpgsqlParseError { get; }

    public PlpgsqlParseException(string plpgsqlParseError, string? plpgsqlCode = null)
        : base($"Failed to parse PL/pgSQL code: {plpgsqlParseError}", plpgsqlCode)
    {
        PlpgsqlParseError = plpgsqlParseError;
    }

    public PlpgsqlParseException(string plpgsqlParseError, Exception innerException, string? plpgsqlCode = null)
        : base($"Failed to parse PL/pgSQL code: {plpgsqlParseError}", innerException, plpgsqlCode)
    {
        PlpgsqlParseError = plpgsqlParseError;
    }
}