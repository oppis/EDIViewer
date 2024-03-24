using System.IO;
using System.Data;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

using EDIViewer.Helper;
using EDIViewer.Parser;
using EDIViewer.Models;
using System.Text;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Windows.Media;


namespace EDIViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Datei Informationen
        static string filePath = string.Empty;
        static string fileName = string.Empty;
        string currentFileFormat = string.Empty;
        Encoding fileEncodingSelected;

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

            //Bereinigen der RichTextBox -> Beim erstellen wird ein Absatz erstellt
            fileOriginalView.Document.Blocks.Clear();

            //Mögliche Encodings in ComboBox laden
            EncodingInfo[] availableEncodings = Encoding.GetEncodings();
            cbCharacterSet.DisplayMemberPath = "DisplayName";
            cbCharacterSet.ItemsSource = availableEncodings;
            cbCharacterSet.SelectedValuePath = "Name";
            cbCharacterSet.SelectedValue = "utf-8";

            //Default Zeichensatz
            fileEncodingSelected = Encoding.Default;

            
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
                originalFile = new(Path.Combine(filePath, fileName), fileEncodingSelected);

                fileOriginalView.Document.Blocks.Clear();
                fileOriginalView.Document.Blocks.Add(new Paragraph(new Run(originalFile.ReadToEnd())));
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
            //Aktuellen Text auf RichTextBox holen
            TextRange textRange = new(fileOriginalView.Document.ContentStart, fileOriginalView.Document.ContentEnd);
            string[] viewLines = textRange.Text.Split(Environment.NewLine);
            
            //Aktuelle Datei Parsen
            ParseFile parseFile = new();
            parseFile.GetFileStructur(currentFileFormat);
            parseFile.ProcessCurrentFile(viewLines);

            //Informationen in Felder schreiben
            ContentInformation contentInformation = parseFile.contentInformation;
            TransferInformation transferInformation = contentInformation.TransferInformation;

            SenderIDValue.Content = transferInformation.SenderID;
            RecipientIDValue.Content = transferInformation.RecipientID;
            DataReferenceValue.Content = transferInformation.DataReference;
            DataTypeValue.Content = transferInformation.DataType;
            
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
        /// <summary>
        /// Reagieren auf Änderung des Zeichensatzes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbCharacterSet_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            EncodingInfo fileEncodingChanged = (EncodingInfo)cbCharacterSet.SelectedItem;
            fileEncodingSelected = Encoding.GetEncoding(fileEncodingChanged.Name);

            //Inhalt der Datei neu laden bei Änderung Zeichensatz
            if (!String.IsNullOrEmpty(new TextRange(fileOriginalView.Document.ContentStart, fileOriginalView.Document.ContentEnd).Text))
            {
                File_LoadView();
            }
        }

        /// <summary>
        /// Suche Starten und Text markieren
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bnt_Search_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(fileOriginalView.Document.ContentStart, fileOriginalView.Document.ContentEnd);

            //clear up highlighted text before starting a new search
            textRange.ClearAllProperties();
            lbl_Status.Content = "";

            //get the richtextbox text
            string textBoxText = textRange.Text;

            //get search text
            string searchText = searchTextBox.Text;

            if (string.IsNullOrWhiteSpace(textBoxText) || string.IsNullOrWhiteSpace(searchText))
            {
                lbl_Status.Content = "Please provide search text or source text to search from";
            }

            else
            {

                //using regex to get the search count
                //this will include search word even it is part of another word
                //say we are searching "hi" in "hi, how are you Mahi?" --> match count will be 2 (hi in 'Mahi' also)

                Regex regex = new Regex(searchText);
                int count_MatchFound = Regex.Matches(textBoxText, regex.ToString()).Count;

                for (TextPointer startPointer = fileOriginalView.Document.ContentStart;
                            startPointer.CompareTo(fileOriginalView.Document.ContentEnd) <= 0;
                                startPointer = startPointer.GetNextContextPosition(LogicalDirection.Forward))
                {
                    //check if end of text
                    if (startPointer.CompareTo(fileOriginalView.Document.ContentEnd) == 0)
                    {
                        break;
                    }

                    //get the adjacent string
                    string parsedString = startPointer.GetTextInRun(LogicalDirection.Forward);

                    //check if the search string present here
                    int indexOfParseString = parsedString.IndexOf(searchText);

                    if (indexOfParseString >= 0) //present
                    {
                        //setting up the pointer here at this matched index
                        startPointer = startPointer.GetPositionAtOffset(indexOfParseString);

                        if (startPointer != null)
                        {
                            //next pointer will be the length of the search string
                            TextPointer nextPointer = startPointer.GetPositionAtOffset(searchText.Length);

                            //create the text range
                            TextRange searchedTextRange = new TextRange(startPointer, nextPointer);

                            //color up 
                            searchedTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));

                            //add other setting property

                        }
                    }
                }

                //update the label text with count
                if (count_MatchFound > 0)
                {
                    lbl_Status.Content = "Anzahl gefunden: " + count_MatchFound;
                }
                else
                {
                    lbl_Status.Content = "Nichts gefunden!";
                }
            }
        }
    }
}