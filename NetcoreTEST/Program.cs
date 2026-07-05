using NetcoreFSL;
using NetcoreFSL.Searcher.Enums;

//
// Test on Linux:
string folder = "/etc";
string pattern = ".conf";
//
// Test on Windows:
//string folder = "C:\ProgramData";
//string pattern = ".log";

FSL fsl = new(ExecuteHandlers.InCurrentTask, folder, pattern);

Console.WriteLine("Running TEST...");
fsl.FileSearch();
Console.WriteLine("TEST finished!");