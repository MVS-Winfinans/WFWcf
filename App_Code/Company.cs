using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
/// <summary>
/// Summary description for Company
/// </summary>
/// 
namespace wfws
{
    public class Company
    {
        int compID = 0;
        string ConnectionString;
       Guid CompanyKey = Guid.Empty;
       public Company(ref DBUser DBUser)
        {
            compID = DBUser.CompID;
            if (string.IsNullOrEmpty(DBUser.ConnectionString))
            {
                var wfconn = new wfws.ConnectLocal(DBUser);
                ConnectionString = wfconn.ConnectionGetByGuid_02(ref DBUser);
            }
            else
            {
                ConnectionString = DBUser.ConnectionString;
            }
            CompanyKey = DBUser.CompanyKey;
        }
        public Company(ref DBUser DBUser, string connstr)
        {
            compID = DBUser.CompID;
            ConnectionString = connstr;
            CompanyKey = DBUser.CompanyKey;
        }
        public string get_company_by_Guid(ref DBUser DBUser){
            string errstr  = "OK";
            int  newCompID = 0;
            try {
                if (DBUser.CompanyKey != Guid.Empty) {
                    SqlConnection Conn = new SqlConnection(ConnectionString);
                    string mysql = "SELECT isnull(CompID,0) FROM ac_companies WHERE CompanyGuid = @CompanyGuid";
                    SqlCommand Comm = new SqlCommand(mysql, Conn);
                    Comm.Parameters.Add("@CompanyGuid", SqlDbType.UniqueIdentifier).Value = CompanyKey;
                    Conn.Open();
                    newCompID = (Int32)Comm.ExecuteScalar();
                    Conn.Close();
                    if (newCompID > 0) { DBUser.CompID = newCompID; compID = newCompID; }
                }
            }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
   }
        public string Company_Load(ref DBUser DBUser,ref CompanyInf companyinf)
        {
            string errstr = "OK";
            wfws.LookUp Konv = new wfws.LookUp();
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string mysql = " SELECT *  FROM ac_Companies Where CompID = @CompID  ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    companyinf.CompanyName = myr["CompanyName"].ToString();
                    companyinf.Street = myr["Street"].ToString();
                    companyinf.HouseNumber = myr["HouseNumber"].ToString();
                    companyinf.CityName = myr["CityName"].ToString();
                    companyinf.PostalZone = myr["PostalZone"].ToString();
                    companyinf.Region = myr["Region"].ToString();
                    companyinf.InHouseMail = myr["InHouseMail"].ToString();
                    companyinf.CompanyNo = myr["CompanyNo"].ToString();
                    companyinf.TaxSchemeID = ((myr["TaxSchemeID"] == DBNull.Value) ? string.Concat(Konv.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, myr["Country"].ToString()), myr["CompanyNo"].ToString()) : myr["TaxSchemeID"].ToString());
                    companyinf.ean = myr["ean"].ToString().Replace(" ", "");
                    companyinf.endpointtype = myr["endpointtype"].ToString();
                    companyinf.Street = myr["Street"].ToString();
                    companyinf.CompanyPhone = myr["CompanyPhone"].ToString().Replace(" ", "");
                    companyinf.MobilePhone = myr["MobilPhone"].ToString().Replace(" ", "");
                    companyinf.Country = myr["Country"].ToString();
                    companyinf.AccMailClient = ((myr["AccMailClient"] == DBNull.Value) ? 0 : (Int32)myr["AccMailClient"]);
                    companyinf.saMailClient = ((myr["saMailClient"] == DBNull.Value) ? 0 : (Int32)myr["saMailClient"]);
                    companyinf.puMailClient = ((myr["puMailClient"] == DBNull.Value) ? 0 : (Int32)myr["puMailClient"]);
                    companyinf.GuidComp = ((myr["CompanyGuid"] == DBNull.Value) ? Guid.Empty : (Guid)myr["CompanyGuid"]);
                    companyinf.DimName1 = myr["Dim1"].ToString();
                    companyinf.DimName2 = myr["Dim2"].ToString();
                    companyinf.DimName3 = myr["Dim3"].ToString();
                    companyinf.DimName4 = myr["Dim4"].ToString();
                    companyinf.Email = myr["Email"].ToString();
                    companyinf.CompanyWeb = myr["WebPage"].ToString();
                    companyinf.AdditionalAccountID = myr["AdditionalAccountID"].ToString();
                }
                conn.Close();
            }
            catch (Exception e) { errstr = e.Message; }
            return errstr;
        }
   public int  Seller_Lookup(ref CompanySeller wfSeller, ref int wfcount) {
        int SellerID = 0;
         //Boolean wfLookUp  = false;
         wfcount = 0;
           if (wfSeller.SellerID > 0) {
                wfcount = 1;
                SellerID = wfSeller.SellerID;
           } else {
                  SqlConnection conn = new SqlConnection(ConnectionString);
                  string mysql = " SELECT isnull(max(SellerID),0) as SellerID, count(*) as wf_Count  FROM ac_Companies_Sellers Where CompID = @CompID  ";
                if (!string.IsNullOrEmpty(wfSeller.Description))
                {
                    mysql = string.Concat(mysql, " AND Description = @Description ");
                }
              SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@Description", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfSeller.Description)) ? "x" : wfSeller.Description);
               conn.Open();
                SqlDataReader myr  = comm.ExecuteReader();
                if  (myr.Read()) {
                    SellerID = ((myr["SellerID"] == DBNull.Value) ? 0 : (Int32)myr["SellerID"]);
                    wfcount = ((myr["wf_count"] == DBNull.Value) ? 0 : (Int32)myr["wf_count"]);
                }
                conn.Close();
            }
              wfSeller.SellerID = SellerID;
            return SellerID;
       }
    public int SellerNo2ID(string SellerNo) {
        int SellerID = 0;
        SqlConnection conn = new SqlConnection(ConnectionString);
        string mysql = " SELECT SellerID FROM ac_Companies_Sellers WHERE CompID = @CompID AND SellerNo = @SellerNo ";
        SqlCommand comm = new SqlCommand(mysql, conn);
        comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
        comm.Parameters.Add("@SellerNo", SqlDbType.NVarChar,20).Value = SellerNo;
        conn.Open();
        SqlDataReader myr  = comm.ExecuteReader();
        if (myr.Read())
            SellerID = ((myr["SellerID"] == DBNull.Value) ? 0 : (Int32)myr["SellerID"]);
        return SellerID;
    }
        public string Seller_Get(ref  CompanySeller wfSeller) {
            string  retstr  = "err";
            try {
               SqlConnection conn = new SqlConnection(ConnectionString);
               string mysql = " SELECT * FROM ac_Companies_Sellers WHERE CompID = @CompID AND SellerID = @SellerID ";
               SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = wfSeller.SellerID;
                conn.Open();
                SqlDataReader myr  = comm.ExecuteReader();
                if (myr.Read()) {
                    wfSeller.Description = myr["Description"].ToString();
                    wfSeller.SellerName = myr["SellerName"].ToString();
                    wfSeller.SellerEAN = myr["SellerEAN"].ToString().Replace(" ", "");
                    wfSeller.Description = myr["Description"].ToString();
                    wfSeller.SellerStreet = myr["SellerStreet"].ToString();
                    wfSeller.SellerHouseNumber = myr["SellerHouseNumber"].ToString();
                    wfSeller.SellerInHouseMail = myr["SellerInHouseMail"].ToString();
                    wfSeller.SellerCityName = myr["SellerCityName"].ToString();
                    wfSeller.SellerInHouseMail = myr["SellerInHouseMail"].ToString();
                    wfSeller.SellerPostalZone = myr["SellerPostalZone"].ToString();
                    wfSeller.SellerRegion = myr["SellerRegion"].ToString();
                    wfSeller.SellerCountryID = myr["SellerCountryID"].ToString();
                    wfSeller.BankName = myr["BankName"].ToString();
                    wfSeller.BankAddress = myr["BankAddress"].ToString();
                    wfSeller.RegistrationNo = myr["RegistrationNo"].ToString().Replace(" ", "");
                    wfSeller.AccountNo = myr["AccountNo"].ToString();
                    wfSeller.BIC = myr["BIC"].ToString();
                    wfSeller.IBAN = myr["IBAN"].ToString();
                    wfSeller.CompanyNo = myr["CompanyNo"].ToString().Replace(" ", "");
                    wfSeller.Email = myr["Email"].ToString();
                    wfSeller.SellerNo = myr["SellerNo"].ToString();
                    wfSeller.PaymentType = myr["PaymentType"].ToString();
                    wfSeller.CreditAccount = myr["CreditAccount"].ToString().Replace(" ", "");
                }
                conn.Close();
                retstr = "OK";
            }
        catch (NullReferenceException ex) { retstr = ex.Message; }
            return retstr;
       }
        public int is_company_user(string UserName, ref string retstr)
        {
            retstr = "OK";
            int UserCount = 0;
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string mysql = "select count(*) FROM ac_Users_Permissions where CompID = @CompID AND UserID = @UserName AND infCode = 'Company' AND exists (SELECT * FROM ac_users where UserID = @UserName)";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@UserName", SqlDbType.NVarChar, 20).Value = UserName;
                conn.Open();
                UserCount = (int)comm.ExecuteScalar();
                conn.Close();
                retstr = "OK";
            }
            catch (NullReferenceException ex) { retstr = ex.Message; }
            return UserCount;
        }
        public string Company_users_load( ref IList<CompanyUser> items)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(ConnectionString);
            var item = new CompanyUser();
            string mysql = "SELECT UserID,UserName,netUserid FROM ac_users WHERE isnull(Blocked,0) = 0 AND  Exists (SELECT * FROM ac_users_permissions Where ac_users_permissions.UserID = ac_users.userID AND InfCode = 'Company' AND CompID = @CompID)";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.UserID = myr["UserID"].ToString();
                item.UserName = myr["UserName"].ToString();
                items.Add(item);
                item = new CompanyUser();
            }
            conn.Close();
            return errStr;
        }
        public string Company_Salesmen_load(ref IList<CompanySalesman> items)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(ConnectionString);
            var item = new CompanySalesman();
            string mysql = "SELECT SalesmanID,Name,email FROM tr_sale_salesmen WHERE CompID = @CompID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SalesmanID = myr["SalesmanID"].ToString();
                item.SalesmanName = myr["Name"].ToString();
                item.email = myr["email"].ToString();
                items.Add(item);
                item = new CompanySalesman();
            }
            conn.Close();
            return errStr;
        }
        public Boolean Verify_Company(CompanyInf TestCompany)
        { 
            //loads company from current connection and compares selected fields with the TestCompany
            CompanyInf ThisCompany = new CompanyInf();
            DBUser NullUser = new DBUser();
            Boolean Answer = false;
            string err;
            err=Company_Load(ref NullUser, ref ThisCompany);
            if (ThisCompany.CompanyNo == TestCompany.CompanyNo) Answer = true;
            return Answer;
        }
        public string Company_load_Sellers(ref IList<int> items)
        {
            string errstr = "OK";
            SqlConnection conn = new SqlConnection(ConnectionString);
            int  item;
            string mysql = "SELECT SellerID FROM ac_Companies_Sellers WHERE CompID = @CompID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item = (int)myr["SellerID"];
                items.Add(item);
                item = new int();
            }
            conn.Close();
            return errstr;
        }
    }
}