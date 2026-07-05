using System.Diagnostics;
using NetcoreFSL;
using NetcoreFSL.Searcher.Enums;
using NetcoreFSL.Tests.Support;
using Xunit;

namespace NetcoreFSL.Tests;

public class SearchLifecycleTests
{
  [Fact]
  public void FileSearch_WithCanceledToken_ShouldRaiseSearchCanceled()
  {
    string root = SearchTestFixture.CreateFileTree();

    try
    {
      using CancellationTokenSource cts = new();
      cts.Cancel();

      bool canceled = false;
      bool completed = false;

      FSL fsl = new(ExecuteHandlers.InCurrentTask, root, ".txt", cts.Token);
      fsl.SearchCanceled += (_, e) => canceled = e.IsCanceled;
      fsl.SearchCompleted += (_, e) => completed = e.IsCompleted;
      fsl.FileSearch();

      Assert.True(canceled);
      Assert.False(completed);
    }
    finally
    {
      SearchTestFixture.DeleteTree(root);
    }
  }

  [Fact]
  public void FileSearch_WithInNewTask_ShouldReturnBeforeCompletion()
  {
    string root = SearchTestFixture.CreateFileTree();

    try
    {
      using ManualResetEventSlim completed = new(false);
      bool searchCompleted = false;

      FSL fsl = new(ExecuteHandlers.InNewTask, root, ".txt");
      fsl.SearchCompleted += (_, e) =>
      {
        searchCompleted = e.IsCompleted;
        completed.Set();
      };

      Stopwatch stopwatch = Stopwatch.StartNew();
      fsl.FileSearch();
      stopwatch.Stop();

      Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1));
      Assert.True(completed.Wait(TimeSpan.FromSeconds(10)));
      Assert.True(searchCompleted);
    }
    finally
    {
      SearchTestFixture.DeleteTree(root);
    }
  }

  [Fact]
  public void FileSearch_WithRestrictedDirectory_ShouldContinueWithoutThrowing()
  {
    if (!OperatingSystem.IsLinux())
    {
      return;
    }

    string root = SearchTestFixture.CreateFileTree();
    string restricted = Path.Combine(root, "restricted");

    try
    {
      Directory.CreateDirectory(restricted);
      File.WriteAllText(Path.Combine(restricted, "hidden.txt"), "secret");
      File.SetUnixFileMode(restricted, UnixFileMode.None);

      Exception? thrown = Record.Exception(() => SearchTestFixture.RunFileSearch(root, ".txt"));

      Assert.Null(thrown);
    }
    finally
    {
      if (Directory.Exists(restricted))
      {
        File.SetUnixFileMode(restricted, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
      }

      SearchTestFixture.DeleteTree(root);
    }
  }
}
