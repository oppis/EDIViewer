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
        private Brush _previousFill = null;
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

        //Drag and Drop einbauen : https://learn.microsoft.com/de-de/dotnet/desktop/wpf/advanced/drag-and-drop-overview?view=netframeworkdesktop-4.8
        private void ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (ellipse != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(ellipse,
                                     ellipse.Fill.ToString(),
                                     DragDropEffects.Copy);
            }
        }

        private void ellipse_DragEnter(object sender, DragEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (ellipse != null)
            {
                // Save the current Fill brush so that you can revert back to this value in DragLeave.
                _previousFill = ellipse.Fill;

                // If the DataObject contains string data, extract it.
                if (e.Data.GetDataPresent(DataFormats.StringFormat))
                {
                    string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                    // If the string can be converted into a Brush, convert it.
                    BrushConverter converter = new BrushConverter();
                    if (converter.IsValid(dataString))
                    {
                        Brush newFill = (Brush)converter.ConvertFromString(dataString);
                        ellipse.Fill = newFill;
                    }
                }
            }
        }
        #endregion
    }
}