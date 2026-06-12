using System.Runtime.InteropServices;
using System.Text;

namespace Npgquery.Native;

/// <summary>
/// Helper methods for preparing inputs and processing outputs from libpg_query native calls.
/// </summary>
internal static unsafe class NativeMethodHelpers
{
    /// <summary>
    /// Internal processed scan result for native operations
    /// </summary>
    internal struct NativeScanResult
    {
        public int? Version { get; set; }
        public SqlToken[]? Tokens { get; set; }
        public string? Error { get; set; }
        public string? Stderr { get; set; }
    }

    internal static string? PtrToString(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
#if NET472
        return PtrToStringUtf8Compat(ptr);
#else
        return Marshal.PtrToStringUTF8(ptr);
#endif
    }

#if NET472
    private static string? PtrToStringUtf8Compat(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        byte* bytes = (byte*)ptr;
        int len = 0;
        while (bytes[len] != 0) len++;
        return Encoding.UTF8.GetString(bytes, len);
    }
#endif

    internal static byte[] StringToUtf8Bytes(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        Array.Resize(ref bytes, bytes.Length + 1);
        return bytes;
    }

    internal static NativeMethods.PgQueryError? MarshalError(IntPtr errorPtr)
    {
        if (errorPtr == IntPtr.Zero)
            return null;

        return Marshal.PtrToStructure<NativeMethods.PgQueryError>(errorPtr);
    }

    internal static NativeMethods.PgQuerySplitStmt[] MarshalSplitStmts(NativeMethods.PgQuerySplitResult result)
    {
        if (result.n_stmts == 0 || result.stmts == IntPtr.Zero)
            return Array.Empty<NativeMethods.PgQuerySplitStmt>();

        var stmts = new NativeMethods.PgQuerySplitStmt[result.n_stmts];
        int ptrSize = Marshal.SizeOf<IntPtr>();
        for (int i = 0; i < result.n_stmts; i++)
        {
            IntPtr stmtPtr = Marshal.ReadIntPtr(result.stmts, i * ptrSize);
            stmts[i] = Marshal.PtrToStructure<NativeMethods.PgQuerySplitStmt>(stmtPtr);
        }

        return stmts;
    }

    internal static NativeScanResult ProcessScanResult(NativeMethods.PgQueryScanResult nativeResult, string originalQuery)
    {
        if (nativeResult.error != IntPtr.Zero)
        {
            string? errorMessage = null;
            var errorStruct = MarshalError(nativeResult.error);
            if (errorStruct?.message != IntPtr.Zero)
            {
                errorMessage = PtrToString(errorStruct.Value.message);
            }

            return new NativeScanResult
            {
                Error = errorMessage ?? "Scan error",
                Stderr = PtrToString(nativeResult.stderr_buffer)
            };
        }

        var stderr = PtrToString(nativeResult.stderr_buffer);

        if (nativeResult.pbuf.data != IntPtr.Zero && nativeResult.pbuf.len != UIntPtr.Zero)
        {
            try
            {
                var protobufData = ProtobufHelper.ExtractProtobufData(nativeResult.pbuf);
                var result = ProtobufHelper.DeserializeScanResult(protobufData, originalQuery);
                result.Stderr = stderr;
                return result;
            }
            catch (Exception ex)
            {
                return new NativeScanResult
                {
                    Error = $"Failed to process protobuf data: {ex.Message}",
                    Stderr = stderr
                };
            }
        }

        return new NativeScanResult
        {
            Error = "No protobuf data available",
            Stderr = stderr
        };
    }

    /// <summary>
    /// Allocates unmanaged memory for a PgQueryProtobuf from a byte array.
    /// </summary>
    internal static PgQueryProtobuf AllocPgQueryProtobuf(byte[] protoBytes)
    {
        var protoStruct = new PgQueryProtobuf
        {
            len = (UIntPtr)protoBytes.Length,
            data = Marshal.AllocHGlobal(protoBytes.Length)
        };
        Marshal.Copy(protoBytes, 0, protoStruct.data, protoBytes.Length);
        return protoStruct;
    }

    /// <summary>
    /// Frees unmanaged memory for a PgQueryProtobuf.
    /// </summary>
    internal static void FreePgQueryProtobuf(PgQueryProtobuf protoStruct)
    {
        if (protoStruct.data != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(protoStruct.data);
        }
    }
}
