namespace EDIViewer.Models
{
    public class FileStructur //Format Informationen
    {
        public int Version { get; set; }
        public string FormatName { get; set; }
        public string Separator { get; set; } //Trennzeichen
        public List<FormatType> FormatType { get; set; }
        public List<ArtDefination> ArtDefination { get; set; }

    }
    public class FormatType //Format Typ -> Entl, Status, Sendung
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RecordType> RecordType { get; set; }
        
    }
    public class RecordType //Satzarten
    {
        public int Position { get; set; } //Satzart Name
        public string Name { get; set; } //Satzart Name
        public List<FieldDefination> FieldDefination { get; set; }
    }
    public class FieldDefination //Felder im Satz
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string DataType { get; set; }
    }
    public class ArtDefination //Angabe von Nummern Zuordnungen (DFÜ-Texte, TextArt)
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}