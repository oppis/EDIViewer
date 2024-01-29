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

        StreamReader originalFile = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private MessageBoxResult ShowMessageBox(string title, string message)
        {
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxResult result;

            result = MessageBox.Show(message, title, button, icon, MessageBoxResult.Yes);

            return result;
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

                //TODO -> Check File Content -> Text

                File_LoadView();
            }
        }

        private void File_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                //read only the First File                
                string filePathName = (files[0]);

                filePath = Path.GetDirectoryName(filePathName);
                fileName = Path.GetFileName(filePathName);

                txtFilePath.Text = filePath;
                txtFileName.Text = fileName;

                //TODO -> Check File Content -> Text

                File_LoadView();
            }
        }
        #endregion

        private void File_LoadView()
        {
            try
            {
                originalFile = new(Path.Combine(filePath, fileName));

                FileOriginalView.Text = originalFile.ReadToEnd();
            }
            catch (Exception ex)
            {
                ShowMessageBox("Datei öffnen", "Fehler: " + ex.Message);
            }
        }
    }
}