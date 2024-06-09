
using System.Collections.ObjectModel;

namespace EDIViewer.Models
{
    public class FileStructur //Format Informationen
    {
        public int FormatVersion {  get; set; }
        public string FormatName { get; set; }
        public string FormatSeparator { get; set; } //Trennzeichen
        public string FormatDetection { get; set; }
        public string FormatVariation { get; set; } //Abwandelungen
        public ObservableCollection<FormatType> FormatTypes { get; set; }
    }
    public class FormatType //Format Typ -> Entl, Status, Sendung
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Detection { get; set; }
        public int EntitySeparatorStart { get ; set; } //Trennung Einheit -> z.B. mehrer Aufträge -> Anfang in Zeile
        public int EntitySeparatorLength { get ; set; } //Trennung Einheit -> z.B. mehrer Aufträge -> Länge
        public ObservableCollection<RecordType> RecordTypes { get; set; }
        
    }
    public class RecordType //Satzarten
    {
        public int Position { get; set; } //Satzart Name
        public string Name { get; set; } //Satzart Name
        public bool Mandatory { get; set; }
        public string RecordDetection { get; set; }
        public string Description { get; set; }
        public ObservableCollection<FieldDefination> FieldDefinations { get; set; }
    }
    public class FieldDefination //Felder im Satz
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public bool Mandatory { get; set; }
        public string TransferInformation {  get; set; }
        public string OrderInformation {  get; set; }
        public string PositionInformation {  get; set; }
        public string StatusInformation {  get; set; }
        public ObservableCollection<ArtDefination> ArtDefinations { get; set; }
    }
    public class ArtDefination //Angabe von Nummern Zuordnungen (DFÜ-Texte, TextArt)
    { 
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}