using System.IO;
using System.Windows;
using Newtonsoft.Json;

using EDIViewer.Models;

namespace EDIViewer
{
    /// <summary>
    /// Interaktionslogik für DialogBox_NewRecordType.xaml
    /// </summary>
    public partial class DialogBox_NewFormatFile : Window
    {
        public DialogBox_NewFormatFile()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Anlage der Datei
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            FileStructur fileStructur = new()
            {
                Version = 1,
                FormatName = formatNameValue.Text,
                Separator = formatSeparatorValue.Text,
                FormatDetection = formatDetectionValue.Text,
                FormatVariation = formationVariationValue.Text,
                FormatType = new()
            };

            string output = JsonConvert.SerializeObject(fileStructur);
            string formatFilePath = Path.Combine(Environment.CurrentDirectory, Path.Combine("Formate", formatNameValue.Text + ".JSON"));
            File.WriteAllText(formatFilePath, output);

            this.Close();
        }

        /// <summary>
        /// Fenster schließen ohne Speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}