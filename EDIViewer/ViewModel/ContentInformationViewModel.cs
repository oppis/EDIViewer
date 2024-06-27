using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;

using EDIViewer.Helper;
using EDIViewer.Models;
using EDIViewer.Parser;

namespace EDIViewer.ViewModel
{
    public class ContentInformationViewModel : INotifyPropertyChanged
    {
        private static ContentInformation contentInformation;
        public ContentInformationViewModel(string currentFileFormat, string[] viewLines)
        {
            ParseFile parseFile = new();
            parseFile.GetFileStructur(currentFileFormat);
            parseFile.ProcessCurrentFile(viewLines);

            contentInformation = parseFile.contentInformation;
        }
        public ContentInformationViewModel() 
        {
            contentInformation = null;
            //RawInformations = null;
            //RawInformationEntity = null;
            //TransferInformation = null;
        }

        public ObservableCollection<RawInformation> RawInformations
        {
            get
            {
                ObservableCollection <RawInformation> rawInformations = new();
                if (contentInformation is not null)
                {
                    rawInformations = contentInformation.RawInformations;
                }

                return rawInformations;
            }
            set
            {
                contentInformation.RawInformations = value;
                OnPropertyChanged(nameof(RawInformations));
            }
        }
        public ObservableCollection<ObservableCollection<RawInformation>> RawInformationEntity
        {
            get
            {
                return contentInformation.RawInformationEntity;
            }
            set
            {
                contentInformation.RawInformationEntity = value;
                OnPropertyChanged(nameof(RawInformationEntity));
            }
        }
        public TransferInformation TransferInformation
        {
            get
            {
                TransferInformation transferInformation = new();

                if (contentInformation != null)
                {
                    //Parsen des Dictionary in Model
                    foreach (var item in contentInformation.TransferInformation)
                    {
                        try
                        {
                            switch (item.Key)
                            {
                                case "DataType":
                                    transferInformation.DataType = item.Value;
                                    break;
                                case "DateTime":
                                    transferInformation.DateTime = item.Value;
                                    break;
                                case "DataReference":
                                    transferInformation.DataReference = item.Value;
                                    break;
                                case "SenderID":
                                    transferInformation.SenderID = item.Value;
                                    break;
                                case "RecipientID":
                                    transferInformation.RecipientID = item.Value;
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            UserMessageHelper.ShowMessageBox("Read TransferInformation", "Es ist folgender Fehler aufgetreten: \n" + ex.Message);
                            throw;
                        }
                    }
                }
                
                return transferInformation;
            }
            set
            {
                contentInformation.TransferInformation = null;
                OnPropertyChanged(nameof(TransferInformation));
            }
        }
        public ObservableCollection<OrderInformation> OrderInformations
        {
            get
            {
                ObservableCollection<OrderInformation> orderInformations = new();

                if (contentInformation != null)
                {
                    //Parsen des Dictionary in Model -> Dann Liste hinzufügen

                    foreach (var orderInformationList in contentInformation.OrderInformations)
                    {
                        OrderInformation currentOrderInformation = new();

                        foreach(var item in orderInformationList)
                        {
                            try
                            {
                                switch (item.Key)
                                {
                                    case "IdOrder":
                                        currentOrderInformation.IdOrder = item.Value;
                                        break;
                                    case "Reference":
                                        currentOrderInformation.Reference = item.Value;
                                        break;
                                    case "DateTimeLoadDat":
                                        currentOrderInformation.DateTimeLoadDat = item.Value;
                                        break;
                                    case "DateTimeUnloadDat":
                                        currentOrderInformation.DateTimeUnloadDat = item.Value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                UserMessageHelper.ShowMessageBox("Read OrderInformations", "Es ist folgender Fehler aufgetreten: \n" + ex.Message);
                                throw;
                            }
                        }
                        orderInformations.Add(currentOrderInformation);
                    }
                }

                return orderInformations;
            }
        }

        public ObservableCollection<PositionInformation> PositionInformations
        {
            get
            {
                ObservableCollection<PositionInformation> positionInformations = new();

                if (contentInformation != null)
                {
                    //Parsen des Dictionary in Model -> Dann Liste hinzufügen

                    foreach (var positionInformationList in contentInformation.PositionInformations)
                    {
                        PositionInformation currentPositionInformation = new();

                        foreach (var item in positionInformationList)
                        {
                            try
                            {
                                switch (item.Key)
                                {
                                    case "IdOrder":
                                        currentPositionInformation.IdOrder = item.Value;
                                        break;
                                    case "IdPosition":
                                        currentPositionInformation.IdPosition = Convert.ToInt32(item.Value);
                                        break;
                                    case "SSCC":
                                        currentPositionInformation.SSCC = item.Value;
                                        break;
                                    case "PackageCount":
                                        currentPositionInformation.PackageCount = Convert.ToInt16(item.Value);
                                        break;
                                    case "PackagingUnit":
                                        currentPositionInformation.PackagingUnit = item.Value;
                                        break;
                                    case "Height":
                                        currentPositionInformation.Height = Convert.ToDouble(item.Value);
                                        break;
                                    case "Width":
                                        currentPositionInformation.Width = Convert.ToDouble(item.Value);
                                        break;
                                    case "Length":
                                        currentPositionInformation.Length = Convert.ToDouble(item.Value);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                UserMessageHelper.ShowMessageBox("Read PositionInformations", "Es ist folgender Fehler aufgetreten: \n" + ex.Message);
                                throw;
                            }
                        }

                        positionInformations.Add(currentPositionInformation);
                    }
                }

                return positionInformations;
            }
        }
        public ObservableCollection<StatusInformation> StatusInformations
        {
            get
            {
                ObservableCollection<StatusInformation> statusInformations = new();

                if (contentInformation != null)
                {
                    //Parsen des Dictionary in Model -> Dann Liste hinzufügen

                    foreach (var statusInformationList in contentInformation.StatusInformations)
                    {
                        StatusInformation currentStatusInformation = new();

                        foreach (var item in statusInformationList)
                        {
                            try
                            {
                                switch (item.Key)
                                {
                                    case "StatusID":
                                        currentStatusInformation.StatusID = Convert.ToInt16(item.Value);
                                        break;
                                    case "StatusDateTime":
                                        currentStatusInformation.StatusDateTime = item.Value;
                                        break;
                                    case "StatusNotes":
                                        currentStatusInformation.StatusNotes = item.Value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                UserMessageHelper.ShowMessageBox("Read StatusInformations", "Es ist folgender Fehelr aufgetreten: \n" + ex.Message);
                                throw;
                            }
                        }

                        statusInformations.Add(currentStatusInformation);
                    }
                }

                return statusInformations;
            }
        }

        //Event 
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}