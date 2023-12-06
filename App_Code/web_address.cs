using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
/// <summary>
/// Implementing part of web-class related to addresses
/// </summary>
namespace wfws
{
    public partial class web
    {
        public string Address_add(Guid AdrGuid, string ad_account, ref int AdrID)
        {
            AdrID = 0;
            string retstr = "err2";
            int errInt = 0;
            try
            {
                if (AdrGuid != Guid.Empty)
                {
                    SqlConnection conn = new SqlConnection(conn_str);
                    SqlCommand comm = new SqlCommand("dbo.wf_wcf_AddCustomer", conn);
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("@AdrGuid", SqlDbType.UniqueIdentifier).Value = AdrGuid;
                    comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                    comm.Parameters.Add("@P_Ad_Account", SqlDbType.NVarChar,20).Value = DBNull.Value;
                    comm.Parameters.Add("@P_AdrID", SqlDbType.Int).Direction = ParameterDirection.Output;
                    comm.Parameters.Add("@P_err", SqlDbType.Int).Direction = ParameterDirection.Output;
                    conn.Open();
                    comm.ExecuteNonQuery();
                    AdrID = ((comm.Parameters["@P_AdrID"].Value == DBNull.Value) ? 0 : (Int32)comm.Parameters["@P_AdrID"].Value);
                    errInt = ((comm.Parameters["@P_err"].Value == DBNull.Value) ? 0 : (Int32)comm.Parameters["@P_err"].Value);
                    conn.Close();
                    retstr = "OK";
                }
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Address_Statement_Load(ref AddressStatement AStatement)
        {
            if (AStatement.AddressID == 0)
                AStatement.AddressID = Address_GetByadAccount(AStatement.adAccount);
            AStatement.FullAddress = new Address();
            AStatement.FullAddress.AddressID = AStatement.AddressID;
            Address_Get(ref AStatement.FullAddress);
            AStatement.TotalCre = 0;    //calculate total before selected period
            AStatement.TotalDeb = 0;
            string retstr = "OK";
            try
            {
                AddressStatementLine CurStat = new AddressStatementLine();
                IList<AddressStatementLine> tempItemsList = new List<AddressStatementLine>();
                if (CheckForBalanceBroughtForward(AStatement.AddressID, AStatement.FromDate, ref CurStat) == 1)
                {
                    tempItemsList.Add(CurStat);
                    CurStat = new AddressStatementLine();
                }
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "SELECT ItemID, Account, EnterDate, Description, CuDebet, CuCredit, SourceID, SourceRef, Voucher, InvoiceNo, DateOfPayment, SettleID FROM fi_Years_Items WHERE (CompID = @CompID) AND (addressID = @AdrID) AND (EnterDate >= @FromDate) AND (EnterDate <= @ToDate) order by itemid desc";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AStatement.AddressID;
                comm.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = wfsh.ToSqlDateTime(AStatement.FromDate);
                comm.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = wfsh.ToSqlDateTime(AStatement.ToDate);
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    CurStat.ItemID = ((myr["ItemID"] == DBNull.Value) ? 0 : (Int32)myr["ItemID"]);
                    CurStat.Account = myr["Account"].ToString();
                    CurStat.Enterdate = (DateTime)myr["EnterDate"];
                    CurStat.Description = myr["Description"].ToString();
                    CurStat.CuDebet = ((myr["CuDebet"] == DBNull.Value) ? 0 : (decimal)myr["CuDebet"]);
                    CurStat.CuCredit = ((myr["CuCredit"] == DBNull.Value) ? 0 : (decimal)myr["CuCredit"]);
                    CurStat.SourceID = ((myr["SourceID"] == DBNull.Value) ? 0 : (int)myr["SourceID"]);
                    CurStat.SourceRef = ((myr["SourceRef"] == DBNull.Value) ? 0 : (int)myr["SourceRef"]);
                    CurStat.Voucher = ((myr["Voucher"] == DBNull.Value) ? 0 : Convert.ToInt64(myr["Voucher"]));
                    CurStat.InvoiceNo = ((myr["InvoiceNo"] == DBNull.Value) ? 0 : Convert.ToInt64(myr["InvoiceNo"]));
                    CurStat.Paydate = ((myr["DateOfPayment"] == DBNull.Value) ? (DateTime)myr["EnterDate"] : (DateTime)myr["DateOfPayment"]);
                    CurStat.SettleID = ((myr["SettleID"] == DBNull.Value) ? 0 : (Int32)myr["SettleID"]);
                    AStatement.TotalCre = AStatement.TotalCre + CurStat.CuCredit;
                    AStatement.TotalDeb = AStatement.TotalDeb + CurStat.CuDebet;
                    tempItemsList.Add(CurStat);
                    CurStat = new AddressStatementLine();
                }
                conn.Close();
                AStatement.StatementLines = tempItemsList.ToArray();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        private int CheckForBalanceBroughtForward(int ThisAddress, DateTime FromDate, ref AddressStatementLine SL)
        {
            int RetVal = 0;
            string mysql = "";
            Decimal BringDebet = 0;
            Decimal BringCredit = 0;
            decimal BringTotal = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            SqlDataReader myr;
            if (FromDate.Day > 1)   //if FromDate is inside the month, calculate sum of amounts since day 1 of month
            {
                DateTime MonthStart = FromDate.AddDays(-1 * (FromDate.Day - 1));
                mysql = "SELECT sum(CuDebet) as SumDebet, Sum(CuCredit) as SumCredit FROM fi_Years_Items WHERE (CompID = @CompID) AND (addressID = @AdrID) AND (EnterDate >= @FromDate) AND (EnterDate <= @ToDate)";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = ThisAddress;
                comm.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = MonthStart;
                comm.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = FromDate;
                conn.Open();
                myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    BringDebet = ((myr["SumDebet"] == DBNull.Value) ? 0 : (decimal)myr["SumDebet"]);
                    BringCredit = ((myr["SumCredit"] == DBNull.Value) ? 0 : (decimal)myr["SumCredit"]);
                }
                conn.Close();
            }
            //Calculate sum of amounts in all predesessing periods
            int PeriodEnd = (FromDate.Year * 100) + FromDate.Month;
            mysql = "SELECT SUM(CuAmount) AS SumAmount FROM fi_Years_Per_Adr WHERE (CompID = @CompID) AND (Period < @ToPeriod) AND (AddressID = @AdrID)";
            SqlCommand comm2 = new SqlCommand(mysql, conn);
            comm2.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm2.Parameters.Add("@AdrID", SqlDbType.Int).Value = ThisAddress;
            comm2.Parameters.Add("@ToPeriod", SqlDbType.Int).Value = PeriodEnd;
            conn.Open();
            myr = comm2.ExecuteReader();
            if (myr.Read())
            {
                BringTotal = ((myr["SumAmount"] == DBNull.Value) ? 0 : (decimal)myr["SumAmount"]);
            }
            conn.Close();
            if ((BringDebet != 0) || (BringCredit != 0) || (BringTotal != 0))
            {
                BringTotal = BringTotal + BringDebet - BringCredit;
                SL.Enterdate = FromDate;
                SL.Account = "Balance brought forward";
                if (BringTotal > 0)
                    SL.CuDebet = BringTotal;
                else
                    SL.CuCredit = -1 * BringTotal;
                SL.BalanceBroughtForward = true;
                RetVal = 1;
            }
            return RetVal;
        }
        public string Address_Statement_LoadOpen(ref AddressStatement AStatement)
        {
            if (AStatement.AddressID == 0)
                AStatement.AddressID = Address_GetByadAccount(AStatement.adAccount);
            AStatement.FullAddress = new Address();
            AStatement.FullAddress.AddressID = AStatement.AddressID;
            Address_Get(ref AStatement.FullAddress);
            string retstr = "OK";
            try
            {
                AddressStatementLine CurStat = new AddressStatementLine();
                IList<AddressStatementLine> tempItemsList = new List<AddressStatementLine>();
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "SELECT ItemID, Account, EnterDate, Description, CuDebet, CuCredit, SourceID, SourceRef, Voucher, InvoiceNo FROM fi_Years_Items WHERE (CompID = @CompID) AND (addressID = @AdrID) AND isnull(SettleID,0)=0";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AStatement.AddressID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    CurStat.ItemID = ((myr["ItemID"] == DBNull.Value) ? 0 : (Int32)myr["ItemID"]);
                    CurStat.Account = myr["Account"].ToString();
                    CurStat.Enterdate = (DateTime)myr["EnterDate"];
                    CurStat.Description = myr["Description"].ToString();
                    CurStat.CuDebet = ((myr["CuDebet"] == DBNull.Value) ? 0 : (decimal)myr["CuDebet"]);
                    CurStat.CuCredit = ((myr["CuCredit"] == DBNull.Value) ? 0 : (decimal)myr["CuCredit"]);
                    CurStat.SourceID = ((myr["SourceID"] == DBNull.Value) ? 0 : (int)myr["SourceID"]);
                    CurStat.SourceRef = ((myr["SourceRef"] == DBNull.Value) ? 0 : (int)myr["SourceRef"]);
                    CurStat.Voucher = ((myr["Voucher"] == DBNull.Value) ? 0 : Convert.ToInt64(myr["Voucher"]));
                    CurStat.InvoiceNo = ((myr["InvoiceNo"] == DBNull.Value) ? 0 : Convert.ToInt64(myr["InvoiceNo"]));
                    AStatement.TotalCre = AStatement.TotalCre + CurStat.CuCredit;
                    AStatement.TotalDeb = AStatement.TotalDeb + CurStat.CuDebet;
                    tempItemsList.Add(CurStat);
                    CurStat = new AddressStatementLine();
                }
                conn.Close();
                AStatement.StatementLines = tempItemsList.ToArray();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public AddressCollectable[] Address_Collectables_Load()
        {
            IList<AddressCollectable> tempItemsList = new List<AddressCollectable>();
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT * FROM ad_Addresses_Collectables WHERE (CompID = @CompID) AND Status > 0";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                AddressCollectable TempItem = new AddressCollectable();
                TempItem.AddressID = (myr["addressID"] == DBNull.Value) ? 0 : Convert.ToInt32(myr["addressID"]);
                TempItem.CollectableFrom = (myr["CollectableFrom"] == DBNull.Value) ? DateTime.Now : Convert.ToDateTime(myr["CollectableFrom"]);
                TempItem.Status = (myr["Status"] == DBNull.Value) ? 0 : Convert.ToInt32(myr["Status"]);
                tempItemsList.Add(TempItem);
            }
            conn.Close();
            return tempItemsList.ToArray();
        }
        public string Address_Collectables_UpdateStatus(int AddressID, int NewStatus)
        {
            string ErrStr = "OK";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "update ad_Addresses_Collectables set status = @status where compid = @Compid and addressid = @Addressid";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@status", SqlDbType.Int).Value = NewStatus;
            comm.Parameters.Add("@Addressid", SqlDbType.Int).Value = AddressID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            return ErrStr;
        }
        public int[] Address_Documents_Load(int AddressID)
        {
            IList<int> tempItemsList = new List<int>();
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT DocID FROM ad_Addresses_Documents WHERE (CompID = @CompID) AND AddressID = @AddressID";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                tempItemsList.Add((myr["DocID"] == DBNull.Value) ? 0 : Convert.ToInt32(myr["DocID"]));
            }
            conn.Close();
            return tempItemsList.ToArray();
        }
        public AddressDocument[] Address_Documents_Get(int AddressID)
        {
            IList<AddressDocument> items = new List<AddressDocument>();
            AddressDocument item = new AddressDocument();
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT DocID, Description, CreateDate, [ContentType], [FileName] FROM ad_Addresses_Documents WHERE (CompID = @CompID) AND AddressID = @AddressID";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.DocID = (int)myr["DocID"];
                item.AddressID = AddressID;
                item.Description = myr["Description"].ToString();
                item.CreateDate = (DateTime)myr["CreateDate"];
                item.ContentType = myr["ContentType"].ToString();
                item.FileName = myr["FileName"].ToString();
                items.Add(item);
                item = new AddressDocument();
            }
            conn.Close();
            return items.ToArray();
        }




        public byte[] Address_Document_Get(int AddressID, int DocumentID, ref string ContentType, ref string Description)
        {
            byte[] Retval= null;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT Description, isnull(ContentType,'image/jpeg') as ContentType, Document FROM ad_Addresses_Documents WHERE (CompID = @CompID) AND AddressID = @AddressID and DocID = @DocID";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            comm.Parameters.Add("@DocID", SqlDbType.Int).Value = DocumentID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                ContentType = myr["ContentType"].ToString();
                Description = myr["Description"].ToString();
                Retval = (byte[])myr["Document"];
            }
            conn.Close();
            return Retval;
        }
        public string Address_Update(int AdrID, ref Address MyAdr, Boolean BlindUpdate)
        {
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            SqlConnection conn = new SqlConnection(conn_str);
            string retstr = wfsh.Left("bbbbb", 4);
            string mysql = "Update ad_addresses set CompanyName = @CompName,Department = @Department, Address = @Address, Address2 = @Address2,LastName = @LastName, ad_Account = isnull(@Account,ad_Account), AddrType = isnull(@AddrType,4), ";
            mysql = string.Concat(mysql, " HouseNumber = @HouseNumber, InHouseMail = @InHouseMail, PostalCode = @PostalCode, City = @City, phone = @Phone, ");
            mysql = string.Concat(mysql, " VATNumber = @VATNumber, ean = @ean, Region = @Region, CountryID = @CountryID,EmailInvoice = @emailInvoice,  Email = @email, Fax = @Fax, ContactPersonmail = @ContactPerson, Category = @Category, ");
            mysql = string.Concat(mysql, " SellerID = @SellerID , Language = @Language, DebtorGroup = @DebtorGroup, TermsOfPaymentDeb = @TermsOfPaymentDeb, CompanyWeb = @CompanyWeb,Notes = @Notes, ImportID = @ImportID,  CreditStop = @CreditStop, internRef = @internRef,  ");
            mysql = string.Concat(mysql, " BankingRegNo = @BankingRegNo, BankingAccount=@BankingAccount, Ticket = @Ticket, TicketEnterDate = @TicketEnterDate, CardExpDate = @CardExpDate, CardNoMask = @CardNoMask, CardType = @CardType ");
            mysql = string.Concat(mysql, " , BIC = @Bic, Iban = @Iban, CreditorNo = @CreditorNo");
            if (BlindUpdate == false) mysql = string.Concat(mysql, ", timeChanged = getdate() ");
            mysql = string.Concat(mysql, " WHERE CompID = @CompID AND AddressID = @AdrID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID; //  ((string.IsNullOrEmpty(wfSeller.Description)) ? DBNull.Value : (object)  wfSeller.Description);
            comm.Parameters.Add("@Account", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.Account)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Account, 20));
            comm.Parameters.Add("@AddrType", SqlDbType.Int).Value = (int)MyAdr.AddrType;
            comm.Parameters.Add("@CompName", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(MyAdr.CompanyName)) ? DBNull.Value : (object)wfsh.Left(MyAdr.CompanyName, 100));
            comm.Parameters.Add("@Department", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(MyAdr.Department)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Department, 100));
            comm.Parameters.Add("@Address", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(MyAdr.Address1)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Address1, 100));
            comm.Parameters.Add("@Address2", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.Address2)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Address2, 50));
            comm.Parameters.Add("@HouseNumber", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.HouseNumber)) ? DBNull.Value : (object)wfsh.Left(MyAdr.HouseNumber, 20));
            comm.Parameters.Add("@InHouseMail", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.InHouseMail)) ? DBNull.Value : (object)wfsh.Left(MyAdr.InHouseMail, 20));
            comm.Parameters.Add("@Region", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty((MyAdr.Region)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Region, 20)));
            comm.Parameters.Add("@CountryID", SqlDbType.NVarChar, 4).Value = ((string.IsNullOrEmpty(MyAdr.CountryID)) ? DBNull.Value : (object)wfsh.Left(MyAdr.CountryID, 4));
            comm.Parameters.Add("@PostalCode", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.PostalCode)) ? DBNull.Value : (object)wfsh.Left(MyAdr.PostalCode, 20));
            comm.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(MyAdr.City)) ? DBNull.Value : (object)wfsh.Left(MyAdr.City, 100));
            comm.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.Phone)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Phone, 20));
            comm.Parameters.Add("@Fax", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.Fax)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Fax, 20));
            comm.Parameters.Add("@email", SqlDbType.NVarChar, 255).Value = ((string.IsNullOrEmpty(MyAdr.email)) ? DBNull.Value : (object)wfsh.Left(MyAdr.email, 255));
            comm.Parameters.Add("@emailInvoice", SqlDbType.NVarChar, 255).Value = ((string.IsNullOrEmpty(MyAdr.emailInvoice)) ? DBNull.Value : (object)wfsh.Left(MyAdr.emailInvoice, 255));
            comm.Parameters.Add("@CompanyWeb", SqlDbType.NVarChar, 200).Value = ((string.IsNullOrEmpty(MyAdr.CompanyWeb)) ? DBNull.Value : (object)wfsh.Left(MyAdr.CompanyWeb, 200));
            comm.Parameters.Add("@VATNumber", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.VATNumber)) ? DBNull.Value : (object)wfsh.Left(MyAdr.VATNumber, 20));
            comm.Parameters.Add("@ean", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.ean)) ? DBNull.Value : (object)wfsh.Left(MyAdr.ean, 20));
            comm.Parameters.Add("@ContactPerson", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.ContactPerson)) ? DBNull.Value : (object)wfsh.Left(MyAdr.ContactPerson, 50));
            comm.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.category)) ? DBNull.Value : (object)wfsh.Left(MyAdr.category, 20));
            comm.Parameters.Add("@SellerID", SqlDbType.Int).Value = MyAdr.SellerID;
            comm.Parameters.Add("@Language", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.Language)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Language, 20));
            comm.Parameters.Add("@DebtorGroup", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.DebtorGroup)) ? DBNull.Value : (object)wfsh.Left(MyAdr.DebtorGroup, 20));
            comm.Parameters.Add("@TermsOfPaymentDeb", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.TermsOfPaymentDeb)) ? DBNull.Value : (object)wfsh.Left(MyAdr.TermsOfPaymentDeb, 100));
            comm.Parameters.Add("@Notes", SqlDbType.NVarChar, -1).Value = ((string.IsNullOrEmpty(MyAdr.Notes)) ? DBNull.Value : (object)MyAdr.Notes);
            comm.Parameters.Add("@ImportID", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.ImportID)) ? DBNull.Value : (object)wfsh.Left(MyAdr.ImportID, 50));
            comm.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(MyAdr.LastName)) ? DBNull.Value : (object)wfsh.Left(MyAdr.LastName, 100));
            comm.Parameters.Add("@internRef", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(MyAdr.internRef)) ? DBNull.Value : (object)wfsh.Left(MyAdr.internRef, 100));
            comm.Parameters.Add("@CreditStop", SqlDbType.Bit).Value = MyAdr.CreditStop;
            comm.Parameters.Add("@BankingRegNo", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.BankingRegNo)) ? DBNull.Value : (object)wfsh.Left(MyAdr.BankingRegNo, 20));
            comm.Parameters.Add("@BankingAccount", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(MyAdr.BankingAccount)) ? DBNull.Value : (object)wfsh.Left(MyAdr.BankingAccount, 20));
            comm.Parameters.Add("@Bic", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.BIC)) ? DBNull.Value : (object)wfsh.Left(MyAdr.BIC, 50));
            comm.Parameters.Add("@Iban", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.IBAN)) ? DBNull.Value : (object)wfsh.Left(MyAdr.IBAN, 50));
            comm.Parameters.Add("@CreditorNo", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.CreditorNo)) ? DBNull.Value : (object)wfsh.Left(MyAdr.CreditorNo, 50));
            comm.Parameters.Add("@Ticket", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.Ticket)) ? DBNull.Value : (object)wfsh.Left(MyAdr.Ticket, 50));
            comm.Parameters.Add("@TicketEnterDate", SqlDbType.DateTime).Value = ((MyAdr.TicketEnterDate < minSqlDate) ? DBNull.Value : (object)MyAdr.TicketEnterDate);
            comm.Parameters.Add("@CardExpDate", SqlDbType.DateTime).Value = ((MyAdr.CardExpDate < minSqlDate) ? DBNull.Value : (object)MyAdr.CardExpDate);
            comm.Parameters.Add("@CardNoMask", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.CardNoMask)) ? DBNull.Value : (object)wfsh.Left(MyAdr.CardNoMask, 50));
            comm.Parameters.Add("@CardType", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(MyAdr.CardType)) ? DBNull.Value : (object)wfsh.Left(MyAdr.CardType, 50));
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            retstr = "OK";
            return retstr;
        }
        public string Address_UpdateHashKey(int AdrID, string HashKey)
        {
            string retstr = wfsh.Left("bbbbb", 4);
            if (!string.IsNullOrEmpty(HashKey))
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "Update ad_addresses set passwordmd5 = @passwordmd5 ";
                mysql = string.Concat(mysql, " WHERE CompID = @CompID AND AddressID = @AdrID ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID; //  ((string.IsNullOrEmpty(wfSeller.Description)) ? DBNull.Value : (object)  wfSeller.Description);
                comm.Parameters.Add("@passwordmd5", SqlDbType.NVarChar, 50).Value = HashKey;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
                retstr = "OK";
            }
            return retstr;
        }
        public string Address_check_account_number(string account, int AdrID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string myAccount = account;
            string mysql = "if (SELECT count(*) FROM ad_addresses WHERE CompID = @compID AND ad_account = @Account AND AddressID <> @AdrID) > 0 ";
            mysql = string.Concat(mysql, " SELECT ad_account from ad_addresses WHERE CompID = @CompID AND AddressID = @AdrID else select @Account ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID;
            comm.Parameters.Add("@Account", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(myAccount)) ? DBNull.Value : (object)wfsh.Left(myAccount, 20));
            conn.Open();
            myAccount = comm.ExecuteScalar().ToString();
            conn.Close();
            return myAccount;
        }
        public string Address_Get(ref Address wfadr)
        {
            string retstr = "err";
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT * FROM ad_addresses WHERE CompID = @CompID AND AddressID = @AdrID";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = wfadr.AddressID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    wfadr.Account = myr["ad_account"].ToString();
                    wfadr.AddrType = ((myr["AddrType"] == DBNull.Value) ? AddressType.Undefined : (AddressType)Enum.ToObject(typeof(AddressType), myr["AddrType"]));                         //(AddressType)(int)myr["AddrType"]   (AddressType)myr["AddrType"]);    Enum.ToObject(typeof(AddressType) , myr["AddrType"])
                    wfadr.Address1 = myr["Address"].ToString();
                    wfadr.Address2 = myr["Address2"].ToString();
                    wfadr.LastName = myr["LastName"].ToString();
                    wfadr.Department = myr["Department"].ToString();
                    wfadr.City = myr["City"].ToString();
                    wfadr.CompanyName = myr["CompanyName"].ToString();
                    wfadr.ContactPerson = myr["ContactPersonMail"].ToString();
                    wfadr.CountryID = myr["CountryID"].ToString();
                    wfadr.Department = myr["Department"].ToString();
                    wfadr.ean = myr["ean"].ToString().Replace(" ", "");
                    wfadr.email = myr["email"].ToString();
                    wfadr.emailInvoice = myr["EmailInvoice"].ToString();
                    wfadr.Fax = myr["Fax"].ToString();
                    wfadr.HouseNumber = myr["HouseNumber"].ToString();
                    wfadr.InHouseMail = myr["InHouseMail"].ToString();
                    wfadr.Phone = myr["Phone"].ToString().Replace(" ", "");
                    wfadr.PostalCode = myr["postalCode"].ToString();
                    wfadr.Region = myr["Region"].ToString();
                    wfadr.AdrGuid = ((myr["AdrGuid"] == DBNull.Value) ? Guid.Empty : (Guid)myr["AdrGuid"]);
                    wfadr.CompanyWeb = myr["CompanyWeb"].ToString();
                    wfadr.category = myr["Category"].ToString();
                    wfadr.SellerID = ((myr["SellerID"] == DBNull.Value) ? 0 : (Int32)myr["SellerID"]);
                    wfadr.Currency = myr["Currency"].ToString();
                    wfadr.Language = myr["Language"].ToString();
                    wfadr.DebtorGroup = myr["DebtorGroup"].ToString();
                    wfadr.TermsOfPaymentDeb = myr["TermsOfPaymentDeb"].ToString();
                    wfadr.Notes = myr["Notes"].ToString();
                    wfadr.ImportID = myr["ImportID"].ToString();
                    wfadr.VATNumber = myr["VatNumber"].ToString().Replace(" ", "");
                    wfadr.PriceIDSales = myr["PriceIDSales"].ToString();
                    wfadr.CreditStop = ((myr["CreditStop"] == DBNull.Value) ? false : (Boolean)myr["CreditStop"]);
                    wfadr.internRef = myr["internRef"].ToString();
                    wfadr.TimeChanged = ((myr["TimeChanged"] == DBNull.Value) ? DateTime.Now : (DateTime)myr["TimeChanged"]);
                    wfadr.EndpointType = myr["EndpointType"].ToString();
                    wfadr.BankingRegNo = myr["BankingRegNo"].ToString();
                    wfadr.BankingAccount = myr["BankingAccount"].ToString();
                    wfadr.IBAN = myr["Iban"].ToString();
                    wfadr.BIC = myr["BIC"].ToString();
                    wfadr.CreditorNo = myr["CreditorNo"].ToString();
                    wfadr.Ticket = myr["Ticket"].ToString();
                    wfadr.TicketEnterDate = (DateTime)((myr["TicketEnterDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["TicketEnterDate"]);
                    wfadr.CardExpDate = (DateTime)((myr["CardExpDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["CardExpDate"]);
                    wfadr.CardNoMask = myr["CardNoMask"].ToString();
                    wfadr.CardType = myr["CardType"].ToString();
                    wfadr.PayPhone = myr["PayPhone"].ToString();
                    LookUp lu = new LookUp();
                    wfadr.CountryISO3166_2 = lu.convCountry(CountryIDType.VehicleCode, CountryIDType.ISO_2, wfadr.CountryID);
                }
                else
                { 
                    wfadr.AddressID = 0;    //nothing found - reset search criteria
                }
                conn.Close();
                retstr = "OK";
                if (string.IsNullOrEmpty(wfadr.EndpointType)) wfadr.EndpointType = "GLN";
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Address_Get_defaults(int AdrID, ref Address wfadr)
        {
            string retstr = "err";
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT ad_account FROM ad_addresses WHERE CompID = @CompID AND AddressID = @AdrID";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    wfadr.Account = myr["ad_account"].ToString();
                }
                conn.Close();
                retstr = "OK";
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public int Address_GetByAdrGuid(Guid AdrGuid)
        {
            int adrID = 0;
            if (AdrGuid != Guid.Empty)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT isnull(max(AddressID),0) FROM ad_addresses Where CompID = @CompID AND AdrGuid = @AdrGuid ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AdrGuid", SqlDbType.UniqueIdentifier).Value = AdrGuid;
                conn.Open();
                adrID = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            return adrID;
        }
        public int Address_GetByImportID(string ImportID)
        {
            int adrID = 0;
            if (ImportID != String.Empty)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT isnull(max(AddressID),0) FROM ad_addresses Where CompID = @CompID AND ImportID = @ImportID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ImportID", SqlDbType.NVarChar, 50).Value = ImportID;
                conn.Open();
                adrID = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            return adrID;
        }
        public int Address_GetByadAccount(string adAccount)
        {
            int adrID = 0;
            if (adAccount != String.Empty)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT isnull(max(AddressID),0) FROM ad_addresses Where CompID = @CompID AND ad_Account = @adAccount ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@adAccount", SqlDbType.NVarChar, 50).Value = adAccount;
                conn.Open();
                adrID = (Int32)comm.ExecuteScalar();
                conn.Close();
            }
            return adrID;
        }
        public int Address_Lookup(ref Address wfadr, ref int wfcount)
        {
            int adrID = 0; 
            wfcount = 0;
            if (wfadr.AddressID > 0)
            {
                wfcount = 1;
                adrID = wfadr.AddressID;
            }
            else
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT max(AddressID) as AdrID, count(*) as wf_Count  FROM ad_addresses Where CompID = @CompID  ";
                if (!string.IsNullOrEmpty(wfadr.ImportID)) mysql = string.Concat(mysql, " AND ImportID = @ImportID ");
                if (!string.IsNullOrEmpty(wfadr.email)) mysql = string.Concat(mysql, " AND email = @email ");
                if (!string.IsNullOrEmpty(wfadr.emailInvoice)) mysql = string.Concat(mysql, " AND email = @emailInvoice ");
                if (!string.IsNullOrEmpty(wfadr.CompanyName)) mysql = string.Concat(mysql, " AND CompanyName like @Name + '%' ");
                if (!string.IsNullOrEmpty(wfadr.Department)) mysql = string.Concat(mysql, " AND Department like @Department + '%' ");
                if (!string.IsNullOrEmpty(wfadr.Account)) mysql = string.Concat(mysql, " AND ad_account = @Account ");
                if (!string.IsNullOrEmpty(wfadr.LastName)) mysql = string.Concat(mysql, " AND LastName like  @LastName  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.Address1)) mysql = string.Concat(mysql, " AND Address like @Address  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.HouseNumber)) mysql = string.Concat(mysql, " AND HouseNumber = @HouseNumber ");
                if (!string.IsNullOrEmpty(wfadr.Address2)) mysql = string.Concat(mysql, " AND Address2 like @Address2  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.City)) mysql = string.Concat(mysql, " AND City like @City  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.PostalCode)) mysql = string.Concat(mysql, " AND PostalCode like  @PostalCode  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.CountryID)) mysql = string.Concat(mysql, " AND CountryID = @CountryID ");
                if (!string.IsNullOrEmpty(wfadr.Phone)) mysql = string.Concat(mysql, " AND Phone = @Phone "); 
                if (!string.IsNullOrEmpty(wfadr.ean)) mysql = string.Concat(mysql, " AND ean = @Ean ");
                if (wfadr.AdrGuid != Guid.Empty) mysql = string.Concat(mysql, " AND AdrGuid = @AdrGuid  ");
                if (wfadr.AddrType != AddressType.Undefined) mysql = string.Concat(mysql, " AND AddrType = @AddrType ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@Account", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfadr.Account)) ? DBNull.Value : (object)wfadr.Account);
                comm.Parameters.Add("@ImportID", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(wfadr.ImportID)) ? DBNull.Value : (object)wfsh.Left(wfadr.ImportID, 50));
                comm.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.email)) ? DBNull.Value : (object)wfadr.email);
                comm.Parameters.Add("@emailInvoice", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.emailInvoice)) ? DBNull.Value : (object)wfadr.emailInvoice);
                comm.Parameters.Add("@phone", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.Phone)) ? DBNull.Value : (object)wfadr.Phone);
                comm.Parameters.Add("@ean", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.ean)) ? DBNull.Value : (object)wfadr.ean);
                comm.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.CompanyName)) ? DBNull.Value : (object)wfadr.CompanyName);
                comm.Parameters.Add("@Department", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.Department)) ? DBNull.Value : (object)wfadr.Department);
                comm.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.CompanyName)) ? DBNull.Value : (object)wfadr.CompanyName);
                comm.Parameters.Add("@Address", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.Address1)) ? DBNull.Value : (object)wfadr.Address1);
                comm.Parameters.Add("@HouseNumber", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.HouseNumber)) ? DBNull.Value : (object)wfadr.HouseNumber);
                comm.Parameters.Add("@Address2", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.Address2)) ? DBNull.Value : (object)wfadr.Address2);
                comm.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.City)) ? DBNull.Value : (object)wfadr.City);
                comm.Parameters.Add("@PostalCode", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.PostalCode)) ? DBNull.Value : (object)wfadr.PostalCode);
                comm.Parameters.Add("@CountryID", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.CountryID)) ? DBNull.Value : (object)wfadr.CountryID);
                comm.Parameters.Add("@AdrGuid", SqlDbType.UniqueIdentifier).Value = wfadr.AdrGuid;
                comm.Parameters.Add("@AddrType", SqlDbType.Int).Value =  (int)wfadr.AddrType;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                if (myr.Read())
                {
                    adrID = ((myr["AdrID"] == DBNull.Value) ? 0 : (Int32)myr["AdrID"]);
                    wfcount = ((myr["wf_count"] == DBNull.Value) ? 0 : (Int32)myr["wf_count"]);
                }
                conn.Close();
            }
            return adrID;
        }
        public int Address_Login(string UserName, string HashKey)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            int adrID = 0;
            string mysql = " SELECT max(AddressID) as AdrID, count(*) as wf_Count  FROM ad_addresses Where CompID = @CompID AND userName = @UserName AND passwordmd5 = @Password ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@UserName", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Account", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(UserName)) ? DBNull.Value : (object)UserName);
            comm.Parameters.Add("@HashKey", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(HashKey)) ? DBNull.Value : (object)HashKey);
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                adrID = ((myr["AdrID"] == DBNull.Value) ? 0 : (Int32)myr["AdrID"]);
            }
            conn.Close();
            return adrID;
        }
        public string Address_items_load(string PropertyID, ref IList<AddressItem> items)
        {
            string retstr = "err";
            AddressItem item = new AddressItem();
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT tb1.Addressid, tb1.ad_account, tb1.CompanyName, tb1.Address,tb1.Address2, tb1.LastName, tb1.HouseNumber,tb1.City,tb1.PostalCode,Phone,Email,CompanyWeb FROM ad_Addresses tb1 inner join ad_addresses_AddrProp tb2 on tb1.CompID = tb2.CompID AND tb1.AddressID = tb2.AddressID ";
                mysql = string.Concat(mysql, " Where tb2.CompID = @CompID AND tb2.propertyid = @PropertyID ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@PropertyID", SqlDbType.Int).Value = PropertyID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.AddressID = (Int32)myr["AddressID"];
                    item.Account = myr["Ad_Account"].ToString();
                    item.CompanyName = myr["CompanyName"].ToString();
                    item.Address1 = myr["Address"].ToString();
                    item.Address2 = myr["Address2"].ToString();
                    item.HouseNumber = myr["HouseNumber"].ToString();
                    item.PostalCode = myr["PostalCode"].ToString();
                    item.City = myr["City"].ToString();
                    item.Phone = myr["Phone"].ToString();
                    item.Email = myr["Email"].ToString();
                    item.CompanyWeb = myr["CompanyWeb"].ToString();
                    items.Add(item);
                    item = new AddressItem();
                }
                conn.Close();
                retstr = "OK";
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public int[] Address_properties_load(int PropertyID)
        {
            string retstr = "err";
            IList<int> tempItemsList = new List<int>();
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " SELECT AddressID from ad_addresses_AddrProp Where CompID = @CompID AND propertyid = @PropertyID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@PropertyID", SqlDbType.Int).Value = PropertyID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    tempItemsList.Add((myr["AddressID"] == DBNull.Value) ? 0 : Convert.ToInt32(myr["AddressID"]));
                }
                conn.Close();
                retstr = "OK";
            }
            catch (Exception e) { retstr = e.Message; }
            return tempItemsList.ToArray();
        }
        public string Address_items_load_find(ref Address wfadr, ref IList<AddressItem> items)
        {
            string retstr = "err";
            AddressItem item = new AddressItem();
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                //top 1000 fjernet aht futurelink/lauritz. SAS
                string mysql = "SELECT tb1.Addressid, ad_account, CompanyName, Address, Address2,LastName, HouseNumber, City, PostalCode, InternRef,Phone,Email,CompanyWeb FROM ad_Addresses tb1 ";
                mysql = String.Concat(mysql, " Where CompID = @CompID ");
                if (wfadr.AddrType != AddressType.Undefined) mysql = string.Concat(mysql, " AND AddrType = @AddrType ");
                if (!string.IsNullOrEmpty(wfadr.email)) mysql = string.Concat(mysql, " AND email = @email ");
                if (!string.IsNullOrEmpty(wfadr.CompanyName)) mysql = string.Concat(mysql, " AND CompanyName like '%' + @Name + '%' ");
                if (!string.IsNullOrEmpty(wfadr.Department)) mysql = string.Concat(mysql, " AND Department like '%' + @Department + '%' ");
                if (!string.IsNullOrEmpty(wfadr.Account)) mysql = string.Concat(mysql, " AND ad_account = @Account ");
                if (!string.IsNullOrEmpty(wfadr.LastName)) mysql = string.Concat(mysql, " AND LastName like  @LastName  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.Address1)) mysql = string.Concat(mysql, " AND Address like @Address  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.HouseNumber)) mysql = string.Concat(mysql, " AND HouseNumber = @HouseNumber ");
                if (!string.IsNullOrEmpty(wfadr.Address2)) mysql = string.Concat(mysql, " AND Address2 like @Address2  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.City)) mysql = string.Concat(mysql, " AND City like @City  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.PostalCode)) mysql = string.Concat(mysql, " AND PostalCode like  @PostalCode  + '%' ");
                if (!string.IsNullOrEmpty(wfadr.CountryID)) mysql = string.Concat(mysql, " AND CountryID = @CountryID ");
                if (!string.IsNullOrEmpty(wfadr.Phone)) mysql = string.Concat(mysql, " AND Phone = @Phone ");
                if (!string.IsNullOrEmpty(wfadr.internRef)) mysql = string.Concat(mysql, " AND InternRef = @InternRef ");
                if (!string.IsNullOrEmpty(wfadr.VATNumber)) mysql = string.Concat(mysql, " AND VATNumber = @VATNumber ");
                if (!string.IsNullOrEmpty(wfadr.category)) mysql = string.Concat(mysql, " AND category = @category ");
                if (!string.IsNullOrEmpty(wfadr.Notes)) {
                    mysql = string.Concat(mysql, " AND (Notes like '%' + @notes + '%' OR exists (select * from ad_addresses_ExtraLines tb2 where tb2.CompID = tb1.CompID AND tb2.AddressID = tb1.AddressID AND Value like '%' + @notes + '%')) ");
                }
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@Account", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfadr.Account)) ? DBNull.Value : (object)wfadr.Account);
                comm.Parameters.Add("@ImportID", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(wfadr.ImportID)) ? DBNull.Value : (object)wfsh.Left(wfadr.ImportID, 50));
                comm.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.email)) ? DBNull.Value : (object)wfadr.email);
                comm.Parameters.Add("@phone", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.Phone)) ? DBNull.Value : (object)wfadr.Phone);
                comm.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.CompanyName)) ? DBNull.Value : (object)wfadr.CompanyName);
                comm.Parameters.Add("@Department", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.Department)) ? DBNull.Value : (object)wfadr.Department);
                comm.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.CompanyName)) ? DBNull.Value : (object)wfadr.CompanyName);
                comm.Parameters.Add("@Address", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.Address1)) ? DBNull.Value : (object)wfadr.Address1);
                comm.Parameters.Add("@HouseNumber", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.HouseNumber)) ? DBNull.Value : (object)wfadr.HouseNumber);
                comm.Parameters.Add("@Address2", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.Address2)) ? DBNull.Value : (object)wfadr.Address2);
                comm.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.City)) ? DBNull.Value : (object)wfadr.City);
                comm.Parameters.Add("@PostalCode", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.PostalCode)) ? DBNull.Value : (object)wfadr.PostalCode);
                comm.Parameters.Add("@CountryID", SqlDbType.NVarChar, 100).Value = ((string.IsNullOrEmpty(wfadr.CountryID)) ? DBNull.Value : (object)wfadr.CountryID);
                comm.Parameters.Add("@AddrType", SqlDbType.Int).Value = (int)wfadr.AddrType;
                comm.Parameters.Add("@InternRef", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfadr.internRef)) ? DBNull.Value : (object)wfadr.internRef);
                comm.Parameters.Add("@Notes", SqlDbType.NVarChar, 50).Value = ((string.IsNullOrEmpty(wfadr.Notes)) ? DBNull.Value : (object)wfadr.Notes);
                comm.Parameters.Add("@VATNumber", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfadr.VATNumber)) ? DBNull.Value : (object)wfadr.VATNumber);
                comm.Parameters.Add("@category", SqlDbType.NVarChar, 20).Value = ((string.IsNullOrEmpty(wfadr.category)) ? DBNull.Value : (object)wfadr.category);

                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.AddressID = (Int32)myr["AddressID"];
                    item.Account = myr["Ad_Account"].ToString();
                    item.CompanyName = myr["CompanyName"].ToString();
                    item.Address1 = myr["Address"].ToString();
                    item.Address2 = myr["Address2"].ToString();
                    item.HouseNumber = myr["HouseNumber"].ToString();
                    item.PostalCode = myr["PostalCode"].ToString();
                    item.City = myr["City"].ToString();
                    item.Phone = myr["Phone"].ToString();
                    item.Email = myr["Email"].ToString();
                    item.CompanyWeb = myr["CompanyWeb"].ToString();

                    items.Add(item);
                    item = new AddressItem();
                }
                conn.Close();
                retstr = "OK";
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Address_load_Changed(ref IList<int> items, ref int AddressID, ref DateTime TimeChanged, int addtype)
        {
            string retstr = "OK";
            int item = 0;
            DateTime TimeForward = TimeChanged.AddMonths(1);
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " select top 10 Addressid  ";
                if (addtype == 2)
                {
                    mysql = string.Concat(mysql, " FROM ad_Addresses where compID = @CompID AND AddressID > @AddressID AND addtype < 3 AND timeChanged > @timeChanged AND timeChanged <= @TimeForward  AND timeChanged is not null order by addressID ");
                }
                else
                {
                    mysql = string.Concat(mysql, " FROM ad_Addresses where compID = @CompID AND AddressID > @AddressID AND timeChanged > @timeChanged AND timeChanged <= @TimeForward  AND timeChanged is not null order by addressID ");
                }
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
                comm.Parameters.Add("@TimeChanged", SqlDbType.DateTime).Value = TimeChanged;
                comm.Parameters.Add("@TimeForward", SqlDbType.DateTime).Value = TimeForward;
                AddressID = 0;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item = (Int32)myr["AddressID"];
                    items.Add(item);
                    AddressID = item;
                    item = new int();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        //public string Address_load_Reminded(ref IList<int> items, ref int LastAddressID, ReminderLevel MinRemLevel)
        //{     //REMOVED FROM INTERFACE
        //    string retstr = "OK";
        //    int item = 0;
        //    try
        //    {
        //        SqlConnection conn = new SqlConnection(conn_str);
        //        SqlCommand comm = new SqlCommand("we_address_reminder_IDList", conn);         //SP IS MISSING
        //        comm.CommandType = CommandType.StoredProcedure;
        //        comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
        //        comm.Parameters.Add("@LastAddressID", SqlDbType.Int).Value = LastAddressID;
        //        comm.Parameters.Add("@MinRemLevel", SqlDbType.Int).Value = (int)MinRemLevel;
        //        LastAddressID = 0;
        //        conn.Open();
        //        SqlDataReader myr = comm.ExecuteReader();
        //        while (myr.Read())
        //        {
        //            item = (Int32)myr["AddressID"];
        //            items.Add(item);
        //            LastAddressID = item;
        //            item = new int();
        //        }
        //        conn.Close();
        //    }
        //    catch (Exception e) { retstr = e.Message; }
        //    return retstr;
        //}
        public string Address_ApproveAudit(int AdrID, string ApprovedBy)
        {
            string retstr;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "update ad_Addresses_Audit set Approved = getdate(), ApprovedBy = @ApprovedBy where CompID = @P_CompID AND AddressID = @AdrID and ApprovedBy is null";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID;
            comm.Parameters.Add("@ApprovedBy", SqlDbType.NVarChar, 20).Value = wfsh.Left(ApprovedBy, 20);
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            retstr = "OK";
            return retstr;
        }
        public string Address_load_ChangedShipTo(ref IList<AddressShipTo> items, ref int AddressID, ref DateTime TimeChanged)
        {
            string retstr = "OK";
            // DateTime TimeForward = TimeChanged.AddMonths(12);
            var item = new AddressShipTo();
            //int adrID = 0;
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " Select addressID as AddressID, isnull(ShipTo,0) as shipTo from ";
                mysql = string.Concat(mysql, " (select top 10 CompID, Addressid FROM ad_Addresses ");
                mysql = string.Concat(mysql, " where compID = @CompID AND AddressID > @AddressID AND timeChanged > @timeChanged  AND timeChanged is not null AND addrType < 3 order by addressID ) as tb1  ");
                mysql = string.Concat(mysql, " left join  ad_Addresses_BillBuyerShip tb2 on tb2.CompID = tb1.CompID AND tb2.BillTo = tb1.AddressID order by addressID ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
                comm.Parameters.Add("@TimeChanged", SqlDbType.DateTime).Value = TimeChanged;
                AddressID = 0;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.AddressID = (Int32)myr["AddressID"];
                    item.ShipTo = (Int32)myr["ShipTo"];
                    items.Add(item);
                    AddressID = item.AddressID;
                    item = new AddressShipTo();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public int Address_update_timeChange(ref DateTime TimeChanged)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            //string mysql = "UPdate tbo set tbo.TimeChanged = tbi.TimeChanged FROM ad_addresses tbo inner join ";
            //mysql = string.Concat(mysql, " (SELECT tb1.CompID, tb1.IDBillTo, tb2.TimeChanged FROM  ad_Addresses_ShipBill tb1 inner join ad_addresses tb2 on tb2.CompID = tb1.CompID AND tb2.addressID = tb1.IDShipTo");
            //mysql = string.Concat(mysql, " Where tb2.CompID = @CompID AND tb2.timeChanged > @timeChanged) as tbi ");
            //mysql = string.Concat(mysql, " on tbo.CompID = tbi.CompID AND tbo.addressID = tbi.IDBIllTo AND tbo.TimeChanged < tbi.TimeChanged ");
            string mysql = "UPdate tbo set tbo.TimeChanged = tbi.TimeChanged FROM ad_addresses tbo inner join  ";
            mysql = string.Concat(mysql, " (SELECT tb1.CompID, tb1.BillTo, tb2.TimeChanged FROM  ad_Addresses_BillBuyerShip tb1 inner join ad_addresses tb2 on tb2.CompID = tb1.CompID AND tb2.addressID = tb1.ShipTo");
            mysql = string.Concat(mysql, " Where tb2.CompID = @CompID AND tb2.timeChanged > @timeChanged) as tbi ");
            mysql = string.Concat(mysql, " on tbo.CompID = tbi.CompID AND tbo.addressID = tbi.BIllTo AND tbo.TimeChanged < tbi.TimeChanged ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@TimeChanged", SqlDbType.DateTime).Value = TimeChanged;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return 0;
        }
        public int Address_PaymentMeans_Update(int AdrID, PaymentMeans PM)
        {  //benyttes endnu ikke, da LC udvikling stoppede midt i dette. SP ligger som script i we_Address_PaymentMeans_Update.sql på M:
            string PaymentType = "";
            string Value1 = "";
            string Value2 = "";
            switch (PM.PaymentMeansCode)
            {
                case 31: PaymentType = "fb"; Value1 = PM.IBAN; Value2 = PM.SWIFT; break;
                case 42: PaymentType = "bb"; Value1 = PM.BankAccount; Value2 = PM.BankRegno; break;
                case 48: PaymentType = "cc"; Value1 = PM.MeansOfPayment; Value2 = ""; break;
                case 50: PaymentType = PM.CardType; Value1 = PM.CreditorID; Value2 = ""; break;
                case 93: PaymentType = PM.CardType; Value1 = PM.CreditorID; Value2 = ""; break;
            }
            if (PaymentType != "")
            {
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand("dbo.we_Address_PaymentMeans_update", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@PaymentType", SqlDbType.NVarChar, 50).Value = PaymentType;
                comm.Parameters.Add("@Value1", SqlDbType.NVarChar, 50).Value = Value1;
                comm.Parameters.Add("@Value2", SqlDbType.NVarChar, 50).Value = Value2;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return 0;
        }
        public int Address_PaymentMeans_Load(int AdrID, ref PaymentMeans PM)
        {  //benyttes endnu ikke, da LC udvikling stoppede midt i dette. SP ligger som script i we_Address_PaymentMeans.sql på M:
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("dbo.we_Address_PaymentMeans", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
            comm.Parameters.Add("@O_PaymentType", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;
            comm.Parameters.Add("@O_Value1", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;
            comm.Parameters.Add("@O_Value2", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;
            conn.Open();
            comm.ExecuteNonQuery();
            string PaymentType = ((comm.Parameters["@O_PaymentType"].Value == DBNull.Value) ? "" : (string)comm.Parameters["@O_PaymentType"].Value);
            string Value1 = ((comm.Parameters["@O_Value1"].Value == DBNull.Value) ? "" : (string)comm.Parameters["@O_Value1"].Value);
            string Value2 = ((comm.Parameters["@O_Value2"].Value == DBNull.Value) ? "" : (string)comm.Parameters["@O_Value2"].Value);
            conn.Close();
            switch (PaymentType)
            {
                case "fb": PM.PaymentMeansCode = 31; PM.PaymentChannelCode = "IBAN"; PM.IBAN = Value1; PM.SWIFT = Value2; break;
                case "bb": PM.PaymentMeansCode = 42; PM.PaymentChannelCode = "DK:BANK"; PM.BankAccount = Value1; PM.BankRegno = Value2; break;
                case "cc": PM.PaymentMeansCode = 48; PM.PaymentChannelCode = "ZZZ"; PM.MeansOfPayment = Value1; break;
                case "01": PM.PaymentMeansCode = 50; PM.PaymentChannelCode = "DK:GIRO"; PM.CardType = PaymentType; PM.CreditorID = Value1; break;
                case "04": PM.PaymentMeansCode = 50; PM.PaymentChannelCode = "DK:GIRO"; PM.CardType = PaymentType; PM.CreditorID = Value1; break;
                case "15": PM.PaymentMeansCode = 50; PM.PaymentChannelCode = "DK:GIRO"; PM.CardType = PaymentType; PM.CreditorID = Value1; break;
                case "71": PM.PaymentMeansCode = 93; PM.PaymentChannelCode = "DK:FIK"; PM.CardType = PaymentType; PM.CreditorID = Value1; break;
                case "73": PM.PaymentMeansCode = 93; PM.PaymentChannelCode = "DK:FIK"; PM.CardType = PaymentType; PM.CreditorID = Value1; break;
                case "75": PM.PaymentMeansCode = 93; PM.PaymentChannelCode = "DK:FIK"; PM.CardType = PaymentType; PM.CreditorID = Value1; break;
            }
            return 0;
        }
        public string Address_load_CreditCardErrors(ref IList<CreditCardErrors> items, int AddressID,int Status, DateTime FromDate)
        {
            string retstr = "OK";
            var item = new CreditCardErrors();
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = " select ID,Merchant,isnull(AddressID,0) as AddressID,isnull(SaleID,0) as SaleID,Currency,isnull(Amount,0) as Amount, isnull(OrderID,0) as OrderID, TransactionNumber, HttpBody,status FROM  wf_dibs_capture ";
                mysql = string.Concat(mysql, " Where CompID = @CompID  AND EnterDate > @FromDate AND status in (18,20) ");
                if (AddressID > 0)
                {
                    mysql = string.Concat(mysql, " AND AddressID = @AddressID ");
                }
                if ((Status == 18) || (Status == 20))
                {
                    mysql = string.Concat(mysql, " AND Status = @Status ");
                }
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
                comm.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = FromDate;
                comm.Parameters.Add("@Status", SqlDbType.Int).Value = Status;
                AddressID = 0;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.ID = (Int32)myr["ID"];
                    item.Merchant = myr["Merchant"].ToString();
                    item.AddressID = (Int32)myr["AddressID"];
                    item.SaleID = (Int32)myr["SaleID"];
                    item.Currency = myr["Currency"].ToString();
                    item.Amount = (decimal)myr["Amount"];
                    item.OrderID = (Int32)myr["OrderID"];
                    item.TransactionNumber = myr["TransactionNumber"].ToString();
                    item.HttpBody = myr["HttpBody"].ToString();
                    item.Status = (Int32)myr["Status"];
                    items.Add(item);
                    AddressID = item.AddressID;
                    item = new CreditCardErrors();
                }
                conn.Close();
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Addresses_ShipBillTo_Load(int AdrID, ref IList<AddressesShipBillItem> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            AddressesShipBillItem item = new AddressesShipBillItem();
            // string mysql = "SELECT IDShipTo,isnull(UseAsDefault,0) as UseAsDefault from ad_Addresses_ShipBill  Where CompID = @CompID AND  IDBillTo =  @AdrID ";
            string mysql = "select isnull(max(ShipTo),0) as IDShipTo,isnull(DefaultBIllTo,0) as UseAsDefault from ad_Addresses_BillBuyerShip where CompID = @CompID AND BillTo = @AdrID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.AddressID = (Int32)myr["IDShipTo"];
                item.UseAsDefault = (Boolean)myr["UseAsDefault"];
                item.ShipBillType = 1;
                items.Add(item);
                item = new AddressesShipBillItem();
            }
            conn.Close();
            return "OK";
        }

        public string Addresses_BillTo_Load(int AdrID, ref IList<AddressesShipBillItem> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            AddressesShipBillItem item = new AddressesShipBillItem();
            //string mysql = "SELECT IDBillTo,isnull(UseAsDefault,0) as UseAsDefault from ad_Addresses_ShipBill  Where CompID = @CompID AND  IDShipTo =  @AdrID ";
            string mysql = "select isnull(max(BillTo),0) as IDBillTo,isnull(DefaultBIllTo,0) as UseAsDefault from ad_Addresses_BillBuyerShip where CompID = @CompID AND ShipTo = @AdrID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.AddressID = (Int32)myr["IDBillTo"];
                item.UseAsDefault = (Boolean)myr["UseAsDefault"];
                item.ShipBillType = 2;
                items.Add(item);
                item = new AddressesShipBillItem();
            }
            conn.Close();
            return "OK";
        }



        public string Addresses_ShipBillTo_add(int AdrID, ref AddressesShipBillItem ShipBillItem)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            AddressesShipBillItem item = new AddressesShipBillItem();
            // string mysql = "if not exists (Select * from ad_Addresses_ShipBill Where CompID = @CompID AND IDShipTo = @ShipTo AND  IDBillTo = @BillTo) ";
            // mysql = string.Concat(mysql, " Insert ad_Addresses_ShipBill (CompID,IDShipTo,IDBillTo, UseAsDefault) values (@CompID,@ShipTo,@BillTo,@UseAsDefault) ");
            string mysql = "if not exists (Select * from ad_Addresses_BillBuyerShip Where CompID = @CompID AND ShipTo = @ShipTo AND Buyer = @BillTo AND BillTo = @BillTo) ";
            mysql = string.Concat(mysql, " Insert ad_Addresses_BillBuyerShip (CompID,ShipTo,Buyer,BillTo, DefaultBIllTo,TimeChanged) values (@CompID,@ShipTo,@BillTo,@BillTo,@UseAsDefault,GetDate()) ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ShipTo", SqlDbType.Int).Value = ShipBillItem.AddressID;
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = AdrID;
            comm.Parameters.Add("@UseAsDefault", SqlDbType.Bit).Value = ShipBillItem.UseAsDefault;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return "OK";
        }
        public string Addresses_ShipBillTo_Delete(int AdrID, ref AddressesShipBillItem ShipBillItem)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            AddressesShipBillItem item = new AddressesShipBillItem();
            //string mysql = "delete from ad_Addresses_ShipBill Where CompID = @CompID AND IDShipTo = @ShipTo AND  IDBillTo = @BillTo ";
            string mysql = "delete from ad_Addresses_BillBuyerShip Where CompID = @CompID AND ShipTo = @ShipTo AND  BillTo = @BillTo ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ShipTo", SqlDbType.Int).Value = ShipBillItem.AddressID;
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = AdrID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return "OK";
        }


        public string Addresses_ShipBillTo_add_BillTo(int AdrID, ref AddressesShipBillItem ShipBillItem)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            AddressesShipBillItem item = new AddressesShipBillItem();
            //string mysql = "if not exists (Select * from ad_Addresses_ShipBill Where CompID = @CompID AND IDShipTo = @ShipTo AND  IDBillTo = @BillTo) ";
            //mysql = string.Concat(mysql, " Insert ad_Addresses_ShipBill (CompID,IDShipTo,IDBillTo, UseAsDefault) values (@CompID,@ShipTo,@BillTo,@UseAsDefault) ");
            string mysql = "if not exists (Select * from ad_Addresses_BillBuyerShip Where CompID = @CompID AND ShipTo = @ShipTo AND buyer = @BillTo AND  BillTo = @BillTo) ";
            mysql = string.Concat(mysql, "Insert ad_Addresses_BillBuyerShip (CompID,ShipTo,Buyer,BillTo, DefaultBIllTo,TimeChanged) values (@CompID,@ShipTo,@BillTo,@BillTo,@UseAsDefault,GetDate()) ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ShipTo", SqlDbType.Int).Value = AdrID;
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = ShipBillItem.AddressID;
            comm.Parameters.Add("@UseAsDefault", SqlDbType.Bit).Value = ShipBillItem.UseAsDefault;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return "OK";
        }
        public string Addresses_ShipBillTo_Delete_BillTo(int AdrID, ref AddressesShipBillItem ShipBillItem)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            AddressesShipBillItem item = new AddressesShipBillItem();
            //string mysql = "delete from ad_Addresses_ShipBill Where CompID = @CompID AND IDShipTo = @ShipTo AND  IDBillTo = @BillTo ";
            string mysql = "delete from ad_Addresses_BillBuyerShip Where CompID = @CompID AND ShipTo = @ShipTo AND  BillTo = @BillTo ";

            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@ShipTo", SqlDbType.Int).Value = AdrID;
            comm.Parameters.Add("@BillTo", SqlDbType.Int).Value = ShipBillItem.AddressID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return "OK";
        }


        public string Address_properties_add(int AdrID, int propertyID)
        {
            string retstr = "err";
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "if not exists (SELECT * FROM ad_addresses_AddrProp WHERE CompID = @CompID AND AddressID = @AddressID AND PropertyID = @PropertyID) ";
                mysql = String.Concat(mysql, " AND  exists (SELECT * FROM ad_addresses_Properties WHERE CompID = @CompID AND PropertyID = @PropertyID) ");
                mysql = String.Concat(mysql, " insert ad_addresses_AddrProp (CompID,AddressID,PropertyID,EnterDate) values (@CompID,@AddressID,@PropertyID, @EnterDate) ");
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@PropertyID", SqlDbType.Int).Value = propertyID;
                comm.Parameters.Add("@EnterDate", SqlDbType.DateTime).Value = DateTime.Today;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
                retstr = "OK";
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Address_properties_delete(int AdrID, int FromID, int ToID)
        {
            string retstr = "err";
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "DELETE  FROM ad_addresses_AddrProp WHERE CompID = @CompID AND AddressID = @AddressID AND PropertyID >= @FromID AND PropertyID <= @ToID";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@FromID", SqlDbType.Int).Value = FromID;
                comm.Parameters.Add("@ToID", SqlDbType.Int).Value = ToID;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
                retstr = "OK";
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Address_properties_present(int AdrID, int propertyID, ref Boolean present)
        {
            string retstr = "err";
            int cpr = 0;
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "SELECT isnull(Count(*),0) as countProp from ad_addresses_AddrProp WHERE CompID = @CompID AND AddressID = @AddressID AND PropertyID = @PropertyID";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@PropertyID", SqlDbType.Int).Value = propertyID;
                conn.Open();
                cpr = (Int32)comm.ExecuteScalar();
                conn.Close();
                if (cpr > 0) present = true; else present = false;
                retstr = "OK";
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Address_properties_loadAll(int FromID, int ToID, ref IList<AddressProperty> items)
        {
            string retstr = "err";
            AddressProperty item = new AddressProperty();
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "SELECT PropertyID, Description from ad_addresses_Properties WHERE CompID = @CompID AND PropertyID >= @FromID AND PropertyID <= @ToID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@FromID", SqlDbType.Int).Value = FromID;
                comm.Parameters.Add("@ToID", SqlDbType.Int).Value = ToID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    item.PropertyID = (Int32)myr["PropertyID"];
                    item.Description = myr["Description"].ToString();
                    items.Add(item);
                    item = new AddressProperty();
                }
                conn.Close();
                retstr = "OK";
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Address_add_Activity(int AdrID, ref Activity Act)
        {
            string retstr = "err";
            try
            {
                if (AdrID > 0)
                {
                    int ActType = Act.ActivityType;
                    if (Act.DueDate == null) Act.DueDate = DateTime.Today;
                    Act.AddressID = AdrID;
                    SqlConnection conn = new SqlConnection(conn_str);
                    int activityID = 0;
                    string mysql = "Select isnull(max(ActivityID),0) as ActivityID from ad_activities tb2 Where tb2.CompID = @CompID AND tb2.AddressID = @AdrID ";
                    SqlCommand comm = new SqlCommand(mysql, conn);
                    comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                    comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID;
                    comm.Parameters.Add("@ActivityID", SqlDbType.Int).Value = 0;
                    comm.Parameters.Add("@ActivityType", SqlDbType.Int).Value = ActType;
                    comm.Parameters.Add("@Desc", SqlDbType.NVarChar,-1).Value = (string.IsNullOrEmpty(Act.Description) ? DBNull.Value : (object)Act.Description);
                    comm.Parameters.Add("@DueDate", SqlDbType.DateTime).Value = (DateTime)Act.DueDate;
                    conn.Open();
                    activityID = (Int32)comm.ExecuteScalar();
                    activityID = activityID + 1;
                    Act.ActivityID = activityID;
                    comm.Parameters["@ActivityID"].Value = activityID;
                    mysql = "insert ad_activities (compID, AddressID, ActivityID, ActivityType, enterDate,DueDate,Description)  VALUES (@CompID, @AdrID, @ActivityID, @ActivityType , getdate(),@DueDate,@Desc) ";
                    comm.CommandText = mysql;
                    comm.ExecuteNonQuery();
                    conn.Close();
                    retstr = "OK";
                }
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public string Activities_Items_get(int AdrID, ref IList<Activity> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            Activity item = new Activity();
            string mysql = "SELECT ActivityID, ActivityType, EnterDate, DueDate, settled, SettleDate, Description, UserID, ExtRef, StateID  ";
            mysql = string.Concat(mysql, "  from ad_activities  Where CompID = @CompID AND AddressID = @AdrID ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AdrID", SqlDbType.Int).Value = AdrID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.AddressID = AdrID;
                item.ActivityID = (Int32)myr["ActivityID"];
                item.ActivityType = (Int32)((myr["ActivityType"] == DBNull.Value) ? 0 : (Int32)myr["ActivityType"]);
                item.Description = myr["Description"].ToString();
                item.DueDate = (DateTime)((myr["DueDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["DueDate"]);
                item.EnterDate = (DateTime)((myr["EnterDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["EnterDate"]);
                item.UserID = myr["UserID"].ToString();
                item.StateID = (Int32)((myr["StateID"] == DBNull.Value) ? 0 : (Int32)myr["StateID"]);
                item.settled = (myr["settled"] == DBNull.Value ? false : (Boolean)myr["settled"]);
                items.Add(item);
                item = new Activity();
            }
            conn.Close();
            return "OK";
        }
        public string Activities_Items_get_passthrough(ref IList<Activity> items, int UserTop, string UserWhere, string UserOrder)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            Activity item = new Activity();
            string mysql = "SELECT ";
            if (UserTop > 0) mysql = String.Concat(mysql, " top ", UserTop);
            mysql = String.Concat(mysql, " tb1.ActivityID, tb1.AddressID, tb1.ActivityType, tb1.EnterDate, tb1.DueDate, tb1.settled, tb1.SettleDate, tb1.Description, tb1.UserID, tb1.ExtRef, tb1.StateID,  ");
            mysql = string.Concat(mysql, " tb2.CompanyName ");
            mysql = string.Concat(mysql, "  from ad_activities tb1 inner join ad_addresses tb2 on tb2.CompID = tb1.compID AND tb2.addressID =  tb1.AddressID  ");
            mysql = string.Concat(mysql, "  Where tb1.CompID = @CompID ");
            if (UserWhere != string.Empty) mysql = string.Concat(mysql, " AND ", UserWhere);
            if (UserOrder != string.Empty) mysql = string.Concat(mysql, " Order by  ", UserOrder);
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.AddressID = (Int32)myr["AddressID"];
                item.ActivityID = (Int32)myr["ActivityID"];
                item.ActivityType = (Int32)((myr["ActivityType"] == DBNull.Value) ? 0 : (Int32)myr["ActivityType"]);
                item.Description = myr["Description"].ToString();
                item.DueDate = (DateTime)((myr["DueDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["DueDate"]);
                item.EnterDate = (DateTime)((myr["EnterDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["EnterDate"]);
                item.UserID = myr["UserID"].ToString();
                item.StateID = (Int32)((myr["StateID"] == DBNull.Value) ? 0 : (Int32)myr["StateID"]);
                item.settled = (myr["settled"] == DBNull.Value ? false : (Boolean)myr["settled"]);
                item.CompanyName = myr["CompanyName"].ToString();
                items.Add(item);
                item = new Activity();
            }
            conn.Close();
            return "OK";
        }
        public string Activities_Items_get_user(ref Activity wfActivity, ref IList<Activity> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            Activity item = new Activity();
            string userid = wfActivity.UserID;
            int AdrID = wfActivity.AddressID;
            string mysql = "set concat_null_yields_null off SELECT tb1.AddressID, ActivityID, ActivityType, EnterDate, DueDate, settled, SettleDate, Description, UserID, ExtRef, StateID , ";
            mysql = string.Concat(mysql, " CompanyName + ' ' +  LastName + ', ' +  Address + ' ' + housenumber + ' ' + inhousemail as CompanyName ");
            mysql = string.Concat(mysql, "  from ad_activities tb1 inner join ad_addresses tb2 on tb2.CompID = tb1.compID AND tb2.addressID =  tb1.AddressID  ");
            mysql = string.Concat(mysql, " Where tb1.CompID = @CompID ");
            if (!string.IsNullOrEmpty(userid)) { mysql = string.Concat(mysql, " AND UserID = @UserID "); }
            if (AdrID != 0) { mysql = string.Concat(mysql, " AND tb1.AddressID = @AddressID "); }
            mysql = string.Concat(mysql, " order by DueDate ");
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
            comm.Parameters.Add("@UserID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(userid) ? DBNull.Value : (object)userid);
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.AddressID = (Int32)myr["AddressID"];
                item.ActivityID = (Int32)myr["ActivityID"];
                item.ActivityType = (Int32)myr["ActivityType"];
                item.Description = myr["Description"].ToString();
                item.DueDate = (DateTime)myr["DueDate"];
                item.EnterDate = (DateTime)myr["EnterDate"];
                item.CompanyName = myr["CompanyName"].ToString();
                item.settled = (myr["settled"] == DBNull.Value ? false : (Boolean)myr["settled"]);
                //item.UserID = myr["UserID"].ToString();
                //item.StateID = (int)myr["StateID"];
                // item.settled = (Boolean)myr["settled"];
                // item.SettleDate = (DateTime)myr["SettleDate"];
                items.Add(item);
                item = new Activity();
            }
            conn.Close();
            return "OK";
        }
        public string Address_Activity_Load(ref Activity wfActivity)
        {
            string errStr = "err";
            int AdrID = wfActivity.AddressID;
            int ActivityID = wfActivity.ActivityID;
            if (AdrID > 0 && ActivityID > 0)
            {
                errStr = "OK";
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "SELECT * from  ad_activities WHERE CompID = @CompID AND AddressID = @AddressID AND ActivityID = @ActivityID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@ActivityID", SqlDbType.Int).Value = ActivityID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    wfActivity.AddressID = (Int32)myr["AddressID"];
                    wfActivity.ActivityID = (Int32)myr["ActivityID"];
                    wfActivity.ActivityType = (Int32)myr["ActivityType"];
                    wfActivity.Description = myr["Description"].ToString();
                    wfActivity.UserID = myr["UserID"].ToString();
                    wfActivity.StateID = (myr["StateID"] == DBNull.Value ? 0 : (int)myr["StateID"]);
                    wfActivity.DueDate = (DateTime)((myr["DueDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["DueDate"]);
                    wfActivity.SettleDate = (DateTime)((myr["SettleDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["SettleDate"]);
                    wfActivity.EnterDate = (DateTime)((myr["EnterDate"].Equals(DBNull.Value)) ? DateTime.MinValue : (DateTime)myr["EnterDate"]);
                    wfActivity.settled = (myr["settled"] == DBNull.Value ? false : (Boolean)myr["settled"]);
                }
                conn.Close();
            }
            return errStr;
        }
        public string Address_Activity_Update(ref Activity wfActivity)
        {
            string errStr = "err";
            int AdrID = wfActivity.AddressID;
            int ActivityID = wfActivity.ActivityID;
            string Description = wfActivity.Description;
            String AHeader = wfActivity.Description;
            if (String.IsNullOrEmpty(Description)) Description = "New activity";
            if (String.IsNullOrEmpty(AHeader)) AHeader = "New alert";
            DateTime OldDate = new DateTime(1900, 1, 1);
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            if (DateTime.Compare(wfActivity.EnterDate, OldDate) < 0) wfActivity.EnterDate = OldDate;
            if (DateTime.Compare(wfActivity.DueDate, OldDate) < 0) wfActivity.DueDate = OldDate;
            if (DateTime.Compare(wfActivity.SettleDate, OldDate) < 0) wfActivity.SettleDate = OldDate;
            if (AdrID > 0 && ActivityID > 0)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "UPDATE ad_activities set ActivityType = @ActivityType, EnterDate = @EnterDate, DueDate = @DueDate, Description = @Description, UserID = @UserID, StateID = @StateID, settled = @settled, SettleDate = @SettleDate   WHERE CompID = @CompID AND AddressID = @AddressID AND ActivityID = @ActivityID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@ActivityID", SqlDbType.Int).Value = ActivityID;
                comm.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(Description) ? DBNull.Value : (object)Description;
                comm.Parameters.Add("@UserID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(wfActivity.UserID) ? DBNull.Value : (object)wfActivity.UserID);
                comm.Parameters.Add("@StateID", SqlDbType.Int).Value = wfActivity.StateID;
                comm.Parameters.Add("@ActivityType", SqlDbType.Int).Value = wfActivity.ActivityType;
                comm.Parameters.Add("@EnterDate", SqlDbType.DateTime).Value = ((wfActivity.EnterDate < minSqlDate) ? DBNull.Value : (object)wfActivity.EnterDate);
                comm.Parameters.Add("@DueDate", SqlDbType.DateTime).Value = ((wfActivity.DueDate < minSqlDate) ? DBNull.Value : (object)wfActivity.DueDate);
                comm.Parameters.Add("@SettleDate", SqlDbType.DateTime).Value = ((wfActivity.SettleDate < minSqlDate) ? DBNull.Value : (object)wfActivity.SettleDate);
                comm.Parameters.Add("@settled", SqlDbType.Bit).Value = wfActivity.settled;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
                errStr = "OK";
            }
            else
            {
                errStr = "Err: AddressID or ActivityId = 0 ";
            }
            return errStr;
        }
        public string Address_Activity_Delete(ref Activity wfActivity)
        {
            string errStr = "err";
            int AdrID = wfActivity.AddressID;
            int ActivityID = wfActivity.ActivityID;
            string Description = wfActivity.Description;
            if (AdrID > 0 && ActivityID > 0)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "DELETE ad_activities WHERE CompID = @CompID AND AddressID = @AddressID AND ActivityID = @ActivityID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@ActivityID", SqlDbType.Int).Value = ActivityID;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
                errStr = "OK";
            }
            return errStr;
        }
        public string Activitie_Types_get(ref IList<ActivityType> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            ActivityType item = new ActivityType();
            string mysql = "select * from ad_activity_types  Where CompID = @CompID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.Type = (Int32)myr["ActivityType"];
                item.Description = myr["Description"].ToString();
                items.Add(item);
                item = new ActivityType();
            }
            conn.Close();
            return "OK";
        }
        public string Activitie_state_get(ref IList<ActivityState> items)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            ActivityState item = new ActivityState();
            string mysql = "select * from ad_activity_state  Where CompID = @CompID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.StateID = (Int32)myr["StateID"];
                item.Description = myr["Description"].ToString();
                items.Add(item);
                item = new ActivityState();
            }
            conn.Close();
            return "OK";
        }
        public string Contact_add_new(ref Contact mycontact)
        {
            string retstr = "OK";
            try
            {
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand("dbo.we_contact_update", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ContID", SqlDbType.Int).Value = mycontact.ContID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = mycontact.AddressID;
                comm.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(mycontact.Type) ? DBNull.Value : (object)mycontact.Type);
                comm.Parameters.Add("@ContactName", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(mycontact.ContactName) ? DBNull.Value : (object)mycontact.ContactName);
                comm.Parameters.Add("@Job", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(mycontact.Job) ? DBNull.Value : (object)mycontact.Job);
                comm.Parameters.Add("@LocalPhone", SqlDbType.NVarChar,50).Value = (string.IsNullOrEmpty(mycontact.LocalPhone) ? DBNull.Value : (object)mycontact.LocalPhone); 
                comm.Parameters.Add("@HomePhone", SqlDbType.NVarChar,50).Value = DBNull.Value;
                comm.Parameters.Add("@MobilPhone", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(mycontact.MobilPhone) ? DBNull.Value : (object)mycontact.MobilPhone);
                comm.Parameters.Add("@CompanyEmail", SqlDbType.NVarChar, 255).Value = (string.IsNullOrEmpty(mycontact.email) ? DBNull.Value : (object)mycontact.email);
                comm.Parameters.Add("@Initials", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mycontact.Initials) ? DBNull.Value : (object)mycontact.Initials);
                comm.Parameters.Add("@AccountingCost", SqlDbType.NVarChar, 255).Value = (string.IsNullOrEmpty(mycontact.AccountingCost) ? DBNull.Value : (object)mycontact.AccountingCost);
                comm.Parameters.Add("@EAN", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(mycontact.EndpointID) ? DBNull.Value : (object)mycontact.EndpointID);
                comm.Parameters.Add("@EndPointScheme", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(mycontact.EndpointScheme) ? DBNull.Value : (object)mycontact.EndpointScheme);
                comm.Parameters.Add("@UBLDefault", SqlDbType.Bit).Value = mycontact.UBLDefault;
                //unavailable parameters
            
                comm.Parameters.Add("@HomeFax", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@Address", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@CountryID", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@Region", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@PostalCode", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@C_City", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@C_Country", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@Note", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@email", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@Password", SqlDbType.NVarChar).Value = DBNull.Value;
                comm.Parameters.Add("@ReceiveInvoice", SqlDbType.Bit).Value = DBNull.Value;
                comm.Parameters.Add("@ReceiveOrder", SqlDbType.Bit).Value = DBNull.Value;
                comm.Parameters.Add("@ReceiveQuotation", SqlDbType.Bit).Value = DBNull.Value;
                comm.Parameters.Add("@ReceiveReminder", SqlDbType.Bit).Value = DBNull.Value;
                comm.Parameters.Add("@ReceiveInformation", SqlDbType.Bit).Value = DBNull.Value;
                comm.Parameters.Add("@ReceiveInquiry", SqlDbType.Bit).Value = DBNull.Value;
                comm.Parameters.Add("@ReceivePuOrder", SqlDbType.Bit).Value = DBNull.Value;
                comm.Parameters.Add("@ReceivePuInvoice", SqlDbType.Bit).Value = DBNull.Value;
                comm.Parameters.Add("@GlobalInitials", SqlDbType.NVarChar, 50).Value = DBNull.Value;



                var returnParameter = comm.Parameters.Add("@ContID", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                //comm.Parameters.Add("@ContID", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                conn.Open();
                //mycontact.ContID = Convert.ToInt32(comm.ExecuteScalar());
                comm.ExecuteNonQuery();
                conn.Close();
                mycontact.ContID = Convert.ToInt32(returnParameter.Value);
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }
        public int Contacts_load(int AddressID,ref List<Contact> items)
        {
            var item = new Contact();
            string mysql = "select tb1.ContID, tb1.initials,tb2.ContactName, tb1.category,tb1.job,tb1.LocalPhone,tb2.MobilPhone,tb1.Job,tb1.email,AccountingCost,tb1.EndpointScheme,tb1.EAN,isnull(UBLDefault,0) as UBLDefault FROM  ad_Contacts_adr tb1 inner join ad_Contacts tb2 on tb2.CompID = tb1.CompID AND tb2.ContID = tb1.ContID ";
            mysql = string.Concat(mysql, "where tb1.CompID = @CompID AND tb1.AddressID = @AddressID");
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.AddressID = AddressID;
                item.ContID = (int)myr["ContID"];
                item.ContactName = myr["ContactName"].ToString();
                item.Type = myr["category"].ToString();
                item.Initials = myr["initials"].ToString();
                item.Job = myr["job"].ToString();
                item.LocalPhone = myr["LocalPhone"].ToString();
                item.MobilPhone = myr["MobilPhone"].ToString();
                item.email = myr["email"].ToString();
                //item.UBLDefault =  (int)myr["UBLDefault"] == 0 ? false : true;
                item.EndpointScheme = myr["EndpointScheme"].ToString();
                item.EndpointID = myr["EAN"].ToString();
                item.AccountingCost = myr["AccountingCost"].ToString();
                items.Add(item);
                item = new Contact();
            }
            conn.Close();
            return 0;
         }
        // Address alerts 
        public string Address_Alerts_load(int AddressID, ref IList<AddressAlert> items)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            var item = new AddressAlert();
            string mysql = "SELECT AlertID, AlertText, AlertHeader, TimeStamp,isnull(on_sa_orders,0) as on_sa_orders, isnull(on_sa_Inv,0) as on_sa_inv ,isnull(on_pu_orders,0) as on_pu_orders, isnull(on_pu_Inv,0) as on_pu_inv FROM ad_Addresses_Alerts Where CompID = @CompID AND AddressID = @AddressID order by TimeStamp desc ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.AddressID = AddressID;
                item.AlertID = (Int32)myr["AlertID"];
                item.AlertText = myr["AlertText"].ToString();
                item.AlertHeader = myr["AlertHeader"].ToString();
                item.on_pu_inv = (Boolean)myr["on_pu_Inv"];
                item.on_pu_orders = (Boolean)myr["on_pu_orders"];
                item.on_sa_Inv = (Boolean)myr["on_sa_Inv"];
                item.on_sa_orders = (Boolean)myr["on_sa_orders"];
                item.TimeStamp = (DateTime)myr["TimeStamp"];
                items.Add(item);
                item = new AddressAlert();
            }
            conn.Close();
            return errStr;
        }
        public string Address_Alerts_Add(ref AddressAlert wfAlert)
        {
            string errStr = "err";
            int AdrID = wfAlert.AddressID;
            string Atext = wfAlert.AlertText;
            String AHeader = wfAlert.AlertHeader;
            if (String.IsNullOrEmpty(Atext)) Atext = "New alert";
            if (String.IsNullOrEmpty(AHeader)) AHeader = "New alert";
            if (AdrID > 0)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "INSERT ad_Addresses_Alerts (CompID,AddressID,AlertText,TimeStamp,AlertHeader) values (@CompID,@AddressID,@AlertText,@ToDay,@AlertHeader)";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@AlertText", SqlDbType.NVarChar,-1).Value = Atext;
                comm.Parameters.Add("@ToDay", SqlDbType.DateTime).Value = DateTime.Now;
                comm.Parameters.Add("@AlertHeader", SqlDbType.NVarChar, 50).Value = AHeader;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return errStr;
        }
        public string Address_Alerts_Update(ref AddressAlert wfAlert)
        {
            string errStr = "err";
            int AdrID = wfAlert.AddressID;
            int AlertID = wfAlert.AlertID;
            string Atext = wfAlert.AlertText;
            String AHeader = wfAlert.AlertHeader;
            if (String.IsNullOrEmpty(Atext)) Atext = "New alert";
            if (String.IsNullOrEmpty(AHeader)) AHeader = "New alert";
            if (AdrID > 0 && AlertID > 0)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "UPDATE ad_Addresses_Alerts set AlertText = @AlertText, on_sa_orders = @on_sa_orders, on_sa_Inv = @on_sa_Inv, on_pu_orders = @on_pu_orders, on_pu_inv = @on_pu_inv, TimeStamp = @TimeStamp, AlertHeader = @AlertHeader   WHERE CompID = @CompID AND AddressID = @AddressID AND AlertID = @AlertID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@AlertID", SqlDbType.Int).Value = AlertID;
                comm.Parameters.Add("@AlertText", SqlDbType.NVarChar, -1).Value = Atext;
                comm.Parameters.Add("@on_sa_orders", SqlDbType.Bit).Value = wfAlert.on_sa_orders;
                comm.Parameters.Add("@on_sa_Inv", SqlDbType.Bit).Value = wfAlert.on_sa_Inv;
                comm.Parameters.Add("@on_pu_orders", SqlDbType.Bit).Value = wfAlert.on_pu_orders;
                comm.Parameters.Add("@on_pu_inv", SqlDbType.Bit).Value = wfAlert.on_pu_inv;
                comm.Parameters.Add("@TimeStamp", SqlDbType.DateTime).Value = wfAlert.TimeStamp;
                comm.Parameters.Add("@AlertHeader", SqlDbType.NVarChar, 50).Value = AHeader;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return errStr;
        }
        public string Address_Alerts_Delete(ref AddressAlert wfAlert)
        {
            string errStr = "err";
            int AdrID = wfAlert.AddressID;
            int AlertID = wfAlert.AlertID;
            string Atext = wfAlert.AlertText;
            if (String.IsNullOrEmpty(Atext)) Atext = "New alert";
            if (AdrID > 0 && AlertID > 0)
            {
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "DELETE FROM ad_Addresses_Alerts  WHERE CompID = @CompID AND AddressID = @AddressID AND AlertID = @AlertID ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@AlertID", SqlDbType.Int).Value = AlertID;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            }
            return errStr;
        }
        // Address ExtraLines
        private void append_extraLines(int AddressID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("wf_ad_addresses_AddExtraLines", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
        }
        public string Address_ExtraLines_load(int AddressID, ref IList<AddressExtraLine> items)
        {
            string errStr = "err";
            append_extraLines(AddressID);
            SqlConnection conn = new SqlConnection(conn_str);
            var item = new AddressExtraLine();
            string mysql = "SELECT * FROM ad_addresses_ExtraLines Where CompID = @CompID AND AddressID = @AddressID order by LinePos ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.AddressID = AddressID;
                item.Description = myr["Description"].ToString();
                item.Value = myr["Value"].ToString();
                items.Add(item);
                item = new AddressExtraLine();
            }
            conn.Close();
            return errStr;
        }
        public string Address_ExtraLines_Update(ref AddressExtraLine wfExtraLine)
        {
            string errStr = "err";
            int AdrID = wfExtraLine.AddressID;
            string Description = wfExtraLine.Description;
            string LineValue = wfExtraLine.Value;
            if (!string.IsNullOrEmpty(Description))
            {
                if (!(Address_ExtraLines_Exists(AdrID, Description)))
                {
                    Address_ExtraLines_AddTemplate(AdrID);
                }
                SqlConnection conn = new SqlConnection(conn_str);
                string mysql = "UPDATE ad_addresses_ExtraLines set Value = @Value WHERE CompID = @CompID AND AddressID = @AddressID  AND Description = @Desc ";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AdrID;
                comm.Parameters.Add("@Desc", SqlDbType.NVarChar, 50).Value = Description;
                comm.Parameters.Add("@Value", SqlDbType.NVarChar, 50).Value = LineValue;
                conn.Open();
                comm.ExecuteNonQuery();
                conn.Close();
            } 
            return errStr;
        }
        public int Address_ExtraLines_Template(int AddressID, ref string TemplateName)
        {
            int TemplateID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT ad_addresses_ExtraTemplates.TemplateID, ad_addresses_ExtraTemplates.Description FROM ad_Addresses_Cat AS tb1 INNER JOIN ad_Addresses AS tb2 ON tb2.CompID = tb1.CompID AND tb2.category = tb1.category INNER JOIN ad_addresses_ExtraTemplates ON tb1.CompID = ad_addresses_ExtraTemplates.CompID AND tb1.TemplateID = ad_addresses_ExtraTemplates.TemplateID WHERE(tb2.CompID = @CompID) AND(tb2.addressID = @AddressID)";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            conn.Open();
            SqlDataReader Template = comm.ExecuteReader();
            if (!Template.HasRows)
            {
                Template.Close();
                comm.CommandText = "SELECT TemplateID, Description FROM ad_addresses_ExtraTemplates WHERE(DefaultTemplate <> 0) AND(CompID = @CompID)";
                Template = comm.ExecuteReader();
            }
            if (!Template.HasRows)
            {
                TemplateID = 0;
                TemplateName = "No default template";
            } else
            {
                Template.Read();
                string strTemplate = Template["TemplateID"].ToString();
                int.TryParse(strTemplate, out TemplateID);
                TemplateName = Template["Description"].ToString();
            }
            conn.Close();
            return TemplateID;
        }
        private bool Address_ExtraLines_Exists(int AddressID, string Description)
        {
            bool Answer = false;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "select case (select count(*) from ad_addresses_ExtraLines WHERE CompID = @CompID AND AddressID = @AddressID  AND Description = @Desc)  when 0 then 0 else -1 end";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            comm.Parameters.Add("@Desc", SqlDbType.NVarChar, 50).Value = Description;
            conn.Open();
            Answer = Convert.ToBoolean(comm.ExecuteScalar());
            conn.Close();
            return Answer;
        }
        private void Address_ExtraLines_AddTemplate(int AddressID)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            SqlCommand comm = new SqlCommand("wf_ad_addresses_AddExtraLines", conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.NVarChar, 20).Value = AddressID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
        }
        public string Address_Categories_load(ref IList<AddressCategory> items)
        {
            string errStr = "err";
            SqlConnection conn = new SqlConnection(conn_str);
            var item = new AddressCategory();
            string mysql = "SELECT * FROM ad_Addresses_Cat Where CompID = @CompID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                item.Category = myr["Category"].ToString();
                item.Cat_Description = myr["Cat_Describtion"].ToString();
                item.TemplateID = (Int32)((myr["TemplateID"] == DBNull.Value) ? 0 : (Int32)myr["TemplateID"]);
                items.Add(item);
                item = new AddressCategory();
            }
            conn.Close();
            return errStr;
        }
        public string Address_Blur(int AddressID)
        {
            string errStr = "OK";
            wfws.LookUp Generate = new wfws.LookUp();

            SqlConnection conn = new SqlConnection(conn_str);
            //other things to blur: Phone, email
            string mysql = "Update ad_Addresses set CompanyName = @NewName, Address = @NewAddress, Address2 = null Where CompID = @CompID and AddressID = @AddressID ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
            comm.Parameters.Add("@NewName", SqlDbType.NVarChar, 100).Value = Generate.GetRandomName(AddressNameType.Fullname) ;
            comm.Parameters.Add("@NewAddress", SqlDbType.NVarChar, 100).Value = Generate.GetRandomName(AddressNameType.Streetname);
            try { 
            conn.Open();
            comm.ExecuteNonQuery();
            comm.CommandText = "Delete from ad_Addresses_Audit Where CompID = @CompID and AddressID = @AddressID ";
            comm.ExecuteNonQuery();
            }
            finally {
                if (conn != null) conn.Close();
            }
            return errStr;
        }
    }
}