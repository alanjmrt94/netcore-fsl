namespace NetcoreFSL.Searcher.Enums
{
  /// <summary>
  /// Define en qué contexto de ejecución se inicia la búsqueda.
  /// </summary>
  public enum ExecuteHandlers
  {
    /// <summary>
    /// La búsqueda se ejecuta en el hilo que invoca <c>FileSearch()</c> o <c>FolderSearch()</c>.
    /// </summary>
    InCurrentTask = 0,

    /// <summary>
    /// La búsqueda se ejecuta en una tarea en segundo plano (<c>Task.Run</c>) y el método retorna de inmediato.
    /// </summary>
    InNewTask = 1
  }
}
