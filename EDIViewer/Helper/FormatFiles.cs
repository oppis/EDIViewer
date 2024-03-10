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

            //Datei Speicher Ort
            string formatFileLocation = Path.Combine(Environment.CurrentDirectory, "Formate");

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
                string messageBoxText = "Folgender Fehler ist beim öffnen der Format Dateien aufgetreten: " + ex.Message;
                string caption = "Fehler bei Format Dateien";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

                throw;
            }

            return currentFormatFiles;
        }
    }
}
