using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using EDIViewer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EDIViewer
{
    /// <summary>
    /// Interaktionslogik für FormatManagement.xaml
    /// </summary>
    public partial class FormatManagement : Window
    {
        public FormatManagement()
        {
            InitializeComponent();

            string fileName = Path.Combine(Environment.CurrentDirectory, "Formate") + "\\" + "Fortras100.json";

            string json = File.ReadAllText(fileName);

            ParseTestJson3(json);
        }

        private void SaveWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public void ParseTestJson3(string json)
        {
            List<FileStructur> fileData =  JsonConvert.DeserializeObject<List<FileStructur>>(json);

            foreach (FileStructur file in fileData)
            {
                //Datei Struktur
                VersionValue.Content = file.Version;
                SeparatorValue.Content = file.Separator;
                cbFormat.Items.Add(file.FormatName);                

                //in Funktionen aufteilen nicht alles auf einmal laden
                foreach (FormatType formatType in file.FormatType)
                {
                    //Information Format Typ -> Entl, Status, Sendung
                    //item2.Description;
                    cbFormatTyp.Items.Add(formatType.Name);

                    //RecordType in Tabelle darstellen
                    DataTable dataTable = new DataTable(typeof(RecordType).Name);
                    //Get all the properties
                    PropertyInfo[] Props = typeof(RecordType).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (PropertyInfo prop in Props)
                    {
                        //Setting column names as Property names
                        dataTable.Columns.Add(prop.Name);
                    }
                    foreach (RecordType recordType in formatType.RecordType)
                    {
                        var values = new object[Props.Length];
                        for (int i = 0; i < Props.Length; i++)
                        {
                            //inserting property values to datatable rows
                            values[i] = Props[i].GetValue(recordType, null);
                        }
                        dataTable.Rows.Add(values);
                    }

                    DataGrid myDataGrid = new()
                    {
                        AutoGenerateColumns = true,
                        Width = 300,
                        Height = 175,

                        ItemsSource = dataTable.AsDataView()
                    };

                    myCanvas.Children.Add(myDataGrid);

                    foreach (RecordType recordType in formatType.RecordType)
                    {
                        //Satzarten -> werden schon in Tabelle angezeigt

                        //Feld Informationen in Tabelle darstellen
                        DataTable dataTable2 = new DataTable(typeof(FieldDefination).Name);
                        //Get all the properties
                        PropertyInfo[] Props2 = typeof(FieldDefination).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (PropertyInfo prop in Props2)
                        {
                            //Setting column names as Property names
                            dataTable2.Columns.Add(prop.Name);
                        }
                        foreach (FieldDefination item4 in recordType.FieldDefination)
                        {
                            var values = new object[Props2.Length];
                            for (int i = 0; i < Props2.Length; i++)
                            {
                                //inserting property values to datatable rows
                                values[i] = Props2[i].GetValue(item4, null);
                            }
                            dataTable2.Rows.Add(values);
                        }

                        DataGrid myDataGrid2 = new()
                        {
                            AutoGenerateColumns = true,
                            Width = 300,
                            Height = 175,

                            ItemsSource = dataTable2.AsDataView()
                        };

                        myCanvas2.Children.Add(myDataGrid2);
                    }
                }
            }  
        }
    }
}