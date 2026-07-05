using NetcoreFSL;
using NetcoreFSL.Searcher.Enums;

// Uso: dotnet run -- <carpeta> <patron> [file|folder] [sync|async]
// Variables de entorno:
//   FSL_FOLDER, FSL_PATTERN, FSL_MODE (file|folder)
//   FSL_HANDLER (sync|async)
//   FSL_TIMEOUT_MS — cancela la búsqueda tras N milisegundos (opcional)

string folder = args.Length > 0
  ? args[0]
  : Environment.GetEnvironmentVariable("FSL_FOLDER") ?? "/etc";

string pattern = args.Length > 1
  ? args[1]
  : Environment.GetEnvironmentVariable("FSL_PATTERN") ?? ".conf";

string mode = args.Length > 2
  ? args[2]
  : Environment.GetEnvironmentVariable("FSL_MODE") ?? "file";

string handlerArg = args.Length > 3
  ? args[3]
  : Environment.GetEnvironmentVariable("FSL_HANDLER") ?? "sync";

ExecuteHandlers handler = handlerArg.Equals("async", StringComparison.OrdinalIgnoreCase)
  || handlerArg.Equals("InNewTask", StringComparison.OrdinalIgnoreCase)
  ? ExecuteHandlers.InNewTask
  : ExecuteHandlers.InCurrentTask;

using CancellationTokenSource cts = new();
if (int.TryParse(Environment.GetEnvironmentVariable("FSL_TIMEOUT_MS"), out int timeoutMs) && timeoutMs > 0)
{
  cts.CancelAfter(timeoutMs);
  Console.WriteLine($"Timeout enabled: {timeoutMs} ms");
}

using ManualResetEventSlim searchDone = new(false);

FSL fsl = new(handler, folder, pattern, cts.Token);

int matchCount = 0;

fsl.FilesFound += (_, e) =>
{
  matchCount += e.Files.Count;
  foreach (var file in e.Files)
  {
    Console.WriteLine(file.FullName);
  }
};

fsl.SearchCanceled += (_, e) =>
{
  Console.WriteLine($"Search canceled: {e.IsCanceled}");
};

fsl.SearchPaused += (_, e) =>
{
  Console.WriteLine($"Search paused: {e.IsPaused}");
};

fsl.SearchResumed += (_, e) =>
{
  Console.WriteLine($"Search resumed: {e.IsResumed}");
};

fsl.SearchCompleted += (_, e) =>
{
  Console.WriteLine($"Search completed: {e.IsCompleted}, matches: {matchCount}");
  searchDone.Set();
};

Console.WriteLine($"NetcoreFSL v{FSL.Version}");
Console.WriteLine($"Running TEST... folder={folder}, pattern={pattern}, mode={mode}, handler={handler}");

if (mode.Equals("folder", StringComparison.OrdinalIgnoreCase))
{
  fsl.FolderSearch();
}
else
{
  fsl.FileSearch();
}

if (handler == ExecuteHandlers.InNewTask)
{
  searchDone.Wait(CancellationToken.None);
}

Console.WriteLine("TEST finished!");
