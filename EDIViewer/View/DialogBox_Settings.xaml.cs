using System.IO;
using System.Windows;

using Microsoft.Win32;

namespace EDIViewer
{
    /// <summary>
    /// Interaktionslogik für DialogBox_NewRecordType.xaml
    /// </summary>
    public partial class DialogBox_Settings : Window
    {
        public DialogBox_Settings()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Fenster schließen beim Speichern 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentFormatsFolder();
            this.Close();
        }
        /// <summary>
        /// Fenster schließen ohne Speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
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
        /// Öffnen eines File Dialog auswahl des Datei Pfads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickLoadFileFormatPath(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();

            if (openFolderDialog.ShowDialog() == true)
            {
                string formatFilesPath = openFolderDialog.FolderName;

                formatPath.Text = formatFilesPath;
            }
        }
        private void LoadCurrentFormatFilePath()
        {

        }
        /// <summary>
        /// Speichern des aktuellen Format Ordner in Registry
        /// </summary>
        /// <returns></returns>
        private bool SaveCurrentFormatsFolder()
        {
            //Status zurückgeben ob Erfolgreich gespeichert
            bool saveStatus = false;

            string currentFormatsFolder = formatPath.Text;

            try
            {
                //Setzen Refitry Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                key.CreateSubKey("EDI-Viewer");
                key = key.OpenSubKey("EDI-Viewer", true);

                key.SetValue("FormatsFolder", currentFormatsFolder);

                saveStatus = true;
            }
            catch (Exception ex)
            {
                ShowMessageBox("Einstellungen - Fehler", ex.Message);
                throw;
            }

            return saveStatus;
        }
    }
}