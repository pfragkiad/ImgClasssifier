
using ImgClasssifier.ControlExtensions;
using ImgClasssifier.Rating;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ImgClasssifier;

public partial class BrowserForm : Form
{
    private readonly PictureRater _rater;

    public BrowserForm(PictureRater rater)
    {
        InitializeComponent();
        _rater = rater;

        listView1.EnableDoubleBuffering();
        _listDragDropper = new ListViewListItemDragDropper(listView1);
        _listDragDropper.ItemMoved += ListDragDropper_ItemMoved;
    }

    private void ListDragDropper_ItemMoved(object? sender, ListItemMovedEventArgs e)
    {
        var currentRating = _rater.GetRatingIndex(e.Item!.Text);
        var previousRating = e.Previous is not null? _rater.GetRatingIndex(e.Previous!.Text) : null;
        var nextRating = e.Next is not null ? _rater.GetRatingIndex(e.Next!.Text) : null;

        if(previousRating is not null && nextRating is not null &&
            previousRating >= nextRating)
        {
            MessageBox.Show("Previous and next are not in correct order.");
            return;
        }

        bool alreadySorted = true;
        if(previousRating is not null) alreadySorted &= currentRating > previousRating;
        if (nextRating is not null) alreadySorted &= currentRating < nextRating;

       //lblReport.Text = $"Current: {e.Item.Text}, Previous: {e.Previous?.Text ?? "<Null>"}, Next: {e.Next?.Text ?? "<Null>"}";
        lblReport.Text = $"Current: {currentRating}, Previous: {previousRating}, Next: {nextRating} ({(alreadySorted? "SORTED" : "NOT SORTED")})";

        //unsorted scenarios

        //3_3 | (3_5) | 3_4  ..  3_6 .. (same rating, index change)
        // every image prior to 3
        var affectedRatedImages = _rater
            .GetRatedImages()
            .Select(img => _rater.GetRatingIndex(img))
            .Where(r => r is not null && r.Rating ==  );

        //lblReport.Text = $"Current: {e.Item.Text}, Previous: {e.Previous?.Text ?? "<Null>"}, Next: {e.Next?.Text ?? "<Null>"}";
    }

    enum ResortScenario
    {
        SameRating
    }

    ListViewListItemDragDropper? _listDragDropper;


    //TODO: Cache thumbnails
    //TODO: Reload a single photo (right-click)
    private void AddThumbnails()
    {
        if (imageList1.Images.Count > 0)
            imageList1.Images.Clear();

        if (listView1.Items.Count > 0)
            listView1.Items.Clear();

        var imagesPaths = _rater.GetRatedImages();
        progressBar1.Minimum = 0;
        progressBar1.Maximum = imagesPaths.Count;
        progressBar1.Visible = true;

        listView1.SuspendLayout();

        int i = 0;
        foreach (var ratedFilename in imagesPaths)
        {
            using FileStream stream = new FileStream(ratedFilename, FileMode.Open, FileAccess.Read);
            // if (ratedFilename.Contains("006_0010.jpg")) Debugger.Break();

            var image = Image.FromStream(stream);

            if(image.Width>image.Height)
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);

            //float ratio = (float)image.Width/ image.Height;

            //var image2 = ResizeImage(image, (int)(imageList1.ImageSize.Width*ratio), imageList1.ImageSize.Height);
            //var image2 = ScaleImage(image, imageList1.ImageSize.Width, imageList1.ImageSize.Height);
            //var image2 = resizeImage2(image, imageList1.ImageSize.Height, imageList1.ImageSize.Width);
            var image2 = resizeImage2(image, imageList1.ImageSize.Width, imageList1.ImageSize.Height);
            image.Dispose();

            string key = Path.GetFileName(ratedFilename);
            imageList1.Images.Add(key, image2);


            var item = listView1.Items.Add(key, key);

            progressBar1.Value = ++i; progressBar1.Refresh();

            //if (chkRotateRight.Checked) pictureBox1.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        progressBar1.Visible = false;
        listView1.ResumeLayout();


    }
    public Image resizeImage2(Image imgPhoto, int newWidth, int newHeight)
    {
        //Image imgPhoto = Image.FromFile(stPhotoPath);

        int sourceWidth = imgPhoto.Width;
        int sourceHeight = imgPhoto.Height;

        ////Consider vertical pics
        //if (sourceWidth < sourceHeight)
        //{
        //    int buff = newWidth;

        //    newWidth = newHeight;
        //    newHeight = buff;
        //}

        int sourceX = 0, sourceY = 0, destX = 0, destY = 0;
        float nPercent = 0, nPercentW = 0, nPercentH = 0;

        nPercentW = ((float)newWidth / (float)sourceWidth);
        nPercentH = ((float)newHeight / (float)sourceHeight);
        if (nPercentH < nPercentW)
        {
            nPercent = nPercentH;
            destX = Convert.ToInt16((newWidth -
                      (sourceWidth * nPercent)) / 2);
        }
        else
        {
            nPercent = nPercentW;
            destY = Convert.ToInt16((newHeight -
                      (sourceHeight * nPercent)) / 2);
        }

        int destWidth = (int)(sourceWidth * nPercent);
        int destHeight = (int)(sourceHeight * nPercent);


        Bitmap bmPhoto = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

        bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);


        Graphics grPhoto = Graphics.FromImage(bmPhoto);

        grPhoto.Clear(listView1.BackColor);

        grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

        grPhoto.DrawImage(imgPhoto,
            new Rectangle(destX, destY, destWidth, destHeight),
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            GraphicsUnit.Pixel);

        grPhoto.Dispose();
        imgPhoto.Dispose();
        return bmPhoto;
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
