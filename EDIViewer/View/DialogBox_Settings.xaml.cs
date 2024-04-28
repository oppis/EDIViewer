using System.IO;
using System.Windows;

using EDIViewer.Helper;

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

            //Laden der Aktuellen Einstellung für den Formats Pfad
            LoadCurrentFormatFilePath();
        }
        
        /// <summary>
        /// Fenster schließen beim Speichern 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentFormatsFolder();

            DialogResult = true;

            this.Close();
        }
        
        // <summary>
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

        /// <summary>
        /// Aktuellen Wert aus der Datenbank holen
        /// </summary>
        private void LoadCurrentFormatFilePath()
        {
            formatPath.Text = RegistryHelper.GetFormatFilePath();
        }

        /// <summary>
        /// Speichern des aktuellen Format Ordner in Registry
        /// </summary>
        /// <returns></returns>
        private bool SaveCurrentFormatsFolder()
        {
            //Status zurückgeben ob Erfolgreich gespeichert
            bool saveStatus = RegistryHelper.SetFormatFilePath(formatPath.Text);

            return saveStatus;
        }
    }
}