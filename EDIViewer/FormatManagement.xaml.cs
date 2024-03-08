using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using EDIViewer.Models;
using Newtonsoft.Json;

namespace EDIViewer
{
    /// <summary>
    /// Interaktionslogik für FormatManagement.xaml
    /// </summary>
    public partial class FormatManagement : Window
    {
        //Listen für Datei Inhalt
        List<FileStructur> fileData;
        List<FormatType> formatTypes;
        List<RecordType> recordTypes;
        public FormatManagement()
        {
            InitializeComponent();

            string fileName = Path.Combine(Environment.CurrentDirectory, "Formate") + "\\" + "Fortras100.json";

            string json = File.ReadAllText(fileName);

            GetFileInfos(json);
        }

        private void SaveWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Informationen zum Format Laden und Typen anzeigen
        /// </summary>
        /// <param name="json"></param>
        private void GetFileInfos(string json)
        {
            fileData = JsonConvert.DeserializeObject<List<FileStructur>>(json);

            foreach (FileStructur file in fileData) 
            {
                cbFormat.Items.Add(file.FormatName);
            }            
        }
        /// <summary>
        /// Reagieren auf auswahl der Datei Struktur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Inhalt zum ausgewählten Element finden
            FileStructur currentFileStructur = fileData.Find(x => x.FormatName == cbFormat.SelectedItem.ToString());
            SetCurrentFileFormat(currentFileStructur);
        }
        /// <summary>
        /// Aktuelles Datei Format setzen und weiteres ComboBox füllen mit Items
        /// </summary>
        /// <param name="currentFileStructur"></param>
        private void SetCurrentFileFormat(FileStructur currentFileStructur)
        {
            //Datei Struktur
            VersionValue.Content = currentFileStructur.Version;
            SeparatorValue.Content = currentFileStructur.Separator;

            formatTypes = currentFileStructur.FormatType;

            //Format Typen ermitteln
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
        private void cbFormatTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FormatType currentFormatType = formatTypes.Find(x => x.Name == cbFormatTyp.SelectedItem.ToString());
            setRecordInfos(currentFormatType);

            //Leeren der Feld Definitionen
            dgFieldDefination.ItemsSource = null;
        }
        /// <summary>
        /// Aktuelle Format Typ Informationen setzen und Mögliche Satzarten füllen
        /// </summary>
        /// <param name="currentFormatType"></param>
        private void setRecordInfos(FormatType currentFormatType)
        {
            //erst setzen wenn ein cb Item ausgewählt wird
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
        private void dgRecordType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Prüfung ob noch keine Werte in der Tabelle sind -> Wechsel der Satzart
            if (dgRecordType.SelectedItem != null)
            {
                RecordType currentRecordType = recordTypes.Find(x => x.Name == (dgRecordType.SelectedItem as DataRowView).Row["Name"]);
                SetFieldDefinations(currentRecordType);               
            }
        }
        /// <summary>
        /// Aktuelle Feld Informationen laden
        /// </summary>
        /// <param name="json"></param>
        private void SetFieldDefinations(RecordType currentRecordType)
        {
            //Inhalt des JSON in die Tabelle laden
            DataTable dtFieldDefinations= new(typeof(FieldDefination).Name);

            //Tabellen Format erstellen
            PropertyInfo[] Props2 = typeof(FieldDefination).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props2)
            {
                //Setting column names as Property names
                dtFieldDefinations.Columns.Add(prop.Name);
            }
            foreach (FieldDefination fieldDefination in currentRecordType.FieldDefination)
            {
                var values = new object[Props2.Length];
                for (int i = 0; i < Props2.Length; i++)
                {
                    values[i] = Props2[i].GetValue(fieldDefination, null);
                }
                dtFieldDefinations.Rows.Add(values);
            }

            dgFieldDefination.ItemsSource = dtFieldDefinations.AsDataView();
        }
    }
}