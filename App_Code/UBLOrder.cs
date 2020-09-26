using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;
using wfws;

/// <summary>
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
///                                                                 DENNE KLASSE ER OVERFLØDIGGJORT AF UBLMAPPER.CS OG SERIALIZER.CS
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// </summary>
public class UBLOrder
{


    public static string oio_err;
    public XmlDocument Doc = new XmlDocument();

    public CompanyInf wfcompany;
    public CompanySeller wfseller;
    public Address BillTo;
    public Address ShipTo;
    public OrderSales wforder;


    // private string cbcURI = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    // private string cacURI = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

    //private XmlNamespaceManager nsmgr = new XmlNamespaceManager( Doc.NameTable);

    private XmlNamespaceManager nsmgr;
    private string cbcURI;
    private string cacURI;

    //private NumberFormatInfo nfi = new CultureInfo("en", false).NumberFormat;
    private NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();



    public UBLOrder()
    {
        cbcURI = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        cacURI = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        nsmgr = new XmlNamespaceManager(Doc.NameTable);
        nsmgr.AddNamespace("cbc", cbcURI);
        nsmgr.AddNamespace("cac", cacURI);
        oio_err = "OK";
        wfcompany = new CompanyInf();
        wfseller = new CompanySeller();
        BillTo = new Address();
        ShipTo = new Address();
        wforder = new OrderSales();
        nfi.NumberGroupSeparator = "";
        wforder.InvoiceCreditnote = InvCre.invoice;
    }

    //public int DocumentInit(ref UBLDoc invoiceubl)
    //{
    //    int err = 0;
    //    Guid myguid = Guid.NewGuid();
    //    string myxml = string.Empty;
    //    cbcURI = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    //    cacURI = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    //    if (!string.IsNullOrEmpty(invoiceubl.XmlString))
    //    {
    //        Doc.LoadXml(invoiceubl.XmlString);
    //        BillTo.ImportID = "xxxxxx";
    //        ShipTo.ImportID = "xxxxxx";
    //        var Elem_1 = Doc.GetElementsByTagName("cbc:SupplierAssignedAccountID")[0];
    //        if (Elem_1 != null) BillTo.ImportID = Elem_1.InnerText;
    //        ShipTo.ImportID = BillTo.ImportID;
    //    }
    //    else
    //    {
    //        myxml = String.Concat("<?xml version=", (char)34, "1.0", (char)34, " encoding=", (char)34, "UTF-8", (char)34, "?>");
    //        if (invoiceubl.doctype == DocType.Invoice)
    //        {
    //            myxml = String.Concat(myxml, "<Invoice xmlns=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2", (char)34, " xmlns:cac=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2", (char)34, " xmlns:cbc=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", (char)34, " xmlns:ccts=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CoreComponentParameters-2", (char)34, " xmlns:sdt=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:SpecializedDatatypes-2", (char)34, " xmlns:udt=", (char)34, "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2", (char)34, " xmlns:xsi=", (char)34, "http://www.w3.org/2001/XMLSchema-instance", (char)34, " xsi:schemaLocation=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 UBL-Invoice-2.0.xsd", (char)34, ">");
    //            myxml = String.Concat(myxml, "<cbc:CustomizationID>OIOUBL-2.02</cbc:CustomizationID>");
    //            myxml = String.Concat(myxml, "<cbc:ProfileID schemeAgencyID=", (char)34, "320", (char)34, " schemeID=", (char)34, "urn:oioubl:id:profileid-1.2", (char)34, ">Procurement-OrdAdvR-BilSim-1.0</cbc:ProfileID>");
    //            myxml = String.Concat(myxml, "<cbc:ID>", invoiceubl.InvoiceNo, "</cbc:ID><cbc:CopyIndicator>false</cbc:CopyIndicator><cbc:UUID>", myguid.ToString(), "</cbc:UUID>");
    //            myxml = String.Concat(myxml, "</Invoice>");
    //        }
    //        if (invoiceubl.doctype == DocType.CreditNote)
    //        {
    //            myxml = String.Concat(myxml, "<CreditNote xmlns=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2", (char)34, " xmlns:cac=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2", (char)34, " xmlns:cbc=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", (char)34, " xmlns:ccts=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CoreComponentParameters-2", (char)34, " xmlns:sdt=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:SpecializedDatatypes-2", (char)34, " xmlns:udt=", (char)34, "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2", (char)34, " xmlns:xsi=", (char)34, "http://www.w3.org/2001/XMLSchema-instance", (char)34, " xsi:schemaLocation=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2 UBL-CreditNote-2.0.xsd", (char)34, ">");
    //            myxml = String.Concat(myxml, "<cbc:UBLVersionID>2.0</cbc:UBLVersionID>");
    //            myxml = String.Concat(myxml, "<cbc:CustomizationID>OIOUBL-2.02</cbc:CustomizationID>");
    //            myxml = String.Concat(myxml, "<cbc:ProfileID schemeAgencyID=", (char)34, "320", (char)34, " schemeID=", (char)34, "urn:oioubl:id:profileid-1.2", (char)34, ">Procurement-OrdAdvR-BilSim-1.0</cbc:ProfileID>");
    //            myxml = String.Concat(myxml, "<cbc:ID>", invoiceubl.InvoiceNo, "</cbc:ID><cbc:CopyIndicator>false</cbc:CopyIndicator><cbc:UUID>", myguid.ToString(), "</cbc:UUID>");
    //            myxml = String.Concat(myxml, "</CreditNote>");
    //        }
    //        Doc.LoadXml(myxml);
    //    }

    //    return err;
    //}



    public string Create_ubl_invoice_from_order(ref CompanyInf wfcompany, ref CompanySeller wfseller, ref OrderSales wforder, ref Address BillToAddress, ref Address ShipToAddress)
    {
        string errstr = "OK";
        // Doc = load_Header_ubl(ref wforder);
        //errstr = load_order_1();
        //errstr = ubl_load_AccountingSupplierParty();
        return errstr;
    }

    //public string Load_ubl(ref SalesInvoiceUBL invoiceubl)
    //{
    //    string errstr = "OK";

    //    //errstr = load_order_1();


    //    //errstr = load_seller_01();

    //    //ubl_load_AccountingCustomerParty();
    //    //ubl_load_SellerSupplierParty();
    //    //ubl_load_Delivery();

    //    //benyttes ikke i lauritz - og gør den det generelt?      ubl_create_PaymentTerms();
    //    // Load_invoice_lines();

    //    invoiceubl.XmlString = Doc.OuterXml;



    //    //errstr = load_seller_01();
    //    //ubl_create_PaymentTerms();
    //    // errstr = ubl_read_AccountingCustomerParty();
    //    // ubl_read_invoice_lines();
    //    // ubl_read_AllowanceCharge();
    //    return errstr;
    //}




    public string Create_ubl_invoice()
    {
        string errstr = "OK";
        // Doc = load_Header_ubl(ref wforder);
        //errstr = load_order_1();
        //errstr = ubl_load_AccountingSupplierParty();
        ubl_create_PaymentTerms();
        errstr = ubl_read_AccountingCustomerParty();
        ubl_read_invoice_lines();
        // ubl_read_AllowanceCharge();
        return errstr;
    }

    public string update_ubl_invoice()
    {
        string errstr = "OK";

        //  errstr = load_order_1();
        return errstr;
    }
    // create UBL
    private XmlDocument load_Header_ubl(ref OrderSales wforder)
    {
        string myxml = string.Empty;
        if (wforder.InvoiceCreditnote == InvCre.invoice)
        {
            myxml = String.Concat("<?xml version=", (char)34, "1.0", (char)34, " encoding=", (char)34, "UTF-8", (char)34, "?>");
            myxml = String.Concat(myxml, "<Invoice xmlns=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2", (char)34, " xmlns:cac=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2", (char)34, " xmlns:cbc=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", (char)34, " xmlns:ccts=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CoreComponentParameters-2", (char)34, " xmlns:sdt=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:SpecializedDatatypes-2", (char)34, " xmlns:udt=", (char)34, "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2", (char)34, " xmlns:xsi=", (char)34, "http://www.w3.org/2001/XMLSchema-instance", (char)34, " xsi:schemaLocation=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 UBL-Invoice-2.0.xsd", (char)34, ">");
            myxml = String.Concat(myxml, "<cbc:UBLVersionID>2.0</cbc:UBLVersionID>");
            myxml = String.Concat(myxml, "<cbc:CustomizationID>OIOUBL-2.02</cbc:CustomizationID>");
            myxml = String.Concat(myxml, "<cbc:ProfileID schemeAgencyID=", (char)34, "320", (char)34, " schemeID=", (char)34, "urn:oioubl:id:profileid-1.2", (char)34, ">Procurement-OrdAdvR-BilSim-1.0</cbc:ProfileID>");
            myxml = String.Concat(myxml, "<cbc:ID>", wforder.InvoiceNo, "</cbc:ID><cbc:CopyIndicator>false</cbc:CopyIndicator><cbc:UUID>", wforder.GuidInv.ToString(), "</cbc:UUID>");
            myxml = String.Concat(myxml, "</Invoice>");
        }
        if (wforder.InvoiceCreditnote == InvCre.CreditNote)
        {
            myxml = String.Concat("<?xml version=", (char)34, "1.0", (char)34, " encoding=", (char)34, "UTF-8", (char)34, "?>");
            myxml = String.Concat(myxml, "<CreditNote xmlns=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2", (char)34, " xmlns:cac=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2", (char)34, " xmlns:cbc=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", (char)34, " xmlns:ccts=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CoreComponentParameters-2", (char)34, " xmlns:sdt=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:SpecializedDatatypes-2", (char)34, " xmlns:udt=", (char)34, "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2", (char)34, " xmlns:xsi=", (char)34, "http://www.w3.org/2001/XMLSchema-instance", (char)34, " xsi:schemaLocation=", (char)34, "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2 UBL-CreditNote-2.0.xsd", (char)34, ">");
            myxml = String.Concat(myxml, "<cbc:UBLVersionID>2.0</cbc:UBLVersionID>");
            myxml = String.Concat(myxml, "<cbc:CustomizationID>OIOUBL-2.02</cbc:CustomizationID>");
            myxml = String.Concat(myxml, "<cbc:ProfileID schemeAgencyID=", (char)34, "320", (char)34, " schemeID=", (char)34, "urn:oioubl:id:profileid-1.2", (char)34, ">Procurement-OrdAdvR-BilSim-1.0</cbc:ProfileID>");
            myxml = String.Concat(myxml, "<cbc:ID>", wforder.InvoiceNo, "</cbc:ID><cbc:CopyIndicator>false</cbc:CopyIndicator><cbc:UUID>", wforder.GuidInv.ToString(), "</cbc:UUID>");
            myxml = String.Concat(myxml, "</CreditNote>");
        }
        Doc.LoadXml(myxml);
        return Doc;
    }

    private string load_seller()
    {
        string errstr = "OK";
        XmlNode Elem_0;
        XmlNode Elem_1;
        XmlNode Elem_2;
        XmlNode Elem_3;
        XmlNode Elem_4;
        XmlNode attr_1;
        // string company = string.Empty;

        string companyNo = (string.IsNullOrEmpty(wfseller.CompanyNo) ? wfcompany.CompanyNo : wfseller.CompanyNo);

        if (string.IsNullOrEmpty(companyNo)) companyNo = "0101010";

        companyNo = companyNo.Replace(" ", "");

        string CompanyEan = wfcompany.ean;

        //Elem_0 = Doc.GetElementById("AccountingSupplierParty");

        Elem_0 = Doc.GetElementsByTagName("AccountingSupplierParty")[0];

        //Elem_0 = Doc.CreateNode(XmlNodeType.Element, "cac", "AccountingSupplierParty", cacURI);
        //Doc.DocumentElement.AppendChild(Elem_0);

        Elem_1 = Doc.CreateNode(XmlNodeType.Element, "cac", "Party", cacURI);

        // endpoint = EAN

        string endpoint = CompanyEan;
        string endpointATT = "GLN";

        if (string.IsNullOrEmpty(endpoint))
        {
            endpoint = String.Concat("DK", companyNo);
            endpointATT = "DK:CVR";
        }
        else
        {
            endpointATT = "GLN";
        }

        Elem_2 = Doc.CreateNode(XmlNodeType.Element, "cbc", "EndpointID", cbcURI);
        Elem_2.InnerText = endpoint;
        attr_1 = Doc.CreateNode(XmlNodeType.Attribute, "schemeID", "");
        attr_1.Value = endpointATT;
        Elem_2.Attributes.SetNamedItem(attr_1);

        Elem_1.AppendChild(Elem_2);

        Elem_2 = Doc.CreateNode(XmlNodeType.Element, "cac", "PartyIdentification", cacURI);

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "ID", cbcURI);
        Elem_3.InnerText = String.Concat("DK", companyNo); // SellerName
        attr_1 = Doc.CreateNode(XmlNodeType.Attribute, "schemeID", "");
        attr_1.Value = "DK:CVR";
        Elem_3.Attributes.SetNamedItem(attr_1);

        Elem_2.AppendChild(Elem_3);

        Elem_1.AppendChild(Elem_2); // end PartyIdentification

        Elem_2 = Doc.CreateNode(XmlNodeType.Element, "cac", "PartyName", cacURI);
        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "Name", cbcURI);
        Elem_3.InnerText = (string.IsNullOrEmpty(wfseller.SellerName) ? wfcompany.CompanyName : wfseller.SellerName);
        Elem_2.AppendChild(Elem_3);
        Elem_1.AppendChild(Elem_2); // end Name

        Elem_2 = Doc.CreateNode(XmlNodeType.Element, "cac", "PostalAddress", cacURI);

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "AddressFormatCode", cbcURI);
        Elem_3.InnerText = "StructuredDK";
        attr_1 = Doc.CreateNode(XmlNodeType.Attribute, "listAgencyID", "");
        attr_1.Value = "320";
        Elem_3.Attributes.SetNamedItem(attr_1);

        attr_1 = Doc.CreateNode(XmlNodeType.Attribute, "listID", "");
        attr_1.Value = "urn:oioubl:codelist:addressformatcode-1.1";
        Elem_3.Attributes.SetNamedItem(attr_1);
        Elem_2.AppendChild(Elem_3);

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "StreetName", cbcURI);
        Elem_3.InnerText = (string.IsNullOrEmpty(wfseller.SellerStreet) ? wfcompany.Street : wfseller.SellerStreet);
        Elem_2.AppendChild(Elem_3);

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "BuildingNumber", cbcURI);
        Elem_3.InnerText = (string.IsNullOrEmpty(wfseller.SellerHouseNumber) ? wfcompany.HouseNumber : wfseller.SellerHouseNumber);
        Elem_2.AppendChild(Elem_3);

        if (!string.IsNullOrEmpty(wfseller.SellerInHouseMail))
        {
            Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "InhouseMail", cbcURI);
            Elem_3.InnerText = wfseller.SellerInHouseMail;
            Elem_2.AppendChild(Elem_3);
        }

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "CityName", cbcURI);
        Elem_3.InnerText = (string.IsNullOrEmpty(wfseller.SellerCityName) ? wfcompany.CityName : wfseller.SellerCityName);
        Elem_2.AppendChild(Elem_3);

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "PostalZone", cbcURI);
        Elem_3.InnerText = (string.IsNullOrEmpty(wfseller.SellerPostalZone) ? wfcompany.PostalZone : wfseller.SellerPostalZone);
        Elem_2.AppendChild(Elem_3);
        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cac", "Country", cacURI);
        Elem_4 = Doc.CreateNode(XmlNodeType.Element, "cbc", "IdentificationCode", cbcURI);
        Elem_4.InnerText = wfseller.SellerCountryID;
        Elem_3.AppendChild(Elem_4);
        Elem_2.AppendChild(Elem_3);

        Elem_1.AppendChild(Elem_2); // end PostalAddress

        Elem_2 = Doc.CreateNode(XmlNodeType.Element, "cac", "PartyTaxScheme", cacURI);
        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "CompanyID", cbcURI);
        Elem_3.InnerText = String.Concat("DK", companyNo); // SellerName

        attr_1 = Doc.CreateNode(XmlNodeType.Attribute, "schemeID", "");
        attr_1.Value = "DK:SE";
        Elem_3.Attributes.SetNamedItem(attr_1);
        Elem_2.AppendChild(Elem_3);

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cac", "TaxScheme", cacURI);
        Elem_4 = Doc.CreateNode(XmlNodeType.Element, "cbc", "ID", cbcURI);
        attr_1 = Doc.CreateNode(XmlNodeType.Attribute, "schemeAgencyID", "");
        attr_1.Value = "320";
        Elem_4.Attributes.SetNamedItem(attr_1);
        attr_1 = Doc.CreateNode(XmlNodeType.Attribute, "schemeID", "");
        attr_1.Value = "urn:oioubl:id:taxschemeid-1.1";
        Elem_4.Attributes.SetNamedItem(attr_1);
        Elem_4.InnerText = "63";
        Elem_3.AppendChild(Elem_4);

        Elem_4 = Doc.CreateNode(XmlNodeType.Element, "cbc", "Name", cbcURI);
        Elem_4.InnerText = "Moms";
        Elem_3.AppendChild(Elem_4);
        Elem_2.AppendChild(Elem_3);
        Elem_1.AppendChild(Elem_2); // end PartyTaxScheme

        Elem_2 = Doc.CreateNode(XmlNodeType.Element, "cac", "PartyLegalEntity", cacURI);

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "RegistrationName", cbcURI);
        Elem_3.InnerText = (string.IsNullOrEmpty(wfseller.SellerName) ? wfcompany.CompanyName : wfseller.SellerName);
        Elem_2.AppendChild(Elem_3);

        Elem_3 = Doc.CreateNode(XmlNodeType.Element, "cbc", "CompanyID", cbcURI);
        Elem_3.InnerText = String.Concat("DK", companyNo); // SellerName

        attr_1 = Doc.CreateNode(XmlNodeType.Attribute, "schemeID", "");
        attr_1.Value = "DK:CVR";
        Elem_3.Attributes.SetNamedItem(attr_1);
        Elem_2.AppendChild(Elem_3);
        Elem_1.AppendChild(Elem_2);  // end PartyLegalEntity

        Elem_0.AppendChild(Elem_1); // end Party
        return errstr;
    }
    public void xmlvalidate()
    {
        string myxml = Doc.OuterXml;
        var context = new XmlParserContext(null, null, null, XmlSpace.Default);
        var sc = new XmlSchemaSet();
        var settings = new XmlReaderSettings();
        settings.ValidationType = ValidationType.Schema;
        settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

        XmlReader reader = XmlReader.Create(myxml, settings);
        while (reader.Read()) ;
    }

    // Display any warnings or errors. 
    private static void ValidationCallBack(object sender, ValidationEventArgs args)
    {
        if (args.Severity == XmlSeverityType.Warning)
            oio_err = string.Concat("\tWarning: Matching schema not found.  No validation occurred.", args.Message);
        else
            oio_err = string.Concat("\tValidation error: ", args.Message);
    }

    private string ubl_create_PaymentTerms()
    {
        string errstr = "OK";
        XmlNode Elem_0;
        XmlNode Elem_1;
        XmlAttribute attr_1;

        decimal t_invIncl = wforder.Total + wforder.TotalVatEx + wforder.TotalVatEx;

        Elem_0 = Doc.GetElementsByTagName("cac:PaymentTerms")[0];
        if (Elem_0 != null)
        {

            Elem_1 = Doc.CreateNode(XmlNodeType.Element, "cbc", "ID", cbcURI);
            Elem_1.InnerText = "1";
            Elem_0.AppendChild(Elem_1);

            Elem_1 = Doc.CreateNode(XmlNodeType.Element, "cbc", "PaymentMeansID", cbcURI);
            Elem_1.InnerText = "1";
            Elem_0.AppendChild(Elem_1);

            Elem_1 = Doc.CreateNode(XmlNodeType.Element, "cbc", "Amount", cbcURI);
            Elem_1.InnerText = t_invIncl.ToString("N", nfi);
            Elem_0.AppendChild(Elem_1);
            attr_1 = Doc.CreateAttribute("currencyID");
            attr_1.Value = "DKK";
            Elem_1.Attributes.SetNamedItem(attr_1);
        }
        return errstr;
    }

    private string ubl_read_AccountingCustomerParty()
    {
        string CompanyName = string.Empty;
        XmlNode Elem_1;
        string errstr = "OK";

        var Elem_0 = Doc.GetElementsByTagName("cac:AccountingCustomerParty")[0];
        if (Elem_0 != null)
        {
            // Doc.LoadXml(Elem_0.OuterXml);

            Elem_1 = Doc.GetElementsByTagName("cbc:SupplierAssignedAccountID")[0];
            if (Elem_1 != null) BillTo.ImportID = Elem_1.InnerText;

            Elem_1 = Doc.GetElementsByTagName("cbc:Name")[0];
            if (Elem_1 != null) CompanyName = Elem_1.InnerText;

            Elem_1 = Doc.GetElementsByTagName("cbc:FirstName")[0];
            if (Elem_1 != null) CompanyName = Elem_1.InnerText;
            BillTo.CompanyName = CompanyName;

            Elem_1 = Doc.GetElementsByTagName("cbc:FamilyName")[0];
            if (Elem_1 != null) BillTo.LastName = Elem_1.InnerText;

            Elem_1 = Doc.GetElementsByTagName("cbc:StreetName")[0];
            if (Elem_1 != null) BillTo.Address1 = Elem_1.InnerText;

            Elem_1 = Doc.GetElementsByTagName("cbc:BuildingNumber")[0];
            if (Elem_1 != null) BillTo.HouseNumber = Elem_1.InnerText;
            Elem_1 = Doc.GetElementsByTagName("cbc:IdentificationCode")[0];
            if (Elem_1 != null) BillTo.CountryID = Elem_1.InnerText;

            Elem_1 = Doc.GetElementsByTagName("cbc:PostalZone")[0];
            if (Elem_1 != null) BillTo.HouseNumber = Elem_1.InnerText;

            Elem_1 = Doc.GetElementsByTagName("cbc:CityName")[0];
            if (Elem_1 != null) BillTo.City = Elem_1.InnerText;
        }
        return errstr;
    }

    private string ubl_read_invoice_lines()
    {
        string errstr = "OK";
        string mystr;
        decimal myVal = 0;
        XmlNode Elem_2;
        OrderLine Item = new OrderLine();
        var Items = new List<OrderLine>();
        var Elem_L1 = Doc.GetElementsByTagName("cac:InvoiceLine");
        XmlDocument subDoc = new XmlDocument();
        foreach (XmlNode Elem_0 in Elem_L1)
        {

            subDoc.LoadXml(Elem_0.OuterXml);

            Elem_2 = subDoc.GetElementsByTagName("cac:SellersItemIdentification")[0];
            if (Elem_2 == null) Elem_2 = subDoc.GetElementsByTagName("cac:AdditionalItemIdentification")[0];

            if (Elem_2 != null) Item.ItemID = Elem_2.ChildNodes[0].InnerText;

            Elem_2 = subDoc.GetElementsByTagName("cac:StandardItemIdentification")[0];
            if (Elem_2 != null) Item.EAN = Elem_2.ChildNodes[0].InnerText;

            Elem_2 = subDoc.GetElementsByTagName("cbc:Description")[0];
            if (Elem_2 != null) Item.ItemDesc = Elem_2.ChildNodes[0].InnerText;

            Elem_2 = subDoc.GetElementsByTagName("cbc:InvoicedQuantity")[0];
            if (Elem_2 != null)
            {
                mystr = Elem_2.InnerText;
                mystr = mystr.Replace(".", ",");
                Decimal.TryParse(mystr, out  myVal);
            }
            Item.Qty = myVal;

            Elem_2 = subDoc.GetElementsByTagName("cbc:PriceAmount")[0];
            if (Elem_2 != null)
            {
                mystr = Elem_2.InnerText;
                mystr = mystr.Replace(".", ",");
                Decimal.TryParse(mystr, out  myVal);
            }
            Item.SalesPrice = myVal;
            Item.DiscountProc = 0;
            Item.OrderAmount = Item.Qty * Item.SalesPrice;

            // wfws.web wfweb = new wfws.web(ref DBUser);
            // errstr = wfweb.Order_add_item(WfOrder.SaleID, LineItem, ref lineid);
            // wfweb.Order_Item_Update(WfOrder.SaleID, LineItem);

            Items.Add(Item);
            Item = new OrderLine();
        }
        Elem_L1 = Doc.GetElementsByTagName("cac:AllowanceCharge");

        foreach (XmlNode Elem_0 in Elem_L1)
        {

            subDoc.LoadXml(Elem_0.OuterXml);

            Elem_2 = subDoc.GetElementsByTagName("cbc:ID")[0];
            if (Elem_2 != null) Item.ItemID = Elem_2.ChildNodes[0].InnerText;

            Elem_2 = subDoc.GetElementsByTagName("cbc:AllowanceChargeReason")[0];
            if (Elem_2 != null) Item.ItemDesc = Elem_2.ChildNodes[0].InnerText;

            Item.Qty = 1;

            Elem_2 = subDoc.GetElementsByTagName("cbc:Amount")[0];
            if (Elem_2 != null)
            {
                mystr = Elem_2.InnerText;
                mystr = mystr.Replace(".", ",");
                Decimal.TryParse(mystr, out  myVal);
            }
            Item.SalesPrice = myVal;
            Item.DiscountProc = 0;
            Item.OrderAmount = Item.Qty * Item.SalesPrice;

            // wfws.web wfweb = new wfws.web(ref DBUser);
            // errstr = wfweb.Order_add_item(WfOrder.SaleID, LineItem, ref lineid);
            // wfweb.Order_Item_Update(WfOrder.SaleID, LineItem);

            Items.Add(Item);
            Item = new OrderLine();  
        }
        wforder.OrderLines = Items.ToArray();
        return errstr;
    }
    
    private string ubl_read_AllowanceCharge_xx()
    {
        string errstr = "OK";
        string mystr;
        decimal myVal = 0;
        XmlNode Elem_2;
        OrderLine Item = new OrderLine();
        var Items = new List<OrderLine>();
        var Elem_L1 = Doc.GetElementsByTagName("cac:AllowanceCharge");
        XmlDocument subDoc = new XmlDocument();
        foreach (XmlNode Elem_0 in Elem_L1)
        {

            subDoc.LoadXml(Elem_0.OuterXml);

            Elem_2 = subDoc.GetElementsByTagName("cbc:ID")[0];
            if (Elem_2 != null) Item.ItemID = Elem_2.ChildNodes[0].InnerText;

            Elem_2 = subDoc.GetElementsByTagName("cbc:AllowanceChargeReason")[0];
            if (Elem_2 != null) Item.ItemDesc = Elem_2.ChildNodes[0].InnerText;

            Item.Qty = 1;

            Elem_2 = subDoc.GetElementsByTagName("cbc:Amount")[0];
            if (Elem_2 != null)
            {
                mystr = Elem_2.InnerText;
                mystr = mystr.Replace(".", ",");
                Decimal.TryParse(mystr, out  myVal);
            }
            Item.SalesPrice = myVal;
            Item.DiscountProc = 0;
            Item.OrderAmount = Item.Qty * Item.SalesPrice;

            // wfws.web wfweb = new wfws.web(ref DBUser);
            // errstr = wfweb.Order_add_item(WfOrder.SaleID, LineItem, ref lineid);
            // wfweb.Order_Item_Update(WfOrder.SaleID, LineItem);

            Items.Add(Item);
            Item = new OrderLine();


        }

        wforder.OrderLines = Items.ToArray();
        return errstr;
    }

}