using System;
using System.IO;
using System.Linq;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Format: FolderSync.exe [source] [target] [logfile] [time in minutes]");
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
            Console.WriteLine(DateTime.Now.AddMinutes(period).ToString());
        }
    }

    static void SyncDirectories(string sourceDir, string destDir, string logfile)
    {
        DirectoryInfo sourceInfo = new DirectoryInfo(sourceDir);
        DirectoryInfo targetInfo = new DirectoryInfo(destDir);

        if (!sourceInfo.Exists)
        {
            throw new DirectoryNotFoundException($"Source Not Found: {sourceInfo.FullName}");
        }

        if (!targetInfo.Exists)
        {
            targetInfo.Create();
            Log($"Created Target Dir: {targetInfo.FullName}", logfile);
        }

        // Copy Files
        foreach (FileInfo sourceFile in sourceInfo.GetFiles())
        {
            string destFilePath = Path.Combine(destDir, sourceFile.Name);
            if (!File.Exists(destFilePath) || sourceFile.LastWriteTime > File.GetLastWriteTime(destFilePath))
            {
                sourceFile.CopyTo(destFilePath, true);
                Log($"Copied/Updated File: {sourceFile.FullName} to {destFilePath}", logfile);
            }
        }

        // Recursive Sync
        foreach (DirectoryInfo sourceSubDir in sourceInfo.GetDirectories())
        {
            string destSubDirPath = Path.Combine(destDir, sourceSubDir.Name);
            SyncDirectories(sourceSubDir.FullName, destSubDirPath, logfile);
        }

        // Remove Non Existent Files
        foreach (FileInfo destFile in targetInfo.GetFiles())
        {
            string sourceFilePath = Path.Combine(sourceDir, destFile.Name);
            if (!File.Exists(sourceFilePath))
            {
                destFile.Delete();
                Log($"Deleted File: {destFile.FullName}", logfile);
            }
        }

        // Remove Non Existent SubDirs
        foreach (DirectoryInfo destSubDir in targetInfo.GetDirectories())
        {
            string sourceSubDirPath = Path.Combine(sourceDir, destSubDir.Name);
            if (!Directory.Exists(sourceSubDirPath))
            {
                destSubDir.Delete(true);
                Log($"Deleted Dir: {destSubDir.FullName}", logfile);
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