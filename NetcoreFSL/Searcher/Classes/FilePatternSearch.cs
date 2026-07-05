using System.Security;
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
    }

    protected override List<DirectoryInfo> GetFolders(string folder)
    {
      try
      {
        DirectoryInfo dir = new(folder);
        DirectoryInfo[] dirArr = dir.GetDirectories();

        if (dirArr.Length > 1)
        {
          Console.WriteLine("Listing dirs in " + dir.FullName);
          foreach (DirectoryInfo dirEnum in dirArr)
          {
            Console.WriteLine("->" + dirEnum.Name);
          }
          return new List<DirectoryInfo>(dirArr);
        }
        else
        {
          Console.WriteLine("Listing dirs in " + dir.FullName + "\n->EMPTY<-");
          return new List<DirectoryInfo>();
        }
      }
      catch (PathTooLongException ex)
      {
        Console.WriteLine(ex);
        return new List<DirectoryInfo>();
      }
      catch (DirectoryNotFoundException ex)
      {
        Console.WriteLine(ex);
        return new List<DirectoryInfo>();
      }
      catch (SecurityException ex)
      {
        Console.WriteLine(ex);
        return new List<DirectoryInfo>();
      }
      catch (UnauthorizedAccessException ex)
      {
        Console.WriteLine(ex);
        return new List<DirectoryInfo>();
      }
    }
  }
}
