using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.ServiceModel;
namespace wfws
{
    public class ConnectLocal
    {
        public int CompID;
        public string connStr;
        public Guid UserGuid;
        String UserHash;
        public String newPassword;
        public String UserEmail;
        //String ShopID;
        Guid ConnGuid;
        // public int ConnID;
        public ConnectLocal(DBUser DBUser)
        {
            ConnGuid = DBUser.DBKey;
            CompID = DBUser.CompID;
            connStr = DBUser.ConnectionString;
            //ConnID = DBUser.ConnID;
        }
     
        public string ConnectionGetByGuid_02(ref DBUser DBUser)
        {
            try
            {
                if (DBUser.DBKey != Guid.Empty)
                {
                    DBUser.DBKey = ResolveServiceProviderGuid(DBUser.DBKey, DBUser.CompanyKey);
                    // SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                    // 'SqlCommand myCommand = new SqlCommand("wf_apl_GetConnectString_02", wfConnection);
                    SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_user"].ConnectionString);
                    SqlCommand myCommand = new SqlCommand("ws_apl_GetConnectString_1", wfConnection);
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.Parameters.Add("@ApiGuid", SqlDbType.UniqueIdentifier).Value = DBUser.DBKey;
                    myCommand.Parameters.Add("@ConnStr", SqlDbType.NVarChar, 200).Direction = ParameterDirection.Output;
                    myCommand.Parameters.Add("@dbID", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Output;
                    myCommand.Parameters.Add("@ShopID", SqlDbType.NVarChar, 20).Direction = ParameterDirection.Output;
                    myCommand.Parameters.Add("@PublicConnection", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    wfConnection.Open();
                    myCommand.ExecuteNonQuery();
                    connStr = myCommand.Parameters["@ConnStr"].Value.ToString();
                    DBUser.ShopID = myCommand.Parameters["@ShopID"].Value.ToString();
                    DBUser.PublicConnection = (Boolean)myCommand.Parameters["@PublicConnection"].Value;
                    wfConnection.Close();
                    var wf_comp = new wfws.Company(ref DBUser, connStr);
                    wf_comp.get_company_by_Guid(ref DBUser);
                    CompID = DBUser.CompID;
                }
            }
            catch (NullReferenceException ex)
            {
                throw new FaultException(String.Concat("wf_wcf: ", ex.Message), new FaultCode("wfwcfFault"));
            }
            return connStr;
        }
        private Guid ResolveServiceProviderGuid(Guid PossibleSPGuid, Guid CompGuid)
        {
            SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_user"].ConnectionString);
            SqlCommand myCommand = new SqlCommand("wf_sp_ServiceProviderGuidResolve", wfConnection);
            myCommand.CommandType = CommandType.StoredProcedure;
            myCommand.Parameters.Add("@P_CompGuid", SqlDbType.UniqueIdentifier).Value = CompGuid;
            myCommand.Parameters.AddWithValue("@P_Guid", PossibleSPGuid).Direction = ParameterDirection.InputOutput;
            wfConnection.Open();
            myCommand.ExecuteNonQuery();
            Guid retval = (Guid)myCommand.Parameters["@P_Guid"].Value;
            wfConnection.Close();
            return retval;
        }
        public Guid[] FetchServiceProviderAdmissions(Guid ServiceProviderGuid)
        {
            IList<Guid> CompGuids = new List<Guid>();
            SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_user"].ConnectionString);
            SqlCommand myCommand = new SqlCommand("wf_sp_ServiceProviderAdmissions", wfConnection);
            myCommand.CommandType = CommandType.StoredProcedure;
            myCommand.Parameters.Add("@P_SPGuid", SqlDbType.UniqueIdentifier).Value = ServiceProviderGuid;
            wfConnection.Open();
            SqlDataReader myr = myCommand.ExecuteReader();
            while (myr.Read())
            {
                CompGuids.Add((Guid)myr["CompanyGuid"]);
            }
            wfConnection.Close();
            return CompGuids.ToArray();
        }
        // Local users
        // Get user from username password. This is the end user´s access. Not then API
        // Users to different databases are separated by the ConnID. 
        public string UserGetByPassword(ref DBUser DBUser, string UserName, String Password)
        {
            string errstr = "OK";
            try
            {
                UserHash = FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "MD5");
                SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                SqlCommand myCommand = new SqlCommand("wf_connect", wfConnection);
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.Parameters.Add("@ConnID", SqlDbType.Int).Value = DBUser.ConnID;
                myCommand.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = UserName;
                myCommand.Parameters.Add("@UserHash", SqlDbType.NVarChar, 100).Value = UserHash;
                myCommand.Parameters.Add("@UserGuid", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Output;
                wfConnection.Open();
                myCommand.ExecuteNonQuery();
                UserGuid = ((Guid)myCommand.Parameters["@UserGuid"].Value);
                wfConnection.Close();
            }
            catch (NullReferenceException ex)
            {
                errstr = ex.Message;
            }
            return errstr;
        }
        public string UserAddNew(ref DBUser DBUser, string UserName, string email)
        {
            string errstr = "OK";
            try
            {
                SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                SqlCommand myCommand = new SqlCommand("wf_users_add_New", wfConnection);
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.Parameters.Add("@ConnID", SqlDbType.Int).Value = DBUser.ConnID;
                myCommand.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = UserName;
                myCommand.Parameters.Add("@UserHash", SqlDbType.NVarChar, 64).Value = string.Empty;
                myCommand.Parameters.Add("@email", SqlDbType.NVarChar, 64).Value = email;
                myCommand.Parameters.Add("@UserGuid", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Output;
                wfConnection.Open();
                myCommand.ExecuteNonQuery();
                UserGuid = ((Guid)myCommand.Parameters["@UserGuid"].Value);
                wfConnection.Close();
            }
            catch (NullReferenceException ex)
            {
                errstr = ex.Message;
            }
            return errstr;
        }
        public string UserUpdate(string UserName, String email)
        {
            string errstr = "err";
            try
            {
                SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                SqlCommand myCommand = new SqlCommand("wf_userUpdate", wfConnection);
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.Parameters.Add("@UserGuid", SqlDbType.UniqueIdentifier).Value = UserGuid;
                myCommand.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = UserName;
                myCommand.Parameters.Add("@UserEmail", SqlDbType.NVarChar, 64).Value = email;
                wfConnection.Open();
                myCommand.ExecuteNonQuery();
                wfConnection.Close();
                errstr = "OK";
            }
            catch (NullReferenceException ex)
            {
                errstr = ex.Message;
            }
            return errstr;
        }
        public string UserEsists(ref DBUser DBUser, string UserName, string email)
        {
            string errstr = "OK";
            int count = 0;
            try
            {
                SqlConnection wfConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                SqlCommand myCommand = new SqlCommand("wf_users_check", wfConnection);
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.Parameters.Add("@ConnID", SqlDbType.Int).Value = DBUser.ConnID;
                myCommand.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = UserName;
                myCommand.Parameters.Add("@Count", SqlDbType.Int).Direction = ParameterDirection.Output;
                wfConnection.Open();
                myCommand.ExecuteNonQuery();
                count = (Int32)myCommand.Parameters["@Count"].Value;
                wfConnection.Close();
                if (count > 0)
                {
                    errstr = "user exists";
                    DBUser.Message = "user existsw";
                }
            }
            catch (NullReferenceException ex)
            {
                errstr = ex.Message;
            }
            return errstr;
        }
        private string PasswordGet()
        {
            var r = new Random(System.DateTime.Now.Millisecond);
            int randomNo = r.Next(1000, 9000);
            string keyvalue = string.Empty;
            SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            SqlCommand comm = new SqlCommand("wf_ut_create_key", Conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@id", SqlDbType.Int).Value = randomNo;
            comm.Parameters.Add("@KeyLength", SqlDbType.Int).Value = 8;
            comm.Parameters.Add("@Link", SqlDbType.NVarChar, 20).Direction = ParameterDirection.Output;
            Conn.Open();
            comm.ExecuteNonQuery();
            keyvalue = comm.Parameters["@Link"].Value.ToString();
            Conn.Close();
            return keyvalue;
        }
        public string PasswordNew(string Password)
        {
            string errstr = "OK";
            try
            {
                Password = "BB";
                if ((Password == string.Empty) || (Password.Length < 6))
                {
                    newPassword = PasswordGet();
                }
                UserHash = FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "MD5");
                UserHash = FormsAuthentication.HashPasswordForStoringInConfigFile(newPassword, "MD5");
                SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                SqlCommand comm = new SqlCommand("wf_PasswordSave", Conn);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@UserGuid", SqlDbType.UniqueIdentifier).Value = UserGuid;
                comm.Parameters.Add("@UserHash", SqlDbType.NVarChar, 64).Value = UserHash;
                comm.Parameters.Add("@UserEmail", SqlDbType.NVarChar, 64).Direction = ParameterDirection.Output;
                Conn.Open();
                comm.ExecuteNonQuery();
                UserEmail = comm.Parameters["@UserEmail"].Value.ToString();
                Conn.Close();
            }
            catch (NullReferenceException ex)
            {
                errstr = ex.Message;
            }
            return errstr;
        }
 
        public Boolean Is_wf_user(string Username, string Password)
        {
            Boolean returnval = false;
            SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString_user"].ConnectionString);
            SqlCommand comm = new SqlCommand("wf_apl_user_connect", Conn);
            comm.CommandType = CommandType.StoredProcedure;
            var userid = Guid.Empty;
            var UserHash = FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "MD5");
            comm.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = Username;
            comm.Parameters.Add("@Guid", SqlDbType.NVarChar, 100).Value = UserHash;
            Conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read())
            {
                userid = (Guid)myr["UserID"];
            }
            if (userid != Guid.Empty) returnval = true;
            return returnval;
        }
    }
}