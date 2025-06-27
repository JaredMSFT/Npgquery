# NpgqueryLib - PostgreSQL Query Parser for .NET

A high-performance .NET 9 C# library for parsing PostgreSQL queries using the battle-tested `libpg_query` library. This library provides the same functionality as popular wrappers in other languages like Go, Rust, Python, and JavaScript.

## Features

- **Parse PostgreSQL queries** into Abstract Syntax Trees (AST)
- **Normalize queries** by removing comments and standardizing formatting
- **Generate query fingerprints** for similarity comparison
- **Deparse AST back to SQL** (convert parse trees back to queries)
- **Split multiple statements** with location information
- **Scan/tokenize queries** for detailed analysis
- **Parse PL/pgSQL code** (stored procedures, functions)
- **Extract metadata** like table names and query types
- **Async/await support** for modern .NET applications
- **Memory-safe** native interop with automatic resource cleanup
- **Strong typing** with nullable reference types and records
- **High performance** with parallel processing capabilities

## Installation
dotnet add package NpgqueryLib
## Quick Start

### Basic Parsing
using NpgqueryLib;

// Parse a query
using var parser = new Npgquery();
var result = parser.Parse("SELECT * FROM users WHERE id = 1");

if (result.IsSuccess)
{
    Console.WriteLine($"Parse tree: {result.ParseTree}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
### Query Normalization
var normalizeResult = parser.Normalize("SELECT * FROM users /* comment */ WHERE id = 1");
Console.WriteLine($"Normalized: {normalizeResult.NormalizedQuery}");
// Output: SELECT * FROM users WHERE id = $1
### Query Fingerprinting
var query1 = "SELECT * FROM users WHERE id = 1";
var query2 = "SELECT * FROM users WHERE id = 2";

var fp1 = parser.Fingerprint(query1);
var fp2 = parser.Fingerprint(query2);

// Same structure, different values
Console.WriteLine($"Same structure: {fp1.Fingerprint == fp2.Fingerprint}");
### Query Deparsing (NEW!)
// Parse then deparse back to SQL
var parseResult = parser.Parse("SELECT * FROM users WHERE id = 1");
var deparseResult = parser.Deparse(parseResult.ParseTree);
Console.WriteLine($"Deparsed: {deparseResult.Query}");
### Statement Splitting (NEW!)
var multiQuery = "SELECT 1; INSERT INTO test VALUES (1); UPDATE test SET col = 2;";
var splitResult = parser.Split(multiQuery);

foreach (var stmt in splitResult.Statements)
{
    Console.WriteLine($"Statement: {stmt.Statement}");
    Console.WriteLine($"Location: {stmt.Location}, Length: {stmt.Length}");
}
### Query Tokenization (NEW!)
var scanResult = parser.Scan("SELECT COUNT(*) FROM users");
foreach (var token in scanResult.Tokens)
{
    Console.WriteLine($"Token: {token.Token}, Keyword: {token.KeywordKind}");
}
### PL/pgSQL Parsing (NEW!)
var plpgsqlCode = @"
    BEGIN
        IF user_count > 0 THEN
            RETURN 'Users exist';
        END IF;
    END;
";

var plpgsqlResult = parser.ParsePlpgsql(plpgsqlCode);
if (plpgsqlResult.IsSuccess)
{
    Console.WriteLine($"PL/pgSQL parse tree: {plpgsqlResult.ParseTree}");
}
### Utility Functions
// Extract table names
var tables = QueryUtils.ExtractTableNames("SELECT u.name, p.title FROM users u JOIN posts p ON u.id = p.user_id");
// Returns: ["users", "posts"]

// Get query type
var queryType = QueryUtils.GetQueryType("INSERT INTO users (name) VALUES ('John')");
// Returns: "INSERT"

// Split statements
var statements = QueryUtils.SplitStatements("SELECT 1; SELECT 2; SELECT 3;");
// Returns: ["SELECT 1", "SELECT 2", "SELECT 3"]

// Get tokens
var tokens = QueryUtils.GetTokens("SELECT COUNT(*) FROM users");

// Get keywords
var keywords = QueryUtils.GetKeywords("SELECT COUNT(*) FROM users WHERE active = true");

// Round-trip test (parse ? deparse)
var (success, roundTripQuery) = QueryUtils.RoundTripTest("SELECT * FROM users");

// Count statements
var count = QueryUtils.CountStatements("SELECT 1; INSERT INTO test VALUES (1);");

// Validate PL/pgSQL
bool isValid = QueryUtils.IsValidPlpgsql("BEGIN RETURN 'test'; END;");

// Validate query
bool isValid = parser.IsValid("SELECT * FROM users");
## Complete API Reference

### Core Methods
- `Parse(query)` - Parse SQL to AST
- `Normalize(query)` - Normalize SQL formatting  
- `Fingerprint(query)` - Generate structural hash
- `Deparse(ast)` - Convert AST back to SQL
- `Split(multiQuery)` - Split multiple statements
- `Scan(query)` - Tokenize/scan query
- `ParsePlpgsql(code)` - Parse PL/pgSQL code
- `IsValid(query)` - Validate syntax
- `GetError(query)` - Get error message

### Static Quick Methods
- `QuickParse(query)` - One-off parsing
- `QuickNormalize(query)` - One-off normalization
- `QuickFingerprint(query)` - One-off fingerprinting
- `QuickDeparse(ast)` - One-off deparsing
- `QuickSplit(multiQuery)` - One-off splitting
- `QuickScan(query)` - One-off scanning
- `QuickParsePlpgsql(code)` - One-off PL/pgSQL parsing

## Async Support
using NpgqueryLib;

// Async parsing
var result = await parser.ParseAsync("SELECT * FROM users WHERE id = 1");

// Async deparsing
var deparseResult = await parser.DeparseAsync(parseTree);

// Async splitting
var splitResult = await parser.SplitAsync(multiQuery);

// Async scanning
var scanResult = await parser.ScanAsync(query);

// Async PL/pgSQL parsing
var plpgsqlResult = await parser.ParsePlpgsqlAsync(plpgsqlCode);

// Parse multiple queries in parallel
var queries = new[] { "SELECT 1", "SELECT 2", "SELECT 3" };
var results = await parser.ParseManyAsync(queries, maxDegreeOfParallelism: 4);

// Static async methods
var quickResult = await NpgqueryAsync.QuickParseAsync("SELECT * FROM users");
var quickDeparse = await NpgqueryAsync.QuickDeparseAsync(parseTree);
var quickSplit = await NpgqueryAsync.QuickSplitAsync(multiQuery);
var quickScan = await NpgqueryAsync.QuickScanAsync(query);
var quickPlpgsql = await NpgqueryAsync.QuickParsePlpgsqlAsync(plpgsqlCode);
## Advanced Usage

### Custom Parse Options
var options = new ParseOptions
{
    IncludeLocations = true,
    PostgreSqlVersion = 160000 // PostgreSQL 16
};

var result = parser.Parse("SELECT * FROM users", options);
### Strongly-Typed AST
// Define your AST model classes
public class Statement
{
    public SelectStmt? SelectStmt { get; set; }
}

public class SelectStmt
{
    public List<ResTarget>? TargetList { get; set; }
    public List<RangeVar>? FromClause { get; set; }
}

// Parse into strongly-typed objects
var ast = parser.ParseAs<Statement>("SELECT * FROM users");
### Batch Processing
var queries = File.ReadAllLines("queries.sql");

// Validate all queries
var validationResults = QueryUtils.ValidateQueries(queries);

// Get errors for invalid queries
var errors = QueryUtils.GetQueryErrors(queries);

// Split and normalize all statements
foreach (var query in queries)
{
    var statements = QueryUtils.SplitStatements(query);
    var normalized = QueryUtils.NormalizeStatements(query);
}
### Error Handling
try
{
    var result = parser.Parse("INVALID SQL");
    if (result.IsError)
    {
        throw new ParseException(result.Error!, "INVALID SQL");
    }
}
catch (ParseException ex)
{
    Console.WriteLine($"Parse error in query '{ex.Query}': {ex.ParseError}");
}
catch (DeparseException ex)
{
    Console.WriteLine($"Deparse error: {ex.DeparseError}");
}
catch (SplitException ex)
{
    Console.WriteLine($"Split error: {ex.SplitError}");
}
catch (ScanException ex)
{
    Console.WriteLine($"Scan error: {ex.ScanError}");
}
catch (PlpgsqlParseException ex)
{
    Console.WriteLine($"PL/pgSQL parse error: {ex.PlpgsqlParseError}");
}
catch (NativeLibraryException ex)
{
    Console.WriteLine($"Native library error: {ex.Message}");
}
## Performance Tips

1. **Reuse parser instances** instead of creating new ones for each operation
2. **Use async methods** for I/O-bound operations
3. **Process queries in parallel** with `ParseManyAsync`
4. **Dispose properly** or use `using` statements
5. **Use static Quick methods** for one-off operations
6. **Cache parsing results** for repeated queries

## Native Dependencies

This library requires the `libpg_query` native library. The NuGet package includes pre-compiled binaries for:

- Windows (x64, ARM64)
- Linux (x64, ARM64)  
- macOS (x64, ARM64)

## Supported Features

### ? Implemented (Same as other wrappers)
- SQL parsing to AST
- Query normalization
- Query fingerprinting  
- AST deparsing to SQL
- Multi-statement splitting
- Query tokenization/scanning
- PL/pgSQL parsing
- Comprehensive error handling
- Async operations
- Batch processing

### ?? .NET-Specific Enhancements
- Strong typing with nullable reference types
- Record types for immutable data
- Extension methods for fluent API
- Comprehensive XML documentation
- Advanced error handling with custom exceptions
- Performance optimizations for high-throughput scenarios

## Thread Safety

The `Npgquery` class is **not thread-safe**. Create separate instances for each thread or use proper synchronization. The static `Quick*` methods are thread-safe.

## Contributing

Contributions are welcome! Please ensure that:

1. All tests pass
2. Code follows .NET conventions
3. Public APIs are documented
4. Memory management is handled properly

## License

MIT License - see LICENSE file for details.

## Acknowledgments

This library is built on top of the excellent [libpg_query](https://github.com/pganalyze/libpg_query) project, which embeds the PostgreSQL parser.