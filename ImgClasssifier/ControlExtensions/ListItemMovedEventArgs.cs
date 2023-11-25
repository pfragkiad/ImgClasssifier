namespace ImgClasssifier.ControlExtensions;

public class ListItemMovedEventArgs(
        ListViewItem? current,
        ListViewItem? previous,
        ListViewItem? next) : EventArgs
{
    public ListViewItem? Item { get; } = current;
    public ListViewItem? Previous { get; } = previous;
    public ListViewItem? Next { get; } = next;
}
