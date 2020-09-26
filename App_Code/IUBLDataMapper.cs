using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace wfxml
{
    public interface IUBLDataMapper
    {
        string GetInvoiceValue(string xpath);
        string GetInvoiceLineValue(long lineId, string xpath);
        string GetInvoiceAllowanceChargeValue(long allowanceChargeId, string xpath);
        string GetSelfBilledValue(string xpath);
        string GetSelfBilledLineValue(long lineId, string xpath);
        string GetSelfBilledAllowanceChargeValue(long allowanceChargeId, string xpath);
        string GetAddressStatementValue(string xpath);
        string GetAddressStatementLineValue(long lineId, string xpath);
        void PutInvoiceValue(string xpath, string value);
        void PutInvoiceLineValue(int LineId, string xpath, string value);
        void PutAllowanceChargeValue(int LineId, string xpath, string value);
        void PutPaymentMeansValue(int PaymentMeanID, string xpath, string value);
        void PutWrapUp();
        List<long> GetOrderLineIDs();
        List<long> GetAllowanceChargeLineIDs();
        List<long> GetStatementLineIDs();
        long GetOrderLineID(byte What = 1);
        long GetAllowanceChargeLineID(byte What = 1);
        long GetStatementLineID(byte What = 1);
    }
}

