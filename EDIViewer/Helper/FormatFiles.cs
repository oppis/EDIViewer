using System.IO;
using System.Windows;

namespace EDIViewer.Helper
{
    class FormatFiles
    {
        /// <summary>
        /// Ermitteln der aktuell Vefügbaren Format Datien
        /// </summary>
        public static Dictionary<string, string> GetCurrentFormatFiles()
        {
            Dictionary<string, string> currentFormatFiles = [];

            //Datei Speicherort
            string formatFileLocation = LoadCurrentFormatFolderPath();

            try
            {
                string[] contentDirectory = Directory.GetFiles(formatFileLocation);

                foreach (string file in contentDirectory) 
                {
                    currentFormatFiles.Add(Path.GetFileNameWithoutExtension(file),file);
                }
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowMessageBox("Fehler bei Format Dateien", "Folgender Fehler ist beim öffnen der Format Dateien aufgetreten: " + ex.Message);

                throw;
            }

            return currentFormatFiles;
        }

        /// <summary>
        /// Ordner Pfad mit Fallback ermitteln
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
                    folderPath = formatFileFolderFail;
                }
                else
                {
                    folderPath = formatFileFolderReg;
                }
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowMessageBox("Einstellungen Fehler", ex.Message);
            }

            return folderPath;
        }
    }
}