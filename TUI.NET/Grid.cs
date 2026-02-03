
namespace TUI.NET;

public interface IUIElement {
    uint Margin { get; set; }
    public int? RowIndex { get; set; }
    public uint ColumnIndex { get; set; }
    public uint RowSpan { get; set; }
    public uint ColumnSpan { get; set; }
    public uint BorderThickness { get; set; }
    public char HorizontalBorderChar { get; set; }
    public char VerticalBorderChar { get; set; }
}

public enum SizeMode {
    Auto,
    Fixed
}

public class GridRow {

    //Public
    public int Height { get; set; } = 0;
    public SizeMode SizeMode { get; set; } = SizeMode.Auto;

    ////Internal
    //internal uint UpperY { get; set; } = 0;
    //internal uint LowerY { get; set; } = 0;
}

public class GridColumn {

    //public
    public int Width { get; set; } = 0;
    public SizeMode SizeMode { get; set; } = SizeMode.Auto;

    //Internal
    internal uint LeftX { get; set; } = 0;
    internal uint RightX { get; set; } = 0;
}

public class Grid : IUIElement {
    //public string Title { get; set; } = "";
    public List<GridRow> Rows { get; set; } = new();
    public List<GridColumn> Columns { get; set; } = new();
    public List<IUIElement> Children { get; set; } = new();

    //IUIElement implementation
    public uint Margin { get; set; } = 0;
    public int? RowIndex { get; set; } = null;
    public uint ColumnIndex { get; set; }
    public uint RowSpan { get; set; }
    public uint ColumnSpan { get; set; }
    public uint BorderThickness { get; set; } = 1;
    public char HorizontalBorderChar { get; set; } = '-';
    public char VerticalBorderChar { get; set; } = '|';

}



