using NetcoreFSL.Tests.Support;
using Xunit;

namespace NetcoreFSL.Tests;

public class FolderSearchTests
{
  [Fact]
  public void FolderSearch_ShouldFindMatchingDirectories()
  {
    string root = SearchTestFixture.CreateFileTree();

    try
    {
      List<string> matches = SearchTestFixture.RunFolderSearch(root, "cache-data");

      Assert.Single(matches);
      Assert.EndsWith("cache-data", matches[0]);
    }
    finally
    {
      SearchTestFixture.DeleteTree(root);
    }
  }

  [Fact]
  public void FolderSearch_WithWildcard_ShouldFindMatchingDirectories()
  {
    string root = SearchTestFixture.CreateFileTree();

    try
    {
      List<string> matches = SearchTestFixture.RunFolderSearch(root, "cache*");

      Assert.Single(matches);
      Assert.Contains("cache-data", matches[0]);
    }
    finally
    {
      SearchTestFixture.DeleteTree(root);
    }
  }
}
