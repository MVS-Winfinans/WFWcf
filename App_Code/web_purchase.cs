using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Implementing part of web-class related to purchases
/// </summary>
/// 
namespace wfws
{
    public partial class web
    {
        public string orderpu_load(int PurcID, ref OrderPurchase MyOrder)
        {
            string retstr = "OK";
            Address so_adr = new Address();
            Address sh_adr = new Address();
            // Dim myOrderLines As new List(Of OrderLine)
            int inv_deb = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT * from tr_Purc where CompID = @CompID AND PurcID = @PurcID ";

            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@PurcID", SqlDbType.Int).Value = PurcID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                if (myr["su_addressID"] == DBNull.Value) MyOrder.BillTo = 0; else MyOrder.BillTo = (Int32)myr["su_addressID"];
                if (myr["sh_addressID"] == DBNull.Value) MyOrder.ShipTo = 0; else MyOrder.ShipTo = (Int32)myr["sh_addressID"];
                inv_deb = (Int32)myr["delreFactor"];
                if (inv_deb == -1) MyOrder.InvoiceDebitnote = InvDeb.DebitNote; else MyOrder.InvoiceDebitnote = InvDeb.invoice;
                MyOrder.OrderNo = Convert.ToInt64(myr["OrderNo"]);
                MyOrder.InvoiceNo = Convert.ToInt64(myr["InvoiceNo"]);
                MyOrder.Currency = myr["Currency"].ToString();
                MyOrder.Language = myr["Language"].ToString();
                // MyOrder.Calendar =myr["Calendar")
                MyOrder.OrderDate = (DateTime)((myr["OrderDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["OrderDate"]);
                MyOrder.InvoiceDate = (DateTime)((myr["InvDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["InvDate"]);
                MyOrder.ShipDate = (DateTime)((myr["ShipDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["ShipDate"]);
                MyOrder.PayDate = (DateTime)((myr["DueDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["DueDate"]);
                MyOrder.Category = ((myr["Category"] == DBNull.Value) ? 0 : (Int32)myr["Category"]);
                MyOrder.seller = ((myr["sellerID"] == DBNull.Value) ? 0 : (Int32)myr["sellerID"]);
                //'MyOrder.EAN =myr["EAN"].ToString()
                // 'MyOrder.requisition =myr["requisition")
                MyOrder.ExtRef = myr["ExtRef"].ToString();
                MyOrder.IntRef = myr["IntRef"].ToString();
                MyOrder.Dim1 = myr["Dim1"].ToString();
                MyOrder.Dim2 = myr["Dim2"].ToString();
                MyOrder.Dim3 = myr["Dim3"].ToString();
                MyOrder.Dim4 = myr["Dim4"].ToString();
                MyOrder.text_1 = myr["text_1"].ToString();
                MyOrder.text_2 = myr["text_2"].ToString();
                MyOrder.text_3 = myr["text_3"].ToString();
                MyOrder.Total = (Decimal)myr["T_Total"];
                MyOrder.TotalVatEx = (Decimal)myr["T_Vat"];
                MyOrder.TotalVatIn = (Decimal)myr["T_incVAT"];
                MyOrder.TotalVatBasisEx = (Decimal)myr["T_VATBasis"];
                MyOrder.TotalVatBasisIn = (Decimal)myr["T_incVATBasis"];
                MyOrder.TermsOfPayment = myr["TermsOfPayment"].ToString();
                MyOrder.ContactPerson = myr["ContactPerson"].ToString();
                MyOrder.LocationID = myr["LocationID"].ToString();
                MyOrder.ContID = ((myr["ContID"] == DBNull.Value) ? 0 : (Int32)myr["ContID"]);
                int puClass = (int)(myr["Class"] == DBNull.Value ? 0 : myr["Class"]);
                switch (puClass) { 
                    case 500: MyOrder.OrderType = PurchaseOrderTypes.Inquery; break;
                    case 1000: MyOrder.OrderType = PurchaseOrderTypes.Order; break;
                    case 2000: MyOrder.OrderType = PurchaseOrderTypes.Invoice; break;
                    case 9000: MyOrder.OrderType = PurchaseOrderTypes.InvoiceClosed; break;
                    default: MyOrder.OrderType = PurchaseOrderTypes.Undefined; break;
                }
            }
            conn.Close();
            return retstr;
        }

        public string orderpu_load_Items(int PurcID, ref IList<OrderLinePurc> items)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            OrderLinePurc item = new OrderLinePurc();
            try
            {
                string mysql = "Select * FROM tr_Purc_LineItems WHERE CompID = @CompID AND PurcID = @PurcID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@PurcID", SqlDbType.Int).Value = PurcID;

                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.PurcID = PurcID;
                    item.Liid = (Int32)myr["LiiD"];
                    item.ItemID = myr["ItemID"].ToString();
                    item.ItemDesc = myr["Description"].ToString();
                    item.EAN = myr["EAN"].ToString();
                    item.CostPrice = (decimal)((myr["CostPrice"] == DBNull.Value) ? 0 : (decimal)myr["CostPrice"]);
                    item.OrderAmount = (decimal)((myr["Amount"] == DBNull.Value) ? 0 : (decimal)myr["Amount"]);
                    item.Qty = (decimal)((myr["OrderQty"] == DBNull.Value) ? 0 : (decimal)myr["OrderQty"]);
                    item.vat_perc = (decimal)((myr["vat_perc"] == DBNull.Value) ? 0 : (decimal)myr["vat_perc"]);
                    item.AddInformation = myr["AddInformation"].ToString();
                    item.VatIncl = (myr["VatIncl"] == DBNull.Value ? false : (Boolean)myr["VatIncl"]);
                    item.Style = myr["Style"].ToString();
                    item.Unit = myr["Unit"].ToString();
                    item.Dim1 = myr["Dim1"].ToString();
                    item.Dim2 = myr["Dim2"].ToString();
                    item.Dim3 = myr["Dim3"].ToString();
                    item.Dim4 = myr["Dim4"].ToString();
                    item.AllowanceCharge = (myr["AllowanceCharge"] == DBNull.Value ? false : (Boolean)myr["AllowanceCharge"]);
                    item.LineAmount = (decimal)((myr["LineAmount"] == DBNull.Value) ? 0 : (decimal)myr["LineAmount"]);
                    item.LineVat = (decimal)((myr["LineVat"] == DBNull.Value) ? 0 : (decimal)myr["LineVat"]);
                    item.LineVatBase = (decimal)((myr["LineVatBase"] == DBNull.Value) ? 0 : (decimal)myr["LineVatBase"]);
                    item.UNSPSC = myr["UNSPSC"].ToString();
                    item.AccountingCost = myr["AccountingCost"].ToString();
                    items.Add(item);
                    item = new OrderLinePurc();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }

        public string PurchaseOrder_Items_get(ref OrderPurchase wfOrderPurc, ref IList<OrderPurchaseItem> items, int orderClass)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            OrderPurchaseItem item = new OrderPurchaseItem();
            string mysql = " set concat_null_yields_null OFF SELECT top 10000 PurcID, invoiceNo, OrderNo, VoucherNo, invdate, ShipDate, DueDate, delreFactor,isnull(T_Total,0) as T_Total,  isnull(T_VAT,0) as T_VAT, isnull(T_IncVat,0) as T_IncVat, ";
            mysql = String.Concat(mysql, " (SELECT ad_account + ' ' + CompanyName + ' ' + Department + ' ' + Address+ ' ' + Address2 + ' ' + postalCode+ ' ' + city FROM ad_addresses tb2 where tb2.CompID = tb1.CompID AND tb2.AddressID = tb1.su_addressID) AdddressText ");
            mysql = String.Concat(mysql, " FROM tr_purc tb1  ");
            mysql = String.Concat(mysql, " WHERE CompID = @CompID ");
            if (orderClass > 0) mysql = String.Concat(mysql, " AND Class = @Class ");
            if (orderClass == 0) mysql = String.Concat(mysql, " AND Class in (1000,2000,9000) ");

            if (!string.IsNullOrEmpty(wfOrderPurc.Dim1)) mysql = String.Concat(mysql, " AND Dim1 = @Dim1 ");
            if (!string.IsNullOrEmpty(wfOrderPurc.Dim2)) mysql = String.Concat(mysql, " AND Dim2 = @Dim2 ");
            if (!string.IsNullOrEmpty(wfOrderPurc.Dim3)) mysql = String.Concat(mysql, " AND Dim3 = @Dim3 ");
            if (!string.IsNullOrEmpty(wfOrderPurc.Dim4)) mysql = String.Concat(mysql, " AND Dim4 = @Dim4 ");
            if (wfOrderPurc.BillTo != 0) mysql = String.Concat(mysql, " AND su_AddressID = @BillTo");
            mysql = String.Concat(mysql, " order by PurcID desc ");

            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Class", SqlDbType.Int).Value = orderClass;
            comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wfOrderPurc.Dim1) ? DBNull.Value : (object)wfOrderPurc.Dim1);
            comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wfOrderPurc.Dim2) ? DBNull.Value : (object)wfOrderPurc.Dim2);
            comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wfOrderPurc.Dim3) ? DBNull.Value : (object)wfOrderPurc.Dim3);
            comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wfOrderPurc.Dim4) ? DBNull.Value : (object)wfOrderPurc.Dim4);
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = ((wfOrderPurc.BillTo == 0) ? DBNull.Value : (object)wfOrderPurc.BillTo);
            //wfOrderPurc.InvoiceDebitnote = InvDeb.DebitNote;

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.PurcID = (Int32)myr["PurcID"];
                item.OrderNo = (Int32)((myr["OrderNo"] == DBNull.Value) ? 0 : myr["OrderNo"]);
                item.InvoiceNo = (Int64)((myr["invoiceNo"] == DBNull.Value) ? 0 : myr["invoiceNo"]);
                item.VoucherNo = (Int32)((myr["VoucherNo"] == DBNull.Value) ? 0 : myr["VoucherNo"]);
                item.InvoiceDebitnote = (((Int32)myr["delreFactor"] < 0) ? InvDeb.DebitNote : InvDeb.invoice);
                item.AddressString = myr["AdddressText"].ToString();
                item.Total = (Decimal)myr["T_Total"];
                item.TotalVatEx = (Decimal)myr["T_VAT"];
                item.TotalVatIn = (Decimal)myr["T_IncVat"];
                item.InvoiceDate = (DateTime)((myr["InvDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["InvDate"]);
                item.ShipDate = (DateTime)((myr["ShipDate"] == DBNull.Value) ? DateTime.MaxValue : myr["ShipDate"]);
                item.PayDate = (DateTime)((myr["DueDate"] == DBNull.Value) ? DateTime.MaxValue : myr["DueDate"]);
                items.Add(item);
                item = new OrderPurchaseItem();
            }
            conn.Close();
            return "OK";
        }

        public int get_Purcid_by_ordreno(long orderno, ref string errstr)
        {
            string retstr = "OK";
            int PurcID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(PurcID),0) FROM tr_purc WHERE CompID = @CompID AND orderno = @OrderNo  ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@OrderNo", SqlDbType.Int).Value = orderno;
            try
            {
                conn.Open();
                PurcID = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }

            return PurcID;
        }

        public string Orderpu_add(ref OrderPurchase wfOrderPurc, ref int O_PurcID, int s_Class)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string errStr = "err";
            SqlCommand comm = new SqlCommand("dbo.wf_web_addOrderPu_02", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_Invoice_adrID", SqlDbType.Int).Value = wfOrderPurc.BillTo;
            comm.Parameters.Add("@P_Ship_adrID", SqlDbType.Int).Value = wfOrderPurc.ShipTo;
            comm.Parameters.Add("@P_Category", SqlDbType.Int).Value = wfOrderPurc.Category;
            comm.Parameters.Add("@O_PurcID", SqlDbType.Int).Direction = ParameterDirection.Output;
            //comm.Parameters.Add("@P_Class ", SqlDbType.Int).Value = s_Class;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                wfOrderPurc.PurcID = (Int32)myr["PurcID"];
                wfOrderPurc.InvoiceNo = System.Convert.ToInt64(myr["InvoiceNo"]);  //?!? 'normal' MyOrder.InvoiceNo = (long)myr["InvoiceNo"] does not work here
                if (string.IsNullOrEmpty(wfOrderPurc.Dim1)) wfOrderPurc.Dim1 = myr["Dim1"].ToString();
                if (string.IsNullOrEmpty(wfOrderPurc.Dim2)) wfOrderPurc.Dim2 = myr["Dim2"].ToString();
                if (string.IsNullOrEmpty(wfOrderPurc.Dim3)) wfOrderPurc.Dim3 = myr["Dim3"].ToString();
                if (string.IsNullOrEmpty(wfOrderPurc.Dim4)) wfOrderPurc.Dim4 = myr["Dim4"].ToString();
                wfOrderPurc.Currency = myr["Currency"].ToString();
                wfOrderPurc.Language = myr["language"].ToString();
                if (wfOrderPurc.seller == 0) wfOrderPurc.seller = (Int32)myr["SellerID"];
                O_PurcID = (Int32)myr["PurcID"];
            }
            conn.Close();
            errStr = s_Class.ToString();
            return errStr;

        }

        public int Orderpu_Update(int PurcID, ref OrderPurchase MyOrder, int s_Class, Boolean BlindUpdate)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            int err = 0;
            int invdeb = 1;

            if (MyOrder.InvoiceDebitnote == InvDeb.DebitNote)
            {
                invdeb = -1;
            }

            if ((!string.IsNullOrEmpty(MyOrder.Dim1)) || (!string.IsNullOrEmpty(MyOrder.Dim2)) || (!string.IsNullOrEmpty(MyOrder.Dim3)) || (!string.IsNullOrEmpty(MyOrder.Dim4)))
            {
                Order_AddDim(MyOrder.Dim1, MyOrder.Dim2, MyOrder.Dim3, MyOrder.Dim4);
            }
            //Order_AddSalesman(MyOrder.salesman);

            DateTime OldDate = new DateTime(1900, 1, 1);
            DateTime minSqlDate = new DateTime(1753, 1, 1);

            if (DateTime.Compare(MyOrder.InvoiceDate, OldDate) < 0) MyOrder.InvoiceDate = OldDate;
            if (DateTime.Compare(MyOrder.OrderDate, OldDate) < 0) MyOrder.OrderDate = OldDate;
            if (DateTime.Compare(MyOrder.ShipDate, OldDate) <= 0) MyOrder.ShipDate = MyOrder.InvoiceDate;
            //Dim MyData As DataSet = new DataSet
            string mysql = " Update tr_purc set orderno = @OrderNo, orderdate = @OrderDate,InvDate = @InvDate,ShipDate = @ShipDate, sellerID = isnull(@sellerID,sellerID), Category = @Category, EAN = @EAN, UserID = isnull(@IntRef,UserID), ExtRef = @ExtRef,  Dim1 =  @Dim1, Dim2 = @Dim2, Dim3 = @Dim3, Dim4 = @Dim4,  ";
            mysql = String.Concat(mysql, " text_1 = @text1, text_2 = @text2, text_3 = @text3, CreInvFactor = @invdeb, su_addressID = @BillTo, sh_addressID = @ShipTo, TermsOfPayment = @TermsOfPayment, DueDate = @PayDate, ContactPerson = @ContactPerson, ContID = @ContID, ");
            if (BlindUpdate == false) mysql = string.Concat(mysql, " AccountingCost = @AccountingCost, timeChanged = getdate() ");
            mysql = String.Concat(mysql, " WHERE CompID = @CompID AND PurcID = @PurcID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@PurcID", SqlDbType.Int).Value = PurcID;
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = MyOrder.BillTo;
            comm.Parameters.Add("@ShipTo", SqlDbType.Int).Value = MyOrder.ShipTo;
            comm.Parameters.Add("@OrderNo", SqlDbType.BigInt).Value = MyOrder.OrderNo;
            //comm.Parameters.Add("@Calendar", SqlDbType.Int).Value = MyOrder.Calendar;
            comm.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = ((MyOrder.OrderDate < minSqlDate) ? DBNull.Value : (object)MyOrder.OrderDate);
            comm.Parameters.Add("@InvDate", SqlDbType.DateTime).Value = ((MyOrder.InvoiceDate < minSqlDate) ? DBNull.Value : (object)MyOrder.InvoiceDate);
            comm.Parameters.Add("@ShipDate", SqlDbType.DateTime).Value = ((MyOrder.ShipDate < minSqlDate) ? DBNull.Value : (object)MyOrder.ShipDate);
            comm.Parameters.Add("@PayDate", SqlDbType.DateTime).Value = ((MyOrder.PayDate < minSqlDate) ? DBNull.Value : (object)MyOrder.PayDate);
            //comm.Parameters.Add("@Salesmann", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.salesman) ? DBNull.Value : (object)MyOrder.salesman);
            comm.Parameters.Add("@ContactPerson", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(MyOrder.ContactPerson) ? DBNull.Value : (object)MyOrder.ContactPerson);
            comm.Parameters.Add("@TermsOfPayment", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.TermsOfPayment) ? DBNull.Value : (object)MyOrder.TermsOfPayment);
            comm.Parameters.Add("@sellerID", SqlDbType.Int).Value = MyOrder.seller;
            comm.Parameters.Add("@Category", SqlDbType.Int).Value = MyOrder.Category;
            comm.Parameters.Add("@EAN", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.EAN) ? DBNull.Value : (object)MyOrder.EAN);
            //comm.Parameters.Add("@requisition", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.requisition) ? DBNull.Value : (object)MyOrder.requisition);
            //comm.Parameters.Add("@trace", SqlDbType.NVarChar, 512).Value = (string.IsNullOrEmpty(MyOrder.Trace) ? DBNull.Value : (object)MyOrder.Trace);
            comm.Parameters.Add("@IntRef", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.IntRef) ? DBNull.Value : (object)MyOrder.IntRef);
            comm.Parameters.Add("@ExtRef", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.ExtRef) ? DBNull.Value : (object)MyOrder.ExtRef);
            comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.Dim1) ? DBNull.Value : (object)MyOrder.Dim1);
            comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.Dim2) ? DBNull.Value : (object)MyOrder.Dim2);
            comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.Dim3) ? DBNull.Value : (object)MyOrder.Dim3);
            comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.Dim4) ? DBNull.Value : (object)MyOrder.Dim4);
            comm.Parameters.Add("@text1", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(MyOrder.text_1) ? DBNull.Value : (object)MyOrder.text_1);
            comm.Parameters.Add("@text2", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(MyOrder.text_2) ? DBNull.Value : (object)MyOrder.text_2);
            comm.Parameters.Add("@text3", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(MyOrder.text_3) ? DBNull.Value : (object)MyOrder.text_3);
            comm.Parameters.Add("@invdeb", SqlDbType.Int).Value = invdeb;
            //comm.Parameters.Add("@blockReason", SqlDbType.Int).Value = MyOrder.blockReason;
            comm.Parameters.Add("@LocationID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.LocationID) ? DBNull.Value : (object)MyOrder.LocationID);
            //comm.Parameters.Add("@PrePaidDate", SqlDbType.DateTime).Value = ((MyOrder.PaidDate < minSqlDate) ? DBNull.Value : (object)MyOrder.PaidDate);    PrePaidDate = @PrePaidDate, 
            comm.Parameters.Add("@ContID", SqlDbType.Int).Value = MyOrder.ContID;
            comm.Parameters.Add("@AccountingCost", SqlDbType.NVarChar, 250).Value = (string.IsNullOrEmpty(MyOrder.AccountingCost) ? DBNull.Value : (object)MyOrder.AccountingCost);

            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            MyOrder.PurcID = PurcID;

            //if (MyOrder.OrderNo == 0) && (s_Class " [200 , 300] ) {

            //if ((MyOrder.OrderNo == 0 && s_Class == 200) || (MyOrder.OrderNo == 0 && s_Class == 300))
            //{
            //    order_new_orderNumber(SaleID);
            //}

            return err;
        }


        public string Orderpu_add_item(int PurcID, OrderLinePurc lineItem, ref int O_LiID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            O_LiID = 0;

            if (!string.IsNullOrEmpty(lineItem.ItemID) || !string.IsNullOrEmpty(lineItem.EAN))
            {
                SqlCommand comm = new SqlCommand("wf_web_AddOrderItemPu", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_PurcID", SqlDbType.Int).Value = PurcID;
                comm.Parameters.Add("@P_ItemID", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineItem.ItemID) ? DBNull.Value : (object)lineItem.ItemID));
                //comm.Parameters.Add("@P_EAN", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineItem.EAN) ? DBNull.Value : (object)lineItem.EAN));
                comm.Parameters.Add("@P_Price", SqlDbType.Money).Value = lineItem.CostPrice;
                comm.Parameters.Add("@P_qty", SqlDbType.Int).Value = lineItem.Qty;
                //comm.Parameters.Add("@P_GroupFi", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineItem.GroupFi) ? DBNull.Value : (object)lineItem.GroupFi));
                comm.Parameters.Add("@O_LiID", SqlDbType.Int).Direction = ParameterDirection.Output;
                comm.Parameters.Add("@P_ItemDescription", SqlDbType.NVarChar, 2000).Value = ((string.IsNullOrEmpty(lineItem.ItemDesc) ? DBNull.Value : (object)lineItem.ItemDesc));
                conn.Open();
                comm.ExecuteNonQuery();
                O_LiID = (Int32)comm.Parameters["@O_LiID"].Value;
                conn.Close();
            }
            lineItem.Liid = O_LiID;
            return retstr;
        }

        public string Orderpu_Item_Update(int SaleID, OrderLinePurc lineItem)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            if (lineItem.Liid > 0)
            {
                if ((lineItem.Dim1 != String.Empty) || (lineItem.Dim2 != String.Empty) || (lineItem.Dim3 != String.Empty) || (lineItem.Dim4 != String.Empty))
                {
                    Order_AddDim(lineItem.Dim1, lineItem.Dim2, lineItem.Dim3, lineItem.Dim4);
                }
                //                string mysql = "Update tr_purc_LineItems set AddInformation = @AddInf,DiscountProc = @DiscountProc,Unit = @Unit, Batch = @Batch, Dim1 = @Dim1, Dim2 = @Dim2, Dim3 = @Dim3, Dim4 = @Dim4, LineAmount = @LineAmount, LineVat = @LineVat, LineVatBase = @LineVatBase, AllowanceCharge = @AllowanceCharge, VatIncl = @VatIncl, VAT_perc = @VatPerc, UNSPSC=@UNSPSC, AccountingCost=@AccountingCost, LinePrice=@LinePrice  ";
                string mysql = "Update tr_purc_LineItems set AddInformation = @AddInf,DiscountProc = @DiscountProc,Unit = @Unit, Batch = @Batch, Dim1 = @Dim1, Dim2 = @Dim2, Dim3 = @Dim3, Dim4 = @Dim4, VatIncl = @VatIncl, VAT_perc = @VatPerc  ";
                mysql = String.Concat(mysql, " WHERE CompID = @CompID AND PurcID = @PurcID AND LiID = @LiID");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@LiID", SqlDbType.Int).Value = lineItem.Liid;
                comm.Parameters.Add("@Unit", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(lineItem.Unit) ? DBNull.Value : (object)lineItem.Unit);
                comm.Parameters.Add("@Batch", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(lineItem.Batch) ? DBNull.Value : (object)lineItem.Batch);
                comm.Parameters.Add("@DiscountProc", SqlDbType.Int).Value = lineItem.DiscountProc;
                comm.Parameters.Add("@AddInf", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(lineItem.AddInformation) ? DBNull.Value : (object)lineItem.AddInformation);
                comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(lineItem.Dim1) ? DBNull.Value : (object)lineItem.Dim1);
                comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(lineItem.Dim2) ? DBNull.Value : (object)lineItem.Dim2);
                comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(lineItem.Dim3) ? DBNull.Value : (object)lineItem.Dim3);
                comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(lineItem.Dim4) ? DBNull.Value : (object)lineItem.Dim4);
                //comm.Parameters.Add("@LineAmount", SqlDbType.Money).Value = lineItem.LineAmount;
                //comm.Parameters.Add("@LineVat", SqlDbType.Money).Value = lineItem.LineVat;
                //comm.Parameters.Add("@LineVatBase", SqlDbType.Money).Value = lineItem.LineVatBase;
                //comm.Parameters.Add("@AllowanceCharge", SqlDbType.Bit).Value = lineItem.AllowanceCharge;
                comm.Parameters.Add("@VatIncl", SqlDbType.Bit).Value = lineItem.VatIncl;
                comm.Parameters.Add("@VatPerc", SqlDbType.Money).Value = lineItem.vat_perc;
                //comm.Parameters.Add("@UNSPSC", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(lineItem.UNSPSC) ? DBNull.Value : (object)lineItem.UNSPSC);
                //comm.Parameters.Add("@AccountingCost", SqlDbType.NVarChar, 250).Value = (string.IsNullOrEmpty(lineItem.AccountingCost) ? DBNull.Value : (object)lineItem.AccountingCost);
                //comm.Parameters.Add("@LinePrice", SqlDbType.Money).Value = lineItem.LinePrice;

                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return retstr;
        }

        public string Orderpu_add_payment(int PurcID, OrderPayment mypa)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            string mysql = " INSERT tr_purc_payment (CompID, PurcID, meansOfPayment, amount, Currency, amountConverted, PaymentRef, OrderID, ToCapture, CardNo, TicketID, Merchant, CurrencyDibs, PaidDate) ";
            mysql = String.Concat(mysql, " values (@CompID, @PurcID, @meansOfPayment, @amount, @Currency, @amountConverted, @PaymentRef, @OrderID, @ToCapture, @CardNo, @TicketID, @Merchant, @CurrencyDibs, @PaidDate) ");
            try
            {
                SqlCommand Comm = new SqlCommand(mysql, conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = PurcID; // (string.IsNullOrEmpty(
                Comm.Parameters.Add("@meansOfPayment", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.meansOfPayment) ? DBNull.Value : (object)mypa.meansOfPayment);
                Comm.Parameters.Add("@amount", SqlDbType.Money).Value = mypa.amount;
                Comm.Parameters.Add("@Currency", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.Currency) ? DBNull.Value : (object)mypa.Currency);
                Comm.Parameters.Add("@amountConverted", SqlDbType.Money).Value = mypa.amountConverted;
                Comm.Parameters.Add("@PaymentRef", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(mypa.PaymentRef) ? DBNull.Value : (object)mypa.PaymentRef);
                Comm.Parameters.Add("@OrderID", SqlDbType.Int).Value = mypa.OrderID;
                Comm.Parameters.Add("@ToCapture", SqlDbType.Int).Value = mypa.ToCapture;
                Comm.Parameters.Add("@CardNo", SqlDbType.NVarChar, 200).Value = (string.IsNullOrEmpty(mypa.CardNo) ? DBNull.Value : (object)mypa.CardNo);
                Comm.Parameters.Add("@TicketID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.TicketID) ? DBNull.Value : (object)mypa.TicketID);
                Comm.Parameters.Add("@Merchant", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.Merchant) ? DBNull.Value : (object)mypa.Merchant);
                Comm.Parameters.Add("@CurrencyDibs", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.CurrencyDibs) ? DBNull.Value : (object)mypa.CurrencyDibs);
                Comm.Parameters.Add("@PaidDate", SqlDbType.DateTime).Value = ((mypa.PaidDate < minSqlDate) ? DBNull.Value : (object)mypa.PaidDate); //(string.IsNullOrEmpty(mypa.CurrencyDibs) ? DBNull.Value : (object)mypa.CurrencyDibs);
                conn.Open();
                Comm.ExecuteNonQuery();
                conn.Close();
            }

            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public bool orderPurc_is_Open(int PurcID)
        {
            bool OrderClosed = false;
            int OrderClass = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(Class),0) from tr_purc where CompID = @CompID AND PurcID = @PurcID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@PurcID", SqlDbType.Int).Value = PurcID;
            conn.Open();
            OrderClass = (Int32)comm.ExecuteScalar();
            conn.Close();
            if ((OrderClass < 9000) && (OrderClass > 0)) OrderClosed = true;
            return OrderClosed;
        }
        public string OrderPurc_add_payment(int PurcID, OrderPurcPayment mypa)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            string mysql = " INSERT tr_purc_payment (CompID, PurcID, meansOfPayment, amount, Currency, amountConverted, PaymentRef, OrderID, CardNo, Merchant, CurrencyDibs, PaidDate) ";
            mysql = String.Concat(mysql, " values (@CompID, @PurcID, @meansOfPayment, @amount, @Currency, @amountConverted, @PaymentRef, @OrderID, @CardNo, @Merchant, @CurrencyDibs, @PaidDate) ");
            try
            {
                SqlCommand Comm = new SqlCommand(mysql, conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@PurcID", SqlDbType.Int).Value = PurcID; // (string.IsNullOrEmpty(
                Comm.Parameters.Add("@meansOfPayment", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.meansOfPayment) ? DBNull.Value : (object)mypa.meansOfPayment);
                Comm.Parameters.Add("@amount", SqlDbType.Money).Value = mypa.amount;
                Comm.Parameters.Add("@Currency", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.Currency) ? DBNull.Value : (object)mypa.Currency);
                Comm.Parameters.Add("@amountConverted", SqlDbType.Money).Value = mypa.amountConverted;
                Comm.Parameters.Add("@PaymentRef", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(mypa.PaymentRef) ? DBNull.Value : (object)mypa.PaymentRef);
                Comm.Parameters.Add("@OrderID", SqlDbType.Int).Value = mypa.OrderID;
                Comm.Parameters.Add("@CardNo", SqlDbType.NVarChar, 200).Value = (string.IsNullOrEmpty(mypa.CardNo) ? DBNull.Value : (object)mypa.CardNo);
                Comm.Parameters.Add("@Merchant", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.Merchant) ? DBNull.Value : (object)mypa.Merchant);
                Comm.Parameters.Add("@CurrencyDibs", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mypa.CurrencyDibs) ? DBNull.Value : (object)mypa.CurrencyDibs);
                Comm.Parameters.Add("@PaidDate", SqlDbType.DateTime).Value = ((mypa.PaidDate < minSqlDate) ? DBNull.Value : (object)mypa.PaidDate); //(string.IsNullOrEmpty(mypa.CurrencyDibs) ? DBNull.Value : (object)mypa.CurrencyDibs);
                conn.Open();
                Comm.ExecuteNonQuery();
                conn.Close();
            }

            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string orderPurc_load_Payments(int PurcID, ref IList<OrderPurcPayment> payments)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            OrderPurcPayment payment = new OrderPurcPayment();

            try
            {
                string mysql = "Select * FROM tr_purc_payment WHERE CompID = @CompID AND PurcID = @PurcID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@PurcID", SqlDbType.Int).Value = PurcID;

                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    payment.meansOfPayment = myr["meansOfPayment"].ToString();
                    payment.amount = (decimal)((myr["amount"] == DBNull.Value) ? 0 : myr["amount"]);
                    payment.Currency = myr["Currency"].ToString();
                    payment.amountConverted = (decimal)((myr["amountConverted"] == DBNull.Value) ? 0 : myr["amountConverted"]);
                    payment.OrderID = (Int32)((myr["OrderID"] == DBNull.Value) ? 0 : myr["OrderID"]);
                    payment.PaymentRef = myr["PaymentRef"].ToString();
                    payment.CardNo = myr["CardNo"].ToString();
                    payment.Merchant = myr["Merchant"].ToString();
                    payment.CurrencyDibs = myr["CurrencyDibs"].ToString();
                    payment.PaidDate = (DateTime)myr["PaidDate"];
                    payments.Add(payment);
                    payment = new OrderPurcPayment();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public int Purchase_Lookup(ref OrderPurchase WfOrderPurc, ref int puCount)
        {
            int purcID = 0;
            puCount = 0;
            if (WfOrderPurc.PurcID > 0)
            {
                puCount = 1;
                purcID = WfOrderPurc.PurcID;
            }
            else
            {
                int puClass = 0;
                if (WfOrderPurc.OrderType == PurchaseOrderTypes.Inquery) puClass = 500;
                if (WfOrderPurc.OrderType == PurchaseOrderTypes.Order) puClass = 1000;
                if (WfOrderPurc.OrderType == PurchaseOrderTypes.Invoice) puClass = 2000;
                if (WfOrderPurc.OrderType == PurchaseOrderTypes.InvoiceClosed) puClass = 9000;

                int puInvDeb = 0;
                if (WfOrderPurc.InvoiceDebitnote == InvDeb.invoice) puInvDeb = 1;
                if (WfOrderPurc.InvoiceDebitnote == InvDeb.DebitNote) puInvDeb = -1;

                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT max(PurcID) as PurcID, count(*) as wf_Count  FROM tr_purc Where CompID = @CompID  ";
                //if (!string.IsNullOrEmpty(wfadr.ImportID)) mysql = string.Concat(mysql, " AND ImportID = @ImportID ");
                if (WfOrderPurc.InvoiceNo > 0) mysql = string.Concat(mysql, " AND InvoiceNo = @InvoiceNo ");
                if (WfOrderPurc.OrderNo > 0) mysql = string.Concat(mysql, " AND OrderNo = @OrderNo ");
                if (WfOrderPurc.VoucherNo > 0) mysql = string.Concat(mysql, " AND VoucherNo = @VoucherNo ");
                if (WfOrderPurc.BillTo > 0) mysql = string.Concat(mysql, " AND Su_AddressID = @BillTo ");
                if (WfOrderPurc.ShipTo > 0) mysql = string.Concat(mysql, " AND Sh_AddressID = @ShipTo ");
                if (puClass > 0) mysql = string.Concat(mysql, " AND Class = @Class ");
                if (puInvDeb != 0) mysql = string.Concat(mysql, " AND DelReFactor = @DelReFactor ");

                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                //comm.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfadr.Account)) ? DBNull.Value : (object)wfadr.Account);
                comm.Parameters.Add("@InvoiceNo", SqlDbType.Int).Value = WfOrderPurc.InvoiceNo;
                comm.Parameters.Add("@OrderNo", SqlDbType.Int).Value = WfOrderPurc.OrderNo;
                comm.Parameters.Add("@VoucherNo", SqlDbType.Int).Value = WfOrderPurc.VoucherNo;
                comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = WfOrderPurc.BillTo;
                comm.Parameters.Add("@ShipTo", SqlDbType.Int).Value = WfOrderPurc.ShipTo;
                comm.Parameters.Add("@Class", SqlDbType.Int).Value = puClass;
                comm.Parameters.Add("@DelReFactor", SqlDbType.Int).Value = puInvDeb;

                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    purcID = ((myr["PurcID"] == DBNull.Value) ? 0 : (Int32)myr["PurcID"]);
                    puCount = ((myr["wf_count"] == DBNull.Value) ? 0 : (Int32)myr["wf_count"]);
                }
                conn.Close();
            }
            return purcID;
        }
    }
}