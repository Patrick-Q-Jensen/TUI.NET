//using System.Runtime.InteropServices;

//namespace TUI.NET
//{
//    internal static class TerminalEngineFactory
//    {
//        public static ITerminalEngine Create()
//        {
//            // Try VT first — it's the best experience on modern terminals.
//            bool vt = TryANSITerminalProcessing();
//            if (vt)
//                return new ANSITerminalEngine();

//            return new FallbackTerminalEngine();
//        }

//        private static bool TryANSITerminalProcessing()
//        {
//            try
//            {
//                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//                {
//                    const int STD_OUTPUT_HANDLE = -11;
//                    const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

//                    IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
//                    if (handle == IntPtr.Zero || handle == new IntPtr(-1)) return false;

//                    if (!GetConsoleMode(handle, out uint mode)) return false;

//                    if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) != ENABLE_VIRTUAL_TERMINAL_PROCESSING)
//                    {
//                        uint newMode = mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING;
//                        return SetConsoleMode(handle, newMode);
//                    }

//                    return true;
//                }

//                // Unix-like terminals typically support ANSI by default
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern IntPtr GetStdHandle(int nStdHandle);

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
//    }
//}
