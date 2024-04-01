using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

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

            ViewSource.Source = RawInformations;
            ViewSource.GroupDescriptions.Add(new PropertyGroupDescription("RecordTyp"));
        }

        public ObservableCollection<RawInformation> RawInformations
        {
            get
            {
                return contentInformation.RawInformations;
            } 
        }
        public ObservableCollection<RawInformation>[] RawInformationEntity
        {
            get
            {
                return contentInformation.RawInformationEntity;
            }
        }
        public CollectionViewSource ViewSource { get; set; } = new CollectionViewSource();

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