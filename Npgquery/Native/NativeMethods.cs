using System.Runtime.InteropServices;
using System.Text;

namespace NpgqueryLib.Native;

/// <summary>
/// Native interop for libpg_query
/// </summary>
internal static unsafe class NativeMethods {
    private const string LibraryName = "pg_query";

    /// <summary>
    /// Result structure from libpg_query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryResult {
        public IntPtr tree;
        public IntPtr stderr_buffer;
        public IntPtr error;
    }

    /// <summary>
    /// Normalize result structure from libpg_query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryNormalizeResult {
        public IntPtr normalized_query;
        public IntPtr error;
    }

    /// <summary>
    /// Fingerprint result structure from libpg_query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryFingerprintResult {
        public ulong fingerprint;
        public IntPtr fingerprint_str;    // Don't read directly - use Marshal.PtrToStringAnsi
        public IntPtr stderr_buffer;
        public IntPtr error;              // Pointer to error struct
    }

    /// <summary>
    /// Deparse result structure from libpg_query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryDeparseResult {
        public IntPtr query;
        public IntPtr error;
    }

    /// <summary>
    /// Split statement structure from libpg_query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQuerySplitStmt {
        public int stmt_location;
        public int stmt_len;
    }

    /// <summary>
    /// Split result structure from libpg_query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQuerySplitResult {
        public IntPtr stmts; // Pointer to array of pointers to PgQuerySplitStmt
        public int n_stmts;
        public IntPtr stderr_buffer;
        public IntPtr error;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryProtobuf {
        public UIntPtr len;   // Use UIntPtr for size_t portability
        public IntPtr data;   // Pointer to the protobuf data buffer
    }

    /// <summary>
    /// Scan result structure from libpg_query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryScanResult {
        public PgQueryProtobuf pbuf;
        public IntPtr stderr_buffer;
        public IntPtr error;
    }

    /// <summary>
    /// PL/pgSQL parse result structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryPlpgsqlParseResult {
        public IntPtr tree;
        public IntPtr error;
    }

    /// <summary>
    /// Internal structure for processed scan results
    /// </summary>
    internal struct ProcessedScanResult {
        public int? Version { get; set; }
        public SqlToken[]? Tokens { get; set; }
        public string? Error { get; set; }
        public string? Stderr { get; set; }
    }

    /// <summary>
    /// Parse a PostgreSQL query
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryResult pg_query_parse(byte[] input);

    /// <summary>
    /// Normalize a PostgreSQL query
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryNormalizeResult pg_query_normalize(byte[] input);

    /// <summary>
    /// Get fingerprint of a PostgreSQL query
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryFingerprintResult pg_query_fingerprint(byte[] input);

    /// <summary>
    /// Deparse a PostgreSQL parse tree back to SQL
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryDeparseResult pg_query_deparse(byte[] input);

    /// <summary>
    /// Split multiple PostgreSQL statements
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQuerySplitResult pg_query_split_with_parser(byte[] input);

    /// <summary>
    /// Split multiple PostgreSQL statements using the scanner
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQuerySplitResult pg_query_split_with_scanner(byte[] input);

    /// <summary>
    /// Scan/tokenize a PostgreSQL query
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryScanResult pg_query_scan(byte[] input);

    /// <summary>
    /// Parse PL/pgSQL code
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryPlpgsqlParseResult pg_query_parse_plpgsql(byte[] input);

    /// <summary>
    /// Free parse result
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_parse_result(PgQueryResult result);

    /// <summary>
    /// Free normalize result
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_normalize_result(PgQueryNormalizeResult result);

    /// <summary>
    /// Free fingerprint result
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_fingerprint_result(PgQueryFingerprintResult result);

    /// <summary>
    /// Free deparse result
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_deparse_result(PgQueryDeparseResult result);

    /// <summary>
    /// Free split result
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_split_result(PgQuerySplitResult result);

    /// <summary>
    /// Free scan result
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_scan_result(PgQueryScanResult result);

    /// <summary>
    /// Free PL/pgSQL parse result
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_plpgsql_parse_result(PgQueryPlpgsqlParseResult result);

    /// <summary>
    /// Convert IntPtr to string safely
    /// </summary>
    internal static string? PtrToString(IntPtr ptr) {
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }

    /// <summary>
    /// Convert string to UTF-8 byte array for native calls
    /// </summary>
    internal static byte[] StringToUtf8Bytes(string input) {
        var bytes = Encoding.UTF8.GetBytes(input);
        Array.Resize(ref bytes, bytes.Length + 1); // Add null terminator
        return bytes;
    }

    /// <summary>
    /// Helper to marshal PgQuerySplitStmt*[] from PgQuerySplitResult
    /// </summary>
    internal static PgQuerySplitStmt[] MarshalSplitStmts(PgQuerySplitResult result) {
        if (result.n_stmts == 0 || result.stmts == IntPtr.Zero)
            return Array.Empty<PgQuerySplitStmt>();

        var stmts = new PgQuerySplitStmt[result.n_stmts];
        int ptrSize = Marshal.SizeOf<IntPtr>();
        for (int i = 0; i < result.n_stmts; i++) {
            IntPtr stmtPtr = Marshal.ReadIntPtr(result.stmts, i * ptrSize);
            stmts[i] = Marshal.PtrToStructure<PgQuerySplitStmt>(stmtPtr);
        }
        return stmts;
    }

    /// <summary>
    /// Helper to process scan results from protobuf data
    /// </summary>
    internal static ProcessedScanResult ProcessScanResult(PgQueryScanResult nativeResult, string originalQuery) {
        // Handle error case
        if (nativeResult.error != IntPtr.Zero) {
            return new ProcessedScanResult {
                Error = PtrToString(nativeResult.error),
                Stderr = PtrToString(nativeResult.stderr_buffer)
            };
        }

        // Handle stderr buffer
        var stderr = PtrToString(nativeResult.stderr_buffer);

        // Process protobuf data if available
        if (nativeResult.pbuf.data != IntPtr.Zero && nativeResult.pbuf.len != UIntPtr.Zero) {
            try {
                var protobufData = ProtobufHelper.ExtractProtobufData(nativeResult.pbuf);
                var result = ProtobufHelper.DeserializeScanResult(protobufData, originalQuery);
                result.Stderr = stderr;
                return result;
            }
            catch (Exception ex) {
                return new ProcessedScanResult {
                    Error = $"Failed to process protobuf data: {ex.Message}",
                    Stderr = stderr
                };
            }
        }
        else {
            return new ProcessedScanResult {
                Error = "No protobuf data available",
                Stderr = stderr
            };
        }
    }
}