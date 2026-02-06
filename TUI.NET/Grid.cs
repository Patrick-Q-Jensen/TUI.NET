namespace TUI.NET;

public class GridRow {
    //Public
    public int Height { get; set; } = -1;

    //Internal
    internal int RenderingHeight { get; set; }
}

public class GridColumn {
    //Public
    public int Width { get; set; } = -1;

    //Internal
    internal int RenderingWidth { get; set; }
}

public readonly struct Margin(int top, int right, int bottom, int left)
{
    public int Top { get; } = top;
    public int Right { get; } = right;
    public int Bottom { get; } = bottom;
    public int Left { get; } = left;

    public bool IsZero => Top == 0 && Right == 0 && Bottom == 0 && Left == 0;
}

public class Grid : IUIElement {
    public List<GridRow> Rows { get; set; } = new();
    public List<GridColumn> Columns { get; set; } = new();
    public List<IUIElement> Elements { get; set; } = new();

    //IUIElement implementation
    public Margin Margins { get; set; } = new Margin(0, 0, 0, 0);
    public int RowIndex { get; set; } = 0;
    public int ColumnIndex { get; set; } = 0;
    public uint RowSpan { get; set; } = 1;
    public uint ColumnSpan { get; set; } = 1;
    public uint BorderThickness { get; set; } = 1;
    public char HorizontalBorderChar { get; set; } = '-';
    public char VerticalBorderChar { get; set; } = '|';

    //Internal
    internal Cell[,] Cells { get; set; } = { };

    internal bool TryGetRow(int rowIndex, out GridRow? row)
    {
        row = null;
        if (Rows.Count < rowIndex + 1) return false;
        row = Rows[rowIndex];
        return true;
    } 

}



