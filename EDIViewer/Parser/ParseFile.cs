﻿using Newtonsoft.Json;
using System.Collections.ObjectModel;

using EDIViewer.Models;
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
        
        readonly ObservableCollection<RawInformation> rawInformations = [];
        readonly ObservableCollection<ObservableCollection<RawInformation>> rawInformationEntity = [];
        ObservableCollection<RawInformation> rawInformationEntityTmp = [];

        Dictionary<string, string> transferInformation = [];
        ObservableCollection<Dictionary<string, string>> orderInformations = [];
        Dictionary<string, string> orderInformation = [];
        ObservableCollection<Dictionary<string, string>> positionInformations = [];
        Dictionary<string, string> positionInformation = [];
        ObservableCollection<Dictionary<string, string>> statusInformations = [];
        Dictionary<string, string> statusInformation = [];

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
                    transferInformation.Add("DataType", formatType.Description);

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
                                    //Alle Felder durchgehen
                                    for (int i = 0; i < (currentFileRowArray.Length - 1); i++)
                                    {
                                        currentFieldContent = currentFileRowArray[i];

                                        //Länge bestimmen für Parsing bis Ende der Zeile
                                        string oldAufNr = currentAufNr;
                                        currentAufNr = currentFileRowArray[currentFormatType.EntitySeparatorStart];

                                        //Prüfen ob neuer Auftrag gebinnt und neue Einheit anlegen
                                        if (oldAufNr != currentAufNr)
                                        {
                                            rawInformationEntityTmp = [];
                                        }
                                         
                                        //Interpretation der Feld Informationen
                                        string fileContent = currentFileRowArray[i];

                                        //Variable für Erweiterte Informationen
                                        string fieldContentExtended = string.Empty;

                                        //ArtDefinitionen einfügen
                                        if (currentFieldDefiniations[i].ArtDefinations is not null)
                                        {
                                            foreach (ArtDefination artDefination in currentFieldDefiniations[i].ArtDefinations)
                                            {
                                                if (fileContent == artDefination.Id)
                                                {
                                                    fieldContentExtended = artDefination.Name;
                                                }
                                            }
                                        }

                                        //Mapping Felder
                                        //Transfer Informationen                           
                                        if (!String.IsNullOrEmpty(currentFieldDefiniations[i].TransferInformation))
                                        {
                                            transferInformation.Add(currentFieldDefiniations[i].TransferInformation, fileContent);
                                        }
                                        //Auftragsinformationen
                                        if (!String.IsNullOrEmpty(currentFieldDefiniations[i].OrderInformation))
                                        {
                                            orderInformation.TryAdd(currentFieldDefiniations[i].OrderInformation, fileContent);
                                        }
                                        //Positionsinformationen
                                        if (!String.IsNullOrEmpty(currentFieldDefiniations[i].PositionInformation)) //TODO -> Trennen nach Position -> Anpassung in FormatManagement
                                        {
                                            positionInformation.TryAdd(currentFieldDefiniations[i].PositionInformation, fileContent);
                                        }
                                        //Statusinformationen
                                        if (!String.IsNullOrEmpty(currentFieldDefiniations[i].StatusInformation))
                                        {
                                            statusInformation.TryAdd(currentFieldDefiniations[i].StatusInformation, fileContent);
                                        }

                                        //Ausgabe Objekt erstellen
                                        RawInformation currentRawInformation = new()
                                        {
                                            RecordTyp = currentRecordType.Name,
                                            Field = currentFieldDefiniations[i].Name,
                                            FieldContent = fileContent,
                                            FieldContentExtended = fieldContentExtended,
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

                                            //Mapping Auftrag in Liste schreiben
                                            if (orderInformation.Count > 0)
                                            {
                                                orderInformations.Add(orderInformation);
                                                orderInformation = [];
                                            }

                                            //Mapping Postion in Liste schreiben
                                            if (positionInformation.Count > 0)
                                            {
                                                positionInformations.Add(positionInformation);
                                                positionInformation = [];
                                            }

                                            //Mapping Satus in Liste schreiben
                                            if (statusInformation.Count > 0)
                                            {
                                                statusInformations.Add(statusInformation);
                                                statusInformation = [];
                                            }
                                        }
                                    }

 
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    UserMessageHelper.ShowMessageBox("Parsen", $"Es wurde die Feld Definition nicht gefunden! \nAktueller Record: {currentRecordType.Name} \nFeld Inhalt: {currentFieldContent}");
                                }
                                catch (Exception ex)
                                {
                                    UserMessageHelper.ShowMessageBox("Parsen", "Folgender Fehler ist beim Lesen der Datei aufgetreten: \n" + ex.Message);
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
                                        //Länge bestimmen für Parsing bis Ende der Zeile
                                        int end = fieldDefination.Length;
                                        int position = (fieldDefination.Start - 1) + fieldDefination.Length;
                                        if (position >= fileRow.Length)
                                        {
                                            //Bis zum Zeilen Ende gehen   
                                            end = fileRow.Length - (fieldDefination.Start - 1);
                                        }

                                        //Prüfen ob neuer Auftrag beginnt und neue Einheit anlegen
                                        string oldAufNr = currentAufNr;

                                        //Prüfen ob Zeile lang genug für Objekt Trennung
                                        if (fileRow.Length > (currentFormatType.EntitySeparatorStart - 1 + currentFormatType.EntitySeparatorLength))
                                        {
                                            currentAufNr = fileRow.Substring(currentFormatType.EntitySeparatorStart - 1, currentFormatType.EntitySeparatorLength);
                                        }
                                        
                                        if (oldAufNr != currentAufNr)
                                        {
                                            rawInformationEntityTmp = [];
                                        }

                                        //Interpretation der Feld Informationen
                                        string fileContent = fileRow.Substring(fieldDefination.Start - 1, end);

                                        //Variable für Erweiterte Informationen
                                        string fieldContentExtended = string.Empty;

                                        //ArtDefinitionen einfügen
                                        if (fieldDefination.ArtDefinations is not null)
                                        {
                                            foreach (ArtDefination artDefination in fieldDefination.ArtDefinations)
                                            {
                                                if (fileContent == artDefination.Id)
                                                {
                                                    fieldContentExtended = artDefination.Name;
                                                }
                                            }
                                        }

                                        //Mapping Felder
                                        //Transfer Informationen                           
                                        if (!String.IsNullOrEmpty(fieldDefination.TransferInformation))
                                        {
                                            transferInformation.Add(fieldDefination.TransferInformation, fileContent);
                                        }
                                        //Auftragsinformationen
                                        if (!String.IsNullOrEmpty(fieldDefination.OrderInformation))
                                        {
                                            orderInformation.TryAdd(fieldDefination.OrderInformation, fileContent);
                                        }
                                        //Positionsinformationen
                                        if (!String.IsNullOrEmpty(fieldDefination.PositionInformation))
                                        {
                                            positionInformation.TryAdd(fieldDefination.PositionInformation, fileContent); //TODO -> Trennen nach Position -> Anpassung in FormatManagement
                                        }
                                        //Statusinformationen
                                        if (!String.IsNullOrEmpty(fieldDefination.StatusInformation))
                                        {
                                            statusInformation.TryAdd(fieldDefination.StatusInformation, fileContent);
                                        }

                                        //Ausgabe in Objekt erstellen
                                        RawInformation currentRawInformation = new()
                                        {
                                            RecordTyp = currentRecordType.Name,
                                            Field = fieldDefination.Name,
                                            FieldContent = fileContent,
                                            FieldContentExtended = fieldContentExtended,
                                            FileRow = fileRowIndex,
                                            AufNr = currentAufNr,
                                        };

                                        //Objekt in Liste sammlen
                                        rawInformations.Add(currentRawInformation);

                                        //Listen mit Gruppierung nach AufNr erstellen
                                        rawInformationEntityTmp.Add(currentRawInformation);

                                        //Informationen in Gruppen Liste speichern wenn sich Einheit ändert
                                        if (oldAufNr != currentAufNr)
                                        {
                                            rawInformationEntity.Add(rawInformationEntityTmp);

                                            //Mapping Auftrag in Liste schreiben
                                            if (orderInformation.Count > 0)
                                            {
                                                orderInformations.Add(orderInformation);
                                                orderInformation = [];
                                            }

                                            //Mapping Postion in Liste schreiben
                                            if (positionInformation.Count > 0)
                                            {
                                                positionInformations.Add(positionInformation);
                                                positionInformation = [];
                                            }

                                            //Mapping Satus in Liste schreiben
                                            if (statusInformation.Count > 0)
                                            {
                                                statusInformations.Add(statusInformation);
                                                statusInformation = [];
                                            }
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
                UserMessageHelper.ShowMessageBox("Parsen", "Folgender Fehler ist beim Lesen der Datei aufgetreten: \n" + ex.Message);    
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
                RawInformations = rawInformations,
                RawInformationEntity = rawInformationEntity,
                TransferInformation = transferInformation,
                OrderInformations = orderInformations,
                PositionInformations = positionInformations,
                StatusInformations = statusInformations,
            };
        }
    }
}