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
        if(!_resizeHandlerEnabled) return;

        RefreshGraph();
    }

    bool _resizeHandlerEnabled = false;
    public void DisableResizeHandler()
    {
        _resizeHandlerEnabled = false;
    }
    public void EnableResizeHandler()
    {
        _resizeHandlerEnabled = true;
    }


    public void RefreshGraph() => _graph?.Update();

    private void ListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (_listView.SelectedItems.Count == 0) return;

        HashSet<Keys> handledKeys = [Keys.Right, Keys.Left, Keys.Up, Keys.Down];
        if (!handledKeys.Contains(e.KeyCode)) return;

        //if (_graph.Items is null) _graph.Update();

        ListViewItem selected =
            e.KeyCode == Keys.Right || e.KeyCode == Keys.Down ?
            _graph.GetLastSelectedItem()! : _graph.GetFirstSelectedItem()!;

        int i = _graph.Items!.IndexOf(selected);

        int nextI =  e.KeyCode switch
        {
            Keys.Right when i + 1 < _graph.Items.Count => i+1,
            Keys.Down when i + _graph.ItemsPerRow < _graph.Items.Count => i + _graph.ItemsPerRow,
            Keys.Left when i - 1 >= 0 => i - 1,
            Keys.Up when i - _graph.ItemsPerRow >= 0 => i - _graph.ItemsPerRow,
            _ => -1
        };

        if (nextI>=0)
        {
            _listView.SelectedItems.Clear();
            _graph.Items[nextI].Selected = true;
            _graph.Items[nextI].EnsureVisible();
        }

        e.Handled = true;
    }

    #region Unused

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

    #endregion
}
