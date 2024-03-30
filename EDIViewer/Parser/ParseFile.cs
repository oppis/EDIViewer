using Newtonsoft.Json;

using EDIViewer.Models;
using System.Collections.ObjectModel;

namespace EDIViewer.Parser
{
    class ParseFile
    {
        //Datei Struktur für aktuelles Format
        FileStructur fileStructur;
        //Aktuelle Satzart zwischen speichern
        ObservableCollection<RecordType> fileRecordTypes;
        //Aktuelle Felder zwischen speichern
        ObservableCollection<FieldDefination> fieldDefs;
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

            TransferInformation transferInformation = new();
            List<RawInformation> rawInformations = [];

            //Erste Zeile einlesen -> Prüfen welcher Formattyp genutzt wird
            //Prüfung was für ein Format Typ
            foreach (FormatType formatType in fileStructur.FormatTypes)
            {
                if (file[0].Contains(formatType.Detection))
                {
                    //Speichern des aktuellen FormatTyp
                    fileRecordTypes = formatType.RecordTypes;

                    //Speichern der Übertragung Informationen
                    transferInformation = new()
                    {
                        DataType = formatType.Description
                    };
                }
            }

            //Berücksichtigen ob Trennzeichen oder Feldlänge
            if (fileStructur.FormatSeparator is not null && fileStructur.FormatSeparator.Length > 0)
            {
                char seperator = fileStructur.FormatSeparator[0];

                //Aktuelle Zeile 
                int fileRowIndex = 0;

                foreach (string fileRow in file)
                {
                    fileRowIndex++;

                    // Es handelt sich um eine CSV Datei
                    currentRecord = fileRow.Split([seperator]);
                    foreach (RecordType recordType in fileRecordTypes)
                    {
                        if (currentRecord[0].StartsWith(recordType.RecordDetection))
                        {
                            fieldDefs = recordType.FieldDefinations;

                            for (int i = 0; i < currentRecord.Length; i++)
                            {
                                RawInformation currentRawInformation = new()
                                {
                                    RecordTyp = recordType.Name,
                                    Field = fieldDefs[i].Name,
                                    FieldContent = currentRecord[i],
                                    FileRow = fileRowIndex
                                };

                                rawInformations.Add(currentRawInformation);
                            }
                        }
                    }
                }
            }
            else
            {
                //Aktuelle Zeile 
                int fileRowIndex = 0;

                //Weitere Zielen ermitteln und prüfen
                foreach (string fileRow in file)
                {
                    fileRowIndex++;

                    //Jede Satzart durchgehen
                    foreach (RecordType recordType in fileRecordTypes)
                    {
                        //Prüfen welcher Record Typ genutzt wird
                        if (fileRow.StartsWith(recordType.RecordDetection))
                        {
                            //Setzen der aktuellen Felder
                            fieldDefs = recordType.FieldDefinations;

                            //Alle Felder Definitionen durchgehen
                            foreach (FieldDefination fieldDefination in fieldDefs)
                            {
                                //Prüfen ob Start noch in der Zeile vorhanden ist
                                if ((fieldDefination.Start - 1) <= fileRow.Length)
                                {
                                    //Aktuelle Länge speichern
                                    int end = fieldDefination.Length;
                                    
                                    //Position vom Ende bestimmen
                                    int position = (fieldDefination.Start - 1) + fieldDefination.Length;

                                    //Prüfen ob die End Position über die Zeile hinaus geht
                                    if (position >= fileRow.Length)
                                    {
                                        //Bis zum Zeilen Ende gehen   
                                        end = fileRow.Length - (fieldDefination.Start - 1);
                                    }

                                    RawInformation currentRawInformation = new()
                                    {
                                        RecordTyp = recordType.Name,
                                        Field = fieldDefination.Name,
                                        FieldContent = fileRow.Substring(fieldDefination.Start - 1, end),
                                        FileRow = fileRowIndex
                                    };

                                    rawInformations.Add(currentRawInformation);
                                }
                            }
                        }
                    }
                }
            }

            //Gefundene Informationen in übergabe Objekt speichern
            contentInformation = new()
            {
                TransferInformation = transferInformation,
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