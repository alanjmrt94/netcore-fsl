using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Classes;
using NetcoreFSL.Searcher.Enums;
using NetcoreFSL.Searcher.Events;

namespace NetcoreFSL
{
  public class FSL
  {
    /// <summary>Versión semántica de la biblioteca (p. ej. <c>0.2.0</c>).</summary>
    public static string Version => FSLVersion.Current;

    private readonly ExecuteHandlers HandlerOption;
    private readonly string folder;
    private readonly string pattern;
    private SearcherBase searcher;

    public event EventHandler<FileEventArgs> FilesFound;
    public event EventHandler<SearchCompletedEventArgs> SearchCompleted;

    public FSL(ExecuteHandlers handlerOption, string folder, string pattern)
    {
      HandlerOption = handlerOption;
      this.folder = folder;
      this.pattern = pattern;
    }

    public void FileSearch()
    {
      searcher = new FilePatternSearch(HandlerOption, folder, pattern);
      WireEvents(searcher);
      searcher.StartSearch();
    }

    public void FolderSearch()
    {
      searcher = new FolderPatternSearch(HandlerOption, folder, pattern);
      WireEvents(searcher);
      searcher.StartSearch();
    }

    private void WireEvents(SearcherBase instance)
    {
      instance.FilesFound += (_, e) => FilesFound?.Invoke(this, e);
      instance.SearchCompleted += (_, e) => SearchCompleted?.Invoke(this, e);
    }
  }
}
