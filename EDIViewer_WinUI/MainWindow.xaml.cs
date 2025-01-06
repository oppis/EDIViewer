using System;
using System.Collections.ObjectModel;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using EDIViewer_WinUI.Models;
using EDIViewer_WinUI.Pages;


namespace EDIViewer_WinUI
{
    /// <summary>
    /// MainWindows für App
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);      // set user ui element as titlebar
        }

        /// <summary>
        /// Tmp bilden der NavigationView Struktur
        /// </summary>
        public ObservableCollection<NavigationFormat> NavigationFormats = new()
        {
            new NavigationFormat()
            {
                Name = "Datei Ansicht",
                Icon = new SymbolIcon(Symbol.OpenFile),
                Tag = "Home",
                ToolTip = "Dartstellung der Dateien"
            },
            new NavigationFormat(){
                Name = "Formate",
                Icon = new SymbolIcon(Symbol.Folder),
                ToolTip = "Formate Test",
                Tag = "Formats",
                Children = [
                    new NavigationFormat(){
                        Name = "TEST 1",
                        Icon = new SymbolIcon(Symbol.Bookmarks),
                        Tag = "FormatDef",
                        Children = [
                            new NavigationFormat() { Name  = "Standard", Icon = new SymbolIcon(Symbol.Page), Tag = "FormatTypDef" }
                        ]
                    }
                ]
            },

        };

        /// <summary>
        /// Reagieren auf änderungen in der Auswahl beim NavigationView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavOnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                sender.Header = "Einstellungen";

                contentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                NavigationFormat selectedItem = (NavigationFormat)args.SelectedItem;

                if (selectedItem != null)
                {
                    sender.Header = selectedItem.Name;

                    Type pageType = Type.GetType("EDIViewer_WinUI.Pages." + selectedItem.Tag + "Page");
                    contentFrame.Navigate(pageType);
                }
            }
        }
    }
}