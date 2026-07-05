using NetcoreFSL;
using NetcoreFSL.Searcher.Enums;

namespace NetcoreFSL.Tests.Support;

internal static class SearchTestFixture
{
  public static string CreateFileTree()
  {
    string root = Path.Combine(Path.GetTempPath(), "netcore-fsl-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(Path.Combine(root, "alpha", "nested"));
    Directory.CreateDirectory(Path.Combine(root, "beta"));
    Directory.CreateDirectory(Path.Combine(root, "cache-data"));

    File.WriteAllText(Path.Combine(root, "alpha", "match.txt"), "a");
    File.WriteAllText(Path.Combine(root, "alpha", "nested", "match.txt"), "b");
    File.WriteAllText(Path.Combine(root, "beta", "match.txt"), "c");
    File.WriteAllText(Path.Combine(root, "skip.log"), "d");

    return root;
  }

  public static void DeleteTree(string root)
  {
    if (Directory.Exists(root))
    {
      Directory.Delete(root, recursive: true);
    }
  }

  public static List<string> RunFileSearch(
    string root,
    string pattern,
    ExecuteHandlers handler = ExecuteHandlers.InCurrentTask,
    CancellationToken cancellationToken = default)
  {
    List<string> matches = new();
    using ManualResetEventSlim completed = new(false);

    FSL fsl = new(handler, root, pattern, cancellationToken);
    fsl.FilesFound += (_, e) => matches.AddRange(e.Files.Select(file => file.FullName));
    fsl.SearchCompleted += (_, _) => completed.Set();
    fsl.FileSearch();

    if (handler == ExecuteHandlers.InNewTask && !completed.Wait(TimeSpan.FromSeconds(10)))
    {
      throw new TimeoutException("La búsqueda no finalizó a tiempo.");
    }

    return matches;
  }

  public static List<string> RunFolderSearch(string root, string pattern)
  {
    List<string> matches = new();
    FSL fsl = new(ExecuteHandlers.InCurrentTask, root, pattern);
    fsl.FoldersFound += (_, e) => matches.AddRange(e.Folders.Select(folder => folder.FullName));
    fsl.FolderSearch();
    return matches;
  }
}
