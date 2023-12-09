
using ImgClasssifier.ControlExtensions;
using ImagesAdvanced;
using ImgClasssifier.Rating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Policy;

namespace ImgClasssifier;

public partial class BrowserForm : Form
{
    private readonly PictureRater _rater;
    private readonly BrowserOptions2 _options;

    public BrowserForm(PictureRater rater,
        IOptions<BrowserOptions2> options)
    {
        InitializeComponent();

        listView1.EnableDoubleBuffering();
        picPreview.BackgroundImageLayout = ImageLayout.Zoom;

        _rater = rater;
        _options = options.Value;
        _listDragDropper = new ListViewListItemDragDropper(listView1);
        //_listDragDropper.ItemMoved += ListDragDropper_ItemMoved;

        toolTip1.SetToolTip(trackBar1, $"{trackBar1.Value}");
        listView1.SelectedIndexChanged += ListView1_SelectedIndexChanged;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (listView1.Items.Count == 0) return;

        listView1.Items[0].Selected = true;
        _listDragDropper.RefreshGraph();
    }

    private void ListView1_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (listView1.SelectedItems.Count == 0) return;

        var selectedItem = listView1.SelectedItems[0];
        var currentRating = RatingIndex.FromFileName(selectedItem.Text);

        picPreview.BackgroundImage =
            ImageExtensions.GetUnlockedImageFromFile(
                Path.Combine(_rater.TargetBasePath!, selectedItem.Text),
                _options.RotateForBrowsing);


        trackBar1.Value = currentRating?.Rating ?? 0;
    }

    private void ListDragDropper_ItemMoved(object? sender, ListItemMovedEventArgs e)
    {
        var currentRating = RatingIndex.FromFileName(e.Item!.Text);
        var previousRating = e.Previous is not null ? RatingIndex.FromFileName(e.Previous!.Text) : null;
        var nextRating = e.Next is not null ? RatingIndex.FromFileName(e.Next!.Text) : null;

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


    //TODO: Fix drag and drop item. (not needed)?
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

        _listDragDropper.RefreshGraph();

        w.Stop();
        if (w.ElapsedMilliseconds > 5000)
            MessageBox.Show(w.Elapsed.TotalSeconds.ToString());
    }



    private void trackBar1_ValueChanged(object sender, EventArgs e)
    {
        toolTip1.SetToolTip(trackBar1, $"{trackBar1.Value}");
    }

    private void btnChangeRating_Click(object sender, EventArgs e)
    {
        if (listView1.SelectedItems.Count == 0) return;

        var selectedItem = listView1.SelectedItems[0];

        btnChangeRating.Enabled = false;
        Wait();


        ChangeRating(selectedItem, trackBar1.Value, true, null);

        btnChangeRating.Enabled = true;
    
        listView1.Focus();
    _listDragDropper.RefreshGraph();
        StopWaiting();

    }

    private void DeleteRatingFile(ListViewItem item)
    {
        string currentFilename = item.Text;

        var items = listView1.GetOrderedListItems().ToList();
        int index = items.IndexOf(item);
        ListViewItem next = index < items.Count - 1 ? listView1.Items[index + 1] : listView1.Items[index - 1];


        //delete item
        _rater.DeleteRatingFile(currentFilename);

        //remove cache file
        if (Directory.Exists(CacheDirectory))
        {
            string currentCacheFilePath = ImageExtensions.GetCachedFilePath(
                currentFilename,
                imageList1.ImageSize.Width,
                imageList1.ImageSize.Height,
                listView1.BackColor,
                _options.RotateForBrowsing,
                CacheDirectory);

            File.Delete(currentCacheFilePath);
        }

        listView1.Items.Remove(item);

        next.Selected = true;
        next.EnsureVisible();

        _listDragDropper.RefreshGraph();


    }

    private void ChangeRating(ListViewItem item, int newRating, bool refreshBrowser, int? newIndex)
    {
        string currentFilename = item.Text;

        var currentRating = RatingIndex.FromFileName(currentFilename);
        if (currentRating is null) return;
        if (currentRating.Rating == newRating && (newIndex is null
            || newIndex == currentRating.Index)) return; //no update

        UnratedRatedFile newRatedFile = _rater.ChangeRatingAndGetNewRatedFile(currentFilename, newRating, newIndex);

        //remove cache file
        if (Directory.Exists(CacheDirectory))
        {
            string currentCacheFilePath = ImageExtensions.GetCachedFilePath(
                currentFilename,
                imageList1.ImageSize.Width,
                imageList1.ImageSize.Height,
                listView1.BackColor,
                _options.RotateForBrowsing,
                CacheDirectory);

            //update cached file
            string newCachedFile = ImageExtensions.GetRenamedCachedFileName(newRatedFile.RatedFilename, currentCacheFilePath);
            File.Move(currentCacheFilePath, Path.Combine(CacheDirectory, newCachedFile));
        }

        string nextRatedFilename = "";
        if (!chkMoveAfterChangeRating.Checked)
        {
            List<ListViewItem> items = listView1.GetOrderedListItems().ToList();
            int index = items.IndexOf(item);
            ListViewItem next = index < items.Count - 1 ? listView1.Items[index + 1] : listView1.Items[index - 1];
            nextRatedFilename = next.Text;
        }


        if (!refreshBrowser)
        { //update listitem text only
            item.Text = newRatedFile.RatedFilename;
            item.EnsureVisible();
        }
        else
        {
            UpdateBrowser();
            if (chkMoveAfterChangeRating.Checked)
            {
                var updatedListItem = listView1.Items.Cast<ListViewItem>().First(item => item.Text == newRatedFile.RatedFilename);
                updatedListItem.Selected = true;
                updatedListItem.EnsureVisible();
            }
            else
            {
                var nextListItem = listView1.Items.Cast<ListViewItem>().First(item => item.Text == nextRatedFilename);
                nextListItem.Selected = true;
                nextListItem.EnsureVisible();

            }
        }
    }

    private void btnRerateUniformly_Click(object sender, EventArgs e)
    {
        Wait();

        btnRerateUniformly.Enabled = false;

        trackBar1.ValueChanged -= trackBar1_ValueChanged;

        //listView1.BeginUpdate();

        List<ListViewItem> items = listView1.GetOrderedListItems().ToList();
        int count = items.Count;
        int countsPerGroup = count / 100 + 1;

        int currentRating = 100;
        int indexInGroup = countsPerGroup;

        listView1.BeginUpdate();

        progressBar1.Maximum = count;
        progressBar1.Minimum = 0;
        progressBar1.Value = 0;
        progressBar1.Visible = true;



        //first pass set the rating
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (indexInGroup == 0)
            {
                indexInGroup = countsPerGroup;
                currentRating--;
            }

            //temporarily use +200 range (to allow re-indexing without conflicts)
            ChangeRating(items[i], currentRating + 200, false, indexInGroup);

            indexInGroup--;

            if (i % 10 == 0)
            {
                progressBar1.Value = items.Count - i;
                progressBar1.Refresh();
            }


        }

        progressBar1.Hide();


        progressBar1.Value = 0;
        progressBar1.Visible = true;


        //second pass reset the rating back to the [0,100] range 
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var ratingIndex = RatingIndex.FromFileName(items[i].Text)!;
            ChangeRating(items[i], ratingIndex.Rating - 200, false, ratingIndex.Index);

            if (i % 10 == 0)
            {
                progressBar1.Value = items.Count - i;
                progressBar1.Refresh();
            }
        }


        progressBar1.Hide();

        trackBar1.ValueChanged += trackBar1_ValueChanged;
        UpdateBrowser();

        listView1.EndUpdate();

        btnRerateUniformly.Enabled = true;

        _listDragDropper.RefreshGraph();
        StopWaiting();
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (listView1.SelectedItems.Count == 0) return;
        var selectedItem = listView1.SelectedItems[0];

        var reply = MessageBox.Show($"Are you sure you want to delete {selectedItem.Text}?", "PictureRater", MessageBoxButtons.YesNo);
        if (reply == DialogResult.No) return;

        DeleteRatingFile(selectedItem);

    }
}
