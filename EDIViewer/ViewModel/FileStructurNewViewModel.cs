using System.IO;
using System.ComponentModel;
using Newtonsoft.Json;

using EDIViewer.Helper;
using EDIViewer.Models;
using EDIViewer.ViewModel.Common;

namespace EDIViewer.ViewModel
{
    public class FileStructurNewViewModel: IDataErrorInfo
    {
        public string FormatName { get; set; }
        public string FormatComment { get; set; }
        public int FormatVersion { get; set; }
        public string FormatSeparator { get; set; }
        public string FormatDetection { get; set; }
        public string FormatVariation { get; set; }
        public FileStructur fileStructur { get; set; }
        public SimpleRelayCommand SaveCommand { get; set; }
        public static bool checkSaveStatus = false;
        public FileStructurNewViewModel() 
        {
            this.SaveCommand = new SimpleRelayCommand(x => this.SaveFile());
        }

        public string Error {  get; }
        public string this[string propertyName]
        {
            get 
            {
                string msg = null;
                switch (propertyName)
                {
                    case "FormatName":
                        if (String.IsNullOrEmpty(FormatName))
                        {
                            msg = "Format Name ist erforderlich";
                            checkSaveStatus = false;
                        }
                        else
                        {
                            checkSaveStatus = true;
                        }
                        break;
                    case "FormatDetection":
                        if (String.IsNullOrEmpty(FormatDetection))
                        {
                            msg = "Format Erkennung ist erforderlich";
                            checkSaveStatus = false;
                        }
                        else
                        { 
                            checkSaveStatus = true;
                        }
                        break;
                    case "FormatVariation":
                        if (String.IsNullOrEmpty(FormatVariation))
                        {
                            msg = "Format Variation ist erforderlich";
                            checkSaveStatus = false;
                        }
                        else
                        {
                            checkSaveStatus = true;
                        }
                        break;
                    default:
                        break;
                }
                return msg;
            }
        }
           
        public void SaveFile()
        {
            //Aus den Informationen das Objekt für die Neue Datei erstellen
            fileStructur = new()
            {
                FormatName = this.FormatName,
                FormatComment = this.FormatComment,
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