using ImgClasssifier.Images;
using static System.Windows.Forms.AxHost;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

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
        listView.Items.Clear();
        listView.SuspendLayout();

        int thumbnailWidth = imageList.ImageSize.Width;
        int thumbnailHeight = imageList.ImageSize.Height;
        Color backColor = listView.BackColor;

        List<Thumbnail> thumbnails = imageList.AddThumbnails(imageFiles, thumbnailWidth, thumbnailHeight, backColor, rotate, cacheDirectory);

        ListViewItem[] listViewItems = thumbnails
            .Select(t => new ListViewItem(t.Key, t.Key))
            .ToArray();

        listView.Items.AddRange(listViewItems);
        listView.ResumeLayout();
    }
}
