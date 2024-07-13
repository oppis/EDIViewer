using System.Windows;

using EDIViewer.Helper;
using EDIViewer.ViewModel;

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
            if (FileStructurNewViewModel.checkSaveStatus)
            {
                DialogResult = true;
                this.Close();
            }
            else 
            {
                UserMessageHelper.ShowMessageBox("Anlegen Format Datei", "Angaben bitte prüfen!");
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