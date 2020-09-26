using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Summary description for Basket
/// </summary>
/// 
namespace wfws {
public class Basket
{

    int CompID = 0;
    string ConnectionString;
    string ShopID;
    Guid CompanyKey = Guid.Empty;

    int BasketID = 0;

    private Guid BasketGuid = Guid.Empty;

	public Basket(ref DBUser DBUser)
	{
            CompID = DBUser.CompID;
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
            if (DBUser.BasketGuid != Guid.Empty) {
                BasketGuid = DBUser.BasketGuid;
                BasketID_byGuid();
                BasketID_exists();
            }
            if (BasketID == 0)
            {
                CreateDefaultBasket();
                DBUser.BasketGuid = BasketGuid;
            }
	}


    private void CreateDefaultBasket()
          {
            if (BasketID == 0) 
            {
                SqlConnection Conn = new SqlConnection(ConnectionString);
                SqlCommand Comm = new SqlCommand("wf_web_shop_basket_defaults_02", Conn);
                Comm.CommandType = CommandType.StoredProcedure;
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketGuid", SqlDbType.UniqueIdentifier).Value = BasketGuid;
                Comm.Parameters.Add("@ShopID", SqlDbType.NVarChar, 20).Value = ShopID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Direction = ParameterDirection.Output;
                Conn.Open();
                Comm.ExecuteNonQuery();
                BasketID = (Int32) Comm.Parameters["@BasketID"].Value;
                Conn.Close();
                basket_Update();
              
            }
        }


        public string AddressesToWf()
        {
            string errstr  = "OK";
            try {
                if (BasketID > 0) {
                    SqlConnection Conn = new SqlConnection(ConnectionString);
                    SqlCommand Comm = new SqlCommand("wf_web_shop_Adr_ToWf_02", Conn);
                    Comm.CommandType = CommandType.StoredProcedure;
                    Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                    Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                    Conn.Open();
                    Comm.ExecuteNonQuery();
                    Conn.Close();
                }
            }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
        }



        private void basket_Update()
        {
             if (BasketID > 0) {
                string mysql = "SELECT Basketguid FROM web_basket WHERE CompID = @CompID AND BasketID = @BasketID";
                SqlConnection Conn = new SqlConnection(ConnectionString);
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                Conn.Open();
                BasketGuid = (Guid) Comm.ExecuteScalar();
                Conn.Close();
            }
        }


        private void BasketID_exists()
        {
            int count;
             if (BasketID > 0) {
                string mysql = "SELECT count(*) FROM web_basket WHERE CompID = @CompID AND BasketID = @BasketID";
                SqlConnection Conn = new SqlConnection(ConnectionString);
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                Conn.Open();
                count = (Int32) Comm.ExecuteScalar();
                Conn.Close();
                if (count == 0) BasketID = 0;
            }
        }

        private void BasketID_byGuid()
        {
            if ((BasketID == 0) && (BasketGuid != Guid.Empty))
            {
               string mysql= "SELECT isnull(max(BasketID),0) FROM web_basket WHERE CompID = @CompID AND basketGuid = @BasketGuid";
                SqlConnection Conn = new SqlConnection(ConnectionString);
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketGuid", SqlDbType.UniqueIdentifier).Value = BasketGuid;
                Conn.Open();
                BasketID = (Int32) Comm.ExecuteScalar();
                Conn.Close();
            }
        }


        public string  load(ref ShopBasket myba) {
            string errstr = "OK";
            try {
                if ((CompID > 0) && (BasketID > 0)) {
                   string mysql= "SELECT * FROM web_basket WHERE CompID = @CompID AND BasketID = @BasketID";
                    SqlConnection Conn = new SqlConnection(ConnectionString);
                    SqlCommand Comm = new SqlCommand(mysql, Conn);
                    Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                    Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                    myba.BasketID = BasketID;
                    Conn.Open();
                     SqlDataReader myr  = Comm.ExecuteReader();
                      if  (myr.Read()) {
                        myba.BasketGuid = (Guid) myr["BasketGuid"];
                        myba.BillTo.AddressID = (Int32) ((myr["AddressID"] == DBNull.Value) ? 0 : myr["AddressID"]);
                        myba.BillTo.email = myr["eMail"].ToString();
                        myba.BillTo.CompanyName = myr["Name_1"].ToString();
                        myba.BillTo.Department = myr["Name_2"].ToString();
                        myba.BillTo.Address1 = myr["Address_1"].ToString();
                        myba.BillTo.Address2 = myr["Address_2"].ToString();
                        myba.BillTo.CountryID = myr["country"].ToString();
                        myba.BillTo.Region = myr["region"].ToString();
                        myba.BillTo.PostalCode = myr["PostalCode"].ToString();
                        myba.BillTo.City = myr["City"].ToString();
                        myba.BillTo.HouseNumber = myr["HouseNumber"].ToString();
                        myba.BillTo.InHouseMail = myr["InHouseMail"].ToString();
                        myba.ShipTo.AddressID = (Int32) ((myr["sh_AddressID"] == DBNull.Value) ?  0 : myr["sh_AddressID"]);
                        if (myba.ShipTo.AddressID > 0) {
                            myba.ShipTo.CompanyName = myr["sh_Name_1"].ToString();
                            myba.ShipTo.Department = myr["sh_Name_2"].ToString();
                            myba.ShipTo.Address1 = myr["sh_Address_1"].ToString();
                            myba.ShipTo.Address2 = myr["sh_Address_2"].ToString();
                            myba.ShipTo.CountryID = myr["sh_country"].ToString();
                            myba.ShipTo.Region = myr["sh_region"].ToString();
                            myba.ShipTo.PostalCode = myr["sh_PostalCode"].ToString();
                            myba.ShipTo.City = myr["sh_City"].ToString();
                            myba.ShipTo.HouseNumber = myr["sh_HouseNumber"].ToString();
                            myba.ShipTo.InHouseMail = myr["sh_InHouseMail"].ToString();
                        }
                        myba.ContID = (Int32) ((myr["ContID"] == DBNull.Value) ? 0 : myr["ContID"]);
                        myba.SaleID = (Int32) ((myr["SaleID"] == DBNull.Value) ? 0 : myr["SaleID"]);
                        myba.Salesman = myr["Salesman"].ToString();

                        myba.text_1 = myr["text_1"].ToString();
                        myba.text_2 = myr["text_2"].ToString();
                        myba.text_3 = myr["text_3"].ToString();

                        myba.InvoiceDate = (DateTime)myr["InvoiceDate"];
                        myba.ShipDate = (DateTime)myr["ShipDate"];
                        myba.requisition = myr["requisition"].ToString();
                        myba.def_addSalesAs = (Int32)((myr["def_addSalesAs"] == DBNull.Value) ? 0 : myr["def_addSalesAs"]);
                     }
                    Conn.Close();
                    myba.Contactname = get_contact(myba.ContID);
                    //  load_Items(myba, errstr)

                }
                  }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
        }


     public string Confirm()
     {
             string errstr = "OK";
            try {
                 if (BasketID > 0) {
                    SqlConnection Conn = new SqlConnection(ConnectionString);
                    SqlCommand Comm = new SqlCommand("wf_web_shop_basketConfirm_02", Conn);
                    Comm.CommandType = CommandType.StoredProcedure;
                    Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                    Comm.Parameters.Add("@ShopID", SqlDbType.NVarChar, 50).Value = ShopID;
                    Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                    Conn.Open();
                    Comm.ExecuteNonQuery();
                    Conn.Close();
                }

                   }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
        }




        private string get_contact(int ContID )
{
            string ContactName = string.Empty;
            if (ContID > 0) {
                SqlConnection Conn = new SqlConnection(ConnectionString);
                string mysql = "SELECT isnull(ContactName,'-') FROM ad_contacts WHERE CompID = @CompID AND ContID = @ContID";
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@ContID", SqlDbType.Int).Value = ContID;
                Conn.Open();
                ContactName = Comm.ExecuteScalar().ToString();
                Conn.Close();
            }
            return ContactName;
        }



        public string save(ref ShopBasket myba,ref string errstr) 
           {
               DateTime minSqlDate = new DateTime(1753, 1, 1);
                if ((CompID > 0) && (BasketID > 0)) {

               string mysql= "Update web_basket set ";
                mysql = string.Concat(mysql, "AddressID = @AddressID, eMail = @eMail, ");
                mysql = string.Concat(mysql, "Name_1  = @Name_1 , Name_2 = @Name_2, Address_1 = @Address_1, Address_2 = @Address_2, country = @country , region = @region, PostalCode = @PostalCode, City = @City , HouseNumber = @HouseNumber, InHouseMail = @InHouseMail, ");
                mysql = string.Concat(mysql, " sh_addressID = @sh_addressID,sh_name_1  = @sh_name_1 , sh_name_2 = @sh_name_2, sh_address_1 = @sh_address_1, sh_address_2 = @sh_address_2, sh_country = @sh_country , sh_region = @sh_region, sh_postalCode = @sh_postalCode, sh_city = @sh_city , sh_houseNumber = @sh_houseNumber, sh_InHouseMail = @sh_InHouseMail, ");
                mysql = string.Concat(mysql, " Salesman = @Salesman, text_1 = @text_1, text_2 = @text_2, text_3 = @text_3 , InvoiceDate = @InvoiceDate, ShipDate = @ShipDate, requisition = @requisition, def_addSalesAs= @def_addSalesAs  ");
                mysql = string.Concat(mysql, " WHERE CompID = @CompID AND BasketID = @BasketID");

                SqlConnection Conn = new SqlConnection(ConnectionString);
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;

                Comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = myba.BillTo.AddressID;
                Comm.Parameters.Add("@eMail", SqlDbType.NVarChar, 255).Value = (string.IsNullOrEmpty(myba.BillTo.email) ? (object) DBNull.Value : myba.BillTo.email);
                Comm.Parameters.Add("@Name_1", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myba.BillTo.CompanyName) ? (object) DBNull.Value : myba.BillTo.CompanyName);
                Comm.Parameters.Add("@Name_2", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myba.BillTo.Department) ? (object) DBNull.Value : myba.BillTo.Department);
                Comm.Parameters.Add("@Address_1", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myba.BillTo.Address1) ? (object) DBNull.Value : myba.BillTo.Address1);
                Comm.Parameters.Add("@Address_2", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myba.BillTo.Address2) ? (object) DBNull.Value : myba.BillTo.Address2);
                Comm.Parameters.Add("@country", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myba.BillTo.CountryID) ? (object) DBNull.Value : myba.BillTo.CountryID);
                Comm.Parameters.Add("@region", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myba.BillTo.Region) ? (object) DBNull.Value : myba.BillTo.Region);
                Comm.Parameters.Add("@PostalCode", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myba.BillTo.PostalCode) ? (object) DBNull.Value : myba.BillTo.PostalCode);
                Comm.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myba.BillTo.City) ? (object) DBNull.Value : myba.BillTo.City);
                Comm.Parameters.Add("@HouseNumber", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myba.BillTo.HouseNumber) ? (object) DBNull.Value : myba.BillTo.HouseNumber);
                Comm.Parameters.Add("@InHouseMail", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myba.BillTo.InHouseMail) ? (object) DBNull.Value : myba.BillTo.InHouseMail);
                Comm.Parameters.Add("@sh_addressID", SqlDbType.Int).Value = myba.ShipTo.AddressID;
                Comm.Parameters.Add("@sh_eMail", SqlDbType.NVarChar, 255).Value = (string.IsNullOrEmpty(myba.ShipTo.email) ? (object) DBNull.Value : myba.ShipTo.email);
                Comm.Parameters.Add("@sh_name_1", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myba.ShipTo.CompanyName) ? (object) DBNull.Value : myba.ShipTo.CompanyName);
                Comm.Parameters.Add("@sh_name_2", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myba.ShipTo.Department) ? (object) DBNull.Value : myba.ShipTo.Department);
                Comm.Parameters.Add("@sh_address_1", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myba.ShipTo.Address1) ? (object) DBNull.Value : myba.ShipTo.Address1);
                Comm.Parameters.Add("@sh_address_2", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myba.ShipTo.Address2) ? (object) DBNull.Value : myba.ShipTo.Address2);
                Comm.Parameters.Add("@sh_country", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myba.ShipTo.CountryID) ? (object) DBNull.Value : myba.ShipTo.CountryID);
                Comm.Parameters.Add("@sh_region", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myba.ShipTo.Region) ? (object) DBNull.Value : myba.ShipTo.Region);
                Comm.Parameters.Add("@sh_postalCode", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myba.ShipTo.PostalCode) ? (object) DBNull.Value : myba.ShipTo.PostalCode);
                Comm.Parameters.Add("@sh_city", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myba.ShipTo.City) ? (object) DBNull.Value : myba.ShipTo.City);
                Comm.Parameters.Add("@sh_houseNumber", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myba.ShipTo.HouseNumber) ? (object) DBNull.Value : myba.ShipTo.HouseNumber);
                Comm.Parameters.Add("@sh_inHouseMail", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myba.ShipTo.InHouseMail) ? (object) DBNull.Value : myba.ShipTo.InHouseMail);
                Comm.Parameters.Add("@Salesman", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(myba.Salesman) ? (object)DBNull.Value : myba.Salesman);
                Comm.Parameters.Add("@text_1", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(myba.text_1) ? (object)DBNull.Value : myba.text_1);
                Comm.Parameters.Add("@text_2", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(myba.text_2) ? (object)DBNull.Value : myba.text_2);
                Comm.Parameters.Add("@text_3", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(myba.text_3) ? (object)DBNull.Value : myba.text_3);
                Comm.Parameters.Add("@InvoiceDate", SqlDbType.DateTime).Value =  ((myba.InvoiceDate < minSqlDate) ? DBNull.Value : (object)myba.InvoiceDate);
                Comm.Parameters.Add("@ShipDate", SqlDbType.DateTime).Value = ((myba.ShipDate < minSqlDate) ? DBNull.Value : (object)myba.ShipDate);
                Comm.Parameters.Add("@requisition", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myba.requisition) ? (object)DBNull.Value : myba.requisition);
                Comm.Parameters.Add("@def_addSalesAs", SqlDbType.Int).Value = myba.def_addSalesAs;
                Conn.Open();
                Comm.ExecuteNonQuery();
                Conn.Close();
            }
            return errstr;
        }


        public string AddBasketItem(ref ShopBasketItem mybasket){
            string errstr = "OK";
            try {
                if ((mybasket.ItemID != String.Empty) && (BasketID > 0)) {
                    SqlConnection Conn = new SqlConnection(ConnectionString);
                    string mysql = "INSERT web_Basket_sub (CompID,BasketID,ItemID,Unit,ItemDescription, Style, Qty,FixedQty,Salesprice,confirmed,ext01,ext02,ext03,ext04,ext05,ext06) values (@CompID, @BasketID,@ItemID , @Unit, @Desc,@Style, @qty, @Fixedqty,@Salesprice,0,@ext01,@ext02,@ext03,@ext04,@ext05,@ext06)";
                    SqlCommand Comm = new SqlCommand(mysql, Conn);
                    Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                    Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                    Comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = mybasket.ItemID;
                    Comm.Parameters.Add("@Unit", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mybasket.unit) ? (object) DBNull.Value : mybasket.unit);
                    Comm.Parameters.Add("@qty", SqlDbType.Money).Value = mybasket.Qty;
                    Comm.Parameters.Add("@Fixedqty", SqlDbType.Money).Value = mybasket.FixedQty;
                    Comm.Parameters.Add("@Salesprice", SqlDbType.Money).Value = mybasket.SalesPrice;
                    Comm.Parameters.Add("@Desc", SqlDbType.NVarChar, 255).Value = (string.IsNullOrEmpty(mybasket.ItemDesc) ? (object) DBNull.Value : mybasket.ItemDesc);
                    Comm.Parameters.Add("@Style", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mybasket.Style) ? (object) DBNull.Value : mybasket.Style);
                    Comm.Parameters.Add("@ext01", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybasket.ext01) ? (object) DBNull.Value : mybasket.ext01);
                    Comm.Parameters.Add("@ext02", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybasket.ext02) ? (object) DBNull.Value : mybasket.ext02);
                    Comm.Parameters.Add("@ext03", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybasket.ext03) ? (object) DBNull.Value : mybasket.ext03);
                    Comm.Parameters.Add("@ext04", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybasket.ext04) ? (object) DBNull.Value : mybasket.ext04);
                    Comm.Parameters.Add("@ext05", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybasket.ext05) ? (object) DBNull.Value : mybasket.ext05);
                    Comm.Parameters.Add("@ext06", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybasket.ext06) ? (object) DBNull.Value : mybasket.ext06);
                    Conn.Open();
                    Comm.ExecuteNonQuery();
                    Conn.Close();
                }

                    }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
        }


        public void load_Items(ref IList<ShopBasketItem> items,ref string errstr) {
            int count  = 0;


            SqlConnection Conn = new SqlConnection(ConnectionString);
           string mysql= "SELECT LiiD, ItemID,Unit,ItemDescription,Qty,FixedQty,SalesPrice,Qty * SalesPrice as amount, Style, ext01,ext02,ext03,ext04,ext05, ext06 FROM web_basket_sub WHERE CompID = @CompID AND BasketID = @BasketID  AND Confirmed = 0 ";
            try {
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                var item = new ShopBasketItem();
                Conn.Open();
                 SqlDataReader myr  = Comm.ExecuteReader();
                count = 10;
                while (myr.Read()) {
                    item.Liid = (Int32) myr["Liid"];
                    item.ItemID = myr["ItemID"].ToString();
                    item.unit = myr["Unit"].ToString();
                    item.ItemDesc = myr["ItemDescription"].ToString();
                    item.Qty = (Decimal) myr["Qty"];
                    item.FixedQty = (Decimal) myr["FixedQty"];
                    item.SalesPrice = (Decimal) myr["Salesprice"];
                    item.Amount = (Decimal) myr["Amount"];
                    item.Style = myr["Style"].ToString();
                    item.ext01 = myr["Ext01"].ToString();
                    item.ext02 = myr["Ext02"].ToString();
                    item.ext03 = myr["Ext03"].ToString();
                    item.ext04 = myr["Ext04"].ToString();
                    item.ext05 = myr["Ext05"].ToString();
                    item.ext06 = myr["Ext06"].ToString();

                    items.Add(item);
                    item = new ShopBasketItem();
                    count = count + 1;
                }
                   }
            catch (Exception e) { errstr = e.Message; }
          }




        public string basket_total(ref ShopBasket myba){
            string errstr = "OK";
            SqlConnection Conn = new SqlConnection(ConnectionString);
            myba.Total = 0;
            try {
               string mysql= "SELECT isnull(sum(Qty * SalesPrice),0) as amount FROM web_basket_sub WHERE CompID = @CompID AND BasketID = @BasketID AND Confirmed = 0 ";
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                Conn.Open();
                myba.Total = (Decimal) Comm.ExecuteScalar();
                Conn.Close();
                    }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
        }


        public string basket_Items_delete_lines(ref ShopBasket myba ) {
            string errstr = "OK";
            SqlConnection Conn = new SqlConnection(ConnectionString);
            myba.Total = 0;
            try {
               string mysql= "DELETE FROM web_basket_sub WHERE CompID = @CompID AND BasketID = @BasketID AND  Confirmed = 0 AND qty = 0  ";
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                Conn.Open();
                Comm.ExecuteNonQuery();
                Conn.Close();
            }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
        }

        public string basket_item_update(ref ShopBasketItem mybaitem) {
            string errstr = "OK";
            SqlConnection Conn = new SqlConnection(ConnectionString);
             try {
                 string mysql = "UPDATE web_basket_sub Set Qty = @Qty,SalesPrice = @SalesPrice,ItemDescription = @ItemDesc, Ext05 = @Ext05 , Ext06 = @Ext06  WHERE CompID = @CompID AND BasketID = @BasketID AND LiID = @LiID ";
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = CompID;
                Comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = BasketID;
                Comm.Parameters.Add("@Liid", SqlDbType.Int).Value = mybaitem.Liid;
                Comm.Parameters.Add("@QTY", SqlDbType.Decimal).Value = mybaitem.Qty;
                Comm.Parameters.Add("@SalesPrice", SqlDbType.Decimal).Value = mybaitem.SalesPrice;
                Comm.Parameters.Add("@ItemDesc", SqlDbType.NVarChar,255).Value = (string.IsNullOrEmpty(mybaitem.ItemDesc) ? (object) DBNull.Value : mybaitem.ItemDesc);
                Comm.Parameters.Add("@Ext01", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybaitem.ext01) ? (object) DBNull.Value : mybaitem.ext01);
                Comm.Parameters.Add("@Ext02", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybaitem.ext02) ? (object) DBNull.Value : mybaitem.ext02);
                Comm.Parameters.Add("@Ext03", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybaitem.ext03) ? (object) DBNull.Value : mybaitem.ext03);
                Comm.Parameters.Add("@Ext04", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybaitem.ext04) ? (object) DBNull.Value : mybaitem.ext04);
                Comm.Parameters.Add("@Ext05", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybaitem.ext05) ? (object) DBNull.Value : mybaitem.ext05);
                Comm.Parameters.Add("@Ext06", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mybaitem.ext06) ? (object) DBNull.Value : mybaitem.ext06);
                Conn.Open();
                Comm.ExecuteNonQuery();
                Conn.Close();
                     }
            catch (Exception e) { errstr = e.Message; }
           return errstr;
        }




}
}