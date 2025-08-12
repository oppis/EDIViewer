using System.Collections.ObjectModel;

namespace EDIViewer.Models
{
    /// <summary>
    /// Aufbau des Objekts
    /// </summary>
    public class ContentInformation
    {
        //Für Tabelle mit allen Informationen -> Satzart, Feld, Inhalt -> Mehere Tabellen nach Satzart getrennt -> foreach -> Flexibel
        public required ObservableCollection<RawInformation> RawInformations { get; set; }
        public ObservableCollection<ObservableCollection<RawInformation>> RawInformationOrder { get; set; }
        public ObservableCollection<ObservableCollection<RawInformation>> RawInformationPosition { get; set; }
        public Dictionary<string,string> TransferInformation { get; set; }
        public ObservableCollection<Dictionary<string, string>> OrderInformations { get; set; }
        public ObservableCollection<Dictionary<string, string>> StatusInformations { get; set; }
        public ObservableCollection<Dictionary<string, string>> PositionInformations { get; set; }
    }
    /// <summary>
    /// Informationen zur Übertragung  -> Auch bei ContentInformationViewModel anpassen
    /// </summary>
    public class TransferInformation
    {
        public string DataType { get; set; }
        public string DateTime { get; set; }
        public string DataReference { get; set; }
        public string SenderID { get; set; }
        public string RecipientID { get; set; }
    }
    /// <summary>
    /// Informationen zum Auftrag  -> Auch bei ContentInformationViewModel anpassen
    /// </summary>
    public class OrderInformation
    {
        public string IdOrder { get; set; }
        public string Reference { get; set; }
        public string DateTimeLoadDat { get; set; }
        public string DateTimeUnloadDat { get; set; }
    }
    /// <summary>
    /// Informationen zur Position -> Auch bei ContentInformationViewModel anpassen
    /// </summary>
    public class PositionInformation
    {
        public string IdOrder { get; set; }
        public int IdPosition { get; set; }
        public string PackagingUnit { get; set; }
        public int PackageCount { get; set; }
        public string SSCC { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
    }
    /// <summary>
    /// Informationen zum Satus  -> Auch bei ContentInformationViewModel anpassen
    /// </summary>
    public class StatusInformation
    {
        public double OrderNo { get; set; }
        public double Nve { get; set; }
        public int Code { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Notes {  get; set; }
    }
    /// <summary>
    /// Alle Informationen aus der Datei gesamt
    /// </summary>
    public class RawInformation
    {
        public string RecordTyp { get; set; }
        public string Field { get; set; }
        public string FieldContent { get; set; }
        public string FieldContentExtended { get; set; }
        public int FileRow { get; set; }
        public string AufNr { get; set; }
        public string PosNr { get; set; }
    }
}