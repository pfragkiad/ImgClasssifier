using System.Diagnostics;

namespace ImgClasssifier.ControlExtensions;

public class ListItemGraph
{
    private readonly ListView _listView;

    public ListItemGraph(ListView listView)
    {
        _listView = listView;
    }

    public List<ListViewItem>? Items { get; private set; }

    public void Update()
    {
        //  if (_listView.Items.Count == 0) return;

            Items = _listView.Items.Cast<ListViewItem>().OrderBy(item => item.Position.Y).ThenBy(item => item.Position.X).ToList();

            _rows = Items.Select(item => item.Position.Y).Distinct().Count();
            _itemsPerRow = Items.Select(item => item.Position.X).Distinct().Count();
    }

    private int _itemsPerRow;
    public int ItemsPerRow { get => _itemsPerRow; }

    private int _rows;
    public int Rows { get => _rows; }

    public int Count { get => Items?.Count ?? 0; }
}
