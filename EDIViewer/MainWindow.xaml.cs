using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace EDIViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string fileName = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Load Files
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                FileName = "EDI-Datei",
                DefaultExt = ".txt",
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                fileName = dialog.FileName;

                txtFilePath.Text = dialog.DefaultDirectory;
                txtFileName.Text = dialog.SafeFileName;
            }
        }

        private void file_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                fileName = (files[0]);

                //txtFilePath.Text = dialog.DefaultDirectory;
                txtFileName.Text = fileName;
            }
        }

        #endregion
    }
}