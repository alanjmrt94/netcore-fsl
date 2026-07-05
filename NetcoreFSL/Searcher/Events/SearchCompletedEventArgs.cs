namespace NetcoreFSL.Searcher.Events
{
  /// <summary>Argumentos del evento <see cref="NetcoreFSL.FSL.SearchCompleted"/>.</summary>
  public class SearchCompletedEventArgs : EventArgs
  {
    /// <summary>
    /// Indica si la búsqueda finalizó correctamente.
    /// Es <c>false</c> si fue cancelada o interrumpida.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>Inicializa los argumentos del evento.</summary>
    /// <param name="isCompleted">Estado de finalización exitosa.</param>
    public SearchCompletedEventArgs(bool isCompleted)
    {
      IsCompleted = isCompleted;
    }
  }
}
