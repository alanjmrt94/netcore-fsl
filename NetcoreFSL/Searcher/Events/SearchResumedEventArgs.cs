namespace NetcoreFSL.Searcher.Events
{
  public class SearchResumedEventArgs : EventArgs
  {
    public bool IsResumed { get; private set; }

    public SearchResumedEventArgs(bool isResumed)
    {
      IsResumed = isResumed;
    }
  }
}
