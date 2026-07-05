namespace NetcoreFSL.Searcher.Events
{
  /// <summary>Argumentos del evento <see cref="NetcoreFSL.FSL.FilesFound"/>.</summary>
  public class FileEventArgs : EventArgs
  {
    /// <summary>Archivos que coinciden con el patrón en el directorio visitado.</summary>
    public List<FileInfo> Files { get; private set; }

    /// <summary>Inicializa los argumentos del evento.</summary>
    /// <param name="files">Archivos encontrados en el lote actual.</param>
    public FileEventArgs(List<FileInfo> files)
    {
      Files = files;
    }
  }
}
