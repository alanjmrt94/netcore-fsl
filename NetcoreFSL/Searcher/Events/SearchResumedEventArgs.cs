namespace NetcoreFSL.Searcher.Events
{
  /// <summary>Argumentos del evento <see cref="NetcoreFSL.FSL.SearchResumed"/>.</summary>
  public class SearchResumedEventArgs : EventArgs
  {
    /// <summary>Indica si la búsqueda fue reanudada.</summary>
    public bool IsResumed { get; private set; }

    /// <summary>Inicializa los argumentos del evento.</summary>
    /// <param name="isResumed">Estado de reanudación.</param>
    public SearchResumedEventArgs(bool isResumed)
    {
      IsResumed = isResumed;
    }
  }
}
