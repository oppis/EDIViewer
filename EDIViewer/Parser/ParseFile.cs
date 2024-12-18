﻿using Newtonsoft.Json;
using System.Collections.ObjectModel;

using EDIViewer.Models;
using EDIViewer.Helper;
using System.ComponentModel.DataAnnotations;

namespace EDIViewer.Parser
{
    class ParseFile
    {
        //Datei Inahlt
        string[] origFile;
        string[] currentFileRowArray;
        string currentAufNr = string.Empty;
        string currentPosNr = string.Empty;

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
        readonly ObservableCollection<ObservableCollection<RawInformation>> rawInformationOrder = [];
        readonly ObservableCollection<ObservableCollection<RawInformation>> rawInformationPosition = [];
        ObservableCollection<RawInformation> rawInformationOrderTmp = [];
        ObservableCollection<RawInformation> rawInformationPositionTmp = [];

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
        /// Aktuelle Feld Definition zum aktuellen Record Typ erhalten
        /// </summary>
        /// <param name="currentFileRow">Aktuelle Zeile</param>
        private bool GetCurrentFieldDefinations(string currentFileRow)
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

                        return true;
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
                        
                        return true;
                    }
                }
            }

            return false;
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

                                //Länge bestimmen für Parsing bis Ende der Zeile
                                string oldAufNr = currentAufNr;

                                //Prüfen ob Zeile lang genug für Objekt Trennung
                                if (currentFileRowArray.Length - 1 >= currentFormatType.OrderSeparatorStart)
                                {
                                    currentAufNr = currentFileRowArray[currentFormatType.OrderSeparatorStart];
                                }


                                string oldPosNr = currentPosNr;

                                //Prüfen ob neuer Auftrag vorliegt und neue Einheit anlegen
                                if (oldAufNr != currentAufNr)
                                {
                                    rawInformationOrderTmp = [];
                                    oldPosNr = string.Empty;
                                }

                                try
                                {
                                    //Alle Felder durchgehen
                                    for (int i = 0; i < (currentFileRowArray.Length - 1); i++)
                                    {
                                        currentFieldContent = currentFileRowArray[i].Trim();

                                        //Variable für Erweiterte Informationen
                                        string fieldContentExtended = string.Empty;

                                        //ArtDefinitionen einfügen
                                        if (currentFieldDefiniations[i].ArtDefinations is not null)
                                        {
                                            foreach (ArtDefination artDefinition in currentFieldDefiniations[i].ArtDefinations)
                                            {
                                                if (currentFieldContent == artDefinition.Id)
                                                {
                                                    fieldContentExtended = artDefinition.Name;
                                                }
                                            }
                                        }

                                        //Mapping Felder
                                        //Transfer Informationen                           
                                        if (!String.IsNullOrEmpty(currentFieldDefiniations[i].TransferInformation))
                                        {
                                            if (currentFieldContent.Length > 0)
                                            {
                                                transferInformation.Add(currentFieldDefiniations[i].TransferInformation, currentFieldContent);
                                            }
                                        }
                                        //Auftragsinformationen
                                        if (!String.IsNullOrEmpty(currentFieldDefiniations[i].OrderInformation))
                                        {
                                            if (currentFieldContent.Length > 0)
                                            {
                                                orderInformation.TryAdd(currentFieldDefiniations[i].OrderInformation, currentFieldContent);
                                            }
                                        }
                                        //Positionsinformationen
                                        if (!String.IsNullOrEmpty(currentFieldDefiniations[i].PositionInformation))
                                        {
                                            if (currentFieldContent.Length > 0)
                                            {
                                                positionInformation.TryAdd(currentFieldDefiniations[i].PositionInformation, currentFieldContent);

                                                if (currentRecordType.PositionTyp)
                                                {
                                                    if(currentFieldDefiniations[i].PositionInformation == "IdPosition") //TODO -> Anpassung Positions ID -> Auch in ohne Feldlänge umsetzen und aus UI entfernen -> Aktuell scheinbar noch verschoben
                                                    {
                                                        currentPosNr = currentFieldContent;

                                                        if (oldPosNr != currentPosNr)
                                                        {
                                                            rawInformationPositionTmp = [];
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        //Statusinformationen
                                        if (!String.IsNullOrEmpty(currentFieldDefiniations[i].StatusInformation))
                                        {
                                            if (currentFieldContent.Length > 0)
                                            {
                                                statusInformation.TryAdd(currentFieldDefiniations[i].StatusInformation, currentFieldContent);
                                            }
                                        }

                                        //Ausgabe Objekt erstellen
                                        RawInformation currentRawInformation = new()
                                        {
                                            RecordTyp = currentRecordType.Name,
                                            Field = currentFieldDefiniations[i].Name,
                                            FieldContent = currentFieldContent,
                                            FieldContentExtended = fieldContentExtended,
                                            FileRow = fileRowIndex,
                                            AufNr = currentAufNr,
                                        };

                                        //Alle Einträge hinzufügen
                                        rawInformations.Add(currentRawInformation);

                                        //Listen mit Gruppierung nach AufNr erstellen
                                        rawInformationOrderTmp.Add(currentRawInformation);

                                        if (currentRecordType.PositionTyp)
                                        {
                                            rawInformationPositionTmp.Add(currentRawInformation);
                                        }

                                    }
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    UserMessageHelper.ShowErrorMessageBox("Parsen", $"Es wurde die Feld Definition nicht gefunden! \nAktueller Record: {currentRecordType.Name} \nFeld Inhalt: {currentFieldContent}");
                                }
                                catch (Exception ex)
                                {
                                    UserMessageHelper.ShowErrorMessageBox("Parsen", "Folgender Fehler ist beim Lesen der Datei aufgetreten: \n" + ex.Message);
                                }

                                //Informationen in Gruppen Liste speichern wenn sich Einheit ändert
                                if (oldAufNr != currentAufNr)
                                {
                                    rawInformationOrder.Add(rawInformationOrderTmp);

                                    //Mapping Auftrag in Liste schreiben
                                    if (orderInformation.Count > 0)
                                    {
                                        orderInformations.Add(orderInformation);
                                        orderInformation = [];
                                    }

                                    //Mapping Satus in Liste schreiben
                                    if (statusInformation.Count > 0)
                                    {
                                        statusInformations.Add(statusInformation);
                                        statusInformation = [];
                                    }
                                }

                                if (oldPosNr != currentPosNr)
                                {
                                    if (positionInformation.Count > 0)
                                    {
                                        positionInformations.Add(positionInformation);
                                        positionInformation = [];
                                    }

                                    rawInformationPosition.Add(rawInformationPositionTmp);
                                }
                            }
                            else if (returnStatus & fileRow.Length == 0)
                            {
                                UserMessageHelper.ShowErrorMessageBox("Parsen", "Es wurde keine Feld Definition gefunden!\n" + fileRow);
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
                                //Prüfen ob neuer Auftrag beginnt und neue Einheit anlegen
                                string oldAufNr = currentAufNr;

                                //Prüfen ob Zeile lang genug für Objekt Trennung
                                if (fileRow.Length > (currentFormatType.OrderSeparatorStart - 1 + currentFormatType.OrderSeparatorLength))
                                {
                                    currentAufNr = fileRow.Substring(currentFormatType.OrderSeparatorStart - 1, currentFormatType.OrderSeparatorLength);
                                }

                                //Prüfen ob neue Position beginnt und neue Einheit anlegen
                                string oldPosNr = currentPosNr;

                                if (oldAufNr != currentAufNr)
                                {
                                    rawInformationOrderTmp = [];
                                    oldPosNr = string.Empty;
                                }

                                if (currentRecordType.PositionTyp)
                                {
                                    //Prüfen ob Zeile lang genug für Objekt Trennung
                                    if (fileRow.Length > (currentFormatType.PostionSeparatorStart - 1 + currentFormatType.PostionSeparatorLength))
                                    {
                                        currentPosNr = fileRow.Substring(currentFormatType.PostionSeparatorStart - 1, currentFormatType.PostionSeparatorLength).Trim();
                                    }

                                    if (oldPosNr != currentPosNr)
                                    {
                                        rawInformationPositionTmp = [];
                                    }
                                }

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

                                        //Interpretation der Feld Informationen
                                        string fileContent = fileRow.Substring(fieldDefination.Start - 1, end).Trim();

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
                                            if (fileContent.Length > 0)
                                            {
                                                transferInformation.Add(fieldDefination.TransferInformation, fileContent);
                                            }
                                        }
                                        //Auftragsinformationen
                                        if (!String.IsNullOrEmpty(fieldDefination.OrderInformation))
                                        {
                                            if (fileContent.Length > 0)
                                            {
                                                orderInformation.TryAdd(fieldDefination.OrderInformation, fileContent);
                                            }
                                        }
                                        //Positionsinformationen
                                        if (!String.IsNullOrEmpty(fieldDefination.PositionInformation))
                                        {
                                            if (fileContent.Length > 0)
                                            {
                                                positionInformation.TryAdd(fieldDefination.PositionInformation, fileContent);
                                            }
                                        }
                                        //Statusinformationen
                                        if (!String.IsNullOrEmpty(fieldDefination.StatusInformation))
                                        {
                                            if (fileContent.Length > 0)
                                            {
                                                statusInformation.TryAdd(fieldDefination.StatusInformation, fileContent);
                                            }
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
                                        rawInformationOrderTmp.Add(currentRawInformation);

                                        if (currentRecordType.PositionTyp)
                                        {
                                            rawInformationPositionTmp.Add(currentRawInformation);
                                        }
                                    }
                                }
                                
                                //Informationen in Gruppen Liste speichern wenn sich Einheit ändert
                                if (oldAufNr != currentAufNr)
                                {
                                    rawInformationOrder.Add(rawInformationOrderTmp);

                                    //Mapping Auftrag in Liste schreiben
                                    if (orderInformation.Count > 0)
                                    {
                                        orderInformations.Add(orderInformation);
                                 
                                        orderInformation = [];
                                    }
                                    //Mapping Satus in Liste schreiben
                                    if (statusInformation.Count > 0)
                                    {
                                        statusInformations.Add(statusInformation);
                                        statusInformation = [];
                                    }
                                }

                                if (oldPosNr != currentPosNr)
                                {
                                    rawInformationPosition.Add(rawInformationPositionTmp);

                                    if (positionInformation.Count > 0)
                                    {
                                        positionInformations.Add(positionInformation);
                                        positionInformation = [];
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
                UserMessageHelper.ShowErrorMessageBox("Parsen", "Folgender Fehler ist beim Lesen der Datei aufgetreten: \n" + ex.Message);
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
                RawInformationOrder = rawInformationOrder,
                RawInformationPosition = rawInformationPosition,
                TransferInformation = transferInformation,
                OrderInformations = orderInformations,
                PositionInformations = positionInformations,
                StatusInformations = statusInformations,
            };
        }
    }
}