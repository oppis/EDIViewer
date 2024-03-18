using Newtonsoft.Json;

using EDIViewer.Models;

namespace EDIViewer.Parser
{
    class ParseFile
    {
        //Datei Struktur für aktuelles Format
        FileStructur fileStructur;
        //Aktuelle Satzart zwischen speichern
        List<RecordType> fileRecordTypes;
        //Aktuelle Felder zwischen speichern
        List<FieldDefination> fieldDefs;
        //Ausgabe Objekt
        public ContentInformation contentInformation;
        /// <summary>
        /// Prüfen aktuelle File Struktur
        /// </summary>
        public void GetFileStructur(string currentFileStructur)
        {
            //aktuelle Format Definition aus JSON laden
            string json = System.IO.File.ReadAllText(currentFileStructur);

            fileStructur = JsonConvert.DeserializeObject<FileStructur>(json);
        }
        /// <summary>
        /// Aktuelle Linie ermitteln
        /// </summary>
        public void ProcessCurrentFile(string[] file)
        {
            string[] currentRecord = null;

            List<TransferInformation> transferInformations = [];
            List<RawInformation> rawInformations = [];

            //Erste Zeile einlesen -> Prüfen welcher Formattyp genutzt wird
            //Prüfung was für ein Format Typ
            foreach (FormatType formatType in fileStructur.FormatType)
            {
                if (file[0].Contains(formatType.Name))
                {
                    //Speichern des aktuellen FormatTyp
                    fileRecordTypes = formatType.RecordType;

                    //Speichern der Übertragung Informationen
                    TransferInformation transferInformation = new()
                    {
                        DataType = formatType.Description
                    };
                    transferInformations.Add(transferInformation);
                }
            }

            //Berücksichtigen ob Trennzeichen oder Feldlänge
            if (fileStructur.Separator.Length > 0)
            {
                char seperator = fileStructur.Separator[0];
                foreach (string fileRow in file)
                {
                    // Es handelt sich um eine CSV Datei
                    currentRecord = fileRow.Split([seperator]);
                }
            }
            else
            {
                //Weitere Zielen ermitteln und prüfen
                foreach (string fileRow in file)
                {
                    //Jede Satzart durchgehen
                    foreach (RecordType recordType in fileRecordTypes)
                    {
                        //Prüfen welcher Record Typ genutzt wird
                        if (fileRow.StartsWith(recordType.RecordDetection))
                        {
                            //Setzen der aktuellen Felder
                            fieldDefs = recordType.FieldDefination;

                            //Alle Felder Definitionen durchgehen
                            foreach (FieldDefination fieldDefination in fieldDefs)
                            {
                                RawInformation currentRawInformation = new()
                                {
                                    RecordTyp = recordType.Name,
                                    Field = fieldDefination.Name,
                                    FieldContent = fileRow.Substring(fieldDefination.Start - 1, fieldDefination.Length)
                                };

                                rawInformations.Add(currentRawInformation);
                            }
                        }
                    }
                }
            }

            //Gefundene Informationen in übergabe Objekt speichern
            contentInformation = new()
            {
                TransferInformation = transferInformations,
                RawInformation = rawInformations
            };  
        }
        /// <summary>
        /// Aktuellen Format Typ ermitteln -> Verfügbar machen für Datei Load > Vorschlag
        /// </summary>
        public void GetFormatType()
        { 
        
        }
        /// <summary>
        /// Aktuellen RecordType ermitteln
        /// </summary>
        private void ProcessCurrentRecord()
        {

        }
    }
}