using EDIViewer.ViewModel.Common;

namespace EDIViewer.ViewModel
{
    public class FormatTypNewViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Detection { get; set; }

        public event EventHandler Save;
        public SimpleRelayCommand SaveCommand { get; set; }
        public FormatTypNewViewModel()
        {
            this.SaveCommand = new SimpleRelayCommand(x => this.Save(this, new EventArgs()));
        }
        //Leeren der Felder  wird in MainViewModel ausgeführt bei Event
        public void Reset()
        {
            this.Name = string.Empty;
            this.Description = string.Empty;     
            this.Detection = string.Empty;
        }
    }
}