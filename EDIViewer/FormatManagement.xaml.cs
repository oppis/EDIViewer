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
        FileStructur selectedFileStructur;

        //Listen der Verfügbaren Felder
        List<FormatType> availableFormatTypes;
        List<RecordType> availableRecordTypes;
        List<FieldDefination> availableFieldDefinations;

        //Listen für aktuelle Format Struktur
        FormatType selectedFormatType;
        RecordType selectedRecordType;        

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
            //Aktuell sichtbare Satzarten und Felder in das Objekt schreiben
            //SaveFieldDefination();
            //SaveRecordType();

            string output = JsonConvert.SerializeObject(selectedFileStructur);
            string formatFilePath = Path.Combine(Environment.CurrentDirectory, Path.Combine("Formate", selectedFileStructur.FormatName + ".JSON")); 
            File.WriteAllText(formatFilePath, output);

            //TODO -> Ansicht aktualisieren davor -> Auch Update wenn Datei geändert wird.

            this.Close();
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
            VersionValue.Content = selectedFileStructur.Version;
            FormatDetection.Content = selectedFileStructur.FormatDetection;
            SeparatorValue.Content = selectedFileStructur.Separator;
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
            //Aktuellen Inhalt der RecordTypes setzen
            //availableRecordTypes = selectedFormatType.RecordType;

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
        /// Aktuelle Felder Informationen laden
        /// </summary>
        /// <param name="json"></param>
        private void SetFieldDefinations()
        {          
            //Prüfen ob Feld Definitionen vorhanden sind
            if (selectedRecordType != null)
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
                availableFieldDefinations = selectedRecordType.FieldDefination;

                dgFieldDefination.ItemsSource = availableFieldDefinations;
            }
            else
            {
                dgFieldDefination.ItemsSource = null;
            }
        }
    }
}