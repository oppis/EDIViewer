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
            //Prüfung ob Inhalt der Felder leer sind
            bool checkSave = false;

            if (!String.IsNullOrEmpty(formatNameValue.Text) && !String.IsNullOrEmpty(formatSeparatorValue.Text) && !String.IsNullOrEmpty(formatDetectionValue.Text) && !String.IsNullOrEmpty(formationVariationValue.Text))
            {
                checkSave = true;
            };

            if (checkSave)
            {
                FileStructur fileStructur = new()
                {
                    FormatVersion = 1,
                    FormatName = formatNameValue.Text,
                    FormatSeparator = formatSeparatorValue.Text,
                    FormatDetection = formatDetectionValue.Text,
                    FormatVariation = formationVariationValue.Text,
                    FormatTypes = []
                };

                string output = JsonConvert.SerializeObject(fileStructur);
                string formatFilePath = Path.Combine(Environment.CurrentDirectory, Path.Combine("Formate", formatNameValue.Text + ".JSON"));
                File.WriteAllText(formatFilePath, output);

                DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Es müssen alle Felder gefüllt sein!", "Anlegen neues Format");
            }
        }

        /// <summary>
        /// Fenster schließen ohne Speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}