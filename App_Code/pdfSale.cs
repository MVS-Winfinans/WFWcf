using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSupergoo.ABCpdf11;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
/// <summary>
/// Summary description for pdfSale
/// </summary>
/// 
namespace wfws
{

    public class pdfSale
    {
        int compID;
        string conn_str;

        int theID;
        //Boolean UseSuperDoc;

        // stationery information
        //private string emailTo;
        private string fontFamily;
        private string ReportHeader;
        private string ReportFooter;
        private string body;
        private Boolean ShowGrid;
        private string rectstring;
        private int i_width;
        private int p_width;
        private string colalign = "bottom";

        private string PayRefBefore;
        private string PayRefAfter;
        //private int calendar;
        private int StatID;
        private int CreInvFactor;

        private string language = "dan";
        private string ordlanguage = "dan";
        private int SellerID;
        string SummarizeInvoiceMask;
        int Proforma;

        public Boolean noVat;
        public Boolean proVat;

        private int UseSeparator = 0;

        //company
        private string CompanyNo;
        private string CompanyName;

        // address
        //   'contacts
        private string ContactPerson;
        string FactoringString = string.Empty;

        // order

        //private int sa_class;

        //private int so_addressID;
        //private int sh_addressID;

        private string text_1;
        private string text_2;

        string ad_account;
        string VATNumber;
        string Phone;
        string EAN;

        string BIC;
        string IBAN;
        string BankName;
        string BankAddress;
        string RegistrationNo;

        string adrBIC;
        string adrIBAN;
        string AdrNote;
        string AdrPhone;


        string Invoice_Text;
        string Vat_Text;
        string noVAT_Text;
        string proVAT_Text;

        private int text_2_opjtype = 0;
        private int text_1_opjtype = 0;

        Boolean UseStyles = false;

        private Doc SuperDoc = new Doc();



        public pdfSale(ref DBUser DBUser)
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            //wfconn.ConnectionGetByGuid(ref DBUser);
            conn_str = wfconn.ConnectionGetByGuid_02(ref DBUser);
            compID = DBUser.CompID;
            //  so_addressID = 0;
            // sh_addressID = 0;
            // sa_class = 0;
            // TheUser = 0;
            SellerID = 0;
            i_width = 750;
            p_width = 750;
            Proforma = 0;
            fontFamily = "Arial";
            language = "dan";
            //UseSuperDoc = false;
            SuperDoc.SetInfo(0, "License", ConfigurationManager.AppSettings["ABCpdfLicence"]);
            SuperDoc.Clear();
            SuperDoc.HtmlOptions.Engine = EngineType.MSHtml;
        }



        //public string GetPDFArray(int SaleID, SalesReports ReportID, ref CompanyInf companyinf, ref OrderSales myorder, ref string errstr)
        //{
        //    string errstr_1 = "OK";
        //    int rid = 1;
        //    string header = string.Empty;
        //    int repID = 1000;
        //    int i = 0;
        //    int pos = 0;
        //    var theDoc = new Doc();
        //    myorder.SaleID = SaleID;
        //    string theString = string.Empty;
        //    string HeadDelivery = String.Empty;
        //    int headerHeight = 48;
        //    Boolean LT = false;
        //    if (ReportID == SalesReports.Order) rid = 2;
        //    if (rid == 5)
        //    {
        //        rid = 1;
        //        Proforma = 1;
        //    }
        //    if (rid == 2) repID = 1001;
        //    if (rid == 3) repID = 1002;
        //    if (rid == 4) repID = 1003;

        //    language = myorder.Language;
        //    ordlanguage = myorder.Language;
        //    if (myorder.InvoiceCreditnote == InvCre.CreditNote) CreInvFactor = -1; else CreInvFactor = 1;
        //    if (string.IsNullOrEmpty(language)) language = companyinf.Country;
        //    errstr = "start";
        //    int d_count = 0; int u_count = 0; int p_count = 0; int theCount;
        //    ReportObject repobj = new ReportObject(conn_str, compID, language);

        //    fontFamily = "Arial";

        //    theDoc.Clear();

        //    theDoc.HtmlOptions.Engine = EngineType.MSHtml;

        //    theDoc.SetInfo(theDoc.Page, "/MediaBox:Rect", "0 0 595 842");
        //    theDoc.Rect.String = "46 25 558 600";
        //    theDoc.MediaBox.String = "A4";
        //    theDoc.Page = theDoc.AddPage();

        //    if (SaleID > 0) Count_invoice_lines_unit_vat_disc(SaleID, ref d_count, ref u_count, ref p_count);
        //    //load_seller(SaleID);
        //    SellerID = myorder.seller;
        //    if (SellerID == 0) load_default_seller();
        //    load_SellerInf(SellerID);
        //    var SellerItems = repobj.Report_object_properties_Seller_Load(SellerID, repID);
        //    repobj.StatID = StatID;
        //    repobj.SellerReportload(repID, rid, ref companyinf);
        //    get_company_inf();
        //    Get_address_information(myorder.BillTo);
        //    theDoc.Font = theDoc.AddFont(fontFamily);
        //    theDoc.FontSize = 8;
        //    theDoc.EmbedFont(fontFamily);
        //    theDoc.HtmlOptions.FontEmbed = true;
        //    theDoc.HtmlOptions.FontSubstitute = true;

        //    get_Text(rid, myorder.InvoiceCreditnote);

        //    var stationeryItems = repobj.Report_object_properties_stationery(StatID);

        //    errstr = string.Concat("Seller xxx ", SellerID, " Stationery:  ", StatID);

        //    if (ShowGrid == true) theDoc.AddGrid();

        //    theDoc.Rect.String = "40 180 580 510";

        //    //invoice lines
        //    if (SaleID > 0)
        //    {
        //        theDoc.Rect.String = SellerItems[5].RectString; // wf_rep.Report_object_properties(repID, SellerID, 9, visible, showFrame, fontSize, objType)
        //        text_1_opjtype = SellerItems[5].objtype;
        //        text_1 = myorder.text_1.Replace(Convert.ToChar(13).ToString(), "<br>");

        //        theDoc.Rect.String = SellerItems[9].RectString; // wf_rep.Report_object_properties(repID, SellerID, 9, visible, showFrame, fontSize, objType)
        //        text_2_opjtype = SellerItems[9].objtype;
        //        text_2 = myorder.text_2.Replace(Convert.ToChar(13).ToString(), "<br>");


        //        theDoc.Rect.String = SellerItems[6].RectString;    // wf_rep.Report_object_properties(repID, SellerID, 6, visible, showFrame, fontSize, objType)
        //        headerHeight = 12 + SellerItems[6].fontSize;
        //        if (!SellerItems[6].showFrame) headerHeight = SellerItems[6].fontSize + 6;

        //        theDoc.Rect.Height = theDoc.Rect.Height - headerHeight;
        //        rectstring = theDoc.Rect.String;
        //        i_width = SellerItems[6].r_width;  // wf_rep.r_width
        //        if (i_width == 0) i_width = 750;
        //        p_width = i_width;

        //        //errstr = string.Concat("SellerID ", SellerID.ToString(), "Report ", repID.ToString(), "Item ", Items[6].theString, "font ", Items[6].fontSize);

        //        if (SellerItems[6].Align01 == 0) { colalign = "top"; } else { colalign = "bottom"; }
        //        if (SellerItems[6].Align01 == 0 || SellerItems[6].Align01 == 6) { UseSeparator = 0; } else { UseSeparator = 1; }




        //        body = create_invoice_lines(d_count, SellerItems[6].fontSize, SellerItems[6].objtype, u_count, p_count, SellerItems[6].showFrame, repID, ref myorder);

        //        if (SellerItems[6].showFrame) theDoc.Rect.Inset(2, 2);

        //        // if (SellerItems[9].objtype == 1)  // invoice text
        //        // {
        //        //    body = String.Concat(body, "<table border=0 cellspacing=0 cellpadding=0  width=", p_width, "px>");
        //        //    body = String.Concat(body, "<tr><td height=4> </td></tr><tr><td align=left ><font class='invoice'>", text_2, "</td></tr></table>");
        //        // }
        //        // errstr = body;
        //        // theDoc.Rect.Inset(2, 2);

        //        // theID = theDoc.AddImageHtml(body, true, i_width + 12, false);

        //        theID = theDoc.AddImageHtml(body, true, i_width, false);
        //        while (true)
        //        {
        //            theDoc.Rect.String = rectstring;
        //            if (SellerItems[6].showFrame) theDoc.FrameRect(2, 2);
        //            //theDoc.Rect.Inset(2, 2);
        //            // if (theDoc.GetInfo(theID, "Truncated") != "1") break;
        //            if (!theDoc.Chainable(theID)) break;


        //            theDoc.Page = theDoc.AddPage();
        //            theID = theDoc.AddImageToChain(theID);
        //        }

        //        for (i = 1; i <= theDoc.PageCount; i++)
        //        {
        //            theDoc.PageNumber = i;
        //            theDoc.Flatten();
        //        }
        //        theDoc.Transform.Magnify(1.0, 1.0, 0.0, 0.0);
        //        theCount = theDoc.PageCount;

        //        // line items header
        //        theDoc.Rect.String = SellerItems[6].RectString;
        //        theDoc.Rect.Height = theDoc.Rect.Height - headerHeight;

        //        // Line header
        //        // create_invoice_Lines_header(int p_SaleID, Boolean showframe, int d_count, int fontsize, int objType, int u_count, int p_count, int RepID, ref OrderSales myorder)

        //        body = create_invoice_Lines_header(SellerItems[6].showFrame, d_count, SellerItems[6].fontSize, SellerItems[6].objtype, u_count, p_count, repID, ref myorder);
        //        theDoc.Rect.Bottom = theDoc.Rect.Top;
        //        theDoc.Rect.Height = headerHeight;
        //        rectstring = theDoc.Rect.String;

        //        for (i = 1; i <= theCount; i++)
        //        {
        //            theDoc.PageNumber = i;
        //            theDoc.Rect.String = rectstring;
        //            if (SellerItems[6].showFrame)
        //            {
        //                theDoc.FrameRect(4, 4);
        //                theDoc.Rect.Inset(2, 2);
        //            }
        //            theDoc.Color.String = "0 0 0";
        //            theDoc.AddImageHtml(body, false, i_width, false);
        //        }

        //    } // SaleID > 0


        //    //  name  stationery
        //    // repobj.StatID = StatID;
        //    theCount = theDoc.PageCount;
        //    theDoc.Rect.String = stationeryItems[1].RectString;


        //    theDoc.TextStyle.HPos = 0;
        //    theDoc.TextStyle.VPos = 0;
        //    theDoc.FontSize = stationeryItems[2].fontSize;
        //    rectstring = theDoc.Rect.String;
        //    if (stationeryItems[1].visible)
        //    {
        //        theDoc.FontSize = stationeryItems[1].fontSize;
        //        theString = repobj.get_stationery_head_1();
        //        theString = replace_vareiables(theString, ref myorder);
        //        pos = theString.IndexOf(">");
        //        for (i = 1; i <= theCount; i++)
        //        {

        //            theDoc.PageNumber = i;
        //            if (stationeryItems[1].showFrame) theDoc.FrameRect(4, 4);
        //            theDoc.Rect.Inset(2, 2);
        //            theDoc.FontSize = stationeryItems[1].fontSize;

        //            if (pos > 0)
        //            {
        //                theDoc.AddImageHtml(theString, false, stationeryItems[1].r_width, false);
        //            }
        //            else
        //            {
        //                theString = theString.Replace(Convert.ToChar(13).ToString(), "<br>");
        //                theDoc.AddTextStyled(theString);
        //            }

        //            theDoc.Rect.Inset(-2, -2);
        //            theDoc.Color.String = "0 0 0";
        //        }
        //    }


        //    //  header stationery
        //    theDoc.Rect.String = stationeryItems[2].RectString;

        //    theDoc.TextStyle.HPos = 0;
        //    theDoc.TextStyle.VPos = 0;
        //    theDoc.FontSize = stationeryItems[2].fontSize;
        //    rectstring = theDoc.Rect.String;
        //    if (stationeryItems[2].visible)
        //    {
        //        theString = repobj.get_stationery_head_2();
        //        theString = replace_vareiables(theString, ref myorder);
        //        pos = theString.IndexOf(">");
        //        for (i = 1; i <= theCount; i++)
        //        {
        //            theDoc.PageNumber = i;
        //            if (stationeryItems[2].showFrame) theDoc.FrameRect(4, 4);
        //            theDoc.Rect.Inset(2, 2);
        //            theDoc.FontSize = stationeryItems[2].fontSize;


        //            if (pos > 0)
        //            {
        //                theDoc.AddImageHtml(theString, false, stationeryItems[2].r_width, false);
        //            }
        //            else
        //            {
        //                theString = theString.Replace(Convert.ToChar(13).ToString(), "<br>");
        //                theDoc.AddTextStyled(theString);
        //            }
        //            theDoc.Rect.Inset(-2, -2);
        //            theDoc.Color.String = "0 0 0";
        //        }
        //    }


        //    //  Logo
        //    theDoc.Rect.String = stationeryItems[4].RectString;
        //    theDoc.TextStyle.HPos = 0;
        //    theDoc.TextStyle.VPos = 0;
        //    double ih = 0;
        //    double iw = 0;
        //    if (stationeryItems[4].visible)
        //    {
        //        ih = theDoc.Rect.Height;
        //        iw = theDoc.Rect.Width;
        //        theDoc.FontSize = 24;
        //        for (i = 1; i <= theCount; i++)
        //        {
        //            theDoc.PageNumber = i;
        //            if (stationeryItems[4].showFrame) theDoc.FrameRect(4, 4);
        //            byte[] img = null;
        //            if (Get_image(1, ref img) == 0)
        //            {
        //                var theImg = new XImage();
        //                theImg.SetData(img);
        //                if (ih > 0)
        //                {
        //                    theDoc.Rect.Width = theImg.Width * theDoc.Rect.Height / theImg.Height;
        //                }
        //                else
        //                {
        //                    theDoc.Rect.Height = 80;
        //                    theDoc.Rect.Width = 80;
        //                }
        //                theDoc.AddImageObject(theImg, true);
        //                theImg.Clear();
        //            }
        //        }
        //    } // logo


        //    //  Logo buttom
        //    theDoc.Rect.String = stationeryItems[6].RectString;
        //    theDoc.TextStyle.HPos = 0;
        //    theDoc.TextStyle.VPos = 0;
        //    ih = 0;
        //    iw = 0;
        //    if (stationeryItems[6].visible)
        //    {
        //        ih = theDoc.Rect.Height;
        //        iw = theDoc.Rect.Width;
        //        theDoc.FontSize = 24;
        //        for (i = 1; i <= theCount; i++)
        //        {
        //            theDoc.PageNumber = i;
        //            if (stationeryItems[6].showFrame) theDoc.FrameRect(4, 4);
        //            byte[] img = null;
        //            if (Get_image(2, ref img) == 0)
        //            {
        //                var theImg = new XImage();
        //                theImg.SetData(img);
        //                if (ih > 0)
        //                {
        //                    theDoc.Rect.Width = theImg.Width * theDoc.Rect.Height / theImg.Height;
        //                }
        //                else
        //                {
        //                    theDoc.Rect.Height = 80;
        //                    theDoc.Rect.Width = 80;
        //                }
        //                theDoc.AddImageObject(theImg, true);
        //                theImg.Clear();
        //            }
        //        }
        //    } // logo buttom


        //    // buttom
        //    theDoc.Width = 1;
        //    theDoc.Rect.String = stationeryItems[3].RectString;
        //    if (stationeryItems[3].visible)
        //    {
        //        theDoc.TextStyle.HPos = 0.5;
        //        theDoc.TextStyle.VPos = 0.5;
        //        theDoc.FontSize = stationeryItems[3].fontSize;
        //        rectstring = theDoc.Rect.String;
        //        for (i = 1; i <= theCount; i++)
        //        {
        //            theDoc.Rect.String = rectstring;
        //            if (stationeryItems[3].showFrame)
        //            {
        //                theDoc.FrameRect(2, 2);
        //                theDoc.Rect.Inset(4, 4);
        //            }
        //            theDoc.Color.String = "0 0 0";
        //            theDoc.PageNumber = i;
        //            theString = repobj.get_stationery_footer(i, theCount);
        //            theString = theString.Replace("#1", i.ToString());
        //            theString = theString.Replace("#2", theCount.ToString());



        //            Decimal valdec = myorder.TotalVatEx + myorder.TotalVatIn + myorder.Total + myorder.TotalTaxAmount;
        //            FactoringString = String.Concat("MF_Styrekode:", myorder.InvoiceNo, ";", myorder.InvoiceDate.ToString("d"), ";", ad_account, ";", valdec.ToString("N"));
        //            theString = theString.Replace("@Factoring@", FactoringString);



        //            pos = theString.IndexOf(">");
        //            if (pos > 0)
        //            {
        //                theDoc.AddImageHtml(theString, false, stationeryItems[3].r_width, false);
        //            }
        //            else
        //            {
        //                theString = theString.Replace(Convert.ToChar(13).ToString(), "<br>");
        //                theDoc.AddTextStyled(theString);
        //            }

        //        }
        //    }

        //    theDoc.PageNumber = 1;
        //    theDoc.TextStyle.VPos = 0;
        //    theDoc.TextStyle.HPos = 0;

        //    if (SaleID > 0)
        //    {

        //        // invoice address

        //        theDoc.Rect.String = SellerItems[1].RectString;
        //        if (SellerItems[1].visible)
        //        {
        //            theDoc.FontSize = SellerItems[1].fontSize;
        //            if (SellerItems[1].showFrame) theDoc.FrameRect(4, 4);
        //            header = Address_header(myorder.BillTo, 1);

        //            if (!string.IsNullOrEmpty(ContactPerson)) header = String.Concat(header, get_global_dictionary(90, ref LT), " ", ContactPerson);

        //            theDoc.AddTextStyled(header);
        //        }

        //        // ship address
        //        if (myorder.BillTo != myorder.ShipTo)
        //        {
        //            theDoc.Rect.String = SellerItems[2].RectString;
        //            if (SellerItems[2].visible)
        //            {
        //                if (SellerItems[2].showFrame) theDoc.FrameRect(4, 4);
        //                theDoc.FontSize = SellerItems[2].fontSize;
        //                header = Address_header(myorder.ShipTo, 1);
        //                HeadDelivery = String.Concat("<b>", get_global_dictionary(430, ref LT), "</b><br><br>");
        //                if (HeadDelivery.Length > 16)
        //                {
        //                    header = String.Concat(HeadDelivery, header);
        //                }


        //                theDoc.AddTextStyled(header);
        //            }
        //        }  // so_addressid


        //        // text_1

        //        theDoc.Rect.String = SellerItems[5].RectString;
        //        if (SellerItems[5].visible && text_1_opjtype < 1)
        //        {
        //            if (SellerItems[5].showFrame) theDoc.FrameRect(0, 0);
        //            theDoc.TextStyle.HPos = 0;
        //            theDoc.FontSize = SellerItems[5].fontSize;
        //            theDoc.AddTextStyled(text_1);
        //        }


        //        //  invoice informations

        //        theDoc.Rect.String = SellerItems[4].RectString;

        //        for (i = 1; i <= theCount; i++)
        //        {
        //            theDoc.PageNumber = i;
        //            if (SellerItems[4].showFrame)
        //            {
        //                theDoc.FrameRect(2, 2);
        //                theDoc.Rect.Inset(4, 4);
        //            }
        //            theDoc.FontSize = SellerItems[4].fontSize;
        //            i_width = SellerItems[4].r_width;
        //            p_width = i_width; // ' 4.4 * i_Width / 3
        //            switch (SellerItems[4].objtype)
        //            {
        //                case 1:
        //                    create_Order_inf_00(repID, ref theDoc, SellerItems[4].fontSize, ref myorder, ref companyinf);
        //                    break;
        //                default:
        //                    create_Order_inf_00(repID, ref theDoc, SellerItems[4].fontSize, ref myorder, ref companyinf);
        //                    break;
        //            }
        //        }  // for i = 1 ...

        //        if (myorder.InvoiceCreditnote >= 0)
        //        {
        //            // ' payment ecc
        //            theDoc.Rect.String = SellerItems[7].RectString;
        //            if (SellerItems[7].visible)
        //            {
        //                theDoc.TextStyle.HPos = 0;
        //                if (SellerItems[7].showFrame) theDoc.FrameRect(4, 4);

        //                theDoc.Rect.Inset(2, 2);
        //                theDoc.FontSize = SellerItems[7].fontSize;
        //                header = create_payment(ref myorder);

        //                header = header.Replace(Convert.ToChar(13).ToString(), "<br>");
        //                theDoc.AddTextStyled(header);
        //            }
        //            // ' payment ref
        //            theDoc.Rect.String = SellerItems[12].RectString;
        //            if (SellerItems[12].visible)
        //            {
        //                theDoc.TextStyle.HPos = 0;
        //                if (SellerItems[12].showFrame) theDoc.FrameRect(4, 4);
        //                theDoc.Rect.Inset(2, 2);
        //                theDoc.FontSize = SellerItems[12].fontSize;
        //                header = String.Concat(PayRefBefore, myorder.PaymentRef, PayRefAfter);
        //                header = header.Replace(Convert.ToChar(13).ToString(), "<br>");
        //                theDoc.AddTextStyled(header);
        //            }
        //        } // Invoicecreditnote


        //        //  text_2
        //        theDoc.Rect.String = SellerItems[9].RectString;

        //        if (SellerItems[9].visible && SellerItems[9].objtype < 1)
        //        {

        //            // If visible <> 0 And objType < 1 Then
        //            if (SellerItems[9].showFrame) theDoc.FrameRect(4, 4);
        //            theDoc.Rect.Inset(2, 2);
        //            theDoc.TextStyle.HPos = 0;
        //            if (!string.IsNullOrEmpty(myorder.text_2))
        //            {

        //                if (myorder.text_2.IndexOf("<tr>") < 1)
        //                {
        //                    header = myorder.text_2.Replace(Convert.ToChar(13).ToString(), "<br>");
        //                    theDoc.FontSize = SellerItems[9].fontSize;
        //                    theDoc.AddTextStyled(header);
        //                }
        //                else
        //                {
        //                    theDoc.FontSize = SellerItems[9].fontSize;
        //                    i_width = SellerItems[9].r_width;
        //                    header = html_head(SellerItems[9].fontSize);
        //                    header = String.Concat(header, "<table border=0 cellspacing=0 height=2 width=", i_width, "px><tr ><td valing=top >", myorder.text_2, "</td></tr></table>");

        //                    theID = theDoc.AddImageHtml(header, true, i_width + 12, false);
        //                }
        //            } // IsNullOrEmpty

        //        }

        //    } // if saleID


        //    //  fixed invoice text
        //    if (rid > 0)
        //    {
        //        theDoc.Rect.String = SellerItems[11].RectString;
        //        if (SellerItems[11].visible)
        //        {
        //            if (SellerItems[11].showFrame) theDoc.FrameRect(4, 4);
        //            theDoc.Rect.Inset(2, 2);
        //            theDoc.TextStyle.HPos = 0;
        //            header = Invoice_Text.Replace(Convert.ToChar(13).ToString(), "<br>");

        //            theDoc.FontSize = SellerItems[11].fontSize;
        //            theDoc.AddTextStyled(header);
        //        }
        //    } // ' if rid



        //    if (SaleID > 0)
        //    {

        //        theDoc.Rect.String = SellerItems[8].RectString;
        //        if (SellerItems[8].visible)
        //        {
        //            theDoc.PageNumber = theCount;
        //            if (SellerItems[8].showFrame) theDoc.FrameRect(4, 4);
        //            theDoc.Rect.Inset(2, 2);
        //            theDoc.TextStyle.VPos = 0;
        //            theDoc.TextStyle.HPos = 0;
        //            theDoc.FontSize = SellerItems[8].fontSize;
        //            i_width = SellerItems[8].r_width;
        //            p_width = i_width; // ' 4.4 * i_Width / 3
        //            switch (SellerItems[8].objtype)
        //            {
        //                case 0:
        //                    Create_totals_1(ref theDoc, rid, ref myorder);
        //                    break;
        //                case 1:
        //                    //Create_totals_2(theDoc, fontSize, rid)
        //                    Create_totals_1(ref theDoc, rid, ref myorder);
        //                    break;
        //                case 2:
        //                    //  Create_totals_3(theDoc, fontSize, rid)
        //                    Create_totals_1(ref theDoc, rid, ref myorder);
        //                    break;
        //                case 3:

        //                    // Create_totals_4(theDoc, fontSize, rid)
        //                    Create_totals_1(ref theDoc, rid, ref myorder);
        //                    break;
        //                case 4:
        //                    Create_totals_5(ref theDoc, rid, ref myorder);
        //                    break;
        //                case 5:
        //                    // Create_totals_6(theDoc, fontSize, rid)
        //                    Create_totals_1(ref theDoc, rid, ref myorder);
        //                    break;
        //                default:
        //                    Create_totals_1(ref theDoc, rid, ref myorder);
        //                    break;
        //            } // switch
        //        }


        //    }  // SaleID


        //    SuperDoc.Append(theDoc);
        //    // 'invoice_doc = SuperDoc.GetData()

        //    theDoc.Clear();
        //    // theDoc.Rect.String = Items[6].theString;
        //    // errstr = Items[6].theString;
        //    // theDoc.FrameRect(4, 4);
        //    // theDoc.FillRect(4, 4);
        //    // theDoc.Color.String = "0 0 0";
        //    // theDoc.FrameRect(4, 4);
        //    // theDoc.AddText(body);


        //    // return theDoc.GetData();
        //    return errstr_1;
        //}




        public byte[] get_superdoc()
        {
            return SuperDoc.GetData();
        }


        public string get_email(ref OrderSales myorder)
        {


            string contactName = myorder.ContactPerson;
            string mymail = string.Empty;
            string str1 = string.Empty;

            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT email FROM ad_Contacts_adr tb1 WHERE tb1.CompID = @CompID AND tb1.addressID = @AdrID ";
            if (string.IsNullOrEmpty(myorder.ContactPerson))
            {
                mysql = string.Concat(mysql, " AND tb1.ReceiveInvoice <> 0 ");
            }
            else
            {
                mysql = string.Concat(mysql, " AND (tb1.ReceiveInvoice <> 0 OR exists (SELECT * FROM ad_contacts tb2 WHERE tb2.CompID = tb1.CompID AND tb2.ContID = tb1.ContID AND tb2.ContactName = @cname ))");
            }

            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = myorder.BillTo;
            comm.Parameters.Add("@cName", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myorder.ContactPerson) ? DBNull.Value : (object)myorder.ContactPerson);

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                str1 = myr["email"].ToString();
                if (!string.IsNullOrEmpty(str1))
                {
                    if (str1.IndexOf('@', 0) > 1)
                    {
                        mymail = String.Concat(mymail, ";", str1);
                    }
                }
            }
            myr.Close();

            if (mymail.Length > 2) mymail = mymail.Substring(mymail.Length - 1);
            if (mymail.Length < 3)
            {
                comm.CommandText = String.Concat("SELECT email,emailInvoice FROM ad_addresses WHERE CompID = @CompID AND addressID = @AdrID ");
                myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    str1 = myr["emailInvoice"].ToString();
                    if (str1.Length < 5) str1 = myr["email"].ToString();
                }
                if (str1.Length > 5) mymail = str1;
            }
            conn.Close();
            return mymail;
        }

        private void Count_invoice_lines_unit_vat_disc(int SaleID, ref int d_count, ref int u_count, ref int p_count)
        {

            string mysql = String.Concat("SELECT isnull(count(*),0) as rcount FROM tr_sale_lineitems WHERE CompID = @CompID AND SaleID = @SaleID AND unit <> '' AND unit is not null");

            SqlConnection conn = new SqlConnection(conn_str);
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            conn.Open();
            u_count = (int)comm.ExecuteScalar();

            comm.CommandText = String.Concat("SELECT isnull(count(*),0) as rcount FROM tr_sale_lineitems WHERE CompID = @CompID AND SaleID = @SaleID AND isnull(discountProc,0) <> 0");
            d_count = (int)comm.ExecuteScalar();
            comm.CommandText = String.Concat("SELECT isnull(count(*),0) as rcount FROM tr_sale_lineitems WHERE CompID = @CompID AND SaleID = @SaleID AND VatOnProfit <> 0 ");
            p_count = (int)comm.ExecuteScalar();
            conn.Close();
        }








        private void get_first_stationery()
        {
            if (StatID == 0)
            {
                SqlConnection conn = new SqlConnection(conn_str);

                string mysql = "SELECT isnull(min(StatID),0) FROM ac_companies_Stationery where CompID = @CompID";

                var comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                conn.Open();
                StatID = (Int32)comm.ExecuteScalar();
                conn.Close();

            }
        }



        private int load_SellerInf(int SellerID)
        {
            int paymentRefType = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = String.Concat("SELECT SellerName,ReportHeader, ReportFooter, PayRefBefore, PayRefAfter, FontName, Isnull(PaymentRefType,0) as PaymentRefType, BIC, IBAN, BankName, BankAddress, RegistrationNo, isnull(StatID,0) as StatID, SummarizeInvoiceMask FROM ac_companies_sellers WHERE CompID = @CompID AND SellerID = @SellerID");
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = SellerID;
            conn.Open();

            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {

                paymentRefType = (int)myr["PaymentRefType"];
                PayRefBefore = myr["PayRefBefore"].ToString();
                PayRefAfter = myr["PayRefAfter"].ToString();
                BIC = myr["BIC"].ToString();
                IBAN = myr["IBAN"].ToString();
                SummarizeInvoiceMask = myr["SummarizeInvoiceMask"].ToString();
                BankName = myr["BankName"].ToString();
                BankAddress = myr["BankAddress"].ToString();
                RegistrationNo = myr["RegistrationNo"].ToString();
                StatID = (int)myr["StatID"];

            }
            conn.Close();

            return paymentRefType;
        }


        public void load_seller(int SaleID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT @SellerID = Isnull(SellerID,0) FROM tr_sale WHERE CompID = @CompID AND SaleID = @SaleID";

            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;

            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            SellerID = (Int32)comm.Parameters["@SellerID"].Value;


        }


        private void load_default_seller()
        {

            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT Isnull(Min(SellerID),0) FROM ac_Companies_sellers WHERE CompID = @CompID";

            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            SellerID = (int)comm.ExecuteScalar();
            conn.Close();
        }





        private void Get_address_information(int addressID)              //TODO Kaldes øjensynligt ikke nogle steder fra--- disse informationer skal bruges før addressen skrives  !!!!!
        {

            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT ad_account, VATNumber,EAN,SellerID,Phone,BIC,IBAN,Notes FROM ad_addresses WHERE CompID = @CompID And AddressID = @so_addressID ";
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@so_addressID", SqlDbType.Int).Value = addressID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                ad_account = myr["ad_account"].ToString();
                VATNumber = myr["VATNumber"].ToString();
                AdrPhone = myr["Phone"].ToString();
                if (EAN == String.Empty) EAN = myr["EAN"].ToString();
                adrBIC = myr["BIC"].ToString();
                adrIBAN = myr["IBAN"].ToString();
                AdrNote = myr["Notes"].ToString();
            }
            conn.Close();

        }

        private DataTable Get_Report_Columns(int RepID, int sellerID, int d_count, int u_count)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT ColID,Pos, ColWidth  FROM  ac_Companies_sellers_rep_Columns where CompID = @CompID AND SellerID = @SellerID AND RepID =  @RepID AND pos > 0 ";
            if (d_count == 0) mysql = String.Concat(mysql, " AND ColID <> 9 AND ColID <> 10");
            if (u_count == 0) mysql = String.Concat(mysql, " AND ColID <> 4 ");
            mysql = String.Concat(mysql, " Order by pos ");
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = SellerID;
            comm.Parameters.Add("@RepID", SqlDbType.Int).Value = RepID;
            conn.Open();
            var dt = new DataTable();
            dt.Load(comm.ExecuteReader());
            conn.Close();
            return dt;
        }


        private string create_invoice_lines(int d_count, int fontsize, int objType, int u_count, int p_count, Boolean showframe, int RepID, ref OrderSales myorder)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string body;
            ReportObject wf_rp_obj = new ReportObject(conn_str, compID, ordlanguage);
            wf_rp_obj.fontSize = fontsize;
            wf_rp_obj.fontFamily = fontFamily;
            var dt = Get_Report_Columns(RepID, myorder.seller, d_count, u_count);

            int ColCount = dt.Rows.Count;
            int ColID;
            int isValue;
            int CountLevel;
            int ColWidth = 80;
            string vatOnProfitDesc;
            int liID = 0;
            int P_Class = 100;
            decimal val = 0;
            long vali;
            decimal valm;
            decimal VatPerc;
            DateTime valDate;
            String thestring;
            string ItemID;
            string Position;

            string beforeText = get_before_Text(RepID);
            string afterText = get_after_Text(RepID);
            
            if (text_1_opjtype == 1 && text_1 != string.Empty) beforeText = string.Concat(beforeText, "<br>", text_1);
            if (text_2_opjtype == 1 && text_2 != String.Empty) afterText = string.Concat(text_2, "<br>", afterText);
            beforeText = replace_vareiables(beforeText, ref myorder);
            afterText = replace_vareiables(afterText, ref myorder);

            body = wf_rp_obj.html_head();

            if (beforeText.Length > 5)
            {
                body = string.Concat(body, "<Table border=0 cellspacing=0 cellpadding=0  width=", p_width, "px>");
                body = string.Concat(body, "<tr><td height=4> </td></tr><tr><td align=left ><font class='invoice'>", beforeText, "<br></td></tr><td height=10> </td></tr>");
                if (UseSeparator == 1) body = String.Concat(body, "<tr height=8px bgcolor ='#efefef' >", "<td valign=top height=4px ></td></tr>");
                body = string.Concat(body, "</table>");
            }


            body = string.Concat(body, "<table border=0 cellspacing=0  height=10 width=", p_width, "px>");

            string MySql = String.Concat("SELECT LiID, isnull(Class,100) as Class, isnull(Pos,0) as Pos, ItemID,Description, Unit,Style,Batch, EAN,OrderQty,QtyPackages,SalesPrice, OrderAmount,DiscountProc,Discount,PrevShipQty,ShipQty, Volume, Weight, ShipDateRequested, ");
            MySql = String.Concat(MySql, " isnull(VatOnProfit,0) as VatOnProfit, isnull(Vat_Perc,0) as Vat_Perc, OrderQty - ShipQty as QtyRemains, isnull(ShipDate,@ShipDate) as ShipDate,isnull(orderno,0) as orderno,  ");
            MySql = String.Concat(MySql, " (select ItemNo from tr_prices_sa_individual tb2 where tb2.compID = tb1.CompID AND tb2.AddressID = @BillTo AND tb2.ItemID = tb1.ItemID) as CustItemID ");

            MySql = String.Concat(MySql, " From tr_sale_lineitems tb1 Where CompID = @CompID And SaleID = @SaleID Order By pos");
            var Comm = new SqlCommand(MySql, conn);
            Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            Comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = myorder.SaleID;
            Comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = myorder.BillTo;
            Comm.Parameters.Add("@ShipDate", SqlDbType.DateTime).Value = myorder.ShipDate;
            conn.Open();
            var Result = Comm.ExecuteReader();
            if (Result.HasRows)
            {

                while (Result.Read())
                {
                    P_Class = (int)Result["Class"];
                    liID = (int)Result["LiID"];
                    ItemID = Result["ItemID"].ToString();
                    body = String.Concat(body, "<tr>");

                    foreach (DataRow dr in dt.Rows)
                    {
                        ColID = (int)dr["ColID"];
                        ColWidth = (int)dr["ColWidth"];
                        isValue = 1;
                        val = 0;
                        switch (ColID)
                        {
                            case 1:
                                long.TryParse(Result["Pos"].ToString(), out vali);
                                body = String.Concat(body, "<td valign=", colalign, " width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                break;
                            case 2:
                                body = String.Concat(body, "<td valign=", colalign, " align=left width=", ColWidth, "px ><font class='invoice'>", Result["ItemID"].ToString(), "</td>");
                                break;
                            case 3:
                                if (UseStyles)
                                {
                                    thestring = String.Concat(Result["Description"].ToString(), " ", Result["Style"].ToString());
                                }
                                else
                                {
                                    thestring = Result["Description"].ToString();
                                }
                                // if (P_Class == 300) {
                                //    thestring = String.Concat(thestring, " ", load_serialized_items(liID));
                                // }
                                thestring = thestring.Replace(Convert.ToChar(13).ToString(), "<br>");
                                body = String.Concat(body, "<td valign=", colalign, " ><font class='invoice'>", thestring, "</td>");
                                break;
                            case 4:
                                isValue = 0;
                                body = String.Concat(body, "<td valign=", colalign, " width=", ColWidth, "px ><font class='invoice'>", Result["Unit"], "</td>");
                                break;
                            case 5:
                                decimal.TryParse(Result["OrderQty"].ToString(), out val);
                                if (val != 0)
                                {
                                    if (decimal.Remainder(val, 1) == 0)
                                    {
                                        vali = (long)val;
                                        body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                    }
                                    else
                                    {
                                        body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                    }
                                }
                                else
                                {
                                    body = String.Concat(body, "<td valign=", colalign, " width=", ColWidth, "px ><font class='invoice'> </td>");
                                }
                                break;
                            case 6:
                                decimal.TryParse(Result["QtyPackages"].ToString(), out val);

                                if (val != 0)
                                {
                                    if (decimal.Remainder(val, 1) == 0)
                                    {
                                        vali = (long)val;
                                        body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                    }
                                    else
                                    {
                                        body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                    }
                                }
                                else
                                {
                                    body = String.Concat(body, "<td valign=", colalign, " width=", ColWidth, "px ><font class='invoice'> </td>");
                                }
                                break;
                            case 7:
                                decimal.TryParse(Result["SalesPrice"].ToString(), out val);
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                break;
                            case 8:
                                decimal.TryParse(Result["OrderAmount"].ToString(), out val);
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                break;
                            case 9:
                                decimal.TryParse(Result["DiscountProc"].ToString(), out val);
                                if (val != 0)
                                {
                                    if (decimal.Remainder(val, 1) == 0)
                                    {
                                        vali = (long)val;
                                        body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), " % </td>");
                                    }
                                    else
                                    {
                                        body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), " % </td>");
                                    }
                                }
                                else
                                {
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'> </td>");
                                }
                                break;
                            case 10:
                                decimal.TryParse(Result["Discount"].ToString(), out val);
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                break;
                            case 11:
                                if ((int)Result["VatOnProfit"] != 0) vatOnProfitDesc = "*"; else vatOnProfitDesc = " ";
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vatOnProfitDesc, "</td>");
                                break;
                            case 12:
                                decimal.TryParse(Result["Vat_Perc"].ToString(), out VatPerc);
                                decimal.TryParse(Result["OrderAmount"].ToString(), out val);
                                if (VatPerc > 0) valm = decimal.Round(val * VatPerc / 100, 2); else valm = 0;
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", valm.ToString("N"), "</td>");
                                break;
                            case 13:
                                decimal.TryParse(Result["Vat_Perc"].ToString(), out VatPerc);
                                decimal.TryParse(Result["OrderAmount"].ToString(), out val);
                                if (VatPerc > 0) valm = Decimal.Round(val * VatPerc / 100, 2); else valm = 0;
                                val = val + valm;
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                break;
                            case 14:
                                decimal.TryParse(Result["Vat_Perc"].ToString(), out VatPerc);
                                vali = (long)val;
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                break;
                            case 15:
                                decimal.TryParse(Result["ShipQty"].ToString(), out val);

                                if (decimal.Remainder(val, 1) == 0)
                                {
                                    vali = (long)val;
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                }
                                else
                                {
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                }
                                break;
                            case 16:
                                decimal.TryParse(Result["PrevShipQty"].ToString(), out val);
                                if (decimal.Remainder(val, 1) == 0)
                                {
                                    vali = (long)val;
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                }
                                else
                                {
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                }
                                break;
                            case 17:
                                decimal.TryParse(Result["QtyRemains"].ToString(), out val);
                                if (Decimal.Remainder(val, 1) == 0)
                                {
                                    vali = (long)val;
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                }
                                else
                                {
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                }
                                break;
                            case 18:
                                Position = Get_item_position(ItemID);
                                body = String.Concat(body, "<td valign=", colalign, " align=left width=", ColWidth, "px ><font class='invoice'>", Position, "</td>");
                                break;
                            case 19:
                                body = String.Concat(body, "<td valign=", colalign, " align=left width=", ColWidth, "px ><font class='invoice'>", Result["EAN"].ToString(), "</td>");
                                break;
                            case 20:
                                body = String.Concat(body, "<td valign=", colalign, " align=left width=", ColWidth, "px ><font class='invoice'>", Result["Style"].ToString(), "</td>");
                                break;
                            case 21:
                                body = String.Concat(body, "<td valign=", colalign, " align=left width=", ColWidth, "px ><font class='invoice'>", Result["Batch"].ToString(), "</td>");
                                break;
                            case 22:
                                valDate = (DateTime)Result["ShipDate"];
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", valDate.ToString("d"), "</td>");
                                break;
                            case 23:
                                body = String.Concat(body, "<td valign=", colalign, " align=left width=", ColWidth, "px ><font class='invoice'>", Result["CustItemID"].ToString(), "</td>");
                                break;
                            case 24:
                                body = String.Concat(body, "<td valign=", colalign, " align=left width=", ColWidth, "px ><font class='invoice'>", Result["orderno"].ToString(), "</td>");
                                break;
                            case 25:
                                if (DateTime.TryParse(Result["ShipDateRequested"].ToString(), out valDate))
                                    thestring = "-";
                                else
                                    thestring = valDate.ToString("d");
                                body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", thestring, "</td>");
                                break;
                            case 26:
                                decimal.TryParse(Result["Volume"].ToString(), out val);
                                if (decimal.Remainder(val, 1) == 0)
                                {
                                    vali = (int)val;
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                }
                                else
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                break;
                            case 27:
                                decimal.TryParse(Result["Weight"].ToString(), out val);
                                if (decimal.Remainder(val, 1) == 0)
                                {
                                    vali = (int)val;
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                }
                                else
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                break;
                            case 28:
                                decimal.TryParse(Result["Time"].ToString(), out val);
                                if (decimal.Remainder(val, 1) == 0)
                                {
                                    vali = (int)val;
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", vali.ToString(), "</td>");
                                }
                                else
                                    body = String.Concat(body, "<td valign=", colalign, " align=right width=", ColWidth, "px ><font class='invoice'>", val.ToString("N"), "</td>");
                                break;
                            case 29:
                                //model check is not implemented here. body = String.Concat(body, "<td valign=", colalign, " align=left width=", ColWidth, "px ><font class='invoice'>", Result["model"].ToString(), "</td>");
                                break;
                        } // switch
                    }
                    body = String.Concat(body, "</tr>");
                    body = String.Concat(body, "</font>");
                    if (UseSeparator == 1) body = String.Concat(body, "<tr height=8px bgcolor ='#efefef' >", "<td valign=top height=4px colspan=", ColCount, "></td></tr>");

                } // while
            }
            else
            {
                body = String.Concat(body, "<tr height=8px>", "<td valign=top height=4px colspan=", ColCount, ">- - -</td></tr>");
            } // if count

            conn.Close();
            if (!showframe && UseSeparator == 0) body = String.Concat(body, "<tr height=8px bgcolor ='#efefef' >", "<td valign=top height=4px colspan=", ColCount, "></td></tr>");
            body = String.Concat(body, "</table>");


            if (afterText != String.Empty)
            {
                body = String.Concat(body, "<table border=0 cellspacing=0 cellpadding=0  width=", p_width, "px>");
                body = String.Concat(body, "<tr><td height=4> </td></tr><tr><td align=left ><font class='invoice'>", afterText, "<br></td></tr><td height=10> </td></tr></table>");
            }

            return body;
        }





        public string get_before_Text(int rid)
        {
            if (ordlanguage == String.Empty) ordlanguage = "dan";
            ReportObject wf_rp_obj = new ReportObject(conn_str, compID, ordlanguage);
            string mytext = wf_rp_obj.ReportTextGet("tr_Sa_before_Invoice", SellerID, ordlanguage);
            if (rid == 1000)
            {
                if (CreInvFactor == -1)
                {
                    mytext = String.Concat(mytext, wf_rp_obj.ReportTextGet("tr_Sa_before_onlyInv", SellerID, ordlanguage));
                }
                if (CreInvFactor == 1)
                {
                    mytext = String.Concat(mytext, wf_rp_obj.ReportTextGet("tr_Sa_before_onlyCrn", SellerID, ordlanguage));
                }
            }
            if (rid == 1001) mytext = wf_rp_obj.ReportTextGet("tr_Sa_before_Order", SellerID, ordlanguage);
            if (rid == 1002) mytext = wf_rp_obj.ReportTextGet("tr_Sa_before_onlyInv", SellerID, ordlanguage);
            if (rid == 1003) mytext = wf_rp_obj.ReportTextGet("tr_Sa_before_Quotation", SellerID, ordlanguage);
            return mytext;
        }

        public string get_after_Text(int rid)
        {
            if (ordlanguage == String.Empty) ordlanguage = "dan";
            ReportObject wf_rp_obj = new ReportObject(conn_str, compID, ordlanguage);
            string mytext = wf_rp_obj.ReportTextGet("tr_Sa_after_Invoice", SellerID, ordlanguage);
            if (rid == 1000)
            {
                if (CreInvFactor == -1)
                {
                    mytext = String.Concat(mytext, wf_rp_obj.ReportTextGet("tr_Sa_after_onlyInv", SellerID, ordlanguage));
                }
                if (CreInvFactor == 1)
                {
                    mytext = String.Concat(mytext, wf_rp_obj.ReportTextGet("tr_Sa_after_onlyCrn", SellerID, ordlanguage));
                }
            }
            if (rid == 1001) mytext = wf_rp_obj.ReportTextGet("tr_Sa_after_Order", SellerID, ordlanguage);
            if (rid == 1002) mytext = wf_rp_obj.ReportTextGet("tr_Sa_after_onlyInv", SellerID, ordlanguage);
            if (rid == 1003) mytext = wf_rp_obj.ReportTextGet("tr_Sa_after_Quotation", SellerID, ordlanguage);
            return mytext;
        }


        private string replace_vareiables(string mystring, ref OrderSales myorder)
        {
            string tostring = mystring;
            int pos = 0;
            decimal Qty = 0;
            string myVatText = string.Empty;
            string myProVat = string.Empty;
            if (noVat) myVatText = noVAT_Text; else myVatText = Vat_Text;
            if (!proVat) myProVat = proVAT_Text;
            tostring = tostring.Replace("@qtno@", myorder.QuotationNo.ToString());
            tostring = tostring.Replace("{qtno}", myorder.QuotationNo.ToString());
            tostring = tostring.Replace("@invno@", myorder.InvoiceNo.ToString());
            tostring = tostring.Replace("{invno}", myorder.InvoiceNo.ToString());
            tostring = tostring.Replace("@ordno@", myorder.OrderNo.ToString());
            tostring = tostring.Replace("{ordno}", myorder.OrderNo.ToString());
            tostring = tostring.Replace("@invdate@", myorder.InvoiceDate.ToString("d"));
            tostring = tostring.Replace("{invdate}", myorder.InvoiceDate.ToString("d"));
            //tostring = tostring.Replace("@duedate@", myorder..DueDate.ToString("d"));
            // tostring = tostring.Replace("{duedate}", DueDate.ToString("d"));
            //  tostring = tostring.Replace("@payref@", String.Concat(PayRefBefore, paymentRef, PayRefAfter))
            //  tostring = tostring.Replace("{payref}", String.Concat(PayRefBefore, paymentRef, PayRefAfter))
            tostring = tostring.Replace("@today@", DateTime.Today.ToString("d"));
            tostring = tostring.Replace("{today}", DateTime.Today.ToString("d"));
            tostring = tostring.Replace("@Acc@", ad_account);
            tostring = tostring.Replace("{Acc}", ad_account);
            tostring = tostring.Replace("{trace}", myorder.Trace);
            tostring = tostring.Replace("{vattext}", myVatText);
            tostring = tostring.Replace("{vatprotext}", myProVat);
            pos = tostring.IndexOf("{qty}");
            if (pos > 0)
            {
                Qty = Calculate_OrderQty(myorder.SaleID);
                tostring = tostring.Replace("{qty}", Qty.ToString("N"));
            }
            tostring = tostring.Replace("{dim1}", myorder.Dim1);
            tostring = tostring.Replace("{dim2}", myorder.Dim2);
            tostring = tostring.Replace("{dim3}", myorder.Dim3);
            tostring = tostring.Replace("{dim4}", myorder.Dim4);


            // if (txt_shipVia == String.Empty) tostring = tostring.Replace("@ShipVia@", "        ") Else tostring = tostring.Replace("@ShipVia@", txt_shipVia);
            // if (txt_shipVia == String.Empty) tostring = tostring.Replace("{ShipVia}", "        ") Else tostring = tostring.Replace("{ShipVia}", txt_shipVia)

            // if txt_TermsOfDelivery = String.Empty Then tostring = tostring.Replace("@Delivery@", "        ") Else tostring = tostring.Replace("@Delivery@", txt_TermsOfDelivery)
            // if txt_TermsOfDelivery = String.Empty Then tostring = tostring.Replace("{Delivery}", "        ") Else tostring = tostring.Replace("{Delivery}", txt_TermsOfDelivery)
            // if txt_TermsOfPayment = String.Empty Then tostring = tostring.Replace("@Payment@", "        ") Else tostring = tostring.Replace("@Payment@", txt_TermsOfPayment)
            // if txt_TermsOfPayment = String.Empty Then tostring = tostring.Replace("{Payment}", "        ") Else tostring = tostring.Replace("{Payment}", txt_TermsOfPayment)
            // if AdrNote = String.Empty Then tostring = tostring.Replace("{note}", "        ") Else tostring = tostring.Replace("{note}", AdrNote)

            string stringpart;
            string FieldDesc = String.Empty;
            string extstr = String.Empty;
            int posstart;
            int posend;
            int pcount = 0;
            while (tostring.Contains("{exf:") && pcount < 20)
            {
                pcount = pcount + 1;
                posstart = tostring.IndexOf("{");
                posend = tostring.IndexOf("}");
                if (posend - posstart > 6)
                {
                    stringpart = tostring.Substring(posstart, 1 + (posend - posstart));
                    FieldDesc = stringpart.Replace("{exf:", "");
                    FieldDesc = FieldDesc.Replace("}", "");


                    extstr = get_extra_field(myorder.BillTo, FieldDesc);

                    tostring = tostring.Replace(stringpart, extstr);

                }

            }


            return tostring;
        }

        private decimal Calculate_OrderQty(int SaleID)
        {
            decimal Qty;
            string mysql = " Select isnull(sum(OrderQty),0) from tr_sale_Lineitems tb1 ";
            mysql = String.Concat(mysql, " inner Join tr_inventory_groupsFi tb2 on tb2.CompID = tb1.CompID And tb2.GroupFi = tb1.GroupFi ");
            mysql = String.Concat(mysql, " where tb1.CompID = @CompID And tb1.saleID = @SaleID And isnull(tb2.NotPicked, 0) = 0 ");
            SqlConnection conn = new SqlConnection(conn_str);
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            conn.Open();
            Qty = (decimal)comm.ExecuteScalar();
            conn.Close();
            return Qty;
        }

        public string get_extra_field(int AdrID, string FieldID)
        {
            string exValue = String.Empty;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "select isnull((select Value from ad_addresses_ExtraLines where CompID = @CompID AND AddressID = @AddressID AND description = @Desc),' ') ";
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
            comm.Parameters.Add("@Desc", SqlDbType.NVarChar, 50).Value = FieldID;
            conn.Open();
            exValue = comm.ExecuteScalar().ToString();
            conn.Close();
            return exValue;
        }


        private string create_invoice_Lines_header(Boolean showframe, int d_count, int fontsize, int objType, int u_count, int p_count, int RepID, ref OrderSales myorder)
        {
            string Header = string.Empty;
            //string param[20];
            // Dim utils As New winfinance.utils
            int mycount = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string body;
            ReportObject wf_rp_obj = new ReportObject(conn_str, compID, ordlanguage);
            wf_rp_obj.fontSize = fontsize;
            wf_rp_obj.fontFamily = fontFamily;
            var dt = Get_ReportColumns(RepID);

            DataRow dr;
            int colwidth = 70;
            int ColID = 0;
            int rowID = 0;
            string HeaderText;

            string TranslateTo;
            string mysql = "SELECT ColID, Pos, ColWidth, ";

            mysql = String.Concat(mysql, " (select max(translateto) from  ac_Companies_sellers_rep_Columns_dictionary tb2 where tb2.COmpID = tb1.CompID AND tb2.SellerID = tb1.SellerID AND tb2.RepID = tb1.RepID AND tb2.ColID = tb1.ColID AND tb2.Language = @Language) as TranslateTo ");
            mysql = String.Concat(mysql, " FROM ac_Companies_sellers_rep_Columns tb1 where tb1.CompID = @CompID AND tb1.SellerID = @SellerID AND tb1.RepID =  @RepID AND tb1.pos > 0   ");

            if (d_count == 0) mysql = String.Concat(mysql, " AND ColID <> 9 AND ColID <> 10 ");
            if (u_count == 0) mysql = String.Concat(mysql, " AND ColID <> 4 ");
            mysql = String.Concat(mysql, " Order by pos ");
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = SellerID;
            comm.Parameters.Add("@RepID", SqlDbType.Int).Value = RepID;
            comm.Parameters.Add("@Language", SqlDbType.NVarChar).Value = ordlanguage;

            conn.Open();
            Header = wf_rp_obj.html_head();
            Header = String.Concat(Header, "<table border=0 cellspacing=0 height=10 width=", p_width, "px>");
            if (!showframe) Header = String.Concat(Header, "<tr bgcolor ='#efefef'>"); else Header = String.Concat(Header, "<tr>");
            var myr = comm.ExecuteReader();

            while (myr.Read())
            {
                mycount = mycount + 1;
                colwidth = (int)myr["ColWidth"];
                ColID = (int)myr["ColID"];
                rowID = ColID - 1;
                dr = dt.Rows[rowID];
                HeaderText = dr["ColDesc"].ToString();

                TranslateTo = myr["TranslateTo"].ToString();
                if (TranslateTo != String.Empty) HeaderText = TranslateTo;


                switch (ColID)
                {
                    case 1:
                        Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>");
                        break;
                    case 2:
                        Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>");
                        break;
                    case 3:
                        Header = String.Concat(Header, " <td align=left ><font class='invoice'>");
                        if (TranslateTo == String.Empty) HeaderText = String.Empty;
                        break;
                    case 4:
                        HeaderText = String.Empty;
                        Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>");
                        break;
                    case 8:
                        HeaderText = String.Concat(HeaderText, " ", myorder.Currency);
                        Header = String.Concat(Header, " <td align=right width=", colwidth, "px ><font class='invoice'>");
                        break;
                    case 18:
                        HeaderText = String.Empty;
                        Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>");
                        break;
                    case 19:
                        Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>");
                        break;
                    case 20:
                        Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>");
                        break;
                    case 21:
                        Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>");
                        break;
                    case 23:
                        Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>");
                        break;
                    default:
                        Header = String.Concat(Header, " <td align=right width=", colwidth, "px ><font class='invoice'>");
                        break;
                }

                Header = String.Concat(Header, HeaderText, "</td>");

            }

            if (mycount == 0) Header = String.Concat(Header, " <td align=right width=", colwidth, "px ><font class='invoice'>---</td>");
            Header = String.Concat(Header, "<tr>");

            conn.Close();
            return Header;
        }


        private DataTable Get_ReportColumns(int RepID)
        {

            SqlConnection mc = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_user"].ConnectionString);
            //  Dim mc As SqlClient.SqlConnection = New SqlClient.SqlConnection(ConfigurationManager.AppSettings("ConnectionString_user"))
            var mycom = new SqlCommand("wf_apl_report_seller_Columns_get", mc);
            mycom.CommandType = CommandType.StoredProcedure;
            mycom.Parameters.Add("@RepID", SqlDbType.Int).Value = 1;
            mycom.Parameters.Add("@OnRepID", SqlDbType.NVarChar, 20).Value = "%1%";
            mycom.Parameters.Add("@language", SqlDbType.NVarChar, 20).Value = ordlanguage;
            var dt = new DataTable();
            mc.Open();
            dt.Load(mycom.ExecuteReader());
            mc.Close();
            return dt;
        }









        public string html_head(int fontsize)
        {
            if (fontsize < 6) fontsize = 6;
            string myhead = "<head id='Head1'><title>Untitled Page</title><style type='text/css'>";
            myhead = String.Concat(myhead, " font.invoice {font: ", fontsize, "px '", fontFamily, "';text-decoration: none; color: #000000}");
            myhead = String.Concat(myhead, "</style><meta  content='text/html; charset=utf-8' /></head><body padding=0 border=0> ");
            return myhead;
        }

        private string Get_item_position(string p_ItemID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string result = String.Empty;
            if (!string.IsNullOrEmpty(p_ItemID))
            {
                string mysql = "SELECT max(position) as position FROM tr_inventory WHERE CompID = @CompID AND ItemID = @ItemID ";
                var comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = p_ItemID;
                conn.Open();
                result = comm.ExecuteScalar().ToString();
                conn.Close();
            }
            return result;
        }


        private string get_global_dictionary(int TextID, ref Boolean LocalTrans)
        {
            string mystr = string.Empty;
            int OK = 0;
            LocalTrans = false;
            string result = String.Empty;

            SqlConnection conn = new SqlConnection(conn_str);

            string mysql = " SELECT TranslateTo FROM ac_companies_sellers_rep_dict Where CompID = @CompID AND SellerID = @SellerID AND Country = @Country AND TextID = @TextID ";

            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = SellerID;
            comm.Parameters.Add("@Country", SqlDbType.NVarChar, 20).Value = language;
            comm.Parameters.Add("@TextID", SqlDbType.Int).Value = TextID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                mystr = myr["TranslateTo"].ToString();
                OK = 1;
                LocalTrans = true;
            }
            conn.Close();
            if (OK == 0)
            {
                mystr = Get_text_by_id(TextID, language);
            }
            return mystr;
        }


        private string Get_text_by_id(int TextID, string CountryID)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_translate"].ConnectionString);
            if (CountryID == string.Empty) CountryID = "DK";
            string theString = "Not present";
            var comm = new SqlCommand("wf_apl_translate_GetTextById", conn);

            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@TextID", SqlDbType.Int).Value = TextID;
            comm.Parameters.Add("@Language", SqlDbType.NVarChar, 4).Value = (string.IsNullOrEmpty(CountryID) ? "UK" : (object)CountryID);
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                theString = myr["TranslateTo"].ToString();
            }
            conn.Close();
            return theString;
        }




        int Get_image(int pictid, ref byte[] img)
        {
            string mysql = string.Empty;
            int Ok = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            if (pictid == 2)
            {
                mysql = "SELECT isnull(Image_2, CONVERT(VARBINARY(MAX), 0)) FROM ac_Companies_Stationery WHERE CompID = @CompID AND StatID = @StatID ";
            }
            else
            {
                mysql = "SELECT isnull(Image_1, CONVERT(VARBINARY(MAX), 0)) FROM ac_Companies_Stationery WHERE CompID = @CompID AND StatID = @StatID ";
            }

            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@StatID", SqlDbType.Int).Value = StatID;
            conn.Open();
            img = (byte[])comm.ExecuteScalar();
            conn.Close();
            if (img.Length < 50) Ok = 1;

            return Ok;
        }


        private string Address_header(int AddressID, int format)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string mystr = string.Empty;
            string Country;
            string zip;
            string city;
            string adrString;
            string endstr = string.Empty;
            string mysql = "SELECT * FROM ad_addresses WHERE CompID = @CompID and AddressID = @AdrID";
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AddressID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {


                ad_account = myr["ad_account"].ToString();
                mystr = myr["CompanyName"].ToString();
                if (!string.IsNullOrEmpty(myr["LastName"].ToString())) mystr = String.Concat(mystr, myr["LastName"].ToString());
                if (!string.IsNullOrEmpty(myr["Department"].ToString()))
                {
                    adrString = myr["Department"].ToString();
                    if (!string.IsNullOrEmpty(adrString)) mystr = String.Concat(mystr, "<br>", myr["Department"].ToString());
                }
                else
                {
                    endstr = String.Concat(endstr, "<br>");
                }

                if (!string.IsNullOrEmpty(myr["Address"].ToString())) mystr = String.Concat(mystr, "<br>", myr["Address"].ToString(), " ", myr["HouseNumber"].ToString(), " ", myr["InHouseMail"].ToString()); else endstr = String.Concat(endstr, "<br>");
                if (!string.IsNullOrEmpty(myr["Address2"].ToString())) mystr = String.Concat(mystr, "<br>", myr["Address2"].ToString()); else endstr = String.Concat(endstr, "<br>");
                Country = myr["CountryID"].ToString();
                zip = myr["PostalCode"].ToString();
                city = myr["city"].ToString();
                mystr = String.Concat(mystr, "<br>", Country, "  ", zip, "  ", city);
                VATNumber = myr["VatNumber"].ToString();    //referenced in invoicehedader info
                EAN = myr["EAN"].ToString();                //referenced in invoicehedader info
            }
            conn.Close();
            if (string.IsNullOrEmpty(endstr)) endstr = "<br>";
            return String.Concat(mystr, endstr);
        }




        private string create_Order_inf_00(int RepID, ref Doc theDoc, int FontSize, ref OrderSales myorder, ref CompanyInf mycompany)
        {

            var dt = Get_ReportInf(RepID);
            DataRow dr;
            string[] Header = new string[20];
            string theString = string.Empty;
            string orderstr;
            string salesmanName = myorder.salesman;

            DateTime quotationDate = DateTime.Today;
            string shipVia = string.Empty;
            string txt_shipVia = string.Empty;


            Boolean LT = true;
            // Dim param(20) As String
            // Dim utils As New winfinance.utils
            ReportObject wf_rp_obj = new ReportObject(conn_str, compID, ordlanguage);
            wf_rp_obj.fontSize = FontSize;
            wf_rp_obj.fontFamily = fontFamily;


            int ColID = 0;
            int rowID = 0;
            string HeaderText;
            string TranslateTo;

            if (RepID == 1000)
            {
                if (Proforma == 0)
                {
                    Header[1] = get_global_dictionary(142, ref LT); // 'winfinance.wf_sys.Get_text_by_id(142, language) ' invoice;
                }
                else
                {
                    Header[1] = get_global_dictionary(1581, ref LT);  // 'winfinance.wf_sys.Get_text_by_id(142, language) ' invoice;
                    if (myorder.InvoiceNo == 0) myorder.InvoiceNo = myorder.OrderNo;
                }
                Header[2] = get_global_dictionary(410, ref LT); // ' credit note
                theDoc.TextStyle.HPos = 0;
                if (CreInvFactor == -1) theString = Header[2]; else theString = Header[1];

                theString = String.Concat("<b>", theString, "</b>");
                theDoc.AddTextStyled(theString);
                theDoc.TextStyle.HPos = 1;
                theString = String.Concat("<b>", myorder.InvoiceNo.ToString(), "</b><br><br>");
                theDoc.AddTextStyled(theString);
            }


            if (RepID == 1001)
            {
                Header[1] = get_global_dictionary(143, ref LT); // ' Order
                Header[2] = get_global_dictionary(143, ref LT); // ' Order
                orderstr = myorder.OrderNo.ToString();
                if (myorder.DeliveryNoteNo > 0) orderstr = String.Concat(orderstr, "-", myorder.DeliveryNoteNo.ToString());
                theString = String.Concat("<b>", orderstr, "</b><br><br>");


                theDoc.TextStyle.HPos = 0;
                if (CreInvFactor == -1) theString = Header[2]; else theString = Header[1];
                theString = String.Concat("<b>", theString, "</b>");
                theDoc.AddTextStyled(theString);
                theDoc.TextStyle.HPos = 1;
                orderstr = myorder.OrderNo.ToString();
                if (myorder.DeliveryNoteNo > 0) orderstr = String.Concat(orderstr, "-", myorder.DeliveryNoteNo.ToString());
                theString = String.Concat("<b>", orderstr, "</b><br><br>");
                theDoc.AddTextStyled(theString);
            }

            if (RepID == 1002)
            {
                Header[1] = get_global_dictionary(1205, ref LT); // 'delivery note
                theDoc.TextStyle.HPos = 0;
                theString = String.Concat("<b>", Header[1], "</b>");
                theDoc.AddTextStyled(theString);
                theDoc.TextStyle.HPos = 1;
                orderstr = myorder.OrderNo.ToString();
                if (myorder.DeliveryNoteNo > 0) orderstr = String.Concat(orderstr, "-", myorder.DeliveryNoteNo.ToString());
                theString = String.Concat("<b>", orderstr, "</b><br><br>");
                theDoc.AddTextStyled(theString);
            }
            if (RepID == 1003)
            {
                Header[1] = get_global_dictionary(131, ref LT); // 'quotation number
                theDoc.TextStyle.HPos = 0;
                theString = String.Concat("<b>", Header[1], "</b>");
                theDoc.AddTextStyled(theString);
                theDoc.TextStyle.HPos = 1;
                theString = String.Concat("<b>", myorder.QuotationNo.ToString(), "</b><br><br>");
                theDoc.AddTextStyled(theString);
            }

            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT ColID, Pos, ";
            mysql = String.Concat(mysql, " (select max(translateto) from  ac_Companies_sellers_rep_inf_dictionary tb2 where tb2.COmpID = tb1.CompID AND tb2.SellerID = tb1.SellerID AND tb2.RepID = tb1.RepID AND tb2.ColID = tb1.ColID AND tb2.Language = @Language) as TranslateTo ");
            mysql = String.Concat(mysql, " FROM ac_Companies_sellers_rep_inf tb1 where tb1.CompID = @CompID AND tb1.SellerID = @SellerID AND tb1.RepID =  @RepID AND tb1.pos > 0   ");

            mysql = String.Concat(mysql, " Order by pos ");
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = myorder.seller;
            comm.Parameters.Add("@RepID", SqlDbType.Int).Value = RepID;
            comm.Parameters.Add("@Language", SqlDbType.NVarChar).Value = ordlanguage;

            conn.Open();
            orderstr = myorder.OrderNo.ToString();
            var myr = comm.ExecuteReader();
            while (myr.Read())
            {
                ColID = (int)myr["ColID"];
                rowID = ColID - 1;
                dr = dt.Rows[rowID];
                HeaderText = dr["ColDesc"].ToString();
                TranslateTo = myr["TranslateTo"].ToString();
                if (TranslateTo != String.Empty) HeaderText = TranslateTo;
                theDoc.TextStyle.HPos = 0;
                switch (ColID)
                {
                    case 1:
                        if (myorder.OrderNo > 0)
                        {
                            if (myorder.DeliveryNoteNo > 0) orderstr = String.Concat(orderstr, "-", myorder.DeliveryNoteNo.ToString());

                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(orderstr, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 2:
                        if (myorder.QuotationNo > 0)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(myorder.QuotationNo.ToString(), "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 3:
                        theDoc.AddTextStyled(HeaderText);
                        theDoc.TextStyle.HPos = 1;
                        theString = String.Concat(myorder.InvoiceDate.ToString("d"), "<br>");
                        theDoc.AddTextStyled(theString);
                        break;
                    case 4:
                        theDoc.AddTextStyled(HeaderText);
                        theDoc.TextStyle.HPos = 1;
                        theString = String.Concat(myorder.OrderDate.ToString("d"), "<br>");
                        theDoc.AddTextStyled(theString);
                        break;
                    case 5:
                        theDoc.AddTextStyled(HeaderText);
                        theDoc.TextStyle.HPos = 1;
                        theString = String.Concat(quotationDate.ToString("d"), "<br>");
                        theDoc.AddTextStyled(theString);
                        break;
                    case 6:
                        theDoc.AddTextStyled(HeaderText);
                        theDoc.TextStyle.HPos = 1;
                        theString = String.Concat(ad_account, "<br>");
                        theDoc.AddTextStyled(theString);
                        break;
                    case 7:
                        if (AdrPhone != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(Phone, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 8:
                        if (VATNumber != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(VATNumber, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 9:
                        if (EAN != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(EAN, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 10:
                        if (myorder.salesman != String.Empty)
                        {
                            salesmanName = Get_Salesman_name(ref myorder);
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(salesmanName, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 11:
                        if (myorder.requisition != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(myorder.requisition, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 12:
                        if (myorder.ExtRef != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(myorder.ExtRef, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 13:
                        theDoc.AddTextStyled(HeaderText);
                        theDoc.TextStyle.HPos = 1;
                        theString = String.Concat(CompanyNo, "<br>");
                        theDoc.AddTextStyled(theString);
                        break;
                    case 14:
                        theDoc.AddTextStyled(HeaderText);
                        theDoc.TextStyle.HPos = 1;
                        theString = String.Concat(myorder.ShipDate.ToString("d"), "<br>");
                        theDoc.AddTextStyled(theString);
                        break;
                    case 15:
                        if (shipVia != string.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(txt_shipVia, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 16:
                        if (myorder.TermsOfDelivery != 0)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(get_txt_TermsOfDelivery(ref myorder), "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 17:
                        if (myorder.Dim1 != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(myorder.Dim1, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 18:
                        if (myorder.Dim2 != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(myorder.Dim2, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 19:
                        if (myorder.Dim3 != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(myorder.Dim3, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 20:
                        if (myorder.Dim4 != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(myorder.Dim4, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 21:
                        if (adrBIC != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(adrBIC, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                    case 22:
                        if (adrIBAN != String.Empty)
                        {
                            theDoc.AddTextStyled(HeaderText);
                            theDoc.TextStyle.HPos = 1;
                            theString = String.Concat(adrIBAN, "<br>");
                            theDoc.AddTextStyled(theString);
                        }
                        break;
                        // 'Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>")
                }
            }
            conn.Close();
            return "0";
        }




        private string create_Order_inf_01(int RepID, ref Doc theDoc, int FontSize, ref OrderSales myorder, ref CompanyInf mycompany)
        {
            var dt = Get_ReportInf(RepID);
            DataRow dr;
            string[] Header = new string[20];
            string theString = string.Empty;
            string orderstr;


            DateTime quotationDate = DateTime.Today;
            string shipVia = string.Empty;
            Boolean LT = true;
            // Dim param(20) As String
            // Dim utils As New winfinance.utils
            ReportObject wf_rp_obj = new ReportObject(conn_str, compID, ordlanguage);
            wf_rp_obj.fontSize = FontSize;
            wf_rp_obj.fontFamily = fontFamily;

            int ColID = 0;
            int rowID = 0;
            string HeaderText;
            string TranslateTo;

            string align_1 = "right";
            string align_2 = "left";
            string body = wf_rp_obj.html_head();
            body = String.Concat(body, "<table border=0 cellspacing=0  height=10 width=", p_width, "px>");

            if (RepID == 1000)
            {
                if (Proforma == 0)
                {
                    Header[1] = get_global_dictionary(142, ref LT); // 'winfinance.wf_sys.Get_text_by_id(142, language) ' invoice;
                }
                else
                {
                    Header[1] = get_global_dictionary(1581, ref LT);  // 'winfinance.wf_sys.Get_text_by_id(142, language) ' invoice;
                    if (myorder.InvoiceNo == 0) myorder.InvoiceNo = myorder.OrderNo;
                }
                Header[2] = get_global_dictionary(410, ref LT); // ' credit note
                theDoc.TextStyle.HPos = 0;
                if (CreInvFactor == -1) theString = Header[2]; else theString = Header[1];

                body = String.Concat(body, "<tr><td valign=top align=", align_1, "><font class='invoice'><b>", theString, "</b></td>");
                body = String.Concat(body, "<td width=4px> </td>");
                body = String.Concat(body, "<td align=", align_2, "><font class='invoice'><b>", myorder.InvoiceNo.ToString(), "</b></td></tr>");

            }
            if (RepID == 1001)
            {
                Header[1] = get_global_dictionary(143, ref LT); // 'winfinance.wf_sys.Get_text_by_id(143, language) ' Order
                body = String.Concat(body, "<tr><td valign=top align=right><font class='invoice'><b>", Header[1], "</b></td>");
                body = String.Concat(body, "<td width=4px> </td>");
                if (myorder.DeliveryNoteNo > 0)
                {
                    body = String.Concat(body, "<td align=left><font class='invoice'><b>", myorder.OrderNo.ToString(), "</b></td></tr>");
                }
                else
                {
                    body = String.Concat(body, "<td align=left><font class='invoice'><b>", myorder.OrderNo.ToString(), "-", myorder.DeliveryNoteNo.ToString(), "</b></td></tr>");
                }
            }

            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT ColID, Pos, ";
            mysql = String.Concat(mysql, " (select max(translateto) from  ac_Companies_sellers_rep_inf_dictionary tb2 where tb2.COmpID = tb1.CompID AND tb2.SellerID = tb1.SellerID AND tb2.RepID = tb1.RepID AND tb2.ColID = tb1.ColID AND tb2.Language = @Language) as TranslateTo ");
            mysql = String.Concat(mysql, " FROM ac_Companies_sellers_rep_inf tb1 where tb1.CompID = @CompID AND tb1.SellerID = @SellerID AND tb1.RepID =  @RepID AND tb1.pos > 0   ");

            mysql = String.Concat(mysql, " Order by pos ");
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = myorder.seller;
            comm.Parameters.Add("@RepID", SqlDbType.Int).Value = RepID;
            comm.Parameters.Add("@Language", SqlDbType.NVarChar).Value = ordlanguage;

            conn.Open();
            orderstr = myorder.OrderNo.ToString();
            var myr = comm.ExecuteReader();
            while (myr.Read())
            {
                ColID = (int)myr["ColID"];
                rowID = ColID - 1;
                dr = dt.Rows[rowID];
                HeaderText = dr["ColDesc"].ToString();
                TranslateTo = myr["TranslateTo"].ToString();
                if (TranslateTo != String.Empty) HeaderText = TranslateTo;
                theDoc.TextStyle.HPos = 0;
                switch (ColID)
                {
                    case 1:
                        if (myorder.OrderNo > 0)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            if (myorder.DeliveryNoteNo > 0)
                            {
                                body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.OrderNo.ToString(), "</td></tr>");
                            }
                            else
                            {
                                body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.OrderNo.ToString(), "-", myorder.DeliveryNoteNo.ToString(), "</td></tr>");
                            }
                        }
                        break;
                    case 2:
                        if (myorder.QuotationNo > 0)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.QuotationNo.ToString(), "</td></tr>");

                        }
                        break;
                    case 3:
                        body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                        body = String.Concat(body, "<td width=4px> </td>");
                        body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.InvoiceDate.ToString("d"), "</td></tr>");
                        break;
                    case 4:
                        body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                        body = String.Concat(body, "<td width=4px> </td>");
                        body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.OrderDate.ToString("d"), "</td></tr>");
                        break;
                    case 5:
                        body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                        body = String.Concat(body, "<td width=4px> </td>");
                        body = String.Concat(body, "<td align=left><font class='invoice'>", quotationDate.ToString("d"), "</td></tr>");
                        break;
                    case 6:
                        body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                        body = String.Concat(body, "<td width=4px> </td>");
                        body = String.Concat(body, "<td align=left><font class='invoice'>", ad_account, "</td></tr>");
                        break;
                    case 7:
                        if (Phone != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", Phone, "</td></tr>");
                        }
                        break;
                    case 8:
                        if (VATNumber != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", VATNumber, "</td></tr>");
                        }
                        break;
                    case 9:
                        if (EAN != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", EAN, "</td></tr>");
                        }
                        break;
                    case 10:
                        if (myorder.salesman != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.salesman, "</td></tr>");
                        }
                        break;
                    case 11:
                        if (myorder.requisition != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.requisition, "</td></tr>");
                        }
                        break;
                    case 12:
                        if (myorder.ExtRef != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.ExtRef, "</td></tr>");
                        }
                        break;
                    case 13:
                        body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                        body = String.Concat(body, "<td width=4px> </td>");
                        body = String.Concat(body, "<td align=left><font class='invoice'>", CompanyNo, "</td></tr>");
                        break;
                    case 14:
                        body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                        body = String.Concat(body, "<td width=4px> </td>");
                        body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.ShipDate.ToString("d"), "</td></tr>");
                        break;
                    case 15:
                        if (shipVia != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", shipVia, "</td></tr>");
                        }
                        break;
                    case 16:
                        if (myorder.TermsOfDelivery != 0)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.TermsOfDelivery, "</td></tr>");
                        }
                        break;
                    case 17:
                        if (myorder.Dim1 != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.Dim1, "</td></tr>");
                        }
                        break;
                    case 18:
                        if (myorder.Dim2 != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.Dim2, "</td></tr>");
                        }
                        break;
                    case 19:
                        if (myorder.Dim3 != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.Dim3, "</td></tr>");
                        }
                        break;
                    case 20:
                        if (myorder.Dim4 != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", myorder.Dim4, "</td></tr>");
                        }
                        break;
                    case 21:
                        if (adrBIC != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", adrBIC, "</td></tr>");
                        }
                        break;
                    case 22:
                        if (adrIBAN != String.Empty)
                        {
                            body = String.Concat(body, "<tr><td  align=right><font class='invoice'>", HeaderText, "</td>");
                            body = String.Concat(body, "<td width=4px> </td>");
                            body = String.Concat(body, "<td align=left><font class='invoice'>", adrIBAN, "</td></tr>");
                        }
                        break;


                        //'Header = String.Concat(Header, " <td align=left width=", colwidth, "px ><font class='invoice'>")
                }


            }
            body = String.Concat(body, "</table></body>");
            theDoc.TextStyle.HPos = 0;
            theDoc.TextStyle.VPos = 0;
            theDoc.AddImageHtml(body, false, p_width + 8, false);

            conn.Close();
            return "0";
        }


        private string Get_Salesman_name(ref OrderSales myorder)
        {
            string myName = string.Empty;
            if (myorder.salesman != String.Empty)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string MySql = "Select name FROM tr_sale_salesmen Where CompID = @CompID And SalesmanID = @SalesmanID";
                var myComm = new SqlCommand(MySql, conn);
                myComm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                myComm.Parameters.Add("@SalesmanID", SqlDbType.NVarChar, 20).Value = myorder.salesman;
                conn.Open();
                var myr = myComm.ExecuteReader();
                if (myr.Read()) myName = myr["name"].ToString(); else myName = myorder.salesman;
                conn.Close();
            }
            return myName;
        }


        private string get_txt_TermsOfDelivery(ref OrderSales myorder)
        {
            string mydelivery = string.Empty;
            if (myorder.TermsOfDelivery > 0)
            {
                string mysql = "SELECT max(Description)FROM tr_termsofdelivery WHERE CompID = @CompID AND TermID = @P_TermsOfDelivery ";
                SqlConnection conn = new SqlConnection(conn_str);
                var Comm = new SqlCommand(mysql, conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@P_TermsOfDelivery", SqlDbType.Int).Value = myorder.TermsOfDelivery;
                conn.Open();
                mydelivery = Comm.ExecuteScalar().ToString();
                conn.Close();
            }
            return mydelivery;
        }





        // Private Sub get_payment_delivery_inf()
        //     Dim MySql As String = "SELECT max(Describtion) FROM ad_Payment_deb WHERE CompID = @CompID AND PaymentDeb = @P_PaymentDeb "
        //     Dim Conn As SqlConnection = New SqlConnection(constr)
        //  Dim Comm As SqlCommand = New SqlCommand(MySql, Conn)
        // Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID
        // Comm.Parameters.Add("@P_PaymentDeb", SqlDbType.NVarChar, 20).Value = IIf(TermsOfPayment = String.Empty, DBNull.Value, TermsOfPayment)
        // Comm.Parameters.Add("@P_ShipID", SqlDbType.Int).Value = shipVia
        //Comm.Parameters.Add("@P_TermsOfDelivery", SqlDbType.Int).Value = TermsOfDelivery
        // Conn.Open()
        // If TermsOfPayment<> String.Empty Then
        //txt_TermsOfPayment = Comm.ExecuteScalar.ToString()
        // End If
        // If shipVia <> 0 Then
        //Comm.CommandText = "SELECT max(Description) FROM tr_ShippedVia WHERE CompID = @CompID AND ShipID = @P_ShipID "
        //txt_shipVia = Comm.ExecuteScalar.ToString()
        //End If
        //Conn.Close()
        //End Sub


        private DataTable Get_ReportInf(int RepID)
        {

            SqlConnection mc = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_user"].ConnectionString);
            var mycom = new SqlCommand("wf_apl_report_seller_inf_get", mc);
            mycom.CommandType = CommandType.StoredProcedure;
            mycom.Parameters.Add("@RepID", SqlDbType.Int).Value = 1;
            mycom.Parameters.Add("@OnRepID", SqlDbType.NVarChar, 20).Value = "%0%";
            mycom.Parameters.Add("@language", SqlDbType.NVarChar, 20).Value = ordlanguage;
            var dt = new DataTable();
            mc.Open();
            dt.Load(mycom.ExecuteReader());
            mc.Close();
            return dt;
        }


        public string create_payment(ref OrderSales myorder)
        {
            SqlConnection conn = new SqlConnection(conn_str);

            string mysql = "SELECT Describtion FROM ad_Payment_deb WHERE CompID = @CompID AND PaymentDeb = @PaymentDeb ";
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@PaymentDeb", SqlDbType.NVarChar, 20).Value = myorder.TermsOfPayment;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();



            string thestring;
            string[] header = new string[5];
            Boolean LT = false;

            header[1] = get_global_dictionary(147, ref LT); // 'winfinance.wf_sys.Get_text_by_id(147, language) '"DueDate"
            header[2] = get_global_dictionary(417, ref LT); // 'winfinance.wf_sys.Get_text_by_id(417, language) '"Shipped via"
            header[3] = get_global_dictionary(1196, ref LT); // 'winfinance.wf_sys.Get_text_by_id(1196, language) '"Terms of delivery"
            thestring = "";
            if (!string.IsNullOrEmpty(myorder.TermsOfPayment))
            {
                //  MySQl = String.Concat("SELECT Describtion FROM ad_Payment_deb WHERE CompID = ", compID, " AND PaymentDeb = '", TermsOfPayment, "'")
                if (myr.Read())
                {
                    thestring = String.Concat(myr["Describtion"].ToString(), "<br>", header[1], ": ", myorder.PayDate.ToString("d"));
                }
            }
            //  if (myorder. shipVia <> 0 Then
            //       MySQl = String.Concat("SELECT Description FROM tr_ShippedVia WHERE COmpID = ", compID, " AND ShipID = '", shipVia, "'")
            //       myreader = wf_util.wf_Reader(constr, MySQl)
            //       If myreader.Read Then
            //           thestring = String.Concat(thestring, "<br>", header[2), " ", myreader("Description").ToString())
            //       End If
            //   End If
            //   If TermsOfDelivery <> 0 Then
            //       MySQl = String.Concat("SELECT Description FROM tr_termsofdelivery WHERE CompID = ", compID, " AND TermID = '", TermsOfDelivery, "'")
            //       myreader = wf_util.wf_Reader(constr, MySQl)
            //       If myreader.Read Then
            //           thestring = String.Concat(thestring, "<br>", header[3), " ", myreader("Description").ToString())
            //       End If
            //       End If 

            return thestring;
        }


        public void get_Text(int rid, InvCre CreInv)
        {

            if (rid == 1)
            {
                Invoice_Text = ReportTextGet("tr_Sale_Invoice", SellerID, language);
                if (CreInv == InvCre.CreditNote) Invoice_Text = String.Concat(Invoice_Text, ReportTextGet("tr_Sale_onlyCrn", SellerID, language));
                if (CreInv == InvCre.CreditNote) Invoice_Text = String.Concat(Invoice_Text, ReportTextGet("tr_Sale_onlyInv", SellerID, language));
            }
            if (rid == 2) Invoice_Text = ReportTextGet("tr_Sale_Order", SellerID, language);
            if (rid == 3) Invoice_Text = ReportTextGet("tr_Sale_Delivery", SellerID, language);
            if (rid == 4) Invoice_Text = ReportTextGet("tr_Sale_Quotation", SellerID, language);
            Vat_Text = ReportTextGet("tr_Sale_VAT", SellerID, language);
            noVAT_Text = ReportTextGet("tr_Sale_NoVAT", SellerID, language);
            proVAT_Text = ReportTextGet("tr_Sale_proVAT", SellerID, language);
            Vat_Text = Vat_Text.Replace(Convert.ToChar(13).ToString(), "<br>");
            noVAT_Text = noVAT_Text.Replace(Convert.ToChar(13).ToString(), "<br>");
            proVAT_Text = proVAT_Text.Replace(Convert.ToChar(13).ToString(), "<br>");
        }


        public string ReportTextGet(string R_Type, int SellerID, string Country)
        {
            SqlConnection conn = new SqlConnection(conn_str);

            string repstr = string.Empty;
            string mysql = "SELECT RText FROM ac_companies_sellers_rep_txt WHERE CompID = @CompID AND SellerID = @SellerID AND CountryID = @CountryID AND RType = @RType ";
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = SellerID;
            comm.Parameters.Add("@CountryID", SqlDbType.NVarChar, 20).Value = Country;
            comm.Parameters.Add("@RType", SqlDbType.NVarChar, 20).Value = R_Type;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                repstr = myr["RText"].ToString();
            }
            conn.Close();
            return repstr;
        } // end



        private string Create_totals_1(ref Doc thedoc, int rid, ref OrderSales myorder)
        {
            string[] header = new string[10];

            decimal val;
            string theString;
            int VatOk = 0;
            Boolean LT = false;

            if (myorder.TotalVatEx + myorder.TotalVatIn != 0) VatOk = 1; else VatOk = 0;


            header[1] = get_global_dictionary(418, ref LT); // 'winfinance.wf_sys.Get_text_by_id(418, language) '"Discount"
            header[2] = tax_text(); //  ' winfinance.wf_sys.Get_text_by_id(419, language) '"Selectiv tax"
            if (VatOk == 1)
            {
                header[3] = get_global_dictionary(420, ref LT);  // 'winfinance.wf_sys.Get_text_by_id(420, language) '"Invoice total"
            }
            else
            {
                header[3] = String.Concat(get_global_dictionary(420, ref LT), " ", myorder.Currency);  //  'winfinance.wf_sys.Get_text_by_id(420, language) '"Invoice total"
            }

            switch (rid)
            {
                case 2:
                    header[3] = get_global_dictionary(1290, ref LT); // 'winfinance.wf_sys.Get_text_by_id(1290, language) '"order"
                    break;
                case 3:
                    header[3] = get_global_dictionary(1290, ref LT); // 'winfinance.wf_sys.Get_text_by_id(1290, language) '"order"
                    break;
                case 4:
                    header[3] = get_global_dictionary(1289, ref LT); // 'winfinance.wf_sys.Get_text_by_id(1289, language) '"order"
                    break;
            }
            header[4] = get_global_dictionary(40, ref LT); // 'winfinance.wf_sys.Get_text_by_id(40, language) '"VAT"
            header[5] = String.Concat(get_global_dictionary(421, ref LT), " ", myorder.Currency); // 'winfinance.wf_sys.Get_text_by_id(421, language) '"Total incl VAT"
            header[6] = "";
            if (myorder.TotalInvDiscount != 0)
            {
                thedoc.TextStyle.HPos = 0;
                thedoc.AddTextStyled(header[1]);
                thedoc.TextStyle.HPos = 1;
                theString = String.Concat(myorder.TotalInvDiscount.ToString("N"), "<br>");
                thedoc.AddTextStyled(theString);
            }

            thedoc.TextStyle.HPos = 0;
            thedoc.AddTextStyled(header[3]);
            theString = String.Concat(myorder.Total.ToString("N"), "<br>");
            thedoc.TextStyle.HPos = 1;
            thedoc.AddTextStyled(theString);
            if (myorder.TotalTaxAmount != 0)
            {
                thedoc.TextStyle.HPos = 0;
                thedoc.AddTextStyled(header[2]);
                thedoc.TextStyle.HPos = 1;
                theString = String.Concat(myorder.TotalTaxAmount.ToString("N"), "<br>");
                thedoc.AddTextStyled(theString);
            }

            if (VatOk == 1)
            {
                val = myorder.TotalVatEx + myorder.TotalVatIn;
                thedoc.TextStyle.HPos = 0;
                thedoc.AddTextStyled(header[4]);
                theString = String.Concat(val.ToString("N"), "<br>");
                thedoc.TextStyle.HPos = 1;
                thedoc.AddTextStyled(theString);
                theString = String.Concat("<hr><br>");
                thedoc.AddTextStyled(theString);
                val = myorder.TotalVatEx + myorder.TotalVatIn + myorder.Total + myorder.TotalTaxAmount;
                thedoc.TextStyle.HPos = 0;
                theString = String.Concat("<b>", header[5], "</b>");
                thedoc.AddTextStyled(theString);
                theString = String.Concat("<b>", val.ToString("N"), "</b><br>");
                thedoc.TextStyle.HPos = 1;
                thedoc.AddTextStyled(theString);
            }
            return "0";
        }


        private string Create_totals_5(ref Doc thedoc, int rid, ref OrderSales myorder)
        {
            string[] header = new string[10];
            decimal val;
            string theString;

            Boolean LT = false;

            header[1] = get_global_dictionary(418, ref LT); // 'winfinance.wf_sys.Get_text_by_id(418, language) '"Discount"
            header[2] = tax_text(); //  ' winfinance.wf_sys.Get_text_by_id(419, language) '"Selectiv tax"
            header[3] = get_global_dictionary(420, ref LT); //get_global_dictionary(420, LT) '"Invoice total"
            header[4] = get_global_dictionary(40, ref LT); //get_global_dictionary(40, LT) '"VAT"
            header[5] = get_global_dictionary(751, ref LT); //get_global_dictionary(751, LT) '"Total"
            header[6] = "";

            if (myorder.TotalInvDiscount != 0)
            {
                thedoc.TextStyle.HPos = 0;
                thedoc.AddTextStyled(header[1]);
                thedoc.TextStyle.HPos = 1;
                theString = String.Concat(myorder.TotalInvDiscount.ToString("N"), "<br>");
                thedoc.AddTextStyled(theString);
            }


            if (myorder.TotalTaxAmount != 0)
            {
                thedoc.TextStyle.HPos = 0;
                thedoc.AddTextStyled(header[2]);
                thedoc.TextStyle.HPos = 1;
                theString = String.Concat(myorder.TotalTaxAmount.ToString("N"), "<br>");
                thedoc.AddTextStyled(theString);
            }

            val = myorder.TotalVatEx + myorder.TotalVatIn + myorder.Total + myorder.TotalTaxAmount;

            thedoc.TextStyle.HPos = 0;
            theString = String.Concat("<b>", header[5], "</b>");
            thedoc.AddTextStyled(theString);
            theString = String.Concat("<b>", val.ToString("N"), "</b><br>");
            thedoc.TextStyle.HPos = 1;
            thedoc.AddTextStyled(theString);

            return "0";
        }




        private string tax_text()
        {
            Boolean LT = false;
            string taxText = string.Empty; //Company_Setup_Get(constr, CompID, "tr_vt_saTaxText", "")
            taxText = get_global_dictionary(419, ref LT); // 'winfinance.wf_sys.Get_text_by_id(419, language) '"Selectiv tax"
            return taxText;
        }



        private void get_company_inf()
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "select CompanyName, Street,HouseNumber,InhouseMail,PostalZone,CityName,CompanyNo, PostalCodeCity,emailsender from ac_companies where compid = @CompID ";
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            var myr = comm.ExecuteReader();
            if (myr.Read())
            {
                CompanyName = myr["CompanyName"].ToString();
                CompanyNo = myr["CompanyNo"].ToString();
                // ' emailFrom = myreader("emailSender").ToString()
            }
            conn.Close();

        }




    }
}