using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IUBLSync" in both code and config file together.
[ServiceKnownType(typeof(ErrCode))]   //forsøg på at gøre enumeratoren ErrCode synlig for klienten

[ServiceContract]
public interface IUBLSync
{
    [OperationContract]
    int IUBLSync(DBUser DBUser);
    [OperationContract]
    string HelloWorld();
    [OperationContract]
    string Ping();
    [OperationContract] DBUser CreateDBUser(Guid DBKey, Guid CompKey);

    //[OperationContract]
    //string SalesInvoiceCreate(ref DBUser DBUser, ref SalesInvoiceUBL invoiceubl);    //obsolete
    [OperationContract]
    UBLDoc CreateInvoice(DBUser DBUser, UBLDoc invoiceubl);
    [OperationContract]
    UBLDoc CreateCreditnoteByInvoiceId(DBUser DBUser, int invoiceId);
    [OperationContract]
    UBLDoc CreateSelfbilledInvoice(DBUser DBUser, UBLDoc invoiceubl);
    [OperationContract]
    UBLDoc CreateSelfbilledCreditnoteBySelfbilledInvoiceId(DBUser DBUser, int selfbilledInvoiceId);
    //[OperationContract]
    //string SalesInvoiceLoadUBL(ref DBUser DBUser, ref SalesInvoiceUBL invoiceubl);  //obsolete
    [OperationContract]
    UBLDoc GetInvoice(DBUser DBUser, int invoiceId);
    [OperationContract]
    UBLDoc GetInvoiceBySaleId(DBUser DBUser, int SaleId);
    [OperationContract]
    UBLDoc GetCreditNote(DBUser DBUser, int creditnoteId);
    [OperationContract]
    UBLDoc GetSelfbilledInvoice(DBUser DBUser, int invoiceID);
    [OperationContract]
    UBLDoc GetSelfbilledCreditNote(DBUser DBUser, int invoiceID);

    [OperationContract]
    string[] GetInvoicesByItem(DBUser DBUser, string itemElement);          //formål ukendt

        
     [OperationContract]
    UBLDoc UpdateInvoice(DBUser DBUser, UBLDoc invoiceubl);
    //[OperationContract]
    //string SalesInvoiceSaveUBL(ref DBUser DBUser, ref SalesInvoiceUBL invoiceubl);  //obsolete
    [OperationContract]
    UBLDoc UpdateSelfbilledInvoice(DBUser DBUser, UBLDoc invoiceubl);
    [OperationContract]
    UBLDoc UpdateCreditNote(DBUser DBUser, UBLDoc invoiceubl);
    [OperationContract]
    UBLDoc UpdateSelfbilledCreditnote(DBUser DBUser, UBLDoc invoiceubl);
    [OperationContract]
    void CloseInvoice(DBUser DBUser, int invoiceId);
    [OperationContract]
    UBLDoc GetStatement(DBUser DBUser, UBLDoc invoiceubl);   //, string customerId, DateTime fromDate, DateTime toDate);
}

// UBLSync
[DataContract]
public class UBLDoc
{
    [DataMember] public int DocID;
    [DataMember] public int InvoiceNo;
    [DataMember] public string Category;
    [DataMember] public string adAccount;
    [DataMember] public DateTime FromDate;
    [DataMember] public DateTime ToDate;
    [DataMember] public string XmlString;
}

/*
[DataContract]
public class SalesInvoiceUBL
{
    [DataMember]
    public int SaleID;
    [DataMember]
    public int InvoiceNo;
    [DataMember]
    public string Category;
    [DataMember]
    public string XmlString;
    [DataMember]
    public DocType doctype;                     //bør udgå, da der laves et håndtag pr. dokumenttype
}
*/

[DataContract]
public enum DocType
{
    [EnumMember]
    Invoice = 1,
    [EnumMember]
    CreditNote = 2,
    [EnumMember]
    SelfBilledinvoice = 3,
    [EnumMember]
    SelfBilledCreditNote = 4,
    [EnumMember]
    Statement = 5
}


//             catch (NullReferenceException ex)  {errstr = ex.Message; throw new FaultException( string.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));}

[DataContract(Name="ErrCode")]
public enum ErrCode     //TODO Bør ligge mere centralt end her ude i UBL'en.
{
    [EnumMember]
    ValidationError = 10000,
    [EnumMember]
    PermissionDenied = 10001,
    [EnumMember]
    DocumentNotFound = 10002,
    [EnumMember]
    InvalidDocumentType = 10003,
    [EnumMember]
    InvoiceNotClosed = 10004,
    [EnumMember]
    GeneralUBLError = 10005,
    [EnumMember]
    MultipleCurrencyCodesNotAllowed = 10006,
    [EnumMember]
    UnknownCurrencyCode = 10007,        //not used
    [EnumMember]
    UnknownAddress = 10008,             //not used
    [EnumMember]
    InvalidCompany = 10009,
    [EnumMember]
    InvalidAccountingCost = 10010,
    [EnumMember]
    InvalidSeller = 10011,
    [EnumMember]
    AllowanceChargeNotFound = 1012,      //not used
    [EnumMember]
    NoLines = 1013,
    [EnumMember]
    InvalidCountryCode = 1014,
    [EnumMember]
    InvalidLanguageCode = 1015,
    [EnumMember]
    InvalidDocument = 1016

}


[DataContract]
public class SalesOrderUBL                          //TODO   bør slettes - bruges kun i iService.cs, hvor den bør fjernes fra.
{
    [DataMember]
    public int SaleID;
    [DataMember]
    public int InvoiceNo;
    [DataMember]
    public string XmlString;
}


