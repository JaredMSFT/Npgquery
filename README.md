# Npgquery - PostgreSQL Query Parser for .NET

A high-performance .NET 9 C# library for parsing PostgreSQL queries using the battle-tested `libpg_query` library. This library provides the same functionality as popular wrappers in other languages like Go, Rust, Python, and JavaScript.

## Features

- **Parse PostgreSQL queries** into Abstract Syntax Trees (AST) in JSON or Protobuf format.
- **Normalize queries** by standardizing formatting and replacing constants with placeholders.
- **Generate query fingerprints** for similarity comparison.
- **Deparse AST back to SQL** (convert parse trees back to queries).
- **Split multiple statements** with location information.
- **Scan/tokenize queries** for detailed analysis.
- **Parse PL/pgSQL code** (stored procedures, functions).
- **Extract metadata** like table names and query types.
- **Async/await support** for modern .NET applications.
- **Memory-safe** native interop with automatic resource cleanup.
- **Strong typing** with nullable reference types and records.
- **High performance** with parallel processing capabilities.

## Installation

```bash
dotnet add package Npgquery
```

## Quick Start

### Basic Parsing

```csharp
using Npgquery;

// Parse a query
using var parser = new Parser();
var result = parser.Parse("SELECT * FROM users WHERE id = 1");

if (result.IsSuccess)
{
    Console.WriteLine($"Parse tree: {result.ParseTree}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

### Query Normalization

```csharp
using var parser = new Parser();
var normalizeResult = parser.Normalize("SELECT * FROM users WHERE id = 1");
Console.WriteLine($"Normalized: {normalizeResult.NormalizedQuery}");
// Output: SELECT * FROM users WHERE id = $1
```

### Query Fingerprinting

```csharp
using var parser = new Parser();
var query1 = "SELECT * FROM users WHERE id = 1";
var query2 = "SELECT * FROM users WHERE id = 2";

var fp1 = parser.Fingerprint(query1);
var fp2 = parser.Fingerprint(query2);

// Same structure, different values
Console.WriteLine($"Same structure: {fp1.Fingerprint == fp2.Fingerprint}");
```

### Query Deparsing

```csharp
using var parser = new Parser();
// Parse then deparse back to SQL
var parseResult = parser.Parse("SELECT * FROM users WHERE id = 1");
if (parseResult.IsSuccess && parseResult.ParseTree is not null)
{
    var deparseResult = parser.Deparse(parseResult.ParseTree);
    Console.WriteLine($"Deparsed: {deparseResult.Query}");
}
```

### Statement Splitting

```csharp
using var parser = new Parser();
var multiQuery = "SELECT 1; INSERT INTO test VALUES (1); UPDATE test SET col = 2;";
var splitResult = parser.Split(multiQuery);

if (splitResult.IsSuccess && splitResult.Statements is not null)
{
    foreach (var stmt in splitResult.Statements)
    {
        Console.WriteLine($"Statement: {stmt.Statement}");
        Console.WriteLine($"Location: {stmt.Location}, Length: {stmt.Length}");
    }
}
```

### Query Tokenization

```csharp
using var parser = new Parser();
var scanResult = parser.Scan("SELECT COUNT(*) FROM users");
if (scanResult.IsSuccess && scanResult.Tokens is not null)
{
    foreach (var token in scanResult.Tokens)
    {
        Console.WriteLine($"Token: {token.Token}, Keyword: {token.KeywordKind}");
    }
}
```

### PL/pgSQL Parsing

```csharp
using var parser = new Parser();
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
```

## Complete API Reference

### Core `Parser` Instance Methods

#### `Parse(query, options)`

Parse a SQL string into a JSON AST.

**Parameters:**

- `query`: The SQL query string.
- `options`: Optional parsing options.

**Returns:**

- A `ParseResult` object with `IsSuccess`, `ParseTree`, and `Error` properties.

```csharp
using var parser = new Parser();
var options = new ParseOptions { IncludeLocations = true };
var result = parser.Parse("SELECT * FROM users", options);

if (result.IsSuccess)
{
    Console.WriteLine($"Parsed AST: {result.ParseTree}");
}
else
{
    Console.WriteLine($"Parse error: {result.Error}");
}
```

#### `ParseProtobuf(query)`

Parse a SQL string into a Protobuf AST.

**Parameters:**

- `query`: The SQL query string.

**Returns:**

- A `ProtobufParseResult` object with `IsSuccess`, `ParseTree`, and `Error` properties.

#### `Normalize(query)`

Normalize the formatting of a SQL query.

**Parameters:**

- `query`: The SQL query string.

**Returns:**

- A `NormalizeResult` object with `NormalizedQuery` and `Error` properties.

```csharp
using var parser = new Parser();
var normalizeResult = parser.Normalize("SELECT   *   FROM    users  WHERE id=1");

Console.WriteLine($"Normalized query: {normalizeResult.NormalizedQuery}");
// Output: SELECT * FROM users WHERE id = $1
```

#### `Fingerprint(query)`

Generate a structural fingerprint for a SQL query.

**Parameters:**

- `query`: The SQL query string.

**Returns:**

- A `FingerprintResult` object with `Fingerprint` and `Error` properties.

```csharp
using var parser = new Parser();
var query = "SELECT * FROM users WHERE id = 1";

var fingerprintResult = parser.Fingerprint(query);
Console.WriteLine($"Query fingerprint: {fingerprintResult.Fingerprint}");
```

#### `Deparse(ast)`

Convert a JSON AST back to a SQL query.

**Parameters:**

- `ast`: The JSON AST object.

**Returns:**

- A `DeparseResult` object with `Query` and `Error` properties.

```csharp
using var parser = new Parser();
var ast = parser.Parse("SELECT * FROM users WHERE id = 1").ParseTree;

var deparseResult = parser.Deparse(ast);
Console.WriteLine($"Deparsed query: {deparseResult.Query}");
```

#### `DeparseProtobuf(protobufAst)`

Convert a Protobuf AST back to a SQL query.

**Parameters:**

- `protobufAst`: The Protobuf AST object.

**Returns:**

- A `DeparseResult` object with `Query` and `Error` properties.

#### `Split(query)`

Split a SQL string with multiple statements into individual statements.

**Parameters:**

- `query`: The SQL query string.

**Returns:**

- A `SplitResult` object with `Statements` and `Error` properties.

```csharp
using var parser = new Parser();
var multiStatementQuery = "SELECT 1; INSERT INTO users VALUES (1, 'John');";
var splitResult = parser.Split(multiStatementQuery);

if (splitResult.IsSuccess)
{
    foreach (var statement in splitResult.Statements)
    {
        Console.WriteLine($"Statement: {statement.Statement}");
    }
}
```

#### `Scan(query)`

Tokenize/scan a SQL query.

**Parameters:**

- `query`: The SQL query string.

**Returns:**

- A `ScanResult` object with `Tokens` and `Error` properties.

```csharp
using var parser = new Parser();
var scanResult = parser.Scan("SELECT id, name FROM users");

if (scanResult.IsSuccess)
{
    foreach (var token in scanResult.Tokens)
    {
        Console.WriteLine($"Token: {token.Token}, Type: {token.KeywordKind}");
    }
}
```

#### `ScanWithProtobuf(query)`

Tokenize/scan a SQL query and return tokens in Protobuf format.

**Parameters:**

- `query`: The SQL query string.

**Returns:**

- A `EnhancedScanResult` object with `Tokens` and `Error` properties.

#### `ParsePlpgsql(code)`

Parse a PL/pgSQL code block.

**Parameters:**

- `code`: The PL/pgSQL code string.

**Returns:**

- A `PlpgsqlParseResult` object with `IsSuccess`, `ParseTree`, and `Error` properties.

```csharp
using var parser = new Parser();
var plpgsqlCode = "BEGIN IF id > 0 THEN RAISE NOTICE 'ID is positive'; END IF; END;";

var plpgsqlResult = parser.ParsePlpgsql(plpgsqlCode);
if (plpgsqlResult.IsSuccess)
{
    Console.WriteLine($"PL/pgSQL AST: {plpgsqlResult.ParseTree}");
}
```

#### `IsValid(query)`

Check if a SQL query is valid.

**Parameters:**

- `query`: The SQL query string.

**Returns:**

- A boolean indicating validity.

```csharp
using var parser = new Parser();
var isValid = parser.IsValid("SELECT * FROM users WHERE id = 1");

Console.WriteLine($"Is valid SQL: {isValid}");
```

#### `GetError(query)`

Get the error message for an invalid SQL query.

**Parameters:**

- `query`: The SQL query string.

**Returns:**

- A string with the error message.

```csharp
using var parser = new Parser();
var result = parser.Parse("SELECT * FROM WHERE id = 1"); // Invalid SQL

if (!result.IsSuccess)
{
    Console.WriteLine($"Error detected: {result.Error}");
}
```

#### `ParseAs<T>(query, options)`

Parse a SQL query into a strongly-typed object.

**Parameters:**

- `query`: The SQL query string.
- `options`: Optional parsing options.

**Returns:**

- An object of type `T` representing the parsed query.

```csharp
// Parse into strongly-typed objects
using var parser = new Parser();
var mySelect = parser.ParseAs<object>("SELECT id, name FROM users");

Console.WriteLine($"Parsed object: {mySelect}");
```

### Static `Parser` Quick Methods

For one-off operations without creating a parser instance.

- `QuickParse(query, options)`
- `QuickNormalize(query)`
- `QuickFingerprint(query)`
- `QuickDeparse(ast)`
- `QuickSplit(query)`
- `QuickScan(query)`
- `QuickParsePlpgsql(code)`
- `QuickScanWithProtobuf(query)`

### Async Support (`ParserAsync` extensions)

All core methods have async counterparts.

```csharp
using Npgquery;

// Async parsing on an instance
using var parser = new Parser();
var result = await parser.ParseAsync("SELECT * FROM users WHERE id = 1");

// Static async method for quick one-off parsing
var quickResult = await ParserAsync.QuickParseAsync("SELECT * FROM users");
```

- `ParseAsync(query, options)`
- `NormalizeAsync(query)`
- `FingerprintAsync(query)`
- `DeparseAsync(ast)`
- `SplitAsync(query)`
- `ScanAsync(query)`
- `ParsePlpgsqlAsync(code)`
- `ParseAsAsync<T>(query, options)`
- `IsValidAsync(query)`
- `ParseManyAsync(queries, options, maxDegreeOfParallelism)`: Parse multiple queries in parallel.

### Static Async Quick Methods (`ParserAsync`)

- `QuickParseAsync(query, options)`
- `QuickNormalizeAsync(query)`
- `QuickFingerprintAsync(query)`
- `QuickDeparseAsync(ast)`
- `QuickSplitAsync(query)`
- `QuickScanAsync(query)`
- `QuickParsePlpgsqlAsync(code)`

### Utility Functions (`QueryUtils`)

A static class with helper methods for common tasks.

- `ExtractTableNames(query)`: Get a list of all table names from a query.
- `GetQueryType(query)`: Get the statement type (e.g., "SELECT", "INSERT").
- `SplitStatements(sqlText)`: Split a string into a list of individual statements.
- `GetTokens(query)`: Get a list of all tokens from a query.
- `GetKeywords(query)`: Get a list of unique keywords from a query.
- `CountStatements(sqlText)`: Count the number of statements in a string.
- `CleanQuery(query)`: A convenient alias for `Parser.QuickNormalize(query)`.
- `NormalizeStatements(sqlText)`: Splits a multi-statement string and normalizes each one.
- `HaveSameStructure(query1, query2)`: Check if two queries have the same fingerprint.
- `AstToSql(parseTree)`: Convert a JSON AST back to an SQL string.
- `RoundTripTest(query)`: A utility to parse a query and deparse it back, checking for consistency.
- `IsValidPlpgsql(plpgsqlCode)`: Check if a string of PL/pgSQL code is valid.
- `ValidateQueries(queries)`: Validate a collection of queries.
- `GetQueryErrors(queries)`: Get detailed errors for a collection of queries.

## Advanced Usage

### Parse Options

The `ParseOptions` class provides several configuration options to customize the parsing behavior:

#### Available Options

**`IncludeLocations`** (boolean, default: `false`)
- When set to `true`, the resulting Abstract Syntax Tree (AST) will include location information for each node
- Location information shows the character position in the original query where each element was found
- Useful for analysis tools, syntax highlighting, or error reporting that need to map back to the original query text
- Note: Including locations increases the size of the parse tree output

**`PostgreSqlVersion`** (integer, default: `160000`)
- Specifies the PostgreSQL version number for the parser to target
- Format: Major version × 10000 + Minor version × 100 + Patch version
- Examples:
  - `170000` = PostgreSQL 17.0
  - `160000` = PostgreSQL 16.0 (default)
  - `150000` = PostgreSQL 15.0
  - `140000` = PostgreSQL 14.0
- Useful for ensuring compatibility with specific PostgreSQL versions
- Parser behavior may vary slightly between versions for edge cases

#### Usage Examples

```csharp
using Npgquery;

// Basic usage with default options
using var parser = new Parser();
var result = parser.Parse("SELECT * FROM users");

// Include location information in parse tree
var optionsWithLocations = new ParseOptions
{
    IncludeLocations = true
};
var resultWithLocations = parser.Parse("SELECT * FROM users WHERE id = 1", optionsWithLocations);

// Target a specific PostgreSQL version
var optionsForPg15 = new ParseOptions
{
    PostgreSqlVersion = 150000 // PostgreSQL 15
};
var resultForPg15 = parser.Parse("SELECT * FROM users", optionsForPg15);

// Combine multiple options
var combinedOptions = new ParseOptions
{
    IncludeLocations = true,
    PostgreSqlVersion = 140000 // PostgreSQL 14
};
var combinedResult = parser.Parse("SELECT * FROM users", combinedOptions);

// Using with static methods
var quickResult = Parser.QuickParse("SELECT * FROM users", combinedOptions);

// Using with async methods
var asyncResult = await parser.ParseAsync("SELECT * FROM users", combinedOptions);
```

#### When to Use Parse Options

- **Include Locations**: Enable when building tools that need to:
  - Highlight syntax in editors
  - Show precise error locations
  - Generate source maps for query transformations
  - Build refactoring tools that modify specific parts of queries

- **PostgreSQL Version**: Specify when:
  - Working with legacy systems running older PostgreSQL versions
  - Ensuring compatibility across different PostgreSQL deployments
  - Testing queries against specific PostgreSQL feature sets
  - Building tools that need to support multiple PostgreSQL versions

#### Performance Considerations

- Including locations adds overhead to parsing and increases memory usage
- Version-specific parsing differences are minimal for most common queries
- For high-throughput scenarios, consider reusing the same `ParseOptions` instance

### Custom Parse Options

```csharp
using var parser = new Parser();
var options = new ParseOptions
{
    IncludeLocations = true,
    PostgreSqlVersion = 160000 // PostgreSQL 16
};

var result = parser.Parse("SELECT * FROM users", options);
```

### Strongly-Typed AST

```csharp
// Parse into strongly-typed objects
using var parser = new Parser();
var ast = parser.ParseAs<object>("SELECT * FROM users");
```

### Batch Processing

```csharp
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
```

### Error Handling

The library uses custom exception types for different operations.

```csharp
using var parser = new Parser();
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
```

## Performance Tips

1.  **Reuse parser instances**: Avoid creating new `Parser` instances for each operation.
2.  **Use async methods**: Offload work to a background thread for better responsiveness in UI or server applications.
3.  **Process in parallel**: Use `ParseManyAsync` for high-throughput batch processing.
4.  **Dispose properly**: Use `using` statements or call `Dispose()` to release resources.
5.  **Use static Quick methods**: Ideal for infrequent, one-off operations.
6.  **Cache results**: Cache parsing results for frequently seen queries.

## Native Dependencies

This library requires the `libpg_query` native library. The NuGet package includes pre-compiled binaries for:

- Windows (x64, ARM64)
- Linux (x64, ARM64)
- macOS (x64, ARM64)

## Supported Features

### ✅ Implemented (Core Features)
- SQL parsing to AST (JSON & Protobuf)
- Query normalization
- Query fingerprinting
- AST deparsing to SQL
- Multi-statement splitting
- Query tokenization/scanning
- PL/pgSQL parsing
- Comprehensive error handling
- Async operations
- Batch processing

### ✨ .NET-Specific Enhancements
- Strong typing with nullable reference types
- Record types for immutable data models
- Extension methods for a fluent async API
- Comprehensive XML documentation
- Advanced error handling with custom exceptions
- Static utility class (`QueryUtils`) for common operations
- Performance optimizations for high-throughput scenarios

## Thread Safety

The `Parser` class is **not thread-safe**. Create separate instances for each thread or use proper synchronization. The static `Quick*` methods and `QueryUtils` methods are thread-safe.

## Contributing

Contributions are welcome! Please ensure that:

1. All tests pass.
2. Code follows .NET conventions.
3. Public APIs are documented with XML comments.
4. Memory management is handled properly.

## License

MIT License - see the LICENSE file for details.

## Acknowledgments

This library is built on top of the excellent [libpg_query](https://github.com/pganalyze/libpg_query) project, which embeds the PostgreSQL parser.