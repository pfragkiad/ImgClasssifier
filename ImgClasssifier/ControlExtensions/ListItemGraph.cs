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

    public ListViewItem? GetLastSelectedItem()
    {
        var selectedItems = _listView.SelectedItems.Cast<ListViewItem>().ToList();
        if(selectedItems.Count==0)  return null;    

        var lastSelectedItem = selectedItems[0];
        var lastPosition = lastSelectedItem.Position;

        for(int i=1; i<selectedItems.Count; i++) {
            var position = selectedItems[i].Position;
            if(position.Y*1000+position.X > lastPosition.Y * 1000 + lastPosition.X)
            {
                lastSelectedItem = selectedItems[i];
                lastPosition = position;
            }
        }
        return lastSelectedItem;
    }

    public ListViewItem? GetFirstSelectedItem()
    {
        var selectedItems = _listView.SelectedItems.Cast<ListViewItem>().ToList(); 
        if (selectedItems.Count == 0) return null;
        
        var firstSelectedItem = selectedItems[0];
        var firstPosition = firstSelectedItem.Position;

        for (int i = 1; i < selectedItems.Count; i++)
        {
            var position = selectedItems[i].Position;
            if (position.Y * 1000 + position.X < firstPosition.Y * 1000 + firstPosition.X)
            {
                firstSelectedItem = selectedItems[i];
                firstPosition = position;
            }
        }
        return firstSelectedItem;
    }

    private int _itemsPerRow;
    public int ItemsPerRow { get => _itemsPerRow; }

    private int _rows;
    public int Rows { get => _rows; }

    public int Count { get => Items?.Count ?? 0; }
}
