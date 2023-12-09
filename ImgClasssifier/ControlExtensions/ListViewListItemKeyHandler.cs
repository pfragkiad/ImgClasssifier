namespace ImgClasssifier.ControlExtensions;

public class ListViewListItemKeyHandler
{
    private readonly ListView _listView;

    private ListItemGraph _graph;
    public ListItemGraph Graph { get => _graph; }

    public ListViewListItemKeyHandler(ListView listView)
    {
        _listView = listView;
        _graph = new ListItemGraph(listView);

        //_listView.MouseDown += ListView_MouseDown;
        //_listView.MouseMove += ListView_MouseMove;
        //_listView.MouseUp += ListView_MouseUp;
        _listView.KeyDown += ListView_KeyDown;

        _listView.FindForm()!.Load += ListViewListItemKeyHandler_Load;
    }

    private void ListViewListItemKeyHandler_Load(object? sender, EventArgs e)
    {
        RefreshGraph();
        _listView.Resize += ListView_Resize;
    }

    private void ListView_Resize(object? sender, EventArgs e)
    {
        RefreshGraph();
    }

    public void RefreshGraph() => _graph?.Update();

    private void ListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (_listView.SelectedItems.Count == 0) return;

        HashSet<Keys> handledKeys = new HashSet<Keys> { Keys.Right, Keys.Left, Keys.Up, Keys.Down };
        if (!handledKeys.Contains(e.KeyCode)) return;

        if (_graph.Items is null) _graph.Update();

        ListViewItem selected =
            e.KeyCode == Keys.Right || e.KeyCode == Keys.Down ?
            _graph.GetLastSelectedItem()! : _graph.GetFirstSelectedItem()!;
        int i = _graph.Items!.IndexOf(selected);

        if (e.KeyCode == Keys.Right && i + 1 < _graph.Items.Count)
        {
            _listView.SelectedItems.Clear();
            _graph.Items[i + 1].Selected = true;
            _graph.Items[i + 1].EnsureVisible();
        }
        else if (e.KeyCode == Keys.Down && i + _graph.ItemsPerRow < _graph.Items.Count)
        {
            _listView.SelectedItems.Clear();
            _graph.Items[i + _graph.ItemsPerRow].Selected = true;
            _graph.Items[i + _graph.ItemsPerRow].EnsureVisible();
        }

        else if (e.KeyCode == Keys.Left && i - 1 >= 0)
        {
            _listView.SelectedItems.Clear();
            _graph.Items[i - 1].Selected = true;
            _graph.Items[i - 1].EnsureVisible();
        }

        else if (e.KeyCode == Keys.Up && i - _graph.ItemsPerRow >= 0)
        {
            _listView.SelectedItems.Clear();
            _graph.Items[i - _graph.ItemsPerRow].Selected = true;
            _graph.Items[i - _graph.ItemsPerRow].EnsureVisible();
        }

        e.Handled = true;

    }

    ListViewItem? _draggedItem;
    Point? _draggedCurrentPoint;

    private void ListView_MouseDown(object? sender, MouseEventArgs e)
    {
        _listView.AutoArrange = false;
        _draggedItem = _listView.GetItemAt(e.X, e.Y);
        if (_draggedItem is null) return;

        _draggedCurrentPoint = new Point(e.X - _draggedItem.Position.X, e.Y - _draggedItem.Position.Y);

    }
    private void ListView_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_draggedItem is null) return;
        _draggedItem.Position = new Point(e.Location.X - _draggedCurrentPoint!.Value.X, e.Location.Y - _draggedCurrentPoint!.Value.Y);

    }
    public event EventHandler<ListItemMovedEventArgs>? ItemMoved;

    private void ListView_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_draggedItem is null) return;

        _draggedCurrentPoint = new Point(e.X - _draggedItem.Position.X, e.Y - _draggedItem.Position.Y);
        //_draggedItem.Position = new Point(e.Location.X, e.Location.Y);

        _listView.AutoArrange = true;

        _graph.Update();

        int i = _graph.Items!.IndexOf(_draggedItem);
        ListViewItem? previous = i > 0 ? _graph.Items[i - 1] : null;
        ListViewItem? next = i < _graph.Items.Count - 1 ? _graph.Items[i + 1] : null;

        ItemMoved?.Invoke(this, new ListItemMovedEventArgs(_draggedItem, previous, next));
        _draggedItem = null;
    }
}
