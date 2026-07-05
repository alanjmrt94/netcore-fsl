using System.Reflection;

namespace NetcoreFSL
{
  /// <summary>
  /// Información de versión de la biblioteca NetcoreFSL.
  /// Los valores se generan desde <c>NetcoreFSL.csproj</c> al compilar.
  /// </summary>
  public static class FSLVersion
  {
    /// <summary>Nombre del producto.</summary>
    public const string Product = "NetcoreFSL";

    /// <summary>Alias público del producto (NetCore FastSearchLibrary).</summary>
    public const string ProductAlias = "NetCore FastSearchLibrary";

    /// <summary>Versión semántica actual (p. ej. <c>1.0.6</c>).</summary>
    public static string Current =>
      Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
      ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
      ?? "0.0.0";

    /// <summary>Versión del ensamblado (p. ej. <c>1.0.6.0</c>).</summary>
    public static System.Version AssemblyVersion =>
      Assembly.GetExecutingAssembly().GetName().Version ?? new System.Version(0, 0, 0, 0);
  }
}
