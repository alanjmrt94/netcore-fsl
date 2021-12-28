namespace NetcoreFSL.Searcher.Events
{
  public class FolderEventArgs : EventArgs
  {
    public List<DirectoryInfo> Folders { get; private set; }

    public FolderEventArgs(List<DirectoryInfo> folders)
    {
      Folders = folders;
    }
  }
}
