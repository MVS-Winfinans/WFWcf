using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Activities.Expressions;

/// <summary>
/// Implementing part of web-class related to inventory
/// </summary>
/// 
namespace wfws
{
    public partial class web
    {
        //  ' INVENTORY

        public string Inventory_Items_get_Changed(ref string ItemID, ref IList<string> items, ref DateTime TimeChanged)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string item = string.Empty;
            string mysql = "Select top 10 ItemID FROM tr_inventory where CompID = @CompID AND (ItemID > @ItemID OR @ItemID is null) AND TimeChanged >= @TimeChanged AND TimeChanged is not null ORDER by ItemID ";

            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(ItemID) ? DBNull.Value : (object)ItemID);
            comm.Parameters.Add("@TimeChanged", SqlDbType.DateTime).Value = TimeChanged;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                string mystr = myr["ItemID"].ToString();
                items.Add(mystr);
                ItemID = myr["ItemID"].ToString();
            }
            conn.Close();
            return "OK";
        }

        public string Inventory_Items_get(ref IList<inventoryItem> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            inventoryItem item = new inventoryItem();
            string mysql = "Select ItemID, Description,isnull(SalesPrice,0) as SalesPrice, 0 as qty,GroupFi,Unit FROM tr_inventory  ";
            mysql = string.Concat(mysql, " WHERE CompID = @CompID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.ItemID = myr["ItemID"].ToString();
                item.ItemDesc = myr["Description"].ToString();
                item.SalesPrice = (Decimal)myr["SalesPrice"];
                item.GroupFi = myr["GroupFI"].ToString();
                item.unit = myr["Unit"].ToString();
                items.Add(item);
                item = new inventoryItem();
            }
            conn.Close();
            return "OK";
        }


        public string Inventory_GroupsFi_get(ref IList<inventoryGroupFi> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            inventoryGroupFi item = new inventoryGroupFi();
            string mysql = "Select GroupFi, Description,isnull(AutoReorder,0) as AutoReorder FROM tr_inventory_GroupsFi  ";
            mysql = string.Concat(mysql, " WHERE CompID = @CompID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.GroupFi = myr["GroupFi"].ToString();
                item.GroupDesc = myr["Description"].ToString();
                item.AutoReorder = (Boolean)myr["AutoReorder"];
                items.Add(item);
                item = new inventoryGroupFi();
            }
            conn.Close();
            return "OK";
        }


        public string Inventory_Locations_get(ref IList<inventoryLocation> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            inventoryLocation item = new inventoryLocation();
            string mysql = "Select LocationID, Description FROM tr_inventory_Locations  ";
            mysql = string.Concat(mysql, " WHERE CompID = @CompID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.LocationID = myr["LocationID"].ToString();
                item.Description = myr["Description"].ToString();
                items.Add(item);
                item = new inventoryLocation();
            }
            conn.Close();
            return "OK";
        }

        public string Inventory_Selections_get(ref IList<inventorySelection> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            inventorySelection item = new inventorySelection();
            string mysql = "Select SelectionID, Selection, Note, ParentID, isnull(UseAsParent,0) as UseAsParent  FROM tr_inventory_selections  ";
            mysql = string.Concat(mysql, " WHERE CompID = @CompID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SelectionID = myr["SelectionID"].ToString();
                item.Selection = myr["Selection"].ToString();
                item.Note = myr["Note"].ToString();
                item.ParentID = myr["ParentID"].ToString();
                item.UseAsParent = (((int)myr["UseAsParent"] == 0) ? false : true);

                items.Add(item);
                item = new inventorySelection();
            }
            conn.Close();
            return "OK";
        }


        public string Inventory_Items_get_selection(ref IList<inventoryItem> items, string SelectionID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            inventoryItem item = new inventoryItem();
            string mysql = "Select tb1.ItemID, tb1.Description,  isnull(SalesPrice,0) as SalesPrice, ";
            mysql = String.Concat(mysql, " volume, 0 as qty,isnull(Weight,0) as Weight, Unit,DateCreate,DateUpdate, DateActive,DateInactive ");
            mysql = String.Concat(mysql, " FROM tr_inventory tb1 inner join  tr_inventory_selections_items tb2 on tb1.compID = tb2.CompID AND tb1.ItemID = tb2.ItemID  ");
            mysql = String.Concat(mysql, " WHERE tb2.CompID = @CompID AND tb2.SelectionID = @SelectionID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SelectionID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(SelectionID) ? DBNull.Value : (object)SelectionID);

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.ItemID = myr["ItemID"].ToString();
                item.ItemDesc = myr["Description"].ToString();
                item.unit = myr["Unit"].ToString();
                item.SalesPrice = ((myr["SalesPrice"] == DBNull.Value) ? 0 : (Decimal)myr["SalesPrice"]);
                item.volume = ((myr["volume"] == DBNull.Value) ? 0 : (Decimal)myr["volume"]);
                item.Weight = ((myr["Weight"] == DBNull.Value) ? 0 : (Decimal)myr["Weight"]);
                item.DateCreate = (DateTime)((myr["DateCreate"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["DateCreate"]);
                item.DateUpdate = (DateTime)((myr["DateUpdate"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["DateUpdate"]);
                item.DateActive = (DateTime)((myr["DateActive"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["DateActive"]);
                item.DateInactive = (DateTime)((myr["DateInactive"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["DateInactive"]);

                //item.volume = ((myr["volume"] == DBNull.Value) ? 0 : (Int32) myr["volume"]);
                items.Add(item);
                item = new inventoryItem();
            }
            conn.Close();
            return "OK";
        }




        public string Inventory_Items_get_List_GroupFI(ref IList<inventoryItem> items, string GroupFI)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            inventoryItem item = new inventoryItem();
            string mysql = "Select ItemID, Description,  isnull(SalesPrice,0) as SalesPrice, ";
            mysql = String.Concat(mysql, " volume, 0 as qty,isnull(Weight,0) as Weight, Unit,DateCreate,DateUpdate, DateActive,DateInactive, GroupFi ");
            mysql = String.Concat(mysql, " FROM tr_inventory  ");
            mysql = String.Concat(mysql, " WHERE CompID = @CompID AND GroupFI = @GroupFi ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@GroupFI", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(GroupFI) ? DBNull.Value : (object)GroupFI);

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.ItemID = myr["ItemID"].ToString();
                item.ItemDesc = myr["Description"].ToString();
                item.unit = myr["Unit"].ToString();
                item.GroupFi = myr["GroupFi"].ToString();
                item.SalesPrice = ((myr["SalesPrice"] == DBNull.Value) ? 0 : (Decimal)myr["SalesPrice"]);
                item.volume = ((myr["volume"] == DBNull.Value) ? 0 : (Decimal)myr["volume"]);
                item.Weight = ((myr["Weight"] == DBNull.Value) ? 0 : (Decimal)myr["Weight"]);
                item.DateCreate = (DateTime)((myr["DateCreate"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["DateCreate"]);
                item.DateUpdate = (DateTime)((myr["DateUpdate"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["DateUpdate"]);
                item.DateActive = (DateTime)((myr["DateActive"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["DateActive"]);
                item.DateInactive = (DateTime)((myr["DateInactive"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["DateInactive"]);

                //item.volume = ((myr["volume"] == DBNull.Value) ? 0 : (Int32) myr["volume"]);
                items.Add(item);
                item = new inventoryItem();
            }
            conn.Close();
            return "OK";
        }





        public string Inventory_Item_get(ref inventoryItem item)
        {   //Will look up one specifik item - since criterias a the primary key. For broader search use Inventory_Item_Lookup()
            SqlConnection conn = new SqlConnection(conn_str);
            string errStr = "err";
            string itemid = item.ItemID;
            if (itemid != String.Empty)
            {
                string mysql = "Select ItemID, Description,EAN,Unit,note,Model,GroupFi,BrowserTitle,MetaData,URL,MetaText,IncludeItemID,SuppliersInvNo,UNSPSC,isnull(NotOnWeb,0) as NotOnWeb, isnull(NotOnMyPage,0) as NotOnMyPage,Position, ";
                mysql = String.Concat(mysql, " isnull(SalesPrice,0) as SalesPrice,CostPriceFixed, isnull(volume,0) as volume, isnull(Weight,0) as Weight, dbo.tr_inventory_onstock(CompID ,ItemID, null , Class,  null,  null) as qty FROM tr_inventory ");
                mysql = String.Concat(mysql, " WHERE CompID = @CompID AND ItemID = @ItemID ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = itemid;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    item.ItemID = myr["ItemID"].ToString();
                    item.EAN = myr["EAN"].ToString();
                    item.ItemDesc = myr["Description"].ToString();
                    item.Model = myr["Model"].ToString();
                    item.note = myr["note"].ToString();
                    item.unit = myr["Unit"].ToString();
                    item.GroupFi = myr["GroupFi"].ToString();
                    item.SalesPrice = ((myr["SalesPrice"] == DBNull.Value) ? 0 : (Decimal)myr["SalesPrice"]);
                    item.volume = ((myr["volume"] == DBNull.Value) ? 0 : (Decimal)myr["volume"]);
                    item.Weight = ((myr["Weight"] == DBNull.Value) ? 0 : (Decimal)myr["Weight"]);

                    item.BrowserTitle = myr["BrowserTitle"].ToString();
                    item.MetaData = myr["MetaData"].ToString();
                    item.URL = myr["URL"].ToString();
                    item.MetaText = myr["MetaText"].ToString();
                    item.IncludeItemID = myr["IncludeItemID"].ToString();
                    item.SubstituteItem = myr["SuppliersInvNo"].ToString();
                    item.UNSPSC = myr["UNSPSC"].ToString();
                    item.CostPriceFixed = ((myr["CostPriceFixed"] == DBNull.Value) ? 0 : (Decimal)myr["CostPriceFixed"]);
                    item.Position = myr["Position"].ToString();
                    item.NotOnWeb = (Boolean)myr["NotOnWeb"];
                    item.NotOnMyPage = (Boolean)myr["NotOnMyPage"];

                    //item.DateCreate = (DateTime)((myr["DateCreate"] == DBNull.Value) ? DateTime.MinValue : myr["DateCreate"]);
                    //item.DateUpdate = (DateTime)((myr["DateUpdate"] == DBNull.Value) ? DateTime.MinValue : myr["DateUpdate"]);
                    //item.DateActive = (DateTime)((myr["DateActive"] == DBNull.Value) ? DateTime.MinValue : myr["DateActive"]);
                    //item.DateInactive = (DateTime)((myr["DateInactive"] == DBNull.Value) ? DateTime.MinValue : myr["DateInactive"]);
                }
                conn.Close();
            }
            return errStr;
        }
        public string Inventory_Item_add(ref inventoryItem item)
        {   //Will look up one specifik item - since criterias a the primary key. For broader search use Inventory_Item_Lookup()
            SqlConnection conn = new SqlConnection(conn_str);
            string errStr = "err";
            string itemid = item.ItemID;
            if (itemid != String.Empty)
            {
                SqlCommand comm = new SqlCommand("wf_tr_inventory_Add", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_ItemID", SqlDbType.NVarChar, 20).Value = itemid;
                comm.Parameters.Add("@P_Description", SqlDbType.NVarChar, 255).Value = (string.IsNullOrEmpty(item.ItemDesc) ? DBNull.Value : (object)item.ItemDesc);
                comm.Parameters.Add("@P_Class", SqlDbType.Int).Value = DBNull.Value;
                comm.Parameters.Add("@P_GroupFi", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(item.GroupFi) ? DBNull.Value : (object)item.GroupFi);
                comm.Parameters.Add("@P_GroupPr", SqlDbType.NVarChar, 20).Value = DBNull.Value;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return errStr;
        }

        public string Inventory_Item_save(ref inventoryItem item)
        {   //Will look up one specifik item - since criterias a the primary key. For broader search use Inventory_Item_Lookup()
            SqlConnection conn = new SqlConnection(conn_str);
            string errStr = "err";
            string itemid = item.ItemID;
            if (itemid != String.Empty)
            {
                string mysql = "update tb1 set Description = @Desc,EAN = @EAN,Unit = @Unit,note = @note,Model = @Model,GroupFi = @GroupFi, IncludeItemID = @IncludeItemID, ";
                mysql = String.Concat(mysql, " SalesPrice = @SalesPrice, volume = @volume, Weight = @Weight, SuppliersInvNo = @SubstituteItem, UNSPSC = @UNSPSC, ");
                mysql = String.Concat(mysql, " CostPriceFixed = @CostPriceFixed, Position = @Position, NotOnWeb = @NotOnWeb, NotOnMyPage = @NotOnMyPage, DateUpdate = getdate() FROM tr_inventory tb1 ");
                mysql = String.Concat(mysql, " WHERE tb1.CompID = @CompID AND tb1.ItemID = @ItemID ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = itemid;
                comm.Parameters.Add("@EAN", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(item.EAN) ? DBNull.Value : (object)item.EAN);
                comm.Parameters.Add("@Desc", SqlDbType.NVarChar, 1024).Value = (string.IsNullOrEmpty(item.ItemDesc) ? DBNull.Value : (object)item.ItemDesc);
                comm.Parameters.Add("@Model", SqlDbType.NVarChar, 512).Value = (string.IsNullOrEmpty(item.Model) ? DBNull.Value : (object)item.Model);
                comm.Parameters.Add("@note", SqlDbType.NVarChar, 512).Value = (string.IsNullOrEmpty(item.note) ? DBNull.Value : (object)item.note);
                comm.Parameters.Add("@unit", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(item.unit) ? DBNull.Value : (object)item.unit);
                comm.Parameters.Add("@GroupFi", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(item.GroupFi) ? DBNull.Value : (object)item.GroupFi);
                comm.Parameters.Add("@IncludeItemID", SqlDbType.NVarChar, 40).Value = (string.IsNullOrEmpty(item.IncludeItemID) ? DBNull.Value : (object)item.IncludeItemID);
                comm.Parameters.Add("@SubstituteItem", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(item.SubstituteItem) ? DBNull.Value : (object)item.SubstituteItem);
                comm.Parameters.Add("@SalesPrice", SqlDbType.Money).Value = item.SalesPrice;
                comm.Parameters.Add("@volume", SqlDbType.Money).Value = item.volume;
                comm.Parameters.Add("@Weight", SqlDbType.Money).Value = item.Weight;
                comm.Parameters.Add("@UNSPSC", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(item.UNSPSC) ? DBNull.Value : (object)item.UNSPSC);
                comm.Parameters.Add("@CostPriceFixed", SqlDbType.Money).Value = item.CostPriceFixed;
                comm.Parameters.Add("@Position", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(item.Position) ? DBNull.Value : (object)item.Position);
                comm.Parameters.Add("@NotOnWeb", SqlDbType.Bit).Value = item.NotOnWeb;
                comm.Parameters.Add("@NotOnMyPage", SqlDbType.Bit).Value = item.NotOnMyPage;

                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
                 if (item.Selections.Length > 0) Inventory_Item_selections_save(ref item);
            }
            
            return errStr;
        }

        public string Inventory_Item_selections_save(ref inventoryItem item)
        {   //Will look up one specifik item - since criterias a the primary key. For broader search use Inventory_Item_Lookup()
            string errStr = "err";
            string itemid = item.ItemID;
            string[] Selections = item.Selections;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql1 = " if not exists(select * from tr_inventory_selections where CompID = @CompID AND SelectionID = @SelectionID) ";
            mysql1 = string.Concat(mysql1, " insert tr_inventory_selections(CompID, SelectionID, Selection, UseAsParent) values(@CompID, @SelectionID, 'Added by API', 1) ");
            string mysql2 = " if not exists (select * from tr_inventory_selections_items where CompID = @CompID AND ItemID = @ItemID AND SelectionID = @SelectionID) ";
            mysql2 = string.Concat(mysql2, " insert tr_inventory_selections_items (CompID,ItemID,SelectionID) values (@CompID,@ItemID,@SelectionID) ");
            SqlCommand comm = new SqlCommand(mysql1, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = itemid;
            comm.Parameters.Add("@SelectionID", SqlDbType.NVarChar, 20).Value = DBNull.Value;
            conn.Open();
            foreach (string Selection in Selections) {
                if (!string.IsNullOrEmpty(Selection))
                    {
                        comm.Parameters["@SelectionID"].Value = Selection;
                        comm.CommandText = mysql1;
                        comm.ExecuteNonQuery();
                        comm.CommandText = mysql2;
                        comm.ExecuteNonQuery();
                    }
        }
        conn.Close();
        return errStr;
    }

    public inventoryPicture Inventory_ItemPicture_get(string ItemID, int PicID, bool Thumpnail)
        {
            inventoryPicture Result = new inventoryPicture();
            List<inventoryPictureList> PicList = new List<inventoryPictureList>();

            if (ItemID != String.Empty)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string AllPicid = "SELECT PicID, Description FROM tr_inventory_Pictures where CompID =  @P_CompID and itemid = @P_ItemID order by DefaultPic desc, PicID";
                SqlCommand comm = new SqlCommand(AllPicid, conn);
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
                conn.Open();
                inventoryPictureList PicForList;
                bool PicFound = false;
                SqlDataReader AllPics = comm.ExecuteReader();
                while (AllPics.Read())
                {
                    PicForList = new inventoryPictureList();
                    if (PicID == (int)AllPics["PicID"]) PicFound = true;
                    PicForList.PicID = (int)AllPics["PicID"];  //add the rest
                    PicForList.Description = AllPics["Description"].ToString();
                    PicList.Add(PicForList);
                }
                if (PicList.Count > 0) {
                    if (!PicFound) PicID = PicList[0].PicID; //If no PicID requested get one (Which will be the defaultPic or the lowest PicID)
                    Result.Alternatives = PicList.ToArray();
                    SqlConnection conn2 = new SqlConnection(conn_str);

                    string MyPic = "Select * FROM tr_inventory_Pictures where Picid = @P_PicID";
                    SqlCommand PicRead = new SqlCommand(MyPic, conn2);
                    PicRead.Parameters.Add("@P_PicID", SqlDbType.Int).Value = PicID;
                    conn2.Open();
                    SqlDataReader ThePic = PicRead.ExecuteReader();
                    if (ThePic.Read())
                    {
                        Result.PicID = (int)ThePic["PicID"];
                        Result.Default = ThePic["DefaultPic"] == DBNull.Value ? false : (bool)ThePic["DefaultPic"];
                        Result.Description = ThePic["Description"].ToString();
                        Result.InsertDate = ThePic["InsertDate"] == DBNull.Value ? DateTime.Now : (DateTime)ThePic["InsertDate"];
                        Result.Origin = ThePic["InsertFrom"].ToString();

                        if ((Thumpnail) & (ThePic["thumbnail"] == DBNull.Value)) Thumpnail = false; //if thumbnail wanted and no thumbnail present, return picture
                        if ((!Thumpnail) & (ThePic["Picture"] == DBNull.Value)) Thumpnail = true;   //if picture wanted and no picture present, return thumbnail
                        if (Thumpnail) {
                            Result.Picture = ThePic["thumbnail"] == DBNull.Value ? null : (byte[])ThePic["thumbnail"];
                            Result.ContentType = ThePic["ContentTypeThumbnail"].ToString();
                        }
                        else {
                            Result.Picture = ThePic["Picture"] == DBNull.Value ? null : (byte[])ThePic["Picture"];
                            Result.ContentType = ThePic["ContentType"].ToString();
                        }
                    }
                    conn2.Close();
                    conn.Close();
                }
            }
            return Result;
        }
        public string Inventory_Item_Lookup(ref inventoryItem Item, ref int wfcount)
        {
            string itemID = "";
            //string errStr = "err";
            wfcount = 0;
            if (!string.IsNullOrEmpty(Item.ItemID))
            {
                wfcount = 1;
                itemID = Item.ItemID;
            }
            else
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT max(ItemID) as ItemID, count(*) as wf_Count  FROM tr_Inventory Where CompID = @CompID  ";
                if (!string.IsNullOrEmpty(Item.EAN)) mysql = string.Concat(mysql, " AND ean = @Ean ");
                if (!string.IsNullOrEmpty(Item.ItemDesc)) mysql = string.Concat(mysql, " AND ItemDesc like @ItemDesc  + '%' ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@Ean", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(Item.EAN)) ? DBNull.Value : (object)Item.EAN);
                comm.Parameters.Add("@ItemDesc", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(Item.ItemDesc)) ? DBNull.Value : (object)Item.ItemDesc);
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    itemID = ((myr["ItemID"] == DBNull.Value) ? "" : myr["ItemID"].ToString());
                    Item.ItemID = itemID;
                    wfcount = ((myr["wf_count"] == DBNull.Value) ? 0 : (Int32)myr["wf_count"]);
                }
                conn.Close();
            }
            return itemID;
        }

        public string inventory_load_sales_prices(string ItemID, ref IList<inventorySalesPrice> items)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            inventorySalesPrice item = new inventorySalesPrice();
            try
            {
                string mysql = "select tb1.PriceID, tb1.Price,tb2.currency,tb2.Description from tr_prices_sa tb1 inner join tr_prices_saList tb2 on tb2.CompID = tb1.CompID AND tb2.PriceID = tb1.PriceID   WHERE tb1.CompID = @CompID AND tb1.itemID = @ItemID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = ItemID;

                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.PriceID = myr["priceID"].ToString();
                    item.Price = (Decimal)myr["Price"];
                    item.Currency = myr["Currency"].ToString();
                    item.PriceDesc = myr["Description"].ToString();
                    items.Add(item);
                    item = new inventorySalesPrice();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }

        public string inventory_load_qty(string ItemID, ref IList<inventoryQty> items)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            inventoryQty item = new inventoryQty();
            try
            {

                SqlCommand comm = new SqlCommand("wf_wcf_Inventory_Get_qty", conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
                comm.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.LocationID = myr["LocationID"].ToString();
                    item.LocationDesc = myr["LocationDesc"].ToString();
                    item.QtyClosed = (Decimal)myr["QtyClosed"];
                    item.QtyOrderPurc = (Decimal)myr["QtyOrderPurc"];
                    item.QtyOrderSale = (Decimal)myr["QtyOrderSale"];
                    item.NextDelivery = (DateTime)((myr["NextDelivery"] == DBNull.Value) ? DateTime.MinValue : (Object)myr["NextDelivery"]);
                    items.Add(item);
                    item = new inventoryQty();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }


        public string inventory_load_Selections(string ItemID, ref IList<string> items)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            //string item = string.Empty;
            try
            {

                SqlCommand comm = new SqlCommand("select SelectionID from  tr_inventory_selections_Items where CompID = @CompID AND ItemID = @ItemID ", conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    string item = myr["SelectionID"].ToString();
                    items.Add(item);
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }



        public string Inventory_Item_get_extra_text(ref inventoryItemExtra item, string textID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string errStr = "err";
            string itemid = item.ItemID;
            string mysql = string.Empty;
            if (itemid != string.Empty)
            {
                if (textID == "1")
                {
                    mysql = "SELECT Description, unit FROM tr_inventory_text  WHERE CompID = @CompID AND ItemID = @ItemID AND CountryID = @LanguageID ";
                }
                else
                {
                    mysql = "SELECT Description, unit FROM tr_inventory_text_web  WHERE CompID = @CompID AND ItemID = @ItemID AND CountryID = @LanguageID ";
                }

                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = itemid;
                comm.Parameters.Add("@LanguageID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(item.LanguageID) ? "DK" : item.LanguageID);

                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    if (textID == "1")
                    {
                        item.ItemDescInvoice = myr["Description"].ToString();
                        item.unitInvoice = myr["Unit"].ToString();
                    }
                    else
                    {
                        item.ItemDescWeb = myr["Description"].ToString();
                        item.unitWeb = myr["Unit"].ToString();
                    }
                }
                conn.Close();
                item.unitInvoice = "xxx";
            }

            return errStr;
        }

        public string get_inventory_invoice_text(string itemID, string Language)
        {
            string InventoryText = string.Empty;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = string.Empty;
            if (itemID != String.Empty) mysql = "Select ItemID, Description,EAN,Unit,note,Model, ";

            return InventoryText;
        }


        public string Inventory_GetVat(ref InventoryVAT VAT)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("wf_wcf_GetVAT", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_GroupFi", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(VAT.GroupFI)) ? DBNull.Value : (object)VAT.GroupFI);
            comm.Parameters.Add("@P_ItemID", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(VAT.ItemID)) ? DBNull.Value : (object)VAT.ItemID);
            comm.Parameters.Add("@P_Seller", SqlDbType.Int).Value = ((VAT.Seller == 0) ? 0 : VAT.Seller);
            comm.Parameters.Add("@P_DebtorGroup", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(VAT.DebtorGroup)) ? DBNull.Value : (object)VAT.DebtorGroup);
            comm.Parameters.Add("@P_CreditorGroup", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(VAT.CreditorGroup)) ? DBNull.Value : (object)VAT.CreditorGroup);
            comm.Parameters.Add("@O_saVAT", SqlDbType.Decimal).Direction = ParameterDirection.Output;
            comm.Parameters.Add("@O_puVAT", SqlDbType.Decimal).Direction = ParameterDirection.Output;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            VAT.puVat = (Decimal)comm.Parameters["@O_saVAT"].Value;
            VAT.saVat = (Decimal)comm.Parameters["@O_puVAT"].Value;
            return errStr;
        }


        public string Menu_Items_get(ref IList<MenuItem> items, string selection)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            MenuItem item = new MenuItem();
            SqlCommand comm = new SqlCommand("we_invt_selections_menu_part", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Selection", SqlDbType.NVarChar, 20).Value = selection;
            comm.Parameters.Add("@ItemCount", SqlDbType.Int).Direction = ParameterDirection.Output;

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SelectionID = myr["SelectionID"].ToString();
                item.ItemLevel = (Int32)myr["ItemLevel"];
                item.FolderName = myr["FolderName"].ToString();
                item.FolderDesc = myr["FolderDesc"].ToString();
                item.parentID = myr["ParentID"].ToString();
                item.p1 = myr["p1"].ToString();
                item.p2 = myr["p2"].ToString();
                item.p3 = myr["p3"].ToString();
                item.Collection = (Boolean)myr["Collection"];
                item.BrowserTitle = myr["BrowserTitle"].ToString();
                item.MetaData = myr["MetaData"].ToString();
                item.URL = myr["URL"].ToString();
                item.MetaText = myr["MetaText"].ToString();
                items.Add(item);
                item = new MenuItem();
            }
            conn.Close();
            return "OK";
        }

        public string Menu_Items_get_translated(ref IList<MenuItem> items, string Language, string selection)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            MenuItem item = new MenuItem();
            SqlCommand comm = new SqlCommand("we_invt_selections_menu_part_translated", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Language", SqlDbType.NVarChar, 20).Value = Language;
            comm.Parameters.Add("@Selection", SqlDbType.NVarChar, 20).Value = selection;
            comm.Parameters.Add("@ItemCount", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SelectionID = myr["SelectionID"].ToString();
                item.ItemLevel = (Int32)myr["ItemLevel"];
                item.FolderName = myr["FolderName"].ToString();
                item.FolderDesc = myr["FolderDesc"].ToString();
                item.NodeNote = myr["NodeNote"].ToString();
                item.parentID = myr["ParentID"].ToString();
                item.p1 = myr["p1"].ToString();
                item.p2 = myr["p2"].ToString();
                item.p3 = myr["p3"].ToString();
                item.p4 = myr["p4"].ToString();
                item.p5 = myr["p5"].ToString();
                items.Add(item);
                item = new MenuItem();
            }
            conn.Close();
            return "OK";
        }

        public byte[] Menu_Item_get_picture(string SelectionID, string Description, bool Thumbnail, ref string PicType)
        {
            byte[] ThePicture = null;
            SqlConnection conn = new SqlConnection(conn_str);
            MenuItem item = new MenuItem();
            string MySQL = "SELECT top(1) case @P_ThumbNail when 0 then Picture else thumbnail end as Picture, case @P_ThumbNail when 0 then ContentType else ContentTypeThumbnail end as PictureType FROM tr_inventory_selections_Pictures  where compid = @P_Compid and selectionid = @P_SelectionID and((Description like @P_Description) or(DefaultPic <> 0))  order by DefaultPic, PicID";
            SqlCommand comm = new SqlCommand(MySQL, conn);
            comm.CommandType = CommandType.Text;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_SelectionID", SqlDbType.NVarChar, 20).Value = SelectionID;
            comm.Parameters.Add("@P_Description", SqlDbType.NVarChar, 20).Value = Description;
            comm.Parameters.Add("@P_ThumbNail", SqlDbType.Bit).Value = Thumbnail;

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                ThePicture = myr["Picture"] == DBNull.Value ? null : (byte[])myr["Picture"];
                PicType = myr["PictureType"] == DBNull.Value ? null : myr["PictureType"].ToString();
            }
            conn.Close();
            return ThePicture;
        }


        public string Menu_ShopItems_get(ref IList<MenuItem> items, string selection)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            MenuItem item = new MenuItem();
            SqlCommand comm = new SqlCommand("we_invt_selections_menu_shop", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Selection", SqlDbType.NVarChar, 20).Value = selection;
            comm.Parameters.Add("@ItemCount", SqlDbType.Int).Direction = ParameterDirection.Output;

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SelectionID = myr["SelectionID"].ToString();
                item.ItemLevel = (Int32)myr["ItemLevel"];
                item.FolderName = myr["FolderName"].ToString();
                item.FolderDesc = myr["FolderDesc"].ToString();
                item.parentID = myr["ParentID"].ToString();
                item.p1 = myr["p1"].ToString();
                item.p2 = myr["p2"].ToString();
                item.p3 = myr["p3"].ToString();
                items.Add(item);
                item = new MenuItem();
            }
            conn.Close();
            return "OK";
        }


        public string get_selection_text(string selectionID, string countryID, ref string[] SelText)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT Header, Note FROM tr_inventory_selections_text WHERE CompID = @CompID AND SelectionID = @SelectionID AND CountryID = @CountryID";
            try
            {
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SelectionID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(selectionID) ? DBNull.Value : (object)selectionID);
                comm.Parameters.Add("@CountryID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(countryID) ? DBNull.Value : (object)countryID);
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    SelText[0] = myr["Header"].ToString();
                    SelText[1] = myr["Note"].ToString();
                }
                conn.Close();
            }
            catch (Exception e) { errStr = e.Message; }
            return errStr;
        }

        public int AddPicture(string ItemID, byte[] Picture, string Description)
        {
            int RetVal = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "If exists(select * from tr_inventory where CompID = @P_CompID and Itemid = @P_ItemID) ";
            mysql = string.Concat(mysql, "Insert into tr_inventory_Pictures (CompID, ItemID, Picture, Description, DefaultPic, InsertFrom) OUTPUT inserted.PicID ");
            mysql = string.Concat(mysql, "VALUES (@P_CompID, @P_ItemID, @P_Picture, @P_Description, 0, 'Webservice') else select 0");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
            comm.Parameters.Add("@P_Picture", SqlDbType.Binary).Value = Picture;
            comm.Parameters.Add("@P_Description", SqlDbType.NVarChar, 255).Value = Description;
            conn.Open();
            int.TryParse(comm.ExecuteScalar().ToString(), out RetVal);
            conn.Close();
            return RetVal;
        }
        private void append_extraLines(string ItemID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("wf_tr_inventory_AddExtraLines", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_ItemID", SqlDbType.NVarChar, 40).Value = ItemID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
        }

        public string Inventory_ExtraLines_load(string ItemID, ref IList<InventoryExtraLine> items)
        {
            string errStr = "err";
            append_extraLines(ItemID);
            SqlConnection conn = new SqlConnection(conn_str);
            var item = new InventoryExtraLine();
            string mysql = "SELECT * FROM tr_inventory_ExtraLines Where CompID = @CompID AND Itemid = @ItemID order by LinePos ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ItemID", SqlDbType.NVarChar,40).Value = ItemID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.ItemID = ItemID;
                item.Description = myr["Description"].ToString();
                item.Value = myr["Value"].ToString();
                items.Add(item);
                item = new InventoryExtraLine();
            }
            conn.Close();
            return errStr;
        }
        //extralines updates are missing

        public string Inventory_Styles_load(string ItemID, ref IList<InventoryStyle> items)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            var item = new InventoryStyle();
            string mysql = "SELECT * FROM tr_inventory_Styles Where CompID = @CompID AND Itemid = @ItemID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.ItemID = ItemID;
                item.StyleID = wfsh.Cint(myr["StyleID"].ToString());
                item.Style = myr["Style"].ToString();
                item.ReorderPoint = wfsh.Cdec(myr["ReorderPoint"].ToString());
                item.ReorderQty = wfsh.Cdec(myr["ReorderQty"].ToString());
                item.QtyPackages = wfsh.Cdec(myr["QtyPackages"].ToString());
                item.Unit = myr["Unit"].ToString();
                item.ReductionProc = wfsh.Cdec(myr["ReductionProc"].ToString());
                item.StEAN = myr["StEAN"].ToString();
                item.ReorderMax = wfsh.Cdec(myr["ReorderMax"].ToString());
                item.StyleNo = wfsh.Cint(myr["StyleNo"].ToString());
                item.StyleDesc = myr["StyleDesc"].ToString();
                items.Add(item);
                item = new InventoryStyle();
            }
            conn.Close();
            return errStr;
        }

        // containers ************************

        public string Inventory_Containers_load(string ItemID, ref IList<inventoryContainer> items)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            var item = new inventoryContainer();
            string mysql = "SELECT * FROM tr_inventory_Containers Where CompID = @CompID AND Itemid = @ItemID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.ItemID = ItemID;
                item.Barcode = myr["Barcode"].ToString();
                item.Unit = myr["Unit"].ToString();
                item.Description = myr["Description"].ToString();
                item.Manufacture = myr["Manufacture"].ToString();
                items.Add(item);
                item = new inventoryContainer();
            }
            conn.Close();
            return errStr;
        }

        public string Inventory_Container_Add(ref inventoryContainer Item)
        {
        string errStr = "err";
            if (Item.Qty == 0) Item.Qty = 1;
        if (Item.ItemID != string.Empty && Item.Barcode != string.Empty)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql1 = "if not exists (select * from tr_inventory_containers where CompID = @CompID AND ItemID = @ItemID AND Barcode = @Barcode) ";
            mysql1 = string.Concat(mysql1, " insert tr_inventory_containers (CompID,ItemID,barcode) values (@CompID,@ItemID,@Barcode) ");
            SqlCommand comm = new SqlCommand(mysql1, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = Item.ItemID;
            comm.Parameters.Add("@Barcode", SqlDbType.NVarChar, 20).Value = Item.Barcode;
            comm.Parameters.Add("@Qty", SqlDbType.Money).Value = Item.Qty;
            comm.Parameters.Add("@manufacture", SqlDbType.NVarChar, 50).Value = (Item.Manufacture == null ? DBNull.Value : (object)Item.Manufacture);
            comm.Parameters.Add("@Unit", SqlDbType.NVarChar, 20).Value = (Item.Unit == null ? DBNull.Value : (object)Item.Unit);
            comm.Parameters.Add("@Description", SqlDbType.NVarChar, 20).Value = (Item.Description == null ? DBNull.Value : (object)Item.Description);
            conn.Open();
            comm.ExecuteNonQuery();
            comm.CommandText = "update tr_inventory_containers set Qty = @Qty, manufacture = @manufacture, Unit = @Unit ,Description = @Description where CompID = @CompID AND ItemID = @ItemID AND Barcode = @Barcode  ";
            comm.ExecuteNonQuery();
            conn.Close();
        }
        return errStr;
        }
        public string Inventory_Container_clear(ref inventoryContainer Item)
        {
            string errStr = "err";
            if (Item.ItemID != null && Item.Barcode != null)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "delete FROM tr_inventory_Containers Where CompID = @CompID AND Itemid = @ItemID ";
                if (Item.Barcode != "*") mysql = string.Concat(mysql, " AND Barcode = @Barcode ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = Item.ItemID;
                comm.Parameters.Add("@Barcode", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(Item.Barcode) ? DBNull.Value : (object)Item.Barcode);
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return errStr;
        }

        // Vendors ************************

        public string Inventory_Vendors_load(string ItemID, ref IList<inventoryVendors> items)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            var item = new inventoryVendors();
            string mysql = "select ad_account,CompanyName from ad_addresses tb1 where CompID = @CompID AND ";
            mysql = string.Concat(mysql, " exists (select * from tr_inventory_vendors tb2 where tb2.CompID = tb1.CompID AND tb2.Vendor = tb1.AddressID ");
            if (!string.IsNullOrEmpty(ItemID)) mysql = string.Concat(mysql, " AND ItemID = @ItemID ");
            mysql = string.Concat(mysql, " ) order by CompanyName ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.Vendor = myr["ad_account"].ToString(); ;
                item.VendorName = myr["CompanyName"].ToString();
                items.Add(item);
                item = new inventoryVendors();
            }
            conn.Close();
            return errStr;
        }

        public string Inventory_Vendor_Add(ref inventoryVendor Item)
        {
            string errStr = "err";
            int AdrID = 0;
           if (Item.ItemID != string.Empty && Item.Vendor != string.Empty)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql1 = "Select isnull(max(AddressID),0) From ad_addresses where CompID = @CompID AND ad_account = @Vendor ";
                string mysql2 = "if not exists (select * from tr_inventory_vendors where CompID = @CompID AND ItemID = @ItemID AND Vendor = @AdrID) ";
                mysql2 = string.Concat(mysql2, " insert tr_inventory_vendors (CompID,ItemID,Vendor,DateUpdate) values (@CompID,@ItemID,@AdrID,@DateUpdate) ");
                SqlCommand comm = new SqlCommand(mysql1, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = Item.ItemID;
                comm.Parameters.Add("@Vendor", SqlDbType.NVarChar, 20).Value = Item.Vendor;
                comm.Parameters.Add("@DateUpdate", SqlDbType.DateTime).Value = DateTime.Today;
                comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = 0;
                conn.Open();
                AdrID = (int)comm.ExecuteScalar();
                if (AdrID > 0) {
                    comm.Parameters["@AdrID"].Value = AdrID;
                    comm.CommandText = mysql2;
                    comm.ExecuteNonQuery();
                }
                conn.Close();
            }
            return errStr;
        }
        public string Inventory_Vendor_clear(string ItemID)
        {
            string errStr = "err";
            if (ItemID != null)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "delete FROM tr_inventory_Vendors Where CompID = @CompID AND Itemid = @ItemID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return errStr;
        }



    }
}