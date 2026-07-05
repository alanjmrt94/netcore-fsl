namespace NetcoreFSL.Searcher.Events
{
  /// <summary>Argumentos del evento <see cref="NetcoreFSL.FSL.FoldersFound"/>.</summary>
  public class FolderEventArgs : EventArgs
  {
    /// <summary>Carpetas que coinciden con el patrón en el directorio visitado.</summary>
    public List<DirectoryInfo> Folders { get; private set; }

    /// <summary>Inicializa los argumentos del evento.</summary>
    /// <param name="folders">Carpetas encontradas en el lote actual.</param>
    public FolderEventArgs(List<DirectoryInfo> folders)
    {
      Folders = folders;
    }
  }
}
