using NetcoreFSL.Searcher.Enums;
using NetcoreFSL.Searcher.Events;

namespace NetcoreFSL.Searcher.BaseClasses
{
  internal abstract class SearcherBase
  {
    protected ExecuteHandlers HandlerOption { get; set; }

    protected string folder;
    protected string pattern;

    public SearcherBase(ExecuteHandlers handlerOption, string folder, string pattern = "")
    {
      HandlerOption = handlerOption;
      this.folder = folder;
      this.pattern = pattern;
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
      DrivesFound?.Invoke(this, new DriveEventArgs(drives));
    }

    protected virtual void OnFilesFound(List<FileInfo> files)
    {
      FilesFound?.Invoke(this, new FileEventArgs(files));
    }

    protected virtual void OnFoldersFound(List<DirectoryInfo> folders)
    {
      FoldersFound?.Invoke(this, new FolderEventArgs(folders));
    }

    protected virtual void OnSearchCanceled(bool isCanceled)
    {
      SearchCanceled?.Invoke(this, new SearchCanceledEventArgs(isCanceled));
    }

    protected virtual void OnSearchCompleted(bool isCompleted)
    {
      SearchCompleted?.Invoke(this, new SearchCompletedEventArgs(isCompleted));
    }

    protected virtual void OnSearchPaused(bool isPaused)
    {
      SearchPaused?.Invoke(this, new SearchPausedEventArgs(isPaused));
    }

    protected virtual void OnSearchResumed(bool isResumed)
    {
      SearchResumed?.Invoke(this, new SearchResumedEventArgs(isResumed));
    }

    //TODO: COMPLETE FUNCTION
    protected virtual void RunFSL()
    {
      Console.WriteLine("Called function: RunFSL() in SearcherBase class.");

      List<DirectoryInfo> startDirs = GetFolders(folder);

      startDirs.AsParallel().ForAll((dir) =>
      {
        GetFolders(dir.FullName).AsParallel().ForAll((dir) =>
        {
          GetFiles(dir.FullName);
        });
      });

      //OnSearchCompleted(false);
    }
  }
}
