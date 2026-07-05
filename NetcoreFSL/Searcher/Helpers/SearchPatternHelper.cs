namespace NetcoreFSL.Searcher.Helpers
{
  internal static class SearchPatternHelper
  {
    /// <summary>
    /// Normaliza un patrón de búsqueda para <see cref="DirectoryInfo.GetFiles(string)"/>.
    /// Acepta <c>.pdf</c>, <c>*.pdf</c> y <c>pdf</c> de forma equivalente.
    /// </summary>
    public static string Normalize(string pattern)
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
  }
}
