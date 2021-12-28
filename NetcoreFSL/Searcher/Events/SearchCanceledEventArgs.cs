namespace NetcoreFSL.Searcher.Events
{
  public class SearchCanceledEventArgs : EventArgs
  {
    public bool IsCanceled { get; private set; }

    public SearchCanceledEventArgs(bool isCanceled)
    {
      IsCanceled = isCanceled;
    }
  }
}
