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

        public ObservableCollection<RawInformation> RawInformations
        {
            get
            {
                return contentInformation.RawInformations;
            } 
        }
        public List<List<RawInformation>> RawInformationEntity
        {
            get
            {
                return contentInformation.RawInformationEntity;
            }
        }
        public TransferInformation TransferInformation 
        { 
            get
            {
                return contentInformation.TransferInformation;
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