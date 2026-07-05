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

    protected ExecuteHandlers HandlerOption { get; set; }

    protected string folder;
    protected string pattern;

    protected SearcherBase(ExecuteHandlers handlerOption, string folder, string pattern = "")
    {
      HandlerOption = handlerOption;
      this.folder = folder;
      this.pattern = pattern;
      concurrencyLimit = new SemaphoreSlim(Math.Max(1, Environment.ProcessorCount));
    }

    public event EventHandler<DriveEventArgs> DrivesFound;
    public event EventHandler<FileEventArgs> FilesFound;
    public event EventHandler<FolderEventArgs> FoldersFound;

    public event EventHandler<SearchCanceledEventArgs> SearchCanceled;
    public event EventHandler<SearchCompletedEventArgs> SearchCompleted;
    public event EventHandler<SearchPausedEventArgs> SearchPaused;
    public event EventHandler<SearchResumedEventArgs> SearchResumed;

    public abstract void StartSearch();

    protected abstract void GetDrives();
    protected abstract void GetFiles(string folder);
    protected abstract List<DirectoryInfo> GetFolders(string folder);

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

      try
      {
        IEnumerable<string> roots = GetSearchRoots();

        Parallel.ForEach(
          roots,
          new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount) },
          root => SearchDirectoryRecursive(root));

        completed = true;
      }
      finally
      {
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

    private void SearchDirectoryRecursive(string directoryPath)
    {
      concurrencyLimit.Wait();

      try
      {
        string fullPath = Path.GetFullPath(directoryPath);

        if (!visitedDirectories.TryAdd(fullPath, 0))
        {
          return;
        }

        GetFiles(fullPath);

        if (!TryEnumerateSubdirectories(fullPath, out DirectoryInfo[] subdirectories))
        {
          return;
        }

        Parallel.ForEach(
          subdirectories,
          new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount) },
          subdirectory => SearchDirectoryRecursive(subdirectory.FullName));
      }
      finally
      {
        concurrencyLimit.Release();
      }
    }

    private void InvokeEvent<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs args)
      where TEventArgs : EventArgs
    {
      EventHandler<TEventArgs> subscribers = handler;

      if (subscribers == null)
      {
        return;
      }

      lock (eventLock)
      {
        subscribers.Invoke(this, args);
      }
    }
  }
}
