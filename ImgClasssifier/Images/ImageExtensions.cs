using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ImgClasssifier.Images;

public static class ImageExtensions
{
    public static bool IsImageFile(string fileName)
    {
        try
        {
            using Image img = Image.FromFile(fileName);
            return true;
        }
        catch (OutOfMemoryException) { return false; }
    }

    public static Image GetUnlockedImageFromFile(string fileName)
    {
        using FileStream stream = new(fileName, FileMode.Open, FileAccess.Read);
        return Image.FromStream(stream);
    }

    public static Image GetThumbnailImage(string fileName, int thumbnailWidth, int thumbnailHeight, Color backColor, RotateFlipType rotate = RotateFlipType.RotateNoneFlipNone)
    {
        var image = GetUnlockedImageFromFile(fileName);

        if (rotate != RotateFlipType.RotateNoneFlipNone)
            image.RotateFlip(rotate);

        //imageList1.ImageSize.Width, imageList1.ImageSize.Height, listView1.BackColor
        Image thumbnailImage = FitImage(image, thumbnailWidth, thumbnailHeight, backColor);
        image.Dispose();

        return thumbnailImage;
    }

    public static List<Thumbnail> GetThumbnails(IEnumerable<string> filePaths, int thumbnailWidth, int thumbnailHeight, Color backColor, RotateFlipType rotate = RotateFlipType.RotateNoneFlipNone)
    {
        //key is the filename
        ConcurrentBag<Thumbnail> images = new();
        Parallel.ForEach(filePaths,
            ratedFilename =>
            images.Add(new Thumbnail
            {
                Key = Path.GetFileName(ratedFilename),
                Image = GetThumbnailImage(ratedFilename, thumbnailWidth, thumbnailHeight, backColor, rotate)
            })
        );
        return images.OrderBy(t=>t.Key).ToList();
    }



    public static Image FitImage(Image image, int newWidth, int newHeight, Color backColor)
    {
        int sourceWidth = image.Width;
        int sourceHeight = image.Height;

        int sourceX = 0, sourceY = 0, destX = 0, destY = 0;

        float nPercentW = (float)newWidth / (float)sourceWidth;
        float nPercentH = (float)newHeight / (float)sourceHeight;
        float nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;
        int destWidth = (int)(sourceWidth * nPercent);
        int destHeight = (int)(sourceHeight * nPercent);

        if (nPercentH < nPercentW)
            destX = (int)((newWidth - sourceWidth * nPercent) / 2);
        else
            destY = (int)((newHeight - sourceHeight * nPercent) / 2);


        Bitmap bmPhoto = new(newWidth, newHeight, PixelFormat.Format24bppRgb);

        bmPhoto.SetResolution(image.HorizontalResolution, image.VerticalResolution);


        Graphics grPhoto = Graphics.FromImage(bmPhoto);

        grPhoto.Clear(backColor);

        grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

        grPhoto.DrawImage(image,
            new Rectangle(destX, destY, destWidth, destHeight),
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            GraphicsUnit.Pixel);

        grPhoto.Dispose();
        image.Dispose();
        return bmPhoto;
    }



}
