using System.Windows;

using EDIViewer.ViewModel;

namespace EDIViewer
{
    /// <summary>
    /// Interaktionslogik für DialogBox_NewRecordType.xaml
    /// </summary>
    public partial class DialogBox_NewFormatTyp : Window
    {
        public DialogBox_NewFormatTyp()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Fenszer mit dem Übergebenen Fomart Typ starten
        /// </summary>
        /// <param name="context"></param>
        public DialogBox_NewFormatTyp(FormatTypNewViewModel context):this()
        {
            this.DataContext = context;
            context.Save += Save;
        }
        /// <summary>
        /// Fenster schließen beim Speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save(object sender, EventArgs e)
        {
            this.Close();
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