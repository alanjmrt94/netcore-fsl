using NetcoreFSL.Searcher.Helpers;
using NetcoreFSL.Tests.Support;
using Xunit;

namespace NetcoreFSL.Tests;

public class PathHelperTests
{
  [Fact]
  public void GetFullPath_OnNonWindows_ReturnsStandardFullPath()
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    string root = SearchTestFixture.CreateFileTree();

    try
    {
      string full = PathHelper.GetFullPath(root);

      Assert.Equal(Path.GetFullPath(root), full);
      Assert.DoesNotContain(@"\\?\", full);
    }
    finally
    {
      SearchTestFixture.DeleteTree(root);
    }
  }

  [Fact]
  public void GetFullPath_OnWindows_UsesExtendedPrefix()
  {
    if (!OperatingSystem.IsWindows())
    {
      return;
    }

    string temp = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    string full = PathHelper.GetFullPath(temp);

    Assert.StartsWith(@"\\?\", full);
  }

  [Fact]
  public void GetCanonicalKey_OnWindows_IsCaseInsensitive()
  {
    if (!OperatingSystem.IsWindows())
    {
      return;
    }

    string root = SearchTestFixture.CreateFileTree();

    try
    {
      string upper = root.ToUpperInvariant();
      string lower = root.ToLowerInvariant();

      Assert.Equal(PathHelper.GetCanonicalKey(upper), PathHelper.GetCanonicalKey(lower));
    }
    finally
    {
      SearchTestFixture.DeleteTree(root);
    }
  }
}
