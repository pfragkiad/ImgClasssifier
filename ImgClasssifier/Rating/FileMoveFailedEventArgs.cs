namespace ImgClasssifier.Rating;

public class FileMoveFailedEventArgs(string unratedFilename, string reason) : EventArgs
{
    public string UnratedFilename { get; } = unratedFilename;
    public string Reason { get; } = reason;
}
