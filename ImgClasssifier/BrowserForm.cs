
using ImgClasssifier.ControlExtensions;
using ImgClasssifier.Images;
using ImgClasssifier.Rating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Policy;

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

        toolTip1.SetToolTip(trackBar1, $"{trackBar1.Value}");
        listView1.SelectedIndexChanged += ListView1_SelectedIndexChanged;
    }

    private void ListView1_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (listView1.SelectedItems.Count == 0) return;

        var selectedItem = listView1.SelectedItems[0];
        var currentRating = RatingIndex.FromFilename(selectedItem.Text);
        trackBar1.Value = currentRating?.Rating ?? 0;
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

    public string? CacheDirectory { get; set; }


    //TODO: Fix drag and drop item.
    //TODO: Reload a single photo (right-click)
    public void UpdateBrowser()
    {
        var ratedImageFiles =
           //UnratedRatedFile.GetRatedImagesPaths(_rater.LogFile!, _rater.TargetBasePath!); 
           _rater.GetRatedImagesPaths(true);

        RotateFlipType rotate = _options.RotateForBrowsing;
        CacheDirectory = _options.CachedThumbnailsDirectory ?? Path.Combine(_rater.TargetBasePath!, "cached");

        Stopwatch w = Stopwatch.StartNew();

        listView1.AddImageListViewItems(imageList1, ratedImageFiles, rotate, CacheDirectory);

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

    private void trackBar1_ValueChanged(object sender, EventArgs e)
    {
        toolTip1.SetToolTip(trackBar1, $"{trackBar1.Value}");
    }

    private void btnChangeRating_Click(object sender, EventArgs e)
    {
        if (listView1.SelectedItems.Count == 0) return;

        var selectedItem = listView1.SelectedItems[0];
        var currentRating = RatingIndex.FromFilename(selectedItem.Text);
        if (currentRating is null) return;

        if (currentRating.Rating == trackBar1.Value) return; //no update


        //remove cache file

        if(!Directory.Exists(CacheDirectory)) return; //////

        Cursor.Current = Cursors.WaitCursor;
        Application.UseWaitCursor = true;

        string cachedFile = ImageExtensions.GetCachedFilePath(
            selectedItem.Text,
            imageList1.ImageSize.Width,
            imageList1.ImageSize.Height,
            listView1.BackColor,
            _options.RotateForBrowsing,
            CacheDirectory);

        UnratedRatedFile newRatedFile = _rater.ChangeRatingAndGetNewRatedFile(selectedItem.Text,trackBar1.Value);

        //update listitem text
        selectedItem.Text = newRatedFile.RatedFilename;

        //update cached file
        string newCachedFile = ImageExtensions.GetRenamedCachedFilename(newRatedFile.RatedFilename, cachedFile);
        File.Move(cachedFile,Path.Combine(CacheDirectory,newCachedFile));

        UpdateBrowser(); //review this

        var updatedListItem = listView1.Items.Cast<ListViewItem>().First(item => item.Text == newRatedFile.RatedFilename);
        updatedListItem.Selected = true;
        updatedListItem.EnsureVisible();

        Cursor.Current = Cursors.Default;
        Application.UseWaitCursor = false;


    }
}
