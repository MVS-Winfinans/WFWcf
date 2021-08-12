using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
 

        private string language = "dan";
        private string ordlanguage = "dan";
        private int SellerID;
        //string SummarizeInvoiceMask;
        //int Proforma;

        public Boolean noVat;
        public Boolean proVat;

        //private int UseSeparator = 0;

        //company
        private string CompanyNo;
        private string CompanyName;

   
        string ad_account;
        string VATNumber;
        string Phone;
        string EAN;

        //string BIC;
        //string IBAN;
        //string BankName;
        //string BankAddress;
        //string RegistrationNo;

        string adrBIC;
        string adrIBAN;
        string AdrNote;
        string AdrPhone;


        string Invoice_Text;
        string Vat_Text;
        string noVAT_Text;
        string proVAT_Text;

        double CreInvFactor = 1;



        public pdfSale(ref DBUser DBUser)
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            conn_str = wfconn.ConnectionGetByGuid_02(ref DBUser);
            compID = DBUser.CompID;
            SellerID = 0;
            language = "dan";
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