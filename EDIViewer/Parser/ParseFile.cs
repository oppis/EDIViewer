using Newtonsoft.Json;
using System.Collections.ObjectModel;

using EDIViewer.Models;
using System.Windows.Controls;

namespace EDIViewer.Parser
{
    class ParseFile
    {
        //Original Datei
        string[] origFile;

        //Datei Struktur für ausgewähltes Format
        FileStructur fileStructur;

        //Aktuelle Definitionen zwischen Speichern
        FormatType currentFormatType;
        ObservableCollection<RecordType> currentRecordTypes;
        RecordType currentRecordType;
        ObservableCollection<FieldDefination> currentFieldDefiniations;
        string[] currentFileRowArray;

        //Ausgabe Objekte
        public ContentInformation contentInformation;
        TransferInformation transferInformation = new();

        //TODO -> Liste für jeweils neuen Datensatz -> Neuer Auftrag / Statusmeldung -> Markierung  in FormatManagement
        ObservableCollection<RawInformation> rawInformations = []; 
        ObservableCollection<RawInformation>[] rawInformationEntity = [];
        ObservableCollection<RawInformation> rawInformationEntityTmp = [];

        /// <summary>
        /// Aktuelle File Struktur als Objekt laden aus JSON Datei
        /// </summary>
        public void GetFileStructur(string currentFileStructur)
        {
            //aktuelle Format Definition aus JSON laden
            string json = System.IO.File.ReadAllText(currentFileStructur);

            fileStructur = JsonConvert.DeserializeObject<FileStructur>(json);
        }

        /// <summary>
        /// Aktuellen Formattyp und Recordtypes ermitteln //TODO -> Verfügbar machen für Datei Load > Vorschlag
        /// </summary>
        private void GetFormatType()
        {
            //Erste Zeile einlesen -> Prüfen welcher Formattyp genutzt wird
            foreach (FormatType formatType in fileStructur.FormatTypes)
            {
                if (origFile[0].Contains(formatType.Detection)) //TODO -> Angabe im FormatManagement Wo die Information zu finden ist. -> Vielleicht in dem RecordType angeben.
                {
                    //Setzen des aktuellen Format Typs
                    currentFormatType = formatType;

                    //Setzen der aktuellen RecordTypes
                    currentRecordTypes = formatType.RecordTypes;

                    //Speichern der Übertragung Informationen
                    transferInformation.DataType = formatType.Description;
                }
            }
        }
        
        /// <summary>
        /// Aktuelle Feld Defition zum aktuellen Record Typ erhalten
        /// </summary>
        /// <param name="currentFileRow">Aktuelle Zeile</param>
        private void GetCurrentFieldDefinations(string currentFileRow)
        {
            if (!string.IsNullOrEmpty(fileStructur.FormatSeparator))// Es handelt sich um eine CSV Datei
            {
                foreach (RecordType recordType in currentRecordTypes)
                {
                    currentFileRowArray = currentFileRow.Split([fileStructur.FormatSeparator[0]]);

                    if (currentFileRowArray[0].StartsWith(recordType.RecordDetection))
                    {
                        currentRecordType = recordType;
                        currentFieldDefiniations = recordType.FieldDefinations;
                    }
                }
            }
            else
            {
                foreach (RecordType recordType in currentRecordTypes)
                {
                    //Prüfen welcher Record Typ genutzt wird
                    if (currentFileRow.StartsWith(recordType.RecordDetection))
                    {
                        //Setzen der aktuellen Felder
                        currentRecordType = recordType;
                        currentFieldDefiniations = recordType.FieldDefinations;
                    }
                }
            }
        }

        /// <summary>
        /// Aktuelle Linie ermitteln
        /// </summary>
        public void ProcessCurrentFile(string[] file)
        {
            origFile = file;   

            //Aktuellen Formarmtyp bestimmen
            GetFormatType();

            string[] currentRecord = null;
           

            bool test_new = false;
            
            //Berücksichtigen ob Trennzeichen oder Feldlänge
            if (!string.IsNullOrEmpty(fileStructur.FormatSeparator))
            {
                //Aktuelle Zeile 
                int fileRowIndex = 0;

                foreach (string fileRow in file)
                {
                    fileRowIndex++;

                    GetCurrentFieldDefinations(fileRow);

                    for (int i = 0; i < currentRecord.Length; i++)
                    {
                        RawInformation currentRawInformation = new()
                        {
                            RecordTyp = currentRecordType.Name,
                            Field = currentFieldDefiniations[i].Name,
                            FieldContent = currentRecord[i],
                            FileRow = fileRowIndex
                        };

                        //Alle Einträge hinzufügen
                        rawInformations.Add(currentRawInformation);
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

                    GetCurrentFieldDefinations(fileRow);

                    //Alle Felder Definitionen durchgehen
                    foreach (FieldDefination fieldDefination in currentFieldDefiniations)
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
                                RecordTyp = currentRecordType.Name,
                                Field = fieldDefination.Name,
                                FieldContent = fileRow.Substring(fieldDefination.Start - 1, end),
                                FileRow = fileRowIndex
                            };

                            rawInformations.Add(currentRawInformation);
                            rawInformationEntityTmp.Add(currentRawInformation);

                            if (fileRow.Substring(fieldDefination.Start - 1, end).Contains("SHP") && fieldDefination.Position == 3)
                            {
                                test_new = true;
                            }

                        }
                        //Liste erstellen mit einzelnen Inhalten
                        if (test_new)
                        {
                            rawInformationEntity = rawInformationEntity.Append(rawInformationEntityTmp).ToArray();
                            rawInformationEntityTmp.Clear();
                            test_new = false;
                        }
                    }

                }
            }

            BuildInfoObject();
        }

        /// <summary>
        /// Erstellen des Rückgabe Objekt
        /// </summary>
        private void BuildInfoObject()
        {
            //Gefundene Informationen in übergabe Objekt speichern
            contentInformation = new()
            {
                TransferInformation = transferInformation,
                RawInformations = rawInformations,
                RawInformationEntity = rawInformationEntity
            };
        }
    }
}