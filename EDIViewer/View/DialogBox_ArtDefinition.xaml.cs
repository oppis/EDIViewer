using System.Windows;
using System.Windows.Input;

using EDIViewer.Helper;
using EDIViewer.Models;
using EDIViewer.ViewModel;

namespace EDIViewer.View
{
    /// <summary>
    /// Interaktionslogik für DialogBox_ArtDefiniation.xaml
    /// </summary>
    public partial class DialogBox_ArtDefiniation : Window
    {
        ArtDefinationViewModel artDefinationViewModel;
        public DialogBox_ArtDefiniation()
        {
            InitializeComponent();
        }
        public DialogBox_ArtDefiniation(ArtDefinationViewModel viewModel):this()
        {
            artDefinationViewModel = viewModel;
            this.DataContext = artDefinationViewModel;

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

        /// <summary>
        /// Einfügen aus Zwischenablage für Art Definitionen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgArtDefination_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (!Clipboard.ContainsText())
                    {
                        UserMessageHelper.ShowMessageBox("Einfügen", "Die Zwischen ablage enthält keinen Text zum einfügen");
                        return;
                    }

                    //Uses tab as the default separator, but if there's no tab, use the system's default

                    String textSeparator = Clipboard.GetText().Contains('\t') ? "\t" : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;

                    List<String> clipboardAsList = new(Clipboard.GetText().Split('\n'));

                    List<String[]> cleanLines = clipboardAsList
                     .Select(s => s.Replace("\n", "").Replace("\r", "").Split(textSeparator.ToCharArray()))
                     .ToList<String[]>()
                     ;

                    foreach (String[] line in cleanLines)
                    {
                        if (line.Length == 3)
                        {
                            ArtDefination artDefination = new()
                            {
                                Id = line[0],
                                Name = line[1],
                                Description = line[2]
                            };
                            artDefinationViewModel.ArtDefinations.Add(artDefination);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowMessageBox("Fehler Einfügen", "Fehler: " + ex.Message);
            }
        }
    }
}