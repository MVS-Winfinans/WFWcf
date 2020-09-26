using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
/// <summary>
/// Summary description for UBLXPath
/// </summary>
namespace wfxml
{
    public class UBLXPath
    {
        private string[] MyHead1XPaths;
        private string[] MyHead2XPaths;
        private string[] MyAllowXPaths;
        private string[] MyLineXPaths;
        private string connstr;
        private enum DocSet
        {   Head1 = 1,
            Head2 = 2,
            Allow = 3,
            Line = 4 
        }

        //public UBLXPath(DocType doctype)    //obsolete. Use the overloaded constructor UBLXPath(ref DBUser DBUser, DocType doctype)
        //{
        //    if (doctype == DocType.Invoice)
        //    {
        //        LoadHeaderPaths1();
        //        LoadHeaderPaths2();
        //        LoadAllowancePaths();
        //        LoadLinePaths();
        //    }
        //    if (doctype == DocType.CreditNote)
        //    {
        //        LoadCreditNoteHeaderPaths1();
        //        LoadCreditNoteHeaderPaths2();
        //        LoadCreditNoteAllowancePaths();
        //        LoadCreditNoteLinePaths();
        //    }
        //}

        public UBLXPath(DBUser DBUser, DocType doctype)
        {
            wfws.LookUp defaults = new wfws.LookUp();
            if (string.IsNullOrEmpty(DBUser.ConnectionString))
            {
                var wfconn = new wfws.ConnectLocal(DBUser);
                connstr = wfconn.ConnectionGetByGuid_02(ref DBUser);
            }
            else
            {
                connstr = DBUser.ConnectionString;
            }

            if (GetUserPathSet(ref MyHead1XPaths, DBUser, doctype, DocSet.Head1) == 0)
            {
                MyHead1XPaths= defaults.GetDefaultPathSet(doctype, (int)DocSet.Head1);
            }
            if (GetUserPathSet(ref MyHead2XPaths, DBUser, doctype, DocSet.Head2) == 0)
            {
                MyHead2XPaths = defaults.GetDefaultPathSet(doctype, (int)DocSet.Head2);
            }
            if (GetUserPathSet(ref MyAllowXPaths, DBUser, doctype, DocSet.Allow) == 0)
            {
                MyAllowXPaths = defaults.GetDefaultPathSet(doctype, (int)DocSet.Allow);
            }
            if (GetUserPathSet(ref MyLineXPaths, DBUser, doctype, DocSet.Line) == 0)
            {
                MyLineXPaths = defaults.GetDefaultPathSet(doctype, (int)DocSet.Line);
            }


        }
       
        private int GetUserPathSet(ref string[] answer, DBUser DBUser, DocType doctype, DocSet Set)
        {
            int pos = 0;
            SqlConnection conn = new SqlConnection(connstr);
            string mysql = "SELECT ubl_XPaths_lines.line FROM ubl_XPaths INNER JOIN ubl_XPaths_lines ON ubl_XPaths.ID = ubl_XPaths_lines.XPathID WHERE (ubl_XPaths.CompID = @CompID) AND (ubl_XPaths.XPathType = @Type) AND (ubl_XPaths.XPathSet = @Set) ORDER BY ubl_XPaths_lines.pos";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = DBUser.CompID;
            comm.Parameters.Add("@Type", SqlDbType.Int).Value = doctype;
            comm.Parameters.Add("@Set", SqlDbType.Int).Value = Set;
            try
            {
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    answer[pos] = myr["line"].ToString();
                    pos++;
                }
            }
            catch (Exception e)
            { };      //any fails yields fetching of defaults
            conn.Close();
            return pos;
        }

        public string[] Head1XPaths()
        {
            return MyHead1XPaths;
        }
        public string[] Head2XPaths()
        {
            return MyHead2XPaths;
        }
        public string[] AllowXPaths()
        {
            return MyAllowXPaths;
        }
        public string[] LineXPaths()
        {
            return MyLineXPaths;
        }

//        public string[] MyHeadValidationPaths()         //pre validation of XML obsolete - now validated in intermediate validation - see UBLmapper.Datacheck()
//        {
//            return new string[]{                 
//                "root:Invoice/cbc:ID",
//                "root:Invoice/cbc:IssueDate",
//                "root:Invoice/cbc:DocumentCurrencyCode",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID",
////                "root:Invoice/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name",
////                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode"  //,
////                "root:Invoice/cac:PaymentMeans/cbc:PaymentDueDate"
//               };
//        }



        //obsolete - lines maintained in wf_lookup database

//        private void LoadHeaderPaths1()
//        {
//            //TODO check for customized xpath scheme in WF otherwise load default
//            //quickstart - loads Lauritz specific xpaths
//            MyHead1XPaths = new string[]{
//                "root:Invoice/cbc:UBLVersionID",
//                "root:Invoice/cbc:CustomizationID",
//                "root:Invoice/cbc:ProfileID",
//                "root:Invoice/cbc:ProfileID/@schemeID",
//                "root:Invoice/cbc:ProfileID/@schemeAgencyID",
//                "root:Invoice/cbc:ID",
//                "root:Invoice/cbc:ID/@schemeID",
//                "root:Invoice/cbc:UUID",
//                "root:Invoice/cbc:IssueDate",
//                "root:Invoice/cbc:IssueTime",
//                "root:Invoice/cbc:DocumentCurrencyCode",
//                "root:Invoice/cbc:AccountingCost",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID",
//                "root:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID",
//                "root:Invoice/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Department",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeAgencyID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ID",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FirstName",
//                "root:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FamilyName",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyName/cbc:Name",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:Contact/cbc:ID",
//                "root:Invoice/cac:SellerSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail",
//                "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode",
//                "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode/@listID",
//                "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:AddressFormatCode/@listAgencyID",
//                "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:StreetName",
//                "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:CityName",
//                "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cbc:PostalZone",
//                "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:IdentificationCode",
//                "root:Invoice/cac:Delivery/cac:DeliveryLocation/cac:Address/cac:Country/cbc:Name",
//                "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyIdentification/cbc:ID",
//                "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyIdentification/cbc:ID/@schemeID",
//                "root:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyName/cbc:Name",
//                                    // TODO This section hardcoded to bank to bank only
//                "root:Invoice/cac:PaymentMeans/cbc:ID",
//                "root:Invoice/cac:PaymentMeans/cbc:PaymentMeansCode",
//                "root:Invoice/cac:PaymentMeans/cbc:PaymentDueDate",
//                "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode",
//                "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listID",
//                "root:Invoice/cac:PaymentMeans/cbc:PaymentChannelCode/@listAgencyID",
//                "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID",
//                "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote",
//                "root:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID",
//                "root:Invoice/cac:PaymentTerms/cbc:ID",
//                "root:Invoice/cac:PaymentTerms/cbc:PaymentMeansID",
//                "root:Invoice/cac:PaymentTerms/cbc:Amount",
//                "root:Invoice/cac:PaymentTerms/cbc:Amount/@currencyID",
//                                // hertil
//                "root:Invoice/cac:PrepaidPayment/cbc:PaidAmount",
//                "root:Invoice/cac:PrepaidPayment/cbc:PaidAmount/@currencyID",
//                "root:Invoice/cac:PrepaidPayment/cbc:PaidDate"
//                };

//        }

//               private void LoadHeaderPaths2()
//        {
//            //TODO check for customized xpath scheme in WF otherwise load default
//            //quickstart - loads Lauritz specific xpaths
//            MyHead2XPaths = new string[]{
//                "root:Invoice/cac:TaxTotal/cbc:TaxAmount",
//                "root:Invoice/cac:TaxTotal/cbc:TaxAmount/@currencyID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID",
//                "root:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount/@currencyID",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount/@currencyID",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount",
//                "root:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID"
//                };
//        }
//        private void LoadAllowancePaths()
//        {
//            //TODO check for customized xpath scheme in WF otherwise load default
//            //quickstart - loads Lauritz specific xpaths
//            MyAllowXPaths = new string[] {
//                "cbc:ID",
//                "cbc:ChargeIndicator",
//                //"cbc:AllowanceChargeReasonCode/@listID",
//                //"cbc:AllowanceChargeReasonCode/@listAgencyID",
//                "cbc:AllowanceChargeReasonCode",
//                "cbc:AllowanceChargeReason",
//                "cbc:Amount/@currencyID",
//                "cbc:Amount",
//                "cac:TaxCategory/cbc:ID/@schemeID",
//                "cac:TaxCategory/cbc:ID/@schemeAgencyID",
//                "cac:TaxCategory/cbc:ID",
//                "cac:TaxCategory/cbc:Percent",
//                "cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID",
//                "cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID",
//                "cac:TaxCategory/cac:TaxScheme/cbc:ID",
//                "cac:TaxCategory/cac:TaxScheme/cbc:Name"
//                };

//        }
//        private void LoadLinePaths()
//        {
//            //TODO check for customized xpath scheme in WF otherwise load default
//            //quickstart - loads Lauritz specific xpaths
//            MyLineXPaths = new string[] {
//                "cbc:InvoicedQuantity",
//                "cbc:InvoicedQuantity/@unitCode",
//                "cbc:LineExtensionAmount",
//                "cbc:LineExtensionAmount/@currencyID",
//                "cbc:AccountingCost",
//                "cac:TaxTotal/cbc:TaxAmount",
//                "cac:TaxTotal/cbc:TaxAmount/@currencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount",
//                "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount",
//                "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name",
//                "cac:Item/cbc:Description",
//                "cac:Item/cbc:Name",
//                "cac:Item/cac:SellersItemIdentification/cbc:ID",
//                "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode",
//                "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listID",
//                "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listAgencyID",
//                "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listVersionID",
//                "cac:Price/cbc:PriceAmount",
//                "cac:Price/cbc:PriceAmount/@currencyID",
//            };
//        }

//        private void LoadCreditNoteHeaderPaths1()
//        {
//            //TODO check for customized xpath scheme in WF otherwise load default
//            //quickstart - loads Lauritz specific xpaths
//            MyHead1XPaths = new string[]{
//                "root:CreditNote/cbc:UBLVersionID",
//                "root:CreditNote/cbc:CustomizationID",
//                "root:CreditNote/cbc:ProfileID",
//                "root:CreditNote/cbc:ProfileID/@schemeID",
//                "root:CreditNote/cbc:ProfileID/@schemeAgencyID",
//                "root:CreditNote/cbc:ID",
//                "root:CreditNote/cbc:ID/@schemeID",
//                "root:CreditNote/cbc:UUID",
//                "root:CreditNote/cbc:IssueDate",
//                "root:CreditNote/cbc:IssueTime",
//                "root:CreditNote/cbc:DocumentCurrencyCode",
//                "root:CreditNote/cac:BillingReference/cac:InvoiceDocumentReference/cbc:ID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cbc:EndpointID/@schemeID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:InhouseMail",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID",
//                "root:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID",
//                "root:CreditNote/cac:AccountingCustomerParty/cbc:SupplierAssignedAccountID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cbc:EndpointID/@schemeID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID/@schemeID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyName/cbc:Name",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Language/cbc:ID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listAgencyID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:AddressFormatCode/@listID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:InhouseMail",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Department",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeAgencyID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:ID/@schemeID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ID",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FirstName",
//                "root:CreditNote/cac:AccountingCustomerParty/cac:Party/cac:Person/cbc:FamilyName",
//                };

//        }

//        private void LoadCreditNoteHeaderPaths2()
//        {
//            //TODO check for customized xpath scheme in WF otherwise load default
//            //quickstart - loads Lauritz specific xpaths
//            MyHead2XPaths = new string[]{
//                "root:CreditNote/cac:TaxTotal/cbc:TaxAmount",
//                "root:CreditNote/cac:TaxTotal/cbc:TaxAmount/@currencyID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID",
//                "root:CreditNote/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount/@currencyID",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableRoundingAmount/@currencyID",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount",
//                "root:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID"
//                };
//        }
//        private void LoadCreditNoteAllowancePaths()
//        {
//            //TODO check for customized xpath scheme in WF otherwise load default
//            //quickstart - loads Lauritz specific xpaths
//            MyAllowXPaths = new string[] {
//                "cbc:ID",
//                "cbc:ChargeIndicator",
//                //"cbc:AllowanceChargeReasonCode/@listID",
//                //"cbc:AllowanceChargeReasonCode/@listAgencyID",
//                "cbc:AllowanceChargeReasonCode",
//                "cbc:AllowanceChargeReason",
//                "cbc:Amount/@currencyID",
//                "cbc:Amount",
//                "cac:TaxCategory/cbc:ID/@schemeID",
//                "cac:TaxCategory/cbc:ID/@schemeAgencyID",
//                "cac:TaxCategory/cbc:ID",
//                "cac:TaxCategory/cbc:Percent",
//                "cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID",
//                "cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID",
//                "cac:TaxCategory/cac:TaxScheme/cbc:ID",
//                "cac:TaxCategory/cac:TaxScheme/cbc:Name"
//                };

//        }
//        private void LoadCreditNoteLinePaths()
//        {
//            //TODO check for customized xpath scheme in WF otherwise load default
//            //quickstart - loads Lauritz specific xpaths
//            MyLineXPaths = new string[] {
//                "cbc:CreditedQuantity",
//                "cbc:CreditedQuantity/@unitCode",
//                "cbc:LineExtensionAmount",
//                "cbc:LineExtensionAmount/@currencyID",
//                "cac:TaxTotal/cbc:TaxAmount",
//                "cac:TaxTotal/cbc:TaxAmount/@currencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount",
//                "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxableAmount/@currencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount",
//                "cac:TaxTotal/cac:TaxSubtotal/cbc:TaxAmount/@currencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID/@schemeAgencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID/@schemeAgencyID",
//                "cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:Name",
//                "cac:Item/cbc:Description",
//                "cac:Item/cbc:Name",
//                "cac:Item/cac:SellersItemIdentification/cbc:ID",
//                "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode",
//                "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listID",
//                "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listAgencyID",
//                "cac:Item/cac:CommodityClassification/cbc:ItemClassificationCode/@listVersionID",
//                "cac:Price/cbc:PriceAmount",
//                "cac:Price/cbc:PriceAmount/@currencyID",
//            };
//        }

    
}
}