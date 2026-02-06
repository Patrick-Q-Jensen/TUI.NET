namespace TUI.NET;

public class TextBlock : IUIElement 
{
    public string Text { get; set; } = "";

    //IUIElement implementation
    public Margin Margins { get; set; } = new Margin(0, 0, 0, 0);
    public int RowIndex { get; set; } = 0;
    public int ColumnIndex { get; set; } = 0;
    public uint RowSpan { get; set; } = 1;
    public uint ColumnSpan { get; set; } = 1;
    public uint BorderThickness { get; set; } = 1;
    public char HorizontalBorderChar { get; set; } = '-';
    public char VerticalBorderChar { get; set; } = '|';
}

