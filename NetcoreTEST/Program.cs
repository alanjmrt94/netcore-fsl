using NetcoreFSL;
using NetcoreFSL.Searcher.Enums;

// Uso: dotnet run -- <carpeta> <patron> [file|folder]
// Variables de entorno: FSL_FOLDER, FSL_PATTERN, FSL_MODE (file|folder)

string folder = args.Length > 0
  ? args[0]
  : Environment.GetEnvironmentVariable("FSL_FOLDER") ?? "/etc";

string pattern = args.Length > 1
  ? args[1]
  : Environment.GetEnvironmentVariable("FSL_PATTERN") ?? ".conf";

string mode = args.Length > 2
  ? args[2]
  : Environment.GetEnvironmentVariable("FSL_MODE") ?? "file";

FSL fsl = new(ExecuteHandlers.InCurrentTask, folder, pattern);

Console.WriteLine($"NetcoreFSL v{FSL.Version}");
Console.WriteLine($"Running TEST... folder={folder}, pattern={pattern}, mode={mode}");

if (mode.Equals("folder", StringComparison.OrdinalIgnoreCase))
{
  fsl.FolderSearch();
}
else
{
  fsl.FileSearch();
}

Console.WriteLine("TEST finished!");
