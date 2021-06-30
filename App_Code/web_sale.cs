using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
/// <summary>
/// Implementing part of web-class related to sale
/// </summary>
namespace wfws
{
    public partial class web
    {
        public string order_load(int SaleID, ref OrderSales MyOrder)
        {
            string retstr = "OK";
            Address so_adr = new Address();
            Address sh_adr = new Address();
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT * from tr_sale where CompID = @CompID AND SaleID = @SaleID ";
            try
            {
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    switch ((Int32)myr["Class"]) {
                        case 100: MyOrder.OrderType = SalesOrderTypes.Quotations; break;
                        case 200: MyOrder.OrderType = SalesOrderTypes.Order; break;
                        case 300: MyOrder.OrderType = SalesOrderTypes.RecurringOrder; break;
                        case 400: MyOrder.OrderType = SalesOrderTypes.Invoice; break;
                        case 900: MyOrder.OrderType = SalesOrderTypes.InvoiceClosed; break;
                        default: MyOrder.OrderType = SalesOrderTypes.InvoiceAndOrders; break;
                    }
                    //MyOrder.BillTo = ((myr["so_addressID"] == DBNull.Value) ? 0 : (Int32)myr["so_addressID"]);
                    //MyOrder.ShipTo = ((myr["sh_addressID"] == DBNull.Value) ? 0 : (Int32)myr["sh_addressID"]);
                    //MyOrder.Buyer = ((myr["bu_addressID"] == DBNull.Value) ? 0 : (Int32)myr["bu_addressID"]);
                    MyOrder.BillTo = ((Convert.IsDBNull(myr["so_addressID"])) ? 0 : (Int32)myr["so_addressID"]);
                    MyOrder.ShipTo = ((Convert.IsDBNull(myr["sh_addressID"])) ? 0 : (Int32)myr["sh_addressID"]);
                    MyOrder.Buyer = ((Convert.IsDBNull(myr["bu_addressID"])) ? 0 : (Int32)myr["bu_addressID"]);
                    if ((Int32)myr["CreInvFactor"] == -1) MyOrder.InvoiceCreditnote = InvCre.CreditNote; else MyOrder.InvoiceCreditnote = InvCre.invoice;
                    MyOrder.OrderNo = (Int64)(myr["OrderNo"] == DBNull.Value ? 0 : Convert.ToInt64(myr["OrderNo"]));
                    MyOrder.InvoiceNo = (Int64)(myr["InvoiceNo"] == DBNull.Value ? 0 : Convert.ToInt64(myr["InvoiceNo"]));
                    MyOrder.Currency = myr["Currency"].ToString().ToUpper();
                    MyOrder.Language = myr["Language"].ToString();
                    MyOrder.Calendar = (Int32)((myr["Calendar"] == DBNull.Value) ? 0 : (Int32)myr["Calendar"]);
                    MyOrder.OrderDate = (DateTime) ((myr["OrderDate"] == DBNull.Value) ? DateTime.MinValue : (DateTime)myr["OrderDate"]) ;
                    MyOrder.InvoiceDate = (DateTime)((myr["InvDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["InvDate"]);
                    MyOrder.ShipDate = (DateTime)((myr["ShipDate"] == DBNull.Value) ? DateTime.MaxValue : myr["ShipDate"]);
                    MyOrder.PayDate = (DateTime)((myr["PayDate"] == DBNull.Value) ? DateTime.MaxValue : myr["PayDate"]);
                    MyOrder.StartDate = (DateTime)((myr["RecurringStart"] == DBNull.Value) ? DateTime.MaxValue : (DateTime)myr["RecurringStart"]);
                    MyOrder.EndDate = (DateTime)((myr["RecurringExpire"] == DBNull.Value) ? DateTime.Today : (DateTime)myr["RecurringExpire"]);
                    MyOrder.Category = ((myr["Category"] == DBNull.Value) ? 0 : (Int32)myr["Category"]);
                    MyOrder.salesman = myr["salesman"].ToString();
                    MyOrder.ContID = (Int32)((myr["ContID"] == DBNull.Value) ? 0 : (Int32)myr["ContID"]);
                    MyOrder.TermsOfPayment = myr["TermsOfPayment"].ToString();
                    MyOrder.seller = (Int32)((myr["sellerID"] == DBNull.Value) ? 0 : (Int32)myr["sellerID"]);
                    MyOrder.EAN = myr["EAN"].ToString().Replace(" ", "");
                    MyOrder.requisition = myr["requisition"].ToString();
                    MyOrder.Trace = myr["trace"].ToString();
                    MyOrder.IntRef = myr["UserID"].ToString();
                    MyOrder.ExtRef = myr["ExtRef"].ToString();
                    MyOrder.Dim1 = myr["Dim1"].ToString();
                    MyOrder.Dim2 = myr["Dim2"].ToString();
                    MyOrder.Dim3 = myr["Dim3"].ToString();
                    MyOrder.Dim4 = myr["Dim4"].ToString();
                    MyOrder.text_1 = myr["text_1"].ToString();
                    MyOrder.text_2 = myr["text_2"].ToString();
                    MyOrder.text_3 = myr["text_3"].ToString();
                    MyOrder.LocationID = myr["LocationID"].ToString();
                    MyOrder.Total = (decimal)((myr["T_Total"] == DBNull.Value) ? 0 : (decimal)myr["T_Total"]);
                    MyOrder.TotalVatEx = (decimal)((myr["T_Vat"] == DBNull.Value) ? 0 : (decimal)myr["T_Vat"]);
                    MyOrder.TotalVatIn = (decimal)((myr["T_incVAT"] == DBNull.Value) ? 0 : (decimal)myr["T_incVAT"]);
                    MyOrder.TotalVatBasisEx = (decimal)((myr["T_VATBasis"] == DBNull.Value) ? 0 : (decimal)myr["T_VATBasis"]);
                    MyOrder.TotalVatBasisIn = (decimal)((myr["T_incVATBasis"] == DBNull.Value) ? 0 : (decimal)myr["T_incVATBasis"]);
                    MyOrder.VatAcquisition = (decimal)((myr["T_VatAcq"] == DBNull.Value) ? 0 : (decimal)myr["T_VatAcq"]);
                    MyOrder.VatAcquisitionBasis = (decimal)((myr["T_VatAcqBasis"] == DBNull.Value) ? 0 : (decimal)myr["T_VatAcqBasis"]);
                    MyOrder.TotalInvDiscount = (decimal)((myr["T_InvDiscount"] == DBNull.Value) ? 0 : (decimal)myr["T_InvDiscount"]);
                    MyOrder.TotalTaxAmount = (decimal)((myr["taxAmount"] == DBNull.Value) ? 0 : (decimal)myr["taxAmount"]);
                    MyOrder.AmountRounding = (decimal)((myr["AmountRounding"] == DBNull.Value) ? 0 : (decimal)myr["AmountRounding"]);
                    MyOrder.GuidInv = (Guid)((myr["GuidInv"] == DBNull.Value) ? Guid.Empty : (Guid)myr["GuidInv"]);
                    MyOrder.blockReason = (Int32)((myr["blockReason"] == DBNull.Value) ? 0 : (Int32)myr["blockReason"]);
                    MyOrder.PaymentRef = myr["PaymentRef"].ToString();
                    MyOrder.AmountAllowance = (decimal)((myr["AmountAllowance"] == DBNull.Value) ? 0 : (decimal)myr["AmountAllowance"]);
                    MyOrder.AmountCharge = (decimal)((myr["AmountCharge"] == DBNull.Value) ? 0 : (decimal)myr["AmountCharge"]);
                    MyOrder.AmountLines = (decimal)((myr["AmountLines"] == DBNull.Value) ? 0 : (decimal)myr["AmountLines"]);
                    //MyOrder.AmountPaid = (decimal)((myr["PrePaidAmountTotal"] == DBNull.Value) ? 0 : (decimal)myr["PrePaidAmountTotal"]);
                    //MyOrder.PaidDate = (DateTime)((myr["PrePaidDate"] == DBNull.Value) ? DateTime.Today : (DateTime)myr["PrePaidDate"]);
                    MyOrder.SettleNo = (Int32)(myr["SettleNo"] == DBNull.Value ? 0 : Convert.ToInt64(myr["SettleNo"]));
                    MyOrder.AccountingCost = myr["AccountingCost"].ToString();
                    MyOrder.SettleID = (Int64)(myr["SettleID"] == DBNull.Value ? 0 : Convert.ToInt64(myr["SettleID"]));
                    MyOrder.DeliveryNoteNo = (Int64)(myr["DeliveryNoteNo"] == DBNull.Value ? 0 : Convert.ToInt64(myr["DeliveryNoteNo"]));
                    MyOrder.QuotationNo = (Int64)(myr["QuotationNo"] == DBNull.Value ? 0 : Convert.ToInt64(myr["QuotationNo"]));
                    MyOrder.TermsOfDelivery = (Int32)((myr["TermsOfDelivery"] == DBNull.Value) ? 0 : (Int32)myr["TermsOfDelivery"]);
                    MyOrder.InvoiceReady = (bool)(myr["InvoiceReady"] == DBNull.Value ? false : (bool)myr["InvoiceReady"]);
                 }
                conn.Close();
                Contact_Sale_convert(ref MyOrder);
                get_order_addressInf(ref MyOrder);
                get_contact(ref MyOrder);
                get_contact_UBL(ref MyOrder);
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        //  sales ordre
        public string order_IsValidSaleID(ref int SaleID)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull( Max(saleid),0) FROM tr_Sale WHERE CompID = @CompID AND SaleID = @SaleID";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@saleID", SqlDbType.Int).Value = SaleID;
            conn.Open();
            SaleID = (Int32)comm.ExecuteScalar();
            conn.Close();
            return errStr;
        }
        public string Order_add(ref OrderSales MyOrder, ref int O_SaleID, int s_Class)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string errStr = "err";
            SqlCommand comm = new SqlCommand("dbo.wf_web_addOrder_02", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_Invoice_adrID", SqlDbType.Int).Value = MyOrder.BillTo;
            comm.Parameters.Add("@P_Ship_adrID", SqlDbType.Int).Value = MyOrder.ShipTo;
            comm.Parameters.Add("@P_Category", SqlDbType.Int).Value = MyOrder.Category;
            comm.Parameters.Add("@O_SaleID", SqlDbType.Int).Direction = ParameterDirection.Output;
            comm.Parameters.Add("@P_Class ", SqlDbType.Int).Value = s_Class;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                MyOrder.SaleID = (Int32)myr["SaleID"];
                MyOrder.InvoiceNo = System.Convert.ToInt64(myr["InvoiceNo"]);  //?!? 'normal' MyOrder.InvoiceNo = (long)myr["InvoiceNo"] does not work here
                if (string.IsNullOrEmpty(MyOrder.Dim1)) MyOrder.Dim1 = myr["Dim1"].ToString();
                if (string.IsNullOrEmpty(MyOrder.Dim2)) MyOrder.Dim2 = myr["Dim2"].ToString();
                if (string.IsNullOrEmpty(MyOrder.Dim3)) MyOrder.Dim3 = myr["Dim3"].ToString();
                if (string.IsNullOrEmpty(MyOrder.Dim4)) MyOrder.Dim4 = myr["Dim4"].ToString();
                MyOrder.Currency = myr["Currency"].ToString();
                MyOrder.Language = myr["language"].ToString();
                if (MyOrder.seller == 0) MyOrder.seller = (Int32)myr["SellerID"];
                O_SaleID = (Int32)myr["SaleID"];
            }
            conn.Close();
            errStr = s_Class.ToString();
            return errStr;
        }
        private void order_new_orderNumber(int saleid)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("wf_tr_sale_new_orderno", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleid;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
        }
        public string order_address_associate(ref OrderSales MyOrder)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                SqlCommand comm = new SqlCommand("we_sa_address_associata", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = MyOrder.SaleID;
                comm.Parameters.Add("@P_BillTo", SqlDbType.Int).Value = MyOrder.BillTo;
                comm.Parameters.Add("@P_ShipTo", SqlDbType.Int).Value = MyOrder.ShipTo;
                comm.Parameters.Add("@P_Contact", SqlDbType.Int).Value = DBNull.Value;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public int get_saleid_by_ordreno(long orderno, long InvoiceNo, ref string errstr)
        {
            int saleID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(saleID),0) FROM tr_sale WHERE CompID = @CompID AND orderno = @OrderNo  ";
            if (InvoiceNo > 0) mysql = "SELECT isnull(max(saleID),0) FROM tr_sale WHERE COmpID = @CompID AND invoiceno = @InvNo  ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@OrderNo", SqlDbType.BigInt).Value = orderno;
            comm.Parameters.Add("@InvNo", SqlDbType.BigInt).Value = InvoiceNo;
            try
            {
                conn.Open();
                saleID = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            catch (Exception e) { errstr = e.Message; }
            return saleID;
        }
        public int get_invoiceno_by_saleid(int saleid, ref string errstr, bool returnOrderno = false)
        {
            int RetVal = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT case @getOrderNo when 1 then Orderno else InvoiceNo end  FROM tr_sale WHERE  CompID = @CompID AND saleid = @SaleID";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleid;
            comm.Parameters.Add("@getOrderNo", SqlDbType.Bit).Value = returnOrderno;
            try
            {
                conn.Open();
                RetVal = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            catch (Exception e) { errstr = e.Message; }
            return RetVal;
        }
        public bool IsSuspicious(long SaleID)
        {
            bool retval = false;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT Suspicious FROM tr_sale WHERE COmpID = @CompID AND saleid = @SaleID  ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.BigInt).Value = SaleID;
            try
            {
                conn.Open();
                retval = (bool)comm.ExecuteScalar();
                //retval = (bool)ret;
                conn.Close();
            }
            catch  { retval = false; }
            return retval;
        }
        public int get_saleid_Lookup(ref OrderSales MyOrder, int OrderClass)
        {
            int saleID = 0;
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(saleID),0) FROM tr_sale WHERE COmpID = @CompID AND Class = @Class  ";
            if (MyOrder.OrderNo > 0) mysql = string.Concat(mysql, " AND orderno = @OrderNo ");
            if (MyOrder.InvoiceNo > 0) mysql = string.Concat(mysql, " AND invoiceno = @InvNo  ");
            if (MyOrder.seller > 0) mysql = string.Concat(mysql, " AND Seller = @Seller  ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Class", SqlDbType.Int).Value = OrderClass;
            comm.Parameters.Add("@OrderNo", SqlDbType.BigInt).Value = MyOrder.OrderNo;
            comm.Parameters.Add("@InvNo", SqlDbType.BigInt).Value = MyOrder.InvoiceNo;
            comm.Parameters.Add("@Seller", SqlDbType.Int).Value = MyOrder.seller;
            try
            {
                conn.Open();
                saleID = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return saleID;
        }
        public int get_saleid_from_guid(Guid MyGuid)
        {
            int saleID = 0;
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(saleID),0) FROM tr_sale WHERE COmpID = @CompID AND GuidInv = @GuidInv  ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@GuidInv", SqlDbType.UniqueIdentifier).Value = MyGuid;
            try
            {
                conn.Open();
                saleID = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return saleID;
        }
        public bool order_is_Open(int SaleID)
        {
            bool OrderClosed = false;
            int OrderClass = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(Class),0) from tr_sale where CompID = @CompID AND SaleID = @SaleID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            conn.Open();
            OrderClass = (Int32)comm.ExecuteScalar();
            conn.Close();
            if ((OrderClass < 900) && (OrderClass > 0)) OrderClosed = true;
            return OrderClosed;
        }
        public int getOrderIDByNumber(long orderno, int category, int orderclass)
        {
            int saleID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(saleID),0) FROM tr_sale WHERE CompID = @CompID AND orderno = @OrderNo AND Class = @Class ";
            if (category > 0) mysql = string.Concat(mysql, " AND Category = @Category ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@OrderNo", SqlDbType.BigInt).Value = orderno;
            comm.Parameters.Add("@Category", SqlDbType.Int).Value = category;
            comm.Parameters.Add("@Class", SqlDbType.Int).Value = orderclass;
            conn.Open();
            saleID = (Int32)comm.ExecuteScalar();
            conn.Close();
            return saleID;
        }
        private void Order_AddSalesman(string Salesman)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            if (!string.IsNullOrEmpty(Salesman))
            {
                string mysql = " if not exists (select * from tr_sale_salesmen WHERE CompID = @CompID AND SalesmanID = @salesman)  insert tr_sale_salesmen (CompID,SalesmanID,Name) values (@CompID,@salesman,@salesman) ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@salesman", SqlDbType.NVarChar, 20).Value = Salesman;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
        }
        public string Order_add_item(int SaleID, OrderLine lineItem, ref int O_LiID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            O_LiID = 0;
            if (!string.IsNullOrEmpty(lineItem.ItemID) || !string.IsNullOrEmpty(lineItem.EAN))
            {
                SqlCommand comm = new SqlCommand("wf_web_AddOrderItem_03", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@P_ItemID", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineItem.ItemID) ? DBNull.Value : (object)lineItem.ItemID));
                comm.Parameters.Add("@P_EAN", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineItem.EAN) ? DBNull.Value : (object)lineItem.EAN));
                comm.Parameters.Add("@P_Price", SqlDbType.Money).Value = lineItem.SalesPrice;
                comm.Parameters.Add("@P_qty", SqlDbType.Money).Value = lineItem.Qty;
                comm.Parameters.Add("@P_GroupFi", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(lineItem.GroupFi) ? DBNull.Value : (object)lineItem.GroupFi));
                comm.Parameters.Add("@P_Volume", SqlDbType.Money).Value = lineItem.Volume;
                comm.Parameters.Add("@P_Weight", SqlDbType.Money).Value = lineItem.Weight;
                comm.Parameters.Add("@P_SuggestedRetail", SqlDbType.Money).Value = lineItem.SuggestedRetail;
                comm.Parameters.Add("@P_AddInformation", SqlDbType.NVarChar, 255).Value = ((string.IsNullOrEmpty(lineItem.AddInformation) ? DBNull.Value : (object)lineItem.AddInformation));
                comm.Parameters.Add("@P_Selection", SqlDbType.NVarChar, 255).Value = ((string.IsNullOrEmpty(lineItem.Selection) ? DBNull.Value : (object)lineItem.Selection));
                comm.Parameters.Add("@P_ItemDescription", SqlDbType.NVarChar, 2000).Value = ((string.IsNullOrEmpty(lineItem.ItemDesc) ? DBNull.Value : (object)lineItem.ItemDesc));
                comm.Parameters.Add("@O_LiID", SqlDbType.Int).Direction = ParameterDirection.Output;
                conn.Open();
                comm.ExecuteNonQuery();
                O_LiID = (Int32)comm.Parameters["@O_LiID"].Value;
                conn.Close();
            }
            lineItem.Liid = O_LiID;
            return retstr;
        }
        public string order_load_Item(int SaleID, int LiiD, ref OrderLine item)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                string mysql = "Select saleID,LiiD,ItemID,Unit,Description,SalesPrice,OrderAmount,DiscountProc,OrderQty,QtyPackages ,AddInformation,Style,EAN,ConsumerUnitEAN, ";
                mysql = string.Concat(mysql, "Dim1, Dim2, Dim3,Dim4 ,Substitutable, ");
                mysql = string.Concat(mysql, "  (select isnull(SelectionID,'') + ';'  from tr_inventory_selections_Items tbse where tbse.CompID = tb1.CompID AND tbse.ItemID = tb1.ItemID  order by SelectionID FOR XML PATH(''))   as  Selections ");
                mysql = string.Concat(mysql, " FROM tr_sale_LineItems tb1 WHERE tb1.CompID = @CompID AND tb1.SaleID = @SaleID AND tb1.LiiD = @LiID ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@LiiD", SqlDbType.Int).Value = LiiD;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.SaleID = (Int32)myr["saleID"];
                    item.Liid = (Int32)myr["LiiD"];
                    item.ItemID = myr["ItemID"].ToString();
                    item.Unit = myr["Unit"].ToString();
                    item.ItemDesc = myr["Description"].ToString();
                    item.SalesPrice = (Decimal)myr["SalesPrice"];
                    item.OrderAmount = (Decimal)myr["OrderAmount"];
                    item.OrderAmount = (Decimal)myr["DiscountProc"];
                    item.Qty = (Decimal)myr["OrderQty"];
                    item.QtyPackages = (Decimal)myr["QtyPackages"];
                    item.AddInformation = myr["AddInformation"].ToString();
                    item.Style = myr["Style"].ToString();
                    item.EAN = myr["EAN"].ToString();
                    item.ConsumerUnitEAN = myr["ConsumerUnitEAN"].ToString();
                    item.Dim1 = myr["Dim1"].ToString();
                    item.Dim2 = myr["Dim2"].ToString();
                    item.Dim3 = myr["Dim3"].ToString();
                    item.Dim4 = myr["Dim4"].ToString();
                    item.Substitutable = myr["Substitutable"] == DBNull.Value ? false : (Boolean)myr["Substitutable"];
                    item.Selection = myr["selections"].ToString();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string order_load_Payments(int SaleID, ref IList<OrderPayment> payments)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            OrderPayment payment = new OrderPayment();
            try
            {
                string mysql = "Select * FROM tr_sale_payment WHERE CompID = @CompID AND SaleID = @SaleID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
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
                    payment.ToCapture = (Int32)((myr["ToCapture"] == DBNull.Value) ? 0 : myr["ToCapture"]);
                    payment.CardNo = myr["CardNo"].ToString();
                    payment.TicketID = myr["TicketID"].ToString();
                    payment.Merchant = myr["Merchant"].ToString();
                    payment.CurrencyDibs = myr["CurrencyDibs"].ToString();
                    payment.PaidDate = (DateTime)((myr["PaidDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["PaidDate"]);
                    payment.CardExpDate = (DateTime)((myr["CardExpDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["CardExpDate"]);
                    payment.CardNoMask = myr["CardNoMask"].ToString();
                    payments.Add(payment);
                    payment = new OrderPayment();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string get_default_meansOfPayment()
        {
            string MeansOfPayments;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "Select max(meansOfPayment) FROM tr_sale_meansOfPayment WHERE CompID = @CompID ";
            SqlCommand Comm = new SqlCommand(mysql, conn);
            Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            MeansOfPayments = Comm.ExecuteScalar().ToString();
            conn.Close();
            return MeansOfPayments;
        }
        public string Order_add_payment(int SaleID, OrderPayment mypa)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            if (string.IsNullOrEmpty(mypa.meansOfPayment))  mypa.meansOfPayment = get_default_meansOfPayment();


            DateTime minSqlDate = new DateTime(1753, 1, 1);
            string mysql = " INSERT tr_sale_payment (CompID, SaleID, meansOfPayment, amount, Currency, amountConverted, PaymentRef, OrderID, ToCapture, CardNo, TicketID, Merchant, CurrencyDibs, PaidDate, CardExpDate, CardNoMask) ";
            mysql = String.Concat(mysql, " values (@CompID, @SaleID, @meansOfPayment, @amount, @Currency, @amountConverted, @PaymentRef, @OrderID, @ToCapture, @CardNo, @TicketID, @Merchant, @CurrencyDibs, @PaidDate, @CardExpDate, @CardNoMask) ");
            try
            {
                SqlCommand Comm = new SqlCommand(mysql, conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID; // (string.IsNullOrEmpty(
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
                Comm.Parameters.Add("@CardExpDate", SqlDbType.DateTime).Value = ((mypa.CardExpDate < minSqlDate) ? DBNull.Value : (object)mypa.CardExpDate);
                Comm.Parameters.Add("@CardNoMask", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(mypa.CardNoMask) ? DBNull.Value : (object)mypa.CardNoMask);
                conn.Open();
                Comm.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Order_Item_Update(int SaleID, OrderLine MyORder)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            if (MyORder.Liid > 0)
            {
                if ((MyORder.Dim1 != String.Empty) || (MyORder.Dim2 != String.Empty) || (MyORder.Dim3 != String.Empty) || (MyORder.Dim4 != String.Empty))
                {
                    Order_AddDim(MyORder.Dim1, MyORder.Dim2, MyORder.Dim3, MyORder.Dim4);
                }
                string mysql = "Update tr_sale_LineItems set AddInformation = @AddInf,Unit = isnull(@Unit, unit), Batch = @Batch, Dim1 = @Dim1, Dim2 = @Dim2, Dim3 = @Dim3, Dim4 = @Dim4, LineAmount = @LineAmount, LineVat = @LineVat, LineVatBase = @LineVatBase, AllowanceCharge = @AllowanceCharge, VatIncl = @VatIncl, VAT_perc = @VatPerc, UNSPSC=isnull(@UNSPSC,unspsc), AccountingCost=@AccountingCost, LinePrice=@LinePrice,  ";
                mysql = string.Concat(mysql, " weight = @weight, volume = @Volume, SuggestedRetail = @SuggestedRetail, Selection = @P_Selection, Substitutable = @Substitutable ");
                mysql = String.Concat(mysql, " WHERE CompID = @CompID AND SaleID = @SaleID AND LiID = @LiID ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@LiID", SqlDbType.Int).Value = MyORder.Liid;
                comm.Parameters.Add("@Unit", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyORder.Unit) ? DBNull.Value : (object)MyORder.Unit);
                comm.Parameters.Add("@Batch", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyORder.Batch) ? DBNull.Value : (object)MyORder.Batch);
                comm.Parameters.Add("@AddInf", SqlDbType.NVarChar, 255).Value = (string.IsNullOrEmpty(MyORder.AddInformation) ? DBNull.Value : (object)MyORder.AddInformation);
                comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyORder.Dim1) ? DBNull.Value : (object)MyORder.Dim1);
                comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyORder.Dim2) ? DBNull.Value : (object)MyORder.Dim2);
                comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyORder.Dim3) ? DBNull.Value : (object)MyORder.Dim3);
                comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyORder.Dim4) ? DBNull.Value : (object)MyORder.Dim4);
                comm.Parameters.Add("@LineAmount", SqlDbType.Money).Value = MyORder.LineAmount;
                comm.Parameters.Add("@LinePrice", SqlDbType.Money).Value = MyORder.LinePrice;
                comm.Parameters.Add("@LineVat", SqlDbType.Money).Value = MyORder.LineVat;
                comm.Parameters.Add("@LineVatBase", SqlDbType.Money).Value = MyORder.LineVatBase;
                comm.Parameters.Add("@AllowanceCharge", SqlDbType.Bit).Value = MyORder.AllowanceCharge;
                comm.Parameters.Add("@VatIncl", SqlDbType.Bit).Value = MyORder.VatIncl;
                comm.Parameters.Add("@VatPerc", SqlDbType.Money).Value = MyORder.vat_perc;
                comm.Parameters.Add("@UNSPSC", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyORder.UNSPSC) ? DBNull.Value : (object)MyORder.UNSPSC);
                comm.Parameters.Add("@AccountingCost", SqlDbType.NVarChar, 250).Value = (string.IsNullOrEmpty(MyORder.AccountingCost) ? DBNull.Value : (object)MyORder.AccountingCost);
                comm.Parameters.Add("@volume", SqlDbType.Money).Value = MyORder.Volume;
                comm.Parameters.Add("@weight", SqlDbType.Money).Value = MyORder.Weight;
                comm.Parameters.Add("@SuggestedRetail", SqlDbType.Money).Value = MyORder.SuggestedRetail;
                comm.Parameters.Add("@P_Selection", SqlDbType.NVarChar, 255).Value = ((string.IsNullOrEmpty(MyORder.Selection) ? DBNull.Value : (object)MyORder.Selection));
                comm.Parameters.Add("@Substitutable", SqlDbType.Bit).Value = MyORder.Substitutable;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return retstr;
        }
        public string Order_Item_UpdatePrice(int SaleID, OrderLine lineItem)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            if (lineItem.Liid > 0)
            {
                string mysql = "Update tr_sale_LineItems set OrderQty = @P_qty, ShipQty = @P_qty, SalesPrice = @P_Price, DiscountProc = @DiscountProc ";
                mysql = String.Concat(mysql, " WHERE CompID = @CompID AND SaleID = @SaleID AND LiID = @LiID");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@LiID", SqlDbType.Int).Value = lineItem.Liid;
                comm.Parameters.Add("@P_Price", SqlDbType.Money).Value = lineItem.SalesPrice;
                comm.Parameters.Add("@DiscountProc", SqlDbType.Int).Value = lineItem.DiscountProc;
                comm.Parameters.Add("@P_qty", SqlDbType.Int).Value = lineItem.Qty;
                conn.Open();
                comm.ExecuteNonQuery();
                mysql = " UPDATE tr_sale_LineItems set OrderAmount = ROUND(orderqty * QtyPackages * SalesPrice ,2), OrderAmountCu = ROUND(orderqty * QtyPackages * SalesPrice ,2) ";
                mysql = String.Concat(mysql, " WHERE CompID = @CompID AND SaleID = @SaleID AND LiID = @LiID");
                comm.CommandText = mysql;
                comm.ExecuteNonQuery();
                mysql = " UPDATE tr_sale_LineItems set Discount = ROUND(orderAmount * discountProc / 100 ,2) ";
                mysql = String.Concat(mysql, " WHERE CompID = @CompID AND SaleID = @SaleID AND LiID = @LiID");
                comm.CommandText = mysql;
                comm.ExecuteNonQuery();
                mysql = " UPDATE tr_sale_LineItems set OrderAmount = OrderAmount - Discount, OrderAmountCu = OrderAmount - Discount  ";
                mysql = String.Concat(mysql, " WHERE CompID = @CompID AND SaleID = @SaleID AND LiID = @LiID");
                comm.CommandText = mysql;
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return retstr;
        }
        public string Order_Line_Delete(int SaleID, OrderLine lineItem)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = "OK";
            if (lineItem.Liid > 0)
            {
                string mysql = "Delete from tr_sale_LineItems  WHERE CompID = @CompID AND SaleID = @SaleID AND LiID = @LiID";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@LiID", SqlDbType.Int).Value = lineItem.Liid;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return retstr;
        }

        public void order_calculate(int SaleID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("dbo.wf_tr_sale_calc_lines", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
            comm.Parameters.Add("@P_Del", SqlDbType.Bit).Value = 0;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
        }
        public void PrepareUBLforCalc(int SaleID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("dbo.wf_tr_sale_calc_lines_UBLWrapUp", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
        }
        public int order_CloseErr(int SaleID, ref string errstr)
        {
            int errno = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("dbo.wf_rs_SaleErr", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
            comm.Parameters.Add("@P_Err", SqlDbType.Int).Direction = ParameterDirection.Output;
            comm.Parameters.Add("@P_ErrStr", SqlDbType.NVarChar, 2000).Direction = ParameterDirection.Output;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            errno = (Int32)comm.Parameters["@P_Err"].Value;
            errstr = comm.Parameters["@P_ErrStr"].Value.ToString();
            return errno;
        }
        public void order_Close(int SaleID)
        {
            if (SaleID > 0) {
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("dbo.wf_tr_sale_CloseInvoice", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
            comm.Parameters.Add("@P_WfCus", SqlDbType.NVarChar, 20).Value = 'C';
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            }
        }

        public void order_delete(int SaleID)
        {
            if (SaleID > 0)
            {
                string mysql = "DELETE From tr_Sale_lot Where CompID = @CompID And SaleID = @SaleID";
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                conn.Open();
                comm.ExecuteNonQuery();
                comm.CommandText = "DELETE From tr_Sale_lot Where CompID = @CompID And SaleID = @SaleID";
                comm.ExecuteNonQuery();
                comm.CommandText = "DELETE FROM tr_Sale_LineItems Where CompID = @CompID And SaleID = @SaleID";
                comm.ExecuteNonQuery();
                comm.CommandText = "DELETE FROM tr_Sale_payment Where CompID = @CompID And SaleID = @SaleID";
                comm.ExecuteNonQuery();
                comm.CommandText = "DELETE FROM tr_sale Where CompID = @CompID And SaleID = @SaleID And Class = 200";
                comm.ExecuteNonQuery();
                conn.Close();
            }
       }

        public void order_Recalc(int SaleID)
        {
            if (SaleID > 0)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand("dbo.wf_tr_Sale_NewPrices", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
                //comm.Parameters.Add("@P_WfCus", SqlDbType.NVarChar, 20).Value = 'C';
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
        }

 






        public void order_save_ubl(int SaleID, string xmldoc)
        {
            if (!string.IsNullOrEmpty(xmldoc))
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " UPDATE tr_sale set oioubl = @oioubl Where CompID = @CompID AND SaleID = @SaleID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@oioubl", SqlDbType.NVarChar, -1).Value = xmldoc;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
        }
        public string order_load_ubl(int SaleID)
        {
            string xmldoc = string.Empty;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = " SELECT oioubl from tr_sale Where CompID = @CompID AND SaleID = @SaleID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                xmldoc = myr["oioubl"].ToString();
            }
            conn.Close();
            return xmldoc;
        }
        public void order_LineItemDelete(int SaleID, string ItemID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            if (ItemID != string.Empty)
            {
                string mysql = "DELETE from tr_sale_lineitems WHERE CompID = @P_CompID AND SaleID = @P_SaleID AND ItemID = @P_ItemID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@P_ItemID", SqlDbType.NVarChar, 20).Value = ItemID;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
        }
        public void order_clear(int saleid, ref string errstr)
        {
            int Saleclass = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT class from tr_sale Where CompID = @CompID AND SaleID = @SaleID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleid;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                Saleclass = (int)myr["class"];
            }
            if (Saleclass > 400)
            {
                errstr = "Invoice closed";
                return;
            }
            if (Saleclass == 300)
            {
                errstr = "Recurring orders can't be cleared";
                return;
            }
            myr.Close();
            comm.CommandText = "DELETE FROM tr_sale_Serialized Where CompID = @CompID AND SaleID = @SaleID ";
            comm.Parameters["@CompID"].Value = compID;
            comm.Parameters["@SaleID"].Value = saleid;
            comm.ExecuteNonQuery();
            comm.CommandText = "DELETE FROM tr_Sale_LineItems Where CompID = @CompID AND SaleID = @SaleID ";
            comm.ExecuteNonQuery();
            if (Saleclass < 400)
            {
                comm.CommandText = "DELETE FROM tr_Sale Where CompID = @CompID AND SaleID = @SaleID ";
                errstr = "Deleted";
            }
            else
            {
                comm.CommandText = "UPDATE tr_sale set So_AddressID = Null, Sh_AddressID = Null Where CompID = @CompID AND SaleID = @SaleID ";
                errstr = "Cleared";
            }
            comm.ExecuteNonQuery();
            conn.Close();
        }
        public int SalesGetCategoryByName(string category)
        {
            int categoryID = 0;
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                if (!string.IsNullOrEmpty(category))
                {
                    string mysql = " select isnull(min(category),0) from tr_sale_categories where CompID = @CompID AND catDescription = @CatDesc ";
                    SqlCommand comm = new SqlCommand(mysql, conn);
                    comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                    comm.Parameters.Add("@CatDesc", SqlDbType.NVarChar, 20).Value = category;
                    conn.Open();
                    categoryID = (Int32)comm.ExecuteScalar();
                    conn.Close();
                }
            }
            catch (Exception e) { retstr = e.Message; }
            return categoryID;
        }
        public string order_load_Items(int SaleID, ref IList<OrderLine> items)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            OrderLine item = new OrderLine();
            try
            {
                string mysql = "Select LiiD,ItemID,Description,EAN,ConsumerUnitEAN,SalesPrice,OrderAmount,DiscountProc,OrderQty,QtyPackages ,AddInformation,VatIncl,Style,GroupFi,Discount,DiscountProc,vat_perc, ";
                mysql = string.Concat(mysql, "Unit,Dim1, Dim2, Dim3,Dim4 ,AllowanceCharge,LineAmount,LineVat,LineVatBase,ShipDate,Substitutable,LineDiscount,UNSPSC,AccountingCost,Batch,LinePrice, ");
                mysql = string.Concat(mysql, "  (select isnull(SelectionID,'') + ';'  from tr_inventory_selections_Items tbse where tbse.CompID = tb1.CompID AND tbse.ItemID = tb1.ItemID  order by SelectionID FOR XML PATH(''))   as  Selections ");
                mysql = string.Concat(mysql, " FROM tr_sale_LineItems tb1 WHERE CompID = @CompID AND SaleID = @SaleID "); //håndtering af forpakning interesserer ikke oio
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.SaleID = SaleID;
                    item.Liid = (Int32)myr["LiiD"];
                    item.ItemID = myr["ItemID"].ToString();
                    item.ItemDesc = myr["Description"].ToString();
                    item.EAN = myr["EAN"].ToString();
                    item.ConsumerUnitEAN = myr["ConsumerUnitEAN"].ToString();
                    item.SalesPrice = (decimal)((myr["SalesPrice"] == DBNull.Value) ? 0 : (decimal)myr["SalesPrice"]);
                    item.OrderAmount = (decimal)((myr["OrderAmount"] == DBNull.Value) ? 0 : (decimal)myr["OrderAmount"]);
                    item.Qty = (decimal)((myr["OrderQty"] == DBNull.Value) ? 0 : (decimal)myr["OrderQty"]);      //håndtering af forpakning interesserer ikke oio
                    item.QtyPackages = (decimal)((myr["QtyPackages"] == DBNull.Value) ? 0 : (decimal)myr["QtyPackages"]);      //håndtering af forpakning interesserer ikke oio, så husk at beregne denne som produktet af disse to.
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
                    item.LineDiscount = (decimal)((myr["LineDiscount"] == DBNull.Value) ? 0 : (decimal)myr["LineDiscount"]);
                    item.UNSPSC = myr["UNSPSC"].ToString();
                    item.AccountingCost = myr["AccountingCost"].ToString();
                    item.Batch = myr["Batch"].ToString();
                    item.LinePrice = (decimal)((myr["LinePrice"] == DBNull.Value) ? 0 : (decimal)myr["LinePrice"]);
                    item.GroupFi = myr["GroupFi"].ToString();
                    item.Discount = (decimal)((myr["Discount"] == DBNull.Value) ? 0 : (decimal)myr["Discount"]);
                    item.DiscountProc = (decimal)((myr["DiscountProc"] == DBNull.Value) ? 0 : (decimal)myr["DiscountProc"]);
                    item.ActualDeliveryDate = (DateTime)((myr["ShipDate"] == DBNull.Value) ? DateTime.MinValue : (DateTime)myr["ShipDate"]);
                    item.Substitutable = myr["Substitutable"] == DBNull.Value ? false : (Boolean)myr["Substitutable"];
                    item.Selection = myr["Selections"].ToString();
                    items.Add(item);
                    item = new OrderLine();
                }
                conn.Close();
                if (CheckHeadAllowance(SaleID, ref item))  //Check for en formastelig bundrabat - puha grissefy. Bør måske kun køres, hvis det er et ubl-load.
                {
                    items.Add(item);
                }
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        private bool CheckHeadAllowance(int SaleID, ref OrderLine item)   //Bundrabathåndtering
        {
            bool retval = false;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "Select * FROM tr_sale WHERE CompID = @CompID AND SaleID = @SaleID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                decimal HeadAllowance = (decimal)((myr["T_InvDiscount"] == DBNull.Value) ? 0 : (decimal)myr["T_InvDiscount"]);
                decimal Vat = (decimal)((myr["T_Vat"] == DBNull.Value) ? 0 : (decimal)myr["T_Vat"]);
                if (HeadAllowance != 0) //negativ rabat = gebyr
                {
                    item.SaleID = SaleID;
                    item.Liid = 999999999;
                    item.ItemID = "Bundrabat";
                    item.ItemDesc = "Bundrabat";
                    item.OrderAmount = -1 * HeadAllowance;
                    item.LineAmount = HeadAllowance;
                    if (Vat!=0) { 
                        item.vat_perc = (decimal)25;
                        item.VatIncl = true;
                    } else { 
                        item.vat_perc = (decimal)0;
                        item.VatIncl = false;
                    }
                    item.AllowanceCharge = true;
                    retval = true;
                }
            }
            conn.Close();
            return retval;
        }
        public int Order_Update(int SaleID, ref OrderSales MyOrder, int s_Class, Boolean BlindUpdate)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            int err = 0;
            int invcr = 1;
            if (MyOrder.InvoiceCreditnote == InvCre.CreditNote)
            {
                invcr = -1;
            }
            if ((!string.IsNullOrEmpty(MyOrder.Dim1)) || (!string.IsNullOrEmpty(MyOrder.Dim2)) || (!string.IsNullOrEmpty(MyOrder.Dim3)) || (!string.IsNullOrEmpty(MyOrder.Dim4)))
            {
                Order_AddDim(MyOrder.Dim1, MyOrder.Dim2, MyOrder.Dim3, MyOrder.Dim4);
            }
            Order_AddSalesman(MyOrder.salesman);
            //DateTime OldDate = new DateTime(1900, 1, 1);
            //DateTime minSqlDate = new DateTime(1753, 1, 1);
            //if (DateTime.Compare(MyOrder.InvoiceDate, OldDate) < 0) MyOrder.InvoiceDate = OldDate;
            //if (DateTime.Compare(MyOrder.OrderDate, OldDate) < 0) MyOrder.OrderDate = OldDate;
            //if (DateTime.Compare(MyOrder.StartDate, OldDate) < 0) MyOrder.StartDate = OldDate;
            //if (DateTime.Compare(MyOrder.EndDate, OldDate) < 0) MyOrder.EndDate = OldDate;
            //if (DateTime.Compare(MyOrder.ShipDate, OldDate) <= 0) MyOrder.ShipDate = MyOrder.InvoiceDate;
            //Dim MyData As DataSet = new DataSet
            string mysql = " Update tr_sale set orderno = @OrderNo, Calendar = @Calendar, orderdate = @OrderDate,RecurringStart = @StartDate ,RecurringExpire = @EndDate,InvDate = @InvDate,ShipDate = @ShipDate, Salesman = @Salesmann,sellerID = isnull(@sellerID,sellerID), Category = @Category, EAN = @EAN, requisition = @requisition, UserID = isnull(@IntRef,UserID), ExtRef = @ExtRef,  Dim1 =  @Dim1, Dim2 = @Dim2, Dim3 = @Dim3, Dim4 = @Dim4,  ";
            mysql = String.Concat(mysql, " text_1 = @text1, text_2 = @text2, text_3 = @text3, CreInvFactor = @invcre, trace = @trace, blockReason = @blockReason, so_addressID = @BillTo, sh_addressID = @ShipTo, TermsOfPayment = @TermsOfPayment, PayDate = @PayDate, ContactPerson = @ContactPerson, ContID = @ContID, AccountingCost = @AccountingCost, InvoiceReady = @InvoiceReady ");
            if (BlindUpdate == false) mysql = string.Concat(mysql, ", timeChanged = getdate() ");
            mysql = String.Concat(mysql, " WHERE CompID = @CompID AND SaleID = @SaleID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = MyOrder.BillTo;
            comm.Parameters.Add("@ShipTo", SqlDbType.Int).Value = MyOrder.ShipTo;
            comm.Parameters.Add("@OrderNo", SqlDbType.BigInt).Value = MyOrder.OrderNo;
            comm.Parameters.Add("@Calendar", SqlDbType.Int).Value = MyOrder.Calendar;
            comm.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = wfsh.ToSqlDateTime(MyOrder.OrderDate); // ((MyOrder.OrderDate < minSqlDate) ? DBNull.Value : (object)MyOrder.OrderDate);
            comm.Parameters.Add("@InvDate", SqlDbType.DateTime).Value = wfsh.ToSqlDateTime(MyOrder.InvoiceDate); // ((MyOrder.InvoiceDate < minSqlDate) ? DBNull.Value : (object)MyOrder.InvoiceDate);
            comm.Parameters.Add("@ShipDate", SqlDbType.DateTime).Value = wfsh.ToSqlDateTime(MyOrder.ShipDate); // ((MyOrder.ShipDate < minSqlDate) ? DBNull.Value : (object)MyOrder.ShipDate);
            comm.Parameters.Add("@PayDate", SqlDbType.DateTime).Value = wfsh.ToSqlDateTime(MyOrder.PayDate); // ((MyOrder.PayDate < minSqlDate) ? DBNull.Value : (object)MyOrder.PayDate);
            comm.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = wfsh.ToSqlDateTime(MyOrder.StartDate); // ((MyOrder.StartDate < minSqlDate) ? DBNull.Value : (object)MyOrder.StartDate);
            comm.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = wfsh.ToSqlDateTime(MyOrder.EndDate); // ((MyOrder.EndDate <= OldDate) ? DBNull.Value : (object)MyOrder.EndDate);
            comm.Parameters.Add("@Salesmann", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.salesman) ? DBNull.Value : (object)MyOrder.salesman);
            comm.Parameters.Add("@ContactPerson", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(MyOrder.ContactPerson) ? DBNull.Value : (object)MyOrder.ContactPerson);
            comm.Parameters.Add("@TermsOfPayment", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.TermsOfPayment) ? DBNull.Value : (object)MyOrder.TermsOfPayment);
            comm.Parameters.Add("@sellerID", SqlDbType.Int).Value = MyOrder.seller;
            comm.Parameters.Add("@Category", SqlDbType.Int).Value = MyOrder.Category;
            comm.Parameters.Add("@EAN", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.EAN) ? DBNull.Value : (object)MyOrder.EAN);
            comm.Parameters.Add("@requisition", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.requisition) ? DBNull.Value : (object)MyOrder.requisition);
            comm.Parameters.Add("@trace", SqlDbType.NVarChar, 512).Value = (string.IsNullOrEmpty(MyOrder.Trace) ? DBNull.Value : (object)MyOrder.Trace);
            comm.Parameters.Add("@IntRef", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.IntRef) ? DBNull.Value : (object)MyOrder.IntRef);
            comm.Parameters.Add("@ExtRef", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.ExtRef) ? DBNull.Value : (object)MyOrder.ExtRef);
            comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.Dim1) ? DBNull.Value : (object)MyOrder.Dim1);
            comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.Dim2) ? DBNull.Value : (object)MyOrder.Dim2);
            comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.Dim3) ? DBNull.Value : (object)MyOrder.Dim3);
            comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.Dim4) ? DBNull.Value : (object)MyOrder.Dim4);
            comm.Parameters.Add("@text1", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(MyOrder.text_1) ? DBNull.Value : (object)MyOrder.text_1);
            comm.Parameters.Add("@text2", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(MyOrder.text_2) ? DBNull.Value : (object)MyOrder.text_2);
            comm.Parameters.Add("@text3", SqlDbType.NVarChar, 2000).Value = (string.IsNullOrEmpty(MyOrder.text_3) ? DBNull.Value : (object)MyOrder.text_3);
            comm.Parameters.Add("@invcre", SqlDbType.Int).Value = invcr;
            comm.Parameters.Add("@blockReason", SqlDbType.Int).Value = MyOrder.blockReason;
            comm.Parameters.Add("@LocationID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(MyOrder.LocationID) ? DBNull.Value : (object)MyOrder.LocationID);
            //comm.Parameters.Add("@PrePaidDate", SqlDbType.DateTime).Value = ((MyOrder.PaidDate < minSqlDate) ? DBNull.Value : (object)MyOrder.PaidDate);    PrePaidDate = @PrePaidDate, 
            comm.Parameters.Add("@ContID", SqlDbType.Int).Value = MyOrder.ContID;
            comm.Parameters.Add("@AccountingCost", SqlDbType.NVarChar, 250).Value = (string.IsNullOrEmpty(MyOrder.AccountingCost) ? DBNull.Value : (object)MyOrder.AccountingCost);
            comm.Parameters.Add("@InvoiceReady",SqlDbType.Bit).Value = MyOrder.InvoiceReady;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            MyOrder.SaleID = SaleID;
            //if (MyOrder.OrderNo == 0) && (s_Class " [200 , 300] ) {
            if ((MyOrder.OrderNo == 0 && s_Class == 200) || (MyOrder.OrderNo == 0 && s_Class == 300))
            {
                order_new_orderNumber(SaleID);
            }
            return err;
        }
        public string order_update_timestamp(int SaleID)
        {
            string retstr = "err";
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "UPDATE tr_sale set TimeChanged = getdate() where CompID = @CompID AND SaleID = @SaleID";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string SalesOrder_Items_get(ref  OrderSales wfOrderSales, ref IList<OrderSalesItem> items, int orderClass)
        {
            string retstr = "err";
            int DebCr = 1;
            if (orderClass<0)
            {
                DebCr = -1;
                orderClass = orderClass * -1;
            }
            if (orderClass==1)
            {
                DebCr = 0;
                orderClass = 900;
            }

            if (orderClass == 0)
            {
                DebCr = 0;
            }
            //if (wfOrderSales.seller.HasValue) wfOrderSales.seller = 0;
            int SellerID = 0;
            if (wfOrderSales.seller > 0) SellerID = wfOrderSales.seller;
            if (wfOrderSales.seller < 0) SellerID = wfOrderSales.seller * -1;
     
            SqlConnection conn = new SqlConnection(conn_str);
            OrderSalesItem item = new OrderSalesItem();
            string mysql = " set concat_null_yields_null OFF SELECT top 10000 SaleID, invoiceNo,OrderNo,SettleID, invdate, orderdate,CreInvFactor,isnull(T_Total,0) as T_Total,  isnull(T_VAT,0) as T_VAT, isnull(T_IncVat,0) as T_IncVat,InvDate,ShipDate,PayDate , ";
            mysql = String.Concat(mysql, " (SELECT ad_account + ' ' + CompanyName + ' ' + Department + ' ' + Address+ ' ' + Address2 + ' ' + postalCode+ ' ' + city FROM ad_addresses tb2 where tb2.CompID = tb1.CompID AND tb2.AddressID = tb1.so_addressID) AdddressText, ");
            mysql = String.Concat(mysql, " isnull((SELECT SUM(amount) FROM tr_sale_payment tb2 WHERE (tb2.CompID = tb1.CompID) AND (tb2.SaleID = tb1.SaleID)),0) AS PrePaid ");
            mysql = String.Concat(mysql, " FROM tr_sale tb1  ");
            mysql = String.Concat(mysql, " WHERE CompID = @CompID ");
            if (orderClass > 0) mysql = String.Concat(mysql, " AND Class = @Class ");
            if (orderClass == 0) mysql = String.Concat(mysql, " AND Class in (200,400,900) ");
            if (DebCr!=0) mysql = String.Concat(mysql, " AND CreInvFactor = ", DebCr.ToString());
            if (wfOrderSales.BillTo != 0) mysql = String.Concat(mysql, " AND so_AddressID = @BillTo");
            if (wfOrderSales.seller > 0) mysql = String.Concat(mysql, " AND SellerID = @SellerID");
            if (wfOrderSales.seller < 0) mysql = String.Concat(mysql, " AND SellerID <> @SellerID");

            if (!string.IsNullOrEmpty(wfOrderSales.Dim1)) mysql = String.Concat(mysql, " AND Dim1 = @Dim1 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim2)) mysql = String.Concat(mysql, " AND Dim2 = @Dim2 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim3)) mysql = String.Concat(mysql, " AND Dim3 = @Dim3 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim4)) mysql = String.Concat(mysql, " AND Dim4 = @Dim4 ");
            if (wfOrderSales.PayDate != DateTime.MinValue) mysql = String.Concat(mysql, " AND PayDate >= @PayDate ");
            mysql = String.Concat(mysql, " order by SaleID desc ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Class", SqlDbType.Int).Value = orderClass;
            comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim1) ? DBNull.Value : (object)wfOrderSales.Dim1));
            comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim2) ? DBNull.Value : (object)wfOrderSales.Dim2));
            comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim3) ? DBNull.Value : (object)wfOrderSales.Dim3));
            comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim4) ? DBNull.Value : (object)wfOrderSales.Dim4));
            comm.Parameters.Add("@PayDate", SqlDbType.DateTime).Value = (wfOrderSales.PayDate == DateTime.MinValue) ? System.Data.SqlTypes.SqlDateTime.MinValue : wfOrderSales.PayDate;
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = ((wfOrderSales.BillTo == 0) ? DBNull.Value : (object)wfOrderSales.BillTo);
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = ((SellerID == 0) ? DBNull.Value : (object)SellerID);

            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SaleID = (Int32)myr["SaleID"];
                if (myr["OrderNo"] == DBNull.Value) item.OrderNo = 0; else item.OrderNo = Convert.ToInt64(myr["OrderNo"]);
                if (myr["InvoiceNo"] == DBNull.Value) item.InvoiceNo = 0; else item.InvoiceNo = Convert.ToInt64(myr["InvoiceNo"]);
                if ((Int32)myr["CreInvFactor"] == -1) item.InvoiceCreditnote = InvCre.CreditNote; else item.InvoiceCreditnote = InvCre.invoice;
                item.AddressString = myr["AdddressText"].ToString();
                item.Total = (Decimal)myr["T_Total"];
                item.TotalVatEx = (Decimal)myr["T_VAT"];
                item.TotalVatIn = (Decimal)myr["T_IncVat"];
                item.PrePaid = (Decimal)myr["PrePaid"];
                item.InvoiceDate = (DateTime)((myr["InvDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["InvDate"]);
                item.ShipDate = (DateTime)((myr["ShipDate"] == DBNull.Value) ? DateTime.MaxValue : myr["ShipDate"]);
                item.PayDate = (DateTime)((myr["PayDate"] == DBNull.Value) ? DateTime.MaxValue : myr["PayDate"]);
                item.SettleID = (Int64)(myr["SettleID"] == DBNull.Value ? 0 : Convert.ToInt64(myr["SettleID"]));
                items.Add(item);
                item = new OrderSalesItem();
            }
            conn.Close();
            return retstr;
        }
        public string SalesOrder_Items_get_unpaid(ref  OrderSales wfOrderSales, ref IList<OrderSalesItem> items, SalesOrderTypes OrderType)
        {
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            OrderSalesItem item = new OrderSalesItem();
            string mysql = " set concat_null_yields_null OFF SELECT TOP (10000) SaleID, InvoiceNo, OrderNo, InvDate, OrderDate, CreInvFactor, ISNULL(T_Total, 0) AS T_Total, ISNULL(T_VAT, 0) AS T_VAT, ISNULL(T_IncVAT, 0) "; 
            mysql = String.Concat(mysql, " AS T_IncVat, InvDate, ShipDate, PayDate, ");
            mysql = String.Concat(mysql, " (SELECT ad_Account + ' ' + CompanyName + ' ' + Department + ' ' + Address + ' ' + Address2 + ' ' + PostalCode + ' ' + City FROM ad_Addresses AS tb2 WHERE (CompID = tb1.CompID) AND (addressID = tb1.So_AddressID)) AS AdddressText, ");
			mysql = String.Concat(mysql, " isnull((SELECT SUM(amount) FROM tr_sale_payment tb2 WHERE (tb2.CompID = tb1.CompID) AND (tb2.SaleID = tb1.SaleID)),0) AS PrePaid ");
            mysql = String.Concat(mysql, " FROM tr_sale AS tb1 ");
            mysql = String.Concat(mysql, " WHERE (CompID = @CompID) AND (Class = 900) AND isnull(settleid,0) = 0 AND (ISNULL(T_Total, 0) + ISNULL(T_VAT, 0) + ISNULL(T_IncVAT, 0) - isnull((SELECT SUM(amount) FROM tr_sale_payment tb2 WHERE (tb2.CompID = tb1.CompID) AND (tb2.SaleID = tb1.SaleID)),0) > 1 ) ");
            if (OrderType == SalesOrderTypes.UnpaidInvoice)
                mysql = String.Concat(mysql, " AND CreInvFactor = 1 ");
            else
                mysql = String.Concat(mysql, " AND CreInvFactor = -1 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim1)) mysql = String.Concat(mysql, " AND Dim1 = @Dim1 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim2)) mysql = String.Concat(mysql, " AND Dim2 = @Dim2 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim3)) mysql = String.Concat(mysql, " AND Dim3 = @Dim3 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim4)) mysql = String.Concat(mysql, " AND Dim4 = @Dim4 ");
            if (wfOrderSales.BillTo != 0) mysql = String.Concat(mysql, " AND so_AddressID = @BillTo");
            mysql = String.Concat(mysql, " order by SaleID desc ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim1) ? DBNull.Value : (object)wfOrderSales.Dim1));
            comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim2) ? DBNull.Value : (object)wfOrderSales.Dim2));
            comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim3) ? DBNull.Value : (object)wfOrderSales.Dim3));
            comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim4) ? DBNull.Value : (object)wfOrderSales.Dim4));
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = ((wfOrderSales.BillTo == 0) ? DBNull.Value : (object)wfOrderSales.BillTo);
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SaleID = (Int32)myr["SaleID"];
                if (myr["OrderNo"] == DBNull.Value) item.OrderNo = 0; else item.OrderNo = Convert.ToInt64(myr["OrderNo"]);
                if (myr["InvoiceNo"] == DBNull.Value) item.InvoiceNo = 0; else item.InvoiceNo = Convert.ToInt64(myr["InvoiceNo"]);
                if ((Int32)myr["CreInvFactor"] == -1) item.InvoiceCreditnote = InvCre.CreditNote; else item.InvoiceCreditnote = InvCre.invoice;
                item.AddressString = myr["AdddressText"].ToString();
                item.Total = (Decimal)myr["T_Total"];
                item.TotalVatEx = (Decimal)myr["T_VAT"];
                item.TotalVatIn = (Decimal)myr["T_IncVat"];
                item.PrePaid = (Decimal)myr["PrePaid"];
                item.InvoiceDate = (DateTime)((myr["InvDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["InvDate"]);
                item.ShipDate = (DateTime)((myr["ShipDate"] == DBNull.Value) ? DateTime.MaxValue : myr["ShipDate"]);
                item.PayDate = (DateTime)((myr["PayDate"] == DBNull.Value) ? DateTime.MaxValue : myr["PayDate"]);
                items.Add(item);
                item = new OrderSalesItem();
            }
            conn.Close();
            return retstr;
        }
        public string SalesStatsItems_get(ref  OrderSales wfOrderSales, ref IList<SalesStatLine> items, int orderClass)
        {
            int creinv = 0;
            decimal valdec = 0;
            string retstr = "err";
            int AdrID = 0;

            SqlConnection conn = new SqlConnection(conn_str);
            SalesStatLine item = new SalesStatLine();
            string mysql = " set concat_null_yields_null OFF SELECT top 10000 tb1.InvoiceNo, tb1.OrderNo, tb1.InvDate, tb1.CreInvFactor, tb2.* ";
            mysql = String.Concat(mysql, " FROM tr_sale tb1 inner join tr_sale_lineitems tb2 on tb2.CompID = tb1.CompID AND tb2.SaleID = tb1.SaleID  ");
            mysql = String.Concat(mysql, " WHERE tb1.CompID = @CompID ");
            if (orderClass > 0) mysql = String.Concat(mysql, " AND tb1.Class = @Class ");
            if (orderClass == 0) mysql = String.Concat(mysql, " AND tb1.Class in (200,400,900) ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim1)) mysql = String.Concat(mysql, " AND Dim1 = @Dim1 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim2)) mysql = String.Concat(mysql, " AND Dim2 = @Dim2 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim3)) mysql = String.Concat(mysql, " AND Dim3 = @Dim3 ");
            if (!string.IsNullOrEmpty(wfOrderSales.Dim4)) mysql = String.Concat(mysql, " AND Dim4 = @Dim4 ");
            if (wfOrderSales.BillTo != 0)
            {
                mysql = String.Concat(mysql, " AND so_AddressID = @AdrID");
                AdrID = wfOrderSales.BillTo;
            } else
            {
                mysql = String.Concat(mysql, " AND sh_AddressID = @AdrID");
                AdrID = wfOrderSales.ShipTo;
            }


            mysql = String.Concat(mysql, " order by tb1.SaleID desc ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Class", SqlDbType.Int).Value = orderClass;
            comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim1) ? DBNull.Value : (object)wfOrderSales.Dim1));
            comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim2) ? DBNull.Value : (object)wfOrderSales.Dim2));
            comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim3) ? DBNull.Value : (object)wfOrderSales.Dim3));
            comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfOrderSales.Dim4) ? DBNull.Value : (object)wfOrderSales.Dim4));
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = ((AdrID == 0) ? DBNull.Value : (object)AdrID);
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                creinv = (Int32)myr["creinvfactor"];
                item.InvoiceDate = (DateTime)((myr["InvDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["InvDate"]);
                if (myr["OrderNo"] == DBNull.Value) item.OrderNo = 0; else item.OrderNo = Convert.ToInt64(myr["OrderNo"]);
                if (myr["InvoiceNo"] == DBNull.Value) item.InvoiceNo = 0; else item.InvoiceNo = Convert.ToInt64(myr["InvoiceNo"]);
                item.SaleID = (Int32)myr["SaleID"];
                item.Liid = (Int32)myr["LiiD"];
                item.ItemID = myr["ItemID"].ToString();
                item.ItemDesc = myr["Description"].ToString();
                item.EAN = myr["EAN"].ToString();
                item.SalesPrice = (decimal)((myr["SalesPrice"] == DBNull.Value) ? 0 : (decimal)myr["SalesPrice"]);
                valdec = (decimal)((myr["OrderAmount"] == DBNull.Value) ? 0 : (decimal)myr["OrderAmount"]);
                item.OrderAmount = valdec * creinv;
                valdec = (decimal)((myr["OrderQty"] == DBNull.Value) ? 0 : (decimal)myr["OrderQty"]);
                item.Qty =  valdec * creinv;
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
                item.Batch = myr["Batch"].ToString();
                item.LinePrice = (decimal)((myr["LinePrice"] == DBNull.Value) ? 0 : (decimal)myr["LinePrice"]);
                item.GroupFi = myr["GroupFi"].ToString();
                item.Discount = (decimal)((myr["Discount"] == DBNull.Value) ? 0 : (decimal)myr["Discount"]);
                item.DiscountProc = (decimal)((myr["DiscountProc"] == DBNull.Value) ? 0 : (decimal)myr["DiscountProc"]);
                items.Add(item);
                item = new SalesStatLine();
            }
            conn.Close();
            return retstr;
        }
        public string Sales_Categories_load(ref IList<SalesCategorie> items)
        {
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            var item = new SalesCategorie();
            string mysql = "SELECT Category, catDescription FROM tr_sale_categories where CompID = @CompID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.Category = (Int32)myr["Category"];
                item.CatDescription = myr["catDescription"].ToString();
                items.Add(item);
                item = new SalesCategorie();
            }
            conn.Close();
            return retstr;
        }
        public string SalesOrder_Items_get_changed(ref DateTime WfTimestamp, OrderSalesItemChanged myItem, ref IList<OrderSalesItemChanged> items, int orderClass, Boolean expanded)
        {
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            OrderSalesItemChanged item = new OrderSalesItemChanged();
            string mysql;
            if (expanded == true)
                mysql = "SELECT tb1.SaleID,isnull(tb1.OrderNo,0) as OrderNo ,tb2.Liid FROM tr_sale tb1 inner join tr_sale_lineitems tb2 on tb2.CompID = tb1.CompID AND tb2.SaleID = tb1.SaleID ";
            else
                mysql = "SELECT SaleID, isnull(OrderNo,0) as OrderNo, 0 as Liid FROM tr_sale tb1 ";
            mysql = String.Concat(mysql, " WHERE tb1.CompID = @CompID AND tb1.Class = @Class AND tb1.TimeChanged is not null AND TimeChanged > @WfTimestamp ");
            if (!string.IsNullOrEmpty(myItem.Dim1)) mysql = String.Concat(mysql, " AND Dim1 = @Dim1 ");
            if (!string.IsNullOrEmpty(myItem.Dim2)) mysql = String.Concat(mysql, " AND Dim2 = @Dim2 ");
            if (!string.IsNullOrEmpty(myItem.Dim3)) mysql = String.Concat(mysql, " AND Dim3 = @Dim3 ");
            if (!string.IsNullOrEmpty(myItem.Dim4)) mysql = String.Concat(mysql, " AND Dim4 = @Dim4 ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Class", SqlDbType.Int).Value = orderClass;
            comm.Parameters.Add("@WfTimestamp", SqlDbType.DateTime).Value = WfTimestamp;
            comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(myItem.Dim1) ? DBNull.Value : (object)myItem.Dim1));
            comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(myItem.Dim2) ? DBNull.Value : (object)myItem.Dim2));
            comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(myItem.Dim3) ? DBNull.Value : (object)myItem.Dim3));
            comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(myItem.Dim4) ? DBNull.Value : (object)myItem.Dim4));
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SaleID = (Int32)myr["SaleID"];
                item.OrderNo = Convert.ToInt64(myr["OrderNo"]);
                item.Liid = (Int32)myr["Liid"];
                items.Add(item);
                item = new OrderSalesItemChanged();
            }
            conn.Close();
            return retstr;
        }
        public string SalesOrder_Items_get_Factoring(ref IList<OrderSalesItemChanged> items)
        {
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            OrderSalesItemChanged item = new OrderSalesItemChanged();
            int ListID = SalesOrder_Factoring_get_listID();
            string mysql = "SELECT SaleID, 0 as OrderNo, @ListID as Liid from pa_payment_collection Where CompID = @CompID AND ListID = @ListID AND Class = 0 ";
            // mysql = "SELECT SaleID, isnull(OrderNo,0) as OrderNo, 0 as Liid FROM tr_sale tb1 ";
            // mysql = String.Concat(mysql, " where tb1.CompID = @CompID AND tb1.Class = 900 AND isnull(tb1.SettleID,0) = 0 AND tb1.Printed = 0 AND ");
            // mysql = String.Concat(mysql, " exists ( SELECT * from ac_Companies_Sellers tb2  WHERE tb2.CompID = tb1.CompID AND tb1.SellerID = tb2.SellerID AND tb2.ExtSellerID = 2) ");
            //select tb1.saleID, tb2.orderno , 0 as Liid from  pa_payment_collection tb1 inner join tr_sale tb2 on tb2.CompID = tb1.CompID AND tb2.SaleID = tb1.SaleID 
            // where tb1.CompID = @CompID AND tb1.ListID = @ListID AND tb1.OperationState = 0
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ListID", SqlDbType.Int).Value = ListID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SaleID = (Int32)myr["SaleID"];
                item.OrderNo = Convert.ToInt64(myr["OrderNo"]);
                item.Liid = (Int32)myr["Liid"];
                items.Add(item);
                item = new OrderSalesItemChanged();
            }
            conn.Close();
            return retstr;
        }
        public string SalesOrder_Factoring_confirm(int listID)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "UPDATE pa_payment_collection set Class = 1 , OperationState = 1 WHERE CompID = @CompID AND ListID = @ListID AND  Class = 0 ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ListID", SqlDbType.Int).Value = listID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return retstr;
        }
        public string SalesOrder_Items_get_B2BBackbone(ref IList<OrderSalesItemChanged> items)
        {
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            OrderSalesItemChanged item = new OrderSalesItemChanged();
            //int ListID = SalesOrder_B2BBackbone_get_listID();
            //string mysql = "SELECT SaleID, 0 as OrderNo, @ListID as Liid from pa_payment_collection Where CompID = @CompID AND ListID = @ListID AND Class = 0 ";
            string mysql = "SELECT pc.SaleID, 0 as OrderNo, pc_head.ListID as Liid FROM pa_payment_collection AS pc INNER JOIN pa_payment_collection_head AS pc_head ON pc.CompID = pc_head.CompID AND pc.ListID = pc_head.ListID WHERE (pc_head.CompID = @CompID) AND (pc_head.Class = 20) AND (pc.Class = 0) ORDER BY pc.ListID, pc.SaleID";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
//            comm.Parameters.Add("@ListID", SqlDbType.Int).Value = ListID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.SaleID = (Int32)myr["SaleID"];
                item.OrderNo = Convert.ToInt64(myr["OrderNo"]);
                item.Liid = (Int32)myr["Liid"];
                items.Add(item);
                item = new OrderSalesItemChanged();
            }
            conn.Close();
            return retstr;
        }
        public string SalesOrder_B2BBackbone_confirm(int SaleID)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "UPDATE pa_payment_collection set Class = 1 , OperationState = 1 WHERE CompID = @CompID AND SaleID = @SaleID AND  Class = 0 ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            string mysql2 = "UPDATE tr_sale set Printed = 1 WHERE CompID = @CompID AND SaleID = @SaleID AND  Printed = 0 ";
            SqlCommand comm2 = new SqlCommand(mysql2, conn);
            comm2.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm2.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
            conn.Open();
            comm.ExecuteNonQuery();
            comm2.ExecuteNonQuery();
            conn.Close();
            return retstr;
        }
        public int SalesOrder_create_CreditNote(int SaleID)
        {
            string retstr = "err";
            int NewSaleID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                SqlCommand comm = new SqlCommand("dbo.wf_tr_sale_createCreditNote_01", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@O_SaleID", SqlDbType.Int).Direction = ParameterDirection.Output;
                conn.Open();
                comm.ExecuteNonQuery();
                NewSaleID = (int)comm.Parameters["@O_SaleID"].Value;
            }
            catch (Exception e) { retstr = e.Message; }
            return NewSaleID;
        }
        public int SalesOrder_To_Invoice(int SaleID, DateTime InvDate) 
        {
            //wf_tr_copy_orderInvoice     we_sale_copy_orderInvoice
            string retstr = "err";
            int NewSaleID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                SqlCommand comm = new SqlCommand("dbo.we_sale_copy_orderInvoice", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@P_AddToSaleID", SqlDbType.Int).Value = 0; //never add lineitems to an existing invoice.
                comm.Parameters.Add("@P_DelNote", SqlDbType.Int).Value = 0; //never delivery note
                comm.Parameters.Add("@P_InvDate", SqlDbType.DateTime).Value = InvDate;
                comm.Parameters.Add("@O_SaleID", SqlDbType.Int).Direction = ParameterDirection.Output;
                comm.Parameters.Add("@O_Return", SqlDbType.Int).Direction = ParameterDirection.Output;
                conn.Open();
                comm.ExecuteNonQuery();
                NewSaleID = (int)comm.Parameters["@O_SaleID"].Value;
            }
            catch (Exception e) { retstr = e.Message; }
            return NewSaleID;
        }
        private int SalesOrder_Factoring_get_listID()
        {
            int listID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(ListID),0)  FROM pa_payment_collection_head WHERE CompID = @CompID AND Class = 10";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            listID = (Int32)comm.ExecuteScalar();
            conn.Close();
            return listID;
        }
        private int SalesOrder_B2BBackbone_get_listID()
        {       //Tages ud af brug, da den kun returnerer senest dannede liste. Man ønsker alle ikke-afsluttede elementer.
            int listID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT isnull(max(ListID),0)  FROM pa_payment_collection_head WHERE CompID = @CompID AND Class = 20";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            listID = (Int32)comm.ExecuteScalar();
            conn.Close();
            return listID;
        }
        public int salesGetCategory(string myCategory)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            int category = 0;
            if (!string.IsNullOrEmpty(myCategory))
            {
                string mysql = "select isnull(min(category),0) from  tr_sale_categories where CompID = @CompID AND catDescription = @CDesc ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@CDesc", SqlDbType.NVarChar, 20).Value = wfsh.Left(myCategory, 20);
                conn.Open();
                category = (int)comm.ExecuteScalar();
                conn.Close();
            }
            return category;
        }
        public string salesUblsave(int SaleID, string ublstring)
        {
            string retstr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "UPDATE tr_sale set oioubl = @oioubl where CompID = @CompID AND saleID = @saleID";
            if (!string.IsNullOrEmpty(ublstring))
            {
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@oioubl", SqlDbType.NVarChar, -1).Value = ublstring;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return retstr;
        }
        public DateTime SalesDuedateGet(DateTime invdate, string TermsOfPayments)
        {
            DateTime dueDate = invdate;
            if (!string.IsNullOrEmpty(TermsOfPayments))
            {
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand("wf_tr_sale_PayDay", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_InvDate", SqlDbType.DateTime).Value = invdate;
                comm.Parameters.Add("@P_Payment", SqlDbType.NVarChar, 20).Value = TermsOfPayments;
                comm.Parameters.Add("@P_DueDate", SqlDbType.DateTime).Direction = ParameterDirection.Output;
                conn.Open();
                comm.ExecuteNonQuery();
                dueDate = (DateTime)comm.Parameters["@P_DueDate"].Value;
                conn.Close();
            }
            // [wf_tr_sale_PayDay]   @P_CompID Int, @P_InvDate datetime,  @P_Payment nvarchar(20),  @P_DueDate datetime OUTPUT    AS
            return dueDate;
        }
        public void get_order_addressInf(ref OrderSales MyOrder)
        {
            if (MyOrder.BillTo != 0)
            {
                string mysql = "select EAN,isnull(ContID,0) as ContID, EndpointType from  ad_addresses WHERE CompID = @CompID AND AddressID = @AddressID";
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = MyOrder.BillTo;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    if (MyOrder.ContID == 0) MyOrder.ContID = (int)myr["ContID"];
                    MyOrder.UBLEndpointScheme = myr["EndpointType"].ToString();        //EndpointType same as EndpointScheme (mix-up in Winfinance)
                    MyOrder.UBLEndpointID = myr["EAN"].ToString();
                }
                conn.Close();
            }
        }
        public void get_contact(ref OrderSales MyOrder)
        {
            if (MyOrder.ContID != 0)
            {
                string mysql = "select ContactName from  ad_contacts WHERE CompID = @CompID AND ContID = @ContID";
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ContID", SqlDbType.Int).Value = MyOrder.ContID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    // 'initials = myr("initials").ToString()
                    // 'AccountingCost = myr("AccountingCost").ToString()
                    MyOrder.ContactPerson = myr["ContactName"].ToString();
                    MyOrder.UBLContactName = MyOrder.ContactPerson;
                }
                conn.Close();
            }
        }
        public void get_contact_UBL(ref OrderSales MyOrder)
        {
            if (MyOrder.BillTo != 0)
            {
                string mysql = "SELECT tb2.ContactName , tb1.initials,tb1.AccountingCost,tb1.email,Tb1.EAN, Tb1.EndpointScheme ";
                mysql = string.Concat(mysql, "from ad_contacts_adr tb1 inner join ad_contacts tb2 on tb1.CompID = tb2.CompID AND tb1.ContID = tb2.ContID ");
                mysql = string.Concat(mysql, "Where tb1.CompID = @CompID AND tb1.AddressID = @AddressID AND (tb1.ContID = @ContID OR @ContID = 0) AND (@ContID <> 0 OR isnull(tb1.UBLDefault,0) <> 0)");
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ContID", SqlDbType.Int).Value = MyOrder.ContID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = MyOrder.BillTo;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    MyOrder.Initials = myr["initials"].ToString();
                    if (string.IsNullOrEmpty(MyOrder.AccountingCost)) MyOrder.AccountingCost = myr["AccountingCost"].ToString();
                    MyOrder.UBLContactName = myr["ContactName"].ToString();
                    MyOrder.UBLemail = myr["email"].ToString();
                    if ((string.IsNullOrEmpty(MyOrder.UBLEndpointID) || MyOrder.UBLEndpointID == "NONE")) {
                        MyOrder.UBLEndpointID = myr["EAN"].ToString();
                        MyOrder.UBLEndpointScheme = myr["EndpointScheme"].ToString();
                    }
                }
                conn.Close();
            }
            if (string.IsNullOrEmpty(MyOrder.Initials)) MyOrder.Initials = "";
            if (string.IsNullOrEmpty(MyOrder.UBLContactName)) MyOrder.UBLContactName = "";
        }
        public void Contact_Sale_convert(ref OrderSales MyOrder)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            int ContID = 0;
            if (MyOrder.SaleID > 0 && MyOrder.ContID == 0)
            {
                SqlCommand comm = new SqlCommand("we_Sale_contact_convert", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = MyOrder.SaleID;
                comm.Parameters.Add("@Contid", SqlDbType.Int).Direction = ParameterDirection.Output;
                conn.Open();
                comm.ExecuteNonQuery();
                ContID = (int)comm.Parameters["@Contid"].Value;
                conn.Close();
            }
        }
        public int SalesGetSellerByName(string seller)
        {
            int categoryID = 0;
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                if (!string.IsNullOrEmpty(seller))
                {
                    string mysql = " select isnull(min(SellerID),0) from ac_Companies_Sellers where CompID = @CompID AND Description = @seller ";
                    SqlCommand comm = new SqlCommand(mysql, conn);
                    comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                    comm.Parameters.Add("@seller", SqlDbType.NVarChar, 20).Value = seller;
                    conn.Open();
                    categoryID = (Int32)comm.ExecuteScalar();
                    conn.Close();
                }
            }
            catch (Exception e) { retstr = e.Message; }
            return categoryID;
        }
        public string order_calendar_block(int SaleID, int CalendarID, DateTime blockDate, string UserID)
        {
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                SqlCommand comm = new SqlCommand("dbo.we_sale_calendar_block", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@CalendarID", SqlDbType.Int).Value = CalendarID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@BlockDate", SqlDbType.DateTime).Value = (DateTime)blockDate;
                comm.Parameters.Add("@UserID", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(UserID)) ? DBNull.Value : (object)UserID);
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string order_calendar_block_upto(int SaleID, int CalendarID, DateTime blockDate, string UserID)
        {
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                SqlCommand comm = new SqlCommand("dbo.we_sale_calendar_block_upto", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@CalendarID", SqlDbType.Int).Value = CalendarID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@BlockDate", SqlDbType.DateTime).Value = (DateTime)blockDate;
                comm.Parameters.Add("@UserID", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(UserID)) ? DBNull.Value : (object)UserID);
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string order_calendar_unblock(int SaleID, int CalendarID, DateTime blockDate)
        {
            string retstr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            try
            {
                SqlCommand comm = new SqlCommand("DELETE from ac_Companies_calendars_items_sa where CompID = @CompID AND CalendarID = @CalendarID AND item = @Item AND SaleID = @SaleID And Type > 0 ", conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@CalendarID", SqlDbType.Int).Value = CalendarID;
                comm.Parameters.Add("@SaleID", SqlDbType.Int).Value = SaleID;
                comm.Parameters.Add("@Item", SqlDbType.DateTime).Value = (DateTime)blockDate;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
                order_update_timestamp(SaleID);
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public void SalesOrder_MarkPaid()
        {
            string mysql = "UPDATE tb1  set SettleID = isnull((SELECT max(SettleID) FROM fi_years_items tb2 WHERE tb2.CompID = tb1.CompID AND ";
            mysql = string.Concat(mysql, "  tb2.SourceID = 1 AND tb2.SourceRef = tb1.SaleID ),0) ");
            mysql = string.Concat(mysql, " FROM tr_sale tb1 WHERE tb1.CompID = @P_CompID AND tb1.Class = 900 AND (tb1.SettleID = 0 OR tb1.SettleID is null) ");
            string mysql2 = " UPDATE tb1  set SettleID = Invoiceno FROM tr_sale tb1 WHERE tb1.CompID = @P_CompID AND tb1.Class = 900 AND (tb1.SettleID = 0 OR tb1.SettleID is null) AND isnull(T_Total,0) = 0  ";
            string mysql3 = " UPDATE tb1 set  SettleID = Invoiceno FROM tr_sale tb1 WHERE tb1.CompID = @P_CompID AND tb1.Class = 900 AND (tb1.SettleID = 0 OR tb1.SettleID is null) ";
            mysql3 = string.Concat(mysql3, " AND exists (SELECT * FROM fi_years_items tb2 WHERE tb2.CompID = tb1.CompID AND tb2.SourceID = 1 AND tb2.SourceRef = tb1.SaleID) ");
            mysql3 = string.Concat(mysql3, " AND not exists (SELECT * FROM fi_years_items tb2 WHERE tb2.CompID = tb1.CompID AND tb2.SourceID = 1 AND tb2.SourceRef = tb1.SaleID AND DebCr = 1) ");
            SqlConnection Conn = new SqlConnection(conn_str);
            SqlCommand Comm = new SqlCommand(mysql, Conn);
            Comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            Comm.CommandTimeout = 0;
            Conn.Open();
            Comm.ExecuteNonQuery();
            Comm.CommandText = mysql2;
            Comm.ExecuteNonQuery();
            Comm.CommandText = mysql3;
            Comm.ExecuteNonQuery();
            Conn.Close();
        }
    }
}