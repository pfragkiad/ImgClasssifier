using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgClasssifier.Rating;

public class UnratedRatedFile : IComparable<UnratedRatedFile>
{
    public UnratedRatedFile(string unratedFilename, string ratedFilename)
    {
        UnratedFilename = unratedFilename;
        RatedFilename = ratedFilename;
    }

    public string UnratedFilename { get; }

    private string _ratedFilename = default!;

    public string RatedFilename
    {
        get { return _ratedFilename; }
        set
        {
            _ratedFilename = value;
            RatingIndex = RatingIndex.FromFilename(_ratedFilename);
        }
    }

    public RatingIndex? RatingIndex { get; private set; }

    public override string ToString() =>
        $"{UnratedFilename}\t{_ratedFilename}";

    public static UnratedRatedFile? FromLogfileLine(string l)
    {
        string[] tokens = l.Split('\t');
        if (tokens.Length < 2) return null;
        return new UnratedRatedFile(tokens[0], tokens[1]);
    }

    //TODO: Do not load lines with duplicate rated or unrated values.

    public static List<UnratedRatedFile> FromLogfile(string logFile)
    {
        if (!File.Exists(logFile))
            return [];

        return File.ReadAllLines(logFile)
            .Where(l => !string.IsNullOrWhiteSpace(l) && l.Contains('\t'))
            .Select(l => FromLogfileLine(l)!)
            .ToList();
    }

    public static HashSet<string> GetRatedImagesFilenamesFromLogFile(string logFile) =>
        FromLogfile(logFile).Select(r => r._ratedFilename).ToHashSet();

    ////unrated name is the key
    //public static Dictionary<string, string> GetRatedImagesDictionaryFromLogFile(string logFile) =>
    //    FromLogfile(logFile).ToDictionary(r => r.UnratedFilename, r => r.RatedFilename, StringComparer.OrdinalIgnoreCase);


    public static List<string> GetRatedImagesPaths(string logFile, string targetBasePath) =>
        GetRatedImagesFilenamesFromLogFile(logFile)
        .Select(f => Path.Combine(targetBasePath, f))
        .ToList();

    public static void SaveToLogFile(IEnumerable<UnratedRatedFile> unratedRatedFiles, string logFile) =>
        File.WriteAllLines(logFile, unratedRatedFiles.Select(e => $"{e}"));

    //Allows the use of Order() in a List.
    public int CompareTo(UnratedRatedFile? other)
    {
        return _ratedFilename.CompareTo(other._ratedFilename);
    }
}

