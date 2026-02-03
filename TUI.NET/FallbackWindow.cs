namespace TUI.NET {
    // Fallback window for environments without ANSI/alternate buffer.
    internal sealed class FallbackWindow : IWindow
    {
        //public string Title;
        private volatile bool _running;
        private int? _originalBufferWidth;
        private int? _originalBufferHeight;

        public string Title { get; set; } = "";

        public void Show(Grid rootGrid)
        {
            Console.Title = Title;
            Console.CursorVisible = false;
            Console.TreatControlCAsInput = true;

            // Best effort: shrink buffer to window to reduce scrollback growth (Windows-only useful).
            TryShrinkBufferToWindow();

            // initial clear
            try { Console.Clear(); Console.SetCursorPosition(0, 0); } catch { }

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
                    if (!stabilized && needsRender)
                    {
                        pending?.Cancel();
                        pending = new CancellationTokenSource();
                        var token = pending.Token;
                        _ = Task.Run(async () =>
                        {
                            try { await Task.Delay(debounceMs, token).ConfigureAwait(false); }
                            catch (TaskCanceledException) { return; }
                            stabilized = true;
                            needsRender = true;
                        }, token);
                    }

                    if (stabilized && needsRender)
                    {
                        // clear using Console API and render final frame
                        try { Console.Clear(); Console.SetCursorPosition(0, 0); } catch { }
                        RenderFrame(rootGrid);
                        needsRender = false;
                        stabilized = false;
                    }
                    else if (needsRender && !stabilized)
                    {
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
                        Thread.Sleep(20);
                    }
                }
            }
            finally
            {
                pending?.Cancel();
                RestoreBufferIfNeeded();
                Console.CursorVisible = true;
                Console.TreatControlCAsInput = false;
            }
        }

        private void RenderFrame(Grid rootGrid) {
            var pre = PreRendering.PreRender(rootGrid);
            Renderering.Render(pre);
        }

        private void TryShrinkBufferToWindow()
        {
            if (Console.IsOutputRedirected) return;
            try
            {
                _originalBufferWidth = Console.BufferWidth;
                _originalBufferHeight = Console.BufferHeight;
                Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            }
            catch
            {
                _originalBufferWidth = null;
                _originalBufferHeight = null;
            }
        }

        private void RestoreBufferIfNeeded()
        {
            if (_originalBufferWidth.HasValue && !Console.IsOutputRedirected)
            {
                try { Console.SetBufferSize(_originalBufferWidth.Value, _originalBufferHeight.Value); }
                catch { }
            }
        }

        private void HandleKey(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                case ConsoleKey.C when key.Modifiers.HasFlag(ConsoleModifiers.Control):
                    _running = false;
                    break;
                default: break;
            }
        }
    }
}
