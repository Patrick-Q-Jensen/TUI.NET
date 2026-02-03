//namespace TUI.NET
//{
//    // Fallback engine that uses Console.Clear and SetCursorPosition
//    internal sealed class FallbackTerminalEngine : ITerminalEngine
//    {
//        public void EnterAlternateBuffer() { /* no-op */ }
//        public void ExitAlternateBuffer() { /* no-op */ }

//        public void Clear()
//        {
//            Console.Clear();
//            try { Console.SetCursorPosition(0, 0); } catch { }
//        }

//        public void SetCursorPosition(int x, int y)
//        {
//            try { Console.SetCursorPosition(x, y); } catch { }
//        }

//        public void Write(string text) => Console.Write(text);

//        public void WriteRow(char[] row, int y)
//        {
//            try
//            {
//                Console.SetCursorPosition(0, y);
//                Console.Write(new string(row));
//            }
//            catch { /* ignore */ }
//        }

//        public void Dispose() { /* nothing special */ }
//    }
//}
