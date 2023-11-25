namespace ImgClasssifier.Rating;

public class FileMovedUnregisteredFileEventEventArgs(string ratedFilename) : EventArgs
{
    public string RatedFilename { get; } = ratedFilename;
}
