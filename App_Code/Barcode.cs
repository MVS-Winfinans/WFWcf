using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Summary description for Barcode
/// </summary>
namespace wfws
{
    public class Barcode
    {
        protected int compID;
        protected string conn_str;
        public Barcode(ref DBUser DBUser)
        {
            if (string.IsNullOrEmpty(DBUser.ConnectionString))
            {
                var wfconn = new wfws.ConnectLocal(DBUser);
                conn_str = wfconn.ConnectionGetByGuid_02(ref DBUser);
            }
            else
            {
                conn_str = DBUser.ConnectionString;
            }
            compID = DBUser.CompID;
        }

        public string BarcodeLookup (string Barcode)
        {
            string retval = "Not found";
            SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "Select min(Description) from tr_inventory where compid = @Compid and ean = @Barcode";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@Barcode", SqlDbType.NVarChar, 20).Value = Barcode;
            conn.Open();
            retval = comm.ExecuteScalar().ToString();
            return retval;
        }
     }
}