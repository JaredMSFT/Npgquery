# Changelog

All notable changes to NpgqueryLib will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-12-19

### Added
- Initial release of NpgqueryLib for .NET 9
- **Complete PostgreSQL query parsing functionality** equivalent to other language wrappers
- **Core parsing features:**
  - PostgreSQL query parsing into Abstract Syntax Trees (AST)
  - Query normalization for removing comments and standardizing formatting
  - Query fingerprinting for similarity comparison
- **Extended parsing features** (same as Go, Rust, Python, JavaScript wrappers):
  - **AST deparsing** - Convert parse trees back to SQL queries
  - **Multi-statement splitting** - Split SQL files with location information
  - **Query tokenization/scanning** - Detailed token analysis with positions
  - **PL/pgSQL parsing** - Parse stored procedures and functions
- **Async/await support** with `NpgqueryAsync` extension methods
- **Batch processing** with `ParseManyAsync` for parallel query processing
- **Comprehensive utility functions:**
  - Extract table names and query types
  - Split statements with `QueryUtils.SplitStatements`
  - Get tokens and keywords with `QueryUtils.GetTokens/GetKeywords`
  - Round-trip testing (parse ? deparse)
  - Statement counting and PL/pgSQL validation
- **Strong typing** with nullable reference types and records
- **Memory-safe** native interop with automatic resource cleanup
- **Comprehensive error handling** with custom exception types for all operations
- **JSON serialization** support for AST objects
- **Support for PostgreSQL versions** 10-16
- **Cross-platform support** (Windows, Linux, macOS) for x64 and ARM64
- **Thread-safe static methods** for quick operations
- **Configurable parse options** including location information
- **Extensive unit tests** with both sync and async test coverage
- **Comprehensive documentation** and examples
- **Performance optimizations** for high-throughput scenarios

### Core API Methods
- **Parse**: `Parse()`, `ParseAsync()`, `QuickParse()`, `QuickParseAsync()`
- **Normalize**: `Normalize()`, `NormalizeAsync()`, `QuickNormalize()`, `QuickNormalizeAsync()`
- **Fingerprint**: `Fingerprint()`, `FingerprintAsync()`, `QuickFingerprint()`, `QuickFingerprintAsync()`
- **Deparse**: `Deparse()`, `DeparseAsync()`, `QuickDeparse()`, `QuickDeparseAsync()`
- **Split**: `Split()`, `SplitAsync()`, `QuickSplit()`, `QuickSplitAsync()`
- **Scan**: `Scan()`, `ScanAsync()`, `QuickScan()`, `QuickScanAsync()`
- **PL/pgSQL**: `ParsePlpgsql()`, `ParsePlpgsqlAsync()`, `QuickParsePlpgsql()`, `QuickParsePlpgsqlAsync()`
- **Validation**: `IsValid()`, `IsValidAsync()`, `GetError()`

### Data Models
- **ParseResult** - Query parsing results with AST
- **NormalizeResult** - Query normalization results
- **FingerprintResult** - Query fingerprinting results
- **DeparseResult** - AST deparsing results
- **SplitResult** - Multi-statement splitting results with `SqlStatement` objects
- **ScanResult** - Query scanning results with `SqlToken` objects
- **PlpgsqlParseResult** - PL/pgSQL parsing results
- **ParseOptions** - Configurable parsing options

### Exception Types
- **NpgqueryException** - Base exception class
- **ParseException** - SQL parsing errors
- **NormalizationException** - Normalization errors
- **FingerprintException** - Fingerprinting errors
- **DeparseException** - AST deparsing errors
- **SplitException** - Statement splitting errors
- **ScanException** - Query scanning errors
- **PlpgsqlParseException** - PL/pgSQL parsing errors
- **NativeLibraryException** - Native library loading errors

### Utility Functions
- **ExtractTableNames()** - Get all table names from queries
- **GetQueryType()** - Determine query type (SELECT, INSERT, etc.)
- **HaveSameStructure()** - Compare query structures via fingerprints
- **CleanQuery()** - Clean and normalize queries
- **SplitStatements()** - Split multi-statement SQL strings
- **GetTokens()** - Extract all tokens from queries
- **GetKeywords()** - Extract SQL keywords from queries
- **AstToSql()** - Convert AST JSON to SQL
- **RoundTripTest()** - Test parse ? deparse round-trip
- **IsValidPlpgsql()** - Validate PL/pgSQL code
- **CountStatements()** - Count statements in SQL strings
- **ValidateQueries()** - Batch query validation
- **GetQueryErrors()** - Batch error checking
- **NormalizeStatements()** - Normalize multiple statements

### API Design Features
- **Modern .NET 9 C#** with nullable reference types
- **Record types** for immutable data structures
- **Extension methods** for async operations
- **IDisposable pattern** for proper resource management
- **Static factory methods** for convenient one-off operations
- **Fluent API design** with method chaining support

### Performance Features
- **Parallel processing** support with configurable concurrency
- **Memory-efficient** native interop with P/Invoke
- **Minimal allocation** patterns for high-performance scenarios
- **Optimized for batch processing** with `ParseManyAsync`
- **Resource pooling** with reusable parser instances

### Developer Experience
- **Comprehensive XML documentation** for all public APIs
- **IntelliSense support** with detailed parameter descriptions
- **Clear error messages** with contextual information
- **Extensive examples** demonstrating all functionality
- **Unit tests** covering both synchronous and asynchronous operations
- **Performance benchmarks** and optimization guidelines

### Native Library Integration
- **P/Invoke bindings** for all libpg_query functions:
  - `pg_query_parse` - Parse SQL to AST
  - `pg_query_normalize` - Normalize SQL
  - `pg_query_fingerprint` - Generate fingerprints
  - `pg_query_deparse` - Deparse AST to SQL
  - `pg_query_split_with_scanner` - Split statements
  - `pg_query_scan` - Tokenize queries
  - `pg_query_parse_plpgsql` - Parse PL/pgSQL
- **Memory management** with automatic cleanup of native resources
- **Cross-platform** native library distribution

### Compatibility
- **.NET 9.0** target framework
- **C# 13.0** language features
- **PostgreSQL 10, 11, 12, 13, 14, 15, and 16** syntax support
- **Cross-platform**: Windows, Linux, macOS
- **Multi-architecture**: x64, ARM64
- **Same API surface** as popular wrappers in Go, Rust, Python, JavaScript

### Package Features
- **NuGet package** with embedded native libraries
- **Runtime-specific** native library distribution
- **Comprehensive documentation** with examples
- **MIT license** with proper attributions
- **Semantic versioning** for stable releases