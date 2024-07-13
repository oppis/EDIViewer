using System.Collections.ObjectModel;

namespace EDIViewer.Models
{
    public class FileStructur //Format Informationen
    {
        public required int FormatVersion {  get; set; }
        public required string FormatName { get; set; }
        public string FormatSeparator { get; set; } //Trennzeichen
        public required string FormatDetection { get; set; }
        public string FormatVariation { get; set; } //Abwandelungen
        public required ObservableCollection<FormatType> FormatTypes { get; set; }
    }
    public class FormatType //Format Typ -> Entl, Status, Sendung
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Detection { get; set; }
        public int OrderSeparatorStart { get ; set; } //Trennung Einheit -> z.B. mehrer Aufträge -> Anfang in Zeile
        public int OrderSeparatorLength { get ; set; } //Trennung Einheit -> z.B. mehrer Aufträge -> Länge
        public int PostionSeparatorStart { get; set; } //Trennung Einheit -> z.B. mehrer Positionen -> Anfang in Zeile
        public int PostionSeparatorLength { get; set; } //Trennung Einheit -> z.B. mehrer Positionen -> Länge
        public ObservableCollection<RecordType> RecordTypes { get; set; }
        
    }
    public class RecordType //Satzarten
    {
        public required int Position { get; set; } //Reihenfolge der Sartarten
        public required string Name { get; set; } //Satzart Name
        public bool Mandatory { get; set; }
        public required string RecordDetection { get; set; }
        public string Description { get; set; }
        public bool PositionTyp { get; set; } //Markierung ob Positions Information
        public ObservableCollection<FieldDefination> FieldDefinations { get; set; }
    }
    public class FieldDefination //Felder im Satz
    {
        public int Position { get; set; }
        public required string Name { get; set; }
        public required int Start { get; set; }
        public required int Length { get; set; }
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
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; }
    }
}