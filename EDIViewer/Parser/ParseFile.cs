using Newtonsoft.Json;
using System.Collections.ObjectModel;

using EDIViewer.Models;
using System.Security.RightsManagement;
using EDIViewer.Helper;

namespace EDIViewer.Parser
{
    class ParseFile
    {
        //Datei Inahlt
        string[] origFile;
        string[] currentFileRowArray;
        string currentAufNr = string.Empty;

        //Datei Struktur für ausgewähltes Format
        FileStructur fileStructur;

        //Aktuelle Definitionen zwischen Speichern
        FormatType currentFormatType;
        ObservableCollection<RecordType> currentRecordTypes;
        RecordType currentRecordType;
        ObservableCollection<FieldDefination> currentFieldDefiniations;

        //Ausgabe Objekte
        public ContentInformation contentInformation;
        readonly TransferInformation transferInformation = new();
        readonly ObservableCollection<RawInformation> rawInformations = [];
        readonly List<List<RawInformation>> rawInformationEntity = [];
        List<RawInformation> rawInformationEntityTmp = [];

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
        private bool GetFormatType()
        {
            bool status = false;
            
            //Erste Zeile einlesen -> Prüfen welcher Formattyp genutzt wird
            foreach (FormatType formatType in fileStructur.FormatTypes)
            {
                if (origFile[0].Contains(formatType.Detection))
                {
                    //Setzen des aktuellen Format Typs
                    currentFormatType = formatType;

                    //Setzen der aktuellen RecordTypes
                    currentRecordTypes = formatType.RecordTypes;

                    //Speichern der Übertragung Informationen
                    transferInformation.DataType = formatType.Description;

                    status = true;
                }
            }

            return status;
        }
        
        /// <summary>
        /// Aktuelle Feld Defition zum aktuellen Record Typ erhalten
        /// </summary>
        /// <param name="currentFileRow">Aktuelle Zeile</param>
        private bool GetCurrentFieldDefinations(string currentFileRow)
        {
            bool status = false;
            
            if (!string.IsNullOrEmpty(fileStructur.FormatSeparator))// Es handelt sich um eine CSV Datei
            {
                foreach (RecordType recordType in currentRecordTypes)
                {
                    currentFileRowArray = currentFileRow.Split([fileStructur.FormatSeparator[0]]);

                    if (currentFileRowArray[0].StartsWith(recordType.RecordDetection))
                    {
                        currentRecordType = recordType;
                        currentFieldDefiniations = recordType.FieldDefinations;

                        status = true;
                    }
                }
            }
            else //Datei mit Feldlänge
            {
                foreach (RecordType recordType in currentRecordTypes)
                {
                    //Prüfen welcher Record Typ genutzt wird
                    if (currentFileRow.StartsWith(recordType.RecordDetection))
                    {
                        //Setzen der aktuellen Felder
                        currentRecordType = recordType;
                        currentFieldDefiniations = recordType.FieldDefinations;
                        
                        status = true;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Parsen der Datei
        /// </summary>
        /// <param name="file">Datei -> Array -> Je Zeile</param>
        public void ProcessCurrentFile(string[] file)
        {
            origFile = file;   

            //Aktuellen Formarmtyp bestimmen
            bool status = GetFormatType();

            try
            {

                if (status)
                {
                    //Berücksichtigen ob Trennzeichen oder Feldlänge
                    if (!string.IsNullOrEmpty(fileStructur.FormatSeparator))
                    {
                        //Aktuelle Zeile 
                        int fileRowIndex = 0;

                        foreach (string fileRow in file)
                        {
                            fileRowIndex++;

                            //Prüfung ob Field Definitionen gefunden wurden
                            bool returnStatus = GetCurrentFieldDefinations(fileRow);
                            if (returnStatus)
                            {
                                string currentFieldContent = string.Empty;

                                try
                                {
                                    for (int i = 0; i < (currentFileRowArray.Length - 1); i++)
                                    {
                                        currentFieldContent = currentFileRowArray[i];

                                        string oldAufNr = currentAufNr;
                                        //Ermitteln der aktuellen Auftragsnummer
                                        currentAufNr = currentFileRowArray[currentFormatType.EntitySeparatorStart];

                                        //Neue Liste erstellen wenn neue Einheit kommt
                                        if (oldAufNr != currentAufNr)
                                        {
                                            rawInformationEntityTmp = [];
                                        }

                                        RawInformation currentRawInformation = new()
                                        {
                                            RecordTyp = currentRecordType.Name,
                                            Field = currentFieldDefiniations[i].Name,
                                            FieldContent = currentFileRowArray[i],
                                            FileRow = fileRowIndex,
                                            AufNr = currentAufNr,
                                        };

                                        //Alle Einträge hinzufügen
                                        rawInformations.Add(currentRawInformation);

                                        //Listen mit Gruppierung nach AufNr erstellen
                                        rawInformationEntityTmp.Add(currentRawInformation);

                                        //Informationen in Gruppen Liste speichern wenn sich Einheit ändert
                                        if (oldAufNr != currentAufNr)
                                        {
                                            rawInformationEntity.Add(rawInformationEntityTmp);
                                        }
                                    }
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    UserMessageHelper.ShowMessageBox("Parsen", $"Es wurde die Feld Definition nicht gefunden! \nAktueller Record: {currentRecordType.Name} \nFeld Inhalt: {currentFieldContent}");
                                }
                                catch (Exception ex)
                                {
                                    UserMessageHelper.ShowMessageBox("Parsen", "Folgender Fehler ist beim Lesen der Datei aufgetreten: " + ex.Message);
                                }
                            }
                        }
                    }
                    //Parsen mit Feldlänge
                    else
                    {
                        //Aktuelle Zeile 
                        int fileRowIndex = 0;

                        //Weitere Zielen ermitteln und prüfen
                        foreach (string fileRow in file)
                        {
                            fileRowIndex++;

                            bool returnstatus = GetCurrentFieldDefinations(fileRow);
                            if (returnstatus)
                            {
                                //Alle Felder Definitionen durchgehen
                                foreach (FieldDefination fieldDefination in currentFieldDefiniations)
                                {
                                    //Prüfen ob Feld noch in der Zeile vorhanden ist
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

                                        string oldAufNr = currentAufNr;
                                        //Ermitteln der aktuellen Auftragsnummer
                                        currentAufNr = fileRow.Substring(currentFormatType.EntitySeparatorStart - 1, currentFormatType.EntitySeparatorLength);

                                        //Neue Liste erstellen wenn neue Einheit kommt
                                        if (oldAufNr != currentAufNr)
                                        {
                                            rawInformationEntityTmp = [];
                                        }

                                        //Ausgabe in Rein Form erstellen
                                        RawInformation currentRawInformation = new()
                                        {
                                            RecordTyp = currentRecordType.Name,
                                            Field = fieldDefination.Name,
                                            FieldContent = fileRow.Substring(fieldDefination.Start - 1, end),
                                            FileRow = fileRowIndex,
                                            AufNr = currentAufNr,
                                        };

                                        //Alle Informationen sammeln
                                        rawInformations.Add(currentRawInformation);

                                        //Listen mit Gruppierung nach AufNr erstellen
                                        rawInformationEntityTmp.Add(currentRawInformation);

                                        //Informationen in Gruppen Liste speichern wenn sich Einheit ändert
                                        if (oldAufNr != currentAufNr)
                                        {
                                            rawInformationEntity.Add(rawInformationEntityTmp);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    BuildInfoObject();
                }

            }
            catch (Exception ex)
            {
                UserMessageHelper.ShowMessageBox("Parsen", "Folgender Fehler ist beim Lesen der Datei aufgetreten: " + ex.Message);    
            }
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