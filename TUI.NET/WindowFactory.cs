using System.Runtime.InteropServices;

namespace TUI.NET
{
    public static class WindowFactory
    {
        // Create best window implementation for the environment.
        public static IWindow Create(string title = "") {
            IWindow window = IsAnsiLikelySupported() 
                ? new ANSIWindow() 
                : new FallbackWindow();

            window.Title = title;
            return window;
        }

        private static bool IsAnsiLikelySupported()
        {
            // If output is redirected, avoid ANSI.
            if (Console.IsOutputRedirected) return false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Try to enable ANSI (VT) on Windows; return true only if succeeded.
                return TryEnableVirtualTerminalProcessing();
            }

            // On Unix-like systems assume TERM is set and not "dumb" (best-effort).
            string? term = Environment.GetEnvironmentVariable("TERM");
            if (string.IsNullOrEmpty(term)) return false;
            if (term.Equals("dumb", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        // Windows helper (guarded P/Invoke)
        private static bool TryEnableVirtualTerminalProcessing()
        {
            try
            {
                const int STD_OUTPUT_HANDLE = -11;
                const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

                IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (handle == IntPtr.Zero || handle == new IntPtr(-1)) return false;

                if (!GetConsoleMode(handle, out uint mode)) return false;

                if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) != ENABLE_VIRTUAL_TERMINAL_PROCESSING)
                {
                    uint newMode = mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                    return SetConsoleMode(handle, newMode);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }
}
