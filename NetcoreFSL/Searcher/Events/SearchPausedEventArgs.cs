namespace NetcoreFSL.Searcher.Events
{
  public class SearchPausedEventArgs : EventArgs
  {
    public bool IsPaused { get; private set; }

    public SearchPausedEventArgs(bool isPaused)
    {
      IsPaused = isPaused;
    }
  }
}
