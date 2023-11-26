using Microsoft.Extensions.Configuration;
using System.CodeDom;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static ImgClasssifier.Images.ImageExtensions;

namespace ImgClasssifier.Rating;

//TODO: Add extension to rated filename

public partial class PictureRater
{
    private readonly IConfiguration _configuration;

    int _currentIndex = -1;
    public int CurrentIndex { get => _currentIndex; }

    string _currentFile = "";
    public string CurrentFile { get => _currentFile; }

    List<string> _unratedFiles = new();
    public int UnratedImagesCount { get => _unratedFiles.Count; }



    public RatingIndex? GetRatingIndex(string filename)
    {
        Match m = Regex.Match(filename, @"^(?<rating>\d{3})_(?<index>\d{4})\.");
        if (!m.Success) return null;

        return new RatingIndex(
            rating: int.Parse(m.Groups["rating"].Value),
            index: int.Parse(m.Groups["index"].Value));
    }


    /*
        {
          "sourceBasePath": "D:\\temp\\ai",
          "targetBasePath": "D:\\temp\\ai\\rated",
          "unregisteredBasePath": "D:\\temp\\ai\\rated\\unregistered",
          "logFile": "D:\\temp\\ai\\rated\\log.txt"
        }
     */

    #region Constructor and reset



    public PictureRater(IConfiguration configuration)
    {
        _configuration = configuration;

        CheckSettings();
    }

    string? _sourceBasePath, _targetBasePath, _logFile, _unregisteredPath;

    private void CheckSettings()
    {
        _sourceBasePath = _configuration["sourceBasePath"] ?? throw new InvalidOperationException("Configure 'sourceBasePath' prior to any operation.");
        if (!Directory.Exists(_sourceBasePath)) throw new InvalidOperationException($"SourceBasePath: '{_sourceBasePath}' does not exist.");
        _targetBasePath = _configuration["targetBasePath"] ?? Path.Combine(_sourceBasePath, "rated");
        if (!Directory.Exists(_targetBasePath)) Directory.CreateDirectory(_targetBasePath);

        _logFile = _configuration["logFile"] ?? Path.Combine(_targetBasePath, "log.txt");
        if (!Directory.Exists(Path.GetDirectoryName(_logFile)))
            Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);

        _unregisteredPath = _configuration["unregisteredBasePath"] ?? Path.Combine(_targetBasePath, "unregistered");
        if (!Directory.Exists(_unregisteredPath)) Directory.CreateDirectory(_unregisteredPath);

    }

    public event EventHandler? ResetCompleted;

    public void Reset()
    {
        _currentIndex = -1;
        _currentFile = "";
        _unratedFiles = [];

        ResetCompleted?.Invoke(this, EventArgs.Empty);
    }

    #endregion


    #region Load and check unrated/rated files

    public event EventHandler? LoadingUnratedFiles;

    public void LoadUnratedFilesAndCheckRatedFiles()
    {
        //txtLog.Clear();
        LoadingUnratedFiles?.Invoke(this, EventArgs.Empty);

        Reset();

        //updates the _images
        LoadUnratedFiles();

        //remove already rated images from _images
        RemoveDuplicateAlreadyRatedFiles();

        //check for orphan rated files
        MoveUnregisteredRatedFiles();

        GotoNextFile();
    }

 

    private void LoadUnratedFiles()
    {
        List<string> excludedDirectoryNames = _configuration.GetSection("excludedDirectoryNames").GetChildren().Select(c => c.Value!).ToList();

        bool searchSubDirectories = _configuration.GetValue<bool?>("searchSubDirectories") ?? true;
        _unratedFiles = Directory
            .GetFiles(_sourceBasePath!, "*.*",
                searchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(f =>
            {
                string fileName = Path.GetFileName(f);
                var m = CodedRegex().Match(fileName);

                //ignore the rated images, non-image files and files with parents in exclusions list
                return !m.Success
                    && IsImageFile(f)
                    && !excludedDirectoryNames.Contains(Path.GetFileName(Path.GetDirectoryName(f)!));
            })
            .ToList();

    }

    public event EventHandler<FileDeletedEventEventArgs>? RemovedDuplicateAlreadyRatedFile;

    private void RemoveDuplicateAlreadyRatedFiles()
    {
        //HashSet<string> ratedImages = GetRatedImages(originalFile:true);
        var ratedImages = GetRatedImagesDictionaryFromLogFile();

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
        HashSet<string> ratedImages = GetRatedImagesFilenamesFromLogFile(originalFile: false);

        string[] allRatedFiles = Directory
            .GetFiles(_targetBasePath!)
            .Where(f=> IsImageFile(f))
            .ToArray();

        //List<string> filesToMove = new();
        for (int i = allRatedFiles.Length - 1; i >= 0; i--)
        {
            string ratedFile = allRatedFiles[i];
            string ratedFilename = Path.GetFileName(ratedFile);
            if (!ratedImages.Contains(ratedFilename))
            {

                File.Move(ratedFile, Path.Combine(_unregisteredPath!, Path.GetFileName(ratedFile)));
                MovedUnregisteredRatedFile?.Invoke(this, new FileMovedUnregisteredFileEventEventArgs(ratedFile));
            }
        }
    }

    private IEnumerable<string> GetLogfileLines() =>
        File.ReadAllLines(_logFile!)
            .Where(l => !string.IsNullOrWhiteSpace(l) && l.Contains('\t'));

    private Dictionary<string, string> GetRatedImagesDictionaryFromLogFile() =>
        GetLogfileLines()
            .ToDictionary(l => l.Split('\t')[0], l => l.Split('\t')[1]);

    public List<string> GetRatedImagesFullPaths() => 
        GetRatedImagesFilenamesFromLogFile(false)
            .Select(f => Path.Combine(_targetBasePath!, f))
            .ToList();

    private HashSet<string> GetRatedImagesFilenamesFromLogFile(bool originalFile) =>
        GetLogfileLines()
            .Select(l => l.Split('\t')[originalFile ? 0 : 1])
            .ToHashSet();


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

    public void SaveUnratedImageAsRated(string fileName, int rating)
    {
        try
        {
            string targetFileNameWithoutCounter = $"{rating:000}";
            string[] existingFiles = Directory.GetFiles(_targetBasePath!, $"{targetFileNameWithoutCounter}_*.jpg");
            int counter = existingFiles.Length > 0 ?
                existingFiles.Select(f => int.Parse(Path.GetFileNameWithoutExtension(f)[4..])).Max() + 1 : 1;

            if (counter > 9999) throw new InvalidOperationException("Cannot have more than 9999 images per rating.");

            string targetFilename = $"{targetFileNameWithoutCounter}_{counter:0000}.jpg";
            string targetPath = Path.Combine(_targetBasePath!, targetFilename);
            File.Move(fileName, targetPath);

            //log file
            var writer = File.AppendText(_logFile!);
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

    public bool SwapRatings(string ratedFilenameWithoutExtension1, string ratedFilenameWithoutExtension2)
    {
        //both files should be in "rating" format
        Match m = CodedRegex().Match(ratedFilenameWithoutExtension1);
        if (!m.Success) return false;
        m = CodedRegex().Match(ratedFilenameWithoutExtension2);
        if (!m.Success) return false;

        BackupLogFile();

        //get all log records (file will be saved back)
        var ratedImages = File.ReadAllLines(_logFile!).
            Select(l => (UnratedFile: l.Split('\t')[0], RatedFile: l.Split('\t')[1])).
            OrderBy(e => e.RatedFile).ToList();




        return false;
    }

    private void BackupLogFile()
    {
        File.Copy(_logFile!, Path.Combine(Path.GetDirectoryName(_logFile!)!, Path.GetFileName(_logFile!) + ".bak"), true);
    }

    public void ResetSortOrderInLogfile()
    {
        BackupLogFile();

        var ratedImages = File.ReadAllLines(_logFile!).
            Select(l => (UnratedFile: l.Split('\t')[0], RatedFile: l.Split('\t')[1])).
            OrderBy(e => e.RatedFile).ToList();

        using StreamWriter writer = new StreamWriter(_logFile!);
        foreach (var e in ratedImages)
            writer.WriteLine($"{e.UnratedFile}\t{e.RatedFile}");
    }


}
