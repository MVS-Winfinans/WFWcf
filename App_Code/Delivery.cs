using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Summary description for Delivery
/// </summary>
public class Delivery
{

    int compID;
    string conn_str;

    public Delivery(ref DBUser DBUser)
	{
         //conn_str = DBUser.ConnectionString;
        compID = DBUser.CompID;
        //if (DBUser.PublicConnection == false) DBUser.ConnectionString = "Not public";
        if (string.IsNullOrEmpty(DBUser.ConnectionString))
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            conn_str = wfconn.ConnectionGetByGuid_02(ref DBUser);
        }
        else
        {
            conn_str = DBUser.ConnectionString;
        }


    }
    public Delivery(ref DBUser DBUser, string ConnStr)
    {
        conn_str = ConnStr;
        compID = DBUser.CompID;
        //DBUser.ConnectionString = "Not public";
    }

    public string ProductListLoad(int BillTo, ref IList<SalesDeliveriesProducts> items)
    {
        string retstr = "OK";
        string ShipTopostelCode = string.Empty;
        SqlConnection conn = new SqlConnection(conn_str);
        SalesDeliveriesProducts Item = new SalesDeliveriesProducts();
        //  SELECT top 10 * FROM ac_Companies_calendars_items where CompID = 1000 AND CalendarID = 20 AND Repeat_last is null
        string mysql = " select tb1.saleID, tb2.Liid,tb2.SourceSaleID, tb2.SourceLineID, isnull(tb2.Orderno,tb1.Orderno) as Orderno , tb1.Class, So_AddressID, tb2.ItemID,tb2.Description, tb1.ShipDate,Calendar, blockReason, isnull(Sh_AddressID,so_addressID) as Sh_AddressID ,isnull(tb3.NotOnWeb,0) as NotOnWeb, isnull(tb3.NotOnMyPage,0) as NotOnMyPage,  ";
        mysql = string.Concat(mysql, " (select LongDesc from ac_Companies_calendars tb3 where  tb3.CompID = tb2.CompID AND tb3.CalendarID = tb1.Calendar) as LongDesc, ");
        mysql = string.Concat(mysql, " (SELECT case pattern when 1 then Qty When 2 then Qty When 3 then Qty when 4 then Qty * 2 When 5 Then Qty * 2 ELSE Qty END  from ac_Companies_calendars tb4 where  tb4.CompID = tb2.CompID AND tb4.CalendarID = tb1.Calendar) as Qty, ");
        mysql = string.Concat(mysql, " tb2.OrderQty as LineQty, tb2.ReplacementProduct, ");
        mysql = string.Concat(mysql, " (select isnull(SelectionID,'') + ';'  from tr_inventory_selections_Items tbse where tbse.CompID = tb2.CompID AND tbse.ItemID = tb2.ItemID  order by SelectionID FOR XML PATH(''))   as  Selections ");
        mysql = string.Concat(mysql, " from tr_sale tb1 inner join tr_sale_LineItems tb2 on tb1.CompID = tb2.CompID AND tb1.SaleID = tb2.SaleID  ");
        mysql = string.Concat(mysql, " inner join tr_inventory tb3 on tb3.CompID = tb2.CompID AND tb3.ItemID = tb2.ItemID ");
        mysql = string.Concat(mysql, "  Where tb1.CompID = @CompID AND tb1.Class in (200,300,400,900) AND So_AddressID = @AdrID AND tb2.ItemID is not null ");
        mysql = string.Concat(mysql, "  AND (tb1.Class <> 300 OR  exists (SELECT * FROM ac_Companies_calendars_items tb3 where tb3.CompID = tb1.CompID AND tb3.CalendarID = tb1.Calendar AND Item > getdate() )) ");
        mysql = string.Concat(mysql, "  And (tb1.Class = 300 OR DATEDIFF(Y,getdate(),tb1.ShipDate) > -14) ");
        mysql = string.Concat(mysql, " AND not exists (SELECT * FROM tr_inventory tb4 where tb4.compID = tb2.CompID AND tb4.ItemID = tb2.ItemID AND isnull(tb4.NotOnWeb,0) <> 0) "); 
        try
        {
        SqlCommand comm = new SqlCommand(mysql, conn);
        comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
        comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = BillTo;
        conn.Open();
        SqlDataReader myr = comm.ExecuteReader();
        while (myr.Read())
        {
            Item.SaleID = (Int32)myr["saleID"];
            Item.Liid = (Int32)myr["Liid"];
            Item.OrderNo = (myr["OrderNo"] == DBNull.Value ? 0 : (Int64)myr["OrderNo"]);
            Item.Frequence = (myr["Qty"] == DBNull.Value ? 0 : (Int32)myr["Qty"]);
            Item.LineQty = (myr["LineQty"] == DBNull.Value ? 0 : (decimal)myr["LineQty"]);
            Item.Class = (Int32)myr["Class"];
            Item.ItemID = myr["ItemID"].ToString();
            Item.ItemDescription =  myr["Description"].ToString();
            Item.Calendar = (myr["Calendar"] == DBNull.Value ? 0 : (Int32)myr["Calendar"]);
            Item.LongDesc = myr["LongDesc"].ToString();
            Item.BlockReason = (myr["BlockReason"] == DBNull.Value ? 0 : (Int32)myr["BlockReason"]);
            Item.ShipTo = (myr["Sh_AddressID"] == DBNull.Value ? 0 : (Int32)myr["Sh_AddressID"]);
            Item.ShipDesc = ShipAddressLoad(Item.ShipTo, ref ShipTopostelCode);
            Item.ShipToPostalCode = ShipTopostelCode;
            Item.DeliveryDay = (myr["ShipDate"] == DBNull.Value ? DateTime.Today : (DateTime)myr["ShipDate"]);
            Item.DeliveryWeekDay = Convert.ToInt32(Item.DeliveryDay.DayOfWeek); //  wfws.wfsh.GetWeekDay(ShipDate);
            Item.SourceSaleID = (myr["SourceSaleID"] == DBNull.Value ? 0 : (Int32)myr["SourceSaleID"]);
            Item.SourceLineID = (myr["SourceLineID"] == DBNull.Value ? 0 : (Int32)myr["SourceLineID"]);
            Item.ReplacementProduct = myr["ReplacementProduct"].ToString();
            Item.Selections = myr["Selections"].ToString();
            Item.NotOnWeb = (Boolean)myr["NotOnWeb"];
            Item.NotOnMyPage = (Boolean)myr["NotOnMyPage"];

                if (Item.Class == 300)
             {
                 Item.Deliveries = ProductListLoadTimes(Item.Calendar, Item.SaleID, ref retstr, Item.ShipTo, Item.ShipDesc,Item.ShipToPostalCode);
            
                 if (Item.Deliveries.Length > 0)
                 {
                     Item.DeliveryDay = Item.Deliveries[0].DeliveryDay;
                     Item.DeliveryWeekDay = Item.Deliveries[0].DeliveryWeekDay;
                }


             } else {

                  Item.Deliveries = ProductListAddTimeItem(Item.Calendar, Item.DeliveryDay, ref retstr, Item.ShipTo, Item.ShipDesc, Item.ShipToPostalCode);
            }
            items.Add(Item);
           Item = new SalesDeliveriesProducts();
        }
        conn.Close();
        }
            catch (Exception e) { retstr = e.Message; }
        return retstr;
    }

    public string ProductListLoad_2(int BillTo, ref IList<SalesDeliveriesProducts> items)
    {
        string retstr = "OK";
        string ShipTopostelCode = string.Empty;
        SqlConnection conn = new SqlConnection(conn_str);
        SalesDeliveriesProducts Item = new SalesDeliveriesProducts();
        //  SELECT top 10 * FROM ac_Companies_calendars_items where CompID = 1000 AND CalendarID = 20 AND Repeat_last is null
        string mysql = " select tb1.saleID, tb2.Liid,tb2.SourceSaleID, tb2.SourceLineID, isnull(tb2.Orderno,tb1.Orderno) as Orderno , tb1.Class, So_AddressID, tb2.ItemID,tb2.Description, tb1.ShipDate,Calendar, blockReason, isnull(Sh_AddressID,so_addressID) as Sh_AddressID ,isnull(tb3.NotOnWeb,0) as NotOnWeb, isnull(tb3.NotOnMyPage,0) as NotOnMyPage,   ";
        mysql = string.Concat(mysql, " (select LongDesc from ac_Companies_calendars tb3 where  tb3.CompID = tb2.CompID AND tb3.CalendarID = tb1.Calendar) as LongDesc, ");
        mysql = string.Concat(mysql, " (SELECT case pattern when 1 then Qty When 2 then Qty When 3 then Qty when 4 then Qty * 2 When 5 Then Qty * 2 ELSE Qty END  from ac_Companies_calendars tb4 where  tb4.CompID = tb2.CompID AND tb4.CalendarID = tb1.Calendar) as Qty, ");
        mysql = string.Concat(mysql, " tb2.OrderQty as LineQty, tb2.ReplacementProduct, ");
        mysql = string.Concat(mysql, " (select isnull(SelectionID,'') + ';'  from tr_inventory_selections_Items tbse where tbse.CompID = tb2.CompID AND tbse.ItemID = tb2.ItemID  order by SelectionID FOR XML PATH(''))   as  Selections ");
        mysql = string.Concat(mysql, " from tr_sale tb1 inner join tr_sale_LineItems tb2 on tb1.CompID = tb2.CompID AND tb1.SaleID = tb2.SaleID  ");
        mysql = string.Concat(mysql, " inner join tr_inventory tb3 on tb3.CompID = tb2.CompID AND tb3.ItemID = tb2.ItemID ");
        mysql = string.Concat(mysql, "  Where tb1.CompID = @CompID AND tb1.Class in (200,300,400,900) AND So_AddressID = @AdrID AND tb2.ItemID is not null ");
        mysql = string.Concat(mysql, "  AND (tb1.Class <> 300 OR  exists (SELECT * FROM ac_Companies_calendars_items tb3 where tb3.CompID = tb1.CompID AND tb3.CalendarID = tb1.Calendar AND Item > getdate() )) ");
        mysql = string.Concat(mysql, "  And (tb1.Class = 300 OR DATEDIFF(Y,getdate(),tb1.ShipDate) > -14) ");
        // mysql = string.Concat(mysql, " AND not exists (SELECT * FROM tr_inventory tb4 where tb4.compID = tb2.CompID AND tb4.ItemID = tb2.ItemID AND isnull(tb4.NotOnMyPage,0) <> 0) ");
        try
        {
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = BillTo;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                Item.SaleID = (Int32)myr["saleID"];
                Item.Liid = (Int32)myr["Liid"];
                Item.OrderNo = (myr["OrderNo"] == DBNull.Value ? 0 : (Int64)myr["OrderNo"]);
                Item.Frequence = (myr["Qty"] == DBNull.Value ? 0 : (Int32)myr["Qty"]);
                Item.LineQty = (myr["LineQty"] == DBNull.Value ? 0 : (decimal)myr["LineQty"]);
                Item.Class = (Int32)myr["Class"];
                Item.ItemID = myr["ItemID"].ToString();
                Item.ItemDescription = myr["Description"].ToString();
                Item.Calendar = (myr["Calendar"] == DBNull.Value ? 0 : (Int32)myr["Calendar"]);
                Item.LongDesc = myr["LongDesc"].ToString();
                Item.BlockReason = (myr["BlockReason"] == DBNull.Value ? 0 : (Int32)myr["BlockReason"]);
                Item.ShipTo = (myr["Sh_AddressID"] == DBNull.Value ? 0 : (Int32)myr["Sh_AddressID"]);
                Item.ShipDesc = ShipAddressLoad(Item.ShipTo, ref ShipTopostelCode);
                Item.ShipToPostalCode = ShipTopostelCode;
                Item.DeliveryDay = (myr["ShipDate"] == DBNull.Value ? DateTime.Today : (DateTime)myr["ShipDate"]);
                Item.DeliveryWeekDay = Convert.ToInt32(Item.DeliveryDay.DayOfWeek); //  wfws.wfsh.GetWeekDay(ShipDate);
                Item.SourceSaleID = (myr["SourceSaleID"] == DBNull.Value ? 0 : (Int32)myr["SourceSaleID"]);
                Item.SourceLineID = (myr["SourceLineID"] == DBNull.Value ? 0 : (Int32)myr["SourceLineID"]);
                Item.ReplacementProduct = myr["ReplacementProduct"].ToString();
                Item.Selections = myr["Selections"].ToString();
                Item.NotOnWeb = (Boolean)myr["NotOnWeb"];
                Item.NotOnMyPage = (Boolean)myr["NotOnMyPage"];
                if (Item.Class == 300)
                {
                    Item.Deliveries = ProductListLoadTimes(Item.Calendar, Item.SaleID, ref retstr, Item.ShipTo, Item.ShipDesc, Item.ShipToPostalCode);

                    if (Item.Deliveries.Length > 0)
                    {
                        Item.DeliveryDay = Item.Deliveries[0].DeliveryDay;
                        Item.DeliveryWeekDay = Item.Deliveries[0].DeliveryWeekDay;
                    }


                }
                else
                {

                    Item.Deliveries = ProductListAddTimeItem(Item.Calendar, Item.DeliveryDay, ref retstr, Item.ShipTo, Item.ShipDesc, Item.ShipToPostalCode);
                }
                items.Add(Item);
                Item = new SalesDeliveriesProducts();
            }
            conn.Close();
        }
        catch (Exception e) { retstr = e.Message; }
        return retstr;
    }



    public SalesDeliveryTime[] ProductListLoadTimes(int CalendarID,int SaleID, ref string errstr, int ShipTo, string ShipDesc,string ShipToPostalCode )
    {
        IList<SalesDeliveryTime> items = new List<SalesDeliveryTime>();
        SalesDeliveryTime Item = new SalesDeliveryTime();
        SqlConnection conn = new SqlConnection(conn_str);
        //string mysql = "SELECT top 10 * FROM ac_Companies_calendars_items where CompID = 1000 AND CalendarID = @CalendarID AND Repeat_last is null";
       string mysql = " SELECT tb1.CalendarID, tb1.Item,tb1.WeekNo,isnull(tb2.blockReason,tb2.type) as blockReason,tb2.UserID FROM ac_Companies_calendars_items tb1 left join ac_Companies_calendars_items_sa tb2 ";
        mysql = string.Concat(mysql, " on tb1.CompID = tb2.CompID AND tb1.CalendarID = tb2.CalendarID  AND tb1.Item = tb2.Item AND tb2.SaleID = @SaleID ");
        mysql = string.Concat(mysql, " Where tb1.CompID = @CompID AND tb1.CalendarID = @CalendarID AND Repeat_last is null  AND DATEDIFF(Y,getdate(),tb1.Item) < 90 AND DATEDIFF(Y,getdate(),tb1.Item) > -14 ");
        SqlCommand comm = new SqlCommand(mysql, conn);
        comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
        comm.Parameters.Add("@CalendarID", SqlDbType.Int).Value = CalendarID;
        comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
        try
        {

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                Item.Calendar = (Int32)myr["CalendarID"];
                Item.BlockReason = (myr["BlockReason"] == DBNull.Value ? 0 : (Int32)myr["BlockReason"]);
                Item.DeliveryDay = (DateTime)myr["Item"];
                Item.DeliveryWeek = (myr["WeekNo"] == DBNull.Value ? 0 : (Int32)myr["WeekNo"]);
                Item.UserID = myr["UserID"].ToString();
                Item.DeliveryWeekDay = Convert.ToInt32(Item.DeliveryDay.DayOfWeek); // wfws.wfsh.GetWeekDay(Item.DeliveryDay);
                Item.ShipTo = ShipTo;
                Item.ShipDesc = ShipDesc;
                Item.ShipToPostalCode = ShipToPostalCode;
                items.Add(Item);
                Item = new SalesDeliveryTime();
            }
            conn.Close();
            errstr = string.Concat(errstr, "-c-", items.Count.ToString());
        }
        catch (Exception e) { errstr = e.Message; }
        return items.ToArray();
    }

    private string ShipAddressLoad( int ShipTo, ref string ShipTopostelCode)
    {
        string ShipDesc = string.Empty;
        SqlConnection conn = new SqlConnection(conn_str);
        string mysql = " SELECT Address + ' ' + isnull(HouseNumber,'') + ' ' + isnull(InhouseMail,'') + ' ' +  isnull(PostalCode,'') + ' ' + isnull(City,'') as ShipDesc, PostalCode FROM ad_addresses ";
        mysql = string.Concat(mysql, " WHERE CompID = @CompID AND AddressID = @AdrID ");
        SqlCommand comm = new SqlCommand(mysql, conn);
        comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
        comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = ShipTo;
        conn.Open();
         SqlDataReader myr  = comm.ExecuteReader();
         if (myr.Read())
         {
             ShipDesc = myr["ShipDesc"].ToString();
             ShipTopostelCode = myr["PostalCode"].ToString();
         }
         conn.Close();

        return ShipDesc;
    }


    public SalesDeliveryTime[] ProductListAddTimeItem(int CalendarID, DateTime ShipDate, ref string errstr, int ShipTo, string ShipDesc, string ShipToPostalCode)
    {
        IList<SalesDeliveryTime> items = new List<SalesDeliveryTime>();
        SalesDeliveryTime Item = new SalesDeliveryTime();
       
        try
        {

                Item.Calendar = 0;
                Item.BlockReason = 0;
                Item.DeliveryDay = ShipDate;
                Item.DeliveryWeek = wfws.wfsh.GetWeekNumber(ShipDate);
                Item.UserID = "xx"; // String.Empty;
                Item.DeliveryWeekDay = Convert.ToInt32(ShipDate.DayOfWeek); //  wfws.wfsh.GetWeekDay(ShipDate);
                Item.ShipDesc = ShipDesc;
                Item.ShipToPostalCode = ShipToPostalCode;
                Item.ShipTo = ShipTo;
                items.Add(Item);
     
         }
        catch (Exception e) { errstr = e.Message; }
        return items.ToArray();
    }




}