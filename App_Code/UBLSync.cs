using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Reflection;
using System.Xml;
using wfxml;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "UBLSync" in code, svc and config file together.
public class UBLSync : IUBLSync
{
    UBLMapper.MapperFunction MapOperation = UBLMapper.MapperFunction.Undefined;

    public int IUBLSync(DBUser DBUser)
    {
        DBUser.DBKey = Guid.Empty;
        return 0;
    }

    public string HelloWorld()
    {
        return "hello  world from UBLSync";
    }

    public string Ping()
    {
        string AppName=string.Empty;
        System.Reflection.Assembly thisAssembly = this.GetType().Assembly;
        object[] attributes = thisAssembly.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);
        if (attributes.Length == 1)
        {
            AppName= (((AssemblyTitleAttribute) attributes[0]).Title);
        }

        return String.Format("{0} ({1})", AppName, Assembly.GetExecutingAssembly().GetName().Version);
    }

    public DBUser CreateDBUser(Guid DBKey, Guid CompKey)
    {
        // Fill in information needed to access database / company in winfinans database
        //Guid dbGuid = new Guid(ConfigurationManager.AppSettings["WfConnecKey"]); // database key
        //Guid companyGuid = new Guid(ConfigurationManager.AppSettings["WfCompanyKey"]); // company key
        //UBLSyncClient wfubl = new wf_ubl.UBLSyncClient();
        DBUser DBUser = new DBUser(); //CreateDBUser(dbGuid);
        DBUser.DBKey = DBKey;
        DBUser.CompanyKey = CompKey;
        return DBUser;
    }

    //public string SalesInvoiceLoadUBL(ref DBUser DBUser, ref SalesInvoiceUBL invoiceubl)
    public UBLDoc GetInvoice(DBUser DBUser, int invoiceId)
    {
        //var wfconn = new wfws.ConnectLocal(DBUser);
        //wfconn.ConnectionGetByGuid(ref DBUser);
        UBLDoc invoiceubl = new UBLDoc();
        invoiceubl.InvoiceNo = invoiceId;
        UBLXPath MyPaths = new UBLXPath(DBUser, DocType.Invoice);

            //string filepath = Assembly.GetExecutingAssembly().Location + "..\\..\\";
        UBLSerializer MyXml = new UBLSerializer(UblDocumentType.Invoice);  //@"C:\vss\wcf\resources\testubl.xml");
        UBLMapper MyMapper = new UBLMapper(ref DBUser, ref invoiceubl, UBLMapper.SubSystem.Sale, UBLMapper.MapperFunction.Load);
        if (!MyMapper.IsDocumentTypeCorrect(UblDocumentType.Invoice)) throw new FaultException(string.Concat("Document found is not an invoice. InvoiceNo: ", invoiceubl.InvoiceNo.ToString()), new FaultCode(ErrCode.InvalidDocumentType.ToString()));
        string MyXMLResult;
        MyXMLResult = MyXml.LoadInvoiceValueByXPath(MyPaths.Head1XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadAllowanceChargeValueByXPath(MyPaths.AllowXPaths(), MyMapper);
        MyXMLResult = MyXml.LoadInvoiceValueByXPath(MyPaths.Head2XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadInvoiceLineValueByXPath(MyPaths.LineXPaths(), MyMapper);
        invoiceubl.XmlString = MyXMLResult;
        return invoiceubl;
    }

    //public string SalesInvoiceLoadUBL(ref DBUser DBUser, ref SalesInvoiceUBL invoiceubl)
    public UBLDoc GetInvoiceBySaleId(DBUser DBUser, int SaleId)
    {
        //var wfconn = new wfws.ConnectLocal(DBUser);
        //wfconn.ConnectionGetByGuid(ref DBUser);
        UBLDoc invoiceubl = new UBLDoc();
        invoiceubl.DocID = SaleId;
        UBLXPath MyPaths = new UBLXPath(DBUser, DocType.Invoice);

        //string filepath = Assembly.GetExecutingAssembly().Location + "..\\..\\";
        UBLSerializer MyXml = new UBLSerializer(UblDocumentType.Invoice);  //@"C:\vss\wcf\resources\testubl.xml");
        UBLMapper MyMapper = new UBLMapper(ref DBUser, ref invoiceubl, UBLMapper.SubSystem.Sale, UBLMapper.MapperFunction.Load);
        if (!MyMapper.IsDocumentTypeCorrect(UblDocumentType.Invoice)) throw new FaultException(string.Concat("Document found is not an invoice. InvoiceNo: ", invoiceubl.InvoiceNo.ToString()), new FaultCode(ErrCode.InvalidDocumentType.ToString()));
        string MyXMLResult;
        MyXMLResult = MyXml.LoadInvoiceValueByXPath(MyPaths.Head1XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadAllowanceChargeValueByXPath(MyPaths.AllowXPaths(), MyMapper);
        MyXMLResult = MyXml.LoadInvoiceValueByXPath(MyPaths.Head2XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadInvoiceLineValueByXPath(MyPaths.LineXPaths(), MyMapper);
        invoiceubl.XmlString = MyXMLResult;
        return invoiceubl;
    }

    public UBLDoc GetCreditNote(DBUser DBUser, int creditnoteId)
    {
        //var wfconn = new wfws.ConnectLocal(DBUser);
        //wfconn.ConnectionGetByGuid(ref DBUser);
        UBLDoc creditubl = new UBLDoc();
        creditubl.InvoiceNo = creditnoteId;
        UBLXPath MyPaths = new UBLXPath(DBUser, DocType.CreditNote);
        UBLSerializer MyXml = new UBLSerializer(UblDocumentType.CreditNote);
        UBLMapper MyMapper = new UBLMapper(ref DBUser, ref creditubl, UBLMapper.SubSystem.Sale, UBLMapper.MapperFunction.Load);
        if (!MyMapper.IsDocumentTypeCorrect(UblDocumentType.CreditNote)) throw new FaultException(string.Concat("Document found is not a creditnote. InvoiceNo: ", creditubl.InvoiceNo.ToString()), new FaultCode(ErrCode.InvalidDocumentType.ToString()));
        string MyXMLResult;
        MyXMLResult = MyXml.LoadInvoiceValueByXPath(MyPaths.Head1XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadCreditNoteAllowanceChargeValueByXPath(MyPaths.AllowXPaths(), MyMapper);
        MyXMLResult = MyXml.LoadInvoiceValueByXPath(MyPaths.Head2XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadCreditNoteLineValueByXPath(MyPaths.LineXPaths(), MyMapper);
        creditubl.XmlString = MyXMLResult;
        return creditubl;
    }

    public UBLDoc GetSelfbilledInvoice(DBUser DBUser, int invoiceId)
    {
       // var wfconn = new wfws.ConnectLocal(DBUser);
       // wfconn.ConnectionGetByGuid(ref DBUser);
        UBLDoc selfbilledubl = new UBLDoc();
        selfbilledubl.InvoiceNo = invoiceId;
        UBLXPath MyPaths = new UBLXPath(DBUser, DocType.SelfBilledinvoice);
        UBLSerializer MyXml = new UBLSerializer(UblDocumentType.SelfBilledInvoice);  //@"C:\vss\wcf\resources\testubl.xml");
        UBLMapper MyMapper = new UBLMapper(ref DBUser, ref selfbilledubl, UBLMapper.SubSystem.Purchase, UBLMapper.MapperFunction.Load);
        string MyXMLResult;
        MyXMLResult = MyXml.LoadSelfBilledValueByXPath(MyPaths.Head1XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadSelfBilledAllowanceChargeValueByXPath(MyPaths.AllowXPaths(), MyMapper);
        MyXMLResult = MyXml.LoadSelfBilledValueByXPath(MyPaths.Head2XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadSelfBilledLineValueByXPath(MyPaths.LineXPaths(), MyMapper);
        selfbilledubl.XmlString = MyXMLResult;
        return selfbilledubl;
    }

    public UBLDoc GetSelfbilledCreditNote(DBUser DBUser, int creditnoteId)
    {
        var wfconn = new wfws.ConnectLocal(DBUser);
        wfconn.ConnectionGetByGuid_02(ref DBUser);
        UBLDoc creditubl = new UBLDoc();
        creditubl.InvoiceNo = creditnoteId;
        UBLXPath MyPaths = new UBLXPath(DBUser, DocType.SelfBilledCreditNote);
        UBLSerializer MyXml = new UBLSerializer(UblDocumentType.SelfBilledCreditNote);
        UBLMapper MyMapper = new UBLMapper(ref DBUser, ref creditubl, UBLMapper.SubSystem.Purchase, UBLMapper.MapperFunction.Load);
        string MyXMLResult;
        MyXMLResult = MyXml.LoadSelfBilledValueByXPath(MyPaths.Head1XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadSelfBilledAllowanceChargeValueByXPath(MyPaths.AllowXPaths(), MyMapper);
        MyXMLResult = MyXml.LoadSelfBilledValueByXPath(MyPaths.Head2XPaths(), MyMapper);
        MyXMLResult = MyXml.LoadSelfBilledLineValueByXPath(MyPaths.LineXPaths(), MyMapper);
        creditubl.XmlString = MyXMLResult;
        return creditubl;
    }

    public UBLDoc CreateInvoice(DBUser DBUser, UBLDoc invoiceubl)
    {
        UBLMapper MyMapper = new UBLMapper(ref DBUser, ref invoiceubl, UBLMapper.SubSystem.Sale, UBLMapper.MapperFunction.Save);
        UBLSerializer MyXML = new UBLSerializer(invoiceubl, UblDocumentType.Invoice);
        //UBLXPath MyPaths = new UBLXPath(DBUser, DocType.Invoice);
        //string ErrStr;

        //UBLValidator.UBLValidationClient Validate = new UBLValidator.UBLValidationClient();
        //if (!Validate.ValidateUBL(invoiceubl.XmlString, out ErrStr))
        //{
        //    throw new FaultException(string.Concat("General validation error: ", ErrStr), new FaultCode(ErrCode.ValidationError.ToString()));
        //}
        //if (!MyXML.ValidateInvoice(MyMapper, MyPaths.MyHeadValidationPaths(), out ErrStr))   //pre validation of XML obsolete - now validated in intermediate validation - see UBLmapper.Datacheck()
        //{
        //    throw new FaultException(string.Concat("The document did not pass the internal validation: ", ErrStr), new FaultCode(ErrCode.ValidationError.ToString()));
        //}
        MyXML.PutInvoice(MyMapper);
        MyMapper.DataCheck();
        MyMapper.SaveOrderToWF(ref DBUser, ref invoiceubl);
        invoiceubl = GetInvoice(DBUser, invoiceubl.InvoiceNo);
        return invoiceubl;
    }

    public UBLDoc CreateSelfbilledInvoice(DBUser DBUser, UBLDoc invoiceubl)
    { return new UBLDoc(); }

    public UBLDoc CreateCreditnoteByInvoiceId(DBUser DBUser, int invoiceId)
    {
        UBLDoc answer = new UBLDoc();
        string ErrStr="";
        var wfconn = new wfws.ConnectLocal(DBUser);
        wfconn.ConnectionGetByGuid_02(ref DBUser);

        wfws.web wfweb = new wfws.web(ref DBUser);
        int InvoiceSaleID = wfweb.get_saleid_by_ordreno(0,invoiceId,ref ErrStr);
        if (InvoiceSaleID==0) 
        {
            throw new FaultException(string.Concat("The document was not found: "), new FaultCode(ErrCode.DocumentNotFound.ToString()));
        }
        answer.DocID = wfweb.SalesOrder_create_CreditNote(InvoiceSaleID);
        answer = GetCreditNote(DBUser, wfweb.get_invoiceno_by_saleid(answer.DocID, ref ErrStr));
        return answer;
    }

    public UBLDoc CreateSelfbilledCreditnoteBySelfbilledInvoiceId(DBUser DBUser, int selfbilledInvoiceId)
    { return new UBLDoc(); }

    public UBLDoc UpdateInvoice(DBUser DBUser, UBLDoc invoiceubl) {
        string ErrStr = "";
        var wfconn = new wfws.ConnectLocal(DBUser);
        wfconn.ConnectionGetByGuid_02(ref DBUser);
        UBLMapper MyMapper = new UBLMapper(ref DBUser, ref invoiceubl, UBLMapper.SubSystem.Sale, UBLMapper.MapperFunction.Save);
        UBLSerializer MyXML = new UBLSerializer(invoiceubl, UblDocumentType.Invoice);
        MyXML.PutInvoice(MyMapper);
        wfws.web wfweb = new wfws.web(ref DBUser);
        invoiceubl.InvoiceNo = (int)MyMapper.GetInvoiceNo();
        int InvoiceSaleID = wfweb.get_saleid_by_ordreno(0, invoiceubl.InvoiceNo, ref ErrStr);
        if (InvoiceSaleID == 0)
        {
            throw new FaultException(string.Concat("InvoiceNo not found: ", ErrStr), new FaultCode(ErrCode.DocumentNotFound.ToString()));
        }




        MyMapper.DataCheck();
        int status = ClearInvoice(DBUser, invoiceubl);
        if (status != 1 & status != 2)
        {
            throw new FaultException(string.Concat("InvoiceNo not found: ", ErrStr), new FaultCode(ErrCode.DocumentNotFound.ToString()));
        }

        if (status == 1) {
            MyMapper.FillOrderInWF(ref DBUser, ref invoiceubl, InvoiceSaleID);
        }
        if (status == 2) {
            MyMapper.SaveOrderToWF(ref DBUser, ref invoiceubl);
        }

        return GetInvoice(DBUser, invoiceubl.InvoiceNo);
    }
    public UBLDoc UpdateSelfbilledInvoice(DBUser DBUser, UBLDoc invoiceubl)
    {
        UBLDoc d = new UBLDoc();
        return d;
    }
    public UBLDoc UpdateCreditNote(DBUser DBUser, UBLDoc invoiceubl)
    {
        UBLDoc d = new UBLDoc();
        return d;
    }
    public UBLDoc UpdateSelfbilledCreditnote(DBUser DBUser, UBLDoc invoiceubl)
    {
        UBLDoc d = new UBLDoc();
        return d;
    }


    private int ClearInvoice(DBUser DBUser, UBLDoc invoiceubl) {
        int response = 0;  //1=cleared, 2=deleted
        string Outcome = "";
        var wfconn = new wfws.ConnectLocal(DBUser);
        wfconn.ConnectionGetByGuid_02(ref DBUser);
        wfws.web wfweb = new wfws.web(ref DBUser);
        int InvoiceSaleID = wfweb.get_saleid_by_ordreno(0, invoiceubl.InvoiceNo, ref Outcome);
        if (InvoiceSaleID == 0)
            throw new FaultException(string.Concat("InvoiceNo not found: ", Outcome), new FaultCode(ErrCode.DocumentNotFound.ToString()));
        wfweb.order_clear(InvoiceSaleID, ref Outcome);
        if (Outcome == "Cleared") response = 1;
        if (Outcome == "Deleted") response = 2;
        if (Outcome != "Cleared" & Outcome != "Deleted")
            throw new FaultException(string.Concat("InvoiceNo ", invoiceubl.InvoiceNo.ToString(), " not updateable: ", Outcome), new FaultCode(ErrCode.PermissionDenied.ToString()));
        return response;
    }

    public void CloseInvoice(DBUser DBUser, int invoiceId)
    {
        string ErrStr = "";
        var wfconn = new wfws.ConnectLocal(DBUser);
        wfconn.ConnectionGetByGuid_02(ref DBUser);

        wfws.web wfweb = new wfws.web(ref DBUser);
        int InvoiceSaleID = wfweb.get_saleid_by_ordreno(0, invoiceId, ref ErrStr);
        if (InvoiceSaleID == 0)
        {
            throw new FaultException(string.Concat("InvoiceNo not found: ", ErrStr), new FaultCode(ErrCode.DocumentNotFound.ToString())); 
        }
        wfweb.order_Close(InvoiceSaleID);
    }

    public UBLDoc GetStatement(DBUser DBUser, UBLDoc statementubl)
    {
        var wfconn = new wfws.ConnectLocal(DBUser);
        wfconn.ConnectionGetByGuid_02(ref DBUser);
        //UBLDoc statementubl = new UBLDoc();
        //statementubl.adAccount = customerId;
        //statementubl.FromDate = fromDate;
        //statementubl.ToDate = toDate;

        UBLXPath MyPaths = new UBLXPath(DBUser, DocType.Statement);
        UBLSerializer MyXML = new UBLSerializer(UblDocumentType.Statement);
        UBLMapper MyMapper = new UBLMapper(ref DBUser, ref statementubl, UBLMapper.SubSystem.Finance, UBLMapper.MapperFunction.Load);
        string MyXMLResult;
        MyXMLResult = MyXML.LoadStatementValueByXPath(MyPaths.Head1XPaths(), MyMapper);
        MyXMLResult = MyXML.LoadStatementLineValueByXPath(MyPaths.LineXPaths(), MyMapper);
        statementubl.XmlString = MyXMLResult;
        return statementubl;
    }

    
    public string[] GetInvoicesByItem(DBUser DBUser, string itemElement)          //formål ukendt
    { return new string[2]; }


    private UBLMapper.MapperFunction ValidatePackage(UBLDoc invoiceubl, UBLMapper.MapperFunction PulledHandle = UBLMapper.MapperFunction.Undefined)
    {
        UBLMapper.MapperFunction Answer = UBLMapper.MapperFunction.Undefined;
        //TODO Validation of invoiceubl.XmlString: Load into xml structure - else set to ""

        if (invoiceubl.DocID==0)
            if (invoiceubl.InvoiceNo==0)
                if (string.IsNullOrEmpty(invoiceubl.XmlString))
                    Answer = UBLMapper.MapperFunction.Create;
                else
                    Answer = UBLMapper.MapperFunction.Save;
            else
                if (string.IsNullOrEmpty(invoiceubl.XmlString))
                    if (PulledHandle == UBLMapper.MapperFunction.Load)            //Exception where actual handle determines what way data goes
                        Answer = UBLMapper.MapperFunction.Load;
                    else
                        Answer = UBLMapper.MapperFunction.Create;
                else
                    Answer = UBLMapper.MapperFunction.Save;
        else
            if (invoiceubl.InvoiceNo==0)
                if (string.IsNullOrEmpty(invoiceubl.XmlString))
                    Answer = UBLMapper.MapperFunction.Load;
                else
                    Answer = UBLMapper.MapperFunction.Update;
            else
                if (string.IsNullOrEmpty(invoiceubl.XmlString))
                    Answer = UBLMapper.MapperFunction.Load;
                else
                    Answer = UBLMapper.MapperFunction.Update;
        MapOperation = Answer;
        return Answer;
    }
}
