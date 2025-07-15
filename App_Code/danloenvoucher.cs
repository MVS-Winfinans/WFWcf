using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;

/// <summary>
/// Summary description for danloenvoucher
/// </summary>
public class danloenvoucher
{

    int compID;
    string connstr;
    public int number;
    public int invoice;
    public int settleno;
    public decimal amount;
    public decimal amountCu;
    public decimal vatcalc;
    public DateTime enterdate = DateTime.Today;
    public DateTime payDate = DateTime.Today;
    public string account;
    public string debcr;
    public int VATYn;
    public string currency;
    public string currency_default;
    public string description;
    public string dim1;
    public string dim2;
    public string dim3;
    public string dim4;
    public string dimx;
    int RetVal;

    XmlDocument XMLDoc = new XmlDocument();
    public danloenvoucher(string myConnstr, int myCompID)
    {
        compID = myCompID;
        connstr = myConnstr;


    }

    public string vouchers_out_test()
    {
        string myxml;
        myxml = "<vouchers>";
        myxml = string.Concat(myxml, "<voucher><number>10</number><enterdate>27-02-2007</enterdate>");
        myxml = string.Concat(myxml, "<transaction>");
        myxml = string.Concat(myxml, "<invoice>100</invoice><account>1010</account><description>Et bilag</description><amount>345.67</amount><novat>1</novat><debcr>0</debcr>");
        myxml = string.Concat(myxml, "<dim1/><dim2/><dim3/><dim4/><dimx/>");
        myxml = string.Concat(myxml, "</transaction>");
        myxml = string.Concat(myxml, "</voucher>");
        myxml = string.Concat(myxml, "</vouchers>");
        return myxml;
    }

    public int vouchers_in(string vouchers)
    {
        XmlNodeList Elem_L1;
        string xmlstr = string.Empty;
        XmlDocument XMLDoc = new XmlDocument();
        string xmlin = vouchers; // vouchers_out_test()

        // string xmlin = vouchers_out_test();

        // Optionally save XML to file, e.g.:
        // XMLDoc.Save($"c:\\tmp_danloen\\{DateTime.Now:yyyyddMMHHmmss}.xml");

        if (RetVal == 0)
        {
            try
            {
                XMLDoc.LoadXml(xmlin);

                string fileName = string.Concat("C://temp//danloen/",DateTime.Now.ToString("yyyyddMMHHmmss"),"_loen.XML");
                XMLDoc.Save(fileName);
                //XMLDoc.Save("C://Temp/danloen/{DateTime.Now:yyyyddMMHHmmss}.xml");

                Elem_L1 = XMLDoc.GetElementsByTagName("lines");

                if (Elem_L1.Count > 0)
                {
                    if (IsValidCompanyNo(Elem_L1[0].Attributes["cvrnr"].Value))
                    {
                        Elem_L1 = XMLDoc.GetElementsByTagName("line");

                        if (Elem_L1.Count > 0)
                        {
                            foreach (XmlNode Elem_0 in Elem_L1)
                            {
                                xmlstr = Elem_0.OuterXml;
                                transaction_in(xmlstr);
                                voucher_save();
                            }
                        }
                        else
                        {
                            RetVal = 104; // "No vouchers in file"
                        }
                    }
                    else
                    {
                        RetVal = 102; // "Invalid company number (CVR)"
                    }

                    // Optionally save XML to file, e.g.:
                    // XMLDoc.Save($"c:\\tmp_danloen\\{DateTime.Now:yyyyddMMHHmmss}.xml");
                }
            }
            catch (Exception ex)
            {
                RetVal = 105; // "Wrong XML:" + ex.Message
            }
        }

        return RetVal;
    }

    public string transaction_in(string voucher)
    {
        DateTime in_enterdate;
        string in_str;
        XmlNodeList Elem_L1;
        string xmlstr = string.Empty;
        XmlDocument XMLDoc = new XmlDocument();
        string CprNr;

        XMLDoc.LoadXml(voucher);

        amount = 0m;
        VATYn = 0;
        amountCu = 0m;
        vatcalc = 0m;
        currency = currency_default;
        decimal parsedAmount = 0;

        Elem_L1 = XMLDoc.GetElementsByTagName("accountnr");
        if (Elem_L1.Count > 0)
        {
            account = Elem_L1[0].InnerText;
        }

        Elem_L1 = XMLDoc.GetElementsByTagName("text");
        if (Elem_L1.Count > 0)
        {
            description = Elem_L1[0].InnerText;
        }

        Elem_L1 = XMLDoc.GetElementsByTagName("cprnr");
        if (Elem_L1.Count > 0)
        {
            CprNr = Elem_L1[0].InnerText;
            if (CprNr.Length > 4)
            {
                description += " (" + CprNr + ")";
            }
        }

        Elem_L1 = XMLDoc.GetElementsByTagName("amount");
        if (Elem_L1.Count > 0)
        {
            string amountText = Elem_L1[0].InnerText;
            decimal.TryParse(amountText.Replace(".", ","), out parsedAmount);

        }

        debcr = "d";

        Elem_L1 = XMLDoc.GetElementsByTagName("novat");
        if (Elem_L1.Count > 0)
        {
            VATYn = (Elem_L1[0].InnerText == "1") ? 1 : 0;
        }
        VATYn = 1;

        amountCu = amount;

        Elem_L1 = XMLDoc.GetElementsByTagName("postingdate");
        if (Elem_L1.Count > 0)
        {
            in_str = Elem_L1[0].InnerText;
            if (DateTime.TryParse(in_str, out in_enterdate))
            {
                enterdate = in_enterdate;
            }
            else
            {
                enterdate = default(DateTime);
            }
        }

        return xmlstr;
    }

    public void voucher_save()
    {
        string mysql = "INSERT INTO fi_vouchers_imp ";
        mysql = string.Concat(mysql," (CompID,Voucher,EnterDate, Invoice, DAccount,CAccount, Description, Amount, cuAmount,VatCalc,VATYn,Currency,PayDate,Source,SourceRef,Dim1,Dim2,Dim3,Dim4)");
        mysql = string.Concat(mysql," values (@P_CompID, @P_Voucher, @P_EnterDate, @P_Invoice, ");
        mysql = string.Concat(mysql," CASE WHEN @P_Amount > 0 THEN @P_Account ELSE null END,   CASE WHEN @P_Amount < 0 THEN @P_Account ELSE null END, ");
        mysql = string.Concat(mysql," @P_Description,  ABS(@P_AmountCu),  ABS(@P_Amount),@P_VatCalc, @P_VATYn, @P_Currency, @P_PayDate,  @P_Source, @P_Sourceref,@Dim1,@Dim2,@Dim3,@Dim4) ");
        using (var conn = new SqlConnection(connstr))
        {
            SqlCommand myComm = new SqlCommand(mysql, conn);
            myComm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            myComm.Parameters.Add("@P_Voucher", SqlDbType.Int).Value = number;
            myComm.Parameters.Add("@P_EnterDate", SqlDbType.DateTime).Value = enterdate != DateTime.MinValue ? (object)enterdate : DateTime.Today;
            myComm.Parameters.Add("@P_Invoice", SqlDbType.Int).Value = invoice;
            myComm.Parameters.Add("@P_Account", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(account) ? (object)DBNull.Value : account;
            myComm.Parameters.Add("@P_Description", SqlDbType.NVarChar, 200).Value = string.IsNullOrEmpty(description) ? (object)DBNull.Value : description;
            myComm.Parameters.Add("@P_Amount", SqlDbType.Money).Value = amount;
            myComm.Parameters.Add("@P_AmountCu", SqlDbType.Money).Value = amountCu;
            myComm.Parameters.Add("@P_VatCalc", SqlDbType.Money).Value = vatcalc;
            myComm.Parameters.Add("@P_VATYn", SqlDbType.Int).Value = VATYn;
            myComm.Parameters.Add("@P_Currency", SqlDbType.NVarChar, 20).Value = string.IsNullOrEmpty(currency) ? (object)DBNull.Value : currency;
            myComm.Parameters.Add("@P_PayDate", SqlDbType.DateTime).Value = payDate != DateTime.MinValue ? (object)payDate : DateTime.Today;
            myComm.Parameters.Add("@P_Source", SqlDbType.Int).Value = 13;
            myComm.Parameters.Add("@P_Sourceref", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim1", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(dim1) ? (object)DBNull.Value : dim1;
            myComm.Parameters.Add("@Dim2", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(dim2) ? (object)DBNull.Value : dim2;
            myComm.Parameters.Add("@Dim3", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(dim3) ? (object)DBNull.Value : dim3;
            myComm.Parameters.Add("@Dim4", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(dim4) ? (object)DBNull.Value : dim4;
            conn.Open();
            myComm.ExecuteNonQuery();
        }
    }

    private void transactions_default()
    {
        invoice = 0;
        settleno = 0;
        amount = 0m;
        amountCu = 0m;
        enterdate = DateTime.Today;
        payDate = DateTime.Today;
        account = string.Empty;
        debcr = "0";
        VATYn = 0;
        currency = string.Empty;
        description = string.Empty;
        dim1 = string.Empty;
        dim2 = string.Empty;
        dim3 = string.Empty;
        dim4 = string.Empty;
        dimx = string.Empty;
    }

    public void get_dimx()
    {
        if (!string.IsNullOrEmpty(dimx))
        {
            string DimID = string.Empty;
            int DimNo = 0;

            string mysql = "SELECT @DimNo = ISNULL(DimNo, 0), @DimID = DimID FROM fi_Dimensions WHERE CompID = @CompID AND Dimtext = @Dimx";
            using (var conn = new SqlConnection(connstr))
            {
                SqlCommand myComm = new SqlCommand(mysql,conn);

                myComm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                myComm.Parameters.Add("@Dimx", SqlDbType.NVarChar, 50).Value = dimx;
                myComm.Parameters.Add("@DimNo", SqlDbType.Int).Direction = ParameterDirection.Output;
                myComm.Parameters.Add("@DimID", SqlDbType.NVarChar,50).Direction = ParameterDirection.Output;
                conn.Open();
                myComm.ExecuteNonQuery();
                DimNo = (int)myComm.Parameters["@DimNo"].Value;
                DimID = (string)myComm.Parameters["@DimID"].Value;
            }
            if (DimNo == 1) dim1 = DimID;
            else if (DimNo == 2) dim2 = DimID;
            else if (DimNo == 3) dim3 = DimID;
            else if (DimNo == 4) dim4 = DimID;
        }
    }

    public void get_currency()
    {
        string mysql = "SELECT isnull(Currency,'DKK') FROM ac_companies WHERE CompID = @CompID";
        using (var conn = new SqlConnection(connstr))
        {
            SqlCommand myComm = new SqlCommand(mysql, conn);
            myComm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            object currency_default = myComm.ExecuteScalar();
        }
    }

    private bool IsValidCompanyNo(string TestCVR)
    {
        string Answer;
        bool Result;
        string mysql = "Select CompanyNo from Ac_Companies where CompID = @CompID and CompanyNo = @CompNo";
        using (var conn = new SqlConnection(connstr))
        {
            SqlCommand myComm = new SqlCommand(mysql,conn);
            myComm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            myComm.Parameters.Add("@CompNo", SqlDbType.NVarChar).Value = TestCVR;
            Answer = (string)myComm.ExecuteScalar();
            if (Answer.Length > 1)
            {
                Result = true;
            }
            else
            {
                Result = false;
            }
        }
        return Result;
    }


}