using System;
using System.Reflection;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Storage;
using Windows.Storage.Pickers;

namespace EDIViewer_WinUI.Pages
{
    /// <summary>
    /// Seite mit den Einstellungen
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            //Aktuelle Informationen laden
            GetInformations();

            //Laden der Aktuellen Einstellung für den Formats Pfad
            LoadCurrentFormatFilePath();
        }

        /// <summary>
        /// Aktuelle Informationen zur Anwendung abrufen
        /// </summary>
        private void GetInformations()
        {
            dotNetVersion.Text = Environment.Version.ToString();
            ediViewerVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Aktuellen Wert aus der Datenbank holen
        /// </summary>
        private void LoadCurrentFormatFilePath()
        {
            folderCard.Description = "TODO";
        }

        /// <summary>
        /// Öffnen eines File Dialog Auswahl des Datei Pfads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ClickLoadFileFormatPathAsync(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            senderButton.IsEnabled = false;

            FolderPicker openFolderPicker = new()
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                FileTypeFilter = { "*" }
            };

            WinRT.Interop.InitializeWithWindow.Initialize(openFolderPicker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));

            StorageFolder folder = await openFolderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                folderCard.Description = folder.Path;

                //TODO -> Auch Speichern in Registry
            }

            senderButton.IsEnabled = true;
        }
    }
}