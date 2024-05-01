using System.Collections.ObjectModel;

using EDIViewer.Models;
using EDIViewer.ViewModel.Common;

namespace EDIViewer.ViewModel
{
    public class ArtDefinationViewModel
    {
        public ObservableCollection<ArtDefination> ArtDefinations { get; set; }
        public string currentFieldDefinition { get; set; }
        
        public event EventHandler Save;
        public SimpleRelayCommand SaveCommand { get; set; }
        public ArtDefinationViewModel()
        {
            this.SaveCommand = new SimpleRelayCommand(x => this.Save(this, new EventArgs()));
        }
    }
}