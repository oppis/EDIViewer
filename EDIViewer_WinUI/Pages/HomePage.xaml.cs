using System;
using System.IO;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace EDIViewer_WinUI.Pages
{
    /// <summary>
    /// Anzeige der TabViews für die Datei Ansicht
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Auf Button reagieren Datei Explorer öffnen zur Datei Auswahl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void TabView_AddButtonClickAsync(TabView sender, object args)
        {
            FileOpenPicker openFilePicker = new()
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                FileTypeFilter = { "*" }
            };

            WinRT.Interop.InitializeWithWindow.Initialize(openFilePicker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));

            StorageFile selectedFile = await openFilePicker.PickSingleFileAsync();
            if (selectedFile != null)
            {
                sender.TabItems.Add(CreateNewTab(selectedFile.Path));
                sender.SelectedIndex = sender.TabItems.Count - 1;
            }
        }
        /// <summary>
        /// Tab schließen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }

        /// <summary>
        /// Neuen Tab anlegen
        /// </summary>
        /// <param name="filePath">Dateipfad zum Anzeigen</param>
        /// <returns></returns>
        private static TabViewItem CreateNewTab(string filePath)
        {
            TabViewItem newItem = new()
            {
                Header = Path.GetFileName(filePath),
                IconSource = new SymbolIconSource() { Symbol = Symbol.Document }
            };

            Frame frame = new();
            frame.Navigate(typeof(FileViewPage), filePath);

            newItem.Content = frame;

            return newItem;
        }
    }
}