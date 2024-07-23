using Microsoft.Win32;
using System.IO;

namespace EDIViewer.Helper
{
    class RegistryHelper
    {
        /// <summary>
        /// Aktuelle Format Folder Einstellung abrufen
        /// </summary>
        /// <returns>Format Folder Path</returns>
        public static string GetFormatFilePath()
        {
            string folderPath = string.Empty;

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software");
                key = key.OpenSubKey("EDI-Viewer");

                if (key != null)
                {
                    folderPath = key.GetValue("FormatsFolder").ToString();

                    if (!Directory.Exists(folderPath))
                    {
                        folderPath = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowErrorMessageBox("Einstellungen Fehler", ex.Message);
            }

            return folderPath;
        }

        public static bool SetFormatFilePath(string formatFolderPath)
        {
            bool saveStatus = false;
            
            try
            {
                //Setzen Registry Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                key.CreateSubKey("EDI-Viewer");
                key = key.OpenSubKey("EDI-Viewer", true);
                key.SetValue("FormatsFolder", formatFolderPath);

                saveStatus = true;
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowErrorMessageBox("Einstellungen - Fehler", ex.Message);
                throw;
            }

            return saveStatus;
        }
    }
}