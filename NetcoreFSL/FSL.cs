using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Classes;
using NetcoreFSL.Searcher.Enums;
using NetcoreFSL.Searcher.Events;

namespace NetcoreFSL
{
  /// <summary>
  /// Fachada pública para búsqueda de archivos y carpetas.
  /// Los eventos se suscriben en esta instancia; persisten entre llamadas a
  /// <see cref="FileSearch"/> y <see cref="FolderSearch"/>.
  /// Cada nueva búsqueda cancela la anterior si aún está en ejecución.
  /// </summary>
  public class FSL
  {
    /// <summary>Versión semántica de la biblioteca (p. ej. <c>0.4.0</c>).</summary>
    public static string Version => FSLVersion.Current;

    private readonly ExecuteHandlers handlerOption;
    private readonly string folder;
    private readonly string pattern;
    private readonly CancellationToken cancellationToken;
    private SearcherBase searcher;

    public event EventHandler<DriveEventArgs> DrivesFound;
    public event EventHandler<FileEventArgs> FilesFound;
    public event EventHandler<FolderEventArgs> FoldersFound;
    public event EventHandler<SearchCanceledEventArgs> SearchCanceled;
    public event EventHandler<SearchCompletedEventArgs> SearchCompleted;
    public event EventHandler<SearchPausedEventArgs> SearchPaused;
    public event EventHandler<SearchResumedEventArgs> SearchResumed;

    public FSL(ExecuteHandlers handlerOption, string folder, string pattern)
      : this(handlerOption, folder, pattern, CancellationToken.None)
    {
    }

    public FSL(ExecuteHandlers handlerOption, string folder, string pattern, CancellationToken cancellationToken)
    {
      this.handlerOption = handlerOption;
      this.folder = folder;
      this.pattern = pattern;
      this.cancellationToken = cancellationToken;
    }

    /// <summary>Inicia búsqueda de archivos por patrón.</summary>
    public void FileSearch()
    {
      PrepareSearcher(new FilePatternSearch(handlerOption, folder, pattern, cancellationToken));
      StartSearcher();
    }

    /// <summary>Inicia búsqueda de carpetas por patrón.</summary>
    public void FolderSearch()
    {
      PrepareSearcher(new FolderPatternSearch(handlerOption, folder, pattern, cancellationToken));
      StartSearcher();
    }

    /// <summary>Cancela la búsqueda en curso.</summary>
    public void CancelSearch()
    {
      searcher?.CancelSearch();
    }

    /// <summary>Pausa la búsqueda en curso.</summary>
    public void PauseSearch()
    {
      searcher?.PauseSearch();
    }

    /// <summary>Reanuda la búsqueda pausada.</summary>
    public void ResumeSearch()
    {
      searcher?.ResumeSearch();
    }

    private void PrepareSearcher(SearcherBase instance)
    {
      if (searcher != null)
      {
        searcher.CancelSearch();
        UnwireEvents(searcher);
      }

      searcher = instance;
      WireEvents(searcher);
    }

    private void StartSearcher()
    {
      if (handlerOption == ExecuteHandlers.InNewTask)
      {
        Task.Run(() => searcher.StartSearch(), cancellationToken);
      }
      else
      {
        searcher.StartSearch();
      }
    }

    private void WireEvents(SearcherBase instance)
    {
      instance.DrivesFound += ForwardDrivesFound;
      instance.FilesFound += ForwardFilesFound;
      instance.FoldersFound += ForwardFoldersFound;
      instance.SearchCanceled += ForwardSearchCanceled;
      instance.SearchCompleted += ForwardSearchCompleted;
      instance.SearchPaused += ForwardSearchPaused;
      instance.SearchResumed += ForwardSearchResumed;
    }

    private void UnwireEvents(SearcherBase instance)
    {
      instance.DrivesFound -= ForwardDrivesFound;
      instance.FilesFound -= ForwardFilesFound;
      instance.FoldersFound -= ForwardFoldersFound;
      instance.SearchCanceled -= ForwardSearchCanceled;
      instance.SearchCompleted -= ForwardSearchCompleted;
      instance.SearchPaused -= ForwardSearchPaused;
      instance.SearchResumed -= ForwardSearchResumed;
    }

    private void ForwardDrivesFound(object sender, DriveEventArgs e) => DrivesFound?.Invoke(this, e);
    private void ForwardFilesFound(object sender, FileEventArgs e) => FilesFound?.Invoke(this, e);
    private void ForwardFoldersFound(object sender, FolderEventArgs e) => FoldersFound?.Invoke(this, e);
    private void ForwardSearchCanceled(object sender, SearchCanceledEventArgs e) => SearchCanceled?.Invoke(this, e);
    private void ForwardSearchCompleted(object sender, SearchCompletedEventArgs e) => SearchCompleted?.Invoke(this, e);
    private void ForwardSearchPaused(object sender, SearchPausedEventArgs e) => SearchPaused?.Invoke(this, e);
    private void ForwardSearchResumed(object sender, SearchResumedEventArgs e) => SearchResumed?.Invoke(this, e);
  }
}
