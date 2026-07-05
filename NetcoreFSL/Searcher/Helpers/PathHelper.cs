namespace NetcoreFSL.Searcher.Helpers
{
  internal static class PathHelper
  {
    /// <summary>
    /// Obtiene la ruta absoluta lista para operaciones de E/S.
    /// En Windows usa el prefijo <c>\\?\</c> para soportar rutas largas.
    /// </summary>
    public static string GetFullPath(string path)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        return path;
      }

      string fullPath = Path.GetFullPath(StripExtendedPrefix(path));

      if (!OperatingSystem.IsWindows())
      {
        return fullPath;
      }

      return ToExtendedWindowsPath(fullPath);
    }

    /// <summary>
    /// Clave canónica para detectar directorios ya visitados.
    /// </summary>
    public static string GetCanonicalKey(string path)
    {
      string fullPath = Path.GetFullPath(StripExtendedPrefix(path));

      return OperatingSystem.IsWindows()
        ? fullPath.ToUpperInvariant()
        : fullPath;
    }

    private static string ToExtendedWindowsPath(string fullPath)
    {
      if (fullPath.StartsWith(@"\\?\", StringComparison.Ordinal))
      {
        return fullPath;
      }

      if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
      {
        return @"\\?\UNC\" + fullPath[2..];
      }

      return @"\\?\" + fullPath;
    }

    private static string StripExtendedPrefix(string path)
    {
      if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
      {
        return @"\\" + path[7..];
      }

      if (path.StartsWith(@"\\?\", StringComparison.Ordinal))
      {
        return path[4..];
      }

      return path;
    }
  }
}
