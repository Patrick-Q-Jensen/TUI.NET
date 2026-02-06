namespace TUI.NET
{
    // Polling resize watcher - cross-platform and simple.
    // Now debounces resize notifications: Resized is raised only after
    // the size has been stable for the debounce interval (default 200ms).
    public sealed class ConsoleResizeWatcher : IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly TimeSpan _pollInterval;
        private Task? _task;
        private int _lastWidth;
        private int _lastHeight;

        // Debounce fields
        private readonly TimeSpan _debounceDelay;
        private CancellationTokenSource? _debounceCts;
        private readonly object _sync = new();

        // Invoked when size changes and has been stable for the debounce interval: (newWidth, newHeight)
        public event Action<int, int>? Resized;

        public ConsoleResizeWatcher(TimeSpan? pollInterval = null, TimeSpan? debounceDelay = null)
        {
            _pollInterval = pollInterval ?? TimeSpan.FromMilliseconds(150);
            _debounceDelay = debounceDelay ?? TimeSpan.FromMilliseconds(200);
            _lastWidth = Console.WindowWidth;
            _lastHeight = Console.WindowHeight;
        }

        public void Start()
        {
            if (_task != null) return;
            _task = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        int w = Console.WindowWidth;
                        int h = Console.WindowHeight;
                        if (w != _lastWidth || h != _lastHeight)
                        {
                            // update tracked values and (re)start debounce
                            _lastWidth = w;
                            _lastHeight = h;
                            StartDebounce(w, h);
                        }
                    }
                    catch
                    {
                        // Console properties may throw on some hosts; ignore and retry.
                    }

                    try
                    {
                        await Task.Delay(_pollInterval, _cts.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) { break; }
                }
            }, _cts.Token);
        }

        private void StartDebounce(int width, int height)
        {
            lock (_sync)
            {
                // cancel previous pending debounce
                _debounceCts?.Cancel();
                _debounceCts?.Dispose();

                // create new debounce token
                _debounceCts = new CancellationTokenSource();
                var token = _debounceCts.Token;

                // schedule invocation after debounce delay
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(_debounceDelay, token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }

                    // invoke on thread-pool thread; subscriber should marshal to main thread if needed
                    try
                    {
                        Resized?.Invoke(width, height);
                    }
                    catch
                    {
                        // swallow subscriber exceptions to keep watcher alive
                    }
                }, token);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            lock (_sync)
            {
                _debounceCts?.Cancel();
                _debounceCts?.Dispose();
                _debounceCts = null;
            }

            try { _task?.Wait(500); } catch { }
            _cts.Dispose();
        }
    }
}
