namespace NetcoreFSL.Searcher.Events
{
  public class SearchCompletedEventArgs : EventArgs
  {
    public bool IsCompleted { get; private set; }

    public SearchCompletedEventArgs(bool isCompleted)
    {
      IsCompleted = isCompleted;
    }
  }
}
