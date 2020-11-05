using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace wfws
{
    public class datatransfers : web
    {
        private DataTransfer activetransfer;
        private DataTransferDefinition activedefinition = new DataTransferDefinition();
        private string username = "";
        public DataTransfer ActiveTransfer
        {
            get
            {
                return activetransfer;
            }
            set
            {
                if (value.TransferName.Length > 1) activetransfer = value;
            }

        }

        public DataTransferDefinition ActiveDefinition
        {
            get
            {
                return activedefinition;
            }
            set
            {
                if (value.TransferName.Length > 1) activedefinition = value;
            }
        }

        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                if (value.Length > 1) username = value;
            }
        }

        public datatransfers(ref DBUser DBUser)
            : base(ref DBUser)
        {
            //Username = "{system}";
        }

        public DataTransferDefinition UseDefinition(string DefinitionName)
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT * FROM pa_DataTransferDefinitions WHERE (CompID = @CompID) and SystemID = @DefName";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@DefName", SqlDbType.NVarChar, 20).Value = DefinitionName;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                ActiveDefinition.TransferName = myr["SystemID"].ToString();
                ActiveDefinition.Type = ((bool)myr["ImportTransfer"]) ? TransferType.Import : TransferType.Export;
                ActiveDefinition.Description = myr["Description"].ToString();
                ActiveDefinition.HandlingSP = myr["HandlingSP"].ToString();
                ActiveDefinition.PathFromWorkStation = myr["FromWorkStation"].ToString();
                ActiveDefinition.PathToFiles = myr["PathToFiles"].ToString();
            }
            else {
                ActiveDefinition.TransferName = "";
                ActiveDefinition.Type = TransferType.Undefined;
                ActiveDefinition.Description = "";
                ActiveDefinition.HandlingSP = "";
                ActiveDefinition.PathFromWorkStation = "";
                ActiveDefinition.PathToFiles = "";
            }
            conn.Close();
            return ActiveDefinition;
        }

        public void SetTransferID(int TransID)
        {
            DateTime DateOfImport;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT TransID, Description, filename, Processed, ImportedRows FROM pa_DataTransferHeader WHERE (BelongsToCompID = @CompID) and TransID = @TransID";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@TransID", SqlDbType.Int).Value = TransID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read()) {
                DateTime.TryParse(myr["Processed"].ToString(), out DateOfImport);
                ActiveTransfer.TransferID = (int)myr["TransID"];
                ActiveTransfer.TransferName = myr["Description"].ToString();
                ActiveTransfer.FileName = myr["filename"].ToString();
                ActiveTransfer.DateOfImport = DateOfImport;
                ActiveTransfer.LineCount = (int)myr["ImportedRows"];
            }
            conn.Close();   
        }

        public int DatatransferRetrieve(ref IList<string> lines)
        {
            int LineCount = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT TextLine FROM pa_DataTransferHeader INNER JOIN pa_DataTransferLines ON pa_DataTransferHeader.TransID = pa_DataTransferLines.TransID WHERE (BelongsToCompID = @CompID) AND (pa_DataTransferLines.TransID = @TransID) ORDER BY pa_DataTransferLines.LineID";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@TransID", SqlDbType.Int).Value = ActiveTransfer.TransferID;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                lines.Add(myr["TextLine"].ToString());
                LineCount++;
            }
            conn.Close();
            return LineCount;
        }

        public int DatatransferGetList(ref IList<DataTransfer> DTIDs, bool OnlySelectedDef)
        {
            int LineCount = 0;
            DateTime DateOfImport;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT TransID, Description, filename, Processed, ImportedRows FROM pa_DataTransferHeader WHERE (BelongsToCompID = @CompID) ";
            if (OnlySelectedDef) mysql = string.Concat(mysql, " AND SystemID = @TransferName ");
            mysql = string.Concat(mysql, " ORDER BY Finished desc");

            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@TransferName", SqlDbType.NVarChar, 20).Value = ActiveDefinition.TransferName;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                DateTime.TryParse(myr["Processed"].ToString(), out DateOfImport);
                DataTransfer DTID = new DataTransfer();
                DTID.TransferID = (int)myr["TransID"];
                DTID.TransferName = myr["Description"].ToString();
                DTID.FileName = myr["filename"].ToString();
                DTID.DateOfImport = DateOfImport;
                DTID.LineCount = (int)myr["ImportedRows"];
                DTIDs.Add(DTID);
                LineCount++;
            }
            conn.Close();
            return LineCount;
        }

        public int DatatransferGetDefinitionList(ref IList<DataTransferDefinition> DTDefs, TransferType Direction)
        {
            Boolean IsImport = true;
            int LineCount = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT * FROM pa_DataTransferDefinitions WHERE (CompID = @CompID) and ((ImportTransfer = @IsImport) or (@IsImport = 999)) ORDER BY LastRun desc";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            if (Direction == TransferType.Export) IsImport = false;
            if (Direction == TransferType.Import) IsImport = true;
            if (Direction == TransferType.Undefined) IsImport = true;
            comm.Parameters.Add("@IsImport", SqlDbType.Bit).Value = IsImport;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                DataTransferDefinition DTDef = new DataTransferDefinition();
                DTDef.TransferName = myr["SystemID"].ToString();
                DTDef.Type = ((Boolean)myr["ImportTransfer"] ) ? TransferType.Import : TransferType.Export;
                DTDef.Description = myr["Description"].ToString();
                DTDefs.Add(DTDef);
                LineCount++;
            }
            conn.Close();
            return LineCount;
        }

        public int DatatransferPutLines(string[] FileLines)
        {
            int LineCount = FileLines.Count();
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "Insert into dbo.pa_DataTransferLines (TransID, Textline) select @TransID, @Line";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@TransID", SqlDbType.Int).Value = ActiveTransfer.TransferID;
            comm.Parameters.Add("@Line", SqlDbType.NVarChar, 2000);
            conn.Open();
            foreach (string line in FileLines)
            {
                comm.Parameters["@Line"].Value = line;
                comm.ExecuteNonQuery();
            }
            conn.Close();
            return LineCount;
        }

        public string[] DatatransferGetLines(int TransID)
        {
            var fileLines = new List<string>();
            string fileLine;
            SqlConnection conn = new SqlConnection(conn_str);
            if (TransID > 0)
            {
                string mysql = "select TextLine from pa_DataTransferLines Where TransID = @TransID order by LineID";
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@TransID", SqlDbType.Int).Value = TransID;
                conn.Open();
                SqlDataReader myr = comm.ExecuteReader();
                while (myr.Read())
                {
                    fileLine = myr["TextLine"].ToString();
                    fileLines.Add(fileLine);
                }
                conn.Close();
            }
            string[] allLines = fileLines.ToArray();
            return allLines;
        }


        public int DatatransferNewTransfer()
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "Insert into pa_DataTransferHeader (Userid, SystemID, Description, Created, BelongstoCompID) Values (@Userid, @SystemID, @Description, Getdate(), @CompID)  Set @TransID = @@Identity";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@UserID", SqlDbType.NVarChar,20).Value = Username;
            comm.Parameters.Add("@SystemID", SqlDbType.NVarChar, 20).Value = ActiveDefinition.TransferName;
            comm.Parameters.Add("@Description", SqlDbType.NVarChar, 20).Value = ActiveDefinition.Description;
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@TransID", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            comm.ExecuteNonQuery();
            DataTransfer ActiveDT = new DataTransfer();
            ActiveDT.TransferID = (int)comm.Parameters["@TransID"].Value;
            conn.Close();
            ActiveDT.TransferName = ActiveDefinition.TransferName;
            ActiveDT.DateOfImport = DateTime.Now;
            ActiveTransfer = ActiveDT;
            return 1;

        }
        public int DatatransferExecute()
        {
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = ActiveDefinition.HandlingSP;
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@TransID", SqlDbType.Int).Value = ActiveTransfer.TransferID;
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            return 1;
        }

        public int DatatransferExecuteOut(int CompID,string expordID)
        {
            int TransID = 0;
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = ActiveDefinition.HandlingSP;
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@P_CompID", SqlDbType.Int).Value = CompID;
            comm.Parameters.Add("@P_Userid", SqlDbType.NVarChar,20).Value = "WCF";
            comm.Parameters.Add("@P_Systemid", SqlDbType.NVarChar, 20).Value = expordID;
            comm.Parameters.Add("@TransID", SqlDbType.Int).Direction = ParameterDirection.ReturnValue; ;
            conn.Open();
            comm.ExecuteNonQuery();
            TransID = (int)comm.Parameters["@TransID"].Value;
            conn.Close();

            return TransID;
        }

    }
}