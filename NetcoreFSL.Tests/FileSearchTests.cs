using NetcoreFSL.Tests.Support;
using Xunit;

namespace NetcoreFSL.Tests;

public class FileSearchTests
{
  [Fact]
  public void FileSearch_ShouldFindFilesRecursively()
  {
    string root = SearchTestFixture.CreateFileTree();

    try
    {
      List<string> matches = SearchTestFixture.RunFileSearch(root, ".txt");

      Assert.Equal(3, matches.Count);
      Assert.All(matches, path => Assert.EndsWith(".txt", path));
    }
    finally
    {
      SearchTestFixture.DeleteTree(root);
    }
  }

  [Fact]
  public void FileSearch_DotPatternAndWildcardPattern_ShouldReturnSameResults()
  {
    string root = SearchTestFixture.CreateFileTree();

    try
    {
      List<string> dotMatches = SearchTestFixture.RunFileSearch(root, ".txt");
      List<string> wildcardMatches = SearchTestFixture.RunFileSearch(root, "*.txt");

      Assert.Equal(dotMatches.OrderBy(path => path), wildcardMatches.OrderBy(path => path));
    }
    finally
    {
      SearchTestFixture.DeleteTree(root);
    }
  }
}
