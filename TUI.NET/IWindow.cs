namespace TUI.NET {
    public interface IWindow {
        void Show(Grid rootGrid);
        string Title { get; set; }

    }
}
