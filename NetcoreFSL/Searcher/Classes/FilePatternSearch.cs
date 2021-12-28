using NetcoreFSL.Searcher.BaseClasses;
using NetcoreFSL.Searcher.Enums;

namespace NetcoreFSL.Searcher.Classes
{
  internal class FilePatternSearch : SearcherBase
  {
    public FilePatternSearch(ExecuteHandlers handlerOption, string folder, string pattern = "") : base(handlerOption, folder, pattern)
    {
    }

    public override void StartSearch()
    {
      RunFSL();
    }

    protected override void GetDrives()
    {
      throw new NotImplementedException();
    }

    protected override void GetFiles(string folder)
    {
      throw new NotImplementedException();
    }

    protected override void GetFolders(string folder)
    {
      throw new NotImplementedException();
    }
  }
}