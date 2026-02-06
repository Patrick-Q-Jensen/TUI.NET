using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace TUI.NET;

[DebuggerTypeProxy(typeof(DebugView))]
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal readonly struct CanvasView : IEquatable<CanvasView>
{
    private readonly char[,] _buffer;
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public CanvasView(char[,] buffer, int x, int y, int width, int height)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        X = Math.Max(0, x);
        Y = Math.Max(0, y);
        Width = Math.Max(0, Math.Min(width, buffer.GetLength(0) - X));
        Height = Math.Max(0, Math.Min(height, buffer.GetLength(1) - Y));
    }

    // Create an inner view by shrinking each side by margin
    public CanvasView CreateInner(int margin)
    {
        if (margin <= 0) return this;
        int newX = X + margin;
        int newY = Y + margin;
        int newWidth = Width - margin * 2;
        int newHeight = Height - margin * 2;
        if (newWidth <= 0 || newHeight <= 0)
            return new CanvasView(_buffer, 0, 0, 0, 0); // empty view
        return new CanvasView(_buffer, newX, newY, newWidth, newHeight);
    }

    public CanvasView CreateInner(Margin margin)
    {
        if (margin.IsZero) return this;

        // Safely coerce to ints (works for int/uint/short/etc. on Margin properties)
        int left = Math.Max(0, margin.Left);
        int top = Math.Max(0, margin.Top);
        int right = Math.Max(0, margin.Right);
        int bottom = Math.Max(0, margin.Bottom);

        int newX = X + left;
        int newY = Y + top;
        int newWidth = Width - left - right;
        int newHeight = Height - top - bottom;

        if (newWidth <= 0 || newHeight <= 0)
            return new CanvasView(_buffer, 0, 0, 0, 0); // empty view

        return new CanvasView(_buffer, newX, newY, newWidth, newHeight);
    }

    //Create an arbitrary sub-view within this view (local coordinates)
    public CanvasView CreateSubView(int localX, int localY, int subWidth, int subHeight)
    {
        if (subWidth <= 0 || subHeight <= 0)
            return new CanvasView(_buffer, 0, 0, 0, 0); // empty

        int newX = X + Math.Max(0, localX);
        int newY = Y + Math.Max(0, localY);

        // clamp to available area
        int clampedWidth = Math.Min(subWidth, Math.Max(0, Width - Math.Max(0, localX)));
        int clampedHeight = Math.Min(subHeight, Math.Max(0, Height - Math.Max(0, localY)));

        if (clampedWidth <= 0 || clampedHeight <= 0)
            return new CanvasView(_buffer, 0, 0, 0, 0);

        return new CanvasView(_buffer, newX, newY, clampedWidth, clampedHeight);
    }

    // Provide a diagnostic string containing the view contents (rows separated by newline).
    public override string ToString()
    {
        if (_buffer == null || Width <= 0 || Height <= 0)
            return string.Empty;

        // Reserve approximate capacity (width * height + newlines)
        var sb = new StringBuilder(Width * Math.Max(1, Height) + Math.Max(0, Height - 1));
        for (int ly = 0; ly < Height; ly++)
        {
            int by = Y + ly;
            for (int lx = 0; lx < Width; lx++)
            {
                int bx = X + lx;
                sb.Append(_buffer[bx, by]);
            }
            if (ly < Height - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    // Useful for dumping the entire screen buffer regardless of this view's slice.
    public string RootToString()
    {
        if (_buffer == null)
            return string.Empty;

        int totalWidth = _buffer.GetLength(0);
        int totalHeight = _buffer.GetLength(1);

        if (totalWidth == 0 || totalHeight == 0)
            return string.Empty;

        var sb = new StringBuilder(totalWidth * Math.Max(1, totalHeight) + Math.Max(0, totalHeight - 1));
        for (int by = 0; by < totalHeight; by++)
        {
            for (int bx = 0; bx < totalWidth; bx++)
            {
                sb.Append(_buffer[bx, by]);
            }
            if (by < totalHeight - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GetDebuggerDisplay()
    {
        return $"CanvasView {Width}x{Height} @({X},{Y})";
    }

    // Debugger proxy exposes a Contents property that Visual Studio will show when you expand the view.
    private sealed class DebugView
    {
        private readonly CanvasView _view;
        public DebugView(CanvasView view) => _view = view;

        // Textual contents for inspection
        public string Contents => _view.ToString();

        // Full textual contents for inspectiom
        public string RootContent => _view.RootToString();

        // Short summary also available
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Summary => _view.GetDebuggerDisplay();
    }


    // The method does not modify either view and requires both views to share the same buffer.
    public CanvasView MergeWith(CanvasView other)
    {
        if (_buffer is null || other._buffer is null)
            throw new ArgumentException("One of the CanvasView buffers is null.");

        if (!ReferenceEquals(_buffer, other._buffer))
            throw new ArgumentException("Cannot merge CanvasViews that use different buffers.", nameof(other));

        int left = Math.Min(X, other.X);
        int top = Math.Min(Y, other.Y);
        int right = Math.Max(X + Width, other.X + other.Width);
        int bottom = Math.Max(Y + Height, other.Y + other.Height);

        int newWidth = right - left;
        int newHeight = bottom - top;

        if (newWidth <= 0 || newHeight <= 0)
            return new CanvasView(_buffer, 0, 0, 0, 0);

        return new CanvasView(_buffer, left, top, newWidth, newHeight);
    }

    public bool Equals(CanvasView other)
    {
        return AreBuffersEqual(_buffer, other._buffer);
    }

    // Indexer: local coordinates
    public char this[int lx, int ly]
    {
        get
        {
            if (lx < 0 || lx >= Width || ly < 0 || ly >= Height) return '\0';
            return _buffer[lx + X, ly + Y];
        }
        set
        {
            if (lx < 0 || lx >= Width || ly < 0 || ly >= Height) return;
            _buffer[lx + X, ly + Y] = value;
        }
    }

    private static bool AreBuffersEqual(char[,] a, char[,] b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;

        int aW = a.GetLength(0), aH = a.GetLength(1);
        int bW = b.GetLength(0), bH = b.GetLength(1);
        if (aW != bW || aH != bH) return false;

        int len = aW * aH;
        if (len == 0) return true;

        // Create spans over the contiguous storage of the multi-dim arrays
        ref char ra = ref a[0, 0];
        ref char rb = ref b[0, 0];

        var sa = MemoryMarshal.CreateSpan(ref ra, len);
        var sb = MemoryMarshal.CreateSpan(ref rb, len);

        return sa.SequenceEqual(sb);
    }
}



