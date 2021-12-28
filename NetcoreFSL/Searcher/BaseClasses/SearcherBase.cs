#pragma warning disable CS0067
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
    protected abstract void GetFolders(string folder);

    //TODO: COMPLETE FUNCTION
    protected virtual void RunFSL()
    {
      Console.WriteLine("Called function: RunFSL() in SearcherBase class.");
    }
  }
}