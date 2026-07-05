namespace NetcoreFSL.Searcher.Events
{
  /// <summary>Argumentos del evento <see cref="NetcoreFSL.FSL.DrivesFound"/>.</summary>
  public class DriveEventArgs : EventArgs
  {
    /// <summary>Unidades de almacenamiento encontradas.</summary>
    public List<DriveInfo> Drives { get; private set; }

    /// <summary>Inicializa los argumentos del evento.</summary>
    /// <param name="drives">Lista de unidades listas para búsqueda.</param>
    public DriveEventArgs(List<DriveInfo> drives)
    {
      Drives = drives;
    }
  }
}
