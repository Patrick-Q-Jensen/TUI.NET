namespace TUI.NET;


internal readonly struct CanvasView
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


    // Create an inner view by shrinking each side by margin
    public CanvasView CreateInner(GridRow row)
    {
        //if (margin <= 0) return this;
        //int newX = X + margin;
        //uint newY = row.UpperY;
        //int newWidth = Width - margin * 2;

        int newHeight = row.Height;
        if (newHeight > Height)
            newHeight = Height;
        if (newHeight <= 0)
            return new CanvasView(_buffer, 0, 0, 0, 0); // empty view
        return new CanvasView(_buffer, X, Y, Width, newHeight);
    }

    public CanvasView RemoveRow(GridRow row)
    {
        int newHeight = Height - row.Height;
        if (newHeight <= 0)
            return new CanvasView(_buffer, 0, 0, 0, 0); // empty view
        return new CanvasView(_buffer, X, Y + row.Height, Width, newHeight);
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
}

internal static class PreRendering
{

    public static char[,] PreRender(Grid rootGrid)
    {
        Console.Clear();
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        char[,] buffer = new char[width, height];


        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                buffer[x, y] = ' ';

        var rootView = new CanvasView(buffer, 0, 0, width, height);

        if (rootGrid.BorderThickness > 0) {
            //Draw root borders
            RenderElementBorders(rootGrid, rootView);
            rootView = rootView.CreateInner((int)rootGrid.BorderThickness);
        }

        SetRowHeights(rootGrid, rootView);
        ////Draw vertical borders
        //for (int y = 0; y < height; y++) {
        //    rootView[0, y] = rootGrid.VerticalBorderChar;
        //    rootView[width - 1, y] = rootGrid.VerticalBorderChar;
        //}

        ////Draw horizontal borders
        //for (int x = 0; x < width; x++) {
        //    rootView[x, 0] = rootGrid.HorizontalBorderChar;
        //    rootView[x, height - 1] = rootGrid.HorizontalBorderChar;
        //}

        //RenderGrid(rootGrid, rootView);

        RenderElementRecursive(rootGrid.Children, rootView, rootGrid);

        //foreach (IUIElement uiElement in rootGrid.Children)
        //{
        //    RenderElementRecursive(uiElement, rootView, rootGrid);
        //}

        return buffer;
    }

    private static void SetRowHeights(Grid grid, CanvasView view)
    {
        int totalFixedRowHeights = 0;
        foreach (GridRow row in grid.Rows.Where(x => x.SizeMode == SizeMode.Fixed))
        {
            totalFixedRowHeights += row.Height;
        }

        int autoRowCount = grid.Rows.Count(x => x.SizeMode == SizeMode.Auto);
        int remainingHeight = view.Height - totalFixedRowHeights;
        int autoRowHeight = autoRowCount > 0 ? remainingHeight / autoRowCount : 0;
        //uint upperY = (uint)view.Y;
        foreach (GridRow row in grid.Rows) {
            if (row.SizeMode == SizeMode.Auto) {
                row.Height = autoRowHeight;
            }
            //row.UpperY = upperY;
            //row.LowerY = upperY + (uint)row.Height - 1;
            //upperY += (uint)row.Height;
        }
    }

    private static void RenderElementRecursive(List<IUIElement> elements, CanvasView parentView, Grid parentGrid)
    {
        foreach (IUIElement element in elements)
        {
            CanvasView inner = parentView.CreateInner((int)element.Margin);

            if (element.RowIndex != null)
            {
                if (parentGrid.Rows.Count >= element.RowIndex + 1)
                {
                    GridRow row = parentGrid.Rows[element.RowIndex.Value];
                    inner = inner.CreateInner(row);
                    parentView = parentView.RemoveRow(row);
                }
            }

            //! If the size of the entire canvas is smaller than or equal to the margin we won't bother rendering the element
            if (element.Margin * 2 >= inner.Height || element.Margin * 2 >= inner.Width)
            {
                return;
            }

            if (inner.Width <= 0 || inner.Height <= 0)
                return;
            if (element.BorderThickness > 0)
            {
                RenderElementBorders(element, inner);
                inner = inner.CreateInner((int)element.BorderThickness);
                if (inner.Width <= 0 || inner.Height <= 0)
                    return;
            }
            RenderElementSpecific(element, inner, parentGrid);
        }
    }

    private static void RenderElementBorders(IUIElement element, CanvasView view)
    {
        int thickness = Math.Max(1, (int)element.BorderThickness);

        // Draw vertical borders (thickness columns on left and right)
        for (int y = 0; y < view.Height; y++)
        {
            for (int b = 0; b < thickness; b++)
            {
                // left side column b
                view[b, y] = element.VerticalBorderChar;

                // right side column (view.Width - 1 - b)
                int rightX = view.Width - 1 - b;
                if (rightX >= 0)
                    view[rightX, y] = element.VerticalBorderChar;
            }
        }

        // Draw horizontal borders (thickness rows on top and bottom)
        for (int b = 0; b < thickness; b++)
        {
            int topY = b;
            int bottomY = view.Height - 1 - b;

            if (topY >= 0 && topY < view.Height)
            {
                for (int x = 0; x < view.Width; x++)
                    view[x, topY] = element.HorizontalBorderChar;
            }

            if (bottomY >= 0 && bottomY < view.Height)
            {
                for (int x = 0; x < view.Width; x++)
                    view[x, bottomY] = element.HorizontalBorderChar;
            }
        }
    }


    private static void RenderElementSpecific(IUIElement element, CanvasView view, Grid parentGrid)
    {
        if (element is Grid grid)
        {
            RenderGrid(grid, view);
            // Render grid-specific content
        }

    }

    private static void RenderGrid(Grid grid, CanvasView view)
    {
        //Draw Title

    }

}



