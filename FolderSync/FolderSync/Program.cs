using System;
using System.IO;
using System.Linq;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: FolderSync.exe <source> <target> <logfile> <time in minutes>");
            return;
        }

        string source = args[0];
        string target = args[1];
        string logfile = args[2];
        int period = Int32.Parse(args[3]);

        while (true)
        {
            try
            {
                SyncDirectories(source, target, logfile);
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}",logfile);
            }

            Thread.Sleep(1000 * 60 * period);
        }
    }

    static void SyncDirectories(string sourceDir, string destDir, string logfile)
    {
        DirectoryInfo sourceInfo = new DirectoryInfo(sourceDir);
        DirectoryInfo targetInfo = new DirectoryInfo(destDir);

        if (!sourceInfo.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceInfo.FullName}");
        }

        if (!targetInfo.Exists)
        {
            targetInfo.Create();
            Log($"Created destination directory: {targetInfo.FullName}", logfile);
        }

        // Copy all files from source to destination
        foreach (FileInfo sourceFile in sourceInfo.GetFiles())
        {
            string destFilePath = Path.Combine(destDir, sourceFile.Name);
            if (!File.Exists(destFilePath) || sourceFile.LastWriteTime > File.GetLastWriteTime(destFilePath))
            {
                sourceFile.CopyTo(destFilePath, true);
                Log($"Copied/Updated file: {sourceFile.FullName} to {destFilePath}", logfile);
            }
        }

        // Recursively sync subdirectories
        foreach (DirectoryInfo sourceSubDir in sourceInfo.GetDirectories())
        {
            string destSubDirPath = Path.Combine(destDir, sourceSubDir.Name);
            SyncDirectories(sourceSubDir.FullName, destSubDirPath, logfile);
        }

        // Remove files from destination that do not exist in source
        foreach (FileInfo destFile in targetInfo.GetFiles())
        {
            string sourceFilePath = Path.Combine(sourceDir, destFile.Name);
            if (!File.Exists(sourceFilePath))
            {
                destFile.Delete();
                Log($"Deleted file: {destFile.FullName}", logfile);
            }
        }

        // Remove subdirectories from destination that do not exist in source
        foreach (DirectoryInfo destSubDir in targetInfo.GetDirectories())
        {
            string sourceSubDirPath = Path.Combine(sourceDir, destSubDir.Name);
            if (!Directory.Exists(sourceSubDirPath))
            {
                destSubDir.Delete(true);
                Log($"Deleted directory: {destSubDir.FullName}", logfile);
            }
        }
    }

    static void Log(string message, string logfile)
    {
        string logMessage = $"{DateTime.Now}: {message}";
        Console.WriteLine(logMessage);
        File.AppendAllText(logfile, logMessage + Environment.NewLine);
    }
}