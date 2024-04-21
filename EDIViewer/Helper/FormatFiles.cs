using System.IO;
using System.Windows;
using System.Windows.Navigation;

using Microsoft.Win32;

namespace EDIViewer.Helper
{
    class FormatFiles
    {

        /// <summary>
        /// Show Message Box for Messages for User
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static MessageBoxResult ShowMessageBox(string title, string message)
        {
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxResult result;

            result = MessageBox.Show(message, title, button, icon, MessageBoxResult.Yes);

            return result;
        }

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
                ShowMessageBox("Fehler bei Format Dateien", "Folgender Fehler ist beim öffnen der Format Dateien aufgetreten: " + ex.Message);

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
                string formatFileFolderReg = LoadCurrentFormatFolderPathReg();
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
                ShowMessageBox("Einstellungen Fehler", ex.Message);
            }

            return folderPath;
        }

        /// <summary>
        /// Format Folder Path aus der Registry ermitteln
        /// </summary>
        /// <returns></returns>
        private static string LoadCurrentFormatFolderPathReg()
        {
            string folderPath = string.Empty;
         
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software");
                key = key.OpenSubKey("EDI-Viewer");

                folderPath = key.GetValue("FormatsFolder").ToString();

                if (!Directory.Exists(folderPath))
                {
                    folderPath = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ShowMessageBox("Einstellungen Fehler", ex.Message);
            }

            return folderPath;
        }
    }
}
