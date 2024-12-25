using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using EDIViewer.Helper;
using EDIViewer.ViewModel;

using Microsoft.Win32;

namespace EDIViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Datei Informationen
        string filePathName = string.Empty;
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

            //Bereinigen der RichTextBox -> Beim erstellen wird ein Absatz erstellt
            fileOriginalView.Document.Blocks.Clear();

            SetFormatCb();
            GetEncodingsCb();
        }

        /// <summary>
        /// Mögliche Encodings in DropDown laden
        /// </summary>
        private void GetEncodingsCb()
        {
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
        /// Befüllen der Auswahl für die Format Typen
        /// </summary>
        private void SetFormatCb()
        {
            cbFileFormat.ItemsSource = null;

            //Aktuelle Formate laden
            Dictionary<string, string> currentFormatFiles = FormatFiles.GetCurrentFormatFiles();

            //Aktuelle Formate in ComboBox laden und nur den Format Typ anzeigen
            cbFileFormat.ItemsSource = currentFormatFiles;
            cbFileFormat.DisplayMemberPath = "Key";
        }

        /// <summary>
        /// Öffnen der Verwaltung Formate und Neuladen der Formate beim schließen der Format Verwaltung
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

            if ((bool)windowStatus)
            {
                SetFormatCb();
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
                SetFormatCb();
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
                filePathName = dialog.FileName;
                File_LoadView();
            }
        }

        /// <summary>
        /// Steuern Maus Zeichen bei eintritt Scroll Viewer für File Drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerFileContent_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Reagieren auf File Drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerFileContent_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                if (files.Length > 0)
                {
                    filePathName = files[0]; 
                    File_LoadView();
                }       
            }
        }
        #endregion

        /// <summary>
        /// Laden der Datei Ansicht
        /// </summary>
        private void File_LoadView()//TODO -> Check File Content -> Text
        {
            filePath = Path.GetDirectoryName(filePathName);
            fileName = Path.GetFileName(filePathName);

            txtFilePathName.Content = filePathName;

            //Ausfall Felder aktivieren
            cbFileFormat.IsEnabled = true;
            cbCharacterSet.IsEnabled = true;

            //Bereinigen der alten Werte
            ClearParsedInformations();

            //Laden der Datei in Fenster
            try
            {
                originalFile = new(Path.Combine(filePath, fileName), fileEncodingSelected);

                fileOriginalView.Document.Blocks.Add(new Paragraph(new Run(originalFile.ReadToEnd())));
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowErrorMessageBox("Datei öffnen", "Fehler: " + ex.Message);
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
        }

        /// <summary>
        /// Reagieren auf File Format Änderung
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFileFormat.SelectedItem != null)
            {
                //Aktuelle Werte FormatName und Pfad
                KeyValuePair<string, string> value = (KeyValuePair<string, string>)cbFileFormat.SelectedItem;
                currentFileFormat = value.Value;
                
                StartParsingFile();
            }
        }

        /// <summary>
        /// Leeren der Informations Felder
        /// </summary>
        private void ClearParsedInformations()
        {
            //Bereinigen Text Box
            fileOriginalView.Document.Blocks.Clear();

            //Formatauswahl leer
            cbFileFormat.SelectedItem = null;

            if (contentInformationViewModel != null)
            {
                contentInformationViewModel = new();
                DataContext = contentInformationViewModel;
            }
        }

        /// <summary>
        /// Reagieren auf Änderung des Zeichensatzes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbCharacterSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EncodingInfo fileEncodingChanged = (EncodingInfo)cbCharacterSet.SelectedItem;
            fileEncodingSelected = Encoding.GetEncoding(fileEncodingChanged.Name);

            //Inhalt der Datei neu laden bei Änderung Zeichensatz
            if (!String.IsNullOrEmpty(new TextRange(fileOriginalView.Document.ContentStart, fileOriginalView.Document.ContentEnd).Text))
            {
                File_LoadView();
            }
        }



        //TODO -> SUCHE VERFEINERN


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
                        TextPointer startPointerMark = text.GetPositionAtOffset(Index + 2);
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
    }
}