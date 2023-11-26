
using ImgClasssifier.ControlExtensions;
using ImgClasssifier.Rating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ImgClasssifier;

public partial class BrowserForm : Form
{
    private readonly PictureRater _rater;
    private readonly BrowserOptions _options;

    public BrowserForm(PictureRater rater,
        IOptions<BrowserOptions> options)
    {
        InitializeComponent();

        listView1.EnableDoubleBuffering();
        _rater = rater;
        _options = options.Value;
        _listDragDropper = new ListViewListItemDragDropper(listView1);
        _listDragDropper.ItemMoved += ListDragDropper_ItemMoved;
    }

    private void ListDragDropper_ItemMoved(object? sender, ListItemMovedEventArgs e)
    {
        var currentRating = RatingIndex.FromFilename(e.Item!.Text);
        var previousRating = e.Previous is not null ? RatingIndex.FromFilename(e.Previous!.Text) : null;
        var nextRating = e.Next is not null ? RatingIndex.FromFilename(e.Next!.Text) : null;

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


    //TODO: Fix drag and drop item.
    //TODO: Reload a single photo (right-click)
    public void UpdateBrowser()
    {
        var ratedImageFiles = //UnratedRatedFile.GetRatedImagesPaths(_rater.LogFile!, _rater.TargetBasePath!); 
            _rater.GetRatedImagesPaths();

        RotateFlipType rotate = _options.RotateForBrowsing;
        string cacheDirectory = _options.CachedThumbnailsDirectory ?? Path.Combine(_rater.TargetBasePath!, "cached");

        Stopwatch w = Stopwatch.StartNew();

        listView1.AddImageListViewItems(imageList1, ratedImageFiles, rotate, cacheDirectory);

        w.Stop();
        if (w.ElapsedMilliseconds > 5000)
            MessageBox.Show(w.Elapsed.TotalSeconds.ToString());
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
