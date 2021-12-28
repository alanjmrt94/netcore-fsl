namespace NetcoreFSL.Searcher.Events
{
  public class FileEventArgs : EventArgs
  {
    public List<FileInfo> Files { get; private set; }

    public FileEventArgs(List<FileInfo> files)
    {
      Files = files;
    }
  }
}
