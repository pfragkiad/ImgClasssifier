using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgClasssifier.Rating;

public class RaterOptions
{
    public required string SourceBasePath { get; init; }
    public bool SearchSubDirectories { get; set; } = false;
    public List<string> ExcludedDirectoryNames { get; set; } =  new ();
    public string? TargetBasePath { get; set; } 
    public string? UnregisteredBasePath { get; set; } 
    public string? LogFile { get; set; } 
}
