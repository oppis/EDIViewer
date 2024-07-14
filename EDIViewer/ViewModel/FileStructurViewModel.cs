using System.ComponentModel;
using System.IO;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

using EDIViewer.Models;
using EDIViewer.Helper;

namespace EDIViewer.ViewModel
{
    public class FileStructurViewModel : INotifyPropertyChanged
    {
        private static FileStructur fileStructurModel;
        public FormatTypNewViewModel FormatTypNewViewModel { get; set; }
        public ArtDefinationViewModel ArtDefinationViewModel { get; set; }
        public string currentFileFormatFile;
        private StreamReader textStream;
        public FileStructurViewModel(string currentFileFormat)
        {
            currentFileFormatFile = currentFileFormat;

            textStream = File.OpenText(currentFileFormatFile);

            fileStructurModel = JsonConvert.DeserializeObject<FileStructur>(textStream.ReadToEnd());

            //Anlage neuer Format Typ
            FormatTypNewViewModel = new FormatTypNewViewModel();
            FormatTypNewViewModel.Save += FormatTypeNewOnSave;

            //Verwaltung ArtDefinitionen
            ArtDefinationViewModel = new ArtDefinationViewModel();
            ArtDefinationViewModel.Save += ArtDefinitionOnSave;
        }
        /// <summary>
        /// Speichern der Informationen aus der Maske in das Format File View Model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void FormatTypeNewOnSave(object sender, EventArgs eventArgs)
        {
            //Neuen Format Typ in Liste hinzufügen
            FormatTypes.Add(new FormatType()
            {
                Name = FormatTypNewViewModel.Name,
                Description = FormatTypNewViewModel.Description,
                Detection = FormatTypNewViewModel.Detection
            });
            
            //Leeren der Felder in der View zur Anlage vom Format Typ
            FormatTypNewViewModel.Reset();
        }

        /// <summary>
        /// Speichern der Informationen aus der Maske in das Format File View Model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ArtDefinitionOnSave(object sender, EventArgs eventArgs)
        {
            SelectedFieldDefination.ArtDefinations = ArtDefinationViewModel.ArtDefinations;
        }

        /// <summary>
        /// Aktuelle Datei schließen
        /// </summary>
        public void CloseCurrentFile()
        {
            if (textStream is not null)
            {
                textStream.Close();
            }
        }
        /// <summary>
        /// Speichern der Anpassungen ins JSON Datei
        /// </summary>
        public void SaveFile()
        {
            CloseCurrentFile();   
            string output = JsonConvert.SerializeObject(fileStructurModel);

            string formatFilePath = Path.Combine(FormatFiles.LoadCurrentFormatFolderPath(), fileStructurModel.FormatName + "_" +  fileStructurModel.FormatVariation + ".JSON");
            File.WriteAllText(formatFilePath, output);
        }
        /// <summary>
        /// Letztes Änderungsdatum der Datei Anzeigen
        /// </summary>
        public string LastChange
        {
            get 
            {
                return File.GetLastWriteTime(currentFileFormatFile).ToString();
            }
        }
        public int FormatVersion
        {
            get => fileStructurModel.FormatVersion;
            set
            {
                fileStructurModel.FormatVersion = value;
                OnPropertyChanged(nameof(Version));
            }
        }
        public string FormatName
        {
            get => fileStructurModel.FormatName;
            set {
                fileStructurModel.FormatName = value;
                OnPropertyChanged(nameof(FormatName));
            }
        }
        public string FormatSeparator
        {
            get => fileStructurModel.FormatSeparator;
            set
            {
                fileStructurModel.FormatSeparator = value;
                OnPropertyChanged(nameof(FormatSeparator));
            }
        }
        public string FormatDetection
        {
            get => fileStructurModel.FormatDetection;
            set
            {
                fileStructurModel.FormatDetection = value;
                OnPropertyChanged(nameof(FormatDetection));
            }
        }
        public string FormatVaritaion
        {
            get => fileStructurModel.FormatVariation;
            set
            {
                fileStructurModel.FormatVariation = value;
                OnPropertyChanged(nameof(FormatVaritaion));
            }
        }
        //Inhalt darstellen
        public ObservableCollection<FormatType> FormatTypes
        {
            get => fileStructurModel.FormatTypes;
            set
            {
                fileStructurModel.FormatTypes = value;
                OnPropertyChanged(nameof(FormatTypes));
            }
        }
        //reagieren auf das ausgewählten Typ
        private FormatType selectedFormatType;
        public FormatType SelectedFormatType
        {
            get
            {
                return selectedFormatType;
            }
            set
            {
                selectedFormatType = value;
                OnPropertyChanged(nameof(SelectedFormatType));

                //Wenn keine RecordTypes vorhanden sind dann ein Leeres anlegen
                if (RecordTypes is null && selectedFormatType is not null)
                {
                    selectedFormatType.RecordTypes = [];
                }
                //Laden der Record Types
                OnPropertyChanged(nameof(RecordTypes));
            }
        }
        //Inhalt darstellen
        public ObservableCollection<RecordType> RecordTypes
        {
            get
            {
                if (selectedFormatType is not null)
                {
                    return selectedFormatType.RecordTypes;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                selectedFormatType.RecordTypes = value;
                OnPropertyChanged(nameof(RecordTypes));
            }
        }
        //reagieren auf das ausgewählten Typ
        private RecordType selectedRecordType;
        public RecordType SelectedRecordType
        {
            get
            { 
                return selectedRecordType; 
            } 
            set 
            {
                selectedRecordType = value;
                OnPropertyChanged(nameof(SelectedRecordType));

                //Wenn keine Feld Definitionen vorhanden sind dann ein Leeres anlegen
                if (FieldDefinations is null && selectedRecordType is not null) 
                {
                    selectedRecordType.FieldDefinations = [];
                }
                //Laden der Feld Definitionen
                OnPropertyChanged(nameof(FieldDefinations));
            }
        }
        //Inhalt darstellen
        public ObservableCollection<FieldDefination> FieldDefinations
        {
            get
            {
                if (selectedRecordType is not null)
                {
                    return selectedRecordType.FieldDefinations;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                selectedRecordType.FieldDefinations = value;
                OnPropertyChanged(nameof(FieldDefinations));
            }
        }
        private FieldDefination selectedFieldDefination;
        public FieldDefination SelectedFieldDefination
        {
            get
            {
                return selectedFieldDefination;
            }
            set
            {
                selectedFieldDefination = value;

                if (selectedFieldDefination is not null)
                {
                    ArtDefinationViewModel.currentFieldDefinition = selectedFieldDefination.Name;

                    if (selectedFieldDefination.ArtDefinations is null)
                    {
                        ArtDefinationViewModel.ArtDefinations = [];
                    }
                    else
                    {
                        ArtDefinationViewModel.ArtDefinations = selectedFieldDefination.ArtDefinations;
                    }
                }
            
                OnPropertyChanged(nameof(SelectedFieldDefination));
            }
        }

        public static ObservableCollection<string> TransferInformationNames
        {
            get
            {
                List<string> values = typeof(TransferInformation).GetProperties().Select(p => p.Name).ToList();
                values.Add("");
                values.Sort();
                ObservableCollection<string> collection = new(values);

                return collection;
            }
        }
        public ObservableCollection<string> OrderInformationNames
        {
            get
            {
                List<string> values = typeof(OrderInformation).GetProperties().Select(p => p.Name).ToList();
                values.Add("");
                values.Sort();
                ObservableCollection<string> collection = new(values);

                return collection;
            }
        }
        public static ObservableCollection<string> PositionInformationNames
        {
            get
            {
                List<string> values = typeof(PositionInformation).GetProperties().Select(p => p.Name).ToList();
                values.Add("");
                values.Sort();
                ObservableCollection<string> collection = new(values);

                return collection;
            }
        }
        public static ObservableCollection<string> StatusInformationNames
        {
            get
            {
                List<string> values = typeof(StatusInformation).GetProperties().Select(p => p.Name).ToList();
                values.Add("");
                values.Sort();
                ObservableCollection<string> collection = new(values);

                return collection;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}