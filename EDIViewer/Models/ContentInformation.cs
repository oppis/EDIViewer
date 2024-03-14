namespace EDIViewer.Models
{
    /// <summary>
    /// Aufbau des Objekts
    /// </summary>
    public class ContentInformation
    {
        //Für Tabelle mit allen Informationen -> Satzart, Feld, Inhalt -> Mehere Tabellen nach Satzart getrennt -> foreach -> Flexibel
        public List<RawInformation> RawInformation { get; set; }
        public List<TransferInformation> TransferInformation { get; set; }
        public List<OrderInformation> OrderInformation { get; set; }
        public List<StatusInformation> StatusInformation { get; set; }
    }
    /// <summary>
    /// Information welche Tabs aktiviert werden
    /// </summary>
    public class TabConfig
    {

    }
    /// <summary>
    /// Informationen zur Übertragung
    /// </summary>
    public class TransferInformation
    {
        public string DataType { get; set; }
        public DateTime DateTime { get; set; }
        public string DataReference { get; set; }
        public string SenderID { get; set; }
        public string RecipientID { get; set; }
    }
    /// <summary>
    /// Informationen zum Auftrag
    /// </summary>
    public class OrderInformation
    {
        public string Reference { get; set; }
        public DateTime DateTimeLoadDat { get; set; }
        public DateTime DateTimeUnloadDat { get; set; }
        public List<Item> Items { get; set; }
    }
    /// <summary>
    /// Informationen zur Position
    /// </summary>
    public class Item
    {
        public int Position { get; set; }
        public string PackagingUnit { get; set; }
    }
    /// <summary>
    /// Informationen zum Satus
    /// </summary>
    public class StatusInformation
    {
        public int StatusID { get; set; }
        public DateTime StatusDateTime { get; set; }
        public string StatusNotes {  get; set; }
    }
    /// <summary>
    /// Alle Informationen aus der Datei
    /// </summary>
    public class RawInformation
    {
        public string RecordTyp { get; set; }
        public string Field { get; set; }
        public string FieldContent { get; set; }
        public int FileRow {  get; set; }
    }
}