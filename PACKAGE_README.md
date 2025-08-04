# Npgquery - PostgreSQL Query Parser for .NET

A high-performance .NET library for parsing PostgreSQL queries using the official PostgreSQL parser via libpg_query.

## Quick Start

```csharp
using Npgquery;

// Parse a query
using var parser = new Parser();
var result = parser.Parse("SELECT * FROM users WHERE id = 1");

if (result.IsSuccess)
{
    Console.WriteLine(result.ParseTree); // JSON AST
}

// Quick one-liners
var normalized = Parser.QuickNormalize("SELECT * FROM users WHERE id = 1");
var fingerprint = Parser.QuickFingerprint("SELECT * FROM users WHERE id = 1");
```

## Key Features

- **Parse SQL to AST**: Convert PostgreSQL queries to JSON or Protobuf AST
- **Query Normalization**: Standardize queries for comparison
- **Query Fingerprinting**: Generate unique identifiers for query patterns
- **Statement Splitting**: Split multi-statement SQL into individual statements
- **Query Tokenization**: Analyze SQL tokens and keywords
- **PL/pgSQL Support**: Parse PL/pgSQL functions and procedures
- **Async Operations**: Full async/await support
- **Batch Processing**: Process multiple queries efficiently
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Installation

```
dotnet add package Npgquery
```

## Documentation

Visit the [GitHub repository](https://github.com/yourusername/Npgquery) for complete documentation, examples, and API reference.

## License

MIT License - see LICENSE file for details.

This library is built on [libpg_query](https://github.com/pganalyze/libpg_query), which embeds the official PostgreSQL parser.