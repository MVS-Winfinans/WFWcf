using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;


/// <summary>
/// Summary description for ReportObject
/// </summary>
/// 

namespace wfws
{

    public class reportItem
    {
        public int ObjID;
        public Boolean visible;
        public Boolean showFrame;
        public int fontSize;
        public int objtype;
        public string RectString;
        public int r_width;
        public int r_hight;
        public int Align01;
    }


public class ReportObject
{


    int compID;
    string conn_str;

        public string Language;
        public string CompLanguage;
        public int fontSize;
        public string fontFamily;
        public int StatID;
        public Boolean ShowGrid;
        public int FontSizeBody;
        public int paperWidth;
        public int paperHeight;
        public string companyFileName;

        //private string head_1;
        //private string head_2;
        //private int PaperSize;
        private string SellerName;
        private string ReportFooter;
        private string ReportHeader;
         // public IList<reportItem> Items = new List<reportItem>();




        public ReportObject(string connstr,int CompID,string v_Language)
    {
     
        conn_str = connstr;
        compID = CompID;
        if (v_Language == string.Empty) Language = "UK"; else Language = v_Language;
    }



    public reportItem[] Report_object_properties_Seller_Load(int SellerID, int RepID )
    {
       string TheString = string.Empty;
       var Items = new reportItem[30];

            for (int i = 0; i < 20; i++) {
                Items[i] = new reportItem();
                Items[i].ObjID = i;
                if (i < 13) Items[i].visible = true; else Items[i].visible = false;
                Items[i].fontSize = 10;
                Items[i].showFrame = false;
                if (RepID >= 9000) {
                    switch (i) {
                        case 1 : TheString = "50 750 570 800";break; //  ' header name 
                        case 2 : TheString = "50 700 570 700"; break; // ' header 
                        case 3 : TheString = "50 20 570 60"; break;  // ' buttom  
                        case 4 : TheString = "400 600 570 700"; break; // ' logo
                        case 11 : TheString = "50 260 120 300"; break; // ' invoice text
                     }
                }
                if (RepID < 9000) {
                    switch (i)
                    {
                        case 1: TheString = "50 650 200 700"; break; // ' header name / invoice address
                        case 2: TheString = "200 650 400 700"; break; //  ' header  / ship address
                        case 3: TheString = "50 700 570 785"; break; //   ' 
                        case 4: TheString = "400 600 570 700"; break; //  ' Invoice information
                        case 5: TheString = "50 550 570 600"; break; //   ' invoice text 1
                        case 6: TheString = "40 200 570 560"; break; //   ' items
                        case 7: TheString = "50 100 370 160"; break; //   ' payments
                        case 8: TheString = "370 70 570 160"; break; //   ' totals
                        case 9: TheString = "50 70 370 100"; break; //    ' text 2
                        case 10: TheString = "50 10 570 40"; break; // 
                        case 11: TheString = "50 260 120 300"; break; //  ' invoice text
                        case 12: TheString = "50 60 200 70"; break; //   ' payment ref
                        case 13: TheString = "50 100 200 200";break; //  ' VAT
                     }
                }
                Items[i].RectString = TheString;
            }

        SqlConnection conn = new SqlConnection(conn_str);
        int L_x; int L_y; int R_x; int R_y;
        int id = 0;

        string mysql = "SELECT * FROM ac_companies_sellers_rep_obj WHERE CompID = @CompID AND SellerID = @SellerID  AND RepID = @RepID order by ObjID";
        SqlCommand comm = new SqlCommand(mysql, conn);
        comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
        comm.Parameters.Add("@RepID", SqlDbType.Int).Value = RepID;
        comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = SellerID;
        conn.Open();
        SqlDataReader myr = comm.ExecuteReader();
        while (myr.Read())
        {
            id = (Int32)myr["ObjID"];
            if (id < 20)
            {
                L_x = (Int32)myr["L_x"];
                L_y = (Int32)myr["L_y"];
                R_x = (Int32)myr["R_x"];
                R_y = (Int32)myr["R_y"];

                if (L_x < 0) L_x = 50;
                if (L_y < 0) L_y = 50;
                if (L_y > 800) L_y = 600;
                if (L_x > 600) L_x = 400;

                Items[id].visible = true;
                if ((Int32)myr["visible"] == 0) Items[id].visible = false;
                Items[id].showFrame = true;
                if ((Int32)myr["showFrame"] == 0) Items[id].showFrame = false;

                Items[id].fontSize = (Int32)((myr["fontSize"] == DBNull.Value) ? 0 : (Int32) myr["fontSize"] ) ;
                if (Items[id].fontSize < 2) Items[id].fontSize = 10;
                Items[id].objtype = (int)((myr["objtype"] == DBNull.Value) ? 0 : (int)myr["objtype"]);
                Items[id].Align01 = (int)((myr["Align01"] == DBNull.Value) ? 0 : (int)myr["Align01"]);
                Items[id].RectString = string.Concat(L_x.ToString(), " ", L_y.ToString(), " ", R_x.ToString(), " ", R_y.ToString());
                Items[id].r_width = R_x - L_x;
                Items[id].r_hight = R_y - L_y;
            }
        }
        conn.Close();
        return Items;
    }

    public reportItem[] Report_object_properties_stationery(int StatID)
    {
            string TheString = string.Empty;
            var Items = new reportItem[30];
            for (int i = 0; i < 20; i++)
            {
                Items[i] = new reportItem();
                Items[i].ObjID = i;
                Items[i].visible = true;
            switch (i)
            {
                case 21: TheString = "50 750 570 800"; break; //  ' header name 
                case 22: TheString = "50 700 570 750"; break; // ' header 
                case 23: TheString = "50 20 570 60"; break; //  ' buttom  
                case 24: TheString = "400 600 570 700"; break; // ' logo
                case 25: TheString = "70 630 370 720"; break; //  ' address
            }
                Items[i].RectString = TheString;
        }
        SqlConnection conn = new SqlConnection(conn_str);
        int L_x; int L_y; int R_x; int R_y;
        int id = 0;
        string mysql = "SELECT * FROM ac_companies_stationery_rep_obj WHERE CompID = @CompID AND StatID = @StatID ";
        SqlCommand comm = new SqlCommand(mysql, conn);
        comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
        comm.Parameters.Add("@StatID", SqlDbType.Int).Value = StatID;
        conn.Open();
        SqlDataReader myr = comm.ExecuteReader();
        while (myr.Read())
        {
            id = (Int32)myr["ObjID"];

            if (id < 10)
            {
                L_x = (Int32)myr["L_x"];
                L_y = (Int32)myr["L_y"];
                R_x = (Int32)myr["R_x"];
                R_y = (Int32)myr["R_y"];

                if (L_x < 0) L_x = 50;
                if (L_y < 0) L_y = 50;
                if (L_y > 800) L_y = 600;
                if (L_x > 600) L_x = 400;

                Items[id].visible = true;
                if ((Int32)myr["visible"] == 0) Items[id].visible = false;
                Items[id].showFrame = true;
                if ((Int32)myr["showFrame"] == 0) Items[id].showFrame = false;

                Items[id].fontSize = (Int32)myr["fontSize"];
                if (Items[id].fontSize < 2) Items[id].fontSize = 10;
                Items[id].objtype = (Int32)myr["objtype"];
                Items[id].RectString = string.Concat(L_x.ToString(), " ", L_y.ToString(), " ", R_x.ToString(), " ", R_y.ToString());
                Items[id].r_width = R_x - L_x;
                Items[id].r_hight = R_y - L_y;
            }
        }
        conn.Close();
        return Items;
    }

        public string ReportTextGet(string R_Type,int SellerID,string Country) {
            string repstr = String.Empty;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT RText FROM ac_companies_sellers_rep_txt WHERE CompID = @CompID AND SellerID = @SellerID AND CountryID = @CountryID AND RType = @RType ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = SellerID;
            comm.Parameters.Add("@CountryID", SqlDbType.NVarChar, 20).Value = Country;
            comm.Parameters.Add("@RType", SqlDbType.NVarChar, 20).Value = R_Type;
            conn.Open();
            var myr = comm.ExecuteReader();
            if (myr.Read())
            {
                repstr = myr["RText"].ToString();
            }
            conn.Close();
            return repstr;
         }


        public string html_head() {
            string myhead = "<head id='Head1'><title>Untitled Page</title><style type='text/css'>";
            myhead = String.Concat(myhead, " font.invoice {font: ", fontSize, "px '", fontFamily, "';text-decoration: none; color: #000000}");
            myhead = String.Concat(myhead, "</style><meta  content='text/html; charset=utf-8' /></head><body topmargin='0'  padding=0 leftmargin='0' rightmargin='0' border=none spacing=0 border=0> ");
            //'myhead = String.Concat(myhead, "</style><meta  content='text/html; /></head><body topmargin = '0'  padding=0 leftmargin='0' rightmargin='0' border=none spacing = 0 border=0> ")
            return myhead;
        }

    


        private string replace_inf(string instring, ref CompanyInf mycompany)
        {
            string Thestring = instring;
            string datestr = DateTime.Today.ToShortDateString();
            string timestr = DateTime.Now.ToShortTimeString();
            Thestring = Thestring.Replace("#d", datestr);
            Thestring = Thestring.Replace("#t", timestr);
            Thestring = Thestring.Replace("{today}", datestr);
            Thestring = Thestring.Replace("{now}", timestr);
            Thestring = Thestring.Replace("{CompName}", mycompany.CompanyName);
            Thestring = Thestring.Replace("{CVR}", mycompany.CompanyNo);
            //Thestring = Thestring.Replace("{EAN}", mycompany.EAN);
            Thestring = Thestring.Replace("{phone}", mycompany.CompanyPhone);
            Thestring = Thestring.Replace("{road}", String.Concat(mycompany.Street, " ", mycompany.HouseNumber, " ", mycompany.InHouseMail));
            Thestring = Thestring.Replace("{zip}", String.Concat(mycompany.PostalZone, " ", mycompany.CityName));
            // Thestring = Thestring.Replace("{account}", String.Concat( mycompanyp.BankRegNo, " ",  mycompany.AccountBank))
            // Thestring = Thestring.Replace("{BIC}",  mycompany.Bic);
            // Thestring = Thestring.Replace("{IBAN}", mycompany.IBAN);
            Thestring = Thestring.Replace("{email}", mycompany.Email);
            // Thestring = Thestring.Replace("{bankname}", mycompany.BankName);
            return Thestring;
        }

        private void get_company_inf()
        {
            string mylanguage = String.Empty;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = String.Concat("Select isnull(statID, isnull((SELECT min(StatID) FROM ac_companies_stationery Where CompID = @CompID),0)) As StatID, country , isnull(Language,'dan') as Language  from ac_companies where compid = @compID");
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            var myreader = comm.ExecuteReader();
            if (myreader.Read()) {
                mylanguage = myreader["Country"].ToString();
                CompLanguage = myreader["Language"].ToString();



            }
            conn.Close();
            if (mylanguage != String.Empty) Language = mylanguage;
            if (CompLanguage == String.Empty) CompLanguage = "dan";

       }



        private void get_company() {
            SqlConnection conn = new SqlConnection(conn_str);
            string compname = String.Empty;
            string Address = String.Empty;
            string mysql = "select CompanyName, Street,HouseNumber,InhouseMail,PostalZone,CityName,CompanyNo, PostalCodeCity,emailsender from ac_companies where compid = @CompID ";
            var comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            var myr = comm.ExecuteReader();
            if (myr.Read()) {
                compname = myr["CompanyName"].ToString();
                Address = String.Concat(myr["Street"].ToString(), " ", myr["HouseNumber"].ToString());
                if (myr["HouseNumber"].ToString() != String.Empty) Address = string.Concat(Address, " ", myr["InhouseMail"].ToString());
                Address = string.Concat(Address, ", ", myr["PostalZone"].ToString(), " ", myr["CityName"].ToString());
                //' CompanyNo = myr("CompanyNo").ToString()
                // ' emailFrom = myreader("emailSender").ToString()
            }
            conn.Close();
      

            string fileComp = compname;
            if (fileComp == String.Empty) fileComp = "Company";
            companyFileName = fileComp.Replace(" ", "_");
            companyFileName = companyFileName.Replace("�", "o");
            companyFileName = companyFileName.Replace( "�", "_");
            companyFileName = companyFileName.Replace( "�", "_");
            companyFileName = companyFileName.Replace( "�", "_");
            companyFileName = companyFileName.Replace( "�", "_");
            companyFileName = companyFileName.Replace( "�", "_");
            companyFileName = companyFileName.Replace( "&", "_");
            companyFileName = companyFileName.Replace( "%", "_");
            companyFileName = companyFileName.Replace( "#", "_");

       }




   


        private void set_paper_size(int PaperSize)
        {
            switch(PaperSize) {
                case 2:
                    paperWidth = 420;
                    paperHeight = 595;
                    break;
                case 3:
                    paperWidth = 595;
                    paperHeight = 420;
                    break;
                case 4:
                    paperWidth = 297;
                    paperHeight = 421;
                    break;
                case 5:
                    paperWidth = 420;
                    paperHeight = 297;
                    break;
                case 6:
                    paperWidth = 210;
                    paperHeight = 297;
                    break;
                case 7:
                    paperWidth = 297;
                    paperHeight = 210;
                    break;
                case 8:
                    paperWidth = 420;
                    paperHeight = 148;
                    break;
                case 9:
                    paperWidth = 297;
                    paperHeight = 105;
                    break;
                case 10:
                    paperWidth = 420;
                    paperHeight = 120;
                    break;
                default:
                    paperWidth = 595;
                    paperHeight = 842;
                    break;
            }
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


        public string get_stationery_footer(int pageno,int pages) {
            string theString;
            theString = ReportFooter;
            string datestr = DateTime.Today.ToShortDateString();
            string timestr = DateTime.Now.ToShortTimeString();
            theString = theString.Replace("#1", pageno.ToString());
            theString = theString.Replace("#2", pages.ToString());
            theString = theString.Replace("{ActP}", pageno.ToString());
            theString = theString.Replace("{TotP}", pages.ToString());
            theString = theString.Replace("#d", datestr);
            theString = theString.Replace("#t", timestr);
            int Pos = theString.IndexOf(">");
            if (Pos == 0) theString = theString.Replace(Convert.ToChar(13).ToString(), "<br>");
            if (StripTags(theString) == "") theString = "-";
            return theString;
        }


        private string StripTags(string html)  {
            //' Remove HTML tags.
              return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", "");
           }



    }

}

