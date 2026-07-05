using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Enums;

namespace NetcoreFSL.Searcher.Classes
{
  internal class FolderPatternSearch : SearcherBase
  {
    public FolderPatternSearch(ExecuteHandlers handlerOption, string folder, string pattern = "") : base(handlerOption, folder, pattern)
    {
    }

    public override void StartSearch()
    {
      throw new NotImplementedException();
    }

    protected override void GetDrives()
    {
      throw new NotImplementedException();
    }

    protected override void GetFiles(string folder)
    {
      throw new NotImplementedException();
    }

    protected override List<DirectoryInfo> GetFolders(string folder)
    {
      throw new NotImplementedException();
    }
  }
}
