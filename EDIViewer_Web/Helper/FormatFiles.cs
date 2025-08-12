using System.IO;
using Newtonsoft.Json;
using EDIViewer.Models;

namespace EDIViewer.Helper
{
    public class FormatFiles
    {
        /// <summary>
        /// Get list of available format files with metadata
        /// </summary>
        public static List<FormatFileInfo> GetFormatFiles()
        {
            var formatFiles = new List<FormatFileInfo>();
            var folderPath = LoadCurrentFormatFolderPath();
            
            try
            {
                if (Directory.Exists(folderPath))
                {
                    var jsonFiles = Directory.GetFiles(folderPath, "*.json");
                    
                    foreach (var file in jsonFiles)
                    {
                        try
                        {
                            var json = File.ReadAllText(file);
                            var format = JsonConvert.DeserializeObject<FileStructur>(json);
                            if (format != null)
                            {
                                formatFiles.Add(new FormatFileInfo
                                {
                                    FileName = Path.GetFileName(file),
                                    FormatName = format.FormatName,
                                    FullPath = file
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading format file {file}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowErrorMessage("Fehler bei Format Dateien", "Folgender Fehler ist beim öffnen der Format Dateien aufgetreten: " + ex.Message);
            }
            
            return formatFiles;
        }
        
        /// <summary>
        /// Determine the currently available format files
        /// </summary>
        public static Dictionary<string, string> GetCurrentFormatFiles()
        {
            Dictionary<string, string> currentFormatFiles = [];

            //Datei Speicherort
            string formatFileLocation = LoadCurrentFormatFolderPath();

            try
            {
                string[] contentDirectory = Directory.GetFiles(formatFileLocation,"*.json");

                foreach (string file in contentDirectory) 
                {
                    currentFormatFiles.Add(Path.GetFileNameWithoutExtension(file),file);
                }
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowErrorMessage("Fehler bei Format Dateien", "Folgender Fehler ist beim öffnen der Format Dateien aufgetreten: " + ex.Message);
            }

            return currentFormatFiles;
        }

        /// <summary>
        /// Determine folder path with fallback
        /// </summary>
        public static string LoadCurrentFormatFolderPath()
        {
            string folderPath = string.Empty;
            
            try
            {
                string formatFileFolderReg = RegistryHelper.GetFormatFilePath();
                string formatFileFolderFail = Path.Combine(Environment.CurrentDirectory, "Formate");

                if (string.IsNullOrEmpty(formatFileFolderReg))
                {
                    //Wenn kein Verzeichnis in Registry
                    folderPath = formatFileFolderReg;
                }
                else
                {
                    folderPath = formatFileFolderReg;
                }
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowErrorMessage("Einstellungen Fehler", ex.Message);
            }

            return folderPath;
        }
    }
    
    public class FormatFileInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FormatName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
    }
}