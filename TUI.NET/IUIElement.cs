namespace TUI.NET;

public interface IUIElement {
    Margin Margins { get; set; }
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public uint RowSpan { get; set; }
    public uint ColumnSpan { get; set; }
    public uint BorderThickness { get; set; }
    public char HorizontalBorderChar { get; set; }
    public char VerticalBorderChar { get; set; }
}



