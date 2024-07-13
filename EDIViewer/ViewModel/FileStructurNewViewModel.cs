using System.IO;
using Newtonsoft.Json;

using EDIViewer.Helper;
using EDIViewer.Models;
using EDIViewer.ViewModel.Common;

namespace EDIViewer.ViewModel
{
    public class FileStructurNewViewModel
    {
        public string FormatName { get; set; }
        public int FormatVersion { get; set; }
        public string FormatSeparator { get; set; }
        public string FormatDetection { get; set; }
        public string FormatVariation { get; set; }
        public FileStructur fileStructur { get; set; }
        public SimpleRelayCommand SaveCommand { get; set; }
        public FileStructurNewViewModel() 
        {
            this.SaveCommand = new SimpleRelayCommand(x => this.SaveFile());
        }
        public void SaveFile()
        {
            //Aus den Informationen das Objekt für die Neue Datei erstellen
            fileStructur = new()
            {
                FormatName = this.FormatName,
                FormatVersion = this.FormatVersion,
                FormatSeparator = this.FormatSeparator,
                FormatDetection = this.FormatDetection,
                FormatVariation = this.FormatVariation,
                FormatTypes = []
            };
            
            //Inhalt und Dateipfad mit passenden Namen erstellen
            string output = JsonConvert.SerializeObject(fileStructur);
            string formatFilePath = Path.Combine(FormatFiles.LoadCurrentFormatFolderPath(), fileStructur.FormatName + "_" + fileStructur.FormatVariation + ".json");
            File.WriteAllText(formatFilePath, output);
        }
    }
}