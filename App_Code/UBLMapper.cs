using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.ServiceModel;
using System.Globalization;
using wfws;
using System.Text.RegularExpressions;
/// <summary>
/// Summary description for UBLMapper
/// </summary>
namespace wfxml
{
    public class UBLMapper : IUBLDataMapper
    {
        //string documentId;
        private CompanyInf wfcompany = new CompanyInf();
        private CompanySeller wfseller = new CompanySeller();
        private Address BillTo = new Address();     //primarily for Accounting Customer Party
        private Address ShipTo = new Address();     //primarily for Delivery Party
        private Address Buyer = new Address();      //primarily for Buyer Customer Party  (Often the same as ShipTo)
        private OrderSales wforder = new OrderSales();
        private OrderPurchase wfpurc = new OrderPurchase();
        private OrderPayment wfpayment = new OrderPayment();
        private PaymentMeans wfpaymentmeans = new PaymentMeans();
        private AddressStatement wfadrState = new AddressStatement();
        private IList<OrderLine> tempItemsList = new List<OrderLine>();
        private IList<OrderLinePurc> tempPurcList = new List<OrderLinePurc>();
        private OrderLine tempItem = new OrderLine();
        private int ChargeIndicator;
        private List<long> OrderLineIDs = new List<long>();
        private int OrderLinePointer =0;
        private List<long> AllowanceChargeLineIDs = new List<long>();
        private int AllowanceChargeLinePointer = 0;
        private List<long> StatementLineIDs = new List<long>();
        private int StatementLinePointer = 0;
        private DBUser MapUser;
        private MapperFunction MappingFunction;
        private SubSystem MapSystem;

        public enum MapperFunction
        {
            Undefined= 0,
            Create = 1,
            Save = 2,
            Update = 3,
            Load = 4
        }
        public enum SubSystem
        { 
            Undefined = 0,
            Sale = 1,
            Purchase = 2,
            Finance = 3
        }
        //private NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

        public UBLMapper(ref DBUser DBUser, ref UBLDoc invoiceubl, SubSystem SubSys, MapperFunction SelectedOperation)
        {
            MappingFunction = SelectedOperation;
            MapSystem = SubSys;
            
            string errstr;
            MapUser = DBUser;

            if (MapSystem == SubSystem.Sale)
            {
                tempItem.Qty = 1;
                //var myorder = new OrderSales();
                wforder.InvoiceCreditnote = InvCre.invoice;
                var wfconn = new wfws.ConnectLocal(DBUser);
                errstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
                if (DBUser.CompID > 0)
                {
                    wfws.web wfweb = new wfws.web(ref DBUser);
                    if (MappingFunction == MapperFunction.Load)
                    {
                        if ((invoiceubl.DocID == 0) && (invoiceubl.InvoiceNo > 0)) invoiceubl.DocID = wfweb.get_saleid_by_ordreno(0, invoiceubl.InvoiceNo, ref errstr);

                        if (invoiceubl.DocID <= 0) throw new FaultException(string.Concat("The document was not found: ", invoiceubl.InvoiceNo.ToString()), new FaultCode(ErrCode.DocumentNotFound.ToString()));
                        if (wfweb.IsSuspicious(invoiceubl.DocID)) throw new FaultException(string.Concat("The document is suspicious: ", invoiceubl.InvoiceNo.ToString(),". Manual handling needed."), new FaultCode(ErrCode.InvalidDocument.ToString()));

                        var wfcomp = new wfws.Company(ref DBUser);
                        //DBUser.Message = "her";

                        //UBLOrder myUBL = new UBLOrder();
                        errstr = wfcomp.Company_Load(ref DBUser, ref wfcompany);
                        wfweb.order_load(invoiceubl.DocID, ref wforder);
                        wfweb.order_load_Items(invoiceubl.DocID, ref tempItemsList);
                        wforder.OrderLines = tempItemsList.ToArray();
                        //bool AllLinesZero = true;
                        foreach (OrderLine ol in tempItemsList)
                           // if (ol.OrderAmount != 0)   lines with no value is allowed since june 2015 revision
                           // {
                                if (ol.AllowanceCharge == true)
                                    AllowanceChargeLineIDs.Add(ol.Liid);
                                else
                                    OrderLineIDs.Add(ol.Liid);
                            //    AllLinesZero = false;
                            //}
                        //if (AllLinesZero) throw new FaultException(string.Concat("The document contains no non-zero lines"), new FaultCode(ErrCode.NoLines.ToString()));
                        OrderLinePointer = 0;
                        AllowanceChargeLinePointer = 0;
                        wfseller.SellerID = wforder.seller;
                        //errstr = wfcomp.Company_Load(ref DBUser, ref wfcompany);
                        errstr = wfcomp.Seller_Get(ref wfseller);
                        BillTo.AddressID = wforder.BillTo;
                        Buyer.AddressID = wforder.Buyer;
                        if ((wforder.BillTo == wforder.ShipTo) || (wforder.Buyer == wforder.ShipTo))        //Delivery må ikke medtages hvis det er den samme 
                            ShipTo.AddressID = 0;
                        else
                        {
                            ShipTo.AddressID = wforder.ShipTo;
                            errstr = wfweb.Address_Get(ref ShipTo);
                        }
                        errstr = wfweb.Address_Get(ref BillTo);
                        errstr = wfweb.Address_Get(ref Buyer);
                        //wfweb.Address_PaymentMeans_Load(BillTo.AddressID, ref wfpaymentmeans);        Udkommenteret da udviklingen hos LC stoppede midt i..
                        IList<OrderPayment> OrderPayments = new List<OrderPayment>();
                        errstr = wfweb.order_load_Payments(invoiceubl.DocID, ref OrderPayments);
                        if (OrderPayments.Count > 0) wfpayment = OrderPayments[0];

                        //myUBL.DocumentInit(ref invoiceubl);                     //Klares af Serializer initialisering
                        //DBUser.Message = string.Concat("her igen");
                        //errstr = myUBL.Load_ubl(ref invoiceubl);
                    }
                    if (MappingFunction == MapperFunction.Save)
                    {
                        //if (invoiceubl.InvoiceNo != 0) invoiceubl.SaleID = wfweb.get_saleid_by_ordreno(0, invoiceubl.InvoiceNo, ref errstr);
                        //if (invoiceubl.SaleID > 0) errstr = "Invoice number already exists";
                        //else
                        //{
                        //wforder.OrderLines= new OrderLine[];


                        //verify company number
                        XmlDocument _document;
                        _document = new XmlDocument();
                        _document.LoadXml(invoiceubl.XmlString);
                        /*      var wfcomp = new wfws.Company(ref DBUser);
                                errstr = wfcomp.Company_Load(ref DBUser, ref wfcompany);

                                if (wfcompany.CompanyNo != xmlComp) errstr = "Wrong Accounting supplier";
                                else
                                {
                                    // TODO Finish this check  - locate SellerID from seller.companyno
                                    //determine seller 
                                    _node = _document.SelectSingleNode("root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID", _nsManager);
                                    if (_node != null) xmlSeller = _node.InnerText;
                                    wfseller.CompanyNo = xmlSeller;
                                    if (wfseller.CompanyNo == "") errstr = "Wrong Seller supplier";
                                }*/
                        // }
                    }
                }
            }

            if (MapSystem == SubSystem.Purchase)
            { 
                tempItem.Qty = 1;
                wfpurc.InvoiceDebitnote = InvDeb.invoice;
                var wfconn = new wfws.ConnectLocal(DBUser);
                errstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
                if (DBUser.CompID > 0)
                {
                    wfws.web wfweb = new wfws.web(ref DBUser);
                    if (MappingFunction == MapperFunction.Load)
                    {
                        if ((invoiceubl.DocID == 0) && (invoiceubl.InvoiceNo > 0)) invoiceubl.DocID = wfweb.get_Purcid_by_ordreno(invoiceubl.InvoiceNo, ref errstr);
                        if (invoiceubl.DocID <= 0) throw new FaultException(string.Concat("The document was not found."), new FaultCode(ErrCode.DocumentNotFound.ToString()));
                        var wfcomp = new wfws.Company(ref DBUser);
                        errstr = wfcomp.Company_Load(ref DBUser, ref wfcompany);

                        wfweb.orderpu_load(invoiceubl.DocID, ref wfpurc);
                        wfweb.orderpu_load_Items(invoiceubl.DocID, ref tempPurcList);
                        //                        wfweb.order_load_Items(invoiceubl.DocID, ref tempItemsList);
                        wfpurc.OrderLines = tempPurcList.ToArray();
                        foreach (OrderLinePurc ol in tempPurcList)
                            if (ol.OrderAmount != 0)
                                if (ol.AllowanceCharge == true)
                                    AllowanceChargeLineIDs.Add(ol.Liid);
                                else
                                    OrderLineIDs.Add(ol.Liid);
                        OrderLinePointer = 0;
                        AllowanceChargeLinePointer = 0;
                        wfseller.SellerID = wforder.seller;
                        //errstr = wfcomp.Company_Load(ref DBUser, ref wfcompany);
                        errstr = wfcomp.Seller_Get(ref wfseller);
                        BillTo.AddressID = wforder.BillTo;
                        Buyer.AddressID = wforder.Buyer;
                        if ((wforder.BillTo == wforder.ShipTo) || (wforder.Buyer == wforder.ShipTo))        //Delivery må ikke medtages hvis det er den samme 
                            ShipTo.AddressID = 0;
                        else
                        {
                            ShipTo.AddressID = wforder.ShipTo;
                            errstr = wfweb.Address_Get(ref ShipTo);
                        }
                        errstr = wfweb.Address_Get(ref BillTo);
                        errstr = wfweb.Address_Get(ref Buyer);
                        //myUBL.DocumentInit(ref invoiceubl);                     //Klares af Serializer initialisering
                        //DBUser.Message = string.Concat("her igen");
                        //errstr = myUBL.Load_ubl(ref invoiceubl);
                    }
                }
            }
            if (MapSystem == SubSystem.Finance)
            {
                if (MappingFunction == MapperFunction.Load)
                {
                    var wfcomp = new wfws.Company(ref DBUser);
                    errstr = wfcomp.Company_Load(ref DBUser, ref wfcompany);

                    wfadrState.adAccount = invoiceubl.adAccount;
                    wfadrState.FromDate = invoiceubl.FromDate;
                    wfadrState.ToDate = invoiceubl.ToDate;

                    wfws.web wfweb = new wfws.web(ref DBUser);
                    wfweb.Address_Statement_Load(ref wfadrState);

                    foreach (AddressStatementLine al in wfadrState.StatementLines)
                        StatementLineIDs.Add(al.ItemID);
                }
            }
            return;
        }

        public string SaveOrderToWF(ref DBUser DBUser, ref UBLDoc ublDoc)
        {
            string errstr="";
            //var wfconn = new wfws.ConnectLocal(DBUser);
            //errstr = wfconn.ConnectionGetByGuid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                
                var wfcomp = new wfws.Company(ref DBUser);
//                if (wfcomp.Verify_Company(wfcompany))
//                {
//                    if (wfweb.ValidateSeller(ref wfseller))
//                    {
                        int NewSaleID = 0;
                        wforder.seller = wfseller.SellerID;   //TODO Look SellerID up!!
                        wfweb.Order_add(ref wforder, ref NewSaleID, 400);
                        //wfweb.Order_Update(NewSaleID, ref wforder, 400, false);   køres bagefter adresse/kontakt-gymnastikken
                        if (NewSaleID > 0)
                        {
                            FillOrderInWF(ref DBUser, ref ublDoc, NewSaleID);
                        }
                        else throw new FaultException(string.Concat("Error creating new document"), new FaultCode(ErrCode.GeneralUBLError.ToString()));
                        //håndter salgsted !
                        //PrepareUBLforCalc(NewSaleID);
                        //wfweb.order_calculate(NewSaleID);
                        //wfweb.order_save_ubl(NewSaleID, XmlString);

  //                  }
                //}
            }
            return errstr;
        }
        public void FillOrderInWF(ref DBUser DBUser, ref UBLDoc ublDoc, int ThisSaleID)
        {
            //string errstr="";
            int CurrentLineID = 0;
            ublDoc.InvoiceNo = (int)wforder.InvoiceNo;   //returns InvoiceNo in UBLDoc - returned from interface
            //              var wfconn = new wfws.ConnectLocal(DBUser);
            //errstr = wfconn.ConnectionGetByGuid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);

                var wfcomp = new wfws.Company(ref DBUser);

                foreach (OrderLine CurrentLine in wforder.OrderLines)
                {
                    wfweb.Order_add_item(ThisSaleID, CurrentLine, ref CurrentLineID);
                    CurrentLine.Liid = CurrentLineID;
                    wfweb.Order_Item_Update(ThisSaleID, CurrentLine);
                }
                Address CheckBillTo = new Address();
                int AddrCount = 0;
                int AddressID = 0;
                CheckBillTo.ImportID = BillTo.ImportID;
                AddressID = wfweb.Address_Lookup(ref CheckBillTo, ref AddrCount);
                if (AddrCount == 0)
                {
                    var newAddress = new Address();
                    if (BillTo.AdrGuid == Guid.Empty) BillTo.AdrGuid = Guid.NewGuid();
                    wfweb.Address_add(BillTo.AdrGuid, BillTo.Account, ref AddressID);
                    BillTo.AddressID = AddressID;
                    newAddress.AddressID = AddressID;
                    wfweb.Address_Get(ref newAddress);
                    BillTo.DebtorGroup = newAddress.DebtorGroup;
                    BillTo.SellerID = newAddress.SellerID;
                    BillTo.TimeChanged = newAddress.TimeChanged;
                    wfweb.Address_Update(AddressID, ref BillTo, false);
                    wforder.BillTo = AddressID;
                    AddrCount = 1;
                    //Create paymentmeans
                    //wfweb.Address_PaymentMeans_Update(AddressID, wfpaymentmeans);             Udkommenteret da udviklingen hos LC stoppede midt i.
                    //Create contact person and associate it with this address
                    Contact NewContact = new Contact();
                    NewContact.ContactName = string.IsNullOrEmpty(wforder.UBLContactName) ? wforder.Initials : wforder.UBLContactName;
                    NewContact.email = string.IsNullOrEmpty(wforder.UBLemail) ? "" : wforder.UBLemail;
                    NewContact.Initials = wforder.Initials;
                    NewContact.AccountingCost = wforder.AccountingCost;
                    NewContact.EndpointID = wforder.UBLEndpointID;
                    NewContact.EndpointScheme = wforder.UBLEndpointScheme;
                    NewContact.AddressID = AddressID;
                    NewContact.UBLDefault = true;
                    wfweb.Contact_add_new(ref NewContact);
                    wforder.ContID = NewContact.ContID;
                }
                if (AddrCount == 1)
                    if (AddressID == BillTo.AddressID)
                    {                                                                                       //TODO this code doesnt take into account new entity Buyer
                        if ((BillTo.ImportID == ShipTo.ImportID) || (string.IsNullOrEmpty(ShipTo.ImportID)))
                        {
                            wforder.ShipTo = AddressID;
                        }
                        else
                        {
                            Address CheckShipTo = new Address();
                            AddrCount = 0;
                            AddressID = 0;
                            CheckShipTo.ImportID = ShipTo.ImportID;
                            AddressID = wfweb.Address_Lookup(ref CheckShipTo, ref AddrCount);
                            if (AddrCount == 0)
                            {
                                var newAddress = new Address();
                                if (ShipTo.AdrGuid == Guid.Empty) ShipTo.AdrGuid = Guid.NewGuid();
                                wfweb.Address_add(ShipTo.AdrGuid, ShipTo.Account, ref AddressID);
                                ShipTo.AddressID = AddressID;
                                newAddress.AddressID = AddressID;
                                wfweb.Address_Get(ref newAddress);
                                ShipTo.DebtorGroup = newAddress.DebtorGroup;
                                ShipTo.SellerID = newAddress.SellerID;
                                ShipTo.TimeChanged = newAddress.TimeChanged;
                                wfweb.Address_Update(AddressID, ref ShipTo, false);
                                wforder.ShipTo = AddressID;
                                AddrCount = 1;
                            }
                        }
                        wfweb.order_address_associate(ref wforder);
                    }
                wfpayment.amountConverted = wfpayment.amount;
                wfpayment.Currency = wforder.Currency;
                wfweb.Order_add_payment(ThisSaleID, wfpayment);

                wfweb.Order_Update(ThisSaleID, ref wforder, 400, false);
                wfweb.order_save_ubl(ThisSaleID, ublDoc.XmlString);
                //kald wf_tr_sale_calc_lines_UBLWrapUp, og efterfølgende wf_tr_sale_calc_lines
                wfweb.PrepareUBLforCalc(ThisSaleID);
                //wfweb.order_calculate(NewSaleID);
            }                      
        }

        public void DataCheck()         //intermediate validator (between xml read and Winfinance write
        {
            wfws.LookUp Check = new wfws.LookUp();
            wfws.Accounts CheckAccounts = new Accounts(ref MapUser);
            if (!IsValidCompanyNo()) throw new FaultException(string.Concat("Invoice cannot insert into company number ",wfcompany.CompanyNo), new FaultCode(ErrCode.InvalidCompany.ToString()));
            if (!IsValidSellerNo()) throw new FaultException(string.Concat("Seller ", wfseller.CompanyNo, " does not exist in company ", wfcompany.CompanyName), new FaultCode(ErrCode.InvalidSeller.ToString()));
            if (!CheckAccounts.IsValidAccountingCost(wforder.AccountingCost)) throw new FaultException(string.Concat("Invalid Account number/Dimension ", wforder.AccountingCost), new FaultCode(ErrCode.InvalidAccountingCost.ToString()));
            if (!IsValidCountryCode(BillTo.CountryID)) throw new FaultException(string.Concat("Invalid AccountingCostumer countrycode ", BillTo.CountryID), new FaultCode(ErrCode.InvalidCountryCode.ToString()));
            if (!IsValidCountryCode(ShipTo.CountryID)) throw new FaultException(string.Concat("Invalid Delivery countrycode ", ShipTo.CountryID), new FaultCode(ErrCode.InvalidCountryCode.ToString()));
            if (!IsValidLanguageCode(BillTo.Language)) throw new FaultException(string.Concat("Invalid AccountingCostumer languagecode", BillTo.Language), new FaultCode(ErrCode.InvalidLanguageCode.ToString()));

            //if (Check.
        }

        
        public void ValidateCurrency(string value)  //validation function used during xml read
        {
            if (value != wforder.Currency) throw new FaultException(string.Concat("Curriencies does not match: "), new FaultCode(ErrCode.MultipleCurrencyCodesNotAllowed.ToString()));
        }

        private bool IsValidCountryCode(string CountryCode)
        {
            bool answer = false;
            if (string.IsNullOrEmpty(CountryCode))
                answer = true;
            else
            {
                wfws.LookUp Konv = new wfws.LookUp();
                if (Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, CountryCode) != "") answer = true;
            }
            return answer;
        }
        private bool IsValidLanguageCode(string LanguageCode)
        {
            bool answer = false;
            if (string.IsNullOrEmpty(LanguageCode))
                answer = true;
            else
            {
                wfws.LookUp Konv = new wfws.LookUp();
                if (Konv.convLanguage(LanguageIDType.PreferredForCountryVC, LanguageIDType.ISO_1, LanguageCode) != "") answer = true;
            }
            return answer;
        }

        public void PutWrapUp() {
            tempItemsList.Add(tempItem);
            //foreach (OrderLine myLine in tempItemsList) {
            //    if (myLine.AllowanceCharge) {
            //        myLine.LineVat = myLine.LineAmount * myLine.vat_perc /100;
            //    }
            //}
            
            wforder.OrderLines = tempItemsList.ToArray();
        }

        
        private string CurrencyString(decimal TheValue)
        {
            //return TheValue.ToString("N", nfi);
            return TheValue.ToString("0.00", CultureInfo.InvariantCulture);
        }
        private string CurrencyString4(decimal TheValue)
        {
            //return TheValue.ToString("N", nfi);
            return TheValue.ToString("0.0000", CultureInfo.InvariantCulture);
        }

        private decimal StringCurrency(string TheValue)
        {
            return Convert.ToDecimal(TheValue, CultureInfo.InvariantCulture);
        }
        private DateTime StringDate(string TheValue)
        {
            return Convert.ToDateTime(TheValue, CultureInfo.InvariantCulture);
        }
        private string VatRate(bool TheValue)
        {
            if (TheValue == true)
                return "StandardRated";
            else
                return "ZeroRated";
        }
        private string VatRate(decimal Percentage, bool VatIncl)
        {
            if ((Percentage <= 0))
                return "ZeroRated";
            else
                return "StandardRated";
        }

        private string VatRate(decimal TheValue)
        {
            if (TheValue != 0)
                return "StandardRated";
            else
                return "ZeroRated";
        }
        private Boolean VatRate(string TheValue)
        {
            if (TheValue == "StandardRated")
                return true;
            else
                return false;
        }

        private string VatRateHead(SubSystem SubSys)         // HACK Grouping of tax-rates needs to be made in WF
        {
            decimal MaxVat=0;
            if (SubSys == SubSystem.Sale)
            {
            int ItemCount = (from line in wforder.OrderLines where line.AllowanceCharge == false select line).Count();
            if (ItemCount == 0)     //Hvis faktura KUN indeholder gebyrlinier  (undtagelse)
            {
                foreach (OrderLine OneLine in wforder.OrderLines)
                        if (OneLine.vat_perc > MaxVat)
                            MaxVat = OneLine.vat_perc;
            }
            else
            {
                foreach (OrderLine OneLine in wforder.OrderLines)
                    if (OneLine.AllowanceCharge == false)
                        if (OneLine.vat_perc > MaxVat)
                            MaxVat = OneLine.vat_perc;
                }
            }
            if (SubSys == SubSystem.Purchase)
            {
                int ItemCount = (from line in wfpurc.OrderLines where line.AllowanceCharge == false select line).Count();
                if (ItemCount == 0)     //Hvis faktura KUN indeholder gebyrlinier  (undtagelse)
                {
                    foreach (OrderLinePurc OneLine in wfpurc.OrderLines)
                        if (OneLine.vat_perc > MaxVat)
                            MaxVat = OneLine.vat_perc;
                }
                else
                {
                    foreach (OrderLinePurc OneLine in wfpurc.OrderLines)
                        if (OneLine.AllowanceCharge == false)
                            if (OneLine.vat_perc > MaxVat)
                                MaxVat = OneLine.vat_perc;
            }
            }

            return CurrencyString(MaxVat);
        }
        private string ValidateCompNo()
        {
            wfws.LookUp Konv = new wfws.LookUp();
            int OutCome = 0;
            string result = "";
            if (!string.IsNullOrEmpty(BillTo.VATNumber))
                if (BillTo.VATNumber.Length == 8)
                {
                    if (int.TryParse(BillTo.VATNumber, out OutCome))
                        result = string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), BillTo.VATNumber);
                } else {
                    if ((BillTo.VATNumber.Length == 10) && (BillTo.VATNumber.Substring(0,2)=="DK"))
                        result = BillTo.VATNumber;
                }
                    
            if (result=="")     //if a foreign Vatnumber disable all spekulations...
                if (BillTo.CountryID!="DK")
                    result = BillTo.VATNumber;

            return result;
        }

        private bool IsValidCompanyNo()
        {   //Gets the CompanyNo from current connection
            bool Answer = false;
            string errstr;

            if (string.IsNullOrEmpty(wfcompany.CompanyNo))
                Answer = true;
            else
            {
                CompanyInf testcompany = new CompanyInf();
                var wfcomp = new wfws.Company(ref MapUser);
                errstr = wfcomp.Company_Load(ref MapUser, ref testcompany);
                if (testcompany.CompanyNo == wfcompany.CompanyNo) Answer = true;                                                //with countrycode
                if (testcompany.CompanyNo == wfcompany.CompanyNo.Substring(wfcompany.CompanyNo.Length - 8)) Answer = true;        //without countrycode
            }
            return Answer;
        }

        private bool IsValidSellerNo()
        {   //Check the validity of the seller to enter the invoice in
            bool Answer = false;
            int SellerID=0;
            string errstr;
            var wfcomp = new wfws.Company(ref MapUser);
            SellerID = wforder.seller; // wfcomp.SellerNo2ID(wfseller.SellerNo);
            if (SellerID != 0)
            {
                CompanySeller testseller = new CompanySeller();
                testseller.SellerID=SellerID;
                errstr = wfcomp.Seller_Get(ref testseller);
                if (string.IsNullOrEmpty(wfseller.CompanyNo))
                    Answer = true;
                else
                {
                    if (testseller.CompanyNo == wfseller.CompanyNo) Answer = true;
                    if (testseller.CompanyNo == wfseller.CompanyNo.Substring(wfseller.CompanyNo.Length - 8)) Answer = true;
                }
            }
            return Answer;
        }


        private string ASPEndPointScheme()   //ASP = AccountingSupplierParty 
        { 
            string Result = "DK:VANS";
            if (!String.IsNullOrEmpty(wfcompany.ean)) Result = "GLN";
            else
                if (!String.IsNullOrEmpty(wfcompany.CompanyNo)) Result = "DK:CVR";
            return Result;
        }
        private string ASPEndPoint()       //ASP = AccountingSupplierParty 
        {
            string Result = "N/A";
            wfws.LookUp Konv = new wfws.LookUp();
            if (!String.IsNullOrEmpty(wfcompany.ean)) Result = wfcompany.ean;
            else
                if (!String.IsNullOrEmpty(wfcompany.CompanyNo)) Result = string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo.Replace(" ", ""));
            return Result;
        }
        public List<long> GetOrderLineIDs()
        {
            return OrderLineIDs;
        }
        public List<long> GetAllowanceChargeLineIDs()
        {
            return AllowanceChargeLineIDs;
        }
        public List<long> GetStatementLineIDs()
        {
            return StatementLineIDs;
        }
        public long GetOrderLineID(byte What = 1)       //0=first, 1=next
        {
            long answer;
            if (What == 0) OrderLinePointer = 0;
            if (OrderLineIDs.Count == 0)
                answer= -1;
            else
            {
                answer = OrderLineIDs[OrderLinePointer];
                OrderLinePointer = +1;
                if (OrderLinePointer >= OrderLineIDs.Count) OrderLinePointer = 0;
            }
            return answer;
        }

        public long GetAllowanceChargeLineID(byte What = 1)       //0=first, 1=next
        {
            long answer;
            if (What == 0) AllowanceChargeLinePointer = 0;
            if (AllowanceChargeLineIDs.Count == 0)
                answer = -1;
            else
            {
                answer = AllowanceChargeLineIDs[AllowanceChargeLinePointer];
                AllowanceChargeLinePointer = +1;
                if (AllowanceChargeLinePointer >= AllowanceChargeLineIDs.Count) AllowanceChargeLinePointer = 0;
            }
            return answer;
        }

        public long GetStatementLineID(byte What = 1)       //0=first, 1=next
        {
            long answer;
            if (What == 0) StatementLinePointer = 0;
            if (StatementLineIDs.Count == 0)
                answer = -1;
            else
            {
                answer = StatementLineIDs[StatementLinePointer];
                StatementLinePointer = +1;
                if (StatementLinePointer >= StatementLineIDs.Count) StatementLinePointer = 0;
            }
            return answer;
        }

        public int FindAddressID(string value, int AddressID)
        { 
            int Result;

            if (AddressID == 0 )
            {
                wfws.web wfweb = new wfws.web(ref MapUser);
                Result = wfweb.Address_GetByadAccount(value);
            }
            else
                Result = AddressID;
            return Result;

        }

        public bool IsDocumentTypeCorrect(UblDocumentType docType)
        {
            bool answer = false;
            if ((docType == UblDocumentType.Invoice) && (wforder.InvoiceCreditnote == InvCre.invoice)) answer = true;
            if ((docType == UblDocumentType.CreditNote) && (wforder.InvoiceCreditnote == InvCre.CreditNote)) answer = true;
            return answer;
        }

        public long GetInvoiceNo() {
            return wforder.InvoiceNo;
        }

        private string makeinitials(string strName)
        {
            Regex extractInitials = new Regex(@"\s*([^\s])[^\s]*\s*");
            string retval = extractInitials.Replace(strName, "$1").ToUpper();
            if ((retval.Length < 2) && (strName.Length > 2)) retval = strName.Substring(0, 3).ToUpper();
            return retval;
        }
        //BENEATH THIS POINT ONLY XPATH CASE STATEMENTS


        public string GetInvoiceValue(string xpath)
        {
            //TODO: tget the object by the document ID      
 
            wfws.LookUp Konv = new wfws.LookUp();
            switch (xpath)
            {
                case "root:Invoice/cbc:UBLVersionID": return "2.0";
                case "root:Invoice/cbc:CustomizationID": return "OIOUBL-2.02";
                case "root:Invoice/cbc:ProfileID": return "urn:www.nesubl.eu:profiles:profile5:ver2.0";
                case "root:Invoice/cbc:ProfileID/@schemeID": return "urn:oioubl:id:profileid-1.2";
                case "root:Invoice/cbc:ProfileID/@schemeAgencyID": return "320";
                case "root:Invoice/cbc:ID": return wforder.InvoiceNo.ToString();
                case "root:Invoice/cbc:ID/@schemeID": return wfcompany.GuidComp.ToString();
                case "root:Invoice/cbc:UUID": return wforder.GuidInv.ToString();
                case "root:Invoice/cbc:IssueDate": return XmlConvert.ToString(wforder.InvoiceDate, "yyyy-MM-dd");
                case "root:Invoice/cbc:IssueTime": return "";
                case "root:Invoice/cbc:Note": return string.IsNullOrEmpty(wforder.text_1) & string.IsNullOrEmpty(wforder.text_2) ? "" : string.Concat(wforder.text_1, " ", wforder.text_2);
                case "root:Invoice/cbc:DocumentCurrencyCode": return wforder.Currency;
                case "root:Invoice/cbc:AccountingCost": return wforder.AccountingCost;
                case "root:Invoice/cac:OrderReference/cbc:ID": return wforder.requisition;
                case "root:Invoice/cac:AccountingSupplierParty/cbc:AdditionalAccountID": return wfcompany.AdditionalAccountID;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID": return wfcompany.ean; // ASPEndPoint();
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID": return wfcompany.endpointtype; // ASPEndPointScheme();
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID": return wfcompany.ean; // ASPEndPoint();   // Sat til samme som EndpointID aht Byg-E /EDB Gruppen. Skal der laves et valgopsæt på seller?                    string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return wfcompany.endpointtype; // ASPEndPointScheme(); // Sat til samme som EndpointID aht Byg-E /EDB Gruppen. Skal der laves et valgopsæt på seller?      string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name": return wfcompany.CompanyName;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfcompany.Street;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfcompany.HouseNumber;                
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return wfcompany.InHouseMail; 
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfcompany.CityName;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfcompany.PostalZone;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country);
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.TaxSchemeID) ? "" : wfcompany.TaxSchemeID;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.TaxSchemeID) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLTaxScheme, wfcompany.Country, "ZZZ");  //One rule exception for DK. Will not be functionalized
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return "63";
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.2";
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return wfcompany.CompanyName;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:ID": return string.IsNullOrEmpty(wfcompany.Email) & string.IsNullOrEmpty(wfcompany.CompanyPhone) ? "" : "1";
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:Telephone": return wfcompany.CompanyPhone;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail": return wfcompany.Email;


                case "root:Invoice/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID": return BillTo.AddressID.ToString();
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID": return string.IsNullOrEmpty(wforder.UBLEndpointID) ? "" : wforder.UBLEndpointID;  //BillTo.ean;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(wforder.UBLEndpointID)) ? "" : (string.IsNullOrEmpty(wforder.UBLEndpointScheme)) ? "GLN" : wforder.UBLEndpointScheme;  //BillTo.EndpointType;
                //case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "DK:VANS" : "GLN";
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID": return string.IsNullOrEmpty(wforder.UBLEndpointID) ? "" : wforder.UBLEndpointID; // Sat til samme som EndpointID aht Byg-E /EDB Gruppen. Skal der laves et valgopsæt på seller?                    string.IsNullOrEmpty(ValidateCompNo()) ? BillTo.Account : ValidateCompNo();
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return (string.IsNullOrEmpty(wforder.UBLEndpointID)) ? "" : (string.IsNullOrEmpty(wforder.UBLEndpointScheme)) ? "GLN" : wforder.UBLEndpointScheme; // Sat til samme som EndpointID aht Byg-E /EDB Gruppen. Skal der laves et valgopsæt på seller?                    string.IsNullOrEmpty(ValidateCompNo()) ? "ZZZ" : (BillTo.CountryID == "DK" ? "DK:CVR" : "ZZZ");
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name": return BillTo.CompanyName;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID": return Konv.convLanguage(LanguageIDType.PreferredForCountryVC,LanguageIDType.ISO_1, BillTo.Language);
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredLax";                  //TODO  intention to use StructuredDK - still has buildingnumber etc
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": return BillTo.Address1;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return BillTo.HouseNumber;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return BillTo.InHouseMail;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Department": return BillTo.Department;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": return BillTo.City;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return BillTo.PostalCode;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID);
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, BillTo.CountryID);
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return ValidateCompNo();
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : (BillTo.CountryID == "DK" ? "DK:SE" : "ZZZ");
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "63";
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeAgencyID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "320";
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "urn:oioubl:id:taxschemeid-1.1";
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "Moms";
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return ValidateCompNo();
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : (BillTo.CountryID == "DK" ? "DK:CVR" : "ZZZ");  //Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, BillTo.CountryID, "ZZZ"); //(BillTo.CountryID == "DK" ? "DK:CVR" : "ZZZ"); //
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ID": return string.IsNullOrEmpty(wforder.Initials) ? string.IsNullOrEmpty(wforder.UBLContactName) ? "" : makeinitials(wforder.UBLContactName) : wforder.Initials;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Name": return wforder.UBLContactName;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": return wforder.UBLemail;
                case "root:Invoice/cac:AccountingCustomerParty/cac:AccountingContact/cbc:Name": return (string.IsNullOrEmpty(BillTo.PayPhone)) ? "" : BillTo.CompanyName;
                case "root:Invoice/cac:AccountingCustomerParty/cac:AccountingContact/cbc:Telephone": return BillTo.PayPhone;
                case "root:Invoice/cac:AccountingCustomerParty/cac:BuyerContact/cbc:Name": return wforder.ExtRef;

                //case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FamilyName": return BillTo.LastName;
                case "root:Invoice/cac:BuyerCustomerParty/cbc:SupplierAssignedAccountID": return Buyer.AddressID==0 ? "" : Buyer.AddressID.ToString();
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cbc:EndpointID": return string.IsNullOrEmpty(Buyer.ean) ? "" : Buyer.ean;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(Buyer.ean)) ? "" : (string.IsNullOrEmpty(Buyer.EndpointType)) ? "GLN" : Buyer.EndpointType;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID": return string.IsNullOrEmpty(Buyer.ean) ? "" : Buyer.ean;                                                                       // Sat til samme som EndpointID aht Byg-E /EDB Gruppen. Skal der laves et valgopsæt på seller?                    string.IsNullOrEmpty(ValidateCompNo()) ? BillTo.Account : ValidateCompNo();
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return (string.IsNullOrEmpty(Buyer.ean)) ? "" : (string.IsNullOrEmpty(Buyer.EndpointType)) ? "GLN" : Buyer.EndpointType; // Sat til samme som EndpointID aht Byg-E /EDB Gruppen. Skal der laves et valgopsæt på seller?                    string.IsNullOrEmpty(ValidateCompNo()) ? "ZZZ" : (BillTo.CountryID == "DK" ? "DK:CVR" : "ZZZ");
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyName/cbc:Name": return Buyer.CompanyName;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:Language/cbc:ID": return Konv.convLanguage(LanguageIDType.PreferredForCountryVC, LanguageIDType.ISO_1, Buyer.Language);
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return Buyer.AddressID == 0 ? "" : "StructuredLax";                  //TODO  intention to use StructuredDK - still has buildingnumber etc
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return Buyer.AddressID == 0 ? "" : "320";
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return Buyer.AddressID == 0 ? "" : "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": return Buyer.Address1;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return Buyer.HouseNumber;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return Buyer.InHouseMail;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:Department": return Buyer.Department;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": return Buyer.City;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return Buyer.PostalCode;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, Buyer.CountryID);
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, Buyer.CountryID);
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(Buyer.VATNumber) ? "" : Buyer.VATNumber;
                case "root:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(Buyer.VATNumber) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, Buyer.CountryID, "ZZZ"); //(BillTo.CountryID == "DK" ? "DK:CVR" : "ZZZ"); //


                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID": return string.IsNullOrEmpty(wfseller.CompanyNo) ? string.IsNullOrEmpty(wfseller.SellerNo) ? wfseller.SellerID.ToString() : wfseller.SellerNo.ToString() : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfseller.SellerCountryID), wfseller.CompanyNo); 
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfseller.SellerCountryID, "ZZZ");
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyName/cbc:Name": return wfseller.SellerName;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfseller.SellerStreet;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfseller.SellerHouseNumber;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return wfseller.SellerInHouseMail;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfseller.SellerCityName;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfseller.SellerPostalZone;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfseller.SellerCountryID);
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : wfseller.SellerName;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfseller.SellerCountryID), wfseller.CompanyNo);
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : (wfseller.SellerCountryID == "DK" ? "DK:CVR" : "ZZZ"); //Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfseller.SellerCountryID, "ZZZ");
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:Contact/cbc:ID": return "1";
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail": return wfseller.Email;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode": return (ShipTo.AddressID==0) ? "" : "StructuredLax" ;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode/@listID": return (ShipTo.AddressID==0) ? "" : "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode/@listAgencyID": return (ShipTo.AddressID==0) ? "" : "320";
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:StreetName": return (ShipTo.AddressID==0) ? "" : ShipTo.Address1;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:BuildingNumber": return (ShipTo.AddressID == 0) ? "" : ShipTo.HouseNumber;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:InhouseMail": return (ShipTo.AddressID == 0) ? "" : ShipTo.InHouseMail;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:Department": return (ShipTo.AddressID == 0) ? "" : ShipTo.Department;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:CityName": return (ShipTo.AddressID==0) ? "" : ShipTo.City;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:PostalZone": return (ShipTo.AddressID==0) ? "" : ShipTo.PostalCode;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:IdentificationCode": return (ShipTo.AddressID==0) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, ShipTo.CountryID);
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:Name": return (ShipTo.AddressID==0) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, ShipTo.CountryID);
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyIdentification/cbc:ID": return (ShipTo.AddressID==0) ? "" : ShipTo.Account;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyIdentification/cbc:ID/@schemeID": return (ShipTo.AddressID == 0) ? "" : "ZZZ";
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyName/cbc:Name": return (ShipTo.AddressID==0) ? "" : ShipTo.CompanyName;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:AddressFormatCode": return ShipTo.AddressID == 0 ? "" : "StructuredLax";                  //TODO  intention to use StructuredDK - still has buildingnumber etc
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return ShipTo.AddressID == 0 ? "" : "320";
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:AddressFormatCode/@listID": return ShipTo.AddressID == 0 ? "" : "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:StreetName": return (ShipTo.AddressID == 0) ? "" : ShipTo.Address1;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:BuildingNumber": return (ShipTo.AddressID == 0) ? "" : ShipTo.HouseNumber;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:InhouseMail": return (ShipTo.AddressID == 0) ? "" : ShipTo.InHouseMail;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:Department": return (ShipTo.AddressID == 0) ? "" : ShipTo.Department;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:CityName": return (ShipTo.AddressID == 0) ? "" : ShipTo.City;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cbc:PostalZone": return (ShipTo.AddressID == 0) ? "" : ShipTo.PostalCode;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return (ShipTo.AddressID == 0) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, ShipTo.CountryID);
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PostalAddress/cac:Country/cbc:Name": return (ShipTo.AddressID == 0) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, ShipTo.CountryID);

                // TODO This section hardcoded to creditcardpayment only
                /*                case "root:Invoice/cac:PaymentMeans/cbc:ID": return "1";        PAYMENTMEANS DISABLED  - LC udvikling stoppet midstream

                                case "root:Invoice/cac:PaymentMeans/cbc:PaymentMeansCode": return "48"; // XmlConvert.ToString(wfpaymentmeans.PaymentMeansCode);
                                case "root:Invoice/cac:PaymentMeans/cbc:PaymentDueDate": return wforder.PayDate == DateTime.MaxValue ? "" : XmlConvert.ToString(wforder.PayDate, "yyyy-MM-dd");
                                case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode": return wfpaymentmeans.PaymentChannelCode;
                                case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listID": return (wfpaymentmeans.PaymentChannelCode == null) ? "" : "urn:oioubl:codelist:paymentchannelcode-1.1";
                                case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listAgencyID": return (wfpaymentmeans.PaymentChannelCode == null) ? "" : "320";
                                case "root:Invoice/cac:PaymentMeans/cbc:InstructionID": return (wfpayment.meansOfPayment == null) ? "" : (wfpaymentmeans.PaymentMeansCode == 48) ? wfpaymentmeans.MeansOfPayment : wfpaymentmeans.CardType;
                                case "root:Invoice/cac:PaymentMeans/cbc:InstructionID/@schemeID": return (wfpayment.meansOfPayment == null) ? "" : "ZZZ";
                                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID": return wfpaymentmeans.BankAccount;
                                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote": return wfpaymentmeans.PaymentNote;
                                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID": return wfpaymentmeans.BankRegno;
                                //case "root:Invoice/cac:PaymentTerms/cbc:ID": return "1";
                                //case "root:Invoice/cac:PaymentTerms/cbc:PaymentMeansID": return "1";
                                //case "root:Invoice/cac:PaymentTerms/cbc:Amount": return CurrencyString(wforder.Total + wforder.TotalVatEx + wforder.TotalVatIn - wforder.AmountPaid);  //samme som "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount"
                                //case "root:Invoice/cac:PaymentTerms/cbc:Amount/@currencyID": return wforder.Currency;
                */                    // hertil

                // TODO This section handles one national or international banktransfers only. Se section above for extension with danish specialities
                // PaymentType=="fb" means Foreign Banktransfer
                //case "root:Invoice/cac:PaymentMeans/cbc:ID": return "1";
                //case "root:Invoice/cac:PaymentMeans/cbc:PaymentMeansCode": return wfseller.PaymentType=="fb" ? "31" : "42";
                //case "root:Invoice/cac:PaymentMeans/cbc:PaymentDueDate": return wforder.PayDate == DateTime.MaxValue ? "" : XmlConvert.ToString(wforder.PayDate, "yyyy-MM-dd");
                //case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode": return wfseller.PaymentType=="fb" ? "IBAN" : "DK:BANK";
                //case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listID": return "urn:oioubl:codelist:paymentchannelcode-1.1";
                //case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listAgencyID": return "320";
                //case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID": return  wfseller.PaymentType=="fb" ? wfseller.IBAN : wfseller.AccountNo;
                //case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:Name": return wfseller.BankName;
                //case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote": return wforder.PaymentRef;
                //case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID": return wfseller.PaymentType=="fb" ? "" : wfseller.RegistrationNo;
                //case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:Name": return wfseller.BankName;
                //case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cac:FinancialInstitution/cbc:ID": return  wfseller.PaymentType=="fb" ? wfseller.BIC : "";

                //Handles one national or one international or one FIK paymentmean
                //Paymenttype=="fb" means foreign banktransfer, "bb" means bank to bank (National), "71" means FIK cardtype 71. Defaults to bb
                case "root:Invoice/cac:PaymentMeans/cbc:ID": return "1";
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentMeansCode": return wfseller.PaymentType == "fb" ? "31" : wfseller.PaymentType == "71" ? "93" : "42";
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentDueDate": return wforder.PayDate == DateTime.MaxValue ? "" : XmlConvert.ToString(wforder.PayDate, "yyyy-MM-dd");
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode": return wfseller.PaymentType == "fb" ? "IBAN" : wfseller.PaymentType == "71" ? "" : "DK:BANK";
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listID": return wfseller.PaymentType == "71" ? "" : "urn:oioubl:codelist:paymentchannelcode-1.1";
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listAgencyID": return wfseller.PaymentType == "71" ? "" : "320";
                case "root:Invoice/cac:PaymentMeans/cbc:InstructionID": return wfseller.PaymentType == "71" ? wforder.PaymentRef : "";
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentID": return wfseller.PaymentType == "71" ? "71" : "";
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentID/@schemeAgencyID": return wfseller.PaymentType == "71" ? "320" : "";
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentID/@schemeID": return wfseller.PaymentType == "71" ? "urn:oioubl:id:paymentid-1.1" : "";
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID": return wfseller.PaymentType == "fb" ? wfseller.IBAN : wfseller.PaymentType == "71" ? "" : wfseller.AccountNo;
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:Name": return wfseller.PaymentType == "71" ? "" : wfseller.BankName;
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote": return wfseller.PaymentType == "71" ? "" : wforder.PaymentRef;
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID": return wfseller.PaymentType == "bb" ? wfseller.RegistrationNo : "";
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:Name": return wfseller.PaymentType == "71" ? "" : wfseller.BankName;
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cac:FinancialInstitution/cbc:ID": return wfseller.PaymentType == "fb" ? wfseller.BIC : "";
                case "root:Invoice/cac:PaymentMeans/cac:CreditAccount/cbc:AccountID": return wfseller.PaymentType == "71" ? wfseller.CreditAccount : "";

                case "root:Invoice/cac:PaymentTerms/cbc:ID": return "1";
                case "root:Invoice/cac:PaymentTerms/cbc:PaymentMeansID": return "1";
                case "root:Invoice/cac:PaymentTerms/cbc:Amount": return CurrencyString(wforder.Total + wforder.TotalVatEx + wforder.TotalVatIn - wfpayment.amount);  //samme som "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount"
                case "root:Invoice/cac:PaymentTerms/cbc:Amount/@currencyID": return wforder.Currency;
                // hertil

                case "root:Invoice/cac:PrepaidPayment/cbc:PaidAmount": return (wfpayment.amount == 0) ? "" : CurrencyString(wfpayment.amount);
                case "root:Invoice/cac:PrepaidPayment/cbc:PaidAmount/@currencyID": return (wfpayment.amount == 0) ? "" : wforder.Currency;
                case "root:Invoice/cac:PrepaidPayment/cbc:PaidDate": return (wfpayment.PaidDate == DateTime.MinValue) ? "" : XmlConvert.ToString(wfpayment.PaidDate, "yyyy-MM-dd");
                case "root:Invoice/cac:PrepaidPayment/cbc:InstructionID": return wfpayment.PaymentRef;
                case "root:Invoice/cac:TaxTotal/cbc:TaxAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:Invoice/cac:TaxTotal/cbc:TaxAmount/@currencyID": return wforder.Currency;
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount": return CurrencyString(wforder.TotalVatBasisEx + (wforder.TotalVatBasisIn-wforder.TotalVatIn));
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": return wforder.Currency; 
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID": return wforder.Currency;
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID": return VatRate(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID": return "urn:oioubl:id:taxcategoryid-1.1";
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID": return "320";
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent": return (VatRate(wforder.TotalVatEx + wforder.TotalVatIn) == "StandardRated") ? VatRateHead(SubSystem.Sale) : "0.00";  // HACK Grouping of tax-rates needs to be made in WF
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID": return "63";
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return "320";
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.1";
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount": return CurrencyString(wforder.AmountLines);
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID": return wforder.Currency;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount/@currencyID": return wforder.Currency;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount": return CurrencyString(wforder.AmountLines + wforder.TotalVatEx + wforder.TotalVatIn + wforder.AmountAllowance - wforder.TotalInvDiscount + wforder.AmountCharge);
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID": return wforder.Currency;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount": return ((wforder.AmountAllowance + wforder.TotalInvDiscount) == 0) ? "" : CurrencyString((wforder.AmountAllowance + wforder.TotalInvDiscount));
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount/@currencyID": return ((wforder.AmountAllowance + wforder.TotalInvDiscount) == 0) ? "" : wforder.Currency;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount": return (wforder.AmountCharge == 0) ? "" : CurrencyString(wforder.AmountCharge);
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID": return (wforder.AmountCharge == 0) ? "" : wforder.Currency;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PrepaidAmount": return (wfpayment.amount == 0) ? "" : CurrencyString(wfpayment.amount);
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PrepaidAmount/@currencyID": return (wfpayment.amount == 0) ? "" : wforder.Currency;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount": return (wforder.AmountRounding==0)?"":CurrencyString(wforder.AmountRounding);
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount/@currencyID": return (wforder.AmountRounding == 0) ? "" : wforder.Currency;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount": return CurrencyString(wforder.Total + wforder.TotalVatEx + wforder.TotalVatIn - wfpayment.amount);   //samme som "root:Invoice/cac:PaymentTerms/cbc:Amount"
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID": return wforder.Currency;


                case "root:CreditNote/cbc:UBLVersionID": return "2.0";
                case "root:CreditNote/cbc:CustomizationID": return "OIOUBL-2.02";
                case "root:CreditNote/cbc:ProfileID": return "urn:www.nesubl.eu:profiles:profile5:ver2.0";
                case "root:CreditNote/cbc:ProfileID/@schemeID": return "urn:oioubl:id:profileid-1.2";
                case "root:CreditNote/cbc:ProfileID/@schemeAgencyID": return "320";
                case "root:CreditNote/cbc:ID": return wforder.InvoiceNo.ToString();
                case "root:CreditNote/cbc:ID/@schemeID": return wfcompany.GuidComp.ToString();
                case "root:CreditNote/cbc:UUID": return wforder.GuidInv.ToString();
                case "root:CreditNote/cbc:IssueDate": return XmlConvert.ToString(wforder.InvoiceDate, "yyyy-MM-dd");
                case "root:CreditNote/cbc:IssueTime": return "";
                case "root:CreditNote/cbc:Note": return string.IsNullOrEmpty(wforder.text_1) & string.IsNullOrEmpty(wforder.text_2) ? "" : string.Concat(wforder.text_1, " ", wforder.text_2);
                case "root:CreditNote/cbc:DocumentCurrencyCode": return wforder.Currency;
                case "root:CreditNote/cbc:AccountingCost": return wforder.AccountingCost;
                case "root:CreditNote/cac:BillingReference/cac:InvoiceDocumentReference/cbc:ID": return wforder.SettleNo.ToString();
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID": return wfcompany.ean; //  ASPEndPoint();
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID": return wfcompany.endpointtype; //ASPEndPointScheme();
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID": return wfcompany.ean; // string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return wfcompany.endpointtype; // string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name": return wfcompany.CompanyName;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfcompany.Street;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfcompany.HouseNumber;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return wfcompany.InHouseMail;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfcompany.CityName;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfcompany.PostalZone;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country);
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.TaxSchemeID) ? "" : wfcompany.TaxSchemeID;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.TaxSchemeID) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLTaxScheme, wfcompany.Country, "ZZZ");
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return "63";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.2";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return wfcompany.CompanyName;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");
                case "root:CreditNote/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID": return BillTo.AddressID.ToString();
                //case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "N/A" : BillTo.ean;
                //case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "DK:VANS" : "GLN";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID": return string.IsNullOrEmpty(wforder.UBLEndpointID) ? "" : wforder.UBLEndpointID; 
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(wforder.UBLEndpointID)) ? "" : (string.IsNullOrEmpty(wforder.UBLEndpointScheme)) ? "GLN" : wforder.UBLEndpointScheme;
                //case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID": return BillTo.Account;
                //case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return "ZZZ";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID": return string.IsNullOrEmpty(wforder.UBLEndpointID) ? "" : wforder.UBLEndpointID;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return (string.IsNullOrEmpty(wforder.UBLEndpointID)) ? "" : (string.IsNullOrEmpty(wforder.UBLEndpointScheme)) ? "GLN" : wforder.UBLEndpointScheme;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name": return BillTo.CompanyName;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID": return Konv.convLanguage(LanguageIDType.PreferredForCountryVC, LanguageIDType.ISO_1, BillTo.Language);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredLax";                  //TODO  intention to use StructuredDK - still has buildingnumber etc
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": return BillTo.Address1;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return BillTo.HouseNumber;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return BillTo.InHouseMail;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Department": return BillTo.Department;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": return BillTo.City;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return BillTo.PostalCode;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, BillTo.CountryID);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return ValidateCompNo(); // string.IsNullOrEmpty(BillTo.VATNumber) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), BillTo.VATNumber);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : (BillTo.CountryID == "DK" ? "DK:SE" : "ZZZ");
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "63";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeAgencyID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "320";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "urn:oioubl:id:taxschemeid-1.1";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "Moms";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return ValidateCompNo(); // string.IsNullOrEmpty(BillTo.VATNumber) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), BillTo.VATNumber);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : (BillTo.CountryID == "DK" ? "DK:CVR" : "ZZZ");  //Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, BillTo.CountryID, "ZZZ");
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ID": return wforder.Initials;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Name": return wforder.UBLContactName;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": return wforder.UBLemail;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FirstName": return BillTo.CompanyName;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FamilyName": return BillTo.LastName;

                case "root:CreditNote/cac:PrepaidPayment/cbc:PaidAmount": return (wfpayment.amount == 0) ? "" : CurrencyString(wfpayment.amount);
                case "root:CreditNote/cac:PrepaidPayment/cbc:PaidAmount/@currencyID": return (wfpayment.amount == 0) ? "" : wforder.Currency;
                case "root:CreditNote/cac:PrepaidPayment/cbc:PaidDate": return (wfpayment.PaidDate == null) ? "" : XmlConvert.ToString(wfpayment.PaidDate, "yyyy-MM-dd");
                case "root:CreditNote/cac:TaxTotal/cbc:TaxAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:CreditNote/cac:TaxTotal/cbc:TaxAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount": return CurrencyString(wforder.TotalVatBasisEx + (wforder.TotalVatBasisIn - wforder.TotalVatIn));
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID": return VatRate(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID": return "urn:oioubl:id:taxcategoryid-1.1";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID": return "320";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent": return (VatRate(wforder.TotalVatEx + wforder.TotalVatIn) == "StandardRated") ? VatRateHead(SubSystem.Sale) : "0.00";  // HACK Grouping of tax-rates needs to be made in WF
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID": return "63";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return "320";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.1";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount": return CurrencyString(wforder.AmountLines);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount": return CurrencyString(wforder.AmountLines + wforder.TotalVatEx + wforder.TotalVatIn + wforder.AmountAllowance + wforder.AmountCharge);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount": return (wforder.AmountCharge == 0) ? "" : CurrencyString(wforder.AmountCharge);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID": return (wforder.AmountCharge == 0) ? "" : wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount": return (wforder.AmountRounding == 0) ? "" : CurrencyString(wforder.AmountRounding);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount/@currencyID": return (wforder.AmountRounding == 0) ? "" : wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount": return CurrencyString(wforder.Total + wforder.TotalVatEx + wforder.TotalVatIn);   //samme som "root:CreditNote/cac:PaymentTerms/cbc:Amount"
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID": return wforder.Currency;

                default: return "";
            }
        }

        public string GetInvoiceAllowanceChargeValue(long allowanceChargeId, string xpath)
        {
            var orderitems = from n in wforder.OrderLines where n.Liid == allowanceChargeId select n;
            OrderLine orderitem = orderitems.FirstOrDefault();

                switch(xpath){
                case "cbc:ID": return (orderitem == null) ? "" : allowanceChargeId.ToString(); 
                case "cbc:ChargeIndicator": return (orderitem.OrderAmount < 0) ? "false" : "true";
                case "cbc:AllowanceChargeReasonCode": return (orderitem == null) ? "" : orderitem.ItemID;
                case "cbc:AllowanceChargeReason":	return (orderitem == null) ? "" : orderitem.ItemDesc;
                case "cbc:Amount":	return (orderitem == null) ? "" : CurrencyString(Math.Abs(orderitem.LineAmount));
                case "cbc:Amount/@currencyID":	return wforder.Currency;
                case "cac:TaxCategory/cbc:Percent": return CurrencyString(orderitem.vat_perc);
                case "cac:TaxCategory/cbc:ID": return (orderitem == null) ? "" : VatRate(orderitem.VatIncl);
                case "cac:TaxCategory/cbc:ID/@schemeID":	return "urn:oioubl:id:taxcategoryid-1.1";
                case "cac:TaxCategory/cbc:ID/@schemeAgencyID":	return "320";
                case "cac:TaxCategory/cac:TaxScheme/cbc:ID":	return "63";
                case "cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID":	return "320";
                case "cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID":	return "urn:oioubl:id:taxschemeid-1.1";
                case "cac:TaxCategory/cac:TaxScheme/cbc:Name":	return "Moms";
                    default:	return "";
                }	
        }

        public string GetInvoiceLineValue(long lineId, string xpath)
        {
            var orderitems = from n in wforder.OrderLines where n.Liid == lineId select n;
            OrderLine orderitem = orderitems.FirstOrDefault();

            switch (xpath)
            {
                case "cbc:InvoicedQuantity": return (decimal.Round(decimal.Multiply(orderitem.Qty, orderitem.QtyPackages),4)).ToString(CultureInfo.InvariantCulture);
                case "cbc:InvoicedQuantity/@unitCode": return "EA";                             // TODO : Omform wf-unit til urn:un:unece:uncefact:codelist:specification:66411:2001
                case "cbc:LineExtensionAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.LineAmount);
                case "cbc:LineExtensionAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cbc:AccountingCost": return orderitem.AccountingCost;
                case "cac:PricingReference/cac:AlternativeConditionPrice/cbc:PriceAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.SalesPrice);
                case "cac:PricingReference/cac:AlternativeConditionPrice/cbc:PriceAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cac:PricingReference/cac:AlternativeConditionPrice/cbc:BaseQuantity": return "1";
                case "cac:PricingReference/cac:AlternativeConditionPrice/cbc:PriceTypeCode": return "AAB";

                case "cac:Delivery/cbc:ActualDeliveryDate": return (orderitem.ActualDeliveryDate == DateTime.MinValue) ? "" : XmlConvert.ToString(orderitem.ActualDeliveryDate, "yyyy-MM-dd");

                //linjerabat
                case "cac:AllowanceCharge/cbc:ID": return (orderitem.DiscountProc != 0) ? "1" : "";
                case "cac:AllowanceCharge/cbc:ChargeIndicator": return (orderitem.DiscountProc != 0) ? "false" : "";
                case "cac:AllowanceCharge/cbc:AllowanceChargeReasonCode": return (orderitem.DiscountProc != 0) ? "DI" : "";
                case "cac:AllowanceCharge/cbc:AllowanceChargeReason": return (orderitem.DiscountProc != 0) ? "Rabat" : "";
                case "cac:AllowanceCharge/cbc:MultiplierFactorNumeric": return (orderitem.DiscountProc != 0) ? (orderitem.DiscountProc / 100).ToString("#0.0000", CultureInfo.InvariantCulture) : "";
                case "cac:AllowanceCharge/cbc:Amount": return (orderitem.DiscountProc != 0) ? CurrencyString(orderitem.LineDiscount) : "";
                case "cac:AllowanceCharge/cbc:Amount/@currencyID": return (orderitem.DiscountProc != 0) ? wforder.Currency : "";
                case "cac:AllowanceCharge/cbc:BaseAmount": return (orderitem.DiscountProc != 0) ? CurrencyString(orderitem.LineAmount + orderitem.LineDiscount) : "";
                case "cac:AllowanceCharge/cbc:BaseAmount/@currencyID": return (orderitem.DiscountProc != 0) ? wforder.Currency : "";
                case "cac:AllowanceCharge/cac:TaxCategory/cbc:ID": return (orderitem.DiscountProc != 0) ? VatRate(orderitem.vat_perc, orderitem.VatIncl) : "";
                case "cac:AllowanceCharge/cac:TaxCategory/cbc:ID/@schemeID": return (orderitem.DiscountProc != 0) ? "urn:oioubl:id:taxcategoryid-1.1" : "";
                case "cac:AllowanceCharge/cac:TaxCategory/cbc:ID/@schemeAgencyID": return (orderitem.DiscountProc != 0) ? "320" : "";
                case "cac:AllowanceCharge/cac:TaxCategory/cbc:Percent": return (orderitem.DiscountProc != 0) ? CurrencyString(orderitem.vat_perc) : "";
                case "cac:AllowanceCharge/cac:TaxCategory/cac:TaxScheme/cbc:ID": return (orderitem.DiscountProc != 0) ? "63" :"";
                case "cac:AllowanceCharge/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return (orderitem.DiscountProc != 0) ? "urn:oioubl:id:taxschemeid-1.1" :"";
                case "cac:AllowanceCharge/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return (orderitem.DiscountProc != 0) ? "320" :"";
                case "cac:AllowanceCharge/cac:TaxCategory/cac:TaxScheme/cbc:Name": return (orderitem.DiscountProc != 0) ? "Moms" : "";

                case "cac:TaxTotal/cbc:TaxAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.LineVat);
                case "cac:TaxTotal/cbc:TaxAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.LineVatBase);
                case "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.LineVat);
                case "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID": return VatRate(orderitem.vat_perc, orderitem.VatIncl);
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID": return "urn:oioubl:id:taxcategoryid-1.1";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID": return "320";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent": return CurrencyString(orderitem.vat_perc);
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID": return "63";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.1";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return "320";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name": return "Moms";
                case "cac:Item/cbc:Description": return (orderitem == null) ? "" : orderitem.ItemDesc;
                case "cac:Item/cbc:Name": return (orderitem == null) ? "" : new string(orderitem.ItemDesc.Take(40).ToArray());
                case "cac:Item/cac:SellersItemIdentification/cbc:ID": return (orderitem == null) ? "" : orderitem.ItemID;
                case "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode": return string.IsNullOrEmpty(orderitem.UNSPSC) ? "" : orderitem.UNSPSC;
                case "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listID": return string.IsNullOrEmpty(orderitem.UNSPSC) ? "" : "UNSPSC";
                case "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listAgencyID": return string.IsNullOrEmpty(orderitem.UNSPSC) ? "" : "113";
                case "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listVersionID": return string.IsNullOrEmpty(orderitem.UNSPSC) ? "" : "7.0401";
                case "cac:Item/cac:ItemInstance/cac:LotIdentification/cbc:LotNumberID": return orderitem.Batch;
                case "cac:Price/cbc:PriceAmount": return CurrencyString4(orderitem.LinePrice); // (orderitem.Qty == 0) ? "" : CurrencyString(orderitem.LineAmount / orderitem.Qty);            
                case "cac:Price/cbc:PriceAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;

                case "cbc:CreditedQuantity": return (decimal.Round(decimal.Multiply(orderitem.Qty, orderitem.QtyPackages), 4)).ToString(CultureInfo.InvariantCulture);
                case "cbc:CreditedQuantity/@unitCode": return "EA";                             // TODO : Omform wf-unit til urn:un:unece:uncefact:codelist:specification:66411:2001

                default: return "";
            }
        }

        public string GetSelfBilledValue(string xpath)
        {
            //TODO: tget the object by the document ID      

            wfws.LookUp Konv = new wfws.LookUp();
            switch (xpath)
            {
                case "root:SelfBilledInvoice/cbc:UBLVersionID": return "2.0";
                case "root:SelfBilledInvoice/cbc:CustomizationID": return "OIOUBL-2.02";
                case "root:SelfBilledInvoice/cbc:ProfileID": return "urn:www.nesubl.eu:profiles:profile5:ver2.0";
                case "root:SelfBilledInvoice/cbc:ProfileID/@schemeID": return "urn:oioubl:id:profileid-1.2";
                case "root:SelfBilledInvoice/cbc:ProfileID/@schemeAgencyID": return "320";
                case "root:SelfBilledInvoice/cbc:ID": return wfpurc.InvoiceNo.ToString();
                case "root:SelfBilledInvoice/cbc:ID/@schemeID": return wfcompany.GuidComp.ToString();
                case "root:SelfBilledInvoice/cbc:UUID": return wfpurc.GuidInv.ToString();
                case "root:SelfBilledInvoice/cbc:IssueDate": return XmlConvert.ToString(wfpurc.InvoiceDate, "yyyy-MM-dd");
                case "root:SelfBilledInvoice/cbc:IssueTime": return "";
                case "root:SelfBilledInvoice/cbc:DocumentCurrencyCode": return wfpurc.Currency;
                case "root:SelfBilledInvoice/cbc:AccountingCost": return wfpurc.AccountingCost;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "N/A" : BillTo.ean;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "DK:VANS" : "GLN";
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), ValidateCompNo());
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, BillTo.CountryID, "ZZZ");
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name": return BillTo.CompanyName;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": return BillTo.Address1;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return BillTo.HouseNumber;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return BillTo.InHouseMail;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": return BillTo.City;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return BillTo.PostalCode;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID);
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), ValidateCompNo());
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : (BillTo.CountryID == "DK" ? "DK:SE" : "ZZZ");    //One rule exception for DK. Will not be functionalized
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return "63";
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.2";
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return BillTo.CompanyName;
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), ValidateCompNo());
                case "root:SelfBilledInvoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, BillTo.CountryID, "ZZZ");
                //case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID": return BillTo.AddressID.ToString();
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID": return ASPEndPoint();
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return ASPEndPointScheme();
                //case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID": return BillTo.Account;
                //case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return "ZZZ";
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name": return wfcompany.CompanyName;
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID": return Konv.convLanguage(LanguageIDType.PreferredForCountryVC, LanguageIDType.ISO_1, wfcompany.Language);
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredLax";                  //TODO  intention to use StructuredDK - still has buildingnumber etc
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfcompany.Street;
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfcompany.HouseNumber;
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return wfcompany.InHouseMail;
                //case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Department": return  BillTo.Department;
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfcompany.CityName;
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfcompany.PostalZone;
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country);
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, wfcompany.Country);
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return wfcompany.CompanyNo;
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : (wfcompany.Country == "DK" ? "DK:SE" : "ZZZ");
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : "63";
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeAgencyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : "320";
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : "urn:oioubl:id:taxschemeid-1.1";
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : "Moms";
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return wfcompany.CompanyNo;
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, BillTo.CountryID, "ZZZ");
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ID": return "1";
               /// case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": return wfcompany;  
                case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FirstName": return wfcompany.CompanyName;
                //case "root:SelfBilledInvoice/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FamilyName": return BillTo.LastName;
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PartyName/cbc:Name": return wfseller.SellerName;
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfseller.SellerStreet;
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfseller.SellerHouseNumber;
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return wfseller.SellerInHouseMail;
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfseller.SellerCityName;
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfseller.SellerPostalZone;
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfseller.SellerCountryID);
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : wfseller.SellerName;
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfseller.SellerCountryID), wfseller.CompanyNo);
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfseller.SellerCountryID, "ZZZ");
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:Contact/cbc:ID": return "1";
                case "root:SelfBilledInvoice/cac:BuyerCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": return wfseller.Email;

/*                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode": return "StructuredLax";
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:StreetName": return ShipTo.Address1;
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:CityName": return ShipTo.City;
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:PostalZone": return ShipTo.PostalCode;
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, ShipTo.CountryID);
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, ShipTo.CountryID);
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryParty/cac:PartyIdentification/cbc:ID": return ShipTo.Account;
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryParty/cac:PartyIdentification/cbc:ID/@schemeID": return "ZZZ";
                case "root:SelfBilledInvoice/cac:Delivery/cac:DeliveryParty/cac:PartyName/cbc:Name": return (ShipTo.CompanyName == BillTo.CompanyName) ? "" : ShipTo.CompanyName;
 Droppet da delivery ikke giver mening, når varen er indleveret hos Lauritz. Koden bevares hvis den skal give mening i andre firmaer*/

                // TODO This section hardcoded to bank to bank only
                case "root:SelfBilledInvoice/cac:PaymentMeans/cbc:ID": return "1";
                case "root:SelfBilledInvoice/cac:PaymentMeans/cbc:PaymentMeansCode": return "42";
                case "root:SelfBilledInvoice/cac:PaymentMeans/cbc:PaymentDueDate": return (wfpurc.PayDate == null) ? "" : XmlConvert.ToString(wfpurc.PayDate, "yyyy-MM-dd");
                case "root:SelfBilledInvoice/cac:PaymentMeans/cbc:PaymentChannelCode": return "DK:BANK";
                case "root:SelfBilledInvoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listID": return "urn:oioubl:codelist:paymentchannelcode-1.1";
                case "root:SelfBilledInvoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listAgencyID": return "320";
                case "root:SelfBilledInvoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID": return wfseller.AccountNo;
                case "root:SelfBilledInvoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote": return wfpurc.PaymentRef;
                case "root:SelfBilledInvoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID": return wfseller.RegistrationNo;
                case "root:SelfBilledInvoice/cac:PaymentTerms/cbc:ID": return "1";
                case "root:SelfBilledInvoice/cac:PaymentTerms/cbc:PaymentMeansID": return "1";
                case "root:SelfBilledInvoice/cac:PaymentTerms/cbc:Amount": return CurrencyString(wfpurc.Total + wfpurc.TotalVatEx + wfpurc.TotalVatIn);  //samme som "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:PayableAmount"
                case "root:SelfBilledInvoice/cac:PaymentTerms/cbc:Amount/@currencyID": return wfpurc.Currency;
                // hertil

                case "root:SelfBilledInvoice/cac:PrepaidPayment/cbc:PaidAmount": return (wfpurc.AmountPaid == 0) ? "" : CurrencyString(wfpurc.AmountPaid);
                case "root:SelfBilledInvoice/cac:PrepaidPayment/cbc:PaidAmount/@currencyID": return (wfpurc.AmountPaid == 0) ? "" : wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:PrepaidPayment/cbc:PaidDate": return (wfpurc.PaidDate == null) ? "" : XmlConvert.ToString(wfpurc.PaidDate, "yyyy-MM-dd");
                case "root:SelfBilledInvoice/cac:TaxTotal/cbc:TaxAmount": return CurrencyString(wfpurc.TotalVatEx + wfpurc.TotalVatIn);
                case "root:SelfBilledInvoice/cac:TaxTotal/cbc:TaxAmount/@currencyID": return wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount": return CurrencyString(wfpurc.TotalVatBasisEx + (wfpurc.TotalVatBasisIn - wfpurc.TotalVatIn));
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": return wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID": return wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount": return CurrencyString(wfpurc.TotalVatEx + wfpurc.TotalVatIn);
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID": return VatRate(wfpurc.TotalVatEx + wfpurc.TotalVatIn);
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID": return "urn:oioubl:id:taxcategoryid-1.1";
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID": return "320";
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent": return (VatRate(wfpurc.TotalVatEx + wfpurc.TotalVatIn) == "StandardRated") ? VatRateHead(SubSystem.Purchase) : "0.00";  // HACK Grouping of tax-rates needs to be made in WF
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID": return "63";
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return "320";
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.1";
                case "root:SelfBilledInvoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount": return CurrencyString(wfpurc.AmountLines);
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID": return wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount": return CurrencyString(wfpurc.TotalVatEx + wfpurc.TotalVatIn);
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount/@currencyID": return wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount": return CurrencyString(wfpurc.AmountLines + wfpurc.TotalVatEx + wfpurc.TotalVatIn + wfpurc.AmountAllowance + wfpurc.AmountCharge);
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID": return wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount": return (wfpurc.AmountCharge == 0) ? "" : CurrencyString(wfpurc.AmountCharge);
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID": return (wfpurc.AmountCharge == 0) ? "" : wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount": return (wfpurc.AmountRounding == 0) ? "" : CurrencyString(wfpurc.AmountRounding);
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount/@currencyID": return (wfpurc.AmountRounding == 0) ? "" : wfpurc.Currency;
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:PayableAmount": return CurrencyString(wfpurc.Total + wfpurc.TotalVatEx + wfpurc.TotalVatIn);   //samme som "root:SelfBilledInvoice/cac:PaymentTerms/cbc:Amount"
                case "root:SelfBilledInvoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID": return wfpurc.Currency;


                case "root:SelfBilledCreditNote/cbc:UBLVersionID": return "2.0";
                case "root:SelfBilledCreditNote/cbc:CustomizationID": return "OIOUBL-2.02";
                case "root:SelfBilledCreditNote/cbc:ProfileID": return "urn:www.nesubl.eu:profiles:profile5:ver2.0";
                case "root:SelfBilledCreditNote/cbc:ProfileID/@schemeID": return "urn:oioubl:id:profileid-1.2";
                case "root:SelfBilledCreditNote/cbc:ProfileID/@schemeAgencyID": return "320";
                case "root:SelfBilledCreditNote/cbc:ID": return wforder.InvoiceNo.ToString();
                case "root:SelfBilledCreditNote/cbc:ID/@schemeID": return wfcompany.GuidComp.ToString();
                case "root:SelfBilledCreditNote/cbc:UUID": return wforder.GuidInv.ToString();
                case "root:SelfBilledCreditNote/cbc:IssueDate": return XmlConvert.ToString(wforder.InvoiceDate, "yyyy-MM-dd");
                case "root:SelfBilledCreditNote/cbc:IssueTime": return "";
                case "root:SelfBilledCreditNote/cbc:DocumentCurrencyCode": return wforder.Currency;
                case "root:SelfBilledCreditNote/cac:BillingReference/cac:InvoiceDocumentReference/cbc:ID": return wforder.SettleNo.ToString();
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID": return ASPEndPoint();
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID": return ASPEndPointScheme();
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name": return wfcompany.CompanyName;
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfcompany.Street;
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfcompany.HouseNumber;
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return wfcompany.InHouseMail;
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfcompany.CityName;
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfcompany.PostalZone;
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country);
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : (wfcompany.Country == "DK" ? "DK:SE" : "ZZZ");    //One rule exception for DK. Will not be functionalized
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return "63";
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.2";
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return wfcompany.CompanyName;
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:SelfBilledCreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID": return BillTo.AddressID.ToString();
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "N/A" : BillTo.ean;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "DK:VANS" : "GLN";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID": return BillTo.Account;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return "ZZZ";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name": return BillTo.CompanyName;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID": return Konv.convLanguage(LanguageIDType.PreferredForCountryVC, LanguageIDType.ISO_1, BillTo.Language);
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredLax";                  //TODO  intention to use StructuredDK - still has buildingnumber etc
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": return BillTo.Address1;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return BillTo.HouseNumber;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return BillTo.InHouseMail;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Department": return BillTo.Department;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": return BillTo.City;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return BillTo.PostalCode;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID);
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, BillTo.CountryID);
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return ValidateCompNo(); // string.IsNullOrEmpty(BillTo.VATNumber) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), BillTo.VATNumber);
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : (BillTo.CountryID == "DK" ? "DK:SE" : "ZZZ");
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "63";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeAgencyID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "320";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "urn:oioubl:id:taxschemeid-1.1";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : "Moms";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return ValidateCompNo(); // string.IsNullOrEmpty(BillTo.VATNumber) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), BillTo.VATNumber);
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(ValidateCompNo()) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, BillTo.CountryID, "ZZZ");
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ID": return "1";
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": return BillTo.email;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FirstName": return BillTo.CompanyName;
                case "root:SelfBilledCreditNote/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FamilyName": return BillTo.LastName;

                case "root:SelfBilledCreditNote/cac:PrepaidPayment/cbc:PaidAmount": return (wfpayment.amount == 0) ? "" : CurrencyString(wfpayment.amount);
                case "root:SelfBilledCreditNote/cac:PrepaidPayment/cbc:PaidAmount/@currencyID": return (wfpayment.amount == 0) ? "" : wforder.Currency;
                case "root:SelfBilledCreditNote/cac:PrepaidPayment/cbc:PaidDate": return (wfpayment.PaidDate == null) ? "" : XmlConvert.ToString(wfpayment.PaidDate, "yyyy-MM-dd");
                case "root:SelfBilledCreditNote/cac:TaxTotal/cbc:TaxAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:SelfBilledCreditNote/cac:TaxTotal/cbc:TaxAmount/@currencyID": return wforder.Currency;
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount": return CurrencyString(wforder.TotalVatBasisEx + (wforder.TotalVatBasisIn - wforder.TotalVatIn));
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": return wforder.Currency;
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID": return wforder.Currency;
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID": return VatRate(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID": return "urn:oioubl:id:taxcategoryid-1.1";
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID": return "320";
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent": return (VatRate(wforder.TotalVatEx + wforder.TotalVatIn) == "StandardRated") ? VatRateHead(SubSystem.Purchase) : "0.00";  // HACK Grouping of tax-rates needs to be made in WF
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID": return "63";
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return "320";
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.1";
                case "root:SelfBilledCreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount": return CurrencyString(wforder.AmountLines);
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID": return wforder.Currency;
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount/@currencyID": return wforder.Currency;
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount": return CurrencyString(wforder.AmountLines + wforder.TotalVatEx + wforder.TotalVatIn + wforder.AmountAllowance + wforder.AmountCharge);
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID": return wforder.Currency;
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount": return (wforder.AmountCharge == 0) ? "" : CurrencyString(wforder.AmountCharge);
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID": return (wforder.AmountCharge == 0) ? "" : wforder.Currency;
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount": return (wforder.AmountRounding == 0) ? "" : CurrencyString(wforder.AmountRounding);
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount/@currencyID": return (wforder.AmountRounding == 0) ? "" : wforder.Currency;
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount": return CurrencyString(wforder.Total + wforder.TotalVatEx + wforder.TotalVatIn);   //samme som "root:SelfBilledCreditNote/cac:PaymentTerms/cbc:Amount"
                case "root:SelfBilledCreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID": return wforder.Currency;

                default: return "";
            }
        }

        public string GetSelfBilledAllowanceChargeValue(long allowanceChargeId, string xpath)
        {
            var orderitems = from n in wfpurc.OrderLines where n.Liid == allowanceChargeId select n;
            OrderLinePurc orderitem = orderitems.FirstOrDefault();

            switch (xpath)
            {
                case "cbc:ID": return (orderitem == null) ? "" : allowanceChargeId.ToString(); 
                case "cbc:ChargeIndicator": return (orderitem.OrderAmount < 0) ? "false" : "true";
                case "cbc:AllowanceChargeReasonCode": return (orderitem == null) ? "" : orderitem.ItemID;
                case "cbc:AllowanceChargeReason": return (orderitem == null) ? "" : orderitem.ItemDesc;
                case "cbc:Amount": return (orderitem == null) ? "" : CurrencyString(Math.Abs(orderitem.LineAmount));
                case "cbc:Amount/@currencyID": return wforder.Currency;
                case "cac:TaxCategory/cbc:Percent": return CurrencyString(orderitem.vat_perc);
                case "cac:TaxCategory/cbc:ID": return (orderitem == null) ? "" : VatRate(orderitem.VatIncl);
                case "cac:TaxCategory/cbc:ID/@schemeID": return "urn:oioubl:id:taxcategoryid-1.1";
                case "cac:TaxCategory/cbc:ID/@schemeAgencyID": return "320";
                case "cac:TaxCategory/cac:TaxScheme/cbc:ID": return "63";
                case "cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return "320";
                case "cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.1";
                case "cac:TaxCategory/cac:TaxScheme/cbc:Name": return "Moms";
                default: return "";
            }
        }

        public string GetSelfBilledLineValue(long lineId, string xpath)
        {
            var orderitems = from n in wfpurc.OrderLines where n.Liid == lineId select n;
            OrderLinePurc orderitem = orderitems.FirstOrDefault();

            switch (xpath)
            {

                case "cbc:InvoicedQuantity": return orderitem.Qty.ToString(CultureInfo.InvariantCulture);
                case "cbc:InvoicedQuantity/@unitCode": return "EA";                             // TODO : Omform wf-unit til urn:un:unece:uncefact:codelist:specification:66411:2001
                case "cbc:LineExtensionAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.LineAmount);
                case "cbc:LineExtensionAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cbc:AccountingCost": return orderitem.AccountingCost;
                case "cac:TaxTotal/cbc:TaxAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.LineVat);
                case "cac:TaxTotal/cbc:TaxAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.LineVatBase);
                case "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount": return (orderitem == null) ? "" : CurrencyString(orderitem.LineVat);
                case "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID": return VatRate(orderitem.vat_perc, orderitem.VatIncl);
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID": return "urn:oioubl:id:taxcategoryid-1.1";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID": return "320";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent": return CurrencyString(orderitem.vat_perc);
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID": return "63";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.1";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return "320";
                case "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name": return "Moms";
                case "cac:Item/cbc:Description": return (orderitem == null) ? "" : orderitem.ItemDesc;
                case "cac:Item/cbc:Name": return (orderitem == null) ? "" : new string(orderitem.ItemDesc.Take(40).ToArray());
                case "cac:Item/cac:SellersItemIdentification/cbc:ID": return (orderitem == null) ? "" : orderitem.ItemID;
                case "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode": return string.IsNullOrEmpty(orderitem.UNSPSC) ? "" : orderitem.UNSPSC;
                case "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listID": return string.IsNullOrEmpty(orderitem.UNSPSC) ? "" : "UNSPSC";
                case "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listAgencyID": return string.IsNullOrEmpty(orderitem.UNSPSC) ? "" : "113";
                case "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listVersionID": return string.IsNullOrEmpty(orderitem.UNSPSC) ? "" : "7.0401";
                case "cac:Price/cbc:PriceAmount": return (orderitem.Qty == 0) ? "" : CurrencyString(orderitem.LineAmount / orderitem.Qty);
                case "cac:Price/cbc:PriceAmount/@currencyID": return (orderitem == null) ? "" : wforder.Currency;

                case "cbc:CreditedQuantity": return orderitem.Qty.ToString(CultureInfo.InvariantCulture);
                case "cbc:CreditedQuantity/@unitCode": return "EA";                             // TODO : Omform wf-unit til urn:un:unece:uncefact:codelist:specification:66411:2001

                default: return "";
            }
        }

        public string GetAddressStatementValue(string xpath)
        {
            wfws.LookUp Konv = new wfws.LookUp();
            switch (xpath)
            {
                case "root:Statement/cbc:UBLVersionID": return "2.0";
                case "root:Statement/cbc:CustomizationID": return "OIOUBL-2.02";
                case "root:Statement/cbc:ProfileID": return "Procurement-OrdSim-BilSim-1.0";
                case "root:Statement/cbc:ProfileID/@schemeID": return "urn:oioubl:id:profileid-1.3";
                case "root:Statement/cbc:ProfileID/@schemeAgencyID": return "320";
                case "root:Statement/cbc:ID": return "1";
                case "root:Statement/cbc:UUID": return "";
                case "root:Statement/cbc:IssueDate": return wfws.wfsh.Left(DateTime.Today.ToString("s"), 10);
                case "root:Statement/cbc:Note": return "";
                case "root:Statement/cbc:DocumentCurrencyCode": return "DKK";   //should be Company currency 
                case "root:Statement/cbc:TotalDebitAmount": return CurrencyString(wfadrState.TotalDeb);
                case "root:Statement/cbc:TotalDebitAmount/@currencyID": return (wfadrState.TotalDeb==0) ? "" : "DKK";
                case "root:Statement/cbc:TotalCreditAmount": return CurrencyString(wfadrState.TotalCre);
                case "root:Statement/cbc:TotalCreditAmount/@currencyID": return (wfadrState.TotalCre==0) ? "" : "DKK";
                case "root:Statement/cbc:TotalBalanceAmount": return CurrencyString(wfadrState.TotalDeb - wfadrState.TotalCre);
                case "root:Statement/cbc:TotalBalanceAmount/@currencyID": return "DKK";
                case "root:Statement/cac:StatementPeriod/cbc:StartDate": return wfws.wfsh.Left(wfadrState.FromDate.ToString("s"), 10);
                case "root:Statement/cac:StatementPeriod/cbc:EndDate": return wfws.wfsh.Left(wfadrState.ToDate.ToString("s"), 10);
                case "root:Statement/cac:StatementPeriod/cbc:Description": return "";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID": return ASPEndPoint();
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID": return ASPEndPointScheme();
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name": return wfcompany.CompanyName;
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfcompany.Street;
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfcompany.HouseNumber;
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfcompany.PostalZone;
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country);
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name": return "";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : (wfcompany.Country == "DK" ? "DK:SE" : "ZZZ");    //One rule exception for DK. Will not be functionalized
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return "63";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.2";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return "Moms";

                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:ID": return "";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:Name": return "";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:Telephone": return "";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:Telefax": return "";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail": return "";
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return wfcompany.CompanyName;
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:Statement/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");

                case "root:Statement/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID": return wfadrState.AddressID.ToString();
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID": return wfadrState.FullAddress.ean;
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(wfadrState.FullAddress.ean)) ? "" : wfadrState.FullAddress.EndpointType;
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name": return wfadrState.FullAddress.CompanyName;

                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredLax";                  //TODO  intention to use StructuredDK - still has buildingnumber etc
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";

                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfadrState.FullAddress.Address1;
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfadrState.FullAddress.HouseNumber;
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfadrState.FullAddress.City;
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfadrState.FullAddress.PostalCode;
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfadrState.FullAddress.CountryID);;
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, wfadrState.FullAddress.CountryID);
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return "";
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return "";
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:TaxTypeCode": return "";
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Name": return "";
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Telephone": return "";
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Telefax": return "";
                case "root:Statement/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": return "";
                default: return "";
            }
        }
        public string GetAddressStatementLineValue(long lineId, string xpath)
        {
            var adrState = from n in wfadrState.StatementLines where n.ItemID == lineId select n;
            AddressStatementLine wfadrStateLine = adrState.FirstOrDefault();

            //SourceIDs
            //0	Kladde
            //1	Salg
            //2	Indkøb
            //3	Renter
            //5	Lagerjustering
            //6	Produktion
            //10	Betaling, debitorer
            //11	Betaling, kreditorer
            //12	Importeret
            //13	Løn
            //100	Andre
            //10000	Fejlet

            switch (xpath)
            {
                case "cbc:ID": return wfadrStateLine.ItemID.ToString();
                case "cbc:Note": return "";
                case "cbc:BalanceBroughtForwardIndicator": return (wfadrStateLine.BalanceBroughtForward==true) ? "true" : "";
                case "cbc:DebitLineAmount": return (wfadrStateLine.CuDebet == 0) ? "" : CurrencyString(wfadrStateLine.CuDebet);
                case "cbc:DebitLineAmount/@currencyID": return (wfadrStateLine.CuDebet == 0) ? "" : "DKK";
                case "cbc:CreditLineAmount": return (wfadrStateLine.CuCredit == 0) ? "" : CurrencyString(wfadrStateLine.CuCredit);
                case "cbc:CreditLineAmount/@currencyID": return (wfadrStateLine.CuCredit == 0) ? "" : "DKK";
                case "cac:BillingReference/cac:InvoiceDocumentReference/cbc:ID": return (wfadrStateLine.SourceID == 2) ? wfadrStateLine.SourceRef.ToString() : "";
                case "cac:BillingReference/cac:InvoiceDocumentReference/cbc:IssueDate": return (wfadrStateLine.SourceID ==2) ? ((wfadrStateLine.Enterdate == null) ? "" : XmlConvert.ToString(wfadrStateLine.Enterdate, "yyyy-MM-dd")) : "";
                case "cac:BillingReference/cac:CreditNoteDocumentReference/cbc:ID": return ((wfadrStateLine.SourceID != 1) & (wfadrStateLine.SourceID != 2) & (wfadrStateLine.CuDebet-wfadrStateLine.CuCredit > 0)) ? (wfadrStateLine.SourceRef.ToString()) : "";
                case "cac:BillingReference/cac:CreditNoteDocumentReference/cbc:IssueDate": return ((wfadrStateLine.SourceID != 1) & (wfadrStateLine.SourceID != 2) & (wfadrStateLine.CuDebet - wfadrStateLine.CuCredit > 0)) ? ((wfadrStateLine.Enterdate == null) ? "" : XmlConvert.ToString(wfadrStateLine.Enterdate, "yyyy-MM-dd")) : "";
                case "cac:BillingReference/cac:SelfBilledInvoiceDocumentReference/cbc:ID": return (wfadrStateLine.SourceID == 1) ? wfadrStateLine.SourceRef.ToString() : "";
                case "cac:BillingReference/cac:SelfBilledInvoiceDocumentReference/cbc:IssueDate": return (wfadrStateLine.SourceID == 1) ? ((wfadrStateLine.Enterdate == null) ? "" : XmlConvert.ToString(wfadrStateLine.Enterdate, "yyyy-MM-dd")) : "";
                case "cac:BillingReference/cac:SelfBilledCreditNoteDocumentReference/cbc:ID": return ((wfadrStateLine.SourceID != 1) & (wfadrStateLine.SourceID != 2) & (wfadrStateLine.CuDebet - wfadrStateLine.CuCredit < 0)) ? (wfadrStateLine.SourceRef.ToString()) : "";
                case "cac:BillingReference/cac:SelfBilledCreditNoteDocumentReference/cbc:IssueDate": return ((wfadrStateLine.SourceID != 1) & (wfadrStateLine.SourceID != 2) & (wfadrStateLine.CuDebet - wfadrStateLine.CuCredit < 0)) ? ((wfadrStateLine.Enterdate == null) ? "" : XmlConvert.ToString(wfadrStateLine.Enterdate, "yyyy-MM-dd")) : "";
                default: return "";
            }
        }


        public void PutInvoiceValue(string xpath, string value)
        {
            int intvalue = 0;
            int.TryParse(value,out intvalue);
            wfws.LookUp Konv = new wfws.LookUp();
            var wfcomp = new wfws.Company(ref MapUser);

            switch (xpath)
            {
                case "root:Invoice/cbc:ID": wforder.InvoiceNo = Convert.ToInt32(value); break;
                //case "root:Invoice/cbc:UUID": wforder.GuidInv = Convert.  value;
                case "root:Invoice/cbc:IssueDate": wforder.InvoiceDate = StringDate(value); break;
                case "root:Invoice/cbc:DocumentCurrencyCode": wforder.Currency = value; break;  //TODO check currency mod WF tbl fi_Currencies
                case "root:Invoice/cbc:AccountingCost": wforder.AccountingCost = value; break;

                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID": wfcompany.CompanyNo = value.Substring(value.Length-8); break;
                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name": wfcompany.CompanyName = value; break;
                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": wfcompany.Street = value; break;
                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": wfcompany.HouseNumber = value; break;
                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": wfcompany.InHouseMail = value; break;
                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": wfcompany.CityName = value; break;
                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": wfcompany.PostalZone = value; break;
                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": wfcompany.Country = Konv.convCountry(CountryIDType.ISO_2, CountryIDType.VehicleCode, value); break;
                ////case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": wfcompany.CompanyNo = value; break;
                //case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": wfcompany.CompanyName = value; break;
                case "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": wfcompany.CompanyNo = value; break;
                //case "root:Invoice/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID": BillTo.AddressID = Convert.ToInt32(value); wforder.BillTo = BillTo.AddressID; break;    //indeholdes sandsynligvis ikke. AddressID fremfindes under root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID": wforder.UBLEndpointID = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": wforder.UBLEndpointScheme = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID": BillTo.Account = value; BillTo.ImportID = value; BillTo.AddressID = FindAddressID(value, BillTo.AddressID); wforder.BillTo = BillTo.AddressID; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name": BillTo.CompanyName = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID": BillTo.Language = Konv.convLanguage(LanguageIDType.ISO_1, LanguageIDType.PreferredForCountryVC, value); break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": BillTo.Address1 = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": BillTo.HouseNumber = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": BillTo.InHouseMail = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Department": BillTo.Department = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": BillTo.City = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": BillTo.PostalCode = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": BillTo.CountryID = Konv.convCountry(CountryIDType.ISO_2, CountryIDType.VehicleCode, value); break;
                //case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": BillTo.VATNumber = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": BillTo.VATNumber = value; break;
                //case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": BillTo.email = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ID": wforder.Initials = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Name": wforder.UBLContactName = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": wforder.UBLemail = value; break;
                case "root:Invoice/cac:AccountingCustomerParty/cac:BuyerContact/cbc:Name": wforder.ExtRef = value; break;

                //case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FirstName": BillTo.CompanyName = value; break;
                //case "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FamilyName": BillTo.LastName = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID": wfseller.SellerNo = value; ; wfseller.SellerID = wfcomp.SellerNo2ID(value); wforder.seller = wfseller.SellerID; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyName/cbc:Name": wfseller.SellerName = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": wfseller.SellerStreet = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": wfseller.SellerHouseNumber = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": wfseller.SellerInHouseMail = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": wfseller.SellerCityName = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": wfseller.SellerPostalZone = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": wfseller.SellerCountryID = Konv.convCountry(CountryIDType.ISO_2, CountryIDType.VehicleCode, value); break;
                //case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": wfseller.SellerName = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": wfseller.CompanyNo = value; break;
                case "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail": wfseller.Email = value; break;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:StreetName": ShipTo.Address1 = value; break;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:CityName": ShipTo.City = value; break;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:PostalZone": ShipTo.PostalCode = value; break;
                case "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:IdentificationCode": ShipTo.CountryID = Konv.convCountry(CountryIDType.ISO_2, CountryIDType.VehicleCode, value); break;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyIdentification/cbc:ID": ShipTo.Account = value; ShipTo.ImportID = value; ShipTo.AddressID = FindAddressID(value, ShipTo.AddressID); wforder.ShipTo = ShipTo.AddressID; break;
                case "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyName/cbc:Name": ShipTo.CompanyName = value; break;
/*  Handled seperately
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentMeansCode": wfpaymentmeans.PaymentMeansCode = Convert.ToInt32(value); break; 
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentDueDate": wforder.PayDate = StringDate(value); break;
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentID": wfpaymentmeans.CardType = value; break;
                case "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode": wfpaymentmeans.PaymentChannelCode = value; break;
                case "root:Invoice/cac:PaymentMeans/cac:CreditAccount/cbc:AccountID": wfpaymentmeans.CreditorID = value; break;
                case "root:Invoice/cac:PaymentMeans/cbc:InstructionID": if (wfpaymentmeans.PaymentMeansCode == 48) wfpaymentmeans.MeansOfPayment = value; else wfpaymentmeans.PaymentRef = value; break;
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID": wfpaymentmeans.BankAccount = value; break;
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote": wfpaymentmeans.PaymentNote = value; break;
                case "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID": wfpaymentmeans.BankRegno = value; break;
 */
                // hertil
                    //Terms benyttes ikke hos Lauritz
                //case "root:Invoice/cac:PaymentTerms/cbc:ID": wforder.PayDate = StringDate(value); break;
                //case "root:Invoice/cac:PaymentTerms/cbc:PaymentMeansID": wforder.PayDate = StringDate(value); break;
                //case "root:Invoice/cac:PaymentTerms/cbc:Amount": wforder.PayDate = StringDate(value); break;
                //case "root:Invoice/cac:PaymentTerms/cbc:Amount/@currencyID": ValidateCurrency(value); break;

                case "root:Invoice/cac:PrepaidPayment/cbc:PaidAmount": wfpayment.amount = StringCurrency(value); break;
                case "root:Invoice/cac:PrepaidPayment/cbc:PaidAmount/@currencyID": ValidateCurrency(value); break;
                case "root:Invoice/cac:PrepaidPayment/cbc:PaidDate": wfpayment.PaidDate = StringDate(value); break;
                case "root:Invoice/cac:PrepaidPayment/cbc:InstructionID":  wfpayment.PaymentRef = value; break;  //eg TransactionID
                
                case "root:Invoice/cac:TaxTotal/cbc:TaxAmount/@currencyID": ValidateCurrency(value); break;

                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": ValidateCurrency(value); break;
                case "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID":  ValidateCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount": wforder.AmountLines = StringCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID": ValidateCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount/@currencyID": ValidateCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID": ValidateCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount": wforder.AmountCharge = StringCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID":  ValidateCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount": wforder.AmountRounding = StringCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount/@currencyID": ValidateCurrency(value); break;
                case "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID": ValidateCurrency(value); break;


/*                case "root:CreditNote/cbc:UBLVersionID": return "2.0";
                case "root:CreditNote/cbc:CustomizationID": return "OIOUBL-2.02";
                case "root:CreditNote/cbc:ProfileID": return "urn:www.nesubl.eu:profiles:profile5:ver2.0";
                case "root:CreditNote/cbc:ProfileID/@schemeID": return "urn:oioubl:id:profileid-1.2";
                case "root:CreditNote/cbc:ProfileID/@schemeAgencyID": return "320";
                case "root:CreditNote/cbc:ID": return wforder.InvoiceNo.ToString();
                case "root:CreditNote/cbc:ID/@schemeID": return wfcompany.GuidComp.ToString();
                case "root:CreditNote/cbc:UUID": return wforder.GuidInv.ToString();
                case "root:CreditNote/cbc:IssueDate": return XmlConvert.ToString(wforder.InvoiceDate, "yyyy-MM-dd");
                case "root:CreditNote/cbc:IssueTime": return "";
                case "root:CreditNote/cbc:DocumentCurrencyCode": return wforder.Currency;
                case "root:CreditNote/cac:BillingReference/cac:InvoiceDocumentReference/cbc:ID": return wforder.SettleNo.ToString();
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID": return EndPoint();
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID": return EndPointScheme();
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name": return wfcompany.CompanyName;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfcompany.Street;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfcompany.HouseNumber;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return wfcompany.InHouseMail;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfcompany.CityName;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfcompany.PostalZone;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country);
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : (wfcompany.Country == "DK" ? "DK:SE" : "ZZZ");    //One rule exception for DK. Will not be functionalized
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return "63";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.2";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return wfcompany.CompanyName;
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfcompany.Country), wfcompany.CompanyNo);
                case "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfcompany.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfcompany.Country, "ZZZ");
                case "root:CreditNote/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID": return BillTo.AddressID.ToString();
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "N/A" : BillTo.ean;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID": return (string.IsNullOrEmpty(BillTo.ean) ? 0 : (BillTo.ean.Length)) != 13 ? "DK:VANS" : "GLN";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID": return BillTo.Account;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID": return "ZZZ";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name": return BillTo.CompanyName;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID": return Konv.convLanguage(LanguageIDType.PreferredForCountryVC, LanguageIDType.ISO_1, BillTo.Language);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredLax";                  //TODO  intention to use StructuredDK - still has buildingnumber etc
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName": return BillTo.Address1;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return BillTo.HouseNumber;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return BillTo.InHouseMail;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName": return BillTo.City;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return BillTo.PostalCode;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, BillTo.CountryID);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID": return string.IsNullOrEmpty(BillTo.VATNumber) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), BillTo.VATNumber);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(BillTo.VATNumber) ? "" : (BillTo.CountryID == "DK" ? "DK:SE" : "ZZZ");
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID": return string.IsNullOrEmpty(BillTo.VATNumber) ? "" : "63";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeAgencyID": return string.IsNullOrEmpty(BillTo.VATNumber) ? "" : "320";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID": return string.IsNullOrEmpty(BillTo.VATNumber) ? "" : "urn:oioubl:id:taxschemeid-1.1";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name": return string.IsNullOrEmpty(BillTo.VATNumber) ? "" : "Moms";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(BillTo.VATNumber) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, BillTo.CountryID), BillTo.VATNumber);
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(BillTo.VATNumber) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, BillTo.CountryID, "ZZZ");
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ID": return "1";
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail": return BillTo.email;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FirstName": return BillTo.CompanyName;
                case "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FamilyName": return BillTo.LastName;
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PartyName/cbc:Name": return wfseller.SellerName;
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode": return "StructuredDK";
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName": return wfseller.SellerStreet;
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber": return wfseller.SellerHouseNumber;
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail": return wfseller.SellerInHouseMail;
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName": return wfseller.SellerCityName;
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone": return wfseller.SellerPostalZone;
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfseller.SellerCountryID);
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : wfseller.SellerName;
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfseller.SellerCountryID), wfseller.CompanyNo);
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID": return string.IsNullOrEmpty(wfseller.CompanyNo) ? "" : Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.UBLPartyScheme, wfseller.SellerCountryID, "ZZZ");
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:Contact/cbc:ID": return "1";
                case "root:CreditNote/cac:SellerSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail": return wfseller.Email;
                case "root:CreditNote/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode": return "StructuredLax";
                case "root:CreditNote/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode/@listID": return "urn:oioubl:codelist:addressformatcode-1.1";
                case "root:CreditNote/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode/@listAgencyID": return "320";
                case "root:CreditNote/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:StreetName": return ShipTo.Address1;
                case "root:CreditNote/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:CityName": return ShipTo.City;
                case "root:CreditNote/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:PostalZone": return ShipTo.PostalCode;
                case "root:CreditNote/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:IdentificationCode": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, ShipTo.CountryID);
                case "root:CreditNote/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:Name": return Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.CountryName, ShipTo.CountryID);
                // TODO This section hardcoded to bank to bank only
                case "root:CreditNote/cac:PaymentMeans/cbc:ID": return "1";
                case "root:CreditNote/cac:PaymentMeans/cbc:PaymentMeansCode": return "42";
                case "root:CreditNote/cac:PaymentMeans/cbc:PaymentDueDate": return (wforder.PayDate == null) ? "" : XmlConvert.ToString(wforder.PayDate, "yyyy-MM-dd");
                case "root:CreditNote/cac:PaymentMeans/cbc:PaymentChannelCode": return "DK:BANK";
                case "root:CreditNote/cac:PaymentMeans/cbc:PaymentChannelCode/@listID": return "urn:oioubl:codelist:paymentchannelcode-1.1";
                case "root:CreditNote/cac:PaymentMeans/cbc:PaymentChannelCode/@listAgencyID": return "320";
                case "root:CreditNote/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID": return wfseller.AccountNo;
                case "root:CreditNote/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote": return wforder.PaymentRef;
                case "root:CreditNote/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID": return wfseller.RegistrationNo;
                case "root:CreditNote/cac:PaymentTerms/cbc:ID": return "1";
                case "root:CreditNote/cac:PaymentTerms/cbc:PaymentMeansID": return "1";
                case "root:CreditNote/cac:PaymentTerms/cbc:Amount": return CurrencyString(wforder.Total + wforder.TotalVatEx + wforder.TotalVatIn);  //samme som "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount"
                case "root:CreditNote/cac:PaymentTerms/cbc:Amount/@currencyID": return wforder.Currency;
                // hertil

                case "root:CreditNote/cac:PrepaidPayment/cbc:PaidAmount": return (wforder.AmountPaid == 0) ? "" : CurrencyString(wforder.AmountPaid);
                case "root:CreditNote/cac:PrepaidPayment/cbc:PaidAmount/@currencyID": return (wforder.AmountPaid == 0) ? "" : wforder.Currency;
                case "root:CreditNote/cac:PrepaidPayment/cbc:PaidDate": return (wforder.PaidDate == null) ? "" : XmlConvert.ToString(wforder.PaidDate, "yyyy-MM-dd");
                case "root:CreditNote/cac:TaxTotal/cbc:TaxAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:CreditNote/cac:TaxTotal/cbc:TaxAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount": return CurrencyString(wforder.TotalVatBasisEx + (wforder.TotalVatBasisIn - wforder.TotalVatIn));
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID": return VatRate(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID": return "urn:oioubl:id:taxcategoryid-1.1";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID": return "320";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent": return (VatRate(wforder.TotalVatEx + wforder.TotalVatIn) == "StandardRated") ? VatRateHead() : "0.00";  // HACK Grouping of tax-rates needs to be made in WF
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID": return "63";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID": return "320";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID": return "urn:oioubl:id:taxschemeid-1.1";
                case "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name": return "Moms";
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount": return CurrencyString(wforder.AmountLines);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount": return CurrencyString(wforder.TotalVatEx + wforder.TotalVatIn);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount": return CurrencyString(wforder.AmountLines + wforder.TotalVatEx + wforder.TotalVatIn + wforder.AmountAllowance + wforder.AmountCharge);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID": return wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount": return (wforder.AmountCharge == 0) ? "" : CurrencyString(wforder.AmountCharge);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID": return (wforder.AmountCharge == 0) ? "" : wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount": return (wforder.AmountRounding == 0) ? "" : CurrencyString(wforder.AmountRounding);
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount/@currencyID": return (wforder.AmountRounding == 0) ? "" : wforder.Currency;
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount": return CurrencyString(wforder.Total + wforder.TotalVatEx + wforder.TotalVatIn);   //samme som "root:CreditNote/cac:PaymentTerms/cbc:Amount"
                case "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID": return wforder.Currency;

                default: return "";   */
            }
 
        }

        public void PutInvoiceLineValue(int LineId, string xpath, string value)
        {
            if (tempItem.Liid == 0)
            {
                tempItem.Liid = LineId;
                tempItem.Qty = 1;
                tempItem.AllowanceCharge = false;
            }
            if (tempItem.Liid != LineId)
            {
                tempItemsList.Add(tempItem);
                tempItem = new OrderLine();
                tempItem.Liid = LineId;
                tempItem.Qty = 1;
                tempItem.AllowanceCharge = false;
            }


            switch (xpath)
            {
                case "/cbc:InvoicedQuantity": tempItem.Qty = StringCurrency(value); break;
                case "/cbc:LineExtensionAmount": tempItem.LineAmount = StringCurrency(value); break;
                case "/cbc:LineExtensionAmount/@currencyID": ValidateCurrency(value); break;
                case "/cac:TaxTotal/cbc:TaxAmount": tempItem.LineVat = StringCurrency(value); break;
                case "/cac:TaxTotal/cbc:TaxAmount/@currencyID": ValidateCurrency(value); break;
                case "/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount": tempItem.LineVatBase = StringCurrency(value); break;
                case "/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID": ValidateCurrency(value); break;
                case "/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount": tempItem.LineVat = StringCurrency(value); break;
                case "/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID": ValidateCurrency(value); break;
                case "/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent": tempItem.vat_perc = StringCurrency(value); break;
                case "/cac:Item/cbc:Description": tempItem.ItemDesc = value; break;
                //case "cac:Item/cbc:Name": return (orderitem == null) ? "" : new string(orderitem.ItemDesc.Take(40).ToArray());
                case "/cac:Item/cac:SellersItemIdentification/cbc:ID": tempItem.ItemID = value; break;
                case "/cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode": tempItem.UNSPSC = value; break;
                case "/cac:Item/cac:AdditionalItemProperty/cbc:Value": tempItem.GroupFi = value; break;   //Value field identified by Name field, but unable to connect the two. Assumes this is always the value of "GroupFI" i.e. the required GroupFi for the Itemid, if it is unknown in WF. Lauritz related.
                case "/cac:Item/cac:ItemInstance/cac:LotIdentification/cbc:LotNumberID": tempItem.Batch = value; break;
                case "/cac:Price/cbc:PriceAmount": tempItem.LinePrice = StringCurrency(value); break;
                case "/cac:Price/cbc:PriceAmount/@currencyID": ValidateCurrency(value); break;
                    
                /*                case "cbc:CreditedQuantity": return orderitem.Qty.ToString(CultureInfo.InvariantCulture);
                                case "cbc:CreditedQuantity/@unitCode": return "EA";                             // TODO : Omform wf-unit til urn:un:unece:uncefact:codelist:specification:66411:2001
                                    */
            }
        }

        public void PutAllowanceChargeValue(int allowanceChargeId, string xpath, string value)
        {
            if (tempItem.Liid == 0)
            {
                tempItem.Liid = allowanceChargeId;
                tempItem.Qty = 1;
                tempItem.AllowanceCharge = true;
                ChargeIndicator=0;
            }
            if (tempItem.Liid!=allowanceChargeId) {
                tempItemsList.Add(tempItem);
                tempItem = new OrderLine();
                tempItem.Liid=allowanceChargeId;
                tempItem.Qty = 1;
                tempItem.AllowanceCharge = true;
                ChargeIndicator=0;
            }
            switch (xpath)
            {
                case "/cbc:ChargeIndicator": ChargeIndicator = ((value == "false") ? -1 : 1); break; 
                case "/cbc:AllowanceChargeReasonCode": tempItem.ItemID = value; break;      //TODO: Check at varenummer er registreret som allowance
                case "/cbc:AllowanceChargeReason": tempItem.ItemDesc = value; break;
                case "/cbc:Amount": tempItem.LineAmount = ChargeIndicator * StringCurrency(value); tempItem.LineVatBase = ChargeIndicator * StringCurrency(value); tempItem.LinePrice = ChargeIndicator * StringCurrency(value); break;
                case "/cbc:Amount/@currencyID": ValidateCurrency(value); break;
                case "/cac:TaxCategory/cbc:Percent": tempItem.vat_perc = StringCurrency(value); break;

                //case "/cac:TaxCategory/cbc:ID": tempItem.VatIncl = VatRate(value); break;
            }
        }
        public void PutPaymentMeansValue(int PaymentMeanID, string xpath, string value)
        {
            switch (xpath)
            {
                case "/cbc:PaymentMeansCode": wfpaymentmeans.PaymentMeansCode = Convert.ToInt16(value); break ;
                case "/cbc:PaymentDueDate": wforder.PayDate = StringDate(value); break;
                case "/cbc:PaymentChannelCode": wfpaymentmeans.PaymentChannelCode = value; break ;
                case "/cbc:InstructionID": if (wfpaymentmeans.PaymentMeansCode == 48) wfpaymentmeans.MeansOfPayment = value; else wfpaymentmeans.CardType = value; break ;
                case "/cac:PayeeFinancialAccount/cbc:ID": wfpaymentmeans.BankAccount = value;break ;
                case "/cac:PayeeFinancialAccount/cbc:PaymentNote": wfpaymentmeans.PaymentNote = value;break ;
                case "/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID": wfpaymentmeans.BankRegno = value;break ;
            }
        }
    }
}