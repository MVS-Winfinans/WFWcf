using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
/// <summary>
/// Summary description for Accounts
/// </summary>
/// 
namespace wfws
{

    public class Accounts
    {
        int compID = 0;
        string ConnectionString;
        Guid CompanyKey = Guid.Empty;

        public Accounts(ref DBUser DBUser)
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

        public void load_ChartOfAccounts(ref IList<AccountItem> ChartOfAccounts, ref string errStr)
        {
            int count = 0;
            SqlConnection Conn = new SqlConnection(ConnectionString);
            string mysql = "SELECT top 2000 Account,Description,AccType,VAT,AccInitial,FromAccount,ToAccount, Currency, Blocked, NotManual FROM fi_ChartOfAccounts WHERE CompID = @CompID ";
            try
            {
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                var account = new AccountItem();
                Conn.Open();
                SqlDataReader myr = Comm.ExecuteReader();
                count = 10;
                AccountType AccT;
                while (myr.Read())
                {
                    account.Account = myr["Account"].ToString();
                    account.Description = myr["Description"].ToString();
                    if (!Enum.TryParse(myr["AccType"].ToString(), out AccT))
                        AccT = AccountType.Undefined;
                    account.AccType = AccT;
                    account.VAT = myr["VAT"].ToString();
                    account.FromAccount = myr["FromAccount"].ToString();
                    account.ToAccount = myr["ToAccount"].ToString();
                    account.Currency = myr["Currency"].ToString();
                    account.Blocked = (bool)myr["Blocked"];
                    account.NotManual = (bool)myr["NotManual"];
                    ChartOfAccounts.Add(account);
                    account = new AccountItem();
                    count = count + 1;
                }
            }
            catch (Exception e) { errStr = e.Message; }
        }

        public void DeleteAccount(AccountItem ThisAccount)
        {

            if (!AccountExists(ThisAccount.Account)) throw new FaultException("wf_wcf: Account does not exist", new FaultCode("wfwcfFault"));
            int errno;
            SqlConnection Conn = new SqlConnection(ConnectionString);
            SqlCommand myComm = new SqlCommand("dbo.we_account_chartOfAccounts_Delete", Conn);
            myComm.CommandType = CommandType.StoredProcedure;
            myComm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            myComm.Parameters.Add("@account", SqlDbType.NVarChar, 20).Value = ThisAccount.Account;
            myComm.Parameters.Add("@ErrNo", SqlDbType.Int).Direction = ParameterDirection.Output;
            Conn.Open();
            myComm.ExecuteNonQuery();
            errno = (int)myComm.Parameters["@ErrNo"].Value;
            Conn.Close();
            if (errno!=0) throw new FaultException("wf_wcf: Account is not deleteable", new FaultCode("wfwcfFault"));
        }
        public void SaveAccount(AccountItem ThisAccount)
        {
            SqlConnection Conn = new SqlConnection(ConnectionString);
            SqlCommand myComm = new SqlCommand("dbo.we_account_chartOfAccounts_save", Conn);
            myComm.CommandType = CommandType.StoredProcedure;
            myComm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            myComm.Parameters.Add("@account", SqlDbType.NVarChar, 20).Value = ThisAccount.Account;
            myComm.Parameters.Add("@AccType", SqlDbType.Int).Value = (int)ThisAccount.AccType;
            myComm.Parameters.Add("@Description", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(ThisAccount.Description) ? (object)DBNull.Value : ThisAccount.Description); 
            myComm.Parameters.Add("@DescriptionShort", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(ThisAccount.Description) ? (object)DBNull.Value : ThisAccount.Description);
            myComm.Parameters.Add("@VAT", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(ThisAccount.VAT) ? (object)DBNull.Value : ThisAccount.VAT);
            myComm.Parameters.Add("@FromAccount", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(ThisAccount.FromAccount) ? (object)DBNull.Value : ThisAccount.FromAccount);
            myComm.Parameters.Add("@ToAccount", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(ThisAccount.ToAccount) ? (object)DBNull.Value : ThisAccount.ToAccount);
            myComm.Parameters.Add("@Blocked", SqlDbType.Int).Value = ThisAccount.Blocked;
            myComm.Parameters.Add("@notManual", SqlDbType.Int).Value = ThisAccount.NotManual;
            myComm.Parameters.Add("@Dim1Yn", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim2Yn", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim3Yn", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim4Yn", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim1use", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim2use", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim3use", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim4use", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim1OnList", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim2OnList", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim3OnList", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@Dim4OnList", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@DimincludeYn", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@DimOnList", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@AccountCat", SqlDbType.NVarChar, 20).Value = DBNull.Value;
            myComm.Parameters.Add("@AccountSumm", SqlDbType.NVarChar, 20).Value = DBNull.Value; 
            myComm.Parameters.Add("@AccInitial", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(ThisAccount.AccInitial) ? (object)DBNull.Value : ThisAccount.AccInitial);
            myComm.Parameters.Add("@HideAccount", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@accConsolidate", SqlDbType.NVarChar, 20).Value = DBNull.Value;
            myComm.Parameters.Add("@permitVATChanges", SqlDbType.Int).Value = 0;
            myComm.Parameters.Add("@currency", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(ThisAccount.Currency) ? (object)DBNull.Value : ThisAccount.Currency);
            Conn.Open();
            myComm.ExecuteNonQuery();
            Conn.Close();
            //convert_Account(account);   kalder [dbo].[we_company_ChartOfAccounts_convert_account]

        }
        private bool AccountExists(string Account)
        {
            bool RetVal = true;
//            SqlConnection Conn = new SqlConnection(ConnectionString);
            //check for existence of account postponed
            return RetVal;
        }
        public string voucher_save(ref  VoucherIn myVo)
        {
            string errStr = "OK";
            try
            {
                if (myVo.Paydate < new DateTime(1800, 1, 1)) myVo.Paydate = DateTime.Today;
                if (myVo.EnterDate < new DateTime(1800, 1, 1)) myVo.EnterDate = DateTime.Today;
                SqlConnection Conn = new SqlConnection(ConnectionString);
                string mysql = "INSERT INTO fi_vouchers_imp ";
                mysql = string.Concat(mysql, " (CompID,Voucher,EnterDate, Invoice, InvoiceNoStr, SettleNo, DAccount,CAccount, Description, Amount, cuAmount,VatCalc,VATYn,Currency,PayDate,Source,SourceRef,Dim1,Dim2,Dim3,Dim4, PaymentRef, AdvisText)");
                mysql = string.Concat(mysql, " values (@P_CompID, @P_Voucher, @P_EnterDate, @P_Invoice, @P_InvoiceStr, @P_SettleNo, ");
                mysql = string.Concat(mysql, " @P_DAccount, @P_CAccount, ");
                mysql = string.Concat(mysql, " @P_Description,  @P_AmountCu,  @P_Amount,@P_VatCalc, @P_VATYn, @P_Currency, @P_PayDate,  @P_Source, @P_Sourceref,@Dim1,@Dim2,@Dim3,@Dim4, @PaymentRef, @AdvisText) ");
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@P_Voucher", SqlDbType.Int).Value = myVo.Voucher;
                Comm.Parameters.Add("@P_EnterDate", SqlDbType.DateTime).Value = myVo.EnterDate;
                Comm.Parameters.Add("@P_Invoice", SqlDbType.BigInt).Value = myVo.Invoice;
                Comm.Parameters.Add("@P_InvoiceStr", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(myVo.InvoiceNoStr) ? (object)DBNull.Value : myVo.InvoiceNoStr);
                Comm.Parameters.Add("@P_SettleNo", SqlDbType.Int).Value = myVo.SettleNo;
                Comm.Parameters.Add("@P_DAccount", SqlDbType.NVarChar).Value = (string.IsNullOrEmpty(myVo.DAccount) ? (object)DBNull.Value : myVo.DAccount);
                Comm.Parameters.Add("@P_CAccount", SqlDbType.NVarChar).Value = (string.IsNullOrEmpty(myVo.CAccount) ? (object)DBNull.Value : myVo.CAccount);
                Comm.Parameters.Add("@P_Description", SqlDbType.NVarChar, 200).Value = (string.IsNullOrEmpty(myVo.Description) ? (object)DBNull.Value : myVo.Description);
                Comm.Parameters.Add("@P_Amount", SqlDbType.Money).Value = myVo.Amount;
                Comm.Parameters.Add("@P_AmountCu", SqlDbType.Money).Value = myVo.cuAmount;
                Comm.Parameters.Add("@P_VatCalc", SqlDbType.Money).Value = myVo.VatCalc;
                Comm.Parameters.Add("@P_VATYn", SqlDbType.Int).Value = ((myVo.VATYn) ? 1 : 0);
                Comm.Parameters.Add("@P_Currency", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myVo.Currency) ? (object)DBNull.Value : myVo.Currency);
                Comm.Parameters.Add("@P_PayDate", SqlDbType.DateTime).Value = myVo.Paydate;
                Comm.Parameters.Add("@P_Source", SqlDbType.Int).Value = myVo.Source;
                Comm.Parameters.Add("@P_Sourceref", SqlDbType.Int).Value = 0;
                Comm.Parameters.Add("@Dim1", SqlDbType.NVarChar,20).Value = (string.IsNullOrEmpty(myVo.Dim1) ? (object)DBNull.Value : myVo.Dim1);
                Comm.Parameters.Add("@Dim2", SqlDbType.NVarChar,20).Value = (string.IsNullOrEmpty(myVo.Dim2) ? (object)DBNull.Value : myVo.Dim2);
                Comm.Parameters.Add("@Dim3", SqlDbType.NVarChar,20).Value = (string.IsNullOrEmpty(myVo.Dim3) ? (object)DBNull.Value : myVo.Dim3);
                Comm.Parameters.Add("@Dim4", SqlDbType.NVarChar,20).Value = (string.IsNullOrEmpty(myVo.Dim4) ? (object)DBNull.Value : myVo.Dim4);
                Comm.Parameters.Add("@PaymentRef", SqlDbType.NVarChar).Value = (string.IsNullOrEmpty(myVo.PaymentRef) ? (object)DBNull.Value : myVo.PaymentRef);
                Comm.Parameters.Add("@AdvisText", SqlDbType.NVarChar).Value = (string.IsNullOrEmpty(myVo.AdvisText) ? (object)DBNull.Value : myVo.AdvisText);
                Conn.Open();
                Comm.ExecuteNonQuery();
                Conn.Close();
            }
            catch (Exception e) { errStr = e.Message; }
            return errStr;
        }

        public string voucher_savepic(VoucherPictures VoucherPict)
        {
            string errStr = "OK";
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                string mysql = "insert fi_vouchers_pict (compID, ScanDate,Picture,FileName,ContentType, enterdate, voucher, SourceDesc, MailSubject ) ";
                mysql = String.Concat(mysql, " values(@compID, Convert(DateTime, FLOOR(Convert(float, getdate()))), @Document,@FileName ,@ContentType, @enterdate, @voucher, @Description, @Subject ) ");
                SqlCommand Comm = new SqlCommand(mysql, conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@ContentType", SqlDbType.NVarChar, 255).Value = VoucherPict.ContentType;
                Comm.Parameters.Add("@Document", SqlDbType.Binary).Value = VoucherPict.Picture;
                Comm.Parameters.Add("@FileName", SqlDbType.NVarChar, 100).Value = VoucherPict.FileName;
                Comm.Parameters.Add("@enterdate", SqlDbType.DateTime).Value = VoucherPict.EnterDate;
                Comm.Parameters.Add("@voucher", SqlDbType.BigInt).Value = VoucherPict.Voucher;
                Comm.Parameters.Add("@Description", SqlDbType.NVarChar, 200).Value = VoucherPict.Description;
                Comm.Parameters.Add("@Subject", SqlDbType.NVarChar, 255).Value = VoucherPict.Subject;
                conn.Open();
                Comm.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e) { errStr = e.Message; }
            return errStr;
        }
        public int VoucherNo_GetNext(string LedgerName, DateTime Enterdate)
        {
            //
            int NewVoucherNo = -1;
            if (!string.IsNullOrEmpty(LedgerName)) { 
                SqlConnection conn = new SqlConnection(ConnectionString);
                SqlCommand comm = new SqlCommand("we_ledger_next_voucherno", conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@P_LedgerName", SqlDbType.NVarChar, 20).Value = LedgerName;
                comm.Parameters.Add("@P_EnterDate", SqlDbType.DateTime).Value = Enterdate;
                comm.Parameters.Add("@O_VoucherNo", SqlDbType.Int).Direction = ParameterDirection.Output;
                conn.Open();
                comm.ExecuteNonQuery();
                NewVoucherNo = (int)comm.Parameters["@O_VoucherNo"].Value;
                conn.Close();
            }
            return NewVoucherNo;
        }

        public void load_Ledgers(ref IList<Ledger> Ledgers)
        {
            SqlConnection Conn = new SqlConnection(ConnectionString);
            string mysql = "SELECT top 2000 LedgerID, LedgerDesc, VoucherFrom, VoucherTo FROM fi_Voucher_ledgers WHERE CompID = @CompID ";
            SqlCommand Comm = new SqlCommand(mysql, Conn);
            Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            Ledger OneLedger = new Ledger();
            Conn.Open();
            SqlDataReader myr = Comm.ExecuteReader();
            while (myr.Read())
            {
                OneLedger.LedgerID = (int)myr["LedgerID"];
                OneLedger.Description = myr["LedgerDesc"].ToString();
                OneLedger.VoucherFrom = (int)myr["VoucherFrom"];
                OneLedger.VoucherTo = (int)myr["VoucherTo"];
                Ledgers.Add(OneLedger);
                OneLedger = new Ledger();
            }
            Conn.Close();
        }

        public void get_vouchers(ref IList<VoucherOut> Vouchers, VoucherCriteria Criteria)
        {
            SqlConnection Conn = new SqlConnection(ConnectionString);
            LookUp test = new LookUp();
            string mysql = "SELECT top 2000 * FROM fi_Years_Items WHERE CompID = @CompID ";
            if (Criteria.FromAccount != null) mysql = string.Concat(mysql, " AND AdAccount >= @FromAccount ");
            if (Criteria.ToAccount != null) mysql = string.Concat(mysql, " AND AdAccount <= @ToAccount ");
            if (test.IsSQLDate(Criteria.Fromdate)) mysql = string.Concat(mysql, " AND Enterdate >= @FromDate ");
            if (test.IsSQLDate(Criteria.Todate)) mysql = string.Concat(mysql, " AND Enterdate <= @Todate ");
            if (Criteria.YearID != 0) mysql = string.Concat(mysql, " AND YearID = @YearID ");
            if (Criteria.FromPeriod != 0) mysql = string.Concat(mysql, " AND Period >= @FromPeriod ");
            if (Criteria.ToPeriod != 0) mysql = string.Concat(mysql, " AND Period <= @ToPeriod ");
            if (Criteria.Type == VoucherType.Debtor) mysql = string.Concat(mysql, " AND Debcr = 1 ");
            if (Criteria.Type == VoucherType.Creditor) mysql = string.Concat(mysql, " AND Debcr = 2 ");
            if (Criteria.Type == VoucherType.DebtorAndCreditor) mysql = string.Concat(mysql, " AND (Debcr = 1 or Debcr = 2) ");
            if (Criteria.Status == VoucherStatus.Settled)
                if (Criteria.SettleID==0)
                    mysql = string.Concat(mysql, " AND SettleID <> 0 ");
                else
                    mysql = string.Concat(mysql, " AND SettleID = ", Criteria.SettleID.ToString() , " ");
            if (Criteria.Status == VoucherStatus.Unsettled) mysql = string.Concat(mysql, " AND SettleID = 0 ");
            if (Criteria.AddressID != 0) mysql = string.Concat(mysql, " AND AddressID = @AddressID ");
            if (Criteria.SourceID != 0) mysql = string.Concat(mysql, " AND SourceID = @SourceID ");
            if (Criteria.SourceRef != 0) mysql = string.Concat(mysql, " AND SourceRef = @SourceRef ");

            mysql = string.Concat(mysql, " ORDER BY Enterdate desc ");

            SqlCommand Comm = new SqlCommand(mysql, Conn);
            Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            if (Criteria.FromAccount != null) Comm.Parameters.Add("@FromAccount", SqlDbType.NVarChar,200).Value = Criteria.FromAccount;
            if (Criteria.ToAccount != null) Comm.Parameters.Add("@ToAccount", SqlDbType.NVarChar, 200).Value = Criteria.ToAccount;
            if (test.IsSQLDate(Criteria.Fromdate)) Comm.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = Criteria.Fromdate;
            if (test.IsSQLDate(Criteria.Todate)) Comm.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = Criteria.Todate;
            if (Criteria.YearID != 0) Comm.Parameters.Add("@YearID", SqlDbType.Int).Value = Criteria.YearID;
            if (Criteria.FromPeriod != 0) Comm.Parameters.Add("@FromPeriod", SqlDbType.Int).Value = Criteria.FromPeriod;
            if (Criteria.ToPeriod != 0) Comm.Parameters.Add("@ToPeriod", SqlDbType.Int).Value = Criteria.ToPeriod;
            if (Criteria.AddressID != 0) Comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = Criteria.AddressID;
            if (Criteria.SourceID != 0) Comm.Parameters.Add("@SourceID", SqlDbType.Int).Value = Criteria.SourceID;
            if (Criteria.SourceRef != 0) Comm.Parameters.Add("@SourceRef", SqlDbType.Int).Value = Criteria.SourceRef;

            VoucherOut OneVoucher = new VoucherOut();
            Conn.Open();
            SqlDataReader myr = Comm.ExecuteReader();
            while (myr.Read())
            {
                OneVoucher.Voucher = (int)myr["Voucher"];
                OneVoucher.EnterDate = (DateTime)(myr["EnterDate"] == DBNull.Value ? DateTime.MinValue : myr["EnterDate"]);
                OneVoucher.Invoice = (int)(myr["InvoiceNo"] == DBNull.Value ? 0 : myr["InvoiceNo"]);
                OneVoucher.SettleNo = (int)(myr["SettleNo"] == DBNull.Value ? 0 : myr["SettleNo"]);
                OneVoucher.SettleId = (int)(myr["SettleId"] == DBNull.Value ? 0 : myr["SettleId"]);
                OneVoucher.Account = myr["Account"].ToString();
                OneVoucher.AdAccount = myr["AdAccount"].ToString();
                OneVoucher.Description = myr["Description"].ToString();
                OneVoucher.Debet = (decimal)myr["Debet"];
                OneVoucher.cuDebet = (decimal)myr["cuDebet"];
                OneVoucher.Credit = (decimal)myr["Credit"];
                OneVoucher.cuCredit = (decimal)myr["cuCredit"];
                OneVoucher.VatCalc = (decimal)myr["VatCalc"];
                OneVoucher.Currency = myr["Currency"].ToString();
                OneVoucher.DateOfPayment = (DateTime)(myr["DateOfPayment"] == DBNull.Value ? DateTime.MinValue : myr["DateOfPayment"]);
                OneVoucher.SourceID = (int)(myr["SourceID"] == DBNull.Value ? 0 : myr["SourceID"]);
                OneVoucher.SourceRef = (int)(myr["SourceRef"] == DBNull.Value ? 0 : myr["SourceRef"]);
                OneVoucher.Dim1 = myr["Dim1"].ToString();
                OneVoucher.Dim2 = myr["Dim2"].ToString();
                OneVoucher.Dim3 = myr["Dim3"].ToString();
                OneVoucher.Dim4 = myr["Dim4"].ToString();
                OneVoucher.PaymentRef = myr["PaymentRef"].ToString();
                OneVoucher.AdvisText = myr["AdvisText"].ToString();
                OneVoucher.ReminderNo = (int)(myr["ReminderNo"] == DBNull.Value ? 0 : myr["ReminderNo"]);
                Vouchers.Add(OneVoucher);
                OneVoucher = new VoucherOut();
            }
            Conn.Close();

        }
        // Dimensions


        public string Dimension_Update(int compID,ref Dimension myDim)
        {
            int dimno = myDim.DimNo;
            string dimid = myDim.DimID;
            string groupid = myDim.GroupID;
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            string retstr = "err2";
            try
            {
                if ((dimno > 0) && (dimid != string.Empty))
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);
                    SqlCommand comm = new SqlCommand("we_dimensions_update", conn);
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                    comm.Parameters.Add("@DimNo", SqlDbType.Int).Value = dimno;
                    comm.Parameters.Add("@DimID", SqlDbType.NVarChar,20).Value = dimid;
                    comm.Parameters.Add("@DimText", SqlDbType.NVarChar,100).Value = (string.IsNullOrEmpty(myDim.DimText) ? DBNull.Value : (object)myDim.DimText);
                    comm.Parameters.Add("@Notes", SqlDbType.NVarChar, 3000).Value = (string.IsNullOrEmpty(myDim.Notes) ? DBNull.Value : (object)myDim.Notes);
                    comm.Parameters.Add("@Closed", SqlDbType.Bit).Value = myDim.Closed; // == null? DBNull.Value : (object)myDim.Closed);
                    comm.Parameters.Add("@BindingOffer", SqlDbType.Bit).Value = myDim.BindingOffer; // == null ? DBNull.Value : (object)myDim.BindingOffer);

                    comm.Parameters.Add("@completion", SqlDbType.Money).Value = myDim.completion; // == null ? 0 : (object)myDim.completion);
                    comm.Parameters.Add("@Budget", SqlDbType.Money).Value = 0;
                    comm.Parameters.Add("@Offer", SqlDbType.Money).Value = myDim.Offer; // == null ? 0 : (object)myDim.Offer);
                    comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = myDim.AddressID; //  == null ? 0 : (object)myDim.AddressID);
                    comm.Parameters.Add("@CompletionDate", SqlDbType.DateTime).Value = (DateTime)((myDim.CompletionDate < minSqlDate) ? (object)DBNull.Value : myDim.CompletionDate);
                    comm.Parameters.Add("@GroupID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myDim.GroupID) ? (object)DBNull.Value : myDim.GroupID);

  

                 // ALTER procedure [dbo].[we_dimensions_update] @CompID int,@DimNo int,
                // @DimID nvarchar(20),@DimText nvarchar(100), @Notes nvarchar(3000),
            // @Closed bit, @BindingOffer bit, @completion money, @Budget money, @Offer money,@AddressID integer, @CompletionDate datetime,@GroupID nvarchar(20)
                   conn.Open();
                   comm.ExecuteNonQuery();
                   conn.Close();
                   retstr = "OK";
                }
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }


        public string Dimension_UpdateLawyer(int compID, ref Dimension myDim)
        {
            int dimno = myDim.DimNo;
            string dimid = myDim.DimID;
            string groupid = myDim.GroupID;
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            string retstr = "err2";
            try
            {
                if ((dimno > 0) && (dimid != string.Empty))
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);
                    SqlCommand comm = new SqlCommand("we_dimensions_update_lawyer", conn);
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                    comm.Parameters.Add("@DimNo", SqlDbType.Int).Value = dimno;
                    comm.Parameters.Add("@DimID", SqlDbType.NVarChar, 20).Value = dimid;
                    comm.Parameters.Add("@DimText", SqlDbType.NVarChar, 100).Value = (string.IsNullOrEmpty(myDim.DimText) ? DBNull.Value : (object)myDim.DimText);
                    comm.Parameters.Add("@Notes", SqlDbType.NVarChar, 3000).Value = (string.IsNullOrEmpty(myDim.Notes) ? DBNull.Value : (object)myDim.Notes);
                    comm.Parameters.Add("@Closed", SqlDbType.Bit).Value = myDim.Closed; // == null? DBNull.Value : (object)myDim.Closed);
                    comm.Parameters.Add("@BindingOffer", SqlDbType.Bit).Value = myDim.BindingOffer; // == null ? DBNull.Value : (object)myDim.BindingOffer);
                    comm.Parameters.Add("@completion", SqlDbType.Money).Value = myDim.completion; // == null ? 0 : (object)myDim.completion);
                    comm.Parameters.Add("@Budget", SqlDbType.Money).Value = 0;
                    comm.Parameters.Add("@Offer", SqlDbType.Money).Value = myDim.Offer; // == null ? 0 : (object)myDim.Offer);
                    comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = myDim.AddressID; //  == null ? 0 : (object)myDim.AddressID);
                    comm.Parameters.Add("@CompletionDate", SqlDbType.DateTime).Value = (DateTime)((myDim.CompletionDate < minSqlDate) ? (object)DBNull.Value : myDim.CompletionDate);
                    comm.Parameters.Add("@GroupID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myDim.GroupID) ? (object)DBNull.Value : myDim.GroupID);
                    comm.Parameters.Add("@Parent", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myDim.GroupID) ? (object)DBNull.Value : myDim.Parent);


                    // ALTER procedure [dbo].[we_dimensions_update] @CompID int,@DimNo int,
                    // @DimID nvarchar(20),@DimText nvarchar(100), @Notes nvarchar(3000),
                    // @Closed bit, @BindingOffer bit, @completion money, @Budget money, @Offer money,@AddressID integer, @CompletionDate datetime,@GroupID nvarchar(20)
                    conn.Open();
                    comm.ExecuteNonQuery();
                    conn.Close();
                    retstr = "OK";
                }
            }
            catch (Exception e) { retstr = e.Message; }
            return retstr;
        }



        public void load_Dimensions(ref Dimension wfDim, ref IList<DimensionItem> DimList, ref string errstr)
        {
            int count = 0;
            int Dimno = wfDim.DimNo;
            SqlConnection Conn = new SqlConnection(ConnectionString);
            string mysql = "SELECT DimNo, DimID, DimText,isnull(AddressID,0) as AddressID FROM fi_Dimensions WHERE CompID = @CompID AND DimNo = @DimNo ";
            try
            {

                Boolean closed = wfDim.Closed;
                if (closed) mysql = String.Concat(mysql, " AND Closed <> 0 ");
                if (!closed) mysql = String.Concat(mysql, " AND Closed = 0 ");

                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@DimNo", SqlDbType.Int).Value = Dimno;
                var DimItem = new DimensionItem();
                Conn.Open();
                SqlDataReader myr = Comm.ExecuteReader();
                count = 10;
                while (myr.Read())
                {
                    DimItem.DimNo = (Int32)myr["DimNo"];
                    DimItem.DimID = myr["DimID"].ToString();
                    DimItem.AddressID = (Int32)myr["AddressID"];
                    DimItem.DimText = myr["DimText"].ToString();
                    DimList.Add(DimItem);
                    DimItem = new DimensionItem();
                    count = count + 1;
                }
            }
            catch (Exception e) { errstr = e.Message; }
        }


        public void load_Dimensions_Lawyer_statement(string Dim3, ref IList<Dimensions_ClientStatement> DimList, ref string errstr)
        {
            int count = 0;
            SqlConnection Conn = new SqlConnection(ConnectionString);
            try
            {
                SqlCommand Comm = new SqlCommand("wf_la_case_expenses_one", Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = Dim3; // (string.IsNullOrEmpty(Dim3) ? DBNull.Value : (object)Dim3);
                Comm.CommandType = CommandType.StoredProcedure;
                var DimItem = new Dimensions_ClientStatement();
                Conn.Open();
                SqlDataReader myr = Comm.ExecuteReader();
                count = 10;
                while (myr.Read())
                {
                    DimItem.FiItemID = (Int32)myr["FiItemID"];
                    DimItem.Dim3 = myr["Dim3"].ToString();
                    DimItem.EnterDate = (DateTime)myr["EnterDate"];
                    DimItem.Account = myr["Account"].ToString();
                    DimItem.GroupFI = myr["GroupFI"].ToString();
                    DimItem.ItemID = myr["ItemID"].ToString();
                    DimItem.ItemDesc = myr["ItemDesc"].ToString();

                    DimItem.Amount = (decimal)myr["Amount"];
                    DimItem.Invoiceno = (Int32)myr["Invoiceno"];
                    DimItem.SaleID = (int)myr["SaleID"];
                    DimItem.Reconciled = (int)myr["Reconciled"];
                    DimItem.OffsetAccount = myr["OffsetAccount"].ToString();
                    DimItem.AccBank = myr["AccBank"].ToString();
                    DimItem.AccClient = myr["AccClient"].ToString();
                    DimList.Add(DimItem);
                    DimItem = new Dimensions_ClientStatement();
                    count = count + 1;
                }
            }
            catch (Exception e) { errstr = e.Message; }
        }

      

        public int Dimension_Get(ref  Dimension myDim)
        {
            int Dimno = myDim.DimNo;
            string DimID = myDim.DimID;
            int count = 0;
            if ((Dimno > 0) && (DimID != String.Empty))
            {
                SqlConnection Conn = new SqlConnection(ConnectionString);
                string mysql = " SELECT * FROM fi_Dimensions Where CompID = @CompID AND DimNo = @DimNo AND DimID = @DimID ";
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@DimNo", SqlDbType.Int).Value = Dimno;
                Comm.Parameters.Add("@DimID", SqlDbType.NVarChar, 20).Value = DimID;
                Conn.Open();
                SqlDataReader myr = Comm.ExecuteReader();

                while (myr.Read())
                {
                    count = count + 1;
                    myDim.AddressID = (Int32)((myr["AddressID"] == DBNull.Value) ? 0 : myr["AddressID"]);
                    myDim.DimText = myr["DimText"].ToString();
                    myDim.Notes = myr["Notes"].ToString();
                    myDim.BindingOffer = (Boolean)((myr["BindingOffer"] == DBNull.Value) ? false : myr["BindingOffer"]);
                    myDim.Closed = (Boolean)(((myr["Closed"] == DBNull.Value) ? false : myr["Closed"]));
                    myDim.CompletionDate = (DateTime)((myr["CompletionDate"] == DBNull.Value) ? DateTime.Today : myr["CompletionDate"]);
                    myDim.DateUpdate = (DateTime)((myr["DateUpdate"] == DBNull.Value) ? DateTime.Today : myr["DateUpdate"]);
                    myDim.EnterDate = (DateTime)((myr["EnterDate"] == DBNull.Value) ? DateTime.Today : myr["EnterDate"]);
                    myDim.Offer = (Int32)((myr["Offer"] == DBNull.Value) ? 0 : myr["Offer"]);
                    myDim.GroupID = myr["GroupID"].ToString();
                    myDim.Parent = myr["Parent"].ToString();

                }
                Conn.Close();

            }
            return count;
        }

        public int Dimension_TimeItemsAdd(ref fi_dimensions_timeitems myItem)
        {
                  
            int oItemID = 0;
            SqlConnection Conn = new SqlConnection(ConnectionString);
            SqlCommand Comm = new SqlCommand("wf_web_Dim_Timeitems_Add", Conn);
            Comm.CommandType = CommandType.StoredProcedure;
            Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            Comm.Parameters.Add("@employee", SqlDbType.NVarChar, 20).Value = myItem.employee;
            Comm.Parameters.Add("@Type", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myItem.vType) ? (object)DBNull.Value : myItem.vType);
            Comm.Parameters.Add("@invItemID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myItem.invItemID) ? (object)DBNull.Value : myItem.invItemID);
            Comm.Parameters.Add("@Dim1", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myItem.Dim1) ? (object)DBNull.Value : myItem.Dim1);
            Comm.Parameters.Add("@Dim2", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myItem.Dim2) ? (object)DBNull.Value : myItem.Dim2);
            Comm.Parameters.Add("@Dim3", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myItem.Dim3) ? (object)DBNull.Value : myItem.Dim3);
            Comm.Parameters.Add("@Dim4", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myItem.Dim4) ? (object)DBNull.Value : myItem.Dim4);
            Conn.Open();
            SqlDataReader myr = Comm.ExecuteReader();
            while (myr.Read())
                {
                  myItem.ItemID = (int)myr["ItemID"];
                  myItem.Class = (int)myr["Class"];
                  myItem.employee = myr["employee"].ToString();
                  myItem.enterDate = (DateTime)myr["enterDate"];
                  myItem.vType = myr["Type"].ToString();
                  myItem.Description = myr["Description"].ToString();
                  myItem.Quantity = (decimal)((myr["Quantity"] == DBNull.Value) ? 0 : (decimal)myr["Quantity"]);
                  myItem.Quantity_invoiced = (decimal)((myr["Quantity_invoiced"] == DBNull.Value) ? 0 : (decimal)myr["Quantity_invoiced"]);
                  myItem.Price = (decimal)((myr["Price"] == DBNull.Value) ? 0 : (decimal)myr["Price"]);
                  myItem.CostPrice = (decimal)((myr["CostPrice"] == DBNull.Value) ? 0 : (decimal)myr["CostPrice"]);
                  myItem.Dim1 = myr["Dim1"].ToString();
                  myItem.Dim1 = myr["Dim2"].ToString();
                  myItem.Dim1 = myr["Dim3"].ToString();
                  myItem.Dim1 = myr["Dim4"].ToString();
                  myItem.ToInvoice = (Int32)((myr["ToInvoice"] == DBNull.Value) ? 0 : (Int32)myr["ToInvoice"]);
                  myItem.Price = (decimal)((myr["HoursPerUnit"] == DBNull.Value) ? 0 : (decimal)myr["HoursPerUnit"]);
                  myItem.invItemID = myr["invItemID"].ToString();
                }
            Conn.Close();
            oItemID = myItem.ItemID;
            return oItemID;
        }

        public string Dimension_TimeItemsUpdate(ref fi_dimensions_timeitems myItem)
        {
            if (myItem.enterDate < new DateTime(1900, 1, 1)) myItem.enterDate = DateTime.Today;
            string errStr = "OK";
            string mysql = "update fi_dimensions_timeitems set ";
            mysql = string.Concat(mysql, " enterDate = @enterDate, Type = @Type, Description = @Description, Quantity = @Quantity, ");
            mysql = string.Concat(mysql, " Quantity_invoiced = @Quantity_invoiced, Price = @Price, CostPrice = @CostPrice,  ToInvoice = @ToInvoice, ");
            mysql = string.Concat(mysql, " HoursPerUnit = @HoursPerUnit, invItemID = @invItemID ");
            mysql = string.Concat(mysql, " where CompID = @CompID AND ItemID = @ItemID ");
            SqlConnection Conn = new SqlConnection(ConnectionString);
            SqlCommand Comm = new SqlCommand(mysql, Conn);
            Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            Comm.Parameters.Add("@ItemID", SqlDbType.NVarChar,20).Value = myItem.ItemID;
            Comm.Parameters.Add("@enterDate", SqlDbType.DateTime).Value = myItem.enterDate;
            Comm.Parameters.Add("@Type", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myItem.vType) ? (object)DBNull.Value : myItem.vType);
            Comm.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (string.IsNullOrEmpty(myItem.Description) ? (object)DBNull.Value : myItem.Description);
            Comm.Parameters.Add("@Quantity", SqlDbType.Money).Value = myItem.Quantity;
            Comm.Parameters.Add("@Quantity_invoiced", SqlDbType.Money).Value = myItem.Quantity_invoiced;
            Comm.Parameters.Add("@Price", SqlDbType.Money).Value = myItem.Price;
            Comm.Parameters.Add("@CostPrice", SqlDbType.Money).Value = myItem.CostPrice;
            Comm.Parameters.Add("@ToInvoice", SqlDbType.Int).Value = myItem.ToInvoice;
            Comm.Parameters.Add("@HoursPerUnit", SqlDbType.Money).Value = myItem.HoursPerUnit;
            Comm.Parameters.Add("@invItemID", SqlDbType.NVarChar, 20).Value = (string.IsNullOrEmpty(myItem.invItemID) ? (object)DBNull.Value : myItem.invItemID);
         
            Conn.Open();
            Comm.ExecuteNonQuery();
            Conn.Close();
            return errStr;
        }


        public int Dimension_TimeItemsToInvoice(DateTime EnterDate)
        {
            int iCount = 0;
            SqlConnection Conn = new SqlConnection(ConnectionString);
            SqlCommand Comm = new SqlCommand("we_timeitems_to_invoice_all", Conn);
            Comm.CommandType = CommandType.StoredProcedure;
            Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            Comm.Parameters.Add("@EnterDate", SqlDbType.DateTime).Value = EnterDate;
            Comm.Parameters.Add("@iCount", SqlDbType.Int).Direction = ParameterDirection.Output;
            Conn.Open();
            Comm.ExecuteNonQuery();
            iCount = (int)Comm.Parameters["@iCount"].Value;
            Conn.Close();
            return iCount;
        }




        public bool IsValidAccountingCost(string TestAccountingCost)
        {
            bool Answer = true;
            if (!string.IsNullOrEmpty(TestAccountingCost))
            {
                string[] TestParts = TestAccountingCost.Split(';');
                if (TestParts.Count() > 1)
                {
                    if ((TestParts[0] == "") | (IsValidAccount(TestParts[0])))
                    {
                        for (int i = 1; i < TestParts.Count(); i++)
                        {
                            if (!IsValidDimension(i, TestParts[i])) Answer = false;
                        }
                    }
                    else
                        Answer = false;
                }
            }
            return Answer;
        }

        public bool IsValidAccount(string TestAccount)
        {
            bool Answer = false;
            string mysql = "";
            if (TestAccount.IndexOf('V') + TestAccount.IndexOf('C') + TestAccount.IndexOf('v') + TestAccount.IndexOf('c') == 1)
            {
                mysql = "SELECT * FROM ad_Addresses Where CompID = @CompID AND ad_Account = @Account ";
                TestAccount = TestAccount.Substring(1);
            }
            else
                mysql = "SELECT * FROM fi_ChartOfAccounts Where CompID = @CompID AND Account = @Account and (accType = 1 or accType = 2)";

            SqlConnection Conn = new SqlConnection(ConnectionString);
            SqlCommand Comm = new SqlCommand(mysql, Conn);
            Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            Comm.Parameters.Add("@Account", SqlDbType.NVarChar, 20).Value = TestAccount;
            Conn.Open();
            SqlDataReader myr = Comm.ExecuteReader();
            if (myr.Read()) Answer = true;

            return Answer;
        }

        public bool IsValidDimension(int TestDimensionNo, string TestDimensionID)
        {
            bool Answer = false;
            if (TestDimensionID == "") Answer = true;
            else
            {
                string mysql = "SELECT * FROM fi_Dimensions Where CompID = @CompID AND DimNo = @DimNo AND DimID = @DimID ";
                SqlConnection Conn = new SqlConnection(ConnectionString);
                SqlCommand Comm = new SqlCommand(mysql, Conn);
                Comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                Comm.Parameters.Add("@DimNo", SqlDbType.Int).Value = TestDimensionNo;
                Comm.Parameters.Add("@DimID", SqlDbType.NVarChar, 20).Value = TestDimensionID;
                Conn.Open();
                SqlDataReader myr = Comm.ExecuteReader();
                if (myr.Read()) Answer = true;
            }
            return Answer;
        }


    }
}