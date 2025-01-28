using System.Collections.ObjectModel;

using EDIViewer.Models;
using EDIViewer.ViewModel.Common;

namespace EDIViewer.ViewModel
{
    public class ArtDefinationViewModel
    {
        public ObservableCollection<ArtDefination> ArtDefinations { get; set; }
        public string CurrentFieldDefinition { get; set; }
        
        public event EventHandler Save;
        public event EventHandler Cancel;
        public SimpleRelayCommand SaveCommand { get; set; }
        public SimpleRelayCommand CancelCommand { get; set; }
        public ArtDefinationViewModel()
        {
            this.SaveCommand = new SimpleRelayCommand(x => this.Save(this, new EventArgs()));
            this.CancelCommand = new SimpleRelayCommand(x => this.Cancel(this, new EventArgs()));
        }
    }
}