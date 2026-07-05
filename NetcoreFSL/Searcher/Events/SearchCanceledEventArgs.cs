namespace NetcoreFSL.Searcher.Events
{
  /// <summary>Argumentos del evento <see cref="NetcoreFSL.FSL.SearchCanceled"/>.</summary>
  public class SearchCanceledEventArgs : EventArgs
  {
    /// <summary>Indica si la búsqueda fue cancelada.</summary>
    public bool IsCanceled { get; private set; }

    /// <summary>Inicializa los argumentos del evento.</summary>
    /// <param name="isCanceled">Estado de cancelación.</param>
    public SearchCanceledEventArgs(bool isCanceled)
    {
      IsCanceled = isCanceled;
    }
  }
}
