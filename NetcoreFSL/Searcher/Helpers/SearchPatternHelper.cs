namespace NetcoreFSL.Searcher.Helpers
{
  internal static class SearchPatternHelper
  {
    /// <summary>
    /// Normaliza un patrón de búsqueda para <see cref="DirectoryInfo.GetFiles(string)"/>.
    /// Acepta <c>.pdf</c>, <c>*.pdf</c> y <c>pdf</c> de forma equivalente.
    /// </summary>
    public static string NormalizeFile(string pattern)
    {
      if (string.IsNullOrWhiteSpace(pattern))
      {
        return "*";
      }

      pattern = pattern.Trim();

      if (pattern.Contains('*') || pattern.Contains('?'))
      {
        return pattern;
      }

      if (pattern.StartsWith('.'))
      {
        return "*" + pattern;
      }

      return "*." + pattern;
    }

    /// <summary>
    /// Normaliza un patrón de búsqueda para <see cref="DirectoryInfo.GetDirectories(string)"/>.
    /// Acepta nombres literales (<c>node_modules</c>) o comodines (<c>cache*</c>).
    /// </summary>
    public static string NormalizeFolder(string pattern)
    {
      if (string.IsNullOrWhiteSpace(pattern))
      {
        return "*";
      }

      pattern = pattern.Trim();

      if (pattern.Contains('*') || pattern.Contains('?'))
      {
        return pattern;
      }

      return pattern;
    }
  }
}
