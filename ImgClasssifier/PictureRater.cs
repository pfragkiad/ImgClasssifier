using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace ImgClasssifier;

public class FileDeletedEventEventArgs(string removedFilename, string ratedFilename, bool success) : EventArgs
{
    public string RemovedFilename { get; } = removedFilename;
    public string RatedFilename { get; } = ratedFilename;

    public bool Success { get; } = success;
}


public class FileMoveFailedEventArgs(string unratedFilename, string reason) : EventArgs
{
    public string UnratedFilename { get; } = unratedFilename;
    public string Reason { get; } = reason;
}

public class FileMovedUnregisteredFileEventEventArgs(string ratedFilename) : EventArgs
{
    public string RatedFilename { get; } = ratedFilename;
}

public partial class PictureRater
{
    private readonly IConfiguration _configuration;

    int _currentIndex = -1;
    public int CurrentIndex { get => _currentIndex; }

    string _currentFile = "";
    public string CurrentFile { get => _currentFile; }

    List<string> _images = new ();
    public int UnratedImagesCount { get => _images.Count; }

    string? _sourceBasePath, _targetBasePath, _logFile, _unregisteredPath;

    public List<string> GetRatedImages()
    {
        return GetRatedImagesFilenames(false).Select(f => Path.Combine(_targetBasePath,f)).ToList();
    }



    /*
        {
          "sourceBasePath": "D:\\temp\\ai",
          "targetBasePath": "D:\\temp\\ai\\rated",
          "unregisteredBasePath": "D:\\temp\\ai\\rated\\unregistered",
          "logFile": "D:\\temp\\ai\\rated\\log.txt"
        }
     */

    public PictureRater(IConfiguration configuration)
    {
        _configuration = configuration;

        CheckSettings();
    }

    private void CheckSettings()
    {
        _sourceBasePath = _configuration["sourceBasePath"] ?? throw new InvalidOperationException("Configure 'sourceBasePath' prior to any operation.");
        if (!Directory.Exists(_sourceBasePath)) throw new InvalidOperationException($"SourceBasePath: '{_sourceBasePath}' does not exist.");
        _targetBasePath = _configuration["targetBasePath"] ?? Path.Combine(_sourceBasePath,"rated");
        if (!Directory.Exists(_targetBasePath)) Directory.CreateDirectory(_targetBasePath);
       
        _logFile = _configuration["logFile"] ?? Path.Combine(_targetBasePath,"log.txt");
        if (!Directory.Exists(Path.GetDirectoryName(_logFile))) 
            Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);
        
        _unregisteredPath = _configuration["unregisteredBasePath"] ?? Path.Combine(_targetBasePath,"unregistered");
        if (!Directory.Exists(_unregisteredPath)) Directory.CreateDirectory(_unregisteredPath);

    }

    public event EventHandler? Refreshing;

    public void RefreshFiles()
    {
        //txtLog.Clear();
        Refreshing?.Invoke(this, EventArgs.Empty);

        Reset();

        //updates the internal images property
        LoadUnratedFiles();

        RemoveDuplicateAlreadyRatedFiles();

        //CheckRegisteredRatedFiles();
        MoveUnregisteredRatedFiles();

        GotoNextFile();
    }


    public event EventHandler? ResetCompleted;

    public void Reset()
    {
        _currentIndex = -1;
        _currentFile = "";
        ResetCompleted?.Invoke(this,EventArgs.Empty);
    }

    private void LoadUnratedFiles()
    {
        var exclusionsParentDirectories = _configuration.GetSection("exclusions").GetChildren().Select(c=> c.Value).ToList();

        _images = Directory.GetFiles(_sourceBasePath!, "*.jpg", SearchOption.AllDirectories).Where(f =>
        {
            string fileName = Path.GetFileNameWithoutExtension(f);
            var m = CodedRegex().Match(fileName);
            //ignore the rated images
            if (m.Success) return false;

            
            if(exclusionsParentDirectories.Contains( Path.GetFileName(Path.GetDirectoryName(f)) ) )
                return false;


            return !m.Success;
        }).ToList();

    }

    public event EventHandler<FileDeletedEventEventArgs>? RemovedDuplicateAlreadyRatedFile;

    private void RemoveDuplicateAlreadyRatedFiles()
    {
        //HashSet<string> ratedImages = GetRatedImages(originalFile:true);
        var ratedImages = GetRatedImagesDictionary();

        for (int i = _images.Count - 1; i >= 0; i--)
        {
            string f= _images[i];
            string fileName = Path.GetFileName(f);
            if (ratedImages.ContainsKey(fileName))
            {
                try
                {
                    File.Delete(f);
                    _images.RemoveAt(i);

                    RemovedDuplicateAlreadyRatedFile?.Invoke(this, new FileDeletedEventEventArgs(fileName, ratedImages[fileName],true));
                }
                catch {
                    RemovedDuplicateAlreadyRatedFile?.Invoke(this, new FileDeletedEventEventArgs(f, ratedImages[fileName], false));
                }
                //txtLog.AppendText($"Removed {fileName}: exists as {ratedImages[fileName]}\r\n");
            }
        }

    }
    private Dictionary<string, string> GetRatedImagesDictionary()
    {
        string logFile = _configuration["logFile"]!;
        var ratedImages = File.ReadAllLines(logFile)
            .ToDictionary(l => l.Split('\t')[0], l => l.Split('\t')[1]);

        return ratedImages;
    }

    public void ResetSortOrderInLogfile()
    {
        string logFile = _configuration["logFile"]!;
        //store backup file
        File.Copy(logFile, Path.Combine(Path.GetDirectoryName(logFile),Path.GetFileName(logFile) + ".bak"),true);

        var ratedImages = File.ReadAllLines(logFile).
            Select(l => (UnratedFile: l.Split('\t')[0], RatedFile: l.Split('\t')[1])).
            OrderBy(e => e.RatedFile).ToList();

        using StreamWriter writer = new StreamWriter(logFile);
        foreach (var e in ratedImages)
            writer.WriteLine($"{e.UnratedFile}\t{e.RatedFile}");
    }

    public event EventHandler<FileMovedUnregisteredFileEventEventArgs>? MovedUnregisteredRatedFile;
    private void MoveUnregisteredRatedFiles()
    {
        HashSet<string> ratedImages = GetRatedImagesFilenames(originalFile: false);

        string[] allRatedFiles = Directory.GetFiles(_targetBasePath!, "*.jpg");

        //List<string> filesToMove = new();
        for (int i = allRatedFiles.Length - 1; i >= 0; i--)
        {
            string ratedFile = allRatedFiles[i];
            string ratedFilename = Path.GetFileName(ratedFile);
            if (!ratedImages.Contains(ratedFilename))
            {

                File.Move(ratedFile, Path.Combine(_unregisteredPath, Path.GetFileName(ratedFile)));
                MovedUnregisteredRatedFile?.Invoke(this, new FileMovedUnregisteredFileEventEventArgs(ratedFile));
            }
        }
    }


    private HashSet<string> GetRatedImagesFilenames(bool originalFile)
    {
        string logFile = _configuration["logFile"]!;
        HashSet<string> ratedImages = File.ReadAllLines(logFile).Select(l =>
            l.Split('\t')[originalFile ? 0 : 1]
        ).ToHashSet();
        return ratedImages;
    }



    public event EventHandler? ProceededToNextFile;
    public void GotoNextFile()
    {
        if (_images.Count == 0)
        {
            Reset();
            return;
        }

        _currentIndex++;

        //cycle
        if (_currentIndex >= _images.Count) _currentIndex = 0;

        _currentFile = _images[_currentIndex];

        ProceededToNextFile?.Invoke(this, EventArgs.Empty);
    }

    public void SaveCurrentFile(int rating)
    {
        if (_currentFile == "") return;

        SaveImageAs(_currentFile, rating);
    }

    public event EventHandler<FileMoveFailedEventArgs> MoveImageFailed;

    public void SaveImageAs(string fileName, int rating)
    {
        try
        {
            string targetFileNameWithoutCounter = $"{rating:000}";
            string[] existingFiles = Directory.GetFiles(_targetBasePath!, $"{targetFileNameWithoutCounter}_*.jpg");
            int counter = existingFiles.Length > 0 ?
                existingFiles.Select(f => int.Parse(Path.GetFileNameWithoutExtension(f).Substring(4))).Max() + 1 : 1;

            if (counter > 9999) throw new InvalidOperationException("Cannot have more than 9999 images per rating.");

            string targetFilename = $"{targetFileNameWithoutCounter}_{counter:0000}.jpg";
            string targetPath = Path.Combine(_targetBasePath!, targetFilename);
            File.Move(fileName, targetPath);

            //log file
            string logFile = _configuration["logFile"]!;
            var writer = File.AppendText(logFile);
            writer.WriteLine($"{Path.GetFileName(fileName)}\t{targetFilename}");
            writer.Flush(); writer.Close();

            _images.Remove(fileName);
            _currentIndex--;
        }
        catch(Exception ex) { 
            MoveImageFailed?.Invoke(this, new FileMoveFailedEventArgs(fileName, ex.Message));
        }
    }

    [GeneratedRegex(@"^\d{3}_\d{4}$", RegexOptions.IgnoreCase, "el-GR")]
    private static partial Regex CodedRegex();
}
