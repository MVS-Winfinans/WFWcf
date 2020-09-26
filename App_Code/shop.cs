using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
/// <summary>
/// Summary description for shop
/// </summary>
/// 

namespace wfws {

    public class shop
    {
        int compID = 0;
        string ConnectionString;
        string ShopID;
        Guid CompanyKey = Guid.Empty;

       public shop(ref DBUser DBUser)
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
            ShopID = DBUser.ShopID;
        }

                public string load(ref ShopWf myshop) {

            string errstr = "OK";
            try {
                SqlConnection Conn = new SqlConnection(ConnectionString);
                string mysql = " SELECT * FROM ac_companies_shop WHERE CompID = @CompID AND ShopID = @ShopID ";
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompanyGuid", SqlDbType.UniqueIdentifier).Value = CompanyKey;
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@ShopID", SqlDbType.NVarChar, 20).Value = ShopID;
                Conn.Open();
                SqlDataReader myr  = Comm.ExecuteReader();
                if  (myr.Read()) {
                    myshop.ShopID = myr["ShopID"].ToString();
                    myshop.ShopDescription = myr["ShopDescription"].ToString();
                    myshop.Fee1 = myr["Fee1"].ToString();
                    myshop.Fee2 = myr["Fee2"].ToString();
                    myshop.Pricelist_1 = myr["Pricelist_1"].ToString();
                    myshop.Pricelist_2 = myr["Pricelist_2"].ToString();
                    myshop.Category = (Int32) ((myr["Category"] == DBNull.Value) ? 0 : (object) myr["Category"]);
                    myshop.Category_np =  (Int32) ((myr["Category_np"] == DBNull.Value) ? 0 : (object) myr["Category_np"]);
                    myshop.SellerID = (Int32) ((myr["SellerID"] == DBNull.Value) ? 0 : myr["SellerID"]);
                    myshop.SellerID_np = (Int32) ((myr["SellerID_np"] == DBNull.Value) ? 0 : myr["SellerID_np"]);
                    myshop.PaymentSolution = (Int32) ((myr["PaymentSolution"] == DBNull.Value) ? 1 : myr["PaymentSolution"]);
                    myshop.b2blogin = (Boolean) ((myr["b2blogin"] == DBNull.Value) ? false : myr["b2blogin"]);
                    myshop.ProductShortTextUse = (Int32) ((myr["ProductShortTextUse"] == DBNull.Value) ? 1 : myr["ProductShortTextUse"]);
            }
                Conn.Close();
            }
                  catch (Exception e) { errstr = e.Message; }
           return errstr;
        }




        public string get_text(string TextKey, string CountryID,ref string header,ref string body)
         {
            string errstr = "OK";
            try {
                 SqlConnection Conn = new SqlConnection(ConnectionString);
     
                string mysql = "SELECT Headertext, ShopText FROM dbo.ac_companies_shop_text_header tb1 inner join ac_companies_shop_text tb2  ";
                mysql = String.Concat(mysql, " on tb1.CompID = tb2.CompID AND tb1.ShopID = tb2.ShopID AND tb1.tid = tb2.Tid ");
                mysql = String.Concat(mysql, " WHERE tb2.CompID = @CompID AND tb2.ShopID = @ShopID AND tb2.CountryID = @CountryID AND TextKey = @TextKey ");
                 SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@ShopID", SqlDbType.NVarChar, 20).Value = ShopID;
                Comm.Parameters.Add("@CountryID", SqlDbType.NVarChar, 20).Value =  (String.IsNullOrEmpty(CountryID) ? "DK" : CountryID);
                Comm.Parameters.Add("@TextKey", SqlDbType.NVarChar, 20).Value = TextKey;
                Conn.Open();
                 SqlDataReader myr  = Comm.ExecuteReader();
                if (myr.Read()) {
                    header = myr["Headertext"].ToString();
                    body = myr["ShopText"].ToString();
                }
                Conn.Close();
        }
             catch (Exception e) { errstr = e.Message; }
           return errstr;
        }



  public string Shop_Items_get_selection(ref IList<inventoryItem> items, string SelectionID) 
  {
            string errstr = "OK";
            try
            {
                SqlConnection Conn = new SqlConnection(ConnectionString);
                var item = new inventoryItem();
                SqlCommand Comm = new SqlCommand("wf_web_shop_Selections_products_02", Conn);
                Comm.CommandType = CommandType.StoredProcedure;
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@ShopID", SqlDbType.NVarChar, 20).Value = ShopID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = 0;
                Comm.Parameters.Add("@SelectionID", SqlDbType.NVarChar, 20).Value = (String.IsNullOrEmpty(SelectionID) ? (object)DBNull.Value : SelectionID);
                Conn.Open();
                SqlDataReader myr = Comm.ExecuteReader();

                while (myr.Read())
                {
                    item.ItemID = myr["ItemID"].ToString();
                    item.ItemDesc = myr["Description"].ToString();
                    item.unit = myr["Unit"].ToString();
                    item.SalesPrice = (decimal) ((myr["SalesPrice"] == DBNull.Value) ? 0 : myr["SalesPrice"]);
                    item.volume = (decimal)((myr["volume"] == DBNull.Value) ? 0 : myr["volume"]);
                    items.Add(item);
                    item = new inventoryItem();
                }
                Conn.Close();
                errstr = items.Count.ToString();
            }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
         }

    }
}