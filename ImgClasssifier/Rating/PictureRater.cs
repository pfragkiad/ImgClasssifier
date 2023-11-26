using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.CodeDom;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static ImgClasssifier.Images.ImageExtensions;

namespace ImgClasssifier.Rating;

public partial class PictureRater
{
    /*
      "rater": {
        "sourceBasePath": "D:\\temp\\ai",
        "searchSubDirectories": true,
        "excludedDirectoryNames": [ "[Originals]", "cached" ],
        "targetBasePath": "D:\\temp\\ai\\rated",
        "unregisteredBasePath": "D:\\temp\\ai\\rated\\unregistered",
        "logFile": "D:\\temp\\ai\\rated\\log.txt",
        }
     */

    #region Constructor and reset

    public PictureRater(IOptions<RaterOptions> options)
    {
        _options = options.Value;
        CheckSettings();
    }

    public event EventHandler? ResetCompleted;

    int _currentIndex = -1;
    public int CurrentIndex { get => _currentIndex; }

    string _currentFile = "";
    public string CurrentFile { get => _currentFile; }

    public void Reset()
    {
        _currentIndex = -1;
        _currentFile = "";
        _unratedFiles = [];

        ResetCompleted?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Rater options

    private readonly RaterOptions _options;

    public string SourceBasePath => _options.SourceBasePath;

    public string? TargetBasePath
    {
        get => _options.TargetBasePath;
        set => _options.TargetBasePath = value;
    }

    public string? LogFile
    {
        get => _options.LogFile;
        set => _options.LogFile = value;
    }

    public string? UnregisteredPath
    {
        get => _options.UnregisteredBasePath;
        set => _options.UnregisteredBasePath = value;
    }

    public List<string> ExcludedDirectoryNames
    {
        get => _options.ExcludedDirectoryNames;
        set => _options.ExcludedDirectoryNames = value;
    }

    public bool SearchSubDirectories
    {
        get => _options.SearchSubDirectories;
        set => _options.SearchSubDirectories = value;
    }

    private void CheckSettings()
    {
        //_sourceBasePath = _configuration["sourceBasePath"] ?? throw new InvalidOperationException("Configure 'sourceBasePath' prior to any operation.");
        if (!Directory.Exists(SourceBasePath)) throw new InvalidOperationException($"SourceBasePath: '{SourceBasePath}' does not exist.");

        //_targetBasePath = _configuration["targetBasePath"] ?? Path.Combine(SourceBasePath, "rated");
        if (string.IsNullOrWhiteSpace(TargetBasePath)) TargetBasePath = Path.Combine(SourceBasePath, "rated");
        if (!Directory.Exists(TargetBasePath)) Directory.CreateDirectory(TargetBasePath);

        if (string.IsNullOrWhiteSpace(LogFile)) LogFile = Path.Combine(TargetBasePath, "log.txt");
        if (!Directory.Exists(Path.GetDirectoryName(LogFile)))
            Directory.CreateDirectory(Path.GetDirectoryName(LogFile)!);

        if (string.IsNullOrWhiteSpace(UnregisteredPath)) UnregisteredPath = Path.Combine(TargetBasePath, "unregistered");
        if (!Directory.Exists(UnregisteredPath)) Directory.CreateDirectory(UnregisteredPath);

        //List<string> excludedDirectoryNames = _configuration.GetSection("excludedDirectoryNames").GetChildren().Select(c => c.Value!).ToList();

        //bool searchSubDirectories = _configuration.GetValue<bool?>("searchSubDirectories") ?? true;
    }

    #endregion

    #region Load and check unrated/rated files

    List<string> _unratedFiles = [];

    public event EventHandler? LoadingUnratedFiles;

    public void LoadUnratedFilesAndCheckRatedFiles()
    {
        //txtLog.Clear();
        LoadingUnratedFiles?.Invoke(this, EventArgs.Empty);

        Reset();

        //updates the _images
        LoadUnratedFilePaths();

        //remove already rated images from _images
        RemoveDuplicateAlreadyRatedFiles();

        //check for orphan rated files
        MoveUnregisteredRatedFiles();

        GotoNextFile();
    }

    private void LoadUnratedFilePaths()
    {
        _unratedFiles = Directory
            .GetFiles(SourceBasePath!, "*.*",
                SearchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(f =>
            {
                string fileName = Path.GetFileName(f);
                var m = CodedRegex().Match(fileName);

                //ignore the rated images, non-image files and files with parents in exclusions list
                return !m.Success
                    && IsImageFile(f)
                    && !ExcludedDirectoryNames.Contains(Path.GetFileName(Path.GetDirectoryName(f)!));
            })
            .ToList();

    }

    public event EventHandler<FileDeletedEventEventArgs>? RemovedDuplicateAlreadyRatedFile;

    private void RemoveDuplicateAlreadyRatedFiles()
    {
        var ratedImages = UnratedRatedFile.GetRatedImagesDictionaryFromLogFile(LogFile!);

        for (int i = _unratedFiles.Count - 1; i >= 0; i--)
        {
            string f = _unratedFiles[i];
            string fileName = Path.GetFileName(f);
            if (ratedImages.TryGetValue(fileName, out string? value))
            {
                try
                {
                    File.Delete(f);
                    _unratedFiles.RemoveAt(i);

                    RemovedDuplicateAlreadyRatedFile?.Invoke(this, new FileDeletedEventEventArgs(fileName, value, true));
                }
                catch
                {
                    RemovedDuplicateAlreadyRatedFile?.Invoke(this, new FileDeletedEventEventArgs(f, value, false));
                }
                //txtLog.AppendText($"Removed {fileName}: exists as {ratedImages[fileName]}\r\n");
            }
        }

    }

    public event EventHandler<FileMovedUnregisteredFileEventEventArgs>? MovedUnregisteredRatedFile;

    private void MoveUnregisteredRatedFiles()
    {
        HashSet<string> ratedImages = UnratedRatedFile.GetRatedImagesFilenamesFromLogFile(LogFile!);

        string[] allRatedFiles = Directory
            .GetFiles(TargetBasePath!)
            .Where(f => IsImageFile(f))
            .ToArray();

        //List<string> filesToMove = new();
        for (int i = allRatedFiles.Length - 1; i >= 0; i--)
        {
            string ratedFile = allRatedFiles[i];
            string ratedFilename = Path.GetFileName(ratedFile);
            if (!ratedImages.Contains(ratedFilename))
            {

                File.Move(ratedFile, Path.Combine(UnregisteredPath!, Path.GetFileName(ratedFile)));
                MovedUnregisteredRatedFile?.Invoke(this, new FileMovedUnregisteredFileEventEventArgs(ratedFile));
            }
        }
    }

    public int UnratedImagesCount { get => _unratedFiles.Count; }

    #endregion



    #region Proceed to next file and save current file

    public event EventHandler? ProceededToNextFile;
    public void GotoNextFile()
    {
        if (_unratedFiles.Count == 0)
        {
            Reset();
            return;
        }

        _currentIndex++;

        //cycle
        if (_currentIndex >= _unratedFiles.Count) _currentIndex = 0;
        _currentFile = _unratedFiles[_currentIndex];

        ProceededToNextFile?.Invoke(this, EventArgs.Empty);
    }

    public void SaveCurrentFile(int rating)
    {
        if (_currentFile == "") return;

        SaveUnratedImageAsRated(_currentFile, rating);
    }

    public event EventHandler<FileMoveFailedEventArgs>? MoveImageFailed;

    private void SaveUnratedImageAsRated(string fileName, int rating)
    {
        try
        {
            string targetFileNameWithoutCounter = $"{rating:000}";
            string[] existingFiles = Directory.GetFiles(TargetBasePath!, $"{targetFileNameWithoutCounter}_*.jpg");
            int counter = existingFiles.Length > 0 ?
                existingFiles.Select(f => int.Parse(Path.GetFileNameWithoutExtension(f)[4..])).Max() + 1 : 1;

            if (counter > 9999) throw new InvalidOperationException("Cannot have more than 9999 images per rating.");

            string targetFilename = $"{targetFileNameWithoutCounter}_{counter:0000}.jpg";
            string targetPath = Path.Combine(TargetBasePath!, targetFilename);
            File.Move(fileName, targetPath);

            //log file
            var writer = File.AppendText(LogFile!);
            writer.WriteLine($"{Path.GetFileName(fileName)}\t{targetFilename}");
            writer.Flush(); writer.Close();

            _unratedFiles.Remove(fileName);
            _currentIndex--;
        }
        catch (Exception ex)
        {
            MoveImageFailed?.Invoke(this, new FileMoveFailedEventArgs(fileName, ex.Message));
        }
    }

    [GeneratedRegex(@"^\d{3}_\d{4}\.", RegexOptions.IgnoreCase)]
    private static partial Regex CodedRegex();

    #endregion


    //public bool SwapRatings(string ratedFilenameWithoutExtension1, string ratedFilenameWithoutExtension2)
    //{
    //    //both files should be in "rating" format
    //    Match m = CodedRegex().Match(ratedFilenameWithoutExtension1);
    //    if (!m.Success) return false;
    //    m = CodedRegex().Match(ratedFilenameWithoutExtension2);
    //    if (!m.Success) return false;

    //    BackupLogFile();

    //    //get all log records (file will be saved back)
    //    var ratedImages = File.ReadAllLines(LogFile!).
    //        Select(l => (UnratedFile: l.Split('\t')[0], RatedFile: l.Split('\t')[1])).
    //        OrderBy(e => e.RatedFile).ToList();




    //    return false;
    //}

    #region Log file

    public List<string> GetRatedImagesPaths() => UnratedRatedFile.GetRatedImagesPaths(LogFile!, TargetBasePath!);

    private void BackupLogFile()
    {
        File.Copy(LogFile!, Path.Combine(Path.GetDirectoryName(LogFile!)!, Path.GetFileName(LogFile!) + ".bak"), true);
    }
    
    public void ResetSortOrderInLogfile()
    {
        BackupLogFile();

        List<UnratedRatedFile> unratedRatedFiles = UnratedRatedFile.FromLogfile(LogFile!);

        SaveToLogFile(unratedRatedFiles);
    }

    private void SaveToLogFile(IEnumerable<UnratedRatedFile> unratedRatedFiles)
    {
        using StreamWriter writer = new(LogFile!);
        foreach (var e in unratedRatedFiles)
            writer.WriteLine($"{e.UnratedFilename}\t{e.RatedFilename}");

    }

    #endregion

    public bool ChangeRating(string ratedFilename, int newRating, int? newIndex=null)
    {
        var oldRatingIndex = RatingIndex.FromFilename(ratedFilename);
        if (oldRatingIndex is null)
            //throw new InvalidOperationException($"The filename is not in rater index format ('{ratedFilename}').");
            return false; //cannot change

        if (newRating == oldRatingIndex.Rating && newIndex == oldRatingIndex.Index)
            return false; //no change

        //if we arrive here we can continue

        

        return false;
    }

}
