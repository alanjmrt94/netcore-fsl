using NetcoreFSL;
using NetcoreFSL.Searcher;
using NetcoreFSL.Searcher.Enums;

Fsl fsl = new();
fsl.Name = "Alan";
Console.WriteLine(fsl.TestGreeting);

// Test on Linux:
string folder = "/etc";
string pattern = ".conf";
FileSearch fileSearch = new(ExecuteHandlers.InCurrentTask, folder, pattern);
Console.WriteLine("Running TEST...");
fileSearch.StartSearch();
Console.WriteLine("TEST finished.");