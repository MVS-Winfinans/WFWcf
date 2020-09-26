using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Threading.Tasks;
using System.Web;
using System.ServiceModel;



namespace wfxml
{
    public enum UblDocumentType
    {
        Invoice = 1,
        SelfBilledInvoice = 2,
        CreditNote = 3,
        SelfBilledCreditNote = 4,
        Statement = 5
    }

    public class UBLSerializer
    {
        private XmlDocument _document;
        private const string _idNodeName = "cbc:ID";
        private const string _paymentMeansCodeNodeName = "cbc:PaymentMeansCode";
        private const string _invoiceLineBasePath = "root:Invoice/cac:InvoiceLine";
        private const string _allowanceChargeBasePath = "root:Invoice/cac:AllowanceCharge";
        private const string _CreditNoteLineBasePath = "root:CreditNote/cac:CreditNoteLine";
        private const string _CreditNoteallowanceChargeBasePath = "root:CreditNote/cac:AllowanceCharge";
        private const string _SelfBilledLineBasePath = "root:SelfBilledInvoice/cac:InvoiceLine";
        private const string _SelfBilledallowanceChargeBasePath = "root:SelfBilledInvoice/cac:AllowanceCharge";
        private const string _SelfBilledCreditNoteLineBasePath = "root:SelfBilledCreditNote/cac:CreditNoteLine";
        private const string _SelfBilledCreditNoteallowanceChargeBasePath = "root:SelfBilledCreditNote/cac:AllowanceCharge";
        private const string _StatementLineBasePath = "root:Statement/cac:StatementLine";
        private const string _paymentMeanBasePath = "root:Invoice/cac:PaymentMeans";
        XmlNamespaceManager _nsManager;



        public UBLSerializer(string filename)
        {
            _document = new XmlDocument();
            _document.Load(filename);
            InitializeUBLNameSpace(_document.DocumentElement.Name);
        }

        public UBLSerializer(UblDocumentType docType)
        {
            _document = new XmlDocument();
            //XmlElement elem = _document.CreateElement("root:" + docType.ToString());
            //_document.AppendChild(elem);
            string xmlDoc = GetEmptyUBLDocument(docType.ToString());
            //switch (docType)
            //{
            //    case UblDocumentType.Invoice:
            //        xmlDoc = "<Invoice xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2>\"></Invoice>";
            //        break;
            //    case UblDocumentType.SelfBilledInvoice:
            //        xmlDoc = "<SelfBilledInvoice xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:SelfBilledInvoice-2>\"></SelfBilledInvoice>";
            //        break;
            //    case UblDocumentType.CreditNote:
            //        xmlDoc = "<CreditNote xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2>\"></CreditNote>";
            //        break;
            //}
            _document.LoadXml(xmlDoc);
            InitializeUBLNameSpace(docType.ToString());
            //string ns = _nsManager.DefaultNamespace;
            //string rootname = docType.ToString();
            //XmlNode root = _document;
            //XmlNode rootNode = AppendNode(ref root, rootname);


        }
        public UBLSerializer(UBLDoc doc, UblDocumentType docType)
        {
            _document = new XmlDocument();
            //XmlElement elem = _document.CreateElement("root:" + docType.ToString());
            //_document.AppendChild(elem);
            //string xmlDoc = GetEmptyUBLDocument(docType.ToString());
            //switch (docType)
            //{
            //    case UblDocumentType.Invoice:
            //        xmlDoc = "<Invoice xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2>\"></Invoice>";
            //        break;
            //    case UblDocumentType.SelfBilledInvoice:
            //        xmlDoc = "<SelfBilledInvoice xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:SelfBilledInvoice-2>\"></SelfBilledInvoice>";
            //        break;
            //    case UblDocumentType.CreditNote:
            //        xmlDoc = "<CreditNote xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2>\"></CreditNote>";
            //        break;
            //}
            _document.LoadXml(doc.XmlString);
            InitializeUBLNameSpace(docType.ToString());
            //string ns = _nsManager.DefaultNamespace;
            //string rootname = docType.ToString();
            //XmlNode root = _document;
            //XmlNode rootNode = AppendNode(ref root, rootname);
        }


        public string LoadInvoiceValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            string value;
            foreach (string xpath in xpathCollection)
            {
                value = mapper.GetInvoiceValue(xpath);
                if (!string.IsNullOrEmpty(value)) AssignNode(xpath, value);
            }
            return _document.OuterXml;
        }


        ////pre validation of XML obsolete - now validated in intermediate validation - see UBLmapper.Datacheck()
        //public bool ValidateInvoice(IUBLDataMapper mapper, string[] invoiceXPathCollection, out string errorString) 
        //{
        //    errorString = "";
           
        //    foreach (string xpath in invoiceXPathCollection)
        //    {
        //        XmlNode node = _document.SelectSingleNode(xpath, _nsManager);
        //        if (node == null || string.IsNullOrEmpty(node.InnerText))
        //        {
        //            errorString = " Value not found: " + xpath;
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        public string LoadInvoiceLineValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            List<long> ids = mapper.GetOrderLineIDs();

            foreach (long id in ids)
            {
                string idText = id.ToString();
                foreach (string xpath in xpathCollection)
                {
                    string value = mapper.GetInvoiceLineValue(id, xpath);
                    if (value != "")
                    {
                        XmlNode node = InsertNode(_invoiceLineBasePath, idText, xpath, value);
                    }
                }
            }
            return _document.OuterXml;
        }
        public string LoadCreditNoteLineValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            List<long> ids = mapper.GetOrderLineIDs();

            foreach (long id in ids)
            {
                string idText = id.ToString();
                foreach (string xpath in xpathCollection)
                {
                    string value = mapper.GetInvoiceLineValue(id, xpath);
                    if (value != "")
                    {
                        XmlNode node = InsertNode(_CreditNoteLineBasePath, idText, xpath, value);
                    }
                }
            }
            return _document.OuterXml;
        }

        public string LoadAllowanceChargeValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            List<long> ids = mapper.GetAllowanceChargeLineIDs();

            foreach (long id in ids)
            {
                //string idText = mapper.GetInvoiceAllowanceChargeValue(id, "cbc:ID");
                string idText = id.ToString();
                foreach (string xpath in xpathCollection)
                {
                    string value = mapper.GetInvoiceAllowanceChargeValue(id, xpath);
                    if (value != "")
                    {
                        XmlNode node = InsertNode(_allowanceChargeBasePath, idText, xpath, value);
                    }
                }

            }
            return _document.OuterXml;
        }

        public string LoadSelfBilledValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            string value;
            foreach (string xpath in xpathCollection)
            {
                value = mapper.GetSelfBilledValue(xpath);
                if (!string.IsNullOrEmpty(value)) AssignNode(xpath, value);
            }
            return _document.OuterXml;
        }

        public string LoadSelfBilledLineValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            List<long> ids = mapper.GetOrderLineIDs();

            foreach (long id in ids)
            {
                string idText = id.ToString();
                foreach (string xpath in xpathCollection)
                {
                    string value = mapper.GetSelfBilledLineValue(id, xpath);
                    if (value != "")
                    {
                        XmlNode node = InsertNode(_SelfBilledLineBasePath, idText, xpath, value);
                    }
                }
            }
            return _document.OuterXml;
        }

        public string LoadSelfBilledAllowanceChargeValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            List<long> ids = mapper.GetAllowanceChargeLineIDs();

            foreach (long id in ids)
            {
                //string idText = mapper.GetInvoiceAllowanceChargeValue(id, "cbc:ID");
                string idText = id.ToString();
                foreach (string xpath in xpathCollection)
                {
                    string value = mapper.GetSelfBilledAllowanceChargeValue(id, xpath);
                    if (value != "")
                    {
                        XmlNode node = InsertNode(_SelfBilledallowanceChargeBasePath, idText, xpath, value);
                    }
                }
            }
            return _document.OuterXml;
        }

        public string LoadCreditNoteAllowanceChargeValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            List<long> ids = mapper.GetAllowanceChargeLineIDs();

            foreach (long id in ids)
            {
                //string idText = mapper.GetInvoiceAllowanceChargeValue(id, "cbc:ID");
                string idText = id.ToString();
                foreach (string xpath in xpathCollection)
                {
                    string value = mapper.GetInvoiceAllowanceChargeValue(id, xpath);
                    if (value != "")
                    {
                        XmlNode node = InsertNode(_CreditNoteallowanceChargeBasePath, idText, xpath, value);
                    }
                }

            }
            return _document.OuterXml;
        }
        public string LoadStatementValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            string value;
            foreach (string xpath in xpathCollection)
            {
                value = mapper.GetAddressStatementValue(xpath);
                if (!string.IsNullOrEmpty(value)) AssignNode(xpath, value);
            }
            return _document.OuterXml;

        }
        public string LoadStatementLineValueByXPath(string[] xpathCollection, IUBLDataMapper mapper)
        {
            List<long> ids = mapper.GetStatementLineIDs();

            foreach (long id in ids)
            {
                string idText = id.ToString();
                foreach (string xpath in xpathCollection)
                {
                    string value = mapper.GetAddressStatementLineValue(id, xpath);
                    if (value != "")
                    {
                        XmlNode node = InsertNode(_StatementLineBasePath, idText, xpath, value);
                    }
                }
            }
            return _document.OuterXml;
        }
        private string GetEmptyUBLDocument(string documentName)
        {
            string xmlString; // = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "\n";
            xmlString = "<" + documentName;
            xmlString += " xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\"";
            xmlString += " xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\" ";
            xmlString += " xmlns:ccts=\"urn:oasis:names:specification:ubl:schema:xsd:CoreComponentParameters-2\" xmlns:sdt=\"urn:oasis:names:specification:ubl:schema:xsd:SpecializedDatatypes-2\"";
            xmlString += " xmlns:udt=\"urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
            switch (documentName)
            {
                case "Invoice": 
                    xmlString += " xsi:schemaLocation=\"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 UBL-Invoice-2.0.xsd\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:" + documentName + "-2\">" ;
                    break;
                case "CreditNote":
                    xmlString += " xsi:schemaLocation=\"urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2 UBL-CreditNote-2.0.xsd\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:" + documentName + "-2\">";
                    break;
                case "SelfBilledInvoice":
                    xmlString += " xsi:schemaLocation=\"urn:oasis:names:specification:ubl:schema:xsd: SelfBilledInvoice-2 UBL-SelfBilledInvoice-2.0.xsd\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:" + documentName + "-2\">";
                    break;
                case "Statement":
                    xmlString += " xsi:schemaLocation=\"urn:oasis:names:specification:ubl:schema:xsd: Statement-2 UBL-Statement-2.0.xsd\" xmlns=\"urn:oasis:names:specification:ubl:schema:xsd:" + documentName + "-2\">";
                    break;
            }
            xmlString += "</" + documentName + ">";
            return xmlString;
        }

        private void InitializeUBLNameSpace(string documentName)
        {
            _nsManager = new XmlNamespaceManager(_document.NameTable);
            _nsManager.AddNamespace("udt", "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");
            _nsManager.AddNamespace("sdt", "urn:oasis:names:specification:ubl:schema:xsd:SpecializedDatatypes-2");
            _nsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            _nsManager.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            _nsManager.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            _nsManager.AddNamespace("ccts", "urn:oasis:names:specification:ubl:schema:xsd:CoreComponentParameters-2");
            switch (documentName)
            {
                case "Invoice":
                    _nsManager.AddNamespace("root", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
                    break;
                case "SelfBilledInvoice":
                    _nsManager.AddNamespace("root", "urn:oasis:names:specification:ubl:schema:xsd:SelfBilledInvoice-2");
                    break;
                case "CreditNote":
                    _nsManager.AddNamespace("root", "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");
                    break;
                case "Statement":
                    _nsManager.AddNamespace("root", "urn:oasis:names:specification:ubl:schema:xsd:Statement-2");
                    break;
            }
        }

        public List<string> GetSubXPathList(String xpath)
        {
            string[] parts = xpath.Split('/');
            if (parts.Length < 2) return null;
            List<string> pathList = new List<string>();
            string path = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                pathList.Add(path);
                path = path + "/" + parts[i];
            }
            return pathList;
        }

        public string GetXML()
        {
            return _document.OuterXml;
        }



        public XmlNode InsertNode(string basePath, string id, string nodePath, string value)
        {
            string[] parts = basePath.Split('/');
            if (parts.Length < 2) throw new FaultException(string.Concat("Full path must be specified for basePath"), new FaultCode(ErrCode.GeneralUBLError.ToString()));
            //throw new ApplicationException("Full path must be specified for basePath");
            List<string> pathList = GetSubXPathList(basePath);

            //XmlNode baseNode = AssignNode(basePath, "");
            string rootPath = pathList[pathList.Count - 1];
            XmlNode rootNode = _document.SelectSingleNode(rootPath, _nsManager);
            if (rootNode == null) rootNode = AssignNode(rootPath, "");
            string nodeName = parts[parts.Length - 1];
            //find out if a node with the given id allready exists
            XmlNode newNode = GetNodeById(rootNode, nodeName, id);
            if (newNode == null)
            {
                //new inserted node eg. another invoiceLine
                newNode = AppendNode(ref rootNode, nodeName);
                XmlNode idNode = AppendNode(ref newNode, _idNodeName);
                idNode.InnerText = id;
            }
            XmlNode node = AssignNode(newNode, nodePath, value);
            return node;
        }
        //get a node with a given name that has a given id
        //
        private XmlNode GetNodeById(XmlNode baseNode, string nodeName, string id)
        {
            foreach (XmlNode child in baseNode.ChildNodes)
            {
                if (child.Name == nodeName)
                {
                    foreach (XmlNode idNode in child.ChildNodes)
                    {
                        if (idNode.Name == _idNodeName && idNode.InnerXml == id) return child;
                    }
                }
            }
            return null;
        }


        public XmlNode AssignNode(XmlNode baseNode, String xpath, string value)
        {

            XmlNode node;
            string[] parts = xpath.Split('/');
            string attribute = "";
            int attLen = 0;
            if (parts.Length > 0)
                if (parts[parts.Length - 1].StartsWith("@"))
                {
                    attribute = parts[parts.Length - 1].Substring(1); //not the '@' sign
                    attLen = 1;
                }

            node = baseNode.SelectSingleNode(xpath, _nsManager);

            if (node != null)
            {
                if (value != "") node.InnerXml = value;
                return node;
            }

            //the node did not exist return a new list find the highest path that exists
            List<string> pathList = new List<string>();

            string path = parts[0];
            int i = 0;
            do
            {
                pathList.Add(path);
                i++;
                if (i < parts.Length) path = path + "/" + parts[i];
            }
            while (i < parts.Length);

            int count = pathList.Count;

            while (node == null && count > 0)
            {

                count--;
                node = baseNode.SelectSingleNode(pathList[count], _nsManager);
            }
            int adjustForBaseNode = 1;
            if (node == null)
            {
                node = baseNode;
                adjustForBaseNode = 0;
            }

            //now build up nodes until the xpath (not the attribute)
            while (count < pathList.Count - attLen - adjustForBaseNode)
            {
                node = AppendNode(ref node, parts[count + adjustForBaseNode]);
                count++;
            }

            if (attribute == "")
            {
                if (value != "") node.InnerXml = HttpUtility.HtmlEncode(value);
            }
            else
            {
                XmlAttribute att = node.Attributes[attribute];
                if (att == null)
                {
                    att = node.Attributes.Append(_document.CreateAttribute(attribute));
                }
                att.Value = value;
            }
            return node;

        }

        public XmlNode AssignNode(String xpath, string value)
        {
            XmlNode node;
            string[] parts = xpath.Split('/');
            string attribute = "";
            int attLen = 0;
            if (parts.Length > 0)
                if (parts[parts.Length - 1].StartsWith("@"))
                {
                    attribute = parts[parts.Length - 1].Substring(1); //not the '@' sign
                    attLen = 1;
                }
            node = _document.SelectSingleNode(xpath, _nsManager);
            if (node != null)  {
                if (value != "") node.InnerXml = value;
                return node;
            }

            //the node did not exist return a new list find the highest path that exists

            List<string> pathList = new List<string>();
            string path = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                pathList.Add(path);
                path = path + "/" + parts[i];
            }

            int count = pathList.Count - 1;

            while (node == null && count >= 0)
            {
                node = _document.SelectSingleNode(pathList[count], _nsManager);
                count--;
            }

            //now build up nodes until the xpath (not the attribute)
            count += 2;
            while (count <= pathList.Count - attLen)
            {
                node = AppendNode(ref node, parts[count]);
                count++;
            }

            if (attribute == "")
            {
                if (value != "") node.InnerXml = HttpUtility.HtmlEncode(value);
            }
            else
            {
                XmlAttribute att = node.Attributes[attribute];
                if (att == null)
                {
                    att = node.Attributes.Append(_document.CreateAttribute(attribute));
                }
                att.Value = value;
            }
            return node;
        }

        private XmlNode AppendNode(ref XmlNode node, string name)
        {
            string[] nameParts = name.Split(':');

            string uri = _nsManager.LookupNamespace(nameParts[0]);
            XmlNode newNode;
            if (nameParts.Length > 1)
                newNode = _document.CreateElement(nameParts[0], nameParts[1], uri);
            else
                newNode = _document.CreateElement(nameParts[0], "");
            node.AppendChild(newNode);
            return newNode;
        }

        public string PutInvoice(IUBLDataMapper mapper)
        {
            XmlNode TopNode = _document.DocumentElement;
            PutInvoiceNode(mapper, TopNode);
            mapper.PutWrapUp();  //the temp orderlines in ordersales
            return "ok";
        }
        
        private string GetXpath(XmlNode node)
        {
            if (node == _document.DocumentElement) return "root:" + _document.DocumentElement.Name;
            return GetXpath(node.ParentNode) + "/" +  node.Name;
        }

        private bool IsSibling(XmlNode potentialParent, XmlNode node)
        {
            if (potentialParent == node) return true;
            if (node == _document.DocumentElement) return false;
            if (node.ParentNode == null) return false;

            return IsSibling(potentialParent, node.ParentNode);

        }

        private int GetIdFromNode(string basePath, XmlNode node)
        {
            XmlNodeList nodes = _document.SelectNodes(basePath, _nsManager);
            int count = 0;

            foreach (XmlNode n in nodes)
            {
                count++;
                //find the id node
                if (IsSibling( n, node))
                {
                    //find the id node
                    foreach (XmlNode child in n.ChildNodes)
                    {
                        int id;
                        //return the id node if found and it contains a number
                        if (child.Name == _idNodeName && int.TryParse(child.InnerText, out id)) return id;
                        if (child.Name == _idNodeName) return count;
                    }
                }
                //no usefull id were found return the count instead
                
            }

            return 0;
        }

        private string GetPaymentMeansCodeFromNode(string basePath, XmlNode node)
        {
            XmlNodeList nodes = _document.SelectNodes(basePath, _nsManager);

            foreach (XmlNode n in nodes)
            {
                //find the paymentmeanscode node
                if (IsSibling( n, node))
                {
                    //find the paymentmeanscode node
                    foreach (XmlNode child in n.ChildNodes)
                    {
                       
                        //return the paymentmeanscode node if found and it contains a code
                        if (child.Name == _paymentMeansCodeNodeName ) return child.InnerText;
                    }
                }
            }

            return "";
        }

        private void PutInvoiceAttributes(IUBLDataMapper mapper, XmlNode node, string xpath)
        {
            foreach (XmlAttribute att in node.Attributes)
            {
                string attXpath = xpath + "/@" + att.Name;
                mapper.PutInvoiceValue(attXpath, att.Value);
            }

        }

        private void PutInvoiceNode(IUBLDataMapper mapper, XmlNode node)
        {
            if (node.NamespaceURI == "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2" || node == _document.DocumentElement)
            {
                foreach (XmlNode n in node.ChildNodes) 
                    PutInvoiceNode(mapper,n);
            }
            else
            {
                string xpath = GetXpath(node);
                string value = node.InnerText;

                if (xpath.Contains(_invoiceLineBasePath))
                {
                    //put the invoiceLine value
                    string lineXpath = xpath.Substring(_invoiceLineBasePath.Length);
                    int id = GetIdFromNode(_invoiceLineBasePath, node);
                    mapper.PutInvoiceLineValue(id, lineXpath, value);

                    foreach (XmlAttribute att in node.Attributes)
                    {
                        string attXpath = lineXpath + "/@" + att.Name;
                        mapper.PutInvoiceLineValue(id, attXpath, att.Value);
                    }
                }
                else if (xpath.Contains(_allowanceChargeBasePath))
                {
                    //put the invoiceLine value
                    string allowanceXpath = xpath.Substring(_allowanceChargeBasePath.Length);
                    int id = GetIdFromNode(_allowanceChargeBasePath, node);
                    mapper.PutAllowanceChargeValue(id, allowanceXpath, value);

                    foreach (XmlAttribute att in node.Attributes)
                    {
                        string attXpath = allowanceXpath + "/@" + att.Name;
                        mapper.PutAllowanceChargeValue(id, attXpath, att.Value);
                    }
                }
                else if (xpath.Contains(_paymentMeanBasePath))
                {
                    int id = GetIdFromNode(_paymentMeanBasePath, node);
                    string PaymentMeansXpath = xpath.Substring(_paymentMeanBasePath.Length);
                    //string paymentMeansCode = GetPaymentMeansCodeFromNode(_paymentMeanBasePath, node);  //kan måske bruges i stedet for id, hvis denne skulle være den samme for alle paymentmeans
                    mapper.PutPaymentMeansValue(id, PaymentMeansXpath, value);
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        string attXpath = PaymentMeansXpath + "/@" + att.Name;
                        mapper.PutPaymentMeansValue(id, attXpath, att.Value);
                    }

                }
                else
                {
                    mapper.PutInvoiceValue(xpath, value);

                    foreach (XmlAttribute att in node.Attributes)
                    {
                        string attXpath = xpath + "/@" + att.Name;
                        mapper.PutInvoiceValue(attXpath, att.Value);
                    }
                }
            }
        }

    }
}

