using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class Service : IService
{
    public int IService(ref DBUser DBUser)
    {
        DBUser.DBKey = Guid.Empty;
        return 0;
    }
    public string HelloWorld()
    {
        return "hello string";
    }
    public string HelloWorldError()
    {
        int val1 = 10;
        int val2 = 0;
        string errstr = "OK";
        try
        {
            int myint = val1 / val2;
        }
        catch (Exception ex)
        {
            errstr = ex.Message;
            new TraceTo(new DBUser(), ex);
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return "hello string";
    }
    public string Ping()
    {
        string AppName = string.Empty;
        System.Reflection.Assembly thisAssembly = this.GetType().Assembly;
        object[] attributes = thisAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length == 1)
        {
            AppName = (((AssemblyTitleAttribute)attributes[0]).Title);
        }
        return String.Format("{0} ({1})", AppName, Assembly.GetExecutingAssembly().GetName().Version);
    }
    public DBUser CreateDBUser(Guid DBKey)
    {
        DBUser DBUser = new DBUser();
        DBUser.DBKey = DBKey;
        DBUser.BlindUpdate = false;
        DBUser.CompanyKey = Guid.Empty;
        return DBUser;
    }
    public string ConnectionTest(ref DBUser DBUser)
    {
        string RetVal = "";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            RetVal = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.PublicConnection == false) RetVal = "Connection string is not public";
        }
        catch (NullReferenceException ex) { throw new FaultException(string.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault")); }
        return RetVal;
    }
    public Guid[] GetServiceProviderAdmissions(Guid ServiceProviderID)
    {
        DBUser DBUser = new DBUser();       //Not needed - just for declaring ConnectLocal object.
        var wfconn = new wfws.ConnectLocal(DBUser);
        try
        {
            return wfconn.FetchServiceProviderAdmissions(ServiceProviderID);
        }
        catch (NullReferenceException ex) { throw new FaultException(string.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault")); }
    }
    public string UserLogin(ref DBUser DBUser, string username, string password, ref Address wfAddress)
    {
        string errstr = "Err";
        string connstr;
        int adrID = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                errstr = "OK";
                errstr = wfconn.UserGetByPassword(ref DBUser, username, password);
                wfws.web wfweb = new wfws.web(ref DBUser);
                adrID = wfweb.Address_GetByAdrGuid(wfconn.UserGuid);
                errstr = wfconn.UserGuid.ToString();
                wfAddress.AddressID = adrID;
                if (adrID > 0) errstr = wfweb.Address_Get(ref wfAddress);
            }
        }
        catch (NullReferenceException ex) { errstr = ex.Message; throw new FaultException(string.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault")); }
        return errstr;
    }
    public string UserPassword(ref DBUser DBUser, ref string password, ref string email, ref Address wfAddress)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                wfconn.UserGuid = wfAddress.AdrGuid;
                errstr = wfconn.PasswordNew(password);
                password = wfconn.newPassword;
                email = wfconn.UserEmail; // wfconn.UserGuid.ToString() ' wfconn.UserHash
            }
        }
        catch (NullReferenceException ex) { errstr = ex.Message; throw new FaultException(string.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault")); }
        return errstr;
    }
    public string UserUpdate(ref DBUser DBUser, string username, string email, ref Address wfAddress)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                wfconn.UserGuid = wfAddress.AdrGuid;
                errstr = wfconn.UserUpdate(username, email);
            }
        }
        catch (NullReferenceException ex) { errstr = ex.Message; throw new FaultException(string.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault")); }
        return errstr;
    }
    public string UserAddNew(ref DBUser DBUser, string username, string email, ref Address wfAddress)
    {
        string errstr = "Err";
        int AdrID = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                errstr = wfconn.UserEsists(ref DBUser, username, email);
                if (errstr == "OK")
                {
                    errstr = wfconn.UserAddNew(ref DBUser, username, email);
                    wfws.web wfweb = new wfws.web(ref DBUser);
                    errstr = wfweb.Address_add(wfconn.UserGuid, String.Empty, ref AdrID);
                    if (AdrID > 0)
                    {
                        errstr = wfweb.Address_Get_defaults(AdrID, ref wfAddress);
                        errstr = wfweb.Address_Update(AdrID, ref wfAddress, true);
                        wfAddress.AdrGuid = wfconn.UserGuid;
                        wfAddress.AddressID = AdrID;
                    }
                }
            }
        }
        catch (NullReferenceException ex) { errstr = ex.Message; throw new FaultException(string.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault")); }
        return errstr;
    }
    // Company
    public CompanyInf CompanyLoad(ref DBUser DBUser)
    {
        CompanyInf companyinf = new CompanyInf();
        string errstr = "OK";
        string connstr;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfcompany = new wfws.Company(ref DBUser);
                errstr = wfcompany.Company_Load(ref DBUser, ref companyinf);
                //int sellerid = wfcompany.Seller_Lookup(ref wfSeller,ref seCount);
                //if (wfSeller.SellerID > 0) { errstr = wfcompany.Seller_Get(ref wfSeller);}
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return companyinf;
    }
    public string CompanySellerLookup(ref DBUser DBUser, ref CompanySeller wfSeller, ref int seCount)
    {
        string errstr = String.Concat("No seller: ", wfSeller.SellerID);
        string connstr;
        seCount = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfcompany = new wfws.Company(ref DBUser);
                int sellerid = wfcompany.Seller_Lookup(ref wfSeller, ref seCount);
                if (wfSeller.SellerID > 0) { errstr = wfcompany.Seller_Get(ref wfSeller); }
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public Boolean CompanyUserLookup(ref DBUser DBUser, string username, string password, ref string retstr)
    {
        Boolean isUser = false;
        string connstr;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfcompany = new wfws.Company(ref DBUser);
                int Usercount = wfcompany.is_company_user(username, ref retstr);
                DBUser.Message = string.Concat("DB user : ", Usercount.ToString());
                if (Usercount > 0) { isUser = wfconn.Is_wf_user(username, password); }
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return isUser;
    }
    public Activity[] CompanyActivitiesLookup(ref DBUser DBUser, ref Activity wfActivity, ref string retstr)
    {
        IList<Activity> items = new List<Activity>();
        Activity lineitem = new Activity();
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Activities_Items_get_user(ref wfActivity, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public CompanyUser[] CompanyUsersLoad(ref DBUser DBUser)
    {
        IList<CompanyUser> items = new List<CompanyUser>();
        CompanyUser lineitem = new CompanyUser();
        string errstr = "Err";
        string connstr;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfcompany = new wfws.Company(ref DBUser);
                wfcompany.Company_users_load(ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public CompanySalesman[] CompanySalesmenLoad(ref DBUser DBUser)
    {
        IList<CompanySalesman> items = new List<CompanySalesman>();
        CompanySalesman lineitem = new CompanySalesman();
        string errstr = "Err";
        string connstr;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfcompany = new wfws.Company(ref DBUser);
                wfcompany.Company_Salesmen_load(ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public int[] CompanySellersLoad(ref DBUser DBUser)
    {
        IList<int> items = new List<int>();
        string errstr = "Err";
        string connstr;
        DateTime newdate = DateTime.Now;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfcompany = new wfws.Company(ref DBUser);
                errstr = wfcompany.Company_load_Sellers(ref items);
            }
            DBUser.Message = errstr;
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public wflist[] WF_GetLists(wfmodule module, string language)
    {
        wfws.LookUp Looker = new wfws.LookUp();
        return Looker.GetWFClasses(module, language);
    }
    public wflist[] WF_GetDropdownList(wfdroplist List, string language)
    {
        wfws.LookUp Looker = new wfws.LookUp();
        return Looker.GetDropdownList(List, language);
    }
    // Address
    public string AddressUpdate(ref DBUser DBUser, ref Address wfAddress)
    {
        string errstr = "Err";
        int AdrID = 0;
        var newAddress = new Address();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfAddress.AdrGuid == Guid.Empty) wfAddress.AdrGuid = Guid.NewGuid();
                DBUser.Message = AdrID.ToString();
                if (wfAddress.AddressID <= 0)
                {
                    wfweb.Address_add(wfAddress.AdrGuid, String.Empty, ref AdrID);
                    wfAddress.AddressID = AdrID;
                    newAddress.AddressID = AdrID;
                    wfweb.Address_Get(ref newAddress);
                    if (string.IsNullOrEmpty(wfAddress.DebtorGroup)) wfAddress.DebtorGroup = newAddress.DebtorGroup;
                    if (wfAddress.SellerID == 0) wfAddress.SellerID = newAddress.SellerID;
                    wfAddress.TimeChanged = newAddress.TimeChanged;
                }
                DBUser.Message = string.Concat(DBUser.Message, "-", wfAddress.AddressID.ToString());
                if (wfAddress.AddressID > 0)
                {
                    AdrID = wfAddress.AddressID;
                    wfAddress.Account = wfweb.Address_check_account_number(wfAddress.Account, AdrID);
                    wfweb.Address_Update(AdrID, ref wfAddress, DBUser.BlindUpdate);
                    wfAddress.AddressID = AdrID;
                    DBUser.Message = string.Concat(DBUser.Message, "//", AdrID.ToString(), "--", wfAddress.AddressID);
                }
                errstr = "OK";
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string AddressGetById(ref DBUser DBUser, Guid AdrGuid, ref Address wfAddress)
    {
        string errstr = "Err";
        try
        {
            wfAddress.AddressID = 0;
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                int AdrID = wfweb.Address_GetByAdrGuid(AdrGuid);
                wfAddress.AddressID = AdrID;
                if (wfAddress.AddressID > 0)
                {
                    errstr = wfweb.Address_Get(ref wfAddress);
                    wfAddress.AddressID = AdrID;
                    errstr = "OK";
                }
                else
                {
                    errstr = String.Concat("No address: ", AdrID);
                }
            }
            else
            {
                errstr = "no company";
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string AddressLookup(ref DBUser DBUser, ref Address wfAddress, ref int adCount)
    {
        string errstr = "Err";
        adCount = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                int AdrID = wfweb.Address_Lookup(ref wfAddress, ref adCount);
                wfAddress.AddressID = AdrID;
                if (wfAddress.AddressID > 0)
                {
                    errstr = wfweb.Address_Get(ref wfAddress);
                    DBUser.Message = errstr;
                    errstr = "OK";
                }
                else
                {
                    errstr = String.Concat("No address: ", AdrID);
                }
            }
            else
            {
                errstr = "no company";
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public int AddressLogin(ref DBUser DBUser, string UserName, string HashKey)
    {
        string errstr = "Err";
        int AdrID = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                AdrID = wfweb.Address_Login(UserName, HashKey);
            }
            else
            {
                errstr = "no company";
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return AdrID;
    }
    public string AddressNewHashKey(ref DBUser DBUser, int AdrID, string HashKey)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                DBUser.Message = AdrID.ToString();
                if (AdrID > 0)
                {
                    wfweb.Address_UpdateHashKey(AdrID, HashKey);
                }
                errstr = "OK";
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public AddressItem[] AddressListGet(ref DBUser DBUser, string propertyID, ref string errstr)
    {
        errstr = "Err";
        IList<AddressItem> items = new List<AddressItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_items_load(propertyID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public AddressItem[] AddressLookupList(ref DBUser DBUser, ref Address wfAddress, ref string errstr)
    {
        errstr = "Err";
        IList<AddressItem> items = new List<AddressItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_items_load_find(ref wfAddress, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string AddressApprove(ref DBUser DBUser, Address wfAddress, string ApprovedBy)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_ApproveAudit(wfAddress.AddressID, ApprovedBy);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    // adresses ShipBill
    public AddressesShipBillItem[] AddressShipBillLoad(ref DBUser DBUser, ref Address wfAddress)
    {
        IList<AddressesShipBillItem> items = new List<AddressesShipBillItem>();
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Addresses_ShipBillTo_Load(wfAddress.AddressID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }

    public AddressesShipBillItem[] AddressBillToLoad(ref DBUser DBUser, ref Address wfAddress)
    {
        IList<AddressesShipBillItem> items = new List<AddressesShipBillItem>();
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Addresses_BillTo_Load(wfAddress.AddressID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }



    public string AddressShipBillAdd(ref DBUser DBUser, ref Address wfAddress, ref AddressesShipBillItem ShipBillItem)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Addresses_ShipBillTo_add(wfAddress.AddressID, ref ShipBillItem);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string AddressShipBillDelete(ref DBUser DBUser, ref Address wfAddress, ref AddressesShipBillItem ShipBillItem)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Addresses_ShipBillTo_Delete(wfAddress.AddressID, ref ShipBillItem);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }


    public string AddressShipBillAddAsBillTo(ref DBUser DBUser, ref Address wfAddress, ref AddressesShipBillItem ShipBillItem)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Addresses_ShipBillTo_add_BillTo(wfAddress.AddressID, ref ShipBillItem);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string AddressShipBillDeleteAsBillTo(ref DBUser DBUser, ref Address wfAddress, ref AddressesShipBillItem ShipBillItem)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Addresses_ShipBillTo_Delete_BillTo(wfAddress.AddressID, ref ShipBillItem);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }





    public int[] AddressListChanged(ref DBUser DBUser, ref int AddressID, ref DateTime TimeChanged)
    {
        IList<int> items = new List<int>();
        string errstr = "Err";
        DateTime newdate = DateTime.Now;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_load_Changed(ref items, ref AddressID, ref TimeChanged, 0);
            }
            if (AddressID == 0) TimeChanged = newdate;
            DBUser.Message = errstr;
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    //public int[] AddressListReminders(ref DBUser DBUser, ref int LastAddressID, ReminderLevel ReminderLevel)
    //{       //REMOVED FROM INTERFACE
    //    IList<int> items = new List<int>();
    //    string errstr = "Err";
    //    try
    //    {
    //        var wfconn = new wfws.ConnectLocal(DBUser);
    //        errstr = wfconn.ConnectionGetByGuid(ref DBUser);
    //        if (DBUser.CompID > 0)
    //        {
    //            wfws.web wfweb = new wfws.web(ref DBUser);
    //            errstr = wfweb.Address_load_Reminded(ref items, ref LastAddressID, ReminderLevel);
    //        }
    //        DBUser.Message = errstr;
    //    }
    //    catch (NullReferenceException ex)
    //    {
    //        errstr = ex.Message;
    //        throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
    //    }
    //    return items.ToArray();
    //}
    public AddressShipTo[] AddressListChangedShip(ref DBUser DBUser, ref int AddressID, ref DateTime TimeChanged)
    {
        IList<AddressShipTo> items = new List<AddressShipTo>();
        string errstr = "Err";
        DateTime newdate = DateTime.Now;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (AddressID == 0)
                {
                    wfweb.Address_update_timeChange(ref TimeChanged);
                }
                errstr = wfweb.Address_load_ChangedShipTo(ref items, ref AddressID, ref TimeChanged);
            }
            if (AddressID == 0) TimeChanged = newdate;
            DBUser.Message = errstr;
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    // Addresses payments
    public CreditCardErrors[] AddressGetCreditCardErrors(ref DBUser DBUser, int AddressID, int Status, DateTime FromDate)
    {
        IList<CreditCardErrors> items = new List<CreditCardErrors>();
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_load_CreditCardErrors(ref items, AddressID, Status, FromDate);
            }
            DBUser.Message = errstr;
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    // Activities
    public string AddressActivityAssociate(ref DBUser DBUser, ref Activity NewActivity, ref Address wfAddress)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                int adrID = wfAddress.AddressID;
                errstr = wfweb.Address_add_Activity(adrID, ref NewActivity);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public Activity[] AddressActivitiesLoad_passthrough(ref DBUser DBUser, int UserTop, string UserWhere, string UserOrder)
    {
        IList<Activity> items = new List<Activity>();
        Activity lineitem = new Activity();
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Activities_Items_get_passthrough(ref items, UserTop, UserWhere, UserOrder);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public Activity[] AddressActivitiesLoad(ref DBUser DBUser, ref Address wfAddress)
    {
        IList<Activity> items = new List<Activity>();
        Activity lineitem = new Activity();
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Activities_Items_get(wfAddress.AddressID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string AddressActivityLoad(ref DBUser DBUser, ref Activity wfActivity)
    {
        string retstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_Activity_Load(ref wfActivity);
            }
            else
            {
                retstr = " DBUser error: CompID = 0 ";
            }

        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }
    public string AddressActivityUpdate(ref DBUser DBUser, ref Activity wfActivity)
    {
        string retstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_Activity_Update(ref wfActivity);
            } 
             else 
            { 
                retstr = " DBUser error: CompID = 0 ";  
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }
    public string AddressActivityDelete(ref DBUser DBUser, ref Activity wfActivity)
    {
        string retstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_Activity_Delete(ref wfActivity);
            }
            else
            {
                retstr = " DBUser error: CompID = 0 ";
            }

        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }
    public ActivityType[] AddressActivityTypesLoad(ref DBUser DBUser)
    {
        IList<ActivityType> items = new List<ActivityType>();
        ActivityType lineitem = new ActivityType();
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Activitie_Types_get(ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public ActivityState[] AddressActivityStateLoad(ref DBUser DBUser)
    {
        IList<ActivityState> items = new List<ActivityState>();
        ActivityState lineitem = new ActivityState();
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Activitie_state_get(ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    // Properties
    public string AddressPropertyAdd(ref DBUser DBUser, int Propertyid, ref Address wfAddress)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_properties_add(wfAddress.AddressID, Propertyid);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string AddressPropertyDelete(ref DBUser DBUser, int FromID, int ToID, ref Address wfAddress)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_properties_delete(wfAddress.AddressID, FromID, ToID);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string AddressPropertyPresent(ref DBUser DBUser, int Propertyid, ref Address wfAddress, ref Boolean present)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                int adrID = wfAddress.AddressID;
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_properties_present(adrID, Propertyid, ref present);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public AddressProperty[] AddressPropertiesLoadAll(ref DBUser DBUser, int FromID, int ToID, ref string errstr)
    {
        errstr = "Err";
        IList<AddressProperty> items = new List<AddressProperty>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_properties_loadAll(FromID, ToID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public int[] AddressPropertyListGet(ref DBUser DBUser, int propertyID)
    {
        string errstr = "Err";
        int[] items = null;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                items = wfweb.Address_properties_load(propertyID);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items;
    }
    public string AddressStatementGet(ref DBUser DBUser, ref AddressStatement wfAddressStatement)
    {       //notice a more detailed function in AccVouchersGet()
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_Statement_Load(ref wfAddressStatement);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string AddressStatementGetOpen(ref DBUser DBUser, ref AddressStatement wfAddressStatement)
    {  //notice a more detailed function in AccVouchersGet()
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_Statement_LoadOpen(ref wfAddressStatement);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public AddressCollectable[] AddressCollectablesGet(ref DBUser DBUser)
    {
        string errstr = "Err";
        AddressCollectable[] answer = null;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                answer = wfweb.Address_Collectables_Load();
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return answer;
    }
    public string AddressCollectablesUpdateStatus(ref DBUser DBUser, int AddressID, int NewStatus)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_Collectables_UpdateStatus(AddressID, NewStatus);
            }
            errstr = "OK";
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public int[] AddressDocumentListGet(ref DBUser DBUser, int AddressID)
    {
        string errstr = "Err";
        int[] answer = null;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                answer = wfweb.Address_Documents_Load(AddressID);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return answer;
    }

   

    public AddressDocument[] AddressDocumentsGet(ref DBUser DBUser, int AddressID)
    {
        string errstr = "Err";
        AddressDocument[] answer = null;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                answer = wfweb.Address_Documents_Get(AddressID);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return answer;
    }
    public byte[] AddressGetArchivedDocument(ref DBUser DBUser, int AddressID, int DocID, ref string ContentType, ref string Description)
    {
        string errstr = "OK";
        byte[] answer = null;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                answer = wfweb.Address_Document_Get(AddressID, DocID, ref ContentType, ref Description);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return answer;
    }
    public string AddressBlur(ref DBUser DBUser, int AddressID)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Address_Blur(AddressID);
            }

        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    // Contacts
    public string ContactNew(ref DBUser DBUser, ref Contact NewContact)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Contact_add_new(ref NewContact);

            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }

    public Contact[] ContactsLoad(ref DBUser DBUser,int AddressID)
    {
        string errstr = "Err";
        List<Contact> items = new List<Contact>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                wfweb.Contacts_load(AddressID,ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }






    // Alerts
    public AddressAlert[] AddressAlertLoad(ref DBUser DBUser, int AddressID, int AlertsTop, ref string retstr)
    {
        retstr = "Err";
        IList<AddressAlert> items = new List<AddressAlert>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_Alerts_load(AddressID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string AddressAlertAdd(ref DBUser DBUser, ref AddressAlert wfAlert)
    {
        string retstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_Alerts_Add(ref wfAlert);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }
    public string AddressAlertUpdate(ref DBUser DBUser, ref AddressAlert wfAlert)
    {
        string retstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_Alerts_Update(ref wfAlert);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }
    public string AddressAlertDelete(ref DBUser DBUser, ref AddressAlert wfAlert)
    {
        string retstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_Alerts_Delete(ref wfAlert);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }
    // Address ExtraLines
    public AddressExtraLine[] AddressExtraLinesLoad(ref DBUser DBUser, int AddressID, ref string retstr)
    {
        retstr = "Err";
        IList<AddressExtraLine> items = new List<AddressExtraLine>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_ExtraLines_load(AddressID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string AddressExtraLinesUpdate(ref DBUser DBUser, ref AddressExtraLine wfExtraLine)
    {
        string retstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_ExtraLines_Update(ref wfExtraLine);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }
    public int AddressExtraLineTemplate(ref DBUser DBUser, int AddressID, ref string TemplateName)
    {
        int retstr = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_ExtraLines_Template(AddressID, ref TemplateName);
            }
        }
        catch (NullReferenceException ex)
        {
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }
    public AddressCategory[] AddressCategoriesLoad(ref DBUser DBUser)
    {
        string retstr = "Err";
        IList<AddressCategory> items = new List<AddressCategory>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Address_Categories_load(ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    // Sales
    public string SalesOrderAdd(ref DBUser DBUser, ref OrderSales WfOrder, SalesOrderTypes OrderType)
    {
        string errstr = "Err";
        int lineid = 0;
        int orderClass = 200;
        if (OrderType == SalesOrderTypes.Quotations) orderClass = 100;
        if (OrderType == SalesOrderTypes.Invoice) orderClass = 400;
        if (OrderType == SalesOrderTypes.RecurringOrder) orderClass = 300;
        if (OrderType == SalesOrderTypes.InvoiceClosed) orderClass = 900;
        var defOrder = new OrderSales();
        defOrder.seller = WfOrder.seller;
        defOrder.Calendar = WfOrder.Calendar;
        defOrder.TermsOfPayment = WfOrder.TermsOfPayment;
        defOrder.Language = WfOrder.Language;
        defOrder.Currency = WfOrder.Currency;
        defOrder.salesman = WfOrder.salesman;
        defOrder.PayDate = WfOrder.PayDate;
        defOrder.IntRef = WfOrder.IntRef;
        // var lineitem =  new OrderLine();
        var payment = new OrderPayment();
        int SaleID = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                if (WfOrder.OrderNo >= 0)
                {
                    wfws.web wfweb = new wfws.web(ref DBUser);
                    SaleID = WfOrder.SaleID;
                    DBUser.Message = "hertil 1";
                    if (SaleID == 0) SaleID = wfweb.getOrderIDByNumber(WfOrder.OrderNo, WfOrder.Category, orderClass);
                    if (SaleID < 0) SaleID = 0;
                    if (SaleID == 0) wfweb.Order_add(ref WfOrder, ref SaleID, orderClass);
                    if (SaleID > 0)
                    {
                        if (!string.IsNullOrEmpty(defOrder.Currency)) WfOrder.Currency = defOrder.Currency;
                        wfweb.Order_Update(SaleID, ref WfOrder, orderClass, DBUser.BlindUpdate);
                        errstr = wfweb.order_address_associate(ref WfOrder);
                        errstr = wfweb.order_load(SaleID, ref WfOrder);
                        if (WfOrder.BillTo > 0)
                        {
                            if (defOrder.seller > 0) WfOrder.seller = defOrder.seller;
                            if (defOrder.Calendar > 0) WfOrder.Calendar = defOrder.Calendar;
                            //if (defOrder.Category > 0) WfOrder.Category = defOrder.Category;
                            if (!string.IsNullOrEmpty(defOrder.TermsOfPayment)) WfOrder.TermsOfPayment = defOrder.TermsOfPayment;
                            if (!string.IsNullOrEmpty(defOrder.Language)) WfOrder.Language = defOrder.Language;
                            if (!string.IsNullOrEmpty(defOrder.Currency)) WfOrder.Currency = defOrder.Currency;
                            if (!string.IsNullOrEmpty(defOrder.salesman)) WfOrder.salesman = defOrder.salesman;
                            if (!string.IsNullOrEmpty(defOrder.IntRef)) WfOrder.IntRef = defOrder.IntRef;
                            //if (WfOrder.Dim1 == "zzzz")
                            //{
                            //   WfOrder.PayDate = DateTime.Today;
                            //   WfOrder.Dim1 = WfOrder.InvoiceDate.ToString();
                            WfOrder.PayDate = wfweb.SalesDuedateGet(WfOrder.InvoiceDate, WfOrder.TermsOfPayment);
                            // }
                            //if (defOrder.PayDate != DateTime.MinValue) WfOrder.PayDate = defOrder.PayDate;
                            wfweb.Order_Update(SaleID, ref WfOrder, orderClass, DBUser.BlindUpdate);
                        }
                        errstr = "OK";
                        DBUser.Message = "hertil 2";
                        if (WfOrder.OrderLines != null)
                        {
                            foreach (OrderLine lineItem in WfOrder.OrderLines)
                            {
                                if (lineItem.ItemID != String.Empty)
                                {
                                    errstr = wfweb.Order_add_item(SaleID, lineItem, ref lineid);
                                    lineItem.Liid = lineid;
                                    DBUser.Message = errstr;
                                    wfweb.Order_Item_Update(SaleID, lineItem);
                                    wfweb.Order_Item_UpdatePrice(SaleID, lineItem);  //salesprice = -1 means price from inv.card is accepted otherwise recalculate to price from shop
                                }
                            }
                            wfweb.order_calculate(SaleID);
                        }
                        if (WfOrder.OrderPayments != null)
                        {
                            foreach (OrderPayment Payment in WfOrder.OrderPayments)
                            {
                                errstr = wfweb.Order_add_payment(SaleID, Payment);
                            }
                        }
                    }
                    else
                    {
                        errstr = "err:  // Saleid = 0";
                    }
                }
                else
                {
                    errstr = "err: Order number missing";
                }
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderSave(ref DBUser DBUser, ref OrderSales WfOrder)
    {
        string errstr = "Err";
        var payment = new OrderPayment();
        int SaleID = 0;
        int lineid = 0;
        string ItemID;
        decimal qty;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                SaleID = WfOrder.SaleID;
                if (SaleID > 0)
                {
                    if (wfweb.order_is_Open(WfOrder.SaleID))
                    {
                        wfweb.Order_Update(SaleID, ref WfOrder, 0, DBUser.BlindUpdate);
                        errstr = "OK";
                        if (WfOrder.OrderLines != null)
                        {
                            foreach (OrderLine lineitem in WfOrder.OrderLines)
                            {
                                ItemID = lineitem.ItemID;
                                qty = lineitem.Qty;
                                if (ItemID != String.Empty)
                                {
                                    errstr = wfweb.Order_add_item(SaleID, lineitem, ref lineid);
                                    lineitem.Liid = lineid;
                                    wfweb.Order_Item_Update(SaleID, lineitem);
                                    wfweb.Order_Item_UpdatePrice(WfOrder.SaleID, lineitem);
                                }
                            }
                            wfweb.order_calculate(SaleID);
                        }
                        if (WfOrder.OrderPayments != null)
                        {
                            foreach (OrderPayment Payment in WfOrder.OrderPayments)
                            {
                                errstr = wfweb.Order_add_payment(SaleID, Payment);
                            }
                        }
                    }
                    else
                    {
                        errstr = "Order is closed";
                    }
                }
                else
                {
                    errstr = "err: Saleid = 0";
                }
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderLoad(ref DBUser DBUser, ref OrderSales WfOrder)
    {
        string errstr = "Err";
        IList<AddressProperty> items = new List<AddressProperty>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if ((WfOrder.SaleID == 0) && ((WfOrder.OrderNo > 0) || (WfOrder.InvoiceNo > 0))) WfOrder.SaleID = wfweb.get_saleid_by_ordreno(WfOrder.OrderNo, WfOrder.InvoiceNo, ref errstr);
                errstr = wfweb.order_load(WfOrder.SaleID, ref WfOrder);
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderLookup(ref DBUser DBUser, ref OrderSales wfOrder, SalesOrderTypes OrderType)
    {
        string errstr = "Err";
        int orderClass = 200;
        if (OrderType == SalesOrderTypes.Quotations) orderClass = 100;
        if (OrderType == SalesOrderTypes.Invoice) orderClass = 400;
        if (OrderType == SalesOrderTypes.RecurringOrder) orderClass = 300;
        if (OrderType == SalesOrderTypes.InvoiceClosed) orderClass = 900;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if ((wfOrder.SaleID == 0) && ((wfOrder.OrderNo > 0) || (wfOrder.InvoiceNo > 0))) wfOrder.SaleID = wfweb.get_saleid_Lookup(ref wfOrder, orderClass);
                errstr = wfweb.order_load(wfOrder.SaleID, ref wfOrder);
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderClose(ref DBUser DBUser, ref OrderSales WfOrder)
    {
        string errstr = "OK";
        IList<AddressProperty> items = new List<AddressProperty>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if ((WfOrder.SaleID == 0) && ((WfOrder.OrderNo > 0) || (WfOrder.InvoiceNo > 0))) WfOrder.SaleID = wfweb.get_saleid_by_ordreno(WfOrder.OrderNo, WfOrder.InvoiceNo, ref errstr);
                string clerr = "OK";
                if (wfweb.order_is_Open(WfOrder.SaleID))
                {
                    int errno = wfweb.order_CloseErr(WfOrder.SaleID, ref clerr);
                    if (errno == 0)
                    {
                        wfweb.order_Close(WfOrder.SaleID);
                    }
                    else
                    {
                        errstr = "Not closed";
                        DBUser.Message = string.Concat(errno.ToString(), clerr);
                    }
                }
                else
                {
                    errstr = "Order is closed";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }

    public string SalesOrderDelete(ref DBUser DBUser, ref OrderSales WfOrder)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.order_is_Open(WfOrder.SaleID))
                {
                    wfweb.order_delete(WfOrder.SaleID);
                }
                else
                {
                    errstr = "Order is closed";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }

    public string SalesOrderEmpty(ref DBUser DBUser, ref OrderSales WfOrder)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.order_is_Open(WfOrder.SaleID))
                {
                    wfweb.order_empty(WfOrder.SaleID);
                }
                else
                {
                    errstr = "Order is closed";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }




    public string SalesOrderRecalc(ref DBUser DBUser, ref OrderSales WfOrder)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.order_is_Open(WfOrder.SaleID))
                {
                    wfweb.order_Recalc(WfOrder.SaleID);
                    wfweb.order_calculate(WfOrder.SaleID);
                }
                else
                {
                    errstr = "Order is closed";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }



    public int SalesOrderGetSaleIDFromGuid(ref DBUser DBUser, Guid GuidInvoice)
    {
        int SaleID = 0;
        String errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                SaleID = wfweb.get_saleid_from_guid(GuidInvoice);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return SaleID;
    }
    public OrderLine[] SalesOrderLoadItems(ref DBUser DBUser, ref OrderSales WfOrder, ref string errstr)
    {
        IList<OrderLine> items = new List<OrderLine>();
        errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.order_load_Items(WfOrder.SaleID, ref items);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string SalesOrderAddItem(ref DBUser DBUser, ref OrderSales WfOrder, OrderLine LineItem)
    {
        string errstr = "OK";
        int lineid = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.order_is_Open(WfOrder.SaleID))
                {
                    errstr = wfweb.Order_add_item(WfOrder.SaleID, LineItem, ref lineid);
                    LineItem.Liid = lineid;
                    wfweb.Order_Item_Update(WfOrder.SaleID, LineItem);
                    if (LineItem.SalesPrice >= 0) wfweb.Order_Item_UpdatePrice(WfOrder.SaleID, LineItem);   //salesprice = -1 means price from inv.card is accepted otherwise recalculate to price from shop
                    wfweb.order_calculate(WfOrder.SaleID);
                }
                else
                {
                    errstr = "Order is closed";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderAddPayment(ref DBUser DBUser, ref OrderSales WfOrder, OrderPayment PaymentItem)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                //if (wfweb.order_is_Open(WfOrder.SaleID))
                //{
                errstr = wfweb.Order_add_payment(WfOrder.SaleID, PaymentItem);
                //}
                //else
                //{
                //    errstr = "Order is closed";
                //}
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public OrderPayment[] SalesOrderLoadPayments(ref DBUser DBUser, ref OrderSales WfOrder, ref string errstr)
    {
        errstr = "OK";
        IList<OrderPayment> items = new List<OrderPayment>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.order_load_Payments(WfOrder.SaleID, ref items);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string SalesOrderLoadItem(ref DBUser DBUser, ref OrderLine LineItem)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.order_load_Item(LineItem.SaleID, LineItem.Liid, ref LineItem);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderChangeQtyPriceOnItem(ref DBUser DBUser, ref OrderLine LineItem)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.order_is_Open(LineItem.SaleID))
                {
                    wfweb.Order_Item_UpdatePrice(LineItem.SaleID, LineItem);
                    wfweb.order_calculate(LineItem.SaleID);
                }
                else
                {
                    errstr = "Order is closed ";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderDeleteOrderLine(ref DBUser DBUser, ref OrderLine LineItem)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.order_is_Open(LineItem.SaleID))
                {
                    wfweb.Order_Line_Delete(LineItem.SaleID, LineItem);
                    wfweb.order_calculate(LineItem.SaleID);
                }
                else
                {
                    errstr = "Order is closed ";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }

    public string SalesOrderDeleteItem(ref DBUser DBUser, ref OrderSales WfOrder, string ItemID)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.order_is_Open(WfOrder.SaleID))
                {
                    wfweb.order_LineItemDelete(WfOrder.SaleID, ItemID);
                    wfweb.order_calculate(WfOrder.SaleID);
                }
                else
                {
                    errstr = "Order is closed";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderAddressAssociate(ref DBUser DBUser, ref OrderSales WfOrder, int BillTo, int ShipTo)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.order_is_Open(WfOrder.SaleID))
                {
                    WfOrder.ShipTo = ShipTo;
                    WfOrder.BillTo = BillTo;
                    errstr = wfweb.order_address_associate(ref WfOrder);
                }
                else
                {
                    errstr = "Order is closed ";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderCalendarBlock(ref DBUser DBUser, ref OrderSales WfOrder, DateTime BlockDate, string UserID)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.order_calendar_block(WfOrder.SaleID, WfOrder.Calendar, BlockDate, UserID);
                errstr = String.Concat(WfOrder.SaleID, "  ", WfOrder.Calendar, "  ", BlockDate);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderCalendarBlock_upto(ref DBUser DBUser, ref OrderSales WfOrder, DateTime BlockDate, string UserID)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.order_calendar_block_upto(WfOrder.SaleID, WfOrder.Calendar, BlockDate, UserID);
                errstr = String.Concat(WfOrder.SaleID, "  ", WfOrder.Calendar, "  ", BlockDate);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string SalesOrderCalendarUnBlock(ref DBUser DBUser, ref OrderSales WfOrder, DateTime BlockDate)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.order_calendar_unblock(WfOrder.SaleID, WfOrder.Calendar, BlockDate);
                errstr = String.Concat(errstr, "  ", WfOrder.SaleID, "  ", WfOrder.Calendar, "  ", BlockDate);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public OrderSalesItem[] SalesOrderListLoad(ref DBUser DBUser, ref OrderSales WfOrder, ref string errstr, SalesOrderTypes OrderType)
    {
        IList<OrderSalesItem> items = new List<OrderSalesItem>();
        int orderClass = 200;
        try
        {
            if (OrderType == SalesOrderTypes.Quotations) orderClass = 100;
            if (OrderType == SalesOrderTypes.Invoice) orderClass = 400;
            if (OrderType == SalesOrderTypes.RecurringOrder) orderClass = 300;
            if (OrderType == SalesOrderTypes.InvoiceClosed) orderClass = 900;
            if (OrderType == SalesOrderTypes.InvoiceAndOrders) orderClass = 0;
            if (OrderType == SalesOrderTypes.CreditNote) orderClass = -400;
            if (OrderType == SalesOrderTypes.CreditNoteClosed) orderClass = -900;
            if (OrderType == SalesOrderTypes.InvoiceCreditNoteClosed) orderClass = 1;
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (OrderType == SalesOrderTypes.UnpaidInvoice || OrderType == SalesOrderTypes.UnpaidCreditNote)
                    errstr = wfweb.SalesOrder_Items_get_unpaid(ref WfOrder, ref items, OrderType);
                else
                    errstr = wfweb.SalesOrder_Items_get(ref WfOrder, ref items, orderClass);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public SalesStatLine[] SalesStatsLoadItems(ref DBUser DBUser, ref OrderSales WfOrder, ref string errstr, SalesOrderTypes OrderType)
    {
        IList<SalesStatLine> items = new List<SalesStatLine>();
        int orderClass = 200;
        try
        {
            if (OrderType == SalesOrderTypes.Quotations) orderClass = 100;
            if (OrderType == SalesOrderTypes.Invoice) orderClass = 400;
            if (OrderType == SalesOrderTypes.RecurringOrder) orderClass = 300;
            if (OrderType == SalesOrderTypes.InvoiceClosed) orderClass = 900;
            if (OrderType == SalesOrderTypes.InvoiceAndOrders) orderClass = 0;
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                //errstr = wfweb.SalesOrder_LineItems_get(ref  OrderSales wfOrderSales, ref IList<OrderLine> items, int orderClass)
                errstr = wfweb.SalesStatsItems_get(ref WfOrder, ref items, orderClass);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }


    public SalesStatSum[] AddressSalesStats(ref DBUser DBUser, ref SalesStatFilter StatFilter, ref string errstr, SalesOrderTypes OrderType)
    {
        IList<SalesStatSum> items = new List<SalesStatSum>();
        int orderClass = 200;
        try
        {
            if (OrderType == SalesOrderTypes.Quotations) orderClass = 100;
            if (OrderType == SalesOrderTypes.Invoice) orderClass = 400;
            if (OrderType == SalesOrderTypes.RecurringOrder) orderClass = 300;
            if (OrderType == SalesOrderTypes.InvoiceClosed) orderClass = 900;
            if (OrderType == SalesOrderTypes.InvoiceAndOrders) orderClass = 0;
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                //errstr = wfweb.SalesOrder_LineItems_get(ref  OrderSales wfOrderSales, ref IList<OrderLine> items, int orderClass)
                errstr = wfweb.SalesAddressStatsItems_get(ref StatFilter, ref items, orderClass);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();

    }




    public int SalesGetCategoryByName(ref DBUser DBUser, string category)
    {
        int CategoryID = 0;
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                CategoryID = wfweb.SalesGetCategoryByName(category);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return CategoryID;
    }
    public int SalesGetSellerByName(ref DBUser DBUser, string seller)
    {
        int SellerID = 0;
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                SellerID = wfweb.SalesGetSellerByName(seller);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return SellerID;
    }
    public SalesCategorie[] SalesCategoriesLoad(ref DBUser DBUser)
    {
        IList<SalesCategorie> items = new List<SalesCategorie>();
        string errstr = "OK";
        DateTime newdate = DateTime.Now;
        DBUser.Message = "OK_0";
        try
        {
            DBUser.Message = "OK_1";
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                DBUser.Message = "OK_3";
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Sales_Categories_load(ref items);
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public OrderSalesItemChanged[] SalesOrderChangedItems(ref DBUser DBUser, OrderSalesItemChanged wfOrder, ref DateTime SalesTimestamp, SalesOrderTypes OrderType, Boolean expanded)
    {
        IList<OrderSalesItemChanged> items = new List<OrderSalesItemChanged>();
        int orderClass = 200;
        string errstr = "OK";
        DateTime newdate = DateTime.Now;
        DBUser.Message = "OK_0";
        try
        {
            DBUser.Message = "OK_1";
            if (OrderType == SalesOrderTypes.Quotations) orderClass = 100;
            if (OrderType == SalesOrderTypes.Invoice) orderClass = 400;
            if (OrderType == SalesOrderTypes.RecurringOrder) orderClass = 300;
            if (OrderType == SalesOrderTypes.InvoiceClosed) orderClass = 900;
            DBUser.Message = "OK_2";
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                DBUser.Message = "OK_3";
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.SalesOrder_Items_get_changed(ref SalesTimestamp, wfOrder, ref items, orderClass, expanded);
                DBUser.Message = errstr;
                SalesTimestamp = newdate;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public void SalesOrder_MarkPaidSales(ref DBUser DBUser)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                DBUser.Message = "OK_3";
                wfws.web wfweb = new wfws.web(ref DBUser);
                wfweb.SalesOrder_MarkPaid();
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
    }
    public OrderSalesItemChanged[] SalesOrderFactoringItems(ref DBUser DBUser)
    {
        IList<OrderSalesItemChanged> items = new List<OrderSalesItemChanged>();
        string errstr = "OK";
        DateTime newdate = DateTime.Now;
        DBUser.Message = "OK_0";
        try
        {
            DBUser.Message = "OK_1";
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                DBUser.Message = "OK_3";
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.SalesOrder_Items_get_Factoring(ref items);
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public int SalesOrderFactoringItemConfirm(ref DBUser DBUser, int ListID)
    {
        string errstr = "OK";
        DateTime newdate = DateTime.Now;
        DBUser.Message = "OK_0";
        try
        {
            DBUser.Message = "OK_1";
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                DBUser.Message = "OK_3";
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.SalesOrder_Factoring_confirm(ListID);
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return 0;
    }
    public OrderSalesItemChanged[] SalesOrderB2BBackboneItems(ref DBUser DBUser)
    {
        IList<OrderSalesItemChanged> items = new List<OrderSalesItemChanged>();
        string errstr = "OK";
        DateTime newdate = DateTime.Now;
        DBUser.Message = "OK_0";
        try
        {
            DBUser.Message = "OK_1";
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                DBUser.Message = "OK_3";
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.SalesOrder_Items_get_B2BBackbone(ref items);
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        //OrderSalesItemChanged[] retval = items.ToArray();
        //return retval;
        return items.ToArray();
    }
    public int SalesOrderB2BBackboneItemConfirm(ref DBUser DBUser, int SaleID)
    {
        string errstr = "OK";
        DateTime newdate = DateTime.Now;
        DBUser.Message = "OK_0";
        try
        {
            DBUser.Message = "OK_1";
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                DBUser.Message = "OK_3";
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.SalesOrder_B2BBackbone_confirm(SaleID);
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return 0;
    }
    public string SalesOrderToUBL(ref CompanyInf wfCompany, ref CompanySeller wfseller, ref OrderSales wfOrder, ref Address BillToAddress, ref Address ShipToAddress, SalesOrderTypes OrderType)
    {
        var Doc = new XmlDocument();
        string errstr = string.Empty;
        UBLOrder myUBL = new UBLOrder();
        try
        {
            errstr = myUBL.Create_ubl_invoice_from_order(ref wfCompany, ref wfseller, ref wfOrder, ref BillToAddress, ref ShipToAddress);
        }
        catch (Exception e)
        {
            //errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return myUBL.Doc.OuterXml;
    }
    public int SalesOrderToInvoice(ref DBUser DBUser, ref OrderSales WfOrder)
    {
        string errstr = string.Empty;
        int NewSaleID = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                NewSaleID = wfweb.SalesOrder_To_Invoice(WfOrder.SaleID, WfOrder.InvoiceDate);
            }
            return NewSaleID;
        }
        catch (Exception e)
        {
            //errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
    }
    public string SalesOrderIDToUBL(ref DBUser DBUser, ref SalesOrderUBL xmlUBL)
    {
        CompanyInf companyinf = new CompanyInf();
        var Doc = new XmlDocument();
        int scount = 0;
        string errstr = "OK";
        string connstr;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                var wfCompany = new wfws.Company(ref DBUser);
                errstr = wfCompany.Company_Load(ref DBUser, ref companyinf);
                var wfOrder = new OrderSales();
                wfOrder.SaleID = xmlUBL.SaleID;
                errstr = wfweb.order_load(wfOrder.SaleID, ref wfOrder);
                var wfseller = new CompanySeller();
                wfseller.SellerID = wfOrder.seller;
                int err = wfCompany.Seller_Lookup(ref wfseller, ref scount);
                var BillToAddress = new Address();
                var ShipToAddress = new Address();
                BillToAddress.AddressID = wfOrder.BillTo;
                ShipToAddress.AddressID = wfOrder.ShipTo;
                errstr = wfweb.Address_Get(ref BillToAddress);
                errstr = wfweb.Address_Get(ref ShipToAddress);
                UBLOrder myUBL = new UBLOrder();
                errstr = myUBL.Create_ubl_invoice_from_order(ref companyinf, ref wfseller, ref wfOrder, ref BillToAddress, ref ShipToAddress);
                xmlUBL.XmlString = myUBL.Doc.OuterXml;
                //int sellerid = wfcompany.Seller_Lookup(ref wfSeller,ref seCount);
                //if (wfSeller.SellerID > 0) { errstr = wfcompany.Seller_Get(ref wfSeller);}
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public SalesDeliveriesProducts[] SalesDeliverySchedule(ref DBUser DBUser, int BillTo)
    {
        string errstr = "OK";
        string connstr;
        DBUser.Message = "start";
        IList<SalesDeliveriesProducts> items = new List<SalesDeliveriesProducts>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfdel = new Delivery(ref DBUser, connstr);
                errstr = wfdel.ProductListLoad(BillTo, ref items);
                DBUser.Message = errstr;
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public SalesDeliveriesProducts[] SalesDeliverySchedule_2(ref DBUser DBUser, int BillTo)
    {
        string errstr = "OK";
        string connstr;
        DBUser.Message = "start";
        IList<SalesDeliveriesProducts> items = new List<SalesDeliveriesProducts>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfdel = new Delivery(ref DBUser, connstr);
                errstr = wfdel.ProductListLoad_2(BillTo, ref items);
                DBUser.Message = errstr;
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }



    public byte[] SalesOrderGetPDFArray(ref DBUser DBUser, int SaleID, SalesReports ReportID)
    {
        Byte[] pdfdoc = null;
        string errstr = "OK";
        DBUser.Message = "start";
        string connstr = string.Empty;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                DBUser.Message = "herfra";
                errstr = wfweb.order_IsValidSaleID(ref SaleID);
                if (SaleID > 0)
                {
                    DBUser.ConnectionString = connstr;
                    var MyRepSvc = new wf_rep.WFReportsClient("BasicHttpsBinding_IWFReports");
                    pdfdoc = MyRepSvc.SaOrder(DBUser.DBKey, DBUser.CompID, SaleID, (int)ReportID);
                  
                    DBUser.Message = errstr;
                }
                else
                {
                    DBUser.Message = "Invalid saleID ";
                }
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return pdfdoc;
    }
    public string SalesOrderPDFmail(ref DBUser DBUser, int SaleID, SalesReports ReportID)
    {
        string errstr = "OK";
        string connstr;
        DBUser.Message = "start";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                wfws.wf_mail wfmail = new wfws.wf_mail(ref DBUser);
                DBUser.Message = "herfra";
                errstr = wfweb.order_IsValidSaleID(ref SaleID);
                if (SaleID > 0)
                {
                    var mysale = new wfws.pdfSale(ref DBUser);  //Dette objekt anvendes kun til at finde info omkring mailing
                    var MyRepSvc = new wf_rep.WFReportsClient("BasicHttpsBinding_IWFReports");  //Dette objekt anvendes kun til at danne faktura
                    var mycomp = new wfws.Company(ref DBUser);
                    var companyinf = new CompanyInf();
                    mycomp.Company_Load(ref DBUser, ref companyinf);
                    var myorder = new OrderSales();
                    wfweb.order_load(SaleID, ref myorder);
                    if (myorder.seller > 0)
                    {
                        DBUser.Message = SaleID.ToString();
                        //errstr = mysale.GetPDFArray(SaleID, ReportID, ref companyinf, ref myorder, ref errstr); Gammel fakturarapport
                        wfmail.get_Client(companyinf.saMailClient);
                        wfmail.mailTo = mysale.get_email(ref myorder);
                        wfmail.mailSubject = mysale.ReportTextGet("tr_Sale_MailSubject", myorder.seller, myorder.Language);
                        wfmail.mailbody = mysale.ReportTextGet("tr_Sale_Mailbody", myorder.seller, myorder.Language);
                        if (!string.IsNullOrEmpty(wfmail.mailTo))
                        {
                            DBUser.Message = string.Concat(wfmail.mailSubject, " ----  ", wfmail.mailbody);
                            wfmail.TheData = MyRepSvc.SaOrder(DBUser.DBKey,DBUser.CompID, SaleID, (int)ReportID); //Ny fakturarapport
                            errstr = wfmail.SendMail();
                            DBUser.Message = string.Concat(companyinf.saMailClient, errstr, "  ", wfmail.MailFrom, " ", wfmail.mailTo, " ", wfmail.HostName);
                        }
                        else
                        {
                            DBUser.Message = "no mail address ";
                            throw new FaultException(String.Concat("wf_wcf: ", "no mail address "), new FaultCode("wfwcfFault"));
                        }
                        DBUser.Message = string.Concat(companyinf.saMailClient, errstr, "  ", wfmail.MailFrom, " ", wfmail.mailTo, " ", wfmail.HostName);
                    }
                    else
                    {
                        DBUser.Message = "no SellerID ";
                        throw new FaultException(String.Concat("wf_wcf: ", "no SellerID "), new FaultCode("wfwcfFault"));
                    }
                }
                else
                {
                    DBUser.Message = "Invalid saleID ";
                    throw new FaultException(String.Concat("wf_wcf: ", "Invalid saleID "), new FaultCode("wfwcfFault"));
                }
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    // purchase 
    public string PurchaseOrderLoad(ref DBUser DBUser, ref OrderPurchase wfOrderPurc)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            //errstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if ((wfOrderPurc.PurcID == 0) && (wfOrderPurc.OrderNo > 0)) wfOrderPurc.PurcID = wfweb.get_Purcid_by_ordreno(wfOrderPurc.OrderNo, ref errstr);
                errstr = wfweb.orderpu_load(wfOrderPurc.PurcID, ref wfOrderPurc);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public OrderLinePurc[] PurcOrderLoadItems(ref DBUser DBUser, ref OrderPurchase wfOrderPurc, ref string errstr)
    {
        IList<OrderLinePurc> items = new List<OrderLinePurc>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.orderpu_load_Items(wfOrderPurc.PurcID, ref items);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string PurcOrderAdd(ref DBUser DBUser, ref OrderPurchase wfOrderPurc)   //skal oprette hele fakturaen!!!
    {
        string errstr = "Err";
        try
        {
            int O_PurcID = 0;
            int s_Class = 0;
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Orderpu_add(ref wfOrderPurc, ref O_PurcID, s_Class);
            }
        }
        //Lav noget med
        // string Orderpu_add_item(int PurcID, OrderLinePurc lineItem, ref int O_LiID);
        // string Orderpu_Item_Update(int SaleID, OrderLinePurc lineItem);
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public OrderPurchaseItem[] PurchaseOrderListLoad(ref DBUser DBUser, ref OrderPurchase wfOrderPurc, ref string retStr, PurchaseOrderTypes OrderType)
    {
        string errstr = "Err";
        IList<OrderPurchaseItem> items = new List<OrderPurchaseItem>();
        int orderClass = 200;
        try
        {
            if (OrderType == PurchaseOrderTypes.Inquery) orderClass = 500;
            if (OrderType == PurchaseOrderTypes.Order) orderClass = 1000;
            if (OrderType == PurchaseOrderTypes.Invoice) orderClass = 2000;
            if (OrderType == PurchaseOrderTypes.InvoiceClosed) orderClass = 9000;
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.PurchaseOrder_Items_get(ref wfOrderPurc, ref items, orderClass);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string PurcOrderAddPayment(ref DBUser DBUser, ref OrderPurchase WfOrderPurc, OrderPurcPayment PaymentItem)
    {
        string errstr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if (wfweb.orderPurc_is_Open(WfOrderPurc.PurcID))
                {
                    errstr = wfweb.OrderPurc_add_payment(WfOrderPurc.PurcID, PaymentItem);
                }
                else
                {
                    errstr = "Order is closed";
                }
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public OrderPurcPayment[] PurcOrderLoadPayments(ref DBUser DBUser, ref OrderPurchase WfOrderPurc, ref string errstr)
    {
        IList<OrderPurcPayment> items = new List<OrderPurcPayment>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.orderPurc_load_Payments(WfOrderPurc.PurcID, ref items);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string PurcOrderLookup(ref DBUser DBUser, ref OrderPurchase WfOrderPurc, ref int puCount)
    {
        string errstr = "Err";
        puCount = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                int PurcID = wfweb.Purchase_Lookup(ref WfOrderPurc, ref puCount);
                WfOrderPurc.PurcID = PurcID;
                if (WfOrderPurc.PurcID > 0)
                {
                    wfweb.orderpu_load(WfOrderPurc.PurcID, ref WfOrderPurc);
                    DBUser.Message = errstr;
                    errstr = "OK";
                }
                else
                {
                    errstr = String.Concat("No purchase: ", PurcID);
                }
            }
            else
            {
                errstr = "No company";
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    // inventory
    public string[] InventoryItemsChanged(ref DBUser DBUser, ref string ItemID, ref DateTime TimeChanged)
    {
        string errstr = "Err";
        IList<string> items = new List<string>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Inventory_Items_get_Changed(ref ItemID, ref items, ref TimeChanged);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public inventoryItem[] InventorylistLoad(ref DBUser DBUser)
    {
        string errstr = "Err";
        IList<inventoryItem> items = new List<inventoryItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Inventory_Items_get(ref items);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public inventoryItem[] InventorylistLoadGroupsFi(ref DBUser DBUser, string groupFI)
    {
        string errstr = "Err";
        IList<inventoryItem> items = new List<inventoryItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Inventory_Items_get_List_GroupFI(ref items, groupFI);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public inventoryGroupFi[] InventoryLoadGroupFI(ref DBUser DBUser)
    {
        string errstr = "Err";
        IList<inventoryGroupFi> items = new List<inventoryGroupFi>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Inventory_GroupsFi_get(ref items);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public inventoryLocation[] InventoryLoadLocations(ref DBUser DBUser)
    {
        string errstr = "Err";
        IList<inventoryLocation> items = new List<inventoryLocation>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Inventory_Locations_get(ref items);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public inventorySelection[] InventoryLoadSelections(ref DBUser DBUser)
    {
        string errstr = "Err";
        IList<inventorySelection> items = new List<inventorySelection>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                errstr = wfweb.Inventory_Selections_get(ref items);
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public inventoryItem[] InventorylistLoadSelection(ref DBUser DBUser, string SelectionID, ref string retStr)
    {
        IList<inventoryItem> items = new List<inventoryItem>();
        //        IList<inventorySalesPrice> pitems = new List<inventorySalesPrice>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Inventory_Items_get_selection(ref items, SelectionID);
                //foreach (inventoryItem Item in items)
                //{
                //    pitems.Clear();
                //    retStr = wfweb.inventory_load_sales_prices(Item.ItemID, ref pitems);
                //    Item.inventorySalesPrices = pitems.ToArray();
                //}
                DBUser.Message = retStr;
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string inventoryGetItemOne(ref DBUser DBUser, ref inventoryItem Item)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                IList<inventorySalesPrice> items = new List<inventorySalesPrice>();
                IList<inventoryQty> itemsQty = new List<inventoryQty>();
                IList<string> itemsSel = new List<string>();
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Inventory_Item_get(ref Item);
                retStr = wfweb.inventory_load_sales_prices(Item.ItemID, ref items);
                retStr = wfweb.inventory_load_qty(Item.ItemID, ref itemsQty);
                retStr = wfweb.inventory_load_Selections(Item.ItemID, ref itemsSel);
                Item.inventorySalesPrices = items.ToArray();
                Item.inventoryQty = itemsQty.ToArray();
                Item.Selections = itemsSel.ToArray();
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string inventoryGetItemOneCardOnly(ref DBUser DBUser, ref inventoryItem Item)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Inventory_Item_get(ref Item);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }

    public string inventoryAddItemOne(ref DBUser DBUser, ref inventoryItem Item)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Inventory_Item_add(ref Item);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }

    public string inventorySaveItemOne(ref DBUser DBUser, ref inventoryItem Item)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Inventory_Item_save(ref Item);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }


    public inventoryPicture inventoryGetItemPicture(ref DBUser DBUser, string ItemID, int PicID, bool Thumbnail)
    {
        inventoryPicture RetVal = new inventoryPicture();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                RetVal = wfweb.Inventory_ItemPicture_get(ItemID, PicID, Thumbnail);
            }
        }
        catch (Exception e)
        {
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return RetVal;
    }
    public string InventoryItemLookup(ref DBUser DBUser, ref inventoryItem Item, ref int ItemCount)
    {
        string retStr = "err";
        string ItemID;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                ItemID = wfweb.Inventory_Item_Lookup(ref Item, ref ItemCount);
                if (ItemCount > 0)
                {
                    wfweb.Inventory_Item_get(ref Item);
                    IList<inventorySalesPrice> items = new List<inventorySalesPrice>();
                    IList<inventoryQty> itemsQty = new List<inventoryQty>();
                    IList<string> itemsSel = new List<string>();
                    retStr = wfweb.inventory_load_sales_prices(Item.ItemID, ref items);
                    retStr = wfweb.inventory_load_qty(Item.ItemID, ref itemsQty);
                    retStr = wfweb.inventory_load_Selections(Item.ItemID, ref itemsSel);
                    Item.inventorySalesPrices = items.ToArray();
                    Item.inventoryQty = itemsQty.ToArray();
                    Item.Selections = itemsSel.ToArray();
                }
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public int inventoryAddPicture(ref DBUser DBUser, string ItemID, byte[] Picture, string Description)
    {
        int RetVal = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                RetVal = wfweb.AddPicture(ItemID, Picture, Description);
            }
        }
        catch (Exception e)
        {
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return RetVal;
    }
    public string inventoryGetItemExtra(ref DBUser DBUser, ref inventoryItemExtra Item)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Inventory_Item_get_extra_text(ref Item, "1");
                retStr = wfweb.Inventory_Item_get_extra_text(ref Item, "2");
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string inventoryGetVat(ref DBUser DBUser, ref InventoryVAT VAT)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Inventory_GetVat(ref VAT);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }

    public InventoryExtraLine[] InventoryExtraLinesLoad(ref DBUser DBUser, string ItemID, ref string retstr)
    {
        retstr = "Err";
        IList<InventoryExtraLine> items = new List<InventoryExtraLine>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Inventory_ExtraLines_load(ItemID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public InventoryStyle[] InventoryStyleLoad(ref DBUser DBUser, string ItemID, ref string retstr)
    {
        retstr = "Err";
        IList<InventoryStyle> items = new List<InventoryStyle>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Inventory_Styles_load(ItemID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }

    public inventoryContainer[] inventoryContainersLoad(ref DBUser DBUser, string ItemID, ref string retstr)
    {
        retstr = "Err";
        IList<inventoryContainer> items = new List<inventoryContainer>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Inventory_Containers_load(ItemID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }

    public string InventoryContainerAdd(ref DBUser DBUser, ref inventoryContainer Item)
    {
        string retstr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Inventory_Container_Add(ref Item);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }


    public string InventoryContainerClear(ref DBUser DBUser, ref inventoryContainer Item)
    {
        string retstr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Inventory_Container_clear(ref Item);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }



    public inventoryVendors[] inventoryVendorsLoad(ref DBUser DBUser, string ItemID, ref string retstr)
    {
        retstr = "Err";
        IList<inventoryVendors> items = new List<inventoryVendors>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Inventory_Vendors_load(ItemID, ref items);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }

    public string InventoryVendorAdd(ref DBUser DBUser, ref inventoryVendor Item)
    {
        string retstr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Inventory_Vendor_Add(ref Item);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }


    public string InventoryVendorClear(ref DBUser DBUser, string ItemID)
    {
        string retstr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retstr = wfweb.Inventory_Vendor_clear(ItemID);
            }
        }
        catch (NullReferenceException ex)
        {
            retstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return retstr;
    }





    public MenuItem[] MenuLoadItems(ref DBUser DBUser, string selection)
    {
        string retStr = "err";
        IList<MenuItem> items = new List<MenuItem>();
        try
        {
            //var wfconn = new wfws.ConnectLocal(DBUser);
            //retStr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Menu_Items_get(ref items, selection);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public MenuItem[] MenuLoadItemsTranslated(ref DBUser DBUser, String Language, string selection)
    {
        string retStr = "err";
        IList<MenuItem> items = new List<MenuItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.Menu_Items_get_translated(ref items, Language, selection);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public byte[] MenuLoadItemPicture(ref DBUser DBUser, string SelectionID, string Description, bool Thumbnail, ref string PictType)
    {
        byte[] answer = null;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                answer = wfweb.Menu_Item_get_picture(SelectionID, Description, Thumbnail, ref PictType);
            }
        }
        catch (Exception e)
        {
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return answer;
    }
    public MenuItem[] MenuShopLoadItems(ref DBUser DBUser, string selection)
    {
        string retStr = "err";
        IList<MenuItem> items = new List<MenuItem>();
        try
        {
            //var wfconn = new wfws.ConnectLocal(DBUser);
            //retStr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            //if (DBUser.CompID > 0)
            //{
            wfws.web wfweb = new wfws.web(ref DBUser);
            retStr = wfweb.Menu_ShopItems_get(ref items, selection);
            //}
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string[] inventoryGetSelectionText(ref DBUser DBUser, string Selection, string countryID, ref string retStr)
    {
        // Dim items As New List(Of MenuItem)
        // Dim lineitem As New OrderLine
        var SelText = new string[2];
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                retStr = wfweb.get_selection_text(Selection, countryID, ref SelText);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return SelText;
    }
    // production
    public string ProdAssAdd(ref DBUser DBUser, ref ProdAssembly wf_Ass)
    {
        string errstr = "Err";
        int lineid = 0;
        int ProdID = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                ProdID = wf_Ass.ProdID;
                if (ProdID < 0) ProdID = 0;
                if (ProdID == 0) wfweb.Prod_add(ref wf_Ass, ref ProdID);
                if (ProdID > 0)
                {
                    wfweb.prod_update(ProdID, ref wf_Ass);
                    wfweb.Prod_Items_Delete(ProdID);
                    errstr = "OK";
                    if (wf_Ass.ProdLines != null)
                    {
                        foreach (ProdAssLine lineItem in wf_Ass.ProdLines)
                        {
                            if (lineItem.ItemID != String.Empty)
                            {
                                errstr = wfweb.Prod_add_item(ProdID, lineItem, ref lineid);
                                lineItem.Liid = lineid;
                                DBUser.Message = errstr;
                                wfweb.prod_item_update(ProdID, lineItem);
                            }
                        }
                    }
                    wfweb.Prod_operations_Delete(ProdID);
                    if (wf_Ass.ProdOpeLines != null)
                    {
                        foreach (ProdOpeLine opeitem in wf_Ass.ProdOpeLines)
                        {
                            errstr = wfweb.prod_add_operation(ProdID, opeitem, ref lineid);
                        }
                    }
                    wfweb.prod_calculate(ProdID);
                }
                else
                {
                    errstr = "err:  // ProdID = 0";
                }
            }
            else
            {
                errstr = "err: Order number missing";
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string ProdAssLoad(ref DBUser DBUser, ref ProdAssembly wf_Ass)
    {
        string errstr = "Err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                if ((wf_Ass.ProdID == 0) && ((wf_Ass.AssemblyNo > 0))) wf_Ass.ProdID = wfweb.get_ProdID_by_AssemblyNo(wf_Ass.AssemblyNo, ref errstr);
                errstr = wfweb.prod_load(wf_Ass.ProdID, ref wf_Ass);
                DBUser.Message = errstr;
            }
        }
        catch (Exception e)
        {
            errstr = e.Message;
            throw new FaultException(String.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    public string ProdAssSave(ref DBUser DBUser, ref ProdAssembly wf_Ass)
    {
        string errstr = "Err";
        var payment = new OrderPayment();
        int ProdID = 0;
        int lineid = 0;
        //string ItemID;
        //decimal qty;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.web wfweb = new wfws.web(ref DBUser);
                ProdID = wf_Ass.ProdID;
                if (ProdID > 0)
                {
                    if (wfweb.Prod_is_Open(wf_Ass.ProdID))
                    {
                        wfweb.prod_update(ProdID, ref wf_Ass);
                        errstr = "OK";
                        if (wf_Ass.ProdLines != null)
                        {
                            foreach (ProdAssLine lineItem in wf_Ass.ProdLines)
                            {
                                if (lineItem.ItemID != String.Empty)
                                {
                                    errstr = wfweb.Prod_add_item(ProdID, lineItem, ref lineid);
                                    lineItem.Liid = lineid;
                                    DBUser.Message = errstr;
                                    wfweb.prod_item_update(ProdID, lineItem);
                                }
                            }
                        }
                        if (wf_Ass.ProdOpeLines != null)
                        {
                            foreach (ProdOpeLine opeitem in wf_Ass.ProdOpeLines)
                            {
                                errstr = wfweb.prod_add_operation(ProdID, opeitem, ref lineid);
                            }
                        }
                        wfweb.prod_calculate(ProdID);
                    }
                    else
                    {
                        errstr = "Order is closed";
                    }
                }
                else
                {
                    errstr = "err: Saleid = 0";
                }
            }
        }
        catch (NullReferenceException ex)
        {
            errstr = ex.Message;
            throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
        }
        return errstr;
    }
    // shop
    public string shopLoad(ref DBUser DBUser, ref ShopWf myShop)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfshop = new wfws.shop(ref DBUser);
                retStr = wfshop.load(ref myShop);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string shopGetText(ref DBUser DBUser, string TextKey, string Country, ref string header, ref string body)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfshop = new wfws.shop(ref DBUser);
                retStr = wfshop.get_text(TextKey, Country, ref header, ref body);
            }
            if (string.IsNullOrEmpty(body)) body = String.Concat(TextKey, " - ", DBUser.ShopID, " -  ", DBUser.CompID);
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public inventoryItem[] shopProductsLoadSelection(ref DBUser DBUser, string selection, ref string retStr)
    {
        IList<inventoryItem> items = new List<inventoryItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfshop = new wfws.shop(ref DBUser);
                retStr = wfshop.Shop_Items_get_selection(ref items, selection);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    //  basket
    public string BasketLoad(ref DBUser DBUser, ref ShopBasket myBasket)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var mybasket = new wfws.Basket(ref DBUser);
                retStr = mybasket.load(ref myBasket);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string BasketSave(ref DBUser DBUser, ref ShopBasket myBasket)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfbasket = new wfws.Basket(ref DBUser);
                retStr = wfbasket.save(ref myBasket, ref retStr);
                DBUser.Message = retStr;
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string BasketIDByAddressID(ref DBUser DBUser, int addressID)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var myweb = new wfws.web(ref DBUser);
                DBUser.BasketGuid = myweb.Address_get_basket_Guid(addressID, ref retStr);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string BasketAddAddresses(ref DBUser DBUser, ref ShopBasket myBasket)
    {
        string retStr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfbasket = new wfws.Basket(ref DBUser);
                retStr = wfbasket.save(ref myBasket, ref retStr);
                retStr = wfbasket.AddressesToWf();
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string BasketConfirm(ref DBUser DBUser, ref ShopBasket myBasket)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfbasket = new wfws.Basket(ref DBUser);
                retStr = wfbasket.Confirm();
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public ShopBasketItem[] BasketLoadItems(ref DBUser DBUser, ref ShopBasket myBasket, ref string retStr)
    {
        retStr = "OK";
        IList<ShopBasketItem> items = new List<ShopBasketItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfbasket = new wfws.Basket(ref DBUser);
                retStr = wfbasket.basket_Items_delete_lines(ref myBasket);
                wfbasket.load_Items(ref items, ref retStr);
                retStr = wfbasket.basket_total(ref myBasket);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return items.ToArray();
    }
    public string BasketAddItem(ref DBUser DBUser, ref ShopBasket myBasket, ref ShopBasketItem myBasketItem)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfbasket = new wfws.Basket(ref DBUser);
                wfbasket.AddBasketItem(ref myBasketItem);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string BasketUpdItem(ref DBUser DBUser, ref ShopBasket myBasket, ref ShopBasketItem myBasketItem)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfbasket = new wfws.Basket(ref DBUser);
                retStr = wfbasket.basket_item_update(ref myBasketItem);
                DBUser.Message = retStr;
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    // Accounts
    public AccountItem[] AccGetChartOfAccounts(ref DBUser DBUser, ref string retStr)
    {
        retStr = "OK";
        IList<AccountItem> ChartOfAccounts = new List<AccountItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            retStr = wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                wfacc.load_ChartOfAccounts(ref ChartOfAccounts, ref retStr);
                retStr = String.Concat(retStr, wfconn.CompID);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return ChartOfAccounts.ToArray();
    }
    public string AccSaveAccount(ref DBUser DBUser, AccountItem Account)
    {
        string retStr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            retStr = wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.Accounts wfacc = new wfws.Accounts(ref DBUser);
                wfacc.SaveAccount(Account);
                retStr = String.Concat(retStr, wfconn.CompID);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string AccDeleteAccount(ref DBUser DBUser, AccountItem Account)
    {
        string retStr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            retStr = wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.Accounts wfacc = new wfws.Accounts(ref DBUser);
                wfacc.DeleteAccount(Account);
                retStr = String.Concat(retStr, wfconn.CompID);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string AccVoucherAdd(ref DBUser DBUser, ref VoucherIn voucher)
    {
        string retStr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            retStr = wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                retStr = wfacc.voucher_save(ref voucher);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string AccVoucherAddPict(ref DBUser DBUser, VoucherPictures VoucherPict)
    {
        string retStr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            retStr = wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                retStr = wfacc.voucher_savepic(VoucherPict);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }

    public int AccVoucherNewPict(ref DBUser DBUser, byte[] VoucherPict)
    {
        string retStr = "OK";
        int PictID = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            retStr = wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                //retStr = wfacc.voucher_savepic(VoucherPict);
                PictID = wfacc.voucher_add_pict(VoucherPict);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return PictID;
    }

 

    public int AccNextVoucherNo(ref DBUser DBUser, string LedgerName, DateTime EnterDate)
    {
        int NewVoucherNo = -1;
        string retStr = "OK";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            retStr = wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                NewVoucherNo = wfacc.VoucherNo_GetNext(LedgerName, EnterDate);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return NewVoucherNo;
    }
    public Ledger[] AccGetLedgerList(ref DBUser DBUser)
    {
        IList<Ledger> Ledgers = new List<Ledger>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                wfacc.load_Ledgers(ref Ledgers);
            }
        }
        catch (Exception e)
        {
            //            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return Ledgers.ToArray();
    }
    public VoucherOut[] AccVouchersGet(ref DBUser DBUser, VoucherCriteria Criteria)
    {
        IList<VoucherOut> Vouchers = new List<VoucherOut>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            wfComp.get_company_by_Guid(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                wfacc.get_vouchers(ref Vouchers, Criteria);
            }
        }
        catch (Exception e)
        {
            //            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return Vouchers.ToArray();
    }
    // Dimension
    public DimensionItem[] DimensionListGet(ref DBUser DBUser, ref Dimension myDim, ref string retStr)
    {
        retStr = "OK";
        IList<DimensionItem> Dimensions = new List<DimensionItem>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                wfacc.load_Dimensions(ref myDim, ref Dimensions, ref retStr);
                retStr = String.Concat(retStr, wfconn.CompID);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return Dimensions.ToArray();
    }
    public string DimensionLoad(ref DBUser DBUser, ref Dimension myDim, ref int dCount)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                dCount = wfacc.Dimension_Get(ref myDim);
            }
            else
            {
                retStr = "no company";
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string DimensionUpdate(ref DBUser DBUser, ref Dimension myDim)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                retStr = wfacc.Dimension_Update(DBUser.CompID, ref myDim);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string DimensionUpdateLawyer(ref DBUser DBUser, ref Dimension myDim)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                retStr = wfacc.Dimension_UpdateLawyer(DBUser.CompID, ref myDim);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public string DimensionTimeItemAdd(ref DBUser DBUser, ref fi_dimensions_timeitems myItem)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                int itemID = wfacc.Dimension_TimeItemsAdd(ref myItem);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public Dimensions_ClientStatement[] DimensionLawyerStatement(ref DBUser DBUser, string Dim3, ref string retstr)
    {
        retstr = "OK";
        IList<Dimensions_ClientStatement> Statement = new List<Dimensions_ClientStatement>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            var wfComp = new wfws.Company(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                wfacc.load_Dimensions_Lawyer_statement(Dim3, ref Statement, ref retstr);
                retstr = String.Concat(retstr, wfconn.CompID);
            }
        }
        catch (Exception e)
        {
            retstr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return Statement.ToArray();
    }
    public string DimensionTimeItemUpdate(ref DBUser DBUser, ref fi_dimensions_timeitems myItem)
    {
        string retStr = "err";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                retStr = wfacc.Dimension_TimeItemsUpdate(ref myItem);
            }
        }
        catch (Exception e)
        {
            retStr = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStr;
    }
    public int DimensionTimeItemToInvoiceAll(ref DBUser DBUser, DateTime EnterDate)
    {
        int iCount = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                iCount = wfacc.Dimension_TimeItemsToInvoice(EnterDate);
            }
        }
        catch (Exception e)
        {
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return iCount;
    }

    public decimal DimensionTotalLoad(ref DBUser DBUser, int DimNo, string DimID, string Account)
    {
        decimal dimTotal = 0;
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                var wfacc = new wfws.Accounts(ref DBUser);
                dimTotal = wfacc.Dimension_Total(DimNo,DimID,Account);
            }
        }
        catch (Exception e)
        {
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }

        return dimTotal;
    }





    public string[] DataTransferOut(ref DBUser DBUser, String UseDefinition)
    {
        string RetVal = "";
        string[] retStrings = { "err", "" };
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.datatransfers dt = new wfws.datatransfers(ref DBUser);
                dt.UseDefinition(UseDefinition);
                dt.Username = "WCF";
                int TransID = dt.DatatransferExecuteOut(DBUser.CompID, UseDefinition);

                retStrings = dt.DatatransferGetLines(TransID);
              
            }
            else
            {
                RetVal = "no company";
            }
        }
        catch (Exception e)
        {
            RetVal = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return retStrings;
    }
    public int DataTransferIn(ref DBUser DBUser, string[] FileContent, string UseDefinition)
    {
        string RetVal = "";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.datatransfers dt = new wfws.datatransfers(ref DBUser);
                dt.UseDefinition(UseDefinition);
                dt.Username = "WCF";
                dt.DatatransferNewTransfer();
                dt.DatatransferPutLines(FileContent);
                dt.DatatransferExecute();
            }
            else
            {
                RetVal = "no company";
            }
        }
        catch (Exception e)
        {
            RetVal = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return -1;
    }
    public DataTransferDefinition[] DataTransferDefinitions(ref DBUser DBUser, TransferType Direction)
    {
        string RetVal = "";
        IList<DataTransferDefinition> DTDefs = new List<DataTransferDefinition>();
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.datatransfers wfweb = new wfws.datatransfers(ref DBUser);
                int LineCount = wfweb.DatatransferGetDefinitionList(ref DTDefs, Direction);
            }
            else
            {
                RetVal = "no company";
            }
        }
        catch (Exception e)
        {
            RetVal = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return DTDefs.ToArray();
    }
    public DataTransferDefinition DataTransferGetDefinition(ref DBUser DBUser, string ThisDef)
    {
        var wfconn = new wfws.ConnectLocal(DBUser);
        DataTransferDefinition TheDef = new DataTransferDefinition();
        wfconn.ConnectionGetByGuid_02(ref DBUser);
        if (DBUser.CompID > 0)
        {
            wfws.datatransfers wfweb = new wfws.datatransfers(ref DBUser);
            TheDef = wfweb.UseDefinition(ThisDef);
            //TheDef = wfweb.GetActiveDefinition();
        }
        return TheDef;
    }
    public DataTransfer[] DataTransfers(ref DBUser DBUser)
    {
        string RetVal = "";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0)
            {
                wfws.datatransfers wfweb = new wfws.datatransfers(ref DBUser);
                IList<DataTransfer> DTIDs = new List<DataTransfer>();
                //int LineCount = wfweb.DatatransferGetList(ref DTIDs);
            }
            else
            {
                RetVal = "no company";
            }
        }
        catch (Exception e)
        {
            RetVal = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return new List<DataTransfer>().ToArray();
    }
    public string GetDataTransferByID(ref DBUser DBUser, int TransferID)
    {
        string RetVal = "";
        try
        {
            var wfconn = new wfws.ConnectLocal(DBUser);
            wfconn.ConnectionGetByGuid_02(ref DBUser);
            if (DBUser.CompID > 0) {
                wfws.datatransfers wfweb = new wfws.datatransfers(ref DBUser);
                wfweb.SetTransferID(TransferID);
                IList<string> lines = new List<string>();
                int LineCount = wfweb.DatatransferRetrieve(ref lines);
                foreach (string line in lines) RetVal = RetVal + line;
            }
            else {
                RetVal = "no company";
            }
        }
        catch (Exception e)
        {
            RetVal = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return RetVal;
    }
    public string CleanDataTransfersOlderThan(ref DBUser DBUser, int TransferID)
    {
        return "Not implemented";
    }
    public string BarcodeLookup(ref DBUser DBUser, string Barcode)
    {
        string RetVal = "Not found";
        try
        {
            wfws.Barcode bc = new wfws.Barcode(ref DBUser);
            if (DBUser.CompID > 0)
            {
                RetVal = bc.BarcodeLookup(Barcode);
            }
        }
        catch (Exception e)
        {
            RetVal = e.Message;
            throw new FaultException(string.Concat("wf_wcf: ", e.Message), new FaultCode("wfwcfFault"));
        }
        return RetVal;
    }

    public int Danloen(string AccountKey,string vouchers)
    {

        string constr;

        int CompID;

        var wfutil = new utils();

        CompID = wfutil.connectstring_split(ref AccountKey);

        constr = AccountKey;

        danloenvoucher MyVoucher = new danloenvoucher(constr, CompID);

        int retVal;

        retVal = MyVoucher.vouchers_in(vouchers);
                
        return retVal;

        // return "Received wage voucher for account key " + AccountKey;


        return 0;
    }


}