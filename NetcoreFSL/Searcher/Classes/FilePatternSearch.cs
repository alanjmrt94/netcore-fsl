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
      searchPattern = SearchPatternHelper.NormalizeFile(pattern);
    }

    public override void StartSearch()
    {
      RunFSL();
    }

    protected override void ProcessDirectory(string folder)
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
  }
}
