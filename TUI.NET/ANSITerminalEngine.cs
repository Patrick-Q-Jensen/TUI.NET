//namespace TUI.NET
//{
//    internal sealed class ANSITerminalEngine : ITerminalEngine
//    {
//        public void EnterAlternateBuffer() => Console.Write("\x1b[?1049h");
//        public void ExitAlternateBuffer() => Console.Write("\x1b[?1049l");
//        public void Clear() => Console.Write("\x1b[2J\x1b[H");
//        public void SetCursorPosition(int x, int y) => Console.SetCursorPosition(x, y);
//        public void Write(string text) => Console.Write(text);

//        public void WriteRow(char[] row, int y)
//        {
//            SetCursorPosition(0, y);
//            Console.Write(new string(row));
//        }

//        public void Dispose() { /* nothing special */ }
//    }
//}
