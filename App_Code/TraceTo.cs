using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

/// <summary>
/// Summary description for TraceTo
/// </summary>
public class TraceTo
{
    public TraceTo(DBUser DBUser, Exception ex)  //Overload for putting trace in Ooops queue
    {
        if (ex != null ) {
            try {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_update"].ConnectionString);
                SqlCommand myComm = new SqlCommand("wf_oops_save_01", conn);
                myComm.CommandType = CommandType.StoredProcedure;
                myComm.Parameters.Add("@Message", SqlDbType.NVarChar, -1).Value = ex.Message;
                myComm.Parameters.Add("@Trace", SqlDbType.NVarChar, -1).Value = ex.StackTrace;
                myComm.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier).Value = DBUser.CompanyKey;
                myComm.Parameters.Add("@DbID", SqlDbType.UniqueIdentifier).Value = DBUser.DBKey;
                myComm.Parameters.Add("@AppID", SqlDbType.NVarChar, 50).Value = "WfWCF";
                myComm.Parameters.Add("@Environment", SqlDbType.NVarChar, -1).Value = ex.TargetSite.ToString();
                conn.Open();
                myComm.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex2)
            {//'Ups'
            }
        }
    }
    public TraceTo(string Text)     //Overload for putting trace in Log
    {

    }
}