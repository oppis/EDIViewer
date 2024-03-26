using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

using EDIViewer.Models;
using EDIViewer.Helper;


namespace EDIViewer
{
    /// <summary>
    /// Interaktionslogik für FormatManagement.xaml
    /// </summary>
    public partial class FormatManagement : Window
    {
        //Listen für Datei Inhalt
        FileStructur selectedFileStructur;

        //Listen der Verfügbaren Felder
        List<FormatType> availableFormatTypes;
        List<RecordType> availableRecordTypes;
        List<FieldDefination> availableFieldDefinations;

        //Listen für aktuelle Format Struktur
        FormatType selectedFormatType;
        RecordType selectedRecordType;

        DataTable dtFieldDefinations;
        
        //Für Anlage neuer Recodtyp
        bool DgRecordTypNewItem = false;

        /// <summary>
        /// Start des Fensters
        /// </summary>
        public FormatManagement()
        {
            InitializeComponent();

            //Aktuelle Formate laden
            LoadFormatFiles();
        }
        /// <summary>
        /// Laden der vorhanden Format Definitionen
        /// </summary>
        private void LoadFormatFiles()
        {
            Dictionary<string, string> currentFormatFiles = FormatFiles.GetCurrentFormatFiles();

            //Aktuelle Formate in ComboBox laden und nur den Format Typ anzeigen
            cbFormat.ItemsSource = currentFormatFiles;
            cbFormat.DisplayMemberPath = "Key";
        }
        #region Fenster Verwaltung
        /// <summary>
        /// Speichern der Informationen aus dem Fenster
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveWindow(object sender, RoutedEventArgs e)
        {
            //Aktuell sichtbare Satzarten und Felder in das Objekt schreiben

            //Speichern der Textboxen
            selectedFileStructur.Version = Convert.ToInt16(VersionValue.Text);
            selectedFileStructur.FormatDetection = FormatDetectionValue.Text;
            selectedFileStructur.Separator = SeparatorValue.Text;
            selectedFileStructur.FormatVariation = FormatVariationValue.Text;

            string output = JsonConvert.SerializeObject(selectedFileStructur);
            string formatFilePath = Path.Combine(Environment.CurrentDirectory, Path.Combine("Formate", selectedFileStructur.FormatName + ".JSON")); 
            File.WriteAllText(formatFilePath, output);

            DialogResult = true;
            this.Close();
        }
        /// <summary>
        /// Fenster schließen ohne Speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        #endregion
        /// <summary>
        /// Reagieren auf auswahl der Datei Struktur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Aktuelle Werte FormatName und Pfad            
            KeyValuePair<string, string> selectedPath = (KeyValuePair<string, string>)cbFormat.SelectedItem;

            //Alle Felder leeren
            cbFormatTyp.Items.Clear();
            dgRecordType.ItemsSource = null;
            dgFieldDefination.ItemsSource = null;

            //Datei Infos in JSON lesen
            GetFileStructur(selectedPath.Value);

            //DropDown füllen mit Format Typen
            SetFileFormats();

            //Datei Informationen in Felder schreiben
            VersionValue.Text = selectedFileStructur.Version.ToString();
            FormatDetectionValue.Text = selectedFileStructur.FormatDetection;
            SeparatorValue.Text = selectedFileStructur.Separator;
            FormatVariationValue.Text = selectedFileStructur.FormatVariation;
        }
        /// <summary>
        /// Aktuelles Format laden
        /// </summary>
        /// <param name="json"></param>
        private void GetFileStructur(string currentFile)
        {
            string json = File.ReadAllText(currentFile);

            selectedFileStructur = JsonConvert.DeserializeObject<FileStructur>(json);
        }
        /// <summary>
        /// Aktuelles Datei Format setzen und weiteres ComboBox füllen mit Items
        /// </summary>
        private void SetFileFormats()
        {
            //Aktuelle Format Typen speichern für weitern Zugriff
            availableFormatTypes = selectedFileStructur.FormatType;
            
            //Format Typen ermitteln und in ComboBox setzen
            foreach (FormatType availableFormatType in availableFormatTypes)
            {
                //ComboBox füllen mit Möglichen Typen
                cbFormatTyp.Items.Add(availableFormatType.Name);
            }
        }
        /// <summary>
        /// Reagieren auf Auswahl von Format Typ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbFormatTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Leeren der Satz und Feld Definitionen
            dgRecordType.ItemsSource = null;
            dgFieldDefination.ItemsSource = null;
            
            //Ermitteln des ausgewählten Format Types und speichern
            if (cbFormatTyp.SelectedItem != null)
            {
                selectedFormatType = availableFormatTypes.Find(x => x.Name == cbFormatTyp.SelectedItem.ToString());

                //Informationen zum Format Typ setzen
                formatTypeDescription.Content = selectedFormatType.Description;

                //Füllen der Satzart Definitionen
                SetRecordInfos();
            }
        }
        /// <summary>
        /// Aktuelle Format Typ Informationen setzen und Mögliche Satzarten füllen
        /// </summary>
        /// <param name="currentFormatType"></param>
        private void SetRecordInfos()
        {
            //Tabelle für Satzarten

            //Inhalt des JSON in die Tabelle laden
            DataTable dtRecordType = new(typeof(RecordType).Name);

            //Tabellen Format erstellen
            dtRecordType.Columns.Add("Position");
            dtRecordType.Columns.Add("Name");
            dtRecordType.Columns.Add("Beschreibung");

            availableRecordTypes = selectedFormatType.RecordType;

            dgRecordType.ItemsSource = availableRecordTypes;
        }
        /// <summary>
        /// Reagieren auf Auswahl von Satzart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgRecordType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Prüfung ob noch keine Werte in der Tabelle sind -> Wechsel der Satzart
            if (dgRecordType.SelectedIndex < dgRecordType.Items.Count - 1)
            {
                //Ausgeählten Satzart in aktuelle Liste schreiben
                if (dgRecordType.SelectedItem != null)
                {
                    selectedRecordType = availableRecordTypes.Find(x => x.Name == (dgRecordType.SelectedItem as RecordType).Name);
                    //Füllen der Feld Definitionen
                    SetFieldDefinations();
                }
            }
            else
            {
                dgFieldDefination.ItemsSource = null;
            }
        }
        /// <summary>
        /// Anlegen eine neuen Feld Definitionsarray mit auswahl
        /// </summary>
        private void CreateNewFieldDefinationTable()
        {          
            //Neue Feld Definitionen
            selectedRecordType.FieldDefination = [];
            
            //Neue Felder auswählen und als Quelle hinterlegen
            availableFieldDefinations = selectedRecordType.FieldDefination;
            dgFieldDefination.ItemsSource = availableFieldDefinations;
        }
        /// <summary>
        /// Aktuelle Felder Informationen laden
        /// </summary>
        private void SetFieldDefinations()
        {          
            //Prüfen ob Feld Definitionen vorhanden sind
            if (selectedRecordType != null)
            {
                availableFieldDefinations = selectedRecordType.FieldDefination;

                dgFieldDefination.ItemsSource = availableFieldDefinations;
            }
            else
            {
                dgFieldDefination.ItemsSource = null;
            }
        }
        /// <summary>
        /// Einfügen aus Zwischenablage für Feld Definitionen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (!Clipboard.ContainsText())
                {
                    Console.WriteLine("Clipboard does not containt any text to paste.");
                    return;
                }

                //Uses tab as the default separator, but if there's no tab, use the system's default

                String textSeparator = (Clipboard.GetText().Contains("\t")) ? "\t" : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;

                List<String> clipboardAsList = new(Clipboard.GetText().Split('\n'));

                List<String[]> cleanLines = clipboardAsList
                 .Select(s => s.Replace("\n", "").Replace("\r", "").Split(textSeparator.ToCharArray()))
                 .ToList<String[]>()
                 ;

                foreach (String[] line in cleanLines)
                {
                    if (dgFieldDefination.Columns.Count < line.Length)
                    {
                        for (int i = dgFieldDefination.Columns.Count; i < line.Length; i++)
                        {
                            dtFieldDefinations.Columns.Add();
                        }
                    }

                    //If the clipboard contains too many columns and createNewColumnsIfRequired is false

                    if (line.Length == 8)
                    {
                        FieldDefination fieldDefination = new()
                        {
                            Position = Convert.ToInt16(line[0]),
                            Name = line[1],
                            Start = Convert.ToInt16(line[2]),
                            Length = Convert.ToInt16(line[3]),
                            Description = line[4],
                            DataType = line[5],
                            Mandatory = Convert.ToBoolean(line[6]),
                            MappingField = line[7]
                        };

                        availableFieldDefinations.Add(fieldDefination);
                        dgFieldDefination.Items.Refresh();
                    }
                }
            }
        }
        /// <summary>
        /// RecordTyp Edit Ending Prüfung  neues Item -> auslösen anlegen erste Fielddefination
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgRecordType_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if ((e.EditAction == DataGridEditAction.Commit) && DgRecordTypNewItem)
            {
                CreateNewFieldDefinationTable();

                //Wenn neues Item Fertig Variable zurücksetzen
                DgRecordTypNewItem = false;
            }
        }
        /// <summary>
        /// RecordTyp bei neue Zeile Variable setzen für Prüfung
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgRecordType_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            DgRecordTypNewItem = true;
        }
        /// <summary>
        /// Öffnen des Fensters zur Anlage einer neuen Datei
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            DialogBox_NewFormatFile windowDialogBoxNewFormatFile = new()
            {
                Owner = this
            };

            //Prüfen wie Fenster geschlossen wurde
            bool? status = windowDialogBoxNewFormatFile.ShowDialog();

            if ((bool)status)
            {
                LoadFormatFiles();
            }
        }
    }
}