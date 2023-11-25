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
        Items = _listView.Items.Cast<ListViewItem>().OrderBy(item => item.Position.Y).ThenBy(item => item.Position.X).ToList();
    }
}
