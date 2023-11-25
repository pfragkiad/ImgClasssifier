
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ImgClasssifier;

public partial class BrowserForm : Form
{
    private readonly PictureRater _rater;

    public BrowserForm(PictureRater rater)
    {
        InitializeComponent();
        _rater = rater;

    }



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

        int i = 0;
        foreach (var ratedFilename in imagesPaths)
        {
            using FileStream stream = new FileStream(ratedFilename, FileMode.Open, FileAccess.Read);
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
            destX = System.Convert.ToInt16((newWidth -
                      (sourceWidth * nPercent)) / 2);
        }
        else
        {
            nPercent = nPercentW;
            destY = System.Convert.ToInt16((newHeight -
                      (sourceHeight * nPercent)) / 2);
        }

        int destWidth = (int)(sourceWidth * nPercent);
        int destHeight = (int)(sourceHeight * nPercent);


        Bitmap bmPhoto = new Bitmap(newWidth, newHeight,
                      PixelFormat.Format24bppRgb);

        bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                     imgPhoto.VerticalResolution);

        
        Graphics grPhoto = Graphics.FromImage(bmPhoto);
        
        grPhoto.Clear(listView1.BackColor);

        grPhoto.InterpolationMode =
            System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        grPhoto.DrawImage(imgPhoto,
            new Rectangle(destX, destY, destWidth, destHeight),
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            GraphicsUnit.Pixel);

        grPhoto.Dispose();
        imgPhoto.Dispose();
        return bmPhoto;
    }

    static Image ScaleImage(Image bmp, int maxWidth, int maxHeight)
    {
        var ratioX = (double)maxWidth / bmp.Width;
        var ratioY = (double)maxHeight / bmp.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(bmp.Width * ratio);
        var newHeight = (int)(bmp.Height * ratio);

        var newImage = new Bitmap(newWidth, newHeight);

        using (var graphics = Graphics.FromImage(newImage))
            graphics.DrawImage(bmp, 0, 0, newWidth, newHeight);


        return newImage;
    }

    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        }

        return destImage;
    }

    private void button1_Click(object sender, EventArgs e)
    {
        Application.UseWaitCursor = true;
        Cursor.Current = Cursors.WaitCursor;

        AddThumbnails();
        Application.UseWaitCursor = false;

        Cursor.Current = Cursors.Default;
    }
}
