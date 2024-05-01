using System.Windows;

using EDIViewer.ViewModel;

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
        public DialogBox_ArtDefiniation(ArtDefinationViewModel viewModel):this()
        {
            this.DataContext = viewModel;
            viewModel.Save += Save;
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