namespace NetcoreFSL.Searcher.Events
{
  public class DriveEventArgs : EventArgs
  {
    public List<DriveInfo> Drives { get; private set; }

    public DriveEventArgs(List<DriveInfo> drives)
    {
      Drives = drives;
    }
  }
}
