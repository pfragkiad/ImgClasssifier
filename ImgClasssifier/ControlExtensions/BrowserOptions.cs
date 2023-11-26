using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgClasssifier.ControlExtensions;

public class BrowserOptions
{
    public RotateFlipType RotateForBrowsing { get; set; } = RotateFlipType.RotateNoneFlipNone;

    public string? CachedThumbnailsDirectory { get; set; }

}
