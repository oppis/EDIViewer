using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

using EDIViewer.Models;
using System.Collections.ObjectModel;
using System;
using Microsoft.Win32;

namespace EDIViewer.ViewModel
{
    public class FileStructurVM : INotifyPropertyChanged
    {
        public static FileStructur fileStructurModel;
        public FileStructurVM(string currentFileFormat)
        {
            //string currentFileFormat = Path.Combine(Environment.CurrentDirectory, Path.Combine("Formate", "Fortras100.JSON"));
            string json = File.ReadAllText(currentFileFormat);

            fileStructurModel = JsonConvert.DeserializeObject<FileStructur>(json);
        }
        public void SaveFile()
        {
            string output = JsonConvert.SerializeObject(fileStructurModel);
            string formatFilePath = Path.Combine(Environment.CurrentDirectory, Path.Combine("Formate", fileStructurModel.FormatName + ".JSON"));
            File.WriteAllText(formatFilePath, output);
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

        private FormatType selectedFormatType;
        public List<FormatType> FormatTypes
        {
            get
            {
                return fileStructurModel.FormatTypes;
            }
            set
            {
                fileStructurModel.FormatTypes = value;
                OnPropertyChanged(nameof(FormatTypes));
            }
        }
        
        public FormatType SelectedFormatType
        {
            get
            {
                return this.selectedFormatType;
            }
            set
            {
                this.selectedFormatType = value;
                OnPropertyChanged(nameof(SelectedFormatType));
                OnPropertyChanged(nameof(RecordTypes));
            }
        }
        public List<RecordType> RecordTypes
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
        private RecordType selectedRecordType;
        public RecordType SelectedRecordType
        {
            get
            { 
                return this.selectedRecordType; 
            } 
            set 
            {
                this.selectedRecordType = value;
                OnPropertyChanged(nameof(SelectedRecordType));
                OnPropertyChanged(nameof(FieldDefinations));
            }
        }
        public List<FieldDefination> FieldDefinations
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}