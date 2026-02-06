namespace TUI.NET {

    // Full-screen window using ANSI (alternate buffer + ANSI clears).
    internal sealed class ANSIWindow : IWindow
    {
        //private readonly string _title;
        private volatile bool _running;
        public string Title { get; set; } = "";
        private CanvasView? currentView = null;


        public void Show(Grid rootGrid)
        {
            Console.Title = Title;
            Console.CursorVisible = false;
            Console.TreatControlCAsInput = true;

            // Enter alternate buffer
            bool enteredAlt = false;
            try
            {
                Console.Write("\x1b[?1049h"); // enter alt buffer (1049)
                enteredAlt = true;
            }
            catch
            {
                enteredAlt = false;
            }

            // Ensure initial clean screen
            SafeWriteAnsiClear();
            RenderFrame(rootGrid);

            using var watcher = new ConsoleResizeWatcher();
            bool needsRender = true;
            bool stabilized = false;
            watcher.Resized += (w, h) => { needsRender = true; stabilized = false; };
            watcher.Start();

            CancellationTokenSource? pending = null;
            const int debounceMs = 200;
            _running = true;

            try
            {
                  while (_running)
                 {
                    // if (needsRender)
                    //{
                    //    RenderFrame(rootGrid);
                    //    needsRender = false;
                    //}
                    if (!stabilized && needsRender)
                    {
                        pending?.Cancel();
                        pending = new CancellationTokenSource();
                        var token = pending.Token;
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(debounceMs, token).ConfigureAwait(false);
                            }
                            catch (TaskCanceledException) { return; }
                            stabilized = true;
                            needsRender = true;
                        }, token);
                    }

                    if (stabilized && needsRender)
                    {
                        RenderFrame(rootGrid);
                        needsRender = false;
                        stabilized = false;
                    }
                    else if (needsRender && !stabilized)
                    {
                        // fast render
                        RenderFrame(rootGrid);
                        needsRender = false;
                    }

                    if (Console.KeyAvailable)
                    {
                        var k = Console.ReadKey(intercept: true);
                        HandleKey(k);
                        needsRender = true;
                    }
                    else
                    {
                        Thread.Sleep(80);
                    }
                }
            }
            finally
            {
                pending?.Cancel();
                try
                {
                    if (enteredAlt)
                        Console.Write("\x1b[?1049l"); // exit alt buffer
                }
                catch { /* ignore */ }

                Console.CursorVisible = true;
                Console.TreatControlCAsInput = false;
            }
        }

        private void RenderFrame(Grid rootGrid) {
            var pre = PreRendering.PreRender(rootGrid);
            SafeWriteAnsiClear();
            Renderering.Render(pre);
            currentView = pre;
        }

        private static void SafeWriteAnsiClear()
        {
            try { Console.Write("\x1b[2J\x1b[H"); } catch { try { Console.Clear(); Console.SetCursorPosition(0, 0); } catch { } }
        }

        private void HandleKey(ConsoleKeyInfo key) {
            switch (key.Key) {
                case ConsoleKey.Escape:
                case ConsoleKey.C when key.Modifiers.HasFlag(ConsoleModifiers.Control):
                    _running = false;
                    break;
                // TODO: navigation/actions
                default: break;
            }
        }
    }
}
