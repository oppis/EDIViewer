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
        List<FieldDefination> availableFieldDefinations;

        //Listen für aktuelle Format Struktur
        RecordType selectedRecordType;

        DataTable dtFieldDefinations;

        ViewModel.FileStructurVM fileStructurVM;
        
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
            fileStructurVM.SaveFile();

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

            //Datei Infos in JSON lesen
            fileStructurVM = new ViewModel.FileStructurVM(selectedPath.Value);

            this.DataContext = fileStructurVM;
        }
        /// <summary>
        /// Anlegen eine neuen Feld Definitionsarray mit auswahl
        /// </summary>
        private void CreateNewFieldDefinationTable()
        {          
            //Neue Feld Definitionen
            selectedRecordType.FieldDefinations = [];
            
            //Neue Felder auswählen und als Quelle hinterlegen
            availableFieldDefinations = selectedRecordType.FieldDefinations;
            dgFieldDefination.ItemsSource = availableFieldDefinations;
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