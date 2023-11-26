using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImgClasssifier.Images;

using static System.Windows.Forms.AxHost;

namespace ImgClasssifier.ControlExtensions;

public static class ImageListExtensions
{
    public static List<Thumbnail> AddThumbnails(this ImageList imageList1, IEnumerable<string> filePaths, int thumbnailWidth, int thumbnailHeight, Color backColor, RotateFlipType rotate = RotateFlipType.RotateNoneFlipNone)
    {
        List<Thumbnail> thumbnailImages = ImageExtensions.GetThumbnails(filePaths, thumbnailWidth, thumbnailHeight, backColor, rotate);

        imageList1.Images.Clear();
        imageList1.Images.AddRange(thumbnailImages.Select(e => e.Image).ToArray());
        for (int i = 0; i < thumbnailImages.Count; i++)
            imageList1.Images.SetKeyName(i, thumbnailImages[i].Key);

        return thumbnailImages;
    }

}
