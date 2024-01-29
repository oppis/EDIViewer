using System.Windows;
using System.IO;
using Microsoft.Win32;


namespace EDIViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string filePath = string.Empty;
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

            if (dialog.ShowDialog() == true)
            {
                filePath = Path.GetDirectoryName(dialog.FileName);
                fileName = dialog.SafeFileName;

                txtFilePath.Text = filePath;
                txtFileName.Text = fileName;
            }
        }

        private void File_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                //read only the First File                
                string filePathName = (files[0]);

                txtFilePath.Text = Path.GetDirectoryName(filePathName);
                txtFileName.Text = Path.GetFileName(filePathName);
            }
        }
        #endregion
    }
}