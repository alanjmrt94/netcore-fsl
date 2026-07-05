using System.Security;
using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Enums;
using NetcoreFSL.Searcher.Helpers;

namespace NetcoreFSL.Searcher.Classes
{
  internal class FilePatternSearch : SearcherBase
  {
    private readonly string searchPattern;

    public FilePatternSearch(
      ExecuteHandlers handlerOption,
      string folder,
      string pattern = "",
      CancellationToken cancellationToken = default) : base(handlerOption, folder, pattern, cancellationToken)
    {
      searchPattern = SearchPatternHelper.Normalize(pattern);
    }

    public override void StartSearch()
    {
      RunFSL();
    }

    protected override void GetDrives()
    {
      List<DriveInfo> drives = DriveInfo.GetDrives()
        .Where(drive => drive.IsReady)
        .ToList();

      if (drives.Count > 0)
      {
        OnDrivesFound(drives);
      }
    }

    protected override void GetFiles(string folder)
    {
      try
      {
        FileInfo[] files = new DirectoryInfo(folder).GetFiles(searchPattern);

        if (files.Length > 0)
        {
          OnFilesFound(files.ToList());
        }
      }
      catch (Exception ex) when (
        ex is PathTooLongException
        or DirectoryNotFoundException
        or SecurityException
        or UnauthorizedAccessException
        or IOException)
      {
      }
    }

    protected override List<DirectoryInfo> GetFolders(string folder)
    {
      if (!TryEnumerateSubdirectories(folder, out DirectoryInfo[] subdirectories))
      {
        return new List<DirectoryInfo>();
      }

      return subdirectories.Length > 0
        ? subdirectories.ToList()
        : new List<DirectoryInfo>();
    }
  }
}
