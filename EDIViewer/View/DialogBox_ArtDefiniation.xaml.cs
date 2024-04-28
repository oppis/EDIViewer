using System.Windows;

namespace EDIViewer.View
{
    /// <summary>
    /// Interaktionslogik für DialogBox_ArtDefiniation.xaml
    /// </summary>
    public partial class DialogBox_ArtDefiniation : Window
    {
        public DialogBox_ArtDefiniation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fenster schließen beim Speichern 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

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