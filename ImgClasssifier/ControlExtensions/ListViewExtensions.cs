using ImagesAdvanced;

namespace ImgClasssifier.ControlExtensions;

public static class ListViewExtensions
{
    public static void EnableDoubleBuffering(this Control control)
    {
        typeof(Control).GetProperty("DoubleBuffered",
                     System.Reflection.BindingFlags.NonPublic |
                     System.Reflection.BindingFlags.Instance)!
       .SetValue(control, true, null);
    }

    public static void AddImageListViewItems(
        this ListView listView,
        ImageList imageList,
        IEnumerable<string> imageFiles,
        RotateFlipType rotate,
        string? cacheDirectory = null
        )
    {
        listView.BeginUpdate();
        //should DISABLE the resize event HERE
        listView.Items.Clear();

        int thumbnailWidth = imageList.ImageSize.Width;
        int thumbnailHeight = imageList.ImageSize.Height;
        Color backColor = listView.BackColor;

        List<Thumbnail> thumbnails = imageList.AddThumbnails(imageFiles, thumbnailWidth, thumbnailHeight, backColor, rotate, cacheDirectory);

        ListViewItem[] listViewItems = thumbnails
            .Select(t => new ListViewItem(t.Key, t.Key))
            .ToArray();

        listView.Items.AddRange(listViewItems);
        listView.EndUpdate();
    }

    public static IEnumerable<ListViewItem> GetOrderedListItems(this ListView listView) =>
        listView.Items
        .Cast<ListViewItem>()
        .Where(item => item is not null) //the items are null on clearing (!)
        .OrderBy(item => item.Position.Y)
        .ThenBy(item => item.Position.X);


}
