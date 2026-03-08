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

using PdfSharp.Drawing;
using PdfSharp.Pdf;

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

        /// <summary>
        /// PDF speichern: EDI Datei Inhalt und geparste Beschreibung als PDF exportieren
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavePdf_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new(fileOriginalView.Document.ContentStart, fileOriginalView.Document.ContentEnd);
            string fileContent = textRange.Text;

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                UserMessageHelper.ShowErrorMessageBox("PDF speichern", "Es ist keine Datei geladen.");
                return;
            }

            SaveFileDialog saveDialog = new()
            {
                Title = "PDF speichern",
                Filter = "PDF Datei (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = string.IsNullOrEmpty(fileName) ? "EDI-Export" : Path.GetFileNameWithoutExtension(fileName)
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                using PdfDocument pdfDocument = new();
                pdfDocument.Info.Title = string.IsNullOrEmpty(fileName) ? "EDI-Export" : fileName;
                pdfDocument.Info.Author = "EDI-Viewer";

                XFont fontTitle = new("Arial", 14, XFontStyleEx.Bold);
                XFont fontHeading = new("Arial", 11, XFontStyleEx.Bold);
                XFont fontNormal = new("Arial", 9, XFontStyleEx.Regular);
                XFont fontMono = new("Courier New", 8, XFontStyleEx.Regular);

                const double margin = 40;
                const double pageWidth = 595;   // A4
                const double pageHeight = 842;  // A4
                const double usableWidth = pageWidth - 2 * margin;
                const double lineHeight = 14;
                // Estimated average character width in points for Courier New 8pt
                const double monoCharWidth = 5.0;

                double y = margin;

                PdfPage page = pdfDocument.AddPage();
                page.Width = XUnit.FromPoint(pageWidth);
                page.Height = XUnit.FromPoint(pageHeight);
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Hilfsfunktion: neue Seite beginnen wenn nötig
                void CheckNewPage()
                {
                    if (y > pageHeight - margin - lineHeight)
                    {
                        gfx.Dispose();
                        page = pdfDocument.AddPage();
                        page.Width = XUnit.FromPoint(pageWidth);
                        page.Height = XUnit.FromPoint(pageHeight);
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;
                    }
                }

                // Hilfsfunktion: Zeile schreiben
                void WriteLine(string text, XFont font, double indent = 0)
                {
                    CheckNewPage();
                    gfx.DrawString(text, font, XBrushes.Black, new XRect(margin + indent, y, usableWidth - indent, lineHeight), XStringFormats.TopLeft);
                    y += lineHeight;
                }

                // Hilfsfunktion: Trennlinie zeichnen
                void DrawSeparator()
                {
                    CheckNewPage();
                    gfx.DrawLine(XPens.Gray, margin, y, margin + usableWidth, y);
                    y += 6;
                }

                // Titel
                gfx.DrawString("EDI-Viewer Export", fontTitle, XBrushes.Black, new XRect(margin, y, usableWidth, 20), XStringFormats.TopLeft);
                y += 22;
                if (!string.IsNullOrEmpty(fileName))
                {
                    gfx.DrawString("Datei: " + filePathName, fontNormal, XBrushes.DarkGray, new XRect(margin, y, usableWidth, lineHeight), XStringFormats.TopLeft);
                    y += lineHeight;
                }
                gfx.DrawString("Datum: " + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"), fontNormal, XBrushes.DarkGray, new XRect(margin, y, usableWidth, lineHeight), XStringFormats.TopLeft);
                y += lineHeight + 4;
                DrawSeparator();

                // Übertragungsinformationen
                if (contentInformationViewModel != null)
                {
                    var transfer = contentInformationViewModel.TransferInformation;
                    if (transfer != null && (!string.IsNullOrEmpty(transfer.DataType) || !string.IsNullOrEmpty(transfer.SenderID)))
                    {
                        WriteLine("Übertragung", fontHeading);
                        y += 2;
                        if (!string.IsNullOrEmpty(transfer.DataType))     WriteLine("Datenart:       " + transfer.DataType, fontNormal, 10);
                        if (!string.IsNullOrEmpty(transfer.DateTime))     WriteLine("Datum:          " + transfer.DateTime, fontNormal, 10);
                        if (!string.IsNullOrEmpty(transfer.DataReference)) WriteLine("Dateireferenz:  " + transfer.DataReference, fontNormal, 10);
                        if (!string.IsNullOrEmpty(transfer.SenderID))     WriteLine("Versender ID:   " + transfer.SenderID, fontNormal, 10);
                        if (!string.IsNullOrEmpty(transfer.RecipientID))  WriteLine("Empfänger ID:   " + transfer.RecipientID, fontNormal, 10);
                        y += 4;
                        DrawSeparator();
                    }

                    // Auftragsinformationen
                    var orders = contentInformationViewModel.OrderInformations;
                    if (orders != null && orders.Count > 0)
                    {
                        WriteLine("Aufträge", fontHeading);
                        y += 2;
                        foreach (var order in orders)
                        {
                            if (!string.IsNullOrEmpty(order.IdOrder))            WriteLine("Auftrags ID:     " + order.IdOrder, fontNormal, 10);
                            if (!string.IsNullOrEmpty(order.Reference))          WriteLine("Referenz:        " + order.Reference, fontNormal, 10);
                            if (!string.IsNullOrEmpty(order.DateTimeLoadDat))    WriteLine("Beladedatum:     " + order.DateTimeLoadDat, fontNormal, 10);
                            if (!string.IsNullOrEmpty(order.DateTimeUnloadDat))  WriteLine("Entladedatum:    " + order.DateTimeUnloadDat, fontNormal, 10);
                            y += 2;
                        }
                        y += 2;
                        DrawSeparator();
                    }

                    // Positionsinformationen
                    var positions = contentInformationViewModel.PositionInformations;
                    if (positions != null && positions.Count > 0)
                    {
                        WriteLine("Positionen", fontHeading);
                        y += 2;
                        foreach (var pos in positions)
                        {
                            if (!string.IsNullOrEmpty(pos.IdOrder))    WriteLine("Order ID:     " + pos.IdOrder, fontNormal, 10);
                            if (pos.IdPosition != 0)                   WriteLine("Positions ID: " + pos.IdPosition, fontNormal, 10);
                            if (!string.IsNullOrEmpty(pos.SSCC))       WriteLine("NVE:          " + pos.SSCC, fontNormal, 10);
                            if (pos.PackageCount != 0)                 WriteLine("Anzahl:       " + pos.PackageCount, fontNormal, 10);
                            if (!string.IsNullOrEmpty(pos.PackagingUnit)) WriteLine("Einheit:      " + pos.PackagingUnit, fontNormal, 10);
                            if (pos.Height != 0)                       WriteLine("Höhe:         " + pos.Height, fontNormal, 10);
                            if (pos.Width != 0)                        WriteLine("Breite:       " + pos.Width, fontNormal, 10);
                            if (pos.Length != 0)                       WriteLine("Länge:        " + pos.Length, fontNormal, 10);
                            y += 2;
                        }
                        y += 2;
                        DrawSeparator();
                    }

                    // Statusinformationen
                    var statuses = contentInformationViewModel.StatusInformations;
                    if (statuses != null && statuses.Count > 0)
                    {
                        WriteLine("Status", fontHeading);
                        y += 2;
                        foreach (var status in statuses)
                        {
                            if (status.OrderNo != 0)                  WriteLine("Auftrags Nr:  " + status.OrderNo, fontNormal, 10);
                            if (status.Nve != 0)                      WriteLine("NVE:          " + status.Nve, fontNormal, 10);
                            if (status.Code != 0)                     WriteLine("Status Code:  " + status.Code, fontNormal, 10);
                            if (!string.IsNullOrEmpty(status.Date))   WriteLine("Datum:        " + status.Date, fontNormal, 10);
                            if (!string.IsNullOrEmpty(status.Time))   WriteLine("Zeit:         " + status.Time, fontNormal, 10);
                            if (!string.IsNullOrEmpty(status.Notes))  WriteLine("Hinweis:      " + status.Notes, fontNormal, 10);
                            y += 2;
                        }
                        y += 2;
                        DrawSeparator();
                    }
                }

                // EDI Rohinhalt
                WriteLine("EDI Datei Inhalt", fontHeading);
                y += 2;
                foreach (string line in fileContent.Split('\n'))
                {
                    string trimmed = line.TrimEnd('\r');
                    // Lange Zeilen umbrechen
                    int maxChars = (int)(usableWidth / monoCharWidth);
                    if (trimmed.Length <= maxChars)
                    {
                        WriteLine(trimmed, fontMono);
                    }
                    else
                    {
                        for (int i = 0; i < trimmed.Length; i += maxChars)
                        {
                            WriteLine(trimmed.Substring(i, Math.Min(maxChars, trimmed.Length - i)), fontMono);
                        }
                    }
                }

                gfx.Dispose();
                pdfDocument.Save(saveDialog.FileName);

                MessageBox.Show("PDF erfolgreich gespeichert:\n" + saveDialog.FileName, "PDF speichern", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowErrorMessageBox("PDF speichern", "Fehler beim Erstellen des PDFs: " + ex.Message);
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