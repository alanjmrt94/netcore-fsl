using System.Collections.Concurrent;
using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Classes;
using NetcoreFSL.Searcher.Enums;

namespace NetcoreFSL
{
  public class FSL
  {
    private readonly ExecuteHandlers HandlerOption;
    private readonly string folder;
    private readonly string pattern;
    private SearcherBase searcher;

    public FSL(ExecuteHandlers handlerOption, string folder, string pattern)
    {
      HandlerOption = handlerOption;
      this.folder = folder;
      this.pattern = pattern;
    }

    public void FileSearch()
    {
      searcher = new FilePatternSearch(HandlerOption, folder, pattern);
      searcher.StartSearch();
    }

    public void FolderSearch()
    {
      searcher = new FolderPatternSearch(HandlerOption, folder, pattern);
      searcher.StartSearch();
    }
  }
}