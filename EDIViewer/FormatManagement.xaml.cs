using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
        FileStructur fileData;
        List<FormatType> formatTypes;
        List<RecordType> recordTypes;
        List<FieldDefination> fieldDefs;
        string currentRecordType;
        /// <summary>
        /// Start des Fensters
        /// </summary>
        public FormatManagement()
        {
            InitializeComponent();

            //Aktuelle Formate laden
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
            //this.Close();
            string output = JsonConvert.SerializeObject(fileData);
            string formatFilePath = Path.Combine(Environment.CurrentDirectory, Path.Combine("Formate", fileData.FormatName + ".JSON")); 
            File.WriteAllText(formatFilePath, output);
        }
        /// <summary>
        /// Fenster schließen ohne Speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
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
            KeyValuePair<string, string> value = (KeyValuePair<string, string>)cbFormat.SelectedItem;

            //Datei Infos in JSON lesen
            GetFileInfos(value.Value);

            //Alle Felder leeren
            cbFormatTyp.Items.Clear();
            dgRecordType.ItemsSource = null;
            dgFieldDefination.ItemsSource = null;

            SetCurrentFileFormat(fileData);
        }
        /// <summary>
        /// Aktuelles Format laden
        /// </summary>
        /// <param name="json"></param>
        private void GetFileInfos(string currentFile)
        {
            string json = File.ReadAllText(currentFile);

            fileData = JsonConvert.DeserializeObject<FileStructur>(json);
        }
        /// <summary>
        /// Aktuelles Datei Format setzen und weiteres ComboBox füllen mit Items
        /// </summary>
        /// <param name="currentFileStructur"></param>
        private void SetCurrentFileFormat(FileStructur currentFileStructur)
        {
            //Datei Struktur Informationen
            VersionValue.Content = currentFileStructur.Version;
            FormatDetection.Content = currentFileStructur.FormatDetection;
            SeparatorValue.Content = currentFileStructur.Separator;

            //Aktuelle Format Typen speichern für weitern Zugriff
            formatTypes = currentFileStructur.FormatType;
            
            //Format Typen ermitteln und in ComboBox setzen
            foreach (FormatType formatType in formatTypes)
            {
                //ComboBox füllen mit Möglichen Typen
                cbFormatTyp.Items.Add(formatType.Name);
            }
        }
        /// <summary>
        /// Reagieren auf Auswahl von Format Typ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbFormatTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Leeren der Feld Definitionen
            dgRecordType.ItemsSource = null;
            dgFieldDefination.ItemsSource = null;
            
            if (cbFormatTyp.SelectedItem != null)
            {
                FormatType currentFormatType = formatTypes.Find(x => x.Name == cbFormatTyp.SelectedItem.ToString());
                SetRecordInfos(currentFormatType);
            }
        }
        /// <summary>
        /// Aktuelle Format Typ Informationen setzen und Mögliche Satzarten füllen
        /// </summary>
        /// <param name="currentFormatType"></param>
        private void SetRecordInfos(FormatType currentFormatType)
        {
            //Informationen zum Format Typ setzen
            formatTypeDescription.Content = currentFormatType.Description;

            //Aktuellen Inhalt der RecordTypes setzen
            recordTypes = currentFormatType.RecordType;

            //Tabelle für Satzarten

            //Inhalt des JSON in die Tabelle laden
            DataTable dtRecordType = new(typeof(RecordType).Name);

            //Tabellen Format erstellen
            dtRecordType.Columns.Add("Position");
            dtRecordType.Columns.Add("Name");
            dtRecordType.Columns.Add("Beschreibung");

            foreach (RecordType recordType in recordTypes)
            {
                var values = new object[3];
                values[0] = recordType.Position;
                values[1] = recordType.Name;
                values[2] = recordType.Description;

                dtRecordType.Rows.Add(values);
            }

            dgRecordType.ItemsSource = dtRecordType.AsDataView();
        }
        /// <summary>
        /// Reagieren auf Auswahl von Satzart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgRecordType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Objekt updaten -> Neue FieldWerte zwischen speichern
            foreach (FormatType formatType in fileData.FormatType) 
            { 
                if (formatType.Name == cbFormatTyp.SelectedItem.ToString())
                {
                    foreach(RecordType recordType in formatType.RecordType)
                    {
                        if (recordType.Name == currentRecordType)
                        {
                            recordType.FieldDefination = fieldDefs;
                        }
                    }
                }
            }       
            
            //Prüfung ob noch keine Werte in der Tabelle sind -> Wechsel der Satzart
            if (dgRecordType.SelectedIndex < dgRecordType.Items.Count - 1)
            {
                if (dgRecordType.SelectedItem != null)
                {
                    RecordType currentRecordType = recordTypes.Find(x => x.Name == (dgRecordType.SelectedItem as DataRowView).Row["Name"]);
                    SetFieldDefinations(currentRecordType);
                }
            }
            else
            {
                dgFieldDefination.ItemsSource = null;
            }
        }
        /// <summary>
        /// Aktuelle Feld Informationen laden
        /// </summary>
        /// <param name="json"></param>
        private void SetFieldDefinations(RecordType currentRecordType)
        {          
            //Prüfen ob Feld Definitionen vorhanden sind
            if (currentRecordType !=null)
            {
                //Inhalt des JSON in die Tabelle laden
                DataTable dtFieldDefinations = new(typeof(FieldDefination).Name);

                //Tabellen Format erstellen
                PropertyInfo[] Props2 = typeof(FieldDefination).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo prop in Props2)
                {
                    //Setting column names as Property names
                    dtFieldDefinations.Columns.Add(prop.Name);
                }
                fieldDefs = currentRecordType.FieldDefination;

                dgFieldDefination.ItemsSource = fieldDefs;
            }
            else
            {
                dgFieldDefination.ItemsSource = null;
            }
        }
    }
}