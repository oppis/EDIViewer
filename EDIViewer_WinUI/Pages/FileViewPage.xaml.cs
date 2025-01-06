using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EDIViewer_WinUI.Pages
{
    /// <summary>
    /// Ansicher der EDI Datei
    /// </summary>
    public sealed partial class FileViewPage : Page
    {
        public FileViewPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Übergabe Parameter erhalten mit dem Dateipfad
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            File_LoadView((string)e.Parameter);

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Laden der Datei Ansicht
        /// </summary>
        private void File_LoadView(string CurrentFilePath)
        {
            fileOriginalView.Text = File.ReadAllText(CurrentFilePath);
        }
    }
}
