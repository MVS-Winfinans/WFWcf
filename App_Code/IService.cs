using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService" in both code and config file together.

[ServiceContract]
public interface IService
{
    [OperationContract] int IService(ref DBUser DBUser);
    [OperationContract] string HelloWorld();
    [OperationContract] string Ping();
    [OperationContract] string HelloWorldError();
    [OperationContract] DBUser CreateDBUser(Guid DBKey);
    [OperationContract] string ConnectionTest(ref DBUser DBUser);
    [OperationContract] string UserLogin(ref DBUser DBUser, string username, string  Psssword, ref Address wfAddress);
    [OperationContract] string UserPassword(ref DBUser DBUser,ref string password,ref string email,  ref Address wfAddress);
    [OperationContract] string UserUpdate(ref DBUser DBUser, string userName, string email,  ref Address wfAddress);
    [OperationContract] string UserAddNew(ref DBUser DBUser, string username, string email,ref Address wfAddress);
    [OperationContract] wflist[] WF_GetLists(wfmodule module, string language);
    [OperationContract] wflist[] WF_GetDropdownList(wfdroplist List, string language);
    [OperationContract] Guid[] GetServiceProviderAdmissions(Guid ServiceProviderID);

    // Company
    [OperationContract] CompanyInf CompanyLoad(ref DBUser DBUser);
    [OperationContract] string CompanySellerLookup(ref DBUser DBUser, ref CompanySeller Seller, ref int seCount);
    [OperationContract] Boolean CompanyUserLookup(ref DBUser DBUser, string username, string Psssword, ref string retstr);
    [OperationContract] CompanyUser[] CompanyUsersLoad(ref DBUser DBUser);
    [OperationContract] Activity[] CompanyActivitiesLookup(ref DBUser DBUser, ref Activity wfActivity, ref string retstr);
    [OperationContract] int[] CompanySellersLoad(ref DBUser DBUser);
    [OperationContract] CompanySalesman[] CompanySalesmenLoad(ref DBUser DBUser);
   
    // Address
    [OperationContract] string AddressUpdate(ref DBUser DBUser,ref Address wfAddress);
    [OperationContract] string AddressGetById(ref DBUser DBUser, Guid AdrGuid,ref Address wfAddress);
    [OperationContract] string AddressLookup(ref DBUser DBUser, ref Address wfAddress,ref int adCount);
    [OperationContract] int AddressLogin(ref DBUser DBUser, string UserName, string HashKey);
    [OperationContract] string AddressNewHashKey(ref DBUser DBUser, int AdrID, string HashKey);
    [OperationContract] AddressesShipBillItem[] AddressShipBillLoad(ref DBUser DBUser, ref Address wfAddress);
    [OperationContract] string AddressShipBillAdd(ref DBUser DBUser, ref Address wfAddress,ref AddressesShipBillItem ShipBillItem);
    [OperationContract] string AddressShipBillDelete(ref DBUser DBUser, ref Address wfAddress, ref AddressesShipBillItem ShipBillItem);
    [OperationContract] AddressItem[] AddressListGet(ref DBUser DBUser,string propertyID,ref string retstr);
    [OperationContract] AddressItem[] AddressLookupList(ref DBUser DBUser, ref Address wfAddress,ref string retstr);
    [OperationContract] int[] AddressListChanged(ref DBUser DBUser, ref int AddressID, ref DateTime TimeChanged);
 //   [OperationContract] int[] AddressListReminders(ref DBUser DBUser, ref int LastAddressID, ReminderLevel ReminderLevel);
    [OperationContract] AddressShipTo[] AddressListChangedShip(ref DBUser DBUser, ref int AddressID, ref DateTime TimeChanged);
    [OperationContract] string AddressStatementGet(ref DBUser DBUser, ref AddressStatement wfAddressStatement);  //notice a more detailed function in AccVouchersGet()
    [OperationContract] string AddressStatementGetOpen(ref DBUser DBUser, ref AddressStatement wfAddressStatement);  //notice a more detailed function in AccVouchersGet()
    [OperationContract] string AddressApprove(ref DBUser DBUser, Address wfAddress, string ApprovedBy);
    [OperationContract] CreditCardErrors[] AddressGetCreditCardErrors(ref DBUser DBUser,int AddressID,int Status, DateTime FromDate);
    [OperationContract] AddressCollectable[] AddressCollectablesGet(ref DBUser DBUser);
    [OperationContract] string AddressCollectablesUpdateStatus(ref DBUser DBUser, int AddressID, int NewStatus);
    [OperationContract] int[] AddressDocumentListGet(ref DBUser DBUser, int AddressID);
    [OperationContract] byte[] AddressGetArchivedDocument(ref DBUser DBUser, int AddressID, int DocID, ref string ContentType, ref string Description);
    [OperationContract] string AddressBlur(ref DBUser DBUser, int AddressID);
    // Activities
    [OperationContract] Activity[] AddressActivitiesLoad_passthrough(ref DBUser DBUser, int UserTop,string UserWhere,string UserOrder);
    [OperationContract] Activity[] AddressActivitiesLoad(ref DBUser DBUser, ref Address wfAddress);
    [OperationContract] string AddressActivityAssociate(ref DBUser DBUser,ref Activity NewActivity,ref Address wfAddress);
    [OperationContract] string AddressActivityLoad(ref DBUser DBUser, ref Activity wfActivity);
    [OperationContract] string AddressActivityUpdate(ref DBUser DBUser, ref Activity wfActivity);
    [OperationContract] string AddressActivityDelete(ref DBUser DBUser, ref Activity wfActivity);
    [OperationContract] ActivityType[] AddressActivityTypesLoad(ref DBUser DBUser);
    [OperationContract] ActivityState[] AddressActivityStateLoad(ref DBUser DBUser);
   
    // Properties
    [OperationContract] string AddressPropertyAdd(ref DBUser DBUser, int Propertyid,ref Address wfAddress);
    [OperationContract] string AddressPropertyDelete(ref DBUser DBUser,int FromID,int ToDi, ref Address wfAddress);
    [OperationContract] string AddressPropertyPresent(ref DBUser DBUser, int Propertyid, ref Address wfAddress,ref Boolean present);
    [OperationContract] AddressProperty[] AddressPropertiesLoadAll(ref DBUser DBUser, int FromID, int ToDi, ref string retstr);
    [OperationContract] int[] AddressPropertyListGet(ref DBUser DBUser, int propertyID);




    // Contacts
    [OperationContract] string ContactNew(ref DBUser DBUser, ref Contact NewContact);
    [OperationContract] Contact[] ContactsLoad(ref DBUser DBUser, int AddressID);





    // Alert
    [OperationContract] AddressAlert[] AddressAlertLoad(ref DBUser DBUser, int AddressID, int AlertsTop, ref string retstr);
    [OperationContract] string AddressAlertAdd(ref DBUser DBUser, ref AddressAlert wfAlert);
    [OperationContract] string AddressAlertUpdate(ref DBUser DBUser, ref AddressAlert wfAlert);
    [OperationContract] string AddressAlertDelete(ref DBUser DBUser, ref AddressAlert wfAlert);

    // ExtraLines
    [OperationContract] AddressExtraLine[] AddressExtraLinesLoad(ref DBUser DBUser, int AddressID, ref string retstr);
    [OperationContract] string AddressExtraLinesUpdate(ref DBUser DBUser, ref AddressExtraLine WfExtraLine);
    [OperationContract] int AddressExtraLineTemplate(ref DBUser DBUser, int AddressID, ref string TemplateName);


    // Categories
    [OperationContract] AddressCategory[] AddressCategoriesLoad(ref DBUser DBUser);
    // Sales
    [OperationContract] string SalesOrderAdd(ref DBUser DBUser, ref OrderSales wfOrder,SalesOrderTypes OrderType );
    [OperationContract] string SalesOrderLoad(ref DBUser DBUser,  ref OrderSales wfOrder);
    [OperationContract] int SalesOrderToInvoice(ref DBUser DBUser, ref OrderSales WfOrder);
    [OperationContract] string SalesOrderClose(ref DBUser DBUser, ref OrderSales wfOrder);
    [OperationContract] string SalesOrderSave(ref DBUser DBUser, ref OrderSales WfOrder);
    [OperationContract] string SalesOrderDelete(ref DBUser DBUser, ref OrderSales WfOrder);

    [OperationContract] string SalesOrderLookup(ref DBUser DBUser, ref OrderSales wfOrder, SalesOrderTypes OrderType);
    [OperationContract] int SalesOrderGetSaleIDFromGuid(ref DBUser DBUser,Guid GuidInvoice);
    [OperationContract] OrderLine[] SalesOrderLoadItems(ref DBUser DBUser, ref OrderSales wfOrder,ref string retstr);
    [OperationContract] string SalesOrderAddItem(ref DBUser DBUser,ref OrderSales wfOrderSales ,OrderLine LineItem);
    [OperationContract] string SalesOrderLoadItem(ref DBUser DBUser,ref OrderLine LineItem);
    [OperationContract] string SalesOrderDeleteItem(ref DBUser DBUser, ref OrderSales wfOrderSales , string ItemID);
    [OperationContract] string SalesOrderChangeQtyPriceOnItem(ref DBUser DBUser,ref  OrderLine LineItem);
    [OperationContract] string SalesOrderDeleteOrderLine(ref DBUser DBUser, ref OrderLine LineItem);


    [OperationContract] string SalesOrderAddPayment(ref DBUser DBUser, ref OrderSales WfOrder, OrderPayment PaymentItem);
    [OperationContract] OrderPayment[] SalesOrderLoadPayments(ref DBUser DBUser, ref OrderSales WfOrder, ref string errstr);

    [OperationContract] string SalesOrderAddressAssociate(ref DBUser DBUser,ref OrderSales wfOrderSales ,int BillTo, int ShipTo);
    [OperationContract] string SalesOrderCalendarBlock(ref DBUser DBUser,ref OrderSales wfOrderSales ,DateTime BlockDate, string UserID);
    [OperationContract] string SalesOrderCalendarBlock_upto(ref DBUser DBUser, ref OrderSales wfOrderSales, DateTime BlockDate, string UserID);
    [OperationContract] string SalesOrderCalendarUnBlock(ref DBUser DBUser, ref OrderSales wfOrderSales, DateTime BlockDate);
    [OperationContract] OrderSalesItem[] SalesOrderListLoad(ref DBUser DBUser, ref OrderSales wfOrderSales, ref string retstr, SalesOrderTypes OrderType);
    [OperationContract] SalesStatLine[] SalesStatsLoadItems(ref DBUser DBUser, ref OrderSales wfOrderSales, ref string retstr, SalesOrderTypes OrderType);
    [OperationContract] int SalesGetCategoryByName(ref DBUser DBUser,string category);
    [OperationContract] int SalesGetSellerByName(ref DBUser DBUser, string seller);
    [OperationContract] SalesCategorie[] SalesCategoriesLoad(ref DBUser DBUser);
    [OperationContract] OrderSalesItemChanged[] SalesOrderChangedItems(ref DBUser DBUser,OrderSalesItemChanged wforder, ref DateTime  SalesTimestamp , SalesOrderTypes OrderType, Boolean expanded);
    [OperationContract] OrderSalesItemChanged[] SalesOrderFactoringItems(ref DBUser DBUser);
    [OperationContract] int SalesOrderFactoringItemConfirm(ref DBUser DBUser,int ListID);
    [OperationContract] OrderSalesItemChanged[] SalesOrderB2BBackboneItems(ref DBUser DBUser);
    [OperationContract] int SalesOrderB2BBackboneItemConfirm(ref DBUser DBUser, int SaleID);
    [OperationContract] void SalesOrder_MarkPaidSales(ref DBUser DBUser);




    // Sales reports
    [OperationContract] byte[] SalesOrderGetPDFArray(ref DBUser DBUser, int SaleID, SalesReports ReportID);
    [OperationContract] string SalesOrderPDFmail(ref DBUser DBUser, int SaleID, SalesReports ReportID);


    // Sales Ubl                //TODO  Bør slettes, da al UBL er isoleret i en seperat klasse.
    [OperationContract] string SalesOrderToUBL(ref CompanyInf Company,ref CompanySeller wfseller, ref OrderSales wfOrder,ref Address BillToAddress,ref Address ShipToAddress, SalesOrderTypes OrderType);
    [OperationContract] string SalesOrderIDToUBL(ref DBUser DBUser, ref  SalesOrderUBL xmlUBL);

    // sales deliveries
    [OperationContract] SalesDeliveriesProducts[] SalesDeliverySchedule(ref DBUser DBUser, int BillTo);
    [OperationContract] SalesDeliveriesProducts[] SalesDeliverySchedule_2(ref DBUser DBUser, int BillTo);

    // purchase
    [OperationContract] string PurchaseOrderLoad(ref DBUser DBUser,ref OrderPurchase wfOrderPurc);
    [OperationContract]  OrderPurchaseItem[]  PurchaseOrderListLoad(ref DBUser DBUser,ref OrderPurchase wfOrderPurc, ref string retstr, PurchaseOrderTypes OrderType);
    [OperationContract] OrderLinePurc[] PurcOrderLoadItems(ref DBUser DBUser, ref OrderPurchase wfOrderPurc, ref string errstr);
    [OperationContract] string PurcOrderAdd(ref DBUser DBUser, ref OrderPurchase wfOrderPurc);
    [OperationContract] string PurcOrderAddPayment(ref DBUser DBUser, ref OrderPurchase WfOrderPurc, OrderPurcPayment PaymentItem);
    [OperationContract] OrderPurcPayment[] PurcOrderLoadPayments(ref DBUser DBUser, ref OrderPurchase WfOrderPurc, ref string errstr);
    [OperationContract] string PurcOrderLookup(ref DBUser DBUser, ref OrderPurchase WfOrderPurc, ref int puCount);

    // inventory
    [OperationContract] inventoryItem[] InventorylistLoad(ref DBUser DBUser);
    [OperationContract] inventoryItem[] InventorylistLoadSelection(ref DBUser DBUser,string SelectionID, ref string retstr);
    [OperationContract] inventoryItem[] InventorylistLoadGroupsFi(ref DBUser DBUser,string groupFI);
    [OperationContract] inventoryLocation[] InventoryLoadLocations(ref DBUser DBUser);
    [OperationContract] inventorySelection[] InventoryLoadSelections(ref DBUser DBUser);
    [OperationContract] inventoryGroupFi[] InventoryLoadGroupFI(ref DBUser DBUser);
    [OperationContract] string inventoryAddItemOne(ref DBUser DBUser, ref inventoryItem Item);
    [OperationContract] string  inventoryGetItemOne(ref DBUser DBUser,ref inventoryItem Item);
    [OperationContract] string inventorySaveItemOne(ref DBUser DBUser, ref inventoryItem Item);
    [OperationContract] string inventoryGetItemOneCardOnly(ref DBUser DBUser, ref inventoryItem Item);
    [OperationContract] inventoryPicture inventoryGetItemPicture(ref DBUser DBUser, string ItemID, int PicID, bool Thumbnail);
    [OperationContract] string  inventoryGetItemExtra(ref DBUser DBUser,ref inventoryItemExtra Item);
    [OperationContract] string[]  inventoryGetSelectionText(ref DBUser DBUser,string Selection,string countryID,ref string errStr);
    [OperationContract] string  inventoryGetVat(ref DBUser DBUser, ref InventoryVAT VAT);
    [OperationContract] MenuItem[] MenuLoadItems(ref DBUser DBUser, string Selection);
    [OperationContract] MenuItem[] MenuShopLoadItems(ref DBUser DBUser, string Selection);
    [OperationContract] MenuItem[] MenuLoadItemsTranslated(ref DBUser DBUser, string Language,  string Selection);
    [OperationContract] byte[] MenuLoadItemPicture(ref DBUser DBUser, string SelectionID, string Description, bool Thumbnail, ref string PictType);

    [OperationContract] string[] InventoryItemsChanged(ref DBUser DBUser,ref string ItemID, ref DateTime  TimeChanged);
    [OperationContract] string InventoryItemLookup(ref DBUser DBUser, ref inventoryItem Item, ref int ItemCount);
    [OperationContract] int inventoryAddPicture(ref DBUser DBUser, string ItemID, byte[] Picture, string Description);
    [OperationContract] InventoryExtraLine[] InventoryExtraLinesLoad(ref DBUser DBUser, string ItemID, ref string retstr);
    [OperationContract] InventoryStyle[] InventoryStyleLoad(ref DBUser DBUser, string ItemID, ref string retstr);
    [OperationContract] inventoryContainer[] inventoryContainersLoad(ref DBUser DBUser, string ItemID, ref string retstr);
    [OperationContract] string InventoryContainerAdd(ref DBUser DBUser, ref inventoryContainer Item);
    [OperationContract] string InventoryContainerClear(ref DBUser DBUser, ref inventoryContainer Item);

    [OperationContract] inventoryVendors[] inventoryVendorsLoad(ref DBUser DBUser, string ItemID, ref string retstr);
    [OperationContract] string InventoryVendorAdd(ref DBUser DBUser, ref inventoryVendor Item);
    [OperationContract] string InventoryVendorClear(ref DBUser DBUser, string ItemID);

    // production

    [OperationContract] string ProdAssAdd(ref DBUser DBUser, ref ProdAssembly wf_Ass);
    [OperationContract] string ProdAssLoad(ref DBUser DBUser, ref ProdAssembly wf_Ass);
    [OperationContract] string ProdAssSave(ref DBUser DBUser, ref ProdAssembly wf_Ass);
    
    // shop 

    [OperationContract] string  shopLoad(ref DBUser DBUser,ref ShopWf myShop);
    [OperationContract] string  shopGetText(ref DBUser DBUser,string TextKey,string Country,ref string header,ref string body);
    [OperationContract] inventoryItem[]  shopProductsLoadSelection(ref DBUser DBUser,string Selection,ref string retstr);

    // basket

    [OperationContract] string  BasketLoad(ref DBUser DBUser, ref ShopBasket myBasket);
    [OperationContract] string  BasketSave(ref DBUser DBUser, ref ShopBasket myBasket);
    [OperationContract] string  BasketAddAddresses(ref DBUser DBUser, ref ShopBasket myBasket);
    [OperationContract] string  BasketConfirm(ref DBUser DBUser, ref ShopBasket myBasket);
    [OperationContract] ShopBasketItem[]  BasketLoadItems(ref DBUser DBUser, ref ShopBasket myBasket,ref string retstr);
    [OperationContract] string  BasketAddItem(ref DBUser DBUser, ref ShopBasket myBasket,ref ShopBasketItem myBasketItem);
    [OperationContract] string  BasketUpdItem(ref DBUser DBUser, ref ShopBasket myBasket,ref ShopBasketItem myBasketItem);
    [OperationContract] string BasketIDByAddressID(ref DBUser DBUser, int addressID);

    // Accounts //Finance

    [OperationContract] AccountItem[]  AccGetChartOfAccounts(ref DBUser DBUser,ref string retstr);
    [OperationContract] string AccSaveAccount(ref DBUser DBUser, AccountItem Account);
    [OperationContract] string AccDeleteAccount(ref DBUser DBUser, AccountItem Account);
    [OperationContract] string  AccVoucherAdd(ref DBUser DBUser,ref VoucherIn  voucher);
    [OperationContract] int AccNextVoucherNo(ref DBUser DBUser, string LedgerName, DateTime EnterDate);
    [OperationContract] Ledger[] AccGetLedgerList(ref DBUser DBUser);
    [OperationContract] VoucherOut[] AccVouchersGet(ref DBUser DBUser, VoucherCriteria Criteria );
    [OperationContract] string AccVoucherAddPict(ref DBUser DBUser, VoucherPictures VoucherPict);


    // Dimensions
    [OperationContract] string  DimensionLoad(ref DBUser DBUser,ref Dimension myDim,ref int dCount);
    [OperationContract] string DimensionUpdate(ref DBUser DBUser, ref Dimension myDim);
    [OperationContract] string DimensionUpdateLawyer(ref DBUser DBUser, ref Dimension myDim);
    [OperationContract] DimensionItem[] DimensionListGet(ref DBUser DBUser, ref Dimension myDim, ref string retstr);
    [OperationContract] string DimensionTimeItemAdd(ref DBUser DBUser, ref fi_dimensions_timeitems myItem);
    [OperationContract] string DimensionTimeItemUpdate(ref DBUser DBUser, ref fi_dimensions_timeitems myItem);
    [OperationContract] int DimensionTimeItemToInvoiceAll(ref DBUser DBUser, DateTime EnterDate);
    [OperationContract] Dimensions_ClientStatement[] DimensionLawyerStatement(ref DBUser DBUser, string Dim3, ref string retstr);
    [OperationContract] decimal DimensionTotalLoad(ref DBUser DBUser,int DimNo,string DimID,string Account);

    //Datatransfers
    [OperationContract] string[] DataTransferOut(ref DBUser DBUser, string UseDefinition);
    [OperationContract] int DataTransferIn(ref DBUser DBUser, string[] FileContent, string UseDefinition );
    [OperationContract] DataTransferDefinition[] DataTransferDefinitions(ref DBUser DBUser, TransferType Direction);
    [OperationContract] DataTransferDefinition DataTransferGetDefinition(ref DBUser DBUser, string ThisDef);
    [OperationContract] DataTransfer[] DataTransfers(ref DBUser DBUser);
    [OperationContract] string GetDataTransferByID(ref DBUser DBUser, int TransferID);
    [OperationContract] string CleanDataTransfersOlderThan(ref DBUser DBUser, int TransferID);

    //Barcode
    [OperationContract] string BarcodeLookup(ref DBUser DBUser, string Barcode);
}


[DataContract]
public class CompositeType
{
	bool boolValue = true;
	string stringValue = "Hello ";
	[DataMember]
	public bool BoolValue
	{
		get { return boolValue; }
		set { boolValue = value; }
	}

	[DataMember]
	public string StringValue
	{
		get { return stringValue; }
		set { stringValue = value; }
	}
}

[DataContract]
public class wflist
{
    [DataMember] public int Class;
    [DataMember] public string ClassText;
}

[DataContract] public enum wfmodule
{
    [EnumMember] Sale = 1,
    [EnumMember] Purchase = 2,
    [EnumMember] Production = 6,
    [EnumMember] AddressType = 7,
    [EnumMember] VoucherSource = 8,
    [EnumMember] Unknown10 = 10,
    [EnumMember] Unknown11 = 11,
    [EnumMember] Unknown12 = 12,
    [EnumMember] Unknown20 = 20,
    [EnumMember] Payment = 30,
    [EnumMember] PrinterTray = 40,
    [EnumMember] ItemStatus = 100,
    [EnumMember] ItemType = 1000,
    [EnumMember] Unknown1001 = 1001,
    [EnumMember] Pricing = 1002,
    [EnumMember] CostPricing = 1003,
    [EnumMember] FieldType = 1005,
    [EnumMember] InvoiceType = 2000,
    [EnumMember] CreditorPaymentStatus = 2010,
    [EnumMember] CollectionStatus = 2020,
    [EnumMember] DebtorPaymentStatus = 2030,
    [EnumMember] AccountType = 5000,
    [EnumMember] Unknown6000 = 6000
}

[DataContract] public enum wfdroplist
{
    [EnumMember] AddressTypes = 1001,
    [EnumMember] Reminders = 1006,
    [EnumMember] SalesTypes = 1020,
    [EnumMember] PurchaseTypes = 1022,
    [EnumMember] ProductionTypes = 1052,
    [EnumMember] CollectionStatuses = 7000,
    [EnumMember] FiscalMonths = 9000
}

[DataContract]
public class DBUser
{
    [DataMember] public Guid DBKey = Guid.Empty;
    [DataMember] public Guid CompanyKey = Guid.Empty;
    [DataMember] public Guid BasketGuid = Guid.Empty;
    [DataMember] public string ConnectionString;
    [DataMember] public int CompID;
    [DataMember] public int ConnID;
    [DataMember] public string ShopID;
    [DataMember] public Boolean PublicConnection;
    [DataMember] public string Message;
    [DataMember] public Boolean BlindUpdate;
 }

[DataContract]
public class CompanyInf
{
    [DataMember] public int CompID;
    [DataMember] public string CompanyName;
    [DataMember] public string Street;
    [DataMember] public string HouseNumber;
    [DataMember] public string CityName;
    [DataMember] public string PostalZone;
    [DataMember] public string Region;
    [DataMember] public string InHouseMail;
    [DataMember] public string CompanyNo;
    [DataMember] public string TaxSchemeID;
    [DataMember] public string ean;
    [DataMember] public string endpointtype;
    [DataMember] public string CompanyPhone;
    [DataMember] public string MobilePhone;
    [DataMember] public string Country;
    [DataMember] public string Language;
    [DataMember] public int AccMailClient;
    [DataMember] public int saMailClient;
    [DataMember] public int puMailClient;
    [DataMember] public Guid GuidComp = Guid.Empty;
    [DataMember] public string DimName1;
    [DataMember] public string DimName2;
    [DataMember] public string DimName3;
    [DataMember] public string DimName4;
    [DataMember] public string Email;
    [DataMember] public string AdditionalAccountID;
    [DataMember] public string CompanyWeb;

}

[DataContract] public class CompanySeller {
    [DataMember] public int SellerID;
    [DataMember] public string SellerNo;
    [DataMember] public string Description;
    [DataMember] public string SellerName;
    [DataMember] public string SellerStreet;
    [DataMember] public string SellerHouseNumber;
    [DataMember] public string SellerInHouseMail;
    [DataMember] public string SellerCityName;
    [DataMember] public string SellerPostalZone;
    [DataMember] public string SellerRegion;
    [DataMember] public string SellerCountryID;
    [DataMember] public string SellerEAN;
    [DataMember] public string PaymentType;
    [DataMember] public string BankName;
    [DataMember] public string BankAddress;
    [DataMember] public string RegistrationNo;
    [DataMember] public string AccountNo;
    [DataMember] public string BIC;
    [DataMember] public string IBAN;
    [DataMember] public string CompanyNo;
    [DataMember] public string Email;
    [DataMember] public string CreditAccount;

}

[DataContract]
public class CompanyUser
{
    [DataMember]  public string UserID;
    [DataMember]  public string UserName;
    [DataMember]  public Guid netUserid;

}


[DataContract]
public class CompanySalesman
{
    [DataMember]
    public string SalesmanID;
    [DataMember]
    public string SalesmanName;
    [DataMember]
    public string email;

}


[DataContract] public class Address {
    [DataMember] public int AddressID;
    [DataMember] public Guid AdrGuid;
    [DataMember] public AddressType AddrType = AddressType.Undefined;
    [DataMember] public string Account;
    [DataMember] public string CompanyName;
    [DataMember] public string LastName;
    [DataMember] public string Department;
    [DataMember] public string Address1;
    [DataMember] public string Address2;
    [DataMember] public string HouseNumber;
    [DataMember] public string InHouseMail;
    [DataMember] public string CountryID;
    [DataMember] public string Region;
    [DataMember] public string PostalCode;
    [DataMember] public string City;
    [DataMember] public string Phone;
    [DataMember] public string Fax;
    [DataMember] public string email;
    [DataMember] public string emailInvoice;
    [DataMember] public string VATNumber;
    [DataMember] public string ean;
    [DataMember] public string CompanyWeb;
    [DataMember] public string ContactPerson;
    [DataMember] public string category;
    [DataMember] public int SellerID;
    [DataMember] public string Currency;
    [DataMember] public string Language;
    [DataMember] public string DebtorGroup;
    [DataMember] public string TermsOfPaymentDeb;
    [DataMember] public string Notes;
    [DataMember] public string ImportID;
    [DataMember] public string PriceIDSales;
    [DataMember] public Boolean CreditStop;
    [DataMember] public string internRef;
    [DataMember] public DateTime TimeChanged;
    [DataMember] public string EndpointType;
    [DataMember] public string CountryISO3166_2;
    [DataMember] public string BankingRegNo;
    [DataMember] public string BankingAccount;
    [DataMember] public string CreditorNo;
    [DataMember] public string IBAN;
    [DataMember] public string BIC;
    [DataMember] public string Ticket;
    [DataMember] public DateTime TicketEnterDate;
    [DataMember] public string CardNoMask;
    [DataMember] public DateTime CardExpDate;
    [DataMember] public string CardType;
    [DataMember] public string PayPhone;
}

[DataContract] public class AddressItem {
    [DataMember] public int AddressID;
    [DataMember] public string Account;
    [DataMember] public string CompanyName;
    [DataMember] public string LastName;
    [DataMember] public string Address1;
    [DataMember] public string Address2;
    [DataMember] public string HouseNumber;
    [DataMember] public string PostalCode;
    [DataMember] public string City;
}

[DataContract]
public enum ReminderLevel
{
    [EnumMember] Undefined = 0,
    [EnumMember] FirstReminder = 1,
    [EnumMember] SecondReminder = 2,
    [EnumMember] ThirdReminder = 3,
    [EnumMember] CollectionReminder = 4
}
[DataContract]
public enum AddressNameType
{
    [EnumMember] Fullname = 1,
    [EnumMember] Firstname = 2,
    [EnumMember] Lastname = 3,
    [EnumMember] Streetname = 10

}
[DataContract] public class Contact {
    [DataMember] public int ContID;
    [DataMember] public int AddressID;
    [DataMember] public string Type;
    [DataMember] public string ContactName;
    [DataMember] public string MobilPhone;
    [DataMember] public string Job;
    [DataMember] public string email;
    [DataMember] public string Initials;
    [DataMember] public string AccountingCost;
    [DataMember] public string EndpointID;
    [DataMember] public string EndpointScheme;
    [DataMember] public Boolean UBLDefault;
}

[DataContract] public class Activity  {
    [DataMember] public int AddressID;
    [DataMember] public int ActivityID;
    [DataMember] public int ActivityType;
    [DataMember] public DateTime EnterDate;
    [DataMember] public DateTime DueDate;
    [DataMember] public Boolean settled;
    [DataMember] public DateTime SettleDate;
    [DataMember] public string Description;
    [DataMember] public string UserID;
    [DataMember] public string ExtRef;
    [DataMember] public int StateID;
    [DataMember] public string CompanyName;
}

[DataContract] public class ActivityType
{
    [DataMember] public int Type;
    [DataMember] public string Description;
}

[DataContract] public class ActivityState
{
    [DataMember] public int StateID;
    [DataMember] public string Description;
}


[DataContract] public class AddressProperty {
    [DataMember] public int PropertyID;
    [DataMember] public string Description;
}


[DataContract] public class AddressShipTo
{
    [DataMember] public int AddressID;
    [DataMember] public int ShipTo;
}

[DataContract] public class AddressesShipBillItem
{
   [DataMember] public int AddressID;
   [DataMember] public Boolean UseAsDefault;
   [DataMember] public int ShipBillType;
}

[DataContract] public class AddressAlert
{
    [DataMember] public int AddressID;
    [DataMember] public int AlertID;
    [DataMember] public DateTime TimeStamp;
    [DataMember] public int Severity;
    [DataMember] public string AlertHeader;
    [DataMember] public string AlertText;
    [DataMember] public int AlertType;
    [DataMember] public Boolean on_sa_orders;
    [DataMember] public Boolean on_sa_Inv;
    [DataMember] public Boolean on_pu_orders;
    [DataMember] public Boolean on_pu_inv;
    [DataMember] public Boolean Active;
}

[DataContract] public class AddressExtraLine
{
    [DataMember] public int AddressID;
    [DataMember] public string Description;
    [DataMember] public int LinePos;
    [DataMember] public int Datatype;
    [DataMember] public string Value;
}

[DataContract] public class AddressCategory
{
    [DataMember] public string Category;
    [DataMember] public string Cat_Description;
    [DataMember] public int TemplateID;
}

[DataContract] public class AddressStatement
{
    [DataMember] public string adAccount;
    [DataMember] public int AddressID;
    [DataMember] public Address FullAddress;
    [DataMember] public DateTime FromDate;
    [DataMember] public DateTime ToDate;
    [DataMember] public string Currency;
    [DataMember] public Decimal TotalDeb;
    [DataMember] public Decimal TotalCre;
    [DataMember] public AddressStatementLine[] StatementLines;
}

[DataContract] public class AddressStatementLine
{
    [DataMember] public int ItemID;
    [DataMember] public string Account;
    [DataMember] public DateTime Enterdate;
    [DataMember] public string Description;
    [DataMember] public Decimal CuDebet;
    [DataMember] public Decimal CuCredit;
    [DataMember] public int SourceID;
    [DataMember] public int SourceRef;
    [DataMember] public long Voucher;
    [DataMember] public long InvoiceNo;
    [DataMember] public DateTime Paydate;
    [DataMember] public Boolean BalanceBroughtForward;
    [DataMember] public int SettleID;
}

[DataContract]
public class AddressCollectable
{
    [DataMember] public int AddressID;
    [DataMember] public DateTime CollectableFrom ;
    [DataMember] public int Status;
}

// sales DDDDD HEJ

[DataContract] public class OrderLine {
    [DataMember] public int SaleID;
    [DataMember] public int Liid;
    [DataMember] public string ItemID;
    [DataMember] public string Unit;
    [DataMember] public string ItemDesc;
    [DataMember] public Decimal Qty;
    [DataMember] public Decimal QtyPackages;
    [DataMember] public Decimal SalesPrice;
    [DataMember] public Decimal OrderAmount;
    [DataMember] public Decimal Discount;
    [DataMember] public Decimal DiscountProc;
    [DataMember] public string EAN;
    [DataMember] public string ConsumerUnitEAN;
    [DataMember] public string AddInformation;
    [DataMember] public string Style;
    [DataMember] public string Batch;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public Decimal vat_perc;
    [DataMember] public Boolean VatIncl;
    [DataMember] public Boolean AllowanceCharge;
    [DataMember] public Decimal LineAmount;
    [DataMember] public Decimal LineVat;
    [DataMember] public Decimal LineVatBase;
    [DataMember] public Decimal LinePrice;
    [DataMember] public Decimal LineDiscount;
    [DataMember] public string UNSPSC;
    [DataMember] public string AccountingCost;
    [DataMember] public string GroupFi;
    [DataMember] public DateTime ActualDeliveryDate; 
    [DataMember] public Decimal Weight;
    [DataMember] public Decimal Volume;
    [DataMember] public Decimal SuggestedRetail;
    [DataMember] public string Selection;
    [DataMember] public Boolean Substitutable;
}

[DataContract] public class OrderSales {
    [DataMember] public int SaleID;
    [DataMember] public SalesOrderTypes OrderType;
    [DataMember] public InvCre InvoiceCreditnote = InvCre.invoice;
    [DataMember] public long OrderNo;
    [DataMember] public long InvoiceNo;
    [DataMember] public int BillTo;
    [DataMember] public int ShipTo;
    [DataMember] public int Buyer;
    [DataMember] public string Currency;
    [DataMember] public string Language;
    [DataMember] public int Calendar;
    [DataMember] public DateTime OrderDate;
    [DataMember] public DateTime InvoiceDate;
    [DataMember] public DateTime ShipDate;
    [DataMember] public DateTime PayDate;
    [DataMember] public DateTime StartDate;
    [DataMember] public DateTime EndDate;
    [DataMember] public string TermsOfPayment;
    [DataMember] public OrderLine[] OrderLines;
    [DataMember] public OrderPayment[] OrderPayments;
    [DataMember] public int Category;
    [DataMember] public string salesman;
    [DataMember] public int seller;
    [DataMember] public string EAN;
    [DataMember] public string requisition;
    [DataMember] public string ContactPerson;
    [DataMember] public string Trace;
    [DataMember] public string IntRef;
    [DataMember] public string ExtRef;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public string text_1;
    [DataMember] public string text_2;
    [DataMember] public string text_3;
    [DataMember] public Decimal Total;
    [DataMember] public Decimal TotalVatEx;
    [DataMember] public Decimal TotalVatIn;
    [DataMember] public Decimal TotalVatBasisEx;
    [DataMember] public Decimal TotalVatBasisIn;
    [DataMember] public Decimal TotalInvDiscount;
    [DataMember] public Decimal TotalTaxAmount;
    [DataMember] public Guid GuidInv;
    [DataMember] public int blockReason;
    [DataMember] public string PaymentRef;
    [DataMember] public Decimal AmountRounding;
    [DataMember] public Decimal AmountCharge;
    [DataMember] public Decimal AmountAllowance;
    [DataMember] public Decimal AmountLines;
    [DataMember] public Decimal VatAcquisition;
    [DataMember] public Decimal VatAcquisitionBasis;
    [DataMember] public long SettleNo;
    [DataMember] public string LocationID;
    [DataMember] public string AccountingCost;
    [DataMember] public string UBLContactName;
    [DataMember] public string UBLemail;
    [DataMember] public int ContID;
    [DataMember] public string Initials;
    [DataMember] public string UBLEndpointID;
    [DataMember] public string UBLEndpointScheme;
    [DataMember] public long SettleID;
    [DataMember] public long DeliveryNoteNo;
    [DataMember] public long QuotationNo;
    [DataMember] public int TermsOfDelivery;
}

[DataContract] public class OrderSalesItem {
    [DataMember] public int SaleID;
    [DataMember] public long OrderNo;
    [DataMember] public long InvoiceNo;
    [DataMember] public string AddressString;
    [DataMember] public SalesOrderTypes OrderType;
    [DataMember] public InvCre InvoiceCreditnote;
    [DataMember] public Decimal Total;
    [DataMember] public Decimal TotalVatEx;
    [DataMember] public Decimal TotalVatIn;
    [DataMember] public Decimal PrePaid;
    [DataMember] public DateTime InvoiceDate;
    [DataMember] public DateTime ShipDate;
    [DataMember] public DateTime PayDate;
    [DataMember] public Guid GuidInv;
    [DataMember] public long SettleID;
}

[DataContract] public class OrderPayment {
    [DataMember] public string meansOfPayment;
    [DataMember] public Decimal amount;
    [DataMember] public string Currency;
    [DataMember] public Decimal amountConverted;
    [DataMember] public int OrderID;
    [DataMember] public string PaymentRef;
    [DataMember] public int ToCapture;
    [DataMember] public string CardNo;
    [DataMember] public string TicketID;
    [DataMember] public string Merchant;
    [DataMember] public string CurrencyDibs;
    [DataMember] public DateTime PaidDate;
    [DataMember] public DateTime CardExpDate;
    [DataMember] public string CardNoMask;
}

[DataContract] public class CreditCardErrors
{
    [DataMember] public int ID;
    [DataMember] public string Merchant;
    [DataMember] public int AddressID;
    [DataMember] public int SaleID;
    [DataMember] public string Currency;
    [DataMember] public Decimal Amount;
    [DataMember] public int OrderID;
    [DataMember] public string TransactionNumber;
    [DataMember] public string HttpBody;
    [DataMember] public int Status;
}



[DataContract] public enum SalesOrderTypes {
    [EnumMember] Quotations,
    [EnumMember] Order,
    [EnumMember] RecurringOrder,
    [EnumMember] Invoice,
    [EnumMember] InvoiceClosed,
    [EnumMember] CreditNote,
    [EnumMember] CreditNoteClosed,
    [EnumMember] InvoiceAndOrders,
    [EnumMember] UnpaidInvoice,
    [EnumMember] UnpaidCreditNote,
    [EnumMember] InvoiceCreditNoteClosed
}

[DataContract] public enum InvCre {
    [EnumMember] Undefined = 0,
    [EnumMember] invoice = 1,
    [EnumMember] CreditNote = -1
}

[DataContract] public class OrderSalesItemChanged
{
    [DataMember] public int SaleID;
    [DataMember] public long OrderNo;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public int Liid;
    [DataMember] public int State;
}

[DataContract] public class SalesDeliveriesProducts
{
    [DataMember] public int SaleID;
    [DataMember] public int ShipTo;
    [DataMember] public string ShipDesc;
    [DataMember] public string ShipToPostalCode;
    [DataMember] public int DeliveryWeekDay;
    [DataMember] public DateTime DeliveryDay;
    [DataMember] public int Liid;
    [DataMember] public long OrderNo;
    [DataMember] public int Class;
    [DataMember] public string ItemID;
    [DataMember] public string ItemDescription;
    [DataMember] public decimal LineQty;
    [DataMember] public int Calendar;
    [DataMember] public string LongDesc;
    [DataMember] public int Frequence;
    [DataMember] public int BlockReason;
    [DataMember] public SalesDeliveryTime[] Deliveries;
    [DataMember] public int SourceSaleID;
    [DataMember] public int SourceLineID;
    [DataMember] public string ReplacementProduct;
    [DataMember] public Boolean NotOnWeb;
    [DataMember] public Boolean NotOnMyPage;
}

[DataContract] public class SalesDeliveryTime
{
    [DataMember] public int Calendar;
    [DataMember] public DateTime DeliveryDay;
    [DataMember] public int DeliveryWeek;
    [DataMember] public int DeliveryWeekDay;
    [DataMember] public int BlockReason;
    [DataMember] public string UserID;
    [DataMember] public int ShipTo;
    [DataMember] public string ShipDesc;
    [DataMember] public string ShipToPostalCode;
}

[DataContract] public class SalesCategorie
{
    [DataMember] public int Category;
    [DataMember] public string CatDescription;
}

[DataContract] public enum SalesReports
{
    [EnumMember] Quotations = 3,
    [EnumMember] Order = 2,
    [EnumMember] Invoice = 1,
    [EnumMember] DeliveryNote = 4,
    [EnumMember] Worksheet = 8,
    [EnumMember] Proforma = 9
}

[DataContract] public class SalesStatLine
{
    [DataMember] public int SaleID;
    [DataMember] public int Liid;
    [DataMember] public long InvoiceNo;
    [DataMember] public long OrderNo;
    [DataMember] public DateTime InvoiceDate;
    [DataMember] public string ItemID;
    [DataMember] public string Unit;
    [DataMember] public string ItemDesc;
    [DataMember] public Decimal Qty;
    [DataMember] public Decimal SalesPrice;
    [DataMember] public Decimal OrderAmount;
    [DataMember] public Decimal Discount;
    [DataMember] public Decimal DiscountProc;
    [DataMember] public string EAN;
    [DataMember] public string AddInformation;
    [DataMember] public string Style;
    [DataMember] public string Batch;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public Decimal vat_perc;
    [DataMember] public Boolean VatIncl;
    [DataMember] public Boolean AllowanceCharge;
    [DataMember] public Decimal LineAmount;
    [DataMember] public Decimal LineVat;
    [DataMember] public Decimal LineVatBase;
    [DataMember] public Decimal LinePrice;
    [DataMember] public string UNSPSC;
    [DataMember] public string AccountingCost;
    [DataMember] public string GroupFi;
}


// purchase

[DataContract] public class OrderLinePurc {
    [DataMember] public int PurcID;
    [DataMember] public int Liid;
    [DataMember] public string ItemID;
    [DataMember] public string Unit;
    [DataMember] public string ItemDesc;
    [DataMember] public Decimal Qty;
    [DataMember] public Decimal CostPrice;
    [DataMember] public Decimal OrderAmount;
    [DataMember] public Decimal DiscountProc;
    [DataMember] public string Batch;
    [DataMember] public string EAN;
    [DataMember] public string AddInformation;
    [DataMember] public string Style;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public Decimal vat_perc;
    [DataMember] public Boolean VatIncl;
    [DataMember] public Boolean AllowanceCharge;
    [DataMember] public Decimal LineAmount;
    [DataMember] public Decimal LineVat;
    [DataMember] public Decimal LineVatBase;
    [DataMember] public string UNSPSC;
    [DataMember] public string AccountingCost;
}

[DataContract] public class OrderPurchase {
    [DataMember] public int PurcID;
    [DataMember] public InvDeb InvoiceDebitnote = InvDeb.invoice;
    [DataMember] public PurchaseOrderTypes OrderType;
    [DataMember] public long OrderNo;
    [DataMember] public long InvoiceNo;
    [DataMember] public long VoucherNo;
    [DataMember] public int BillTo;
    [DataMember] public int ShipTo;
    [DataMember] public string Currency;
    [DataMember] public string Language;
    [DataMember] public DateTime OrderDate;
    [DataMember] public DateTime InvoiceDate;
    [DataMember] public DateTime ShipDate;
    [DataMember] public DateTime PayDate;
    [DataMember] public string TermsOfPayment;
    [DataMember] public OrderLinePurc[] OrderLines;
    [DataMember] public OrderPurcPayment[] OrderPayments;
    [DataMember] public int Category;
    [DataMember] public int seller;
    [DataMember] public string EAN;
    [DataMember] public string ContactPerson;
    [DataMember] public string ExtRef;
    [DataMember] public string IntRef;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public string text_1;
    [DataMember] public string text_2;
    [DataMember] public string text_3;
    [DataMember] public Decimal Total;
    [DataMember] public Decimal TotalVatEx;
    [DataMember] public Decimal TotalVatIn;
    [DataMember] public Decimal TotalVatBasisEx;
    [DataMember] public Decimal TotalVatBasisIn;
    [DataMember] public Decimal TotalInvDiscount;
    [DataMember] public Decimal TotalTaxAmount;
    [DataMember] public Guid GuidInv;
    [DataMember] public string PaymentRef;
    [DataMember] public Decimal AmountRounding;
    [DataMember] public Decimal AmountCharge;
    [DataMember] public Decimal AmountAllowance;
    [DataMember] public Decimal AmountLines;
    [DataMember] public Decimal AmountPaid;
    [DataMember] public DateTime PaidDate;
    [DataMember] public string LocationID;
    [DataMember] public int ContID;
    [DataMember] public long SettleNo;
    [DataMember] public string AccountingCost;
}

[DataContract] public class OrderPurchaseItem {
    [DataMember] public int PurcID;
    [DataMember] public long OrderNo;
    [DataMember] public long VoucherNo;
    [DataMember] public long InvoiceNo;
    [DataMember] public string AddressString;
    [DataMember] public InvDeb InvoiceDebitnote;
    [DataMember] public Decimal Total;
    [DataMember] public Decimal TotalVatEx;
    [DataMember] public Decimal TotalVatIn;
    [DataMember] public DateTime InvoiceDate;
    [DataMember] public DateTime ShipDate;
    [DataMember] public DateTime PayDate;
    [DataMember] public Guid GuidInv;
}

[DataContract] public class OrderPurcPayment {
    [DataMember] public string meansOfPayment;
    [DataMember] public Decimal amount;
    [DataMember] public string Currency;
    [DataMember] public Decimal amountConverted;
    [DataMember] public int OrderID;
    [DataMember] public string PaymentRef;
    [DataMember] public string CardNo;
    [DataMember] public string Merchant;
    [DataMember] public string CurrencyDibs;
    [DataMember] public DateTime PaidDate;
}

[DataContract] public enum InvDeb {
    [EnumMember] Undefined = 0,
    [EnumMember] invoice = 1,
    [EnumMember] DebitNote = -1
}

[DataContract] public enum PurchaseOrderTypes {
    [EnumMember] Undefined,
    [EnumMember] Inquery,
    [EnumMember] Order,
    [EnumMember] Invoice,
    [EnumMember] InvoiceClosed
}

// inventory

[DataContract] public class inventoryItem {
    [DataMember] public string ItemID;
    [DataMember] public string EAN;
    [DataMember] public string ItemDesc;
    [DataMember] public string Model;
    [DataMember] public string unit;
    [DataMember] public Decimal SalesPrice;
    [DataMember] public Decimal Qty;
    [DataMember] public Decimal volume;
    [DataMember] public Decimal Weight;
    [DataMember] public string note;
    [DataMember] public string GroupFi;
    [DataMember] public DateTime DateCreate;
    [DataMember] public DateTime DateUpdate;
    [DataMember] public DateTime DateActive;
    [DataMember] public DateTime DateInactive;
    [DataMember] public inventorySalesPrice[] inventorySalesPrices;
    [DataMember] public inventoryQty[] inventoryQty;
    [DataMember] public string[] Selections;
    [DataMember] public string MetaData;
    [DataMember] public string BrowserTitle;
    [DataMember] public string URL;
    [DataMember] public string MetaText;
    [DataMember] public string IncludeItemID;
    [DataMember] public string SubstituteItem;
    [DataMember] public Decimal CostPriceFixed;
    [DataMember] public string UNSPSC;
    [DataMember] public string Position;
    [DataMember] public Boolean NotOnWeb;
    [DataMember] public Boolean NotOnMyPage;
}

[DataContract] public class SelectionParams
{
    [DataMember] public string Selection;
    [DataMember] public string PriceID;
    [DataMember] public string Currency;
 }

[DataContract] public class inventorySalesPrice
{
    [DataMember] public string PriceID;
    [DataMember] public string Currency;
    [DataMember] public string PriceDesc;
    [DataMember] public decimal Price;
}

[DataContract] public class inventoryQty
{
   [DataMember] public string LocationID;
   [DataMember] public string Style;
   [DataMember] public decimal QtyClosed;
   [DataMember] public decimal QtyOrderPurc;
   [DataMember] public decimal QtyOrderSale;
   [DataMember] public DateTime NextDelivery;
   [DataMember] public string LocationDesc;
}

[DataContract] public class inventoryGroupFi
{
    [DataMember] public string GroupFi;
    [DataMember] public string GroupDesc;
    [DataMember] public Boolean AutoReorder;

}
[DataContract]
public class inventoryContainer
{
    [DataMember] public string ItemID;
    [DataMember] public string Barcode;
    [DataMember] public string Unit;
    [DataMember] public decimal Qty;
    [DataMember] public string Description;
    [DataMember] public string Manufacture;
}
public class inventoryVendor
{
    [DataMember] public string ItemID;
    [DataMember] public string Vendor;
    [DataMember] public string ItemVendor;
    [DataMember] public DateTime DateUpdate;
}

public class inventoryVendors
{
    [DataMember] public string Vendor;
    [DataMember] public string VendorName;
}




[DataContract] public class inventoryItemExtra {
    [DataMember] public string ItemID;
    [DataMember] public string LanguageID;
    [DataMember] public string ItemDescInvoice;
    [DataMember] public string ItemDescWeb;
    [DataMember] public string unitInvoice;
    [DataMember] public string unitWeb;
}
[DataContract] public class inventoryPicture
{
    [DataMember] public int PicID;
    [DataMember] public string Description;
    [DataMember] public bool Default;
    [DataMember] public DateTime InsertDate;
    [DataMember] public string Origin;
    [DataMember] public inventoryPictureList[] Alternatives;
    [DataMember] public byte[] Picture;
    [DataMember] public string ContentType;
}

[DataContract] public class inventoryPictureList
{
    [DataMember] public int PicID;
    [DataMember] public string Description;

}
[DataContract] public class inventoryLocation
{
    [DataMember] public string LocationID;
    [DataMember] public string Description;
}

[DataContract] public class inventorySelection
{
    [DataMember] public string SelectionID;
    [DataMember] public string Selection;
    [DataMember] public string Note;
    [DataMember] public string ParentID;
    [DataMember] public Boolean UseAsParent;
}

[DataContract] public class inventorylistParameters {
    [DataMember] public string FromItemID;
    [DataMember] public string ToItemID;
    [DataMember] public string ItemDesc;
    [DataMember] public string Group;
    [DataMember] public string Selection;
}

[DataContract] public class InventoryVAT {
    [DataMember] public string ItemID;
    [DataMember] public string GroupFI;
    [DataMember] public int Seller;
    [DataMember] public string DebtorGroup;
    [DataMember] public string CreditorGroup;
    [DataMember] public Decimal saVat;
    [DataMember] public Decimal puVat;
}
[DataContract]
public class InventoryExtraLine
{
    [DataMember] public string ItemID;
    [DataMember] public string Description;
    [DataMember] public int LinePos;
    [DataMember] public int Datatype;
    [DataMember] public string Value;
}

[DataContract]
public class InventoryStyle
{
    [DataMember] public string ItemID;
    [DataMember] public int StyleID;
    [DataMember] public string Style;
    [DataMember] public decimal ReorderPoint;
    [DataMember] public decimal ReorderQty;
    [DataMember] public decimal QtyPackages;
    [DataMember] public string Unit;
    [DataMember] public decimal ReductionProc;
    [DataMember] public string StEAN;
    [DataMember] public decimal ReorderMax;
    [DataMember] public int StyleNo;
    [DataMember] public string StyleDesc;
}

[DataContract] public class MenuItem {
    [DataMember] public string SelectionID;
    [DataMember] public string parentID;
    [DataMember] public int ItemLevel;
    [DataMember] public string FolderName;
    [DataMember] public string FolderDesc;
    [DataMember] public string NodeNote;
    [DataMember] public string p1;
    [DataMember] public string p2;
    [DataMember] public string p3;
    [DataMember] public string p4;
    [DataMember] public string p5;
    [DataMember] public Boolean Collection;
    [DataMember] public string MetaData;
    [DataMember] public string BrowserTitle;
    [DataMember] public string URL;
    [DataMember] public string MetaText;
}

// production

[DataContract] public class ProdAssLine
{
    [DataMember] public int ProdID;
    [DataMember] public int Liid;
    [DataMember] public string ItemID;
    [DataMember] public string Description;
    [DataMember] public string Style;
    [DataMember] public string Batch;
    [DataMember] public decimal QtyPerUnit;
    [DataMember] public decimal UnitPrice;
    [DataMember] public string UnitInventory;
    [DataMember] public string UnitAssembly;
    [DataMember] public decimal UnitFactor;

}

[DataContract]
public class ProdOpeLine
{
    [DataMember] public int SaleID;
    [DataMember] public int Liid;
    [DataMember] public string OperID;
    [DataMember] public string Description;
    [DataMember] public decimal QtyPerUnit;
}

[DataContract]
public class ProdAssembly
{
    [DataMember] public int ProdID;
    [DataMember] public int Class;
    [DataMember] public long OrderNo;
    [DataMember] public int Category;
    [DataMember] public int AssemblyNo;
    [DataMember] public string ItemID;
    [DataMember] public string Style;
    [DataMember] public string Batch;
    [DataMember] public string LocationID;
    [DataMember] public string Description;
    [DataMember] public decimal qtyAss;
    [DataMember] public string UnitAss;
    [DataMember] public decimal qtyUnit;
    [DataMember] public decimal QtyEstimate;
    [DataMember] public decimal PriceEstimate;
    [DataMember] public string UserID;
    [DataMember] public DateTime DateCreate;
    [DataMember] public DateTime DateStart;
    [DataMember] public DateTime DateEnd;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public decimal volume;
    [DataMember] public decimal weight;
    [DataMember] public decimal time;
    [DataMember] public decimal waste;
    [DataMember] public string notes;
    [DataMember] public string UnitInventory;
    [DataMember] public ProdAssLine[] ProdLines;
    [DataMember] public ProdOpeLine[] ProdOpeLines;
}

// shop

[DataContract] public class ShopWf {
    [DataMember] public string ShopID;
    [DataMember] public string ShopDescription;
    [DataMember] public string Fee1;
    [DataMember] public string Fee2;
    [DataMember] public string Pricelist_1;
    [DataMember] public string Pricelist_2;
    [DataMember] public int Category;
    [DataMember] public int Category_np;
    [DataMember] public int SellerID;
    [DataMember] public int SellerID_np;
    [DataMember] public int PaymentSolution;
    [DataMember] public Boolean b2blogin;
    [DataMember] public int ProductShortTextUse;
}

[DataContract] public class ShopBasket {
    [DataMember] public Guid BasketGuid;
    [DataMember] public int BasketID;
    [DataMember] public Address BillTo;
    [DataMember] public Address ShipTo;
    [DataMember] public int  AddressID;
    [DataMember] public ShopBasketItem[] BasketItems;
    [DataMember] public Decimal Total;
    [DataMember] public int ContID;
    [DataMember] public string Contactname;
    [DataMember] public int SaleID;
    [DataMember] public string Salesman;
    [DataMember] public string text_1;
    [DataMember] public string text_2;
    [DataMember] public string text_3;
    [DataMember] public DateTime InvoiceDate;
    [DataMember] public DateTime ShipDate;
    [DataMember] public string requisition;
    [DataMember] public int def_addSalesAs;
}

[DataContract] public class ShopBasketItem {
    [DataMember] public int Liid;
    [DataMember] public string ItemID;
    [DataMember] public string ItemDesc;
    [DataMember] public Decimal SalesPrice;
    [DataMember] public Decimal Qty;
    [DataMember] public Decimal FixedQty;
    [DataMember] public string unit;
    [DataMember] public Decimal Amount;
    [DataMember] public string Style;
    [DataMember] public string ext01;
    [DataMember] public string ext02;
    [DataMember] public string ext03;
    [DataMember] public string ext04;
    [DataMember] public string ext05;
    [DataMember] public string ext06;
}

[DataContract] public class AccountItem {
    [DataMember] public string Account;
    [DataMember] public string Description;
    [DataMember] public string VAT;
    [DataMember] public AccountType AccType;
    [DataMember] public string FromAccount;
    [DataMember] public string ToAccount;
    [DataMember] public string AccInitial;
    [DataMember] public string Currency;
    [DataMember] public bool Blocked;
    [DataMember] public bool NotManual;
}

[DataContract] public class Ledger {
    [DataMember] public long LedgerID;
    [DataMember] public string Description;
    [DataMember] public long VoucherFrom;
    [DataMember] public long VoucherTo;
}

[DataContract] public class VoucherIn {     //For elements to fi_vouchers_Imp
    [DataMember] public long Voucher;
    [DataMember] public DateTime EnterDate;
    [DataMember] public long Invoice;
    [DataMember] public string InvoiceNoStr;
    [DataMember] public long SettleNo;
    [DataMember] public string DAccount;
    [DataMember] public string CAccount;
    [DataMember] public string Description;
    [DataMember] public Decimal Amount;
    [DataMember] public Decimal cuAmount;
    [DataMember] public Decimal VatCalc;
    [DataMember] public Boolean VATYn;
    [DataMember] public string Currency;
    [DataMember] public  DateTime Paydate;
    [DataMember] public int Source;
    [DataMember] public int SourceRef;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public string PaymentRef;
    [DataMember] public string AdvisText;
}
[DataContract] public class VoucherOut {   //For elements from fi_Years_item
    [DataMember] public long Voucher;
    [DataMember] public DateTime EnterDate;
    [DataMember] public long Invoice;
    [DataMember] public long SettleNo;
    [DataMember] public long SettleId;
    [DataMember] public string Account;
    [DataMember] public string AdAccount;
    [DataMember] public string Description;
    [DataMember] public Decimal Debet;
    [DataMember] public Decimal cuDebet;
    [DataMember] public Decimal Credit;
    [DataMember] public Decimal cuCredit;
    [DataMember] public Decimal VatCalc;
    [DataMember] public string Currency;
    [DataMember] public DateTime DateOfPayment;
    [DataMember] public int SourceID;
    [DataMember] public int SourceRef;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public string PaymentRef;
    [DataMember] public string AdvisText;
    [DataMember] public int ReminderNo;
}
[DataContract] public class VoucherPictures {
    [DataMember] public byte[] Picture;
    [DataMember] public long Voucher;
    [DataMember] public DateTime EnterDate;
    [DataMember] public string FileName;
    [DataMember] public string ContentType;
    [DataMember] public string Subject;
    [DataMember] public string Description;
}
[DataContract] public class VoucherCriteria {
    [DataMember] public string FromAccount;
    [DataMember] public string ToAccount;
    [DataMember] public DateTime Fromdate;
    [DataMember] public DateTime Todate;
    [DataMember] public int YearID;
    [DataMember] public int FromPeriod;
    [DataMember] public int ToPeriod;
    [DataMember] public VoucherType Type = VoucherType.All;
    [DataMember] public VoucherStatus Status = VoucherStatus.Undefined;
    [DataMember] public long SettleID;
    [DataMember] public int AddressID;
    [DataMember] public int SourceID;
    [DataMember] public int SourceRef;
}

[DataContract] public enum VoucherType
{
    [EnumMember] All = 0,
    [EnumMember] DebtorAndCreditor = 1,
    [EnumMember] Debtor = 2,
    [EnumMember] Creditor = 3,
    [EnumMember] Other = 4
}

[DataContract] public enum VoucherStatus
{
    [EnumMember] Undefined = 0,
    [EnumMember] Settled = 1,
    [EnumMember] Unsettled = 2
}


[DataContract] public class Dimension {
    [DataMember] public int DimNo;
    [DataMember] public string DimID;
    [DataMember] public string DimText;
    [DataMember] public DateTime EnterDate;
    [DataMember] public Boolean Closed;
    [DataMember] public int AddressID;
    [DataMember] public string Notes;
    [DataMember] public Boolean BindingOffer;
    [DataMember] public Decimal completion;
    [DataMember] public Decimal Offer;
    [DataMember] public DateTime CompletionDate;
    [DataMember] public string GroupID;
    [DataMember] public string Parent;
    [DataMember] public DateTime DateUpdate;
}

[DataContract] public class DimensionItem {
    [DataMember] public int DimNo;
    [DataMember] public string DimID;
    [DataMember] public int AddressID;
    [DataMember] public string DimText;
}

[DataContract] public class fi_dimensions_timeitems
{
    [DataMember] public int ItemID;
    [DataMember] public int Class;
    [DataMember] public string employee;
    [DataMember] public DateTime enterDate;
    [DataMember] public string vType;
    [DataMember] public string Description;
    [DataMember] public Decimal Quantity;
    [DataMember] public Decimal Quantity_invoiced;
    [DataMember] public Decimal Price;
    [DataMember] public Decimal CostPrice;
    [DataMember] public string Dim1;
    [DataMember] public string Dim2;
    [DataMember] public string Dim3;
    [DataMember] public string Dim4;
    [DataMember] public int ToInvoice;
    [DataMember] public Decimal HoursPerUnit;
    [DataMember] public string invItemID;
}

[DataContract] public class Dimensions_ClientStatement
{
    [DataMember] public int FiItemID;
    [DataMember] public string Dim3;
    [DataMember] public DateTime EnterDate;
    [DataMember] public String Account;
    [DataMember] public String GroupFI;
    [DataMember] public String ItemID;
    [DataMember] public String ItemDesc;
    [DataMember] public decimal Amount;
    [DataMember] public int Invoiceno;
    [DataMember] public int SaleID;
    [DataMember] public int Reconciled;
    [DataMember] public string OffsetAccount;
    [DataMember] public string  AccBank;
    [DataMember] public string AccClient;
}

[DataContract] public class PaymentMeans                //TODO
{
    [DataMember] public int PaymentMeansCode;
    [DataMember] public string MeansOfPayment;      //kort type i.e. VisaDK
    [DataMember] public string BankAccount;
    [DataMember] public string BankRegno;
    [DataMember] public string PaymentNote;
    [DataMember] public string SWIFT;
    [DataMember] public string IBAN;
    [DataMember] public string CardType;            //FIK/GIK
    [DataMember] public string CreditorID;          //FIK/GIK
    [DataMember] public string PaymentRef;          //FIK/GIK
    [DataMember] public string PaymentChannelCode;
}

[DataContract] public enum AddressType
{
    [EnumMember] Undefined = 0,
    [EnumMember] CustomerAndVendor = 1,
    [EnumMember] Customer = 2,
    [EnumMember] Vendor = 3,
    [EnumMember] Other = 4,
    [EnumMember] LeftOut = 5
}

[DataContract] public class DataTransferDefinition 
{
    [DataMember] public string TransferName;
    [DataMember] public TransferType Type;
    [DataMember] public string Description;
    [DataMember] public string HandlingSP;
    [DataMember] public string PathToFiles;
    [DataMember] public string PathFromWorkStation;
}

[DataContract] public class DataTransfer
{
    [DataMember] public string TransferName;
    [DataMember] public int TransferID;
    [DataMember] public string FileName;
    [DataMember] public DateTime DateOfImport;
    [DataMember] public int LineCount;
}


[DataContract] public enum TransferType
{
    [EnumMember] Undefined = 0,
    [EnumMember] Import = 1,
    [EnumMember] Export = 2
}

[DataContract] public enum AccountType
{
    [EnumMember] Undefined = 0,
    [EnumMember] ProfitAndLoss = 1,
    [EnumMember] Balance = 2,
    [EnumMember] Total = 3,
    [EnumMember] Heading = 4,
    [EnumMember] MainHeading = 5,
    [EnumMember] HeadingOnNewPage = 6,
    [EnumMember] MainHeadingOnNewPage = 7,
    [EnumMember] ProfitLossTotal = 10,
    [EnumMember] ProfitLossTotalInitial = 11,


}

