using System;
using System.IO;
using System.Linq;
using Thundershock.Content;
using Thundershock.Debugging;

namespace Thundershock.ThunderPak
{
    public class ThunderPakApp : CommandLineApp
    {
        private string _src;
        private string _dest;
        
        protected override void Main()
        {
            _src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Program.Source);
            _dest = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Program.Destination);

            Logger.Log($"Source Path: {_src}", LogLevel.Message);
            Logger.Log($"Destination Path: {_dest}", LogLevel.Message);
            
            if (File.Exists(_src))
            {
                Logger.Log("Source path is a file - operating in Extract mode.");
                ExtractPak();
            }
            else if (Directory.Exists(_src))
            {
                Logger.Log("Source path is a directory - operating in Package mode.");
                MakePak();
            }
            else
            {
                Logger.Log("ERROR: Source path not found!", LogLevel.Error);
            }
        }

        private void ExtractPak()
        {
            if (File.Exists(_dest))
            {
                Logger.Log("Destination is a file. Quitting.", LogLevel.Error);
                return;
            }

            if (!Directory.Exists(_dest))
            {
                Logger.Log("Destination directory does not exist. Quitting.", LogLevel.Error);
                return;
            }

            if (Directory.EnumerateFileSystemEntries(_dest).Any())
            {
                Logger.Log("Destination directory IS NOT EMPTY. Refusing to extract PakFile contents. Quitting.",
                    LogLevel.Error);
                return;
            }
            
            Logger.Log("Reading PakFile data...");
            var pakFile = PakUtils.OpenPak(_src);
            
            Logger.Log("Extracting Pak data to disk...");

            WriteDirectoryData(pakFile, pakFile.RootDirectory, _dest);
        }

        private void WriteDirectoryData(PakFile pakFile, PakDirectory entry, string host)
        {
            if (entry.DirectoryType == PakDirectoryType.File)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    Logger.Log(
                        $"WARNING: Detected a file entry with an empty name. That shouldn't be there, ignoring it.",
                        LogLevel.Warning);
                    return;
                }
                
                var filePath = Path.Combine(host, entry.FileName);
                Logger.Log("Writing file: " + filePath);

                using var fileStream = File.OpenWrite(filePath);
                using var pakMem = pakFile.LoadData(entry);

                pakMem.CopyTo(fileStream);
            }
            else
            {
                var fPath = host;
                
                if (entry.DirectoryType == PakDirectoryType.Folder)
                {
                    var name = entry.Name;
                    fPath = Path.Combine(host, name);
                    Logger.Log("Creating directory: " + fPath);
                    Directory.CreateDirectory(fPath);
                }

                foreach (var child in entry.Children)
                {
                    WriteDirectoryData(pakFile, child, fPath);
                }
            }
        }

        private void MakePak()
        {
            if (File.Exists(_dest))
            {
                Logger.Log(
                    "Destination file exists. Refusing to overwrite a file. Please delete the file first before invoking ThunderPak. Quitting.",
                    LogLevel.Error);
                return;
            }

            if (Directory.Exists(_dest))
            {
                Logger.Log("Destination path is a directory. Quitting.", LogLevel.Error);
                return;
            }

            Logger.Log("Starting PAK File Build...", LogLevel.Message);

            PakUtils.MakePak(_src, _dest);
        }
    }
}