using System.Security;
using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Enums;
using NetcoreFSL.Searcher.Helpers;

namespace NetcoreFSL.Searcher.Classes
{
  internal class FolderPatternSearch : SearcherBase
  {
    private readonly string searchPattern;

    public FolderPatternSearch(
      ExecuteHandlers handlerOption,
      string folder,
      string pattern = "",
      CancellationToken cancellationToken = default) : base(handlerOption, folder, pattern, cancellationToken)
    {
      searchPattern = SearchPatternHelper.NormalizeFolder(pattern);
    }

    public override void StartSearch()
    {
      RunFSL();
    }

    protected override void ProcessDirectory(string folder)
    {
      try
      {
        DirectoryInfo[] folders = new DirectoryInfo(folder).GetDirectories(searchPattern);

        if (folders.Length > 0)
        {
          OnFoldersFound(folders.ToList());
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
  }
}
