namespace TUI.NET;

internal static class PreRendering
{
    public static CanvasView PreRender(Grid rootGrid)
    {
        Console.Clear();
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        char[,] buffer = new char[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                buffer[x, y] = ' ';

        var rootView = new CanvasView(buffer, 0, 0, width, height);

        RenderElement(rootGrid, rootView);

        return rootView;
    }

    private static Cell[,] GenerateCells(List<GridRow> rows, List<GridColumn> columns, CanvasView view)
    {
        SetRowHeights(rows, view.Height);
        SetColumnWidths(columns, view.Width);

        Cell[,] cells = new Cell[rows.Count, columns.Count];

        int yOffset = 0;
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            int rowHeight = rows[rowIndex].RenderingHeight;
            int xOffset = 0;
            for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                int colWidth = columns[columnIndex].RenderingWidth;

                // Create a sub-view for this cell (local coordinates within rootView)
                var cellView = view.CreateSubView(xOffset, yOffset, colWidth, rowHeight);
                cells[rowIndex, columnIndex] = new Cell(cellView);

                xOffset += colWidth;
            }
            yOffset += rowHeight;
        }

        return cells;
    }

    private static void SetRowHeights(List<GridRow> rows, int availableHeight)
    {
        foreach (GridRow row in rows.Where(r=>r.Height >= 0)) {
            row.RenderingHeight = row.Height;
        }

        int totalFixedRowHeights = rows.Where(r => r.Height >= 0).Sum(r => r.Height);

        int autoRowCount = rows.Count(r => r.Height < 0);
        int remainingHeight = Math.Max(0, availableHeight - totalFixedRowHeights);
        int autoRowHeight = autoRowCount > 0 ? remainingHeight / autoRowCount : 0;

        foreach (GridRow row in rows.Where(r => r.Height < 0))
        {
            row.RenderingHeight = autoRowHeight;
        }
    }

    private static void SetColumnWidths(List<GridColumn> columns, int availableWidth)
    {
        foreach (GridColumn column in columns.Where(c => c.Width >= 0)) {
            column.RenderingWidth = column.Width;
        }

        int totalFixedColumnWidths = columns.Where(c => c.Width >= 0).Sum(c => c.Width);

        int autoColumnCount = columns.Count(c => c.Width < 0);
        int remainingWidth = Math.Max(0, availableWidth - totalFixedColumnWidths);
        int autoColumnWidth = autoColumnCount > 0 ? remainingWidth / autoColumnCount : 0;

        foreach (GridColumn column in columns.Where(r=>r.Width < 0))
        {
            column.RenderingWidth = autoColumnWidth;
        }
    }

    private static void RenderElementRecursive(List<IUIElement> elements, Cell[,] cells)
    {
        foreach (IUIElement element in elements)
        {
            Cell targetCell = cells[element.RowIndex, element.ColumnIndex];
            targetCell.View = MergeCellViewsAcrossSpan(cells, targetCell.View, element);
            RenderElement(element, targetCell.View);
        }
    }

    private static CanvasView MergeCellViewsAcrossSpan(Cell[,] cells, CanvasView view, IUIElement element)
    {
        int totalRows = cells.GetLength(0);
        int totalCols = cells.GetLength(1);

        int startRow = element.RowIndex;
        int startCol = element.ColumnIndex;
        int rowSpan = Math.Max(1, (int)element.RowSpan);
        int colSpan = Math.Max(1, (int)element.ColumnSpan);

        int endRow = Math.Min(startRow + rowSpan, totalRows);
        int endCol = Math.Min(startCol + colSpan, totalCols);

        for (int r = startRow; r < endRow; r++)
        {
            for (int c = startCol; c < endCol; c++)
            {
                // skip the starting cell (already in view)
                if (r == startRow && c == startCol) continue;
                view = view.MergeWith(cells[r, c].View);
            }
        }

        return view;
    }

    private static void RenderElement(IUIElement element, CanvasView view)
    {

        view = view.CreateInner(element.Margins);

        if (element.BorderThickness > 0)
        {
            RenderElementBorders(element, view);
            view = view.CreateInner((int)element.BorderThickness);
            if (view.Width <= 0 || view.Height <= 0)
                return;
        }

        RenderElementSpecific(element, view);
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

    private static void RenderElementSpecific(IUIElement element, CanvasView view)
    {
        switch (element)
        {
            case Grid grid:
                RenderGrid(grid, view);
                break;
            case TextBlock textBlock:
                RenderTextBlock(textBlock, view);
                break;
        }

    }

    private static void RenderTextBlock(TextBlock textBlock, CanvasView view)
    {
        string cleanText = textBlock.Text.Replace('\r', '\0');

        string[] lines = cleanText.Split('\n');
        for (int i = 0; i < lines.Length && i < view.Height; i++)
        {
            string line = lines[i];
            for (int j = 0; j < line.Length && j < view.Width; j++)
            {
                view[j, i] = line[j];
            }
        }
    }

    private static void RenderGrid(Grid grid, CanvasView view)
    {
        if (grid.Rows.Count == 0) grid.Rows.Add(new GridRow());
        if (grid.Columns.Count == 0) grid.Columns.Add(new GridColumn());

        grid.Cells = GenerateCells(grid.Rows, grid.Columns, view);

        RenderElementRecursive(grid.Elements, grid.Cells);

    }

}



