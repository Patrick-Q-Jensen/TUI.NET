//using System.Runtime.InteropServices;

//namespace TUI.NET {

//    public class Window(string title = "") {
//        public string Title = title;
//        private bool running = true;
//        private ITerminalEngine terminalEngine = ITerminalEngine.Create();

//        // Show uses the terminal alternate buffer to avoid scrollback of intermediate frames.
//        // If alternate buffer or VT processing cannot be enabled, code falls back safely.
//        public void Show1(Grid rootGrid)
//        {
//            Console.Title = Title;
//            Console.CursorVisible = false;
//            Console.TreatControlCAsInput = true;

//            bool vtEnabled = TryEnableVirtualTerminalProcessing();
//            bool enteredAlternate = false;

//            try
//            {
//                if (vtEnabled)
//                {
//                    try
//                    {
//                        // Enter alternate screen buffer - cross-platform xterm/VT sequence
//                        Console.Write("\x1b[?1049h");
//                        enteredAlternate = true;
//                    }
//                    catch
//                    {
//                        enteredAlternate = false;
//                    }
//                }

//                // try to set buffer to avoid initial scrollback (best-effort)
//                try { Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight); } catch { }

//                using var resizeWatcher = new ConsoleResizeWatcher();
//                bool needsRender = true;
//                resizeWatcher.Resized += (w, h) =>
//                {
//                    // mark dirty, actual handling is debounced below
//                    needsRender = true;
//                };
//                resizeWatcher.Start();

//                // Debounce helper: keep only the latest pending stabilization task
//                CancellationTokenSource? pendingStabilizeCts = null;
//                const int debounceMs = 200;

//                running = true;
//                while (running)
//                {
//                    if (needsRender)
//                    {
//                        // schedule stabilization: wait for size to stop changing for debounceMs before expensive operations
//                        pendingStabilizeCts?.Cancel();
//                        pendingStabilizeCts = new CancellationTokenSource();

//                        var token = pendingStabilizeCts.Token;
//                        _ = Task.Run(async () =>
//                        {
//                            try
//                            {
//                                await Task.Delay(debounceMs, token).ConfigureAwait(false);
//                            }
//                            catch (TaskCanceledException)
//                            {
//                                return;
//                            }

//                            // At this point size is stable — try a clean reset.
//                            try
//                            {
//                                int w = Console.WindowWidth;
//                                int h = Console.WindowHeight;

//                                // We avoid relying on SetBufferSize for correctness on Linux, but attempt it as best-effort.
//                                try { Console.SetBufferSize(w, h); } catch { /* host may disallow */ }

//                                // Clear screen (ANSI clear also works when VT enabled)
//                                try
//                                {
//                                    if (vtEnabled)
//                                    {
//                                        // Clear entire screen and move cursor home
//                                        Console.Write("\x1b[2J\x1b[H");
//                                    }
//                                    else
//                                    {
//                                        Console.Clear();
//                                        Console.SetCursorPosition(0, 0);
//                                    }
//                                }
//                                catch { /* ignore */ }
//                            }
//                            catch
//                            {
//                                // ignore unexpected exceptions accessing Console properties
//                            }

//                            needsRender = true; // re-render the single final frame
//                        }, token);

//                        // prevent double immediate renders; stabilization task will set needsRender true when ready
//                        needsRender = false;
//                    }

//                    // If rendering was requested after stabilization, render now
//                    if (!Console.KeyAvailable && !needsRender)
//                    {
//                        // idle; sleep briefly
//                        Thread.Sleep(20);
//                    }

//                    if (needsRender)
//                    {
//                        var preRender = PreRendering.PreRender(rootGrid);
//                        Renderering.Render(preRender);
//                        needsRender = false;
//                    }

//                    // handle input without echoing
//                    if (Console.KeyAvailable)
//                    {
//                        var keyInfo = Console.ReadKey(intercept: true);
//                        HandleKeyInput(keyInfo);
//                    }
//                }

//                pendingStabilizeCts?.Cancel();
//            }
//            finally
//            {
//                // Exit alternate buffer if we entered it
//                if (enteredAlternate)
//                {
//                    try
//                    {
//                        Console.Write("\x1b[?1049l");
//                    }
//                    catch { /* ignore */ }
//                }

//                Console.CursorVisible = true;
//                Console.TreatControlCAsInput = false;
//            }
//        }

//        public void Show(Grid rootGrid)
//        {
//            Console.Title = Title;
//            Console.CursorVisible = false;
//            Console.TreatControlCAsInput = true;

//            // Enter alternate buffer and clear using the engine (engine may be a no-op fallback).
//            try {
//                terminalEngine.EnterAlternateBuffer();
//            }
//            catch
//            {
//                // Best-effort; if the engine throws, continue with fallback behavior.
//            }

//            // Ensure initial clear and cursor home so first render is visible across platforms.
//            try
//            {
//                terminalEngine.Clear();
//            }
//            catch
//            {
//                // ignore and continue
//            }

//            using var resizeWatcher = new ConsoleResizeWatcher();
//            bool needsRender = true;
//            bool stabilized = false;
//            resizeWatcher.Resized += (w, h) =>
//            {
//                // Mark dirty and reset stabilization — actual clearing will happen on main thread
//                needsRender = true;
//                stabilized = false;
//            };
//            resizeWatcher.Start();

//            // Debounce helper: keep only the latest pending stabilization task
//            CancellationTokenSource? pendingStabilizeCts = null;
//            const int debounceMs = 200;

//            running = true;
//            while (running)
//            {
//                // If a resize has been detected, schedule stabilization.
//                if (!stabilized && needsRender)
//                {
//                    pendingStabilizeCts?.Cancel();
//                    pendingStabilizeCts = new CancellationTokenSource();
//                    var token = pendingStabilizeCts.Token;

//                    _ = Task.Run(async () =>
//                    {
//                        try
//                        {
//                            await Task.Delay(debounceMs, token).ConfigureAwait(false);
//                        }
//                        catch (TaskCanceledException)
//                        {
//                            return;
//                        }

//                        // mark stabilized; main loop will perform the engine.Clear() and final render
//                        stabilized = true;
//                        needsRender = true;
//                    }, token);
//                }

//                // perform stabilization actions and final clean render on main thread
//                if (stabilized && needsRender)
//                {
//                    // clear using engine (main thread) so we don't race console calls
//                    try
//                    {
//                        terminalEngine.Clear();
//                        terminalEngine.SetCursorPosition(0, 0);
//                    }
//                    catch
//                    {
//                        // ignore errors from engine on clear/position
//                    }

//                    // re-render final frame
//                    var preRender = PreRendering.PreRender(rootGrid);
//                    Renderering.Render(preRender);
//                    needsRender = false;
//                    stabilized = false; // reset until next resize
//                }
//                else if (needsRender && !stabilized)
//                {
//                    // initial or non-stabilized render (fast path) - render in place
//                    var preRender = PreRendering.PreRender(rootGrid);
//                    Renderering.Render(preRender);
//                    needsRender = false;
//                }

//                // handle input without echoing
//                if (Console.KeyAvailable)
//                {
//                    var keyInfo = Console.ReadKey(intercept: true);
//                    HandleKeyInput(keyInfo);

//                    // If input handling mutated UI state, request a render
//                    needsRender = true;
//                }
//                else
//                {
//                    // idle; sleep briefly
//                    Thread.Sleep(20);
//                }
//            }

//            pendingStabilizeCts?.Cancel();

//            // Exit alternate buffer and dispose engine
//            try
//            {
//                terminalEngine.ExitAlternateBuffer();
//            }
//            catch
//            {
//                // ignore
//            }
//            terminalEngine.Dispose();

//            Console.CursorVisible = true;
//            Console.TreatControlCAsInput = false;
//        }

//        private void HandleKeyInput(ConsoleKeyInfo keyInfo)
//        {
//            // Handle navigation keys and control actions. All other keys are ignored (swallowed).
//            switch (keyInfo.Key)
//            {
//                case ConsoleKey.UpArrow:
//                    // TODO: move selection up
//                    break;
//                case ConsoleKey.DownArrow:
//                    // TODO: move selection down
//                    break;
//                case ConsoleKey.LeftArrow:
//                    // TODO: move selection left
//                    break;
//                case ConsoleKey.RightArrow:
//                    // TODO: move selection right
//                    break;
//                case ConsoleKey.Enter:
//                    // TODO: activate current selection
//                    break;
//                case ConsoleKey.Escape:
//                    running = false;
//                    break;
//                case ConsoleKey.C when keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control):
//                    // Example: Ctrl+C captured because TreatControlCAsInput = true
//                    // Handle or ignore; here we break loop
//                    running = false;
//                    break;
//                default:
//                    // swallow all printable characters so nothing appears in the console
//                    break;
//            }
//        }

//        // Attempt to enable VT processing on Windows so ANSI sequences are honored.
//        private static bool TryEnableVirtualTerminalProcessing()
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
//                        // Try to set the flag
//                        uint newMode = mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING;
//                        return SetConsoleMode(handle, newMode);
//                    }

//                    return true;
//                }
//                else
//                {
//                    // On Unix-like systems ANSI is typically supported by default
//                    return true;
//                }
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        // P/Invoke for Windows console mode toggling
//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern IntPtr GetStdHandle(int nStdHandle);

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
//    }
//}
