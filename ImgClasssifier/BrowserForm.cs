
using ImgClasssifier.ControlExtensions;
using ImgClasssifier.Images;
using ImgClasssifier.Rating;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ImgClasssifier;

public partial class BrowserForm : Form
{
    private readonly PictureRater _rater;
    private readonly IConfiguration _configuration;

    public BrowserForm(PictureRater rater, IConfiguration configuration)
    {
        InitializeComponent();

        listView1.EnableDoubleBuffering();
        _rater = rater;
        _configuration = configuration;
        _listDragDropper = new ListViewListItemDragDropper(listView1);
        _listDragDropper.ItemMoved += ListDragDropper_ItemMoved;
    }

    private void ListDragDropper_ItemMoved(object? sender, ListItemMovedEventArgs e)
    {
        var currentRating = _rater.GetRatingIndex(e.Item!.Text);
        var previousRating = e.Previous is not null ? _rater.GetRatingIndex(e.Previous!.Text) : null;
        var nextRating = e.Next is not null ? _rater.GetRatingIndex(e.Next!.Text) : null;

        if (previousRating is not null && nextRating is not null &&
            previousRating >= nextRating)
        {
            MessageBox.Show("Previous and next are not in correct order.");
            return;
        }

        bool alreadySorted = true;
        if (previousRating is not null) alreadySorted &= currentRating > previousRating;
        if (nextRating is not null) alreadySorted &= currentRating < nextRating;

        //lblReport.Text = $"Current: {e.Item.Text}, Previous: {e.Previous?.Text ?? "<Null>"}, Next: {e.Next?.Text ?? "<Null>"}";
        lblReport.Text = $"Current: {currentRating}, Previous: {previousRating}, Next: {nextRating} ({(alreadySorted ? "SORTED" : "NOT SORTED")})";

        //unsorted scenarios

        //3_3 | (3_5) | 3_4  ..  3_6 .. (same rating, index change)
        // 3_5 <-> 3_4


        //lblReport.Text = $"Current: {e.Item.Text}, Previous: {e.Previous?.Text ?? "<Null>"}, Next: {e.Next?.Text ?? "<Null>"}";
    }

    enum ResortScenario
    {
        SameRating
    }

    ListViewListItemDragDropper? _listDragDropper;


    //TODO: Cache thumbnails
    //TODO: Reload a single photo (right-click)

    //TODO: Add options (rater and browser -> modify json)
    private void AddThumbnails()
    {
        var ratedImageFiles = _rater.GetRatedImagesFullPaths();
        RotateFlipType rotate = Enum.Parse<RotateFlipType>(_configuration["rotateForBrowsing"] ?? RotateFlipType.RotateNoneFlipNone.ToString());
        string cacheDirectory = _configuration["cachedThumbnailsDirectory"] ?? Path.Combine(_rater.TargetBasePath!, "cached");

        Stopwatch w = Stopwatch.StartNew();
        
        listView1.AddImageListViewItems(imageList1, ratedImageFiles, rotate, cacheDirectory);

        w.Stop(); MessageBox.Show(w.Elapsed.TotalSeconds.ToString());
    }


    private void button1_Click(object sender, EventArgs e)
    {
        Application.UseWaitCursor = true;
        Cursor.Current = Cursors.WaitCursor;

        AddThumbnails();
        Application.UseWaitCursor = false;

        Cursor.Current = Cursors.Default;
    }

    private void listView1_KeyDown(object sender, KeyEventArgs e)
    {
        //if(e.KeyCode ==Keys.Right )
        //{
        //    if ((listView1.SelectedItems[0] as ListViewItem)?.FindNearestItem(SearchDirectionHint.Right) is null)
        //    {
        //        listView1.SelectedItems.Clear();

        //    }
        //}
    }
}
