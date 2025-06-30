using Google.Protobuf;
using PgQuery;
using System.Runtime.InteropServices;
using static NpgqueryLib.Native.NativeMethods;

namespace NpgqueryLib.Native;

/// <summary>
/// Helper class for protobuf operations with libpg_query
/// </summary>
internal static class ProtobufHelper
{
    /// <summary>
    /// Deserialize scan result from protobuf data
    /// </summary>
    /// <param name="protobufData">Raw protobuf data</param>
    /// <param name="originalQuery">Original query string for text extraction</param>
    /// <returns>Processed scan result</returns>
    internal static NativeMethods.ProcessedScanResult DeserializeScanResult(byte[] protobufData, string originalQuery)
    {
        try
        {
            var scanResult = PgQuery.ScanResult.Parser.ParseFrom(protobufData);
            
            return new NativeMethods.ProcessedScanResult
            {
                Version = scanResult.Version,
                Tokens = ConvertProtobufTokensToSqlTokens(scanResult.Tokens, originalQuery),
                Error = null,
                Stderr = null
            };
        }
        catch (Exception ex)
        {
            return new NativeMethods.ProcessedScanResult
            {
                Version = null,
                Tokens = null,
                Error = $"Failed to deserialize protobuf scan result: {ex.Message}",
                Stderr = null
            };
        }
    }

    /// <summary>
    /// Deserialize parse result from protobuf data
    /// </summary>
    /// <param name="protobufData">Raw protobuf data</param>
    /// <returns>Deserialized parse result</returns>
    internal static PgQuery.ParseResult? DeserializeParseResult(byte[] protobufData)
    {
        try
        {
            return PgQuery.ParseResult.Parser.ParseFrom(protobufData);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Convert protobuf ScanTokens to SqlTokens
    /// </summary>
    /// <param name="protobufTokens">Protobuf scan tokens</param>
    /// <param name="originalQuery">Original query for text extraction</param>
    /// <returns>Array of SqlToken objects</returns>
    private static SqlToken[] ConvertProtobufTokensToSqlTokens(
        Google.Protobuf.Collections.RepeatedField<ScanToken> protobufTokens, 
        string originalQuery)
    {
        var tokens = new SqlToken[protobufTokens.Count];
        
        for (int i = 0; i < protobufTokens.Count; i++)
        {
            var protobufToken = protobufTokens[i];
            
            tokens[i] = new SqlToken
            {
                Token = (int)protobufToken.Token,
                TokenKind = protobufToken.Token.ToString(),
                KeywordKind = protobufToken.KeywordKind.ToString(),
                Start = protobufToken.Start,
                End = protobufToken.End,
                Text = ExtractTokenText(originalQuery, protobufToken.Start, protobufToken.End)
            };
        }
        
        return tokens;
    }

    /// <summary>
    /// Extract token text from the original query using start and end positions
    /// </summary>
    /// <param name="query">Original query string</param>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    /// <returns>Extracted token text</returns>
    private static string ExtractTokenText(string query, int start, int end)
    {
        if (start < 0 || end < 0 || start >= query.Length || end > query.Length || start >= end)
        {
            return string.Empty;
        }
        
        return query.Substring(start, end - start);
    }

    /// <summary>
    /// Convert protobuf data pointer to byte array
    /// </summary>
    /// <param name="protobuf">Native protobuf structure</param>
    /// <returns>Byte array containing protobuf data</returns>
    internal static byte[] ExtractProtobufData(NativeMethods.PgQueryProtobuf protobuf)
    {
        if (protobuf.data == IntPtr.Zero || protobuf.len == UIntPtr.Zero)
        {
            return Array.Empty<byte>();
        }

        var length = (int)protobuf.len;
        var data = new byte[length];
        Marshal.Copy(protobuf.data, data, 0, length);
        return data;
    }
}