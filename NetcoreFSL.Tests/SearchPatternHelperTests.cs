using NetcoreFSL.Searcher.Helpers;
using Xunit;

namespace NetcoreFSL.Tests;

public class SearchPatternHelperTests
{
  [Theory]
  [InlineData(".txt", "*.txt")]
  [InlineData("*.txt", "*.txt")]
  [InlineData("txt", "*.txt")]
  [InlineData("", "*")]
  [InlineData("data?.log", "data?.log")]
  public void NormalizeFile_ShouldNormalizePatterns(string input, string expected)
  {
    Assert.Equal(expected, SearchPatternHelper.NormalizeFile(input));
  }

  [Theory]
  [InlineData("node_modules", "node_modules")]
  [InlineData("cache*", "cache*")]
  [InlineData("", "*")]
  public void NormalizeFolder_ShouldNormalizePatterns(string input, string expected)
  {
    Assert.Equal(expected, SearchPatternHelper.NormalizeFolder(input));
  }
}
