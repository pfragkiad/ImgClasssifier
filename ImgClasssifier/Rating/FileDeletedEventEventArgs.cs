namespace ImgClasssifier.Rating;

public class FileDeletedEventEventArgs(string removedFilename, string ratedFilename, bool success) : EventArgs
{
    public string RemovedFilename { get; } = removedFilename;
    public string RatedFilename { get; } = ratedFilename;

    public bool Success { get; } = success;
}
