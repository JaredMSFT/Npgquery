using System.Runtime.InteropServices;
namespace Npgquery.Native;

/// <summary>
/// Native interop for libpg_query
/// </summary>
internal static class NativeMethods {
    private const string LibraryName = "pg_query";

    #region Native Structures

    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryError
    {
        public IntPtr message;      // char*
        public IntPtr funcname;     // char*
        public IntPtr filename;     // char*
        public int lineno;
        public int cursorpos;
        public IntPtr context;      // char*
    }

    /// <summary>
    /// Result structure from libpg_query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryParseResult {
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
        public IntPtr fingerprint_str;
        public IntPtr stderr_buffer;
        public IntPtr error;
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
        public IntPtr stmts;
        public int n_stmts;
        public IntPtr stderr_buffer;
        public IntPtr error;
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

    #endregion

    #region DLL Imports

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryParseResult pg_query_parse(byte[] input);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryNormalizeResult pg_query_normalize(byte[] input);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryFingerprintResult pg_query_fingerprint(byte[] input);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryDeparseResult pg_query_deparse_protobuf(PgQueryProtobuf parseTree);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQuerySplitResult pg_query_split_with_parser(byte[] input);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQuerySplitResult pg_query_split_with_scanner(byte[] input);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryScanResult pg_query_scan(byte[] input);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryPlpgsqlParseResult pg_query_parse_plpgsql(byte[] input);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PgQueryProtobufParseResult pg_query_parse_protobuf(byte[] input);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_parse_result(PgQueryParseResult result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_normalize_result(PgQueryNormalizeResult result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_fingerprint_result(PgQueryFingerprintResult result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_deparse_result(PgQueryDeparseResult result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_split_result(PgQuerySplitResult result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_scan_result(PgQueryScanResult result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_plpgsql_parse_result(PgQueryPlpgsqlParseResult result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void pg_query_free_protobuf_parse_result(PgQueryProtobufParseResult result);

    #endregion
}