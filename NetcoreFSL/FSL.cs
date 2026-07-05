using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Classes;
using NetcoreFSL.Searcher.Enums;

namespace NetcoreFSL
{
  public class FSL
  {
    /// <summary>Versión semántica de la biblioteca (p. ej. <c>0.1.0</c>).</summary>
    public static string Version => FSLVersion.Current;

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
