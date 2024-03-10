using System.IO;
using System.Data;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

using EDIViewer.Helper;
using EDIViewer.Parser;
using EDIViewer.Models;


namespace EDIViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string filePath = string.Empty;
        static string fileName = string.Empty;
        string currentFileFormat = string.Empty;

        StreamReader originalFile;

        /// <summary>
        /// Start des Fensters
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //Aktuelle Formate laden
            Dictionary<string, string> currentFormatFiles = FormatFiles.GetCurrentFormatFiles();

            //Aktuelle Formate in ComboBox laden und nur den Format Typ anzeigen
            cbFileFormat.ItemsSource = currentFormatFiles;
            cbFileFormat.DisplayMemberPath = "Key";
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
        /// Öffnen der Verwaltung Formate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFormatManagement(object sender, RoutedEventArgs e)
        {
            FormatManagement window = new();
            window.Show();
        }
        #region Load Files
        /// <summary>
        /// Datei Explorer Starten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                FileName = "EDI-Datei",
                DefaultExt = ".txt",
            };

            if (dialog.ShowDialog() == true)
            {
                filePath = Path.GetDirectoryName(dialog.FileName);
                fileName = dialog.SafeFileName;

                txtFilePath.Text = filePath;
                txtFileName.Text = fileName;

                //TODO -> Check File Content -> Text

                File_LoadView();
            }
        }    
        /// <summary>
        /// Reagieren auf File Drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void File_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                //read only the First File                
                string filePathName = (files[0]);

                if (filePathName != null)//TODO -> Check File Content -> Text
                {
                    filePath = Path.GetDirectoryName(filePathName);
                    fileName = Path.GetFileName(filePathName);
                }

                txtFilePath.Text = filePath;
                txtFileName.Text = fileName;

                File_LoadView();
            }
        }
        #endregion
        /// <summary>
        /// Laden der Datei Ansicht
        /// </summary>
        private void File_LoadView()
        {
            cbFileFormat.IsEnabled = true;
            cbCharacterSet.IsEnabled = true;
            
            try
            {
                originalFile = new(Path.Combine(filePath, fileName));

                FileOriginalView.Text = originalFile.ReadToEnd();
            }
            catch (Exception ex)
            {
                ShowMessageBox("Datei öffnen", "Fehler: " + ex.Message);
            }
        }
        /// <summary>
        /// Starten des Parsen der Datei
        /// </summary>
        private void StartParsingFile()
        {
            //Aktuelle Datei Parsen
            ParseFile parseFile = new();
            parseFile.GetFileStructur(currentFileFormat);
            parseFile.ProcessCurrentFile(Path.Combine(filePath, fileName));

            //Informationen in Felder schreiben
            ContentInformation contentInformation = parseFile.contentInformation;
            foreach (TransferInformation test2 in contentInformation.TransferInformation)
            {
                SenderIDValue.Content = test2.SenderID;
                RecipientIDValue.Content = test2.RecipientID;
                DataReferenceValue.Content = test2.DataReference;
                DataTypeValue.Content = test2.DataType;
            }
            //Inhalt des JSON in die Tabelle laden
            DataTable dtRawInformation = new(typeof(RawInformation).Name);

            //Tabellen Format erstellen
            PropertyInfo[] Props2 = typeof(RawInformation).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props2)
            {
                //Setting column names as Property names
                dtRawInformation.Columns.Add(prop.Name);
            }         
            foreach (RawInformation rawInformation in contentInformation.RawInformation)
            {
                var values = new object[Props2.Length];
                for (int i = 0; i < Props2.Length; i++)
                {
                    values[i] = Props2[i].GetValue(rawInformation, null);
                }
                dtRawInformation.Rows.Add(values);
            }
            dgRawInformation.ItemsSource = dtRawInformation.AsDataView();
        }
        /// <summary>
        /// Reagieren auf File Format Änderung
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbFileFormat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //TODO -> Alle Felder in der Ansicht leeren -> Neu Zuordnung der Felder

            //Aktuelle Werte FormatName und Pfad            
            KeyValuePair<string, string> value = (KeyValuePair<string, string>)cbFileFormat.SelectedItem;
            currentFileFormat = value.Value;

            StartParsingFile();
        }
    }
}