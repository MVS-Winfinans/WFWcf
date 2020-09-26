using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Summary description for web_prod
/// </summary>
namespace wfws
{
    public partial class web
    {

        //  Production


        public string Prod_add(ref ProdAssembly wf_Ass, ref int O_ProdID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string errStr = "err";
            SqlCommand comm = new SqlCommand("dbo.wf_web_prod_add", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Class", SqlDbType.Int).Value = 10000;
            comm.Parameters.Add("@Category", SqlDbType.Int).Value = wf_Ass.Category;
            comm.Parameters.Add("@O_ProdID", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                wf_Ass.ProdID = (Int32)myr["ProdID"];
                wf_Ass.AssemblyNo = (Int32)myr["AssemblyNo"];
                wf_Ass.Class = (Int32)myr["Class"];
                if (string.IsNullOrEmpty(wf_Ass.Dim1)) wf_Ass.Dim1 = myr["Dim1"].ToString();
                if (string.IsNullOrEmpty(wf_Ass.Dim2)) wf_Ass.Dim2 = myr["Dim2"].ToString();
                if (string.IsNullOrEmpty(wf_Ass.Dim3)) wf_Ass.Dim3 = myr["Dim3"].ToString();
                if (string.IsNullOrEmpty(wf_Ass.Dim4)) wf_Ass.Dim4 = myr["Dim4"].ToString();
            }
            O_ProdID = wf_Ass.ProdID;
            conn.Close();
            return errStr;
        }


        public int prod_update(int ProdID, ref ProdAssembly wf_Ass)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            int err = 0;
            DateTime OldDate = new DateTime(1900, 1, 1);
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            if (DateTime.Compare(wf_Ass.DateCreate, OldDate) < 0) wf_Ass.DateCreate = OldDate;
            if (DateTime.Compare(wf_Ass.DateStart, OldDate) < 0) wf_Ass.DateStart = OldDate;
            if (DateTime.Compare(wf_Ass.DateEnd, OldDate) < 0) wf_Ass.DateEnd = OldDate;
            string mysql = " UPDATE pr_prod set  ";
            mysql = String.Concat(mysql, " Class = @Class, Category = @Category, AssemblyNo = @AssemblyNo, ItemID = @ItemID, Style = @Style, Batch = @Batch, LocationID = @LocationID, ");
            mysql = String.Concat(mysql, " Description = @Description, QtyEstimate = @QtyEstimate,  UserID = @UserID, DateCreate = @DateCreate, DateStart = @DateStart, DateEnd = @DateEnd, ");
            mysql = String.Concat(mysql, " Dim1 = @Dim1, Dim2 = @Dim2, Dim3 = @Dim3, Dim4 = @Dim4, notes = @Notes, qtyAss = @qtyAss, qtyUnit = @QtyUnit, UnitAss = @UnitAss, ");
            mysql = String.Concat(mysql, " waste = @waste, UnitInventory = @UnitInventory, timeChanged = getdate() ");
            mysql = String.Concat(mysql, " WHERE CompID = @CompID AND ProdID = @ProdID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
            comm.Parameters.Add("@Class", SqlDbType.Int).Value = wf_Ass.Class;
            comm.Parameters.Add("@Description", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(wf_Ass.Description) ? DBNull.Value : (object)wf_Ass.Description);
            comm.Parameters.Add("@Category", SqlDbType.Int).Value = wf_Ass.Category;
            comm.Parameters.Add("@AssemblyNo", SqlDbType.Int).Value = wf_Ass.AssemblyNo;
            comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.ItemID) ? DBNull.Value : (object)wf_Ass.ItemID);
            comm.Parameters.Add("@Style", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(wf_Ass.Style) ? DBNull.Value : (object)wf_Ass.Style);
            comm.Parameters.Add("@LocationID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.LocationID) ? DBNull.Value : (object)wf_Ass.LocationID);
            comm.Parameters.Add("@Batch", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.Batch) ? DBNull.Value : (object)wf_Ass.Batch);
            comm.Parameters.Add("@QtyEstimate", SqlDbType.Int).Value = wf_Ass.QtyEstimate;
            comm.Parameters.Add("@UserID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.UserID) ? DBNull.Value : (object)wf_Ass.UserID);
            comm.Parameters.Add("@DateCreate", SqlDbType.DateTime).Value = ((wf_Ass.DateCreate < minSqlDate) ? DBNull.Value : (object)wf_Ass.DateCreate);
            comm.Parameters.Add("@DateStart", SqlDbType.DateTime).Value = ((wf_Ass.DateStart < minSqlDate) ? DBNull.Value : (object)wf_Ass.DateStart);
            comm.Parameters.Add("@DateEnd", SqlDbType.DateTime).Value = ((wf_Ass.DateEnd < minSqlDate) ? DBNull.Value : (object)wf_Ass.DateEnd);
            comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.Dim1) ? DBNull.Value : (object)wf_Ass.Dim1);
            comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.Dim2) ? DBNull.Value : (object)wf_Ass.Dim2);
            comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.Dim3) ? DBNull.Value : (object)wf_Ass.Dim3);
            comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.Dim4) ? DBNull.Value : (object)wf_Ass.Dim4);
            comm.Parameters.Add("@notes", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(wf_Ass.notes) ? DBNull.Value : (object)wf_Ass.notes);
            comm.Parameters.Add("@qtyAss", SqlDbType.Money).Value = wf_Ass.qtyAss;
            comm.Parameters.Add("@qtyUnit", SqlDbType.Money).Value = wf_Ass.qtyUnit;
            comm.Parameters.Add("@UnitAss", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.UnitAss) ? DBNull.Value : (object)wf_Ass.UnitAss);
            comm.Parameters.Add("@waste", SqlDbType.Money).Value = wf_Ass.waste;
            comm.Parameters.Add("@UnitInventory", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wf_Ass.UnitInventory) ? DBNull.Value : (object)wf_Ass.UnitInventory);
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return err;
        }



        public string prod_load(int ProdID, ref ProdAssembly wf_Ass)
        {
            string retstr = "OK";

            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT * from pr_prod where CompID = @CompID AND ProdID = @ProdID ";
            try
            {

                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {

                    wf_Ass.Class = ((Convert.IsDBNull(myr["Class"])) ? 1000 : (Int32)myr["Class"]);
                    wf_Ass.Category = ((Convert.IsDBNull(myr["Category"])) ? 100 : (Int32)myr["Category"]);
                    wf_Ass.AssemblyNo = ((Convert.IsDBNull(myr["AssemblyNo"])) ? 0 : (Int32)myr["AssemblyNo"]);
                    wf_Ass.ItemID = myr["ItemID"].ToString();
                    wf_Ass.Style = myr["Style"].ToString();
                    wf_Ass.Batch = myr["Batch"].ToString();
                    wf_Ass.LocationID = myr["LocationID"].ToString();
                    wf_Ass.Description = myr["Description"].ToString();
                    wf_Ass.QtyEstimate = ((Convert.IsDBNull(myr["QtyEstimate"])) ? 0 : (Int32)myr["QtyEstimateo"]);
                    wf_Ass.UserID = myr["UserID"].ToString();
                    wf_Ass.DateCreate = (DateTime)((myr["DateCreate"] == DBNull.Value) ? DateTime.MinValue : (DateTime)myr["DateCreate"]);
                    wf_Ass.DateStart = (DateTime)((myr["DateStart"] == DBNull.Value) ? DateTime.MinValue : (DateTime)myr["DateStart"]);
                    wf_Ass.DateEnd = (DateTime)((myr["DateEnd"] == DBNull.Value) ? DateTime.MinValue : (DateTime)myr["DateEnd"]);
                    wf_Ass.Dim1 = myr["Dim1"].ToString();
                    wf_Ass.Dim2 = myr["Dim2"].ToString();
                    wf_Ass.Dim3 = myr["Dim3"].ToString();
                    wf_Ass.Dim4 = myr["Dim4"].ToString();
                    wf_Ass.notes = myr["Dim4"].ToString();
                    wf_Ass.qtyAss = ((Convert.IsDBNull(myr["qtyAsse"])) ? 0 : (Int32)myr["qtyAss"]);
                    wf_Ass.qtyUnit = ((Convert.IsDBNull(myr["qtyUnite"])) ? 0 : (Int32)myr["qtyUnit"]);
                    wf_Ass.UnitAss = myr["UnitAss"].ToString();
                    wf_Ass.UnitInventory = myr["UnitInventory"].ToString();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }



        public string Prod_add_item(int ProdID, ProdAssLine lineItem, ref int O_LiID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            O_LiID = 0;

            if (!string.IsNullOrEmpty(lineItem.ItemID))
            {
                SqlCommand comm = new SqlCommand("we_prod_add_item", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
                comm.Parameters.Add("@ItemID", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineItem.ItemID) ? DBNull.Value : (object)lineItem.ItemID));
                comm.Parameters.Add("@QtyPerUnit", SqlDbType.Money).Value = lineItem.QtyPerUnit;
                comm.Parameters.Add("@O_LiID", SqlDbType.Int).Direction = ParameterDirection.Output;
                conn.Open();
                comm.ExecuteNonQuery();
                O_LiID = (Int32)comm.Parameters["@O_LiID"].Value;
                conn.Close();
            }
            lineItem.Liid = O_LiID;
            return retstr;
        }

        public string prod_load_Item(int ProdID, int LiID, ref ProdAssLine item)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                string mysql = "Select * FROM pr_prod_lineitems WHERE CompID = @CompID AND ProdID = @ProdID AND LiID = @LiID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
                comm.Parameters.Add("@LiID", SqlDbType.Int).Value = LiID;

                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.ProdID = (Int32)myr["ProdID"];
                    item.Liid = (Int32)myr["LiiD"];
                    item.ItemID = myr["ItemID"].ToString();
                    item.Description = myr["Description"].ToString();
                    item.Style = myr["Style"].ToString();
                    item.Batch = myr["Batch"].ToString();
                    item.QtyPerUnit = (Decimal)myr["QtyPerUnit"];
                    item.UnitPrice = (Decimal)myr["UnitPricet"];
                    item.UnitInventory = myr["UnitInventory"].ToString();
                    item.UnitAssembly = myr["UnitAssemblyy"].ToString();
                    item.UnitFactor = (Decimal)myr["UnitFactor"];
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }


        public string prod_item_update(int ProdID, ProdAssLine lineitem)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            if (lineitem.Liid > 0)
            {

                string mysql = "Update pr_prod_LineItems set Description = @Description,Style = @Style,Batch = @Batch, QtyPerUnit = @QtyPerUnit, UnitInventory = @UnitInventory, UnitAssembly = @UnitAssembly, UnitFactor = @UnitFactor ";
                mysql = String.Concat(mysql, " WHERE CompID = @CompID AND ProdID = @ProdID AND LiID = @LiID");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
                comm.Parameters.Add("@LiID", SqlDbType.Int).Value = lineitem.Liid;
                comm.Parameters.Add("@Description", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineitem.Description) ? DBNull.Value : (object)lineitem.Description));
                comm.Parameters.Add("@Style", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineitem.Style) ? DBNull.Value : (object)lineitem.Style));
                comm.Parameters.Add("@Batch", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineitem.Batch) ? DBNull.Value : (object)lineitem.Batch));

                comm.Parameters.Add("@QtyPerUnit", SqlDbType.Money).Value = lineitem.QtyPerUnit;
                comm.Parameters.Add("@UnitInventory", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineitem.UnitInventory) ? DBNull.Value : (object)lineitem.UnitInventory));
                comm.Parameters.Add("@UnitAssembly", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineitem.UnitAssembly) ? DBNull.Value : (object)lineitem.UnitAssembly));
                comm.Parameters.Add("@UnitFactor", SqlDbType.Money).Value = lineitem.UnitFactor;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return retstr;
        }





        public string prod_add_operation(int ProdID, ProdOpeLine lineItem, ref int O_LiID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            O_LiID = 0;

            if (!string.IsNullOrEmpty(lineItem.OperID))
            {
                SqlCommand comm = new SqlCommand("we_prod_add_operation", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
                comm.Parameters.Add("@OperID", SqlDbType.NVarChar, 20).Value = lineItem.OperID;
                comm.Parameters.Add("@QtyPerUnit", SqlDbType.Money).Value = lineItem.QtyPerUnit;
                comm.Parameters.Add("@O_LiID", SqlDbType.Int).Direction = ParameterDirection.Output;
                conn.Open();
                comm.ExecuteNonQuery();
                O_LiID = (Int32)comm.Parameters["@O_LiID"].Value;
                conn.Close();
            }
            lineItem.Liid = O_LiID;
            return retstr;
        }

        public void prod_calculate(int ProdID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("wf_pr_CalculateOrder", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_ProdID", SqlDbType.Int).Value = ProdID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
        }


        public int get_ProdID_by_AssemblyNo(long AssemblyNo, ref string errstr)
        {
            int ProdID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(ProdID),0) FROM  pr_prod WHERE CompID = @CompID AND AssemblyNo = @AssemblyNo  ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AssemblyNo", SqlDbType.BigInt).Value = AssemblyNo;
            try
            {
                conn.Open();
                ProdID = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            catch (Exception e) { errstr = e.Message; }

            return ProdID;
        }


        public bool Prod_is_Open(int ProdID)
        {
            bool ProdOpen = false;
            int ProdClass = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(Class),0) from pr_prod where CompID = @CompID AND ProdID = @ProdID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
            conn.Open();
            ProdClass = (Int32)comm.ExecuteScalar();
            conn.Close();
            if ((ProdClass < 10900) && (ProdClass > 0)) ProdOpen = true;
            return ProdOpen;
        }

        public bool Prod_Items_Delete(int ProdID)
        {
            bool ProdOpen = false;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "delete from pr_prod_LineItems  where CompID = @CompID AND ProdID = @ProdID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return ProdOpen;
        }

        public bool Prod_operations_Delete(int ProdID)
        {
            bool ProdOpen = false;
            //int ProdClass = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "delete from pr_prod_operations where CompID = @CompID AND ProdID = @ProdID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ProdID", SqlDbType.Int).Value = ProdID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return ProdOpen;
        }

    }
}