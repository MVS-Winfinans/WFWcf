using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for utils
/// </summary>
public class utils
{
    public utils()
    {
        //
        // TODO: Add constructor logic here
        //
    }


    public int connectstring_split(ref string ConnKey)
    {
        string[] conn1;
        string comp_key;
        string conn_key;
        string constr = string.Empty;
        int CompID = 0;

        conn1 = ConnKey.Split(';');

        if (conn1.Length == 2)
        {
            conn_key = conn1[1];
            comp_key = conn1[0];

            // IIf(IsNumeric(conn1(0)), conn1(0), 0) i VB.NET oversættes til:
            // Prøv at parse string til int, hvis ikke muligt, sæt til 0
            int.TryParse(conn1[0], out CompID);

            if (CompID == 0)
            {
                constr = get_connection(conn_key);
                CompID = get_company_by_key(constr, comp_key);
            }
        }
        else
        {
            // Right(ConnKey, Len(ConnKey) - 5) i C#: substring fra index 5 til slut
            conn_key = ConnKey.Substring(5);
            // Left(ConnKey, 5) i C#: substring fra start til index 5 (eksklusiv)
            comp_key = ConnKey.Substring(0, 5);

            constr = get_connection(conn_key);
            CompID = get_company_by_key(constr, comp_key);
        }

        ConnKey = constr;
        return CompID;
    }

    public string get_connection(string connKey)
    {
        string connstr;

        using (SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_user"].ConnectionString))
        {
            //SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_user"].ConnectionString);

            using (SqlCommand myCommand = new SqlCommand("wf_apl_createConnectstring_DanLoen", wfConnection))
            {
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.Parameters.Add("@connkey", SqlDbType.NVarChar).Value = connKey;
                myCommand.Parameters.Add("@ConnStr", SqlDbType.NVarChar, 200).Direction = ParameterDirection.Output;
                wfConnection.Open();
                myCommand.ExecuteNonQuery();
                connstr = myCommand.Parameters["@ConnStr"].Value.ToString();
            }
        }

        return connstr;
    }

    public int get_company_by_key(string connKey, string compKey)
    {
        int CompID = 1000;
        try
        {
            using (SqlConnection wfConnection = new SqlConnection(connKey))
            {
                string mysql = "SELECT ISNULL(CompID, 0) FROM ac_companies WHERE connKey = @connKey";

                using (SqlCommand myCommand = new SqlCommand(mysql, wfConnection))
                {
                    myCommand.Parameters.Add("@connKey", SqlDbType.NVarChar, 20).Value = compKey;
                    wfConnection.Open();
                    CompID = (int)myCommand.ExecuteScalar();
                }
            }
        }
        catch (Exception)
        {
            CompID = 0;
        }
        return CompID;
    }


}