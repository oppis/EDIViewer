using System.Collections.ObjectModel;
using System.ComponentModel;

using EDIViewer.Models;
using EDIViewer.Parser;

namespace EDIViewer.ViewModel
{
    public class ContentInformationViewModel : INotifyPropertyChanged
    {
        private static ContentInformation contentInformation;
        public ContentInformationViewModel(string currentFileFormat, string[] viewLines)
        {
            ParseFile parseFile = new();
            parseFile.GetFileStructur(currentFileFormat);
            parseFile.ProcessCurrentFile(viewLines);

            contentInformation = parseFile.contentInformation;
        }
        public ContentInformationViewModel() 
        {
            RawInformations = null;
            RawInformationEntity = null;
            TransferInformation = null;
        }

        public ObservableCollection<RawInformation> RawInformations
        {
            get
            {
                return contentInformation.RawInformations;
            }
            set
            {
                contentInformation.RawInformations = value;
                OnPropertyChanged(nameof(RawInformations));
            }
        }
        public List<List<RawInformation>> RawInformationEntity
        {
            get
            {
                return contentInformation.RawInformationEntity;
            }
            set
            {
                contentInformation.RawInformationEntity = value;
                OnPropertyChanged(nameof(RawInformationEntity));
            }
        }
        public TransferInformation TransferInformation
        {
            get
            {
                TransferInformation transferInformation = new();

                if (contentInformation != null)
                {
                    //Parsen des Dictionary in Model
                    foreach (var item in contentInformation.TransferInformation)
                    {
                        switch (item.Key)
                        {
                            case "DataType":
                                transferInformation.DataType = item.Value;
                                break;
                            case "DateTime":
                                transferInformation.DateTime = item.Value;
                                break;
                            case "SenderID":
                                transferInformation.SenderID = item.Value;
                                break;
                            case "RecipientID":
                                transferInformation.RecipientID = item.Value;
                                break;
                            default:
                                break;
                        }
                    }
                }
                
                return transferInformation;
            }
            set
            {
                contentInformation.TransferInformation = null;
                OnPropertyChanged(nameof(TransferInformation));
            }
        }

        //Event 
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}