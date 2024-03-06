using System.Data;
using System.IO;
using System.IO.Enumeration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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

            //ParseTestJson(json);
            //ParseTestJson2(json);
            //JsonToDataTable(json);
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

        private void ParseTestJson(string json)
        {
            DataTable dt = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

            DataGrid myDataGrid = new()
            {
                AutoGenerateColumns = true,
                Width = 300,
                Height = 300,

                ItemsSource = dt.AsDataView()
            };

            myCanvas.Children.Add(myDataGrid);
        }
        private void ParseTestJson2(string json)
        {
            // Deserialize the JSON into a JArray
            JArray jsonArray = JArray.Parse(json);

            // Create DataTables for each "Type"
            foreach (JToken item in jsonArray)
            {
                string type = item["Typ"].ToString();
                DataTable dt = (DataTable)JsonConvert.DeserializeObject(type, (typeof(DataTable)));

                foreach (var item2 in item["Typ"])
                {
                    foreach (var item3 in item2["Field"])
                    {
                        foreach (var item4 in item3["START"])
                        {
                            string test = item4["1"].ToString();
                            DataTable dt3 = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));

                            DataGrid myDataGrid = new()
                            {
                                AutoGenerateColumns = true,
                                Width = 300,
                                Height = 300,

                                ItemsSource = dt3.AsDataView()
                            };

                            myCanvas.Children.Add(myDataGrid);
                        }
                    }
                }
            }
        }
        public void JsonToDataTable(string jsonString)
        {
            var jsonLinq = JObject.Parse(jsonString);

            // Find the first array using Linq  
            var srcArray = jsonLinq.Descendants().Where(d => d is JArray).First();
            var trgArray = new JArray();
            foreach (JObject row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types  
                    if (column.Value is JValue)
                    {
                        cleanRow.Add(column.Name, column.Value);
                    }
                }
                trgArray.Add(cleanRow);
            }

            DataTable test = JsonConvert.DeserializeObject<DataTable>(trgArray.ToString());

            DataGrid myDataGrid = new()
            {
                AutoGenerateColumns = true,
                Width = 300,
                Height = 300,

                ItemsSource = test.AsDataView()
            };
            myCanvas.Children.Add(myDataGrid);



            var srcArray2 = jsonLinq.Descendants().Where(d => d is JArray).Last();
            var trgArray2 = new JArray();
            foreach (JObject row in srcArray2.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types  
                    if (column.Value is JValue)
                    {
                        cleanRow.Add(column.Name, column.Value);
                    }
                }
                trgArray2.Add(cleanRow);
            }

            DataTable test2 = JsonConvert.DeserializeObject<DataTable>(trgArray2.ToString());

            DataGrid myDataGrid2 = new()
            {
                AutoGenerateColumns = true,
                Width = 300,
                Height = 300,

                ItemsSource = test2.AsDataView()
            };
            myCanvas2.Children.Add(myDataGrid2);
        }
        public void ParseTestJson3(string json)
        {
            JObject data =  JsonConvert.DeserializeObject<JObject>(json);

            var nodeList = data.Flatten(node => node.Children).ToList();
        }
    }
}
