using System.Collections.Concurrent;
using System.Security;
using NetcoreFSL.Searcher.Enums;
using NetcoreFSL.Searcher.Events;

namespace NetcoreFSL.Searcher.BaseClasses
{
  internal abstract class SearcherBase
  {
    private readonly object eventLock = new();
    private readonly ConcurrentDictionary<string, byte> visitedDirectories = new();
    private readonly SemaphoreSlim concurrencyLimit;
    private readonly CancellationTokenSource searchCancellation;
    private readonly ManualResetEventSlim pauseGate = new(true);
    private volatile bool isPaused;

    protected ExecuteHandlers HandlerOption { get; set; }

    protected string folder;
    protected string pattern;

    protected SearcherBase(
      ExecuteHandlers handlerOption,
      string folder,
      string pattern = "",
      CancellationToken cancellationToken = default)
    {
      HandlerOption = handlerOption;
      this.folder = folder;
      this.pattern = pattern;
      concurrencyLimit = new SemaphoreSlim(Math.Max(1, Environment.ProcessorCount));
      searchCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }

    public event EventHandler<DriveEventArgs>? DrivesFound;
    public event EventHandler<FileEventArgs>? FilesFound;
    public event EventHandler<FolderEventArgs>? FoldersFound;

    public event EventHandler<SearchCanceledEventArgs>? SearchCanceled;
    public event EventHandler<SearchCompletedEventArgs>? SearchCompleted;
    public event EventHandler<SearchPausedEventArgs>? SearchPaused;
    public event EventHandler<SearchResumedEventArgs>? SearchResumed;

    public bool IsPaused => isPaused;

    public bool IsCancellationRequested => searchCancellation.IsCancellationRequested;

    public abstract void StartSearch();

    public void CancelSearch()
    {
      if (!searchCancellation.IsCancellationRequested)
      {
        searchCancellation.Cancel();
      }
    }

    public void PauseSearch()
    {
      if (isPaused)
      {
        return;
      }

      isPaused = true;
      pauseGate.Reset();
      OnSearchPaused(true);
    }

    public void ResumeSearch()
    {
      if (!isPaused)
      {
        return;
      }

      isPaused = false;
      pauseGate.Set();
      OnSearchResumed(true);
    }

    protected virtual void GetDrives()
    {
      List<DriveInfo> drives = DriveInfo.GetDrives()
        .Where(drive => drive.IsReady)
        .ToList();

      if (drives.Count > 0)
      {
        OnDrivesFound(drives);
      }
    }

    protected abstract void ProcessDirectory(string folder);

    protected virtual List<DirectoryInfo> GetFolders(string folder)
    {
      if (!TryEnumerateSubdirectories(folder, out DirectoryInfo[] subdirectories))
      {
        return new List<DirectoryInfo>();
      }

      return subdirectories.Length > 0
        ? subdirectories.ToList()
        : new List<DirectoryInfo>();
    }

    protected CancellationToken SearchToken => searchCancellation.Token;

    protected virtual void OnDrivesFound(List<DriveInfo> drives)
    {
      InvokeEvent(DrivesFound, new DriveEventArgs(drives));
    }

    protected virtual void OnFilesFound(List<FileInfo> files)
    {
      InvokeEvent(FilesFound, new FileEventArgs(files));
    }

    protected virtual void OnFoldersFound(List<DirectoryInfo> folders)
    {
      InvokeEvent(FoldersFound, new FolderEventArgs(folders));
    }

    protected virtual void OnSearchCanceled(bool isCanceled)
    {
      InvokeEvent(SearchCanceled, new SearchCanceledEventArgs(isCanceled));
    }

    protected virtual void OnSearchCompleted(bool isCompleted)
    {
      InvokeEvent(SearchCompleted, new SearchCompletedEventArgs(isCompleted));
    }

    protected virtual void OnSearchPaused(bool isPaused)
    {
      InvokeEvent(SearchPaused, new SearchPausedEventArgs(isPaused));
    }

    protected virtual void OnSearchResumed(bool isResumed)
    {
      InvokeEvent(SearchResumed, new SearchResumedEventArgs(isResumed));
    }

    protected virtual bool ShouldSearchAllDrives()
    {
      return string.IsNullOrWhiteSpace(folder) || folder == "*" || folder == "/";
    }

    protected virtual void RunFSL()
    {
      bool completed = false;
      bool canceled = false;

      try
      {
        visitedDirectories.Clear();
        IEnumerable<string> roots = GetSearchRoots();
        CancellationToken token = searchCancellation.Token;
        ParallelOptions options = new()
        {
          MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount),
          CancellationToken = token
        };

        Parallel.ForEach(roots, options, root => SearchDirectoryRecursive(root));
        completed = !token.IsCancellationRequested;
      }
      catch (OperationCanceledException)
      {
        canceled = true;
      }
      finally
      {
        if (canceled || searchCancellation.IsCancellationRequested)
        {
          OnSearchCanceled(true);
          completed = false;
        }

        OnSearchCompleted(completed);
      }
    }

    protected IEnumerable<string> GetSearchRoots()
    {
      if (ShouldSearchAllDrives())
      {
        GetDrives();

        return DriveInfo.GetDrives()
          .Where(drive => drive.IsReady)
          .Select(drive => drive.RootDirectory.FullName);
      }

      return new[] { Path.GetFullPath(folder) };
    }

    protected bool TryEnumerateSubdirectories(string path, out DirectoryInfo[] subdirectories)
    {
      subdirectories = Array.Empty<DirectoryInfo>();

      try
      {
        subdirectories = new DirectoryInfo(path).GetDirectories();
        return true;
      }
      catch (Exception ex) when (
        ex is PathTooLongException
        or DirectoryNotFoundException
        or SecurityException
        or UnauthorizedAccessException
        or IOException)
      {
        return false;
      }
    }

    private void WaitIfPaused()
    {
      pauseGate.Wait(searchCancellation.Token);
    }

    private void SearchDirectoryRecursive(string directoryPath)
    {
      searchCancellation.Token.ThrowIfCancellationRequested();
      WaitIfPaused();

      concurrencyLimit.Wait(searchCancellation.Token);

      try
      {
        string fullPath = Path.GetFullPath(directoryPath);

        if (!visitedDirectories.TryAdd(fullPath, 0))
        {
          return;
        }

        ProcessDirectory(fullPath);

        if (!TryEnumerateSubdirectories(fullPath, out DirectoryInfo[] subdirectories))
        {
          return;
        }

        ParallelOptions options = new()
        {
          MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount),
          CancellationToken = searchCancellation.Token
        };

        Parallel.ForEach(
          subdirectories,
          options,
          subdirectory => SearchDirectoryRecursive(subdirectory.FullName));
      }
      finally
      {
        concurrencyLimit.Release();
      }
    }

    private void InvokeEvent<TEventArgs>(EventHandler<TEventArgs>? handler, TEventArgs args)
      where TEventArgs : EventArgs
    {
      if (handler == null)
      {
        return;
      }

      lock (eventLock)
      {
        handler.Invoke(this, args);
      }
    }
  }
}
