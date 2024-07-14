using System.IO;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using EDIViewer.Models;
using EDIViewer.Helper;
using EDIViewer.ViewModel;
using EDIViewer.View;

namespace EDIViewer
{
    /// <summary>
    /// Interaktionslogik für FormatManagement.xaml
    /// </summary>
    public partial class FormatManagement : Window
    {
        //ViewModel für Daten Kontext
        FileStructurViewModel fileStructurViewModel;
       
        /// <summary>
        /// Start des Fensters
        /// </summary>
        public FormatManagement()
        {
            InitializeComponent();

            //Aktuelle Formate laden
            LoadFormatFiles();
        }

        /// <summary>
        /// Laden der vorhanden Format Definitionen
        /// </summary>
        private void LoadFormatFiles()
        {
            Dictionary<string, string> currentFormatFiles = FormatFiles.GetCurrentFormatFiles();

            //Aktuelle Formate in ComboBox laden und nur den Format Typ anzeigen
            cbFormat.ItemsSource = currentFormatFiles;
            cbFormat.DisplayMemberPath = "Key";
        }
        #region Fenster Verwaltung

        /// <summary>
        /// Speichern der Informationen aus dem Fenster
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveWindow(object sender, RoutedEventArgs e)
        {
            fileStructurViewModel.SaveFile();
        }

        /// <summary>
        /// Speichern der Informationen aus dem Fenster und Schließen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            fileStructurViewModel.SaveFile();

            DialogResult = true;


            this.Close();
        }

        /// <summary>
        /// Fenster schließen ohne Speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        #endregion
        /// <summary>
        /// Reagieren auf auswahl der Datei Struktur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Aktuelle Werte FormatName und Pfad            
            KeyValuePair<string, string> selectedPath = (KeyValuePair<string, string>)cbFormat.SelectedItem;

            //Datei Infos in JSON lesen
            fileStructurViewModel = new FileStructurViewModel(selectedPath.Value);
            DataContext = fileStructurViewModel;

            //Felder aktivieren
            cbFormatTyp.IsEnabled = true;
            createNewFormatTyp.IsEnabled = true;
            VersionValue.IsEnabled = true;
            FormatDetectionValue.IsEnabled = true;
            SeparatorValue.IsEnabled = true;
            FormatVariationValue.IsEnabled = true;
            SaveButton.IsEnabled = true;
            CloseSaveButton.IsEnabled = true;

            //FormatType Felder deaktivieren
            formatTypeDetection.IsEnabled = false;
            formatTypeDescription.IsEnabled = false;
            formatTypeOrderSeparatorStart.IsEnabled = false;
            formatTypeOrderSeparatorLength.IsEnabled = false;
            formatTypePositionSeparatorStart.IsEnabled = false;
            formatTypePositionSeparatorLength.IsEnabled = false;
        }

        /// <summary>
        /// Einfügen aus Zwischenablage für Feld Definitionen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (!Clipboard.ContainsText())
                    {
                        Console.WriteLine("Clipboard does not containt any text to paste.");
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
                        if (line.Length == 8)
                        {
                            FieldDefination fieldDefination = new()
                            {
                                Position = Convert.ToInt16(line[0]),
                                Name = line[1],
                                Start = Convert.ToInt16(line[2]),
                                Length = Convert.ToInt16(line[3]),
                                Description = line[4],
                                DataType = line[5],
                                Mandatory = Convert.ToBoolean(line[6]),
                                //MappingField = line[7]
                            };
                            fileStructurViewModel.SelectedRecordType.FieldDefinations.Add(fieldDefination);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowMessageBox("Fehler Einfügen", "Fehler: " + ex.Message);
            }
        }

        #region Actions Buttons

        /// <summary>
        /// Öffnen des Fensters zur Anlage einer neuen Format Definition
        /// </summary>  
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            DialogBox_NewFormatFile windowDialogBoxNewFormatFile = new()
            {
                Owner = this
            };

            //Prüfen wie Fenster geschlossen wurde
            bool? status = windowDialogBoxNewFormatFile.ShowDialog();

            if ((bool)status)
            {
                LoadFormatFiles();
            }
        }

        /// <summary>
        /// Öffnen Dialog -> Erstellen neuen Format Typ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewFormatTyp_Click(object sender, RoutedEventArgs e)
        {
            //Dialog Box erstellen mit Übergabe des aktuellen Kontextes
            DialogBox_NewFormatTyp dialogBoxNewFormatTyp = new(((FileStructurViewModel)this.DataContext).FormatTypNewViewModel);
            dialogBoxNewFormatTyp.ShowDialog();
        }
        
        /// <summary>
        /// Öffnen des aktuellen Format Ordner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFormatFolder(object sender, RoutedEventArgs e)
        {
            string currentFormatFolderPath = FormatFiles.LoadCurrentFormatFolderPath();

            if (Directory.Exists(currentFormatFolderPath))
            {
                Process.Start("explorer.exe", currentFormatFolderPath);
            }
            else
            {
                UserMessageHelper.ShowMessageBox("Öffnen Format Ordner", "Das Verzeichnis exsisitiert nicht: " + currentFormatFolderPath);
            }
        }

        #endregion

        /// <summary>
        /// Felder aktivieren wenn ein FormatTyp ausgewählt wird
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbFormatTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Felder aktivieren
            formatTypeDetection.IsEnabled = true;
            formatTypeDescription.IsEnabled = true;
            formatTypeOrderSeparatorStart.IsEnabled = true;
            formatTypeOrderSeparatorLength.IsEnabled = true;
            formatTypePositionSeparatorStart.IsEnabled = true;
            formatTypePositionSeparatorLength.IsEnabled = true;
        }

        /// <summary>
        /// Öffnen Dialog Art Definitionen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgFieldDefinations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Dialog Box erstellen mit Übergabe des aktuellen Kontextes
            DialogBox_ArtDefiniation dialogBox_ArtDefiniation = new(((FileStructurViewModel)this.DataContext).ArtDefinationViewModel); //TODO -> Auswahl Field Def -> Button
            dialogBox_ArtDefiniation.ShowDialog();
        }

    }
}