using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Classes;
using NetcoreFSL.Searcher.Enums;
using NetcoreFSL.Searcher.Events;

namespace NetcoreFSL
{
  /// <summary>
  /// Fachada pública para búsqueda de archivos y carpetas en el sistema de archivos.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Los eventos se suscriben en esta instancia y persisten entre llamadas a
  /// <see cref="FileSearch"/> y <see cref="FolderSearch"/>.
  /// </para>
  /// <para>
  /// Cada nueva búsqueda cancela la anterior si aún está en ejecución.
  /// Para búsquedas concurrentes independientes, use instancias separadas de <see cref="FSL"/>.
  /// </para>
  /// </remarks>
  public class FSL
  {
    /// <summary>Versión semántica de la biblioteca (p. ej. <c>1.0.6</c>).</summary>
    public static string Version => FSLVersion.Current;

    private readonly ExecuteHandlers handlerOption;
    private readonly string folder;
    private readonly string pattern;
    private readonly CancellationToken cancellationToken;
    private SearcherBase? searcher;

    /// <summary>Se dispara al enumerar unidades de almacenamiento.</summary>
    public event EventHandler<DriveEventArgs>? DrivesFound;

    /// <summary>Se dispara al encontrar archivos que coinciden con el patrón.</summary>
    public event EventHandler<FileEventArgs>? FilesFound;

    /// <summary>Se dispara al encontrar carpetas que coinciden con el patrón.</summary>
    public event EventHandler<FolderEventArgs>? FoldersFound;

    /// <summary>Se dispara cuando la búsqueda es cancelada.</summary>
    public event EventHandler<SearchCanceledEventArgs>? SearchCanceled;

    /// <summary>Se dispara al finalizar la búsqueda (éxito o cancelación).</summary>
    public event EventHandler<SearchCompletedEventArgs>? SearchCompleted;

    /// <summary>Se dispara cuando la búsqueda se pausa.</summary>
    public event EventHandler<SearchPausedEventArgs>? SearchPaused;

    /// <summary>Se dispara cuando la búsqueda se reanuda.</summary>
    public event EventHandler<SearchResumedEventArgs>? SearchResumed;

    /// <summary>
    /// Crea una instancia de búsqueda con cancelación externa deshabilitada.
    /// </summary>
    /// <param name="handlerOption">Define si la búsqueda corre en el hilo actual o en una tarea nueva.</param>
    /// <param name="folder">Carpeta raíz. Use <c>""</c>, <c>"*"</c> o <c>"/"</c> para todas las unidades.</param>
    /// <param name="pattern">Patrón de búsqueda (p. ej. <c>.conf</c>, <c>*.pdf</c>, <c>systemd</c>).</param>
    public FSL(ExecuteHandlers handlerOption, string folder, string pattern)
      : this(handlerOption, folder, pattern, CancellationToken.None)
    {
    }

    /// <summary>
    /// Crea una instancia de búsqueda con <see cref="CancellationToken"/> opcional.
    /// </summary>
    /// <param name="handlerOption">Define si la búsqueda corre en el hilo actual o en una tarea nueva.</param>
    /// <param name="folder">Carpeta raíz. Use <c>""</c>, <c>"*"</c> o <c>"/"</c> para todas las unidades.</param>
    /// <param name="pattern">Patrón de búsqueda (p. ej. <c>.conf</c>, <c>*.pdf</c>, <c>systemd</c>).</param>
    /// <param name="cancellationToken">Token para cancelar la búsqueda desde el consumidor.</param>
    public FSL(ExecuteHandlers handlerOption, string folder, string pattern, CancellationToken cancellationToken)
    {
      this.handlerOption = handlerOption;
      this.folder = folder;
      this.pattern = pattern;
      this.cancellationToken = cancellationToken;
    }

    /// <summary>Inicia una búsqueda recursiva de archivos por patrón.</summary>
    public void FileSearch()
    {
      PrepareSearcher(new FilePatternSearch(handlerOption, folder, pattern, cancellationToken));
      StartSearcher();
    }

    /// <summary>Inicia una búsqueda recursiva de carpetas por patrón de nombre.</summary>
    public void FolderSearch()
    {
      PrepareSearcher(new FolderPatternSearch(handlerOption, folder, pattern, cancellationToken));
      StartSearcher();
    }

    /// <summary>Cancela la búsqueda en curso, si existe.</summary>
    public void CancelSearch()
    {
      searcher?.CancelSearch();
    }

    /// <summary>Pausa la búsqueda en curso, si existe.</summary>
    public void PauseSearch()
    {
      searcher?.PauseSearch();
    }

    /// <summary>Reanuda la búsqueda pausada, si existe.</summary>
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
        Task.Run(() => searcher!.StartSearch(), cancellationToken);
      }
      else
      {
        searcher!.StartSearch();
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

    private void ForwardDrivesFound(object? sender, DriveEventArgs e) => DrivesFound?.Invoke(this, e);
    private void ForwardFilesFound(object? sender, FileEventArgs e) => FilesFound?.Invoke(this, e);
    private void ForwardFoldersFound(object? sender, FolderEventArgs e) => FoldersFound?.Invoke(this, e);
    private void ForwardSearchCanceled(object? sender, SearchCanceledEventArgs e) => SearchCanceled?.Invoke(this, e);
    private void ForwardSearchCompleted(object? sender, SearchCompletedEventArgs e) => SearchCompleted?.Invoke(this, e);
    private void ForwardSearchPaused(object? sender, SearchPausedEventArgs e) => SearchPaused?.Invoke(this, e);
    private void ForwardSearchResumed(object? sender, SearchResumedEventArgs e) => SearchResumed?.Invoke(this, e);
  }
}
