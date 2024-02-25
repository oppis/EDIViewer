using System.Windows;

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
        }

        private void SaveWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
