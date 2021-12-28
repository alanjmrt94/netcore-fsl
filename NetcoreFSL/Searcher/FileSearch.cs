using System.Collections.Concurrent;
using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Classes;
using NetcoreFSL.Searcher.Enums;

namespace NetcoreFSL.Searcher
{
  public class FileSearch
  {
    private SearcherBase searcher;

    public FileSearch(ExecuteHandlers handlerOption, string folder, string pattern)
    {
      searcher = new FilePatternSearch(handlerOption, folder, pattern);
    }

    public void StartSearch()
    {
      searcher.StartSearch();
    }
  }
}