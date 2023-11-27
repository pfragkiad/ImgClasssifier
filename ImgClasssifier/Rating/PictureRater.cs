using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.CodeDom;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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

    public void Reset(bool resetRatedFiles)
    {
        _currentIndex = -1;
        _currentFile = "";
        _unratedFilePaths = [];

        if (resetRatedFiles)
            _ratedFiles = [];

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

    List<string> _unratedFilePaths = [];
    public List<string> UnratedFilePaths { get => _unratedFilePaths; }
    public int UnratedImagesCount { get => _unratedFilePaths.Count; }


    List<UnratedRatedFile> _ratedFiles = [];
    public List<UnratedRatedFile> RatedFiles { get => _ratedFiles; }

    public List<string> GetRatedImagesPaths() =>
        _ratedFiles
        .Select(r => Path.Combine(TargetBasePath!, r.RatedFilename))
        .ToList();
    //UnratedRatedFile.GetRatedImagesPaths(LogFile!, TargetBasePath!);


    public int RatedImagesCount { get => _ratedFiles.Count; }


    public event EventHandler? LoadingUnratedFiles;

    public void LoadUnratedFilesAndCheckRatedFiles()
    {
        //txtLog.Clear();
        LoadingUnratedFiles?.Invoke(this, EventArgs.Empty);

        Reset(resetRatedFiles: true);

        //updates the _unratedFilePaths
        LoadUnratedFilePaths();

        //load rated files
        _ratedFiles = UnratedRatedFile.FromLogfile(LogFile!);

        //remove already rated images from _images
        RemoveDuplicateAlreadyRatedFiles();

        //check for orphan rated files
        MoveUnregisteredRatedFiles();

        GotoNextFile();
    }

    private void LoadUnratedFilePaths()
    {
        _unratedFilePaths = Directory
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
        for (int i = _unratedFilePaths.Count - 1; i >= 0; i--)
        {
            string f = _unratedFilePaths[i];
            string fileName = Path.GetFileName(f);
            UnratedRatedFile? ratedFileEntry = _ratedFiles.FirstOrDefault(r => String.Compare(r.UnratedFilename, fileName, true) == 0);
            if (ratedFileEntry is not null)
            {
                try
                {
                    File.Delete(f);
                    _unratedFilePaths.RemoveAt(i);

                    RemovedDuplicateAlreadyRatedFile?.Invoke(this, new FileDeletedEventEventArgs(fileName, ratedFileEntry.RatedFilename, true));
                }
                catch
                {
                    RemovedDuplicateAlreadyRatedFile?.Invoke(this, new FileDeletedEventEventArgs(f, ratedFileEntry.RatedFilename, false));
                }
            }
        }

    }

    public event EventHandler<FileMovedUnregisteredFileEventEventArgs>? MovedUnregisteredRatedFile;

    private void MoveUnregisteredRatedFiles()
    {
        HashSet<string> registeredRatedFiles = _ratedFiles.Select(r => r.RatedFilename).ToHashSet();

        string[] unregisteredFiles = Directory
            .GetFiles(TargetBasePath!)
            .Where(f =>
            {
                string filename = Path.GetFileName(f);
                if (registeredRatedFiles.Contains(filename)) return false;

                return IsImageFile(f);
            })
            .ToArray();

        //List<string> filesToMove = new();
        foreach (string ratedFile in unregisteredFiles)
        {
            File.Move(ratedFile, Path.Combine(UnregisteredPath!, Path.GetFileName(ratedFile)));
            MovedUnregisteredRatedFile?.Invoke(this, new FileMovedUnregisteredFileEventEventArgs(ratedFile));
        }
    }



    #endregion



    #region Proceed to next file and save current file

    public event EventHandler? ProceededToNextFile;
    public void GotoNextFile()
    {
        if (_unratedFilePaths.Count == 0)
        {
            Reset(resetRatedFiles: false);
            return;
        }

        _currentIndex++;

        //cycle
        if (_currentIndex >= _unratedFilePaths.Count) _currentIndex = 0;
        _currentFile = _unratedFilePaths[_currentIndex];

        ProceededToNextFile?.Invoke(this, EventArgs.Empty);
    }

    public void SaveCurrentFile(int rating) =>
        SaveUnratedImageAsRated(_currentFile, rating);

    public event EventHandler<FileMoveFailedEventArgs>? MoveImageFailed;

    private void SaveUnratedImageAsRated(string fileName, int rating)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName)) return;

        try
        {
            string extension = Path.GetExtension(fileName);

            //string targetFileNameWithoutCounter = $"{rating:000}";
            //string[] existingFiles = Directory.GetFiles(TargetBasePath!, $"{targetFileNameWithoutCounter}_*{extension}");
            //int counter = existingFilePaths.Length > 0 ?
            //    existingFilePaths.Select(f => int.Parse(Path.GetFileNameWithoutExtension(f)[4..])).Max() + 1 : 1;
            //string targetFilename = $"{targetFileNameWithoutCounter}_{index:0000}{extension}";

            RatingIndex ratingIndex = new(rating, _ratedFiles
                .Select(r => r.GetRatingIndex())
                .Where(r => r is not null && r.Rating == rating)
                .Select(r => r!.Index)
                .DefaultIfEmpty()
                .Max() + 1);
            string targetFilename = $"{ratingIndex}{extension}";

            //if (ratingIndex.Index > 9999) throw new InvalidOperationException("Cannot have more than 9999 images per rating.");

            string targetPath = Path.Combine(TargetBasePath!, targetFilename);
            File.Move(fileName, targetPath);

            //update ratedFiles
            UnratedRatedFile newRecord = new(Path.GetFileName(fileName), targetFilename);
            _ratedFiles.Add(newRecord);

            //append new record to log file
            File.AppendAllText(LogFile!, $"{newRecord}\r\n");

            //update unratedFiles
            _unratedFilePaths.Remove(fileName);
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


    private void BackupLogFile()
    {
        File.Copy(LogFile!, Path.Combine(Path.GetDirectoryName(LogFile!)!, Path.GetFileName(LogFile!) + ".bak"), true);
    }

    public void ResetSortOrderInLogfile()
    {
        BackupLogFile();

        //_ratedFiles = [.. _ratedFiles.OrderBy(f => f.RatedFilename)];
        _ratedFiles = [.. _ratedFiles.Order()];

        UnratedRatedFile.SaveToLogFile(_ratedFiles, LogFile!);
    }

    #endregion

    public bool ChangeRating(string ratedFilename, int newRating, int? newIndex = null)
    {
        int iRated = _ratedFiles.FindIndex(r => r.RatedFilename == ratedFilename);
        if (iRated == -1) //unregistered case SHOULD NOT HAPPEN
            throw new InvalidOperationException(ratedFilename + " is unregistered!");

        var oldRatingIndex = _ratedFiles[iRated].GetRatingIndex(); //RatingIndex.FromFilename(ratedFilename);
        if (oldRatingIndex is null)
            throw new InvalidOperationException($"The filename is not in rater index format ('{ratedFilename}').");
        //return false; //cannot change

        if (newRating == oldRatingIndex.Rating && newIndex == oldRatingIndex.Index)
            return false; //no change


        string extension = Path.GetExtension(ratedFilename);

        if (newIndex.HasValue)
        {
            _ratedFiles[iRated] = new UnratedRatedFile(
                _ratedFiles[iRated].UnratedFilename, $"{newRating:000}_{newIndex:0000}{extension}");
        } //ELSE GET ALL COUNT


        //ResetSortOrderInLogfile();


        //SHOULD REMOVE CACHED FILE



        return false;
    }

}
