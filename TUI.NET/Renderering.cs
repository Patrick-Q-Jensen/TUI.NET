
namespace TUI.NET
{
    public static class Renderering { 
    
        public static void Render(char[,] preRender) {

            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            // Build row buffer once
            char[] rowBuffer = new char[width];

            // Render each row at a fixed cursor position to avoid scrolling
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char ch = preRender[x, y];
                    rowBuffer[x] = ch == '\0' ? ' ' : ch;
                }

                // Set cursor position explicitly for each row (prevents accidental scrolling)
                try
                {
                    Console.SetCursorPosition(0, y);
                }
                catch
                {
                    // Some hosts or states may throw; ignore and continue writing.
                }

                Console.Write(new string(rowBuffer));
            }
        }
    }


}
