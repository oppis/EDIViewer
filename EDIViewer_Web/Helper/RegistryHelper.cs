using System.IO;

namespace EDIViewer.Helper
{
    public class RegistryHelper
    {
        private static string _formatFilePath = string.Empty;

        /// <summary>
        /// Get current format folder setting (web version uses local storage/configuration)
        /// </summary>
        /// <returns>Format Folder Path</returns>
        public static string GetFormatFilePath()
        {
            // For web version, we'll use a default formats folder or configuration
            if (string.IsNullOrEmpty(_formatFilePath))
            {
                _formatFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "formats");
                
                if (!Directory.Exists(_formatFilePath))
                {
                    Directory.CreateDirectory(_formatFilePath);
                }
            }

            return _formatFilePath;
        }

        public static bool SetFormatFilePath(string formatFolderPath)
        {
            try
            {
                _formatFilePath = formatFolderPath;
                
                if (!Directory.Exists(_formatFilePath))
                {
                    Directory.CreateDirectory(_formatFilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowErrorMessage("Settings Error", ex.Message);
                return false;
            }
        }
    }
}