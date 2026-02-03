using TUI.NET;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args) {
            Grid grid = new Grid();

            grid.Rows.Add(new GridRow() { SizeMode= SizeMode.Fixed, Height = 10});
            grid.Rows.Add(new GridRow());

            grid.Children.Add(new Grid()
            {
                HorizontalBorderChar = '#',
                VerticalBorderChar = '║',
                Margin = 1,
                RowIndex = 0,
                BorderThickness = 1
            });

            grid.Children.Add(new Grid()
            {
                HorizontalBorderChar = '@',
                VerticalBorderChar = '║',
                Margin = 1,
                RowIndex = 1,
                BorderThickness = 1
            });


            IWindow view = WindowFactory.Create("My TUI");
            view.Show(grid);
            Console.Read();
        }
    }
}
