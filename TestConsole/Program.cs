using System.Text;
using TUI.NET;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args) {
            Grid rootGrid = new Grid();

            rootGrid.Rows.Add(new GridRow() { Height = 10 });
            rootGrid.Rows.Add(new GridRow());

            rootGrid.Columns.Add(new GridColumn() { Width = 10 });
            rootGrid.Columns.Add(new GridColumn());
            rootGrid.Columns.Add(new GridColumn());

            Grid grid = new Grid()
            {
                HorizontalBorderChar = '#',
                VerticalBorderChar = '║',
                Margins = new Margin(1, 1, 1, 1),
                ColumnSpan = 2,
                RowSpan = 2,
                RowIndex = 0,
                BorderThickness = 1
            };
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("My TextBlock");
            sb.AppendLine("My TextBlock");
            sb.AppendLine("My TextBlock");


            grid.Elements.Add(new TextBlock() { Text = sb.ToString() });
            rootGrid.Elements.Add(grid);

            //rootGrid.Children.Add(new Grid()
            //{
            //    HorizontalBorderChar = '@',
            //    VerticalBorderChar = '║',
            //    Margin = 1,
            //    RowIndex = 1,
            //    BorderThickness = 1
            //});


            IWindow view = WindowFactory.Create("My TUI");
            view.Show(rootGrid);
            Console.Read();
        }
    }
}
