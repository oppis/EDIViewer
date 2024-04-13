using System.IO;
using System.Windows;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Microsoft.Win32;

using EDIViewer.Helper;
using EDIViewer.Models;
using EDIViewer.ViewModel;

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
        
        //Datei Konvertierung
        Encoding fileEncodingSelected;

        StreamReader originalFile;
        
        //View Variables
        private int SearchStart = -1;
        ContentInformationViewModel contentInformationViewModel;

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
            FormatManagement windowFormatManagement = new()
            {
                Owner = this
            };
            bool? windowStatus = windowFormatManagement.ShowDialog();

            //Prüfen wie Fenster geschlossen wurde
            if ((bool)windowStatus)
            {
                //Neu Laden der Field Definitionen wenn Änderung bei FieldDefinitionenen vorgenommen wurde
                if (cbFileFormat.SelectedIndex != -1)
                {
                    StartParsingFile();
                }
            }
        }
        /// <summary>
        /// Öffnen der Einstellungen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            DialogBox_Settings windowSettings = new()
            {
                Owner = this
            };
            bool? windowStatus = windowSettings.ShowDialog();

            //Aktion wenn Fenster geschlossen wurde
            if ((bool)windowStatus)
            {
                //Mit Ok geschlossen
            }
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

                fileOriginalView.Document.Blocks.Add(new Paragraph(new Run(originalFile.ReadToEnd())));

               //StartParsingFile();
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

            //DataContext -> RawInformations
            contentInformationViewModel = new ContentInformationViewModel(currentFileFormat, viewLines);
            DataContext = contentInformationViewModel;
            test_entity();
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
        private void searchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                //TODO -> nicht aus dem Suchfeld springen
                //TODO -> Liste erstellen um Parameter zum springen zu nutzen
                //TODO -> Alle Markieren und erstes auswählen in einer Funktion packen
                //TODO -> Beim tippen liste erstellen mit enter andere Funktion und Liste durchgehen

                TextRange textRange = new(fileOriginalView.Document.ContentStart, fileOriginalView.Document.ContentEnd);

                //clear up highlighted text before starting a new search
                textRange.ClearAllProperties();

                //get search text
                string searchText = searchTextBox.Text;
           
                int Index = textRange.Text.IndexOf(searchText, SearchStart + 1);
                if (Index != -1)
                {
                    TextPointer text = fileOriginalView.Document.ContentStart;
                    
                    if (Index >= 0) //present
                    {
                        //setting up the pointer here at this matched index
                        TextPointer startPointerMark = text.GetPositionAtOffset(Index + 2 );
                        TextPointer nextPointerMark = startPointerMark.GetPositionAtOffset(searchText.Length);         
                        
                        //Suchtext einfärben
                        TextRange searchedTextRange = new(startPointerMark, nextPointerMark);
                        searchedTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.OrangeRed));

                        //Zum Text springen
                        fileOriginalView.Focus();
                        fileOriginalView.Selection.Select(startPointerMark, nextPointerMark);

                        SearchStart = Index;
                    }
                }
                else
                {
                    MessageBox.Show("Found all the instances of [" + searchText + "]", "End Search");
                    SearchStart = -1;
                }
                txtOriginalMarkText(searchText);
            }
        }
        private void txtOriginalMarkText(string searchText)
        { 
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
                        TextRange searchedTextRange = new(startPointer, nextPointer);

                        //color up 
                        searchedTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
                    }
                }
            }
        }
        private void test_entity()
        {
            int test_no = 0;
            foreach (ObservableCollection<RawInformation> item in contentInformationViewModel.RawInformationEntity)
            {
                DataGrid dataGridEntity = new();

                dataGridEntity.ItemsSource = item;

                TabItem newTabItem = new()
                {
                    Header = test_no,
                    Content = dataGridEntity,
                };
                test.Items.Add(newTabItem);

                test_no++;
            }
        }
    }
}