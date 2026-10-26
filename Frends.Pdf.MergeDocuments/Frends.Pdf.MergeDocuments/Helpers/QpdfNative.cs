using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Frends.Pdf.MergeDocuments.Helpers;

/// <summary>
/// Minimal binding to the QPDF job C API (qpdfjob-c.h) provided by the QPdf.RuntimeLibraries package.
/// </summary>
internal static class QpdfNative
{
    private const string LibraryName = "qpdf29";

    // Exit codes of the qpdf command line: 0 = success, 2 = error, 3 = warnings (output still written).
    internal const int ExitSuccess = 0;
    internal const int ExitWarnings = 3;

    static QpdfNative()
    {
        NativeLibrary.SetDllImportResolver(typeof(QpdfNative).Assembly, Resolve);
    }

    /// <summary>
    /// Runs QPDF with the given command line arguments (without the program name).
    /// </summary>
    /// <param name="args">QPDF command line arguments.</param>
    /// <returns>QPDF exit code.</returns>
    internal static int Run(IReadOnlyList<string> args)
    {
        var argv = new IntPtr[args.Count + 2];
        try
        {
            argv[0] = ToUtf8("qpdf");
            for (var i = 0; i < args.Count; i++) argv[i + 1] = ToUtf8(args[i]);
            argv[^1] = IntPtr.Zero;
            return qpdfjob_run_from_argv(argv);
        }
        finally
        {
            foreach (var ptr in argv) if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
        }
    }

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int qpdfjob_run_from_argv(IntPtr[] argv);

    private static IntPtr ToUtf8(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Marshal.WriteByte(ptr, bytes.Length, 0);
        return ptr;
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName) return IntPtr.Zero;

        var rid = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win-x64" : "linux-x64";
        var file = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "qpdf29.dll" : "libqpdf29.so";
        var baseDir = Path.GetDirectoryName(assembly.Location) ?? AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "runtimes", rid, "native", file);

        return File.Exists(path) && NativeLibrary.TryLoad(path, out var handle) ? handle : IntPtr.Zero;
    }
}
