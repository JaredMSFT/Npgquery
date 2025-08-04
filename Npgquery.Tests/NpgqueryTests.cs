using System.Text.Json;
using Google.Protobuf;
using NpgqueryLib;
using PgQuery;
using Xunit;

namespace NpgqueryLib.Tests;

public class NpgqueryTests : IDisposable
{
    private readonly Npgquery _parser;

    public NpgqueryTests()
    {
        _parser = new Npgquery();
    }

    public void Dispose()
    {
        _parser.Dispose();
    }

    [Fact]
    public void Parse_ValidQuery_ReturnsSuccess()
    {
        // Arrange
        var query = "SELECT * FROM users WHERE id = 1";

        // Act
        var result = _parser.Parse(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ParseTree);
        Assert.Null(result.Error);
        Assert.Equal(query, result.Query);
    }

    [Fact]
    public void Parse_InvalidQuery_ReturnsError()
    {
        // Arrange
        var query = "INVALID SQL SYNTAX";

        // Act
        var result = _parser.Parse(query);

        // Assert
        Assert.True(result.IsError);
        Assert.Null(result.ParseTree);
        Assert.NotNull(result.Error);
        Assert.Equal(query, result.Query);
    }

    [Fact]
    public void Normalize_ValidQuery_ReturnsNormalized()
    {
        // Arrange
        var query = "SELECT * FROM users /* comment */ WHERE id = 1";

        // Act
        var result = _parser.Normalize(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.NormalizedQuery);
        Assert.Null(result.Error);
        Assert.Contains("SELECT", result.NormalizedQuery);
        //Assert.DoesNotContain("comment", result.NormalizedQuery); //TODO determine if the response is accurate
    }

    [Fact]
    public void Fingerprint_SimilarQueries_ReturnsSameFingerprint()
    {
        // Arrange
        var query1 = "SELECT * FROM users WHERE id = 1";
        var query2 = "SELECT * FROM users WHERE id = 2";

        // Act
        var fp1 = _parser.Fingerprint(query1);
        var fp2 = _parser.Fingerprint(query2);

        // Assert
        Assert.True(fp1.IsSuccess);
        Assert.True(fp2.IsSuccess);
        Assert.Equal(fp1.Fingerprint, fp2.Fingerprint);
    }

    [Fact]
    public void IsValid_ValidQuery_ReturnsTrue()
    {
        // Arrange
        var query = "SELECT 1";

        // Act
        var isValid = _parser.IsValid(query);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_InvalidQuery_ReturnsFalse()
    {
        // Arrange
        var query = "INVALID SQL";

        // Act
        var isValid = _parser.IsValid(query);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GetError_InvalidQuery_ReturnsErrorMessage()
    {
        // Arrange
        var query = "INVALID SQL";

        // Act
        var error = _parser.GetError(query);

        // Assert
        Assert.NotNull(error);
        Assert.NotEmpty(error);
    }

    [Fact]
    public void GetError_ValidQuery_ReturnsNull()
    {
        // Arrange
        var query = "SELECT 1";

        // Act
        var error = _parser.GetError(query);

        // Assert
        Assert.Null(error);
    }

    [Theory]
    [InlineData("SELECT * FROM users", "SELECT")]
    [InlineData("INSERT INTO users (name) VALUES ('test')", "INSERT")]
    [InlineData("UPDATE users SET name = 'test'", "UPDATE")]
    [InlineData("DELETE FROM users WHERE id = 1", "DELETE")]
    public void QuickParse_StaticMethod_Works(string query, string expectedType)
    {
        // Act
        var result = Npgquery.QuickParse(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ParseTree);
        
        // Use the expectedType parameter to verify the query type
        var actualType = QueryUtils.GetQueryType(query);
        Assert.Equal(expectedType, actualType);
    }

    [Fact]
    public void Parse_WithNullQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.Parse(null!));
    }

    [Fact]
    public void Parse_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        _parser.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => _parser.Parse("SELECT 1"));
    }
}

public class QueryUtilsTests
{
    [Fact]
    public void ExtractTableNames_SimpleQuery_ReturnsTableNames()
    {
        // Arrange
        var query = "SELECT * FROM users WHERE id = 1";

        // Act
        var tables = QueryUtils.ExtractTableNames(query);

        // Assert
        Assert.Contains("users", tables);
    }

    [Fact]
    public void ExtractTableNames_JoinQuery_ReturnsAllTableNames()
    {
        // Arrange
        var query = "SELECT u.name, p.title FROM users u JOIN posts p ON u.id = p.user_id";

        // Act
        var tables = QueryUtils.ExtractTableNames(query);

        // Assert
        Assert.Contains("users", tables);
        Assert.Contains("posts", tables);
    }

    [Fact]
    public void HaveSameStructure_SimilarQueries_ReturnsTrue()
    {
        // Arrange
        var query1 = "SELECT * FROM users WHERE id = 1";
        var query2 = "SELECT * FROM users WHERE id = 2";

        // Act
        var result = QueryUtils.HaveSameStructure(query1, query2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HaveSameStructure_DifferentQueries_ReturnsFalse()
    {
        // Arrange
        var query1 = "SELECT * FROM users";
        var query2 = "INSERT INTO users (name) VALUES ('test')";

        // Act
        var result = QueryUtils.HaveSameStructure(query1, query2);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("SELECT * FROM users", "SELECT")]
    [InlineData("INSERT INTO users (name) VALUES ('test')", "INSERT")]
    [InlineData("UPDATE users SET name = 'test'", "UPDATE")]
    [InlineData("DELETE FROM users WHERE id = 1", "DELETE")]
    public void GetQueryType_VariousQueries_ReturnsCorrectType(string query, string expectedType)
    {
        // Act
        var queryType = QueryUtils.GetQueryType(query);

        // Assert
        Assert.Equal(expectedType, queryType);
    }

    [Fact]
    public void CleanQuery_QueryWithWhitespace_ReturnsCleanedQuery()
    {
        // Arrange
        var query = "  SELECT   *   FROM   users  ";

        // Act
        var cleaned = QueryUtils.CleanQuery(query);

        // Assert
        Assert.False(cleaned.StartsWith(" "));
        Assert.False(cleaned.EndsWith(" "));
    }

    [Fact]
    public void ValidateQueries_MixedQueries_ReturnsValidationResults()
    {
        // Arrange
        var queries = new[] { "SELECT 1", "INVALID SQL", "SELECT 2" };

        // Act
        var results = QueryUtils.ValidateQueries(queries);

        // Assert
        Assert.True(results["SELECT 1"]);
        Assert.False(results["INVALID SQL"]);
        Assert.True(results["SELECT 2"]);
    }

    [Fact]
    public void Parse_SerializeToProtobuf_And_Deparse() {
        // Arrange
        using var parser = new Npgquery();
        const string query = "SELECT id, name FROM users WHERE active = true";

        // Act
        // Step 1: Parse the SQL query to get the parse tree
        var parseResult = parser.Parse(query);
        Assert.True(parseResult.IsSuccess);
        Assert.NotNull(parseResult.ParseTree);

        // Step 2: Convert JSON parse tree to protobuf
        var parseTreeJson = parseResult.ParseTree.RootElement.GetRawText();

        // Create a ParseResult protobuf message
        var protoParseResult = new PgQuery.ParseResult();

        // Parse the JSON to populate the protobuf structure
        // This is a simplified example - you'll need to map the JSON structure to protobuf
        var jsonDoc = JsonDocument.Parse(parseTreeJson);

        if (jsonDoc.RootElement.TryGetProperty("stmts", out var stmtsElement)) {
            foreach (var stmtElement in stmtsElement.EnumerateArray()) {
                var rawStmt = new RawStmt();

                // Map the SELECT statement
                if (stmtElement.TryGetProperty("stmt", out var stmt) &&
                    stmt.TryGetProperty("SelectStmt", out var selectStmt)) {
                    var selectNode = new SelectStmt();

                    // Map target list (SELECT columns)
                    if (selectStmt.TryGetProperty("targetList", out var targetList)) {
                        foreach (var target in targetList.EnumerateArray()) {
                            var resTarget = new ResTarget();
                            if (target.TryGetProperty("ResTarget", out var resTargetJson)) {
                                if (resTargetJson.TryGetProperty("name", out var name)) {
                                    resTarget.Name = name.GetString();
                                }
                            }
                            selectNode.TargetList.Add(new Node { ResTarget = resTarget });
                        }
                    }

                    // Map FROM clause
                    if (selectStmt.TryGetProperty("fromClause", out var fromClause)) {
                        foreach (var from in fromClause.EnumerateArray()) {
                            if (from.TryGetProperty("RangeVar", out var rangeVar)) {
                                var rangeVarNode = new RangeVar();
                                if (rangeVar.TryGetProperty("relname", out var relname)) {
                                    rangeVarNode.Relname = relname.GetString();
                                }
                                selectNode.FromClause.Add(new Node { RangeVar = rangeVarNode });
                            }
                        }
                    }

                    rawStmt.Stmt = new Node { SelectStmt = selectNode };
                }

                protoParseResult.Stmts.Add(rawStmt);
            }
        }

        // Step 3: Serialize to protobuf bytes
        var protobufBytes = protoParseResult.ToByteArray();

        // Step 4: Deserialize back (to verify serialization worked)
        var deserializedProto = PgQuery.ParseResult.Parser.ParseFrom(protobufBytes);

        // Step 5: Convert protobuf back to JSON for deparse
        var protoJson = JsonFormatter.Default.Format(deserializedProto);
        var protoJsonDoc = JsonDocument.Parse(protoJson);

        // Step 6: Call deparse with the protobuf-based parse tree
        var deparseResult = parser.Deparse(protoJsonDoc);

        // Assert
        Assert.True(deparseResult.IsSuccess);
        Assert.NotNull(deparseResult.Query);
        Assert.Contains("SELECT", deparseResult.Query);
        Assert.Contains("users", deparseResult.Query);
    }

    [Fact]
    public void SimpleSelect_RoundTrip_Through_Protobuf() {
        // Arrange
        using var parser = new Npgquery();
        const string query = "SELECT 1";

        // Act
        var parseResult = parser.Parse(query);
        Assert.True(parseResult.IsSuccess);

        // For a simple test, we can just verify the parse tree structure
        var parseTreeJson = parseResult.ParseTree.RootElement.GetRawText();

        // Create a minimal protobuf representation
        var protoResult = new PgQuery.ParseResult {
            Version = 160001 // PostgreSQL 16.0.1
        };

        var rawStmt = new RawStmt();
        var selectStmt = new SelectStmt();

        // Add a simple integer constant to target list
        var resTarget = new ResTarget();
        var aConst = new A_Const();
        aConst.Ival = new Integer { Ival = 1 };
        resTarget.Val = new Node { AConst = aConst };

        selectStmt.TargetList.Add(new Node { ResTarget = resTarget });
        rawStmt.Stmt = new Node { SelectStmt = selectStmt };
        protoResult.Stmts.Add(rawStmt);

        // Serialize and deserialize
        var bytes = protoResult.ToByteArray();
        var deserialized = PgQuery.ParseResult.Parser.ParseFrom(bytes);

        // Convert to JSON for deparse
        var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
        var json = jsonFormatter.Format(deserialized);
        var jsonDoc = JsonDocument.Parse(json);

        var deparseResult = parser.Deparse(jsonDoc);

        // Assert
        Assert.True(deparseResult.IsSuccess);
        Assert.NotNull(deparseResult.Query);
    }

}