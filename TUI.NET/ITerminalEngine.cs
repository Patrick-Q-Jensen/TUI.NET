//namespace TUI.NET
//{
//    public interface ITerminalEngine : IDisposable
//    {
//        // lifecycle
//        void EnterAlternateBuffer();
//        void ExitAlternateBuffer();

//        // simple drawing primitives the Window will call from its main loop
//        void Clear();
//        void SetCursorPosition(int x, int y);
//        void Write(string text);
//        void WriteRow(char[] row, int y);

//        // convenience: create appropriate engine for the current environment
//        static ITerminalEngine Create() => TerminalEngineFactory.Create();
//    }
//}
