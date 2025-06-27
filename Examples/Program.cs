using NpgqueryLib;

namespace Examples;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("NpgqueryLib Examples");
        Console.WriteLine("===================");

        await BasicParsingExample();
        Console.WriteLine();
        
        await NormalizationExample();
        Console.WriteLine();
        
        await FingerprintingExample();
        Console.WriteLine();
        
        await UtilityFunctionsExample();
        Console.WriteLine();
        
        await AsyncExample();
        Console.WriteLine();
        
        await BatchProcessingExample();
        Console.WriteLine();
        
        await ExtendedFeaturesExample();
    }

    static async Task BasicParsingExample()
    {
        Console.WriteLine("1. Basic Parsing Example");
        Console.WriteLine("------------------------");

        using var parser = new Npgquery();
        
        var queries = new[]
        {
            "SELECT * FROM users WHERE id = 1",
            "INSERT INTO posts (title, content) VALUES ('Hello', 'World')",
            "INVALID SQL SYNTAX"
        };

        foreach (var query in queries)
        {
            var result = parser.Parse(query);
            Console.WriteLine($"Query: {query}");
            Console.WriteLine($"Valid: {result.IsSuccess}");
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Parse Tree Length: {result.ParseTree?.Length ?? 0} characters");
            }
            else
            {
                Console.WriteLine($"Error: {result.Error}");
            }
            Console.WriteLine();
        }
    }

    static async Task NormalizationExample()
    {
        Console.WriteLine("2. Normalization Example");
        Console.WriteLine("------------------------");

        using var parser = new Npgquery();
        
        var queries = new[]
        {
            "SELECT * FROM users /* this is a comment */ WHERE id = 1",
            "SELECT   *   FROM   users   WHERE   id   =   2  ",
            "select name, email from users where active = true"
        };

        foreach (var query in queries)
        {
            var result = parser.Normalize(query);
            Console.WriteLine($"Original:   {query}");
            Console.WriteLine($"Normalized: {result.NormalizedQuery}");
            Console.WriteLine();
        }
    }

    static async Task FingerprintingExample()
    {
        Console.WriteLine("3. Fingerprinting Example");
        Console.WriteLine("-------------------------");

        using var parser = new Npgquery();
        
        var queries = new[]
        {
            "SELECT * FROM users WHERE id = 1",
            "SELECT * FROM users WHERE id = 2",
            "SELECT * FROM users WHERE id = 999",
            "SELECT name FROM users WHERE id = 1"
        };

        var fingerprints = new List<(string query, string? fingerprint)>();
        
        foreach (var query in queries)
        {
            var result = parser.Fingerprint(query);
            fingerprints.Add((query, result.Fingerprint));
            Console.WriteLine($"Query: {query}");
            Console.WriteLine($"Fingerprint: {result.Fingerprint}");
            Console.WriteLine();
        }

        // Check for similar queries
        Console.WriteLine("Similar query analysis:");
        for (int i = 0; i < fingerprints.Count; i++)
        {
            for (int j = i + 1; j < fingerprints.Count; j++)
            {
                if (fingerprints[i].fingerprint == fingerprints[j].fingerprint)
                {
                    Console.WriteLine($"Queries {i + 1} and {j + 1} have the same structure");
                }
            }
        }
        Console.WriteLine();
    }

    static async Task UtilityFunctionsExample()
    {
        Console.WriteLine("4. Utility Functions Example");
        Console.WriteLine("-----------------------------");

        var complexQuery = @"
            SELECT u.name, u.email, p.title, c.content
            FROM users u
            JOIN posts p ON u.id = p.user_id
            LEFT JOIN comments c ON p.id = c.post_id
            WHERE u.active = true
            AND p.published_at > '2023-01-01'
            ORDER BY p.published_at DESC
            LIMIT 10
        ";

        // Extract table names
        var tables = QueryUtils.ExtractTableNames(complexQuery);
        Console.WriteLine("Tables found:");
        foreach (var table in tables)
        {
            Console.WriteLine($"  - {table}");
        }
        Console.WriteLine();

        // Get query type
        var queryType = QueryUtils.GetQueryType(complexQuery);
        Console.WriteLine($"Query type: {queryType}");
        Console.WriteLine();

        // Clean query
        var cleaned = QueryUtils.CleanQuery(complexQuery);
        Console.WriteLine("Cleaned query:");
        Console.WriteLine(cleaned);
        Console.WriteLine();

        // Validate multiple queries
        var testQueries = new[]
        {
            "SELECT 1",
            "INVALID SQL",
            "INSERT INTO test VALUES (1)",
            "DELETE FROM test WHERE id = 1"
        };

        var validationResults = QueryUtils.ValidateQueries(testQueries);
        Console.WriteLine("Validation results:");
        foreach (var (query, isValid) in validationResults)
        {
            Console.WriteLine($"  {query}: {(isValid ? "? Valid" : "? Invalid")}");
        }
    }

    static async Task AsyncExample()
    {
        Console.WriteLine("5. Async Operations Example");
        Console.WriteLine("----------------------------");

        using var parser = new Npgquery();

        // Single async operation
        var query = "SELECT * FROM users WHERE created_at > '2023-01-01'";
        var result = await parser.ParseAsync(query);
        Console.WriteLine($"Async parse successful: {result.IsSuccess}");

        // Multiple queries in parallel
        var queries = new[]
        {
            "SELECT COUNT(*) FROM users",
            "SELECT COUNT(*) FROM posts",
            "SELECT COUNT(*) FROM comments",
            "SELECT COUNT(*) FROM categories"
        };

        Console.WriteLine($"Processing {queries.Length} queries in parallel...");
        var startTime = DateTime.UtcNow;
        var results = await parser.ParseManyAsync(queries, maxDegreeOfParallelism: 4);
        var endTime = DateTime.UtcNow;

        Console.WriteLine($"Completed in {(endTime - startTime).TotalMilliseconds:F2}ms");
        Console.WriteLine($"Successful parses: {results.Count(r => r.IsSuccess)}/{results.Length}");
        Console.WriteLine();

        // Static async methods
        var quickResult = await NpgqueryAsync.QuickParseAsync("SELECT version()");
        Console.WriteLine($"Quick async parse successful: {quickResult.IsSuccess}");
    }

    static async Task BatchProcessingExample()
    {
        Console.WriteLine("6. Batch Processing Example");
        Console.WriteLine("----------------------------");

        // Simulate processing a file of SQL queries
        var sqlQueries = new[]
        {
            "-- User management queries",
            "SELECT * FROM users WHERE active = true;",
            "UPDATE users SET last_login = NOW() WHERE id = 1;",
            "-- This is an invalid query",
            "SELECT * FORM users;", // Typo: FORM instead of FROM
            "DELETE FROM users WHERE id = 999;",
            "-- Post queries",
            "SELECT p.*, u.name FROM posts p JOIN users u ON p.user_id = u.id;",
            "INSERT INTO posts (title, content, user_id) VALUES ('Test', 'Content', 1);"
        };

        Console.WriteLine($"Processing {sqlQueries.Length} SQL statements...");

        var validQueries = new List<string>();
        var invalidQueries = new List<(string query, string error)>();

        using var parser = new Npgquery();

        foreach (var query in sqlQueries)
        {
            // Skip comments and empty lines
            var trimmed = query.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("--"))
            {
                continue;
            }

            var result = parser.Parse(trimmed);
            if (result.IsSuccess)
            {
                validQueries.Add(trimmed);
                
                // Extract metadata
                var queryType = QueryUtils.GetQueryType(trimmed);
                var tables = QueryUtils.ExtractTableNames(trimmed);
                
                Console.WriteLine($"? {queryType} query affecting tables: {string.Join(", ", tables)}");
            }
            else
            {
                invalidQueries.Add((trimmed, result.Error!));
                Console.WriteLine($"? Invalid query: {trimmed}");
                Console.WriteLine($"  Error: {result.Error}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Summary:");
        Console.WriteLine($"  Valid queries: {validQueries.Count}");
        Console.WriteLine($"  Invalid queries: {invalidQueries.Count}");

        if (validQueries.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Generating fingerprints for valid queries...");
            var fingerprints = new Dictionary<string, List<string>>();

            foreach (var query in validQueries)
            {
                var fp = parser.Fingerprint(query);
                if (fp.IsSuccess && fp.Fingerprint != null)
                {
                    if (!fingerprints.ContainsKey(fp.Fingerprint))
                    {
                        fingerprints[fp.Fingerprint] = new List<string>();
                    }
                    fingerprints[fp.Fingerprint].Add(query);
                }
            }

            var duplicateStructures = fingerprints.Where(kvp => kvp.Value.Count > 1).ToList();
            if (duplicateStructures.Any())
            {
                Console.WriteLine("Found queries with similar structures:");
                foreach (var (fingerprint, similarQueries) in duplicateStructures)
                {
                    Console.WriteLine($"  Fingerprint: {fingerprint}");
                    foreach (var similarQuery in similarQueries)
                    {
                        Console.WriteLine($"    - {similarQuery}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No duplicate query structures found.");
            }
        }
    }

    static async Task ExtendedFeaturesExample()
    {
        Console.WriteLine("7. Extended Features Example");
        Console.WriteLine("----------------------------");

        using var parser = new Npgquery();

        // Query splitting example
        Console.WriteLine("A. Query Splitting:");
        var multiQuery = @"
            SELECT * FROM users WHERE active = true;
            INSERT INTO audit_log (action, timestamp) VALUES ('query', NOW());
            UPDATE users SET last_accessed = NOW() WHERE id = 1;
        ";

        var splitResult = parser.Split(multiQuery);
        if (splitResult.IsSuccess && splitResult.Statements != null)
        {
            Console.WriteLine($"Found {splitResult.Statements.Length} statements:");
            foreach (var stmt in splitResult.Statements)
            {
                if (!string.IsNullOrWhiteSpace(stmt.Statement))
                {
                    Console.WriteLine($"  - {stmt.Statement.Trim()}");
                }
            }
        }
        Console.WriteLine();

        // Query scanning/tokenization example
        Console.WriteLine("B. Query Tokenization:");
        var query = "SELECT COUNT(*) FROM users WHERE created_at > '2023-01-01'";
        var scanResult = parser.Scan(query);
        if (scanResult.IsSuccess && scanResult.Tokens != null)
        {
            Console.WriteLine($"Query: {query}");
            Console.WriteLine($"Found {scanResult.Tokens.Length} tokens:");
            foreach (var token in scanResult.Tokens.Take(10)) // Show first 10 tokens
            {
                Console.WriteLine($"  Token {token.Token}: {token.KeywordKind ?? "N/A"} (pos {token.Start}-{token.End})");
            }
            if (scanResult.Tokens.Length > 10)
            {
                Console.WriteLine($"  ... and {scanResult.Tokens.Length - 10} more tokens");
            }
        }
        Console.WriteLine();

        // Round-trip example (parse then deparse)
        Console.WriteLine("C. Round-trip Test (Parse ? Deparse):");
        var originalQuery = "SELECT u.name, COUNT(p.id) as post_count FROM users u LEFT JOIN posts p ON u.id = p.user_id GROUP BY u.id, u.name ORDER BY post_count DESC";
        Console.WriteLine($"Original: {originalQuery}");

        var parseResult = parser.Parse(originalQuery);
        if (parseResult.IsSuccess && !string.IsNullOrEmpty(parseResult.ParseTree))
        {
            var deparseResult = parser.Deparse(parseResult.ParseTree);
            if (deparseResult.IsSuccess)
            {
                Console.WriteLine($"Deparsed: {deparseResult.Query}");
                Console.WriteLine($"Round-trip successful: {!string.IsNullOrEmpty(deparseResult.Query)}");
            }
            else
            {
                Console.WriteLine($"Deparse failed: {deparseResult.Error}");
            }
        }
        Console.WriteLine();

        // PL/pgSQL parsing example
        Console.WriteLine("D. PL/pgSQL Parsing:");
        var plpgsqlCode = @"
            BEGIN
                IF user_count > 0 THEN
                    RETURN 'Users exist';
                ELSE
                    RETURN 'No users found';
                END IF;
            END;
        ";
        
        var plpgsqlResult = parser.ParsePlpgsql(plpgsqlCode);
        Console.WriteLine($"PL/pgSQL parsing successful: {plpgsqlResult.IsSuccess}");
        if (plpgsqlResult.IsError)
        {
            Console.WriteLine($"Error: {plpgsqlResult.Error}");
        }
        else if (!string.IsNullOrEmpty(plpgsqlResult.ParseTree))
        {
            Console.WriteLine($"Parse tree length: {plpgsqlResult.ParseTree.Length} characters");
        }
        Console.WriteLine();

        // Utility functions example
        Console.WriteLine("E. Enhanced Utility Functions:");
        
        // Split statements utility
        var statements = QueryUtils.SplitStatements(multiQuery);
        Console.WriteLine($"Split {statements.Count} statements using utility function");

        // Get tokens utility
        var tokens = QueryUtils.GetTokens(originalQuery);
        Console.WriteLine($"Found {tokens.Count} tokens using utility function");

        // Get keywords utility
        var keywords = QueryUtils.GetKeywords(originalQuery);
        Console.WriteLine($"Keywords found: {string.Join(", ", keywords)}");

        // Round-trip utility
        var (success, roundTripQuery) = QueryUtils.RoundTripTest(originalQuery);
        Console.WriteLine($"Round-trip test: {(success ? "? Success" : "? Failed")}");

        // Count statements utility
        var statementCount = QueryUtils.CountStatements(multiQuery);
        Console.WriteLine($"Statement count: {statementCount}");

        // PL/pgSQL validation utility
        var isValidPlpgsql = QueryUtils.IsValidPlpgsql(plpgsqlCode);
        Console.WriteLine($"PL/pgSQL validation: {(isValidPlpgsql ? "? Valid" : "? Invalid")}");
    }
}