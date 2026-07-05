namespace NetcoreFSL.Searcher.Events
{
  /// <summary>Argumentos del evento <see cref="NetcoreFSL.FSL.SearchPaused"/>.</summary>
  public class SearchPausedEventArgs : EventArgs
  {
    /// <summary>Indica si la búsqueda está pausada.</summary>
    public bool IsPaused { get; private set; }

    /// <summary>Inicializa los argumentos del evento.</summary>
    /// <param name="isPaused">Estado de pausa.</param>
    public SearchPausedEventArgs(bool isPaused)
    {
      IsPaused = isPaused;
    }
  }
}
