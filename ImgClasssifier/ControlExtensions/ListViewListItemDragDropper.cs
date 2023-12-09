namespace ImgClasssifier.ControlExtensions;

public class ListViewListItemDragDropper
{
    private readonly ListView _listView;

    private ListItemGraph _graph;
    public ListItemGraph Graph { get => _graph; }

    public ListViewListItemDragDropper(ListView listView)
    {
        _listView = listView;
        _graph = new ListItemGraph(listView);

        //_listView.MouseDown += ListView_MouseDown;
        //_listView.MouseMove += ListView_MouseMove;
        //_listView.MouseUp += ListView_MouseUp;
        _listView.KeyDown += ListView_KeyDown;
        //  _listView.Resize += _listView_Resize;
    }

    private void _listView_Resize(object? sender, EventArgs e)
    {
        //  _graph.Update();
    }


    public void RefreshGraph() => _graph?.Update();

    private void ListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (_listView.SelectedItems.Count == 0) return;

        HashSet<Keys> handledKeys = new HashSet<Keys> { Keys.Right, Keys.Left, Keys.Up, Keys.Down };
        if (handledKeys.Contains(e.KeyCode) && _graph.Items is null)
            _graph.Update();

        if (e.KeyCode == Keys.Right)
        {
            //var firstSelectedItem = _listView.SelectedItems.Cast<ListViewItem>().First();
            var firstSelectedItem = _graph.GetFirstSelectedItem()!;
            int i = _graph.Items.IndexOf(firstSelectedItem);
            if (i < _graph.Items.Count - 1)
            {
                _listView.SelectedItems.Clear();
                _graph.Items[i + 1].Selected = true;
                _graph.Items[i + 1].EnsureVisible();
            }
        }
        else if (e.KeyCode == Keys.Left)
        {
            //var lastSelectedItem = _listView.SelectedItems.Cast<ListViewItem>().Last();
            var lastSelectedItem = _graph.GetLastSelectedItem()!;
            int i = _graph.Items.IndexOf(lastSelectedItem);
            if (i > 0)
            {
                _listView.SelectedItems.Clear();
                _graph.Items[i - 1].Selected = true;
                _graph.Items[i - 1].EnsureVisible();
            }
        }
        else if (e.KeyCode == Keys.Up)
        {
            var firstSelectedItem = _graph.GetFirstSelectedItem()!;
            int i = _graph.Items.IndexOf(firstSelectedItem);
            if (i > _graph.ItemsPerRow - 1)
            {
                _listView.SelectedItems.Clear();
                _graph.Items[i - _graph.ItemsPerRow].Selected = true;
                _graph.Items[i - _graph.ItemsPerRow].EnsureVisible();
            }
        }

        e.Handled = handledKeys.Contains(e.KeyCode);

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
