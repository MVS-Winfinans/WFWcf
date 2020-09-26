using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Configuration;
/// <summary>
/// Special class for fast lookups in IIS's local sql. Countrycodes, PostalCodes, CN8, UNSPC.
/// </summary>
namespace wfws
{
    public enum CountryIDType { VehicleCode, ISO_1, ISO_2, ISO_3, CountryNo, CountryName, UBLPartyScheme, UBLTaxScheme };
    public enum LanguageIDType { ISO_1, ISO_2, LanguageName, NomDeLaLangue, PreferredForCountryVC };
    public class LookUp
    {
        private string LookupDBString;
        private string TranslateDBString;

        public LookUp()
        {
            LookupDBString = ConfigurationManager.ConnectionStrings["wf_lookup"].ConnectionString;
            TranslateDBString = ConfigurationManager.ConnectionStrings["ConnectionString_translate"].ConnectionString;
        }
        //UTILS
        public string convCountry(CountryIDType FromCode, CountryIDType ToCode, string Code, string defVal = "")
        {
            string Result = "";
            if (string.IsNullOrEmpty(Code))
            {
                Result = "";
            }
            else
            {
                string SQL = string.Concat("Select min(", FieldName(ToCode), ") from ac_Countries where ", FieldName(FromCode), "=@P_Code");
                SqlConnection Conn = new SqlConnection(LookupDBString);
 
                try{
                    SqlCommand comm = new SqlCommand(SQL, Conn);
                    comm.Parameters.Add("@P_Code", SqlDbType.NVarChar).Value = Code;
                    Conn.Open();
                    SqlDataReader myr  = comm.ExecuteReader(CommandBehavior.SingleRow);
                    if (myr.Read())
                        Result= myr[0].ToString();
                    Conn.Close();
                    if (string.IsNullOrEmpty(Result)) Result = defVal;
                }
                catch (Exception e) { Result = e.Message; }
            }
            return Result;
        }
        public string convLanguage(LanguageIDType FromCode, LanguageIDType ToCode, string Code, string defVal = "")
        {
            string Result = "";
            if (string.IsNullOrEmpty(Code))
            {
                Result = "";
            }
            else
            {
                string SQL = string.Concat("SELECT MIN(", FieldName(ToCode), ") FROM ac_CountryLanguage INNER JOIN ac_Languages ON ac_CountryLanguage.Language = ac_Languages.Iso639_1 WHERE (ac_CountryLanguage.Priority = 1) AND ", FieldName(FromCode), "=@P_Code");
                SqlConnection Conn = new SqlConnection(LookupDBString);

                try
                {
                    SqlCommand comm = new SqlCommand(SQL, Conn);
                    comm.Parameters.Add("@P_Code", SqlDbType.NVarChar).Value = Code;
                    Conn.Open();
                    SqlDataReader myr = comm.ExecuteReader(CommandBehavior.SingleRow);
                    if (myr.Read())
                        Result = myr[0].ToString();
                    Conn.Close();
                    if (string.IsNullOrEmpty(Result)) Result=defVal;
                }
                catch (Exception e) { Result = e.Message; }
            }
            return Result;
        }

        public wflist[] GetWFClasses(wfmodule module, string language)
        {
            List<wflist> answer = new List<wflist>();
            SqlConnection conn = new SqlConnection(TranslateDBString);
            string mysql = "wf_apl_classes_get_01";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@Module", SqlDbType.Int).Value = (int)module;
            comm.Parameters.Add("@Language", SqlDbType.NVarChar,20).Value = language;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                wflist line = new wflist();
                line.Class = (int)myr["ClassID"];
                line.ClassText = myr["ClassText"].ToString();
                answer.Add (line);
            }
            conn.Close();
            return answer.ToArray();
        }

        public wflist[] GetDropdownList(wfdroplist ListID, string language)
        {
            List<wflist> answer = new List<wflist>();
            int intClass;
            SqlConnection conn = new SqlConnection(TranslateDBString);
            string mysql = "wf_apl_DL_get";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@dlid", SqlDbType.Int).Value = (int)ListID;
            comm.Parameters.Add("@CountryID", SqlDbType.NVarChar, 20).Value = language;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                wflist line = new wflist();
                int.TryParse(myr["DataValue"].ToString(), out intClass);
                line.Class = intClass;
                line.ClassText = myr["DataText"].ToString();
                answer.Add(line);
            }
            conn.Close();
            return answer.ToArray();

        }
        public bool IsSQLDate(DateTime TheDate)
        {
            return (bool)((TheDate > (DateTime)SqlDateTime.MinValue) && (TheDate < (DateTime)SqlDateTime.MaxValue));
        }
        //XML
        public string[] GetDefaultPathSet(DocType doctype, int Set)
        {
            List<string> answer = new List<string>();
            SqlConnection conn = new SqlConnection(LookupDBString);
            string mysql = "SELECT ubl_XPaths_lines.line FROM ubl_XPaths INNER JOIN ubl_XPaths_lines ON ubl_XPaths.ID = ubl_XPaths_lines.XPathID WHERE (ubl_XPaths.XPathType = @Type) AND (ubl_XPaths.XPathSet = @Set) ORDER BY ubl_XPaths_lines.pos";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@Type", SqlDbType.Int).Value = (int)doctype;
            comm.Parameters.Add("@Set", SqlDbType.Int).Value = Set;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            while (myr.Read())
            {
                answer.Add (myr["line"].ToString());
            }
            conn.Close();
            return answer.ToArray();
        }

        public string GetValidationScheme(string SchemeName)
        {
            string answer="";
            SqlConnection conn = new SqlConnection(LookupDBString);
            string mysql = "SELECT Definition FROM ubl_ValidationSchemas WHERE (schema_name = @Scheme)";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@Scheme", SqlDbType.NVarChar,100).Value = SchemeName;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read()) answer = myr["Definition"].ToString();
            conn.Close();
            return answer;
        }

        public bool IsEndpointTypeValid(string EndpointType)
        {
            bool answer = false;
            SqlConnection conn = new SqlConnection(LookupDBString);
            string mysql = "SELECT * FROM ubl_EndpointID WHERE (Value = @EndpointType)";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@EndpointType", SqlDbType.NVarChar, 100).Value = EndpointType;
            conn.Open();
            SqlDataReader myr = comm.ExecuteReader();
            if (myr.Read()) answer = true;
            conn.Close();
            return answer;
        }

        public string GetRandomName(AddressNameType type = AddressNameType.Fullname)
        {
            string answer = string.Empty;
            SqlConnection conn = new SqlConnection(LookupDBString);
            string mysql = "apl_GenerateName";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.Add("@p_Type", SqlDbType.Int).Value = (int)type;
            conn.Open();
            answer = comm.ExecuteScalar().ToString();
            conn.Close();
            return answer;
        }


        private int PutDefaultPathSet(ref string[] answer, DocType doctype, int Set)         //Temporary function
        {
            int pos = 0;
            int NewID = 0;
            SqlConnection conn = new SqlConnection(LookupDBString);
            string mysql = "INSERT INTO [dbo].[ubl_XPaths] ([XPathType],[XPathSet],[Description]) VALUES (@Type, @Set, 'Default');SELECT CAST(scope_identity() AS int)";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@Type", SqlDbType.Int).Value = (int)doctype;
            comm.Parameters.Add("@Set", SqlDbType.Int).Value = Set;
            conn.Open();
            NewID = (int)comm.ExecuteScalar();
            if (NewID != 0) {
                mysql = "INSERT INTO [dbo].[ubl_XPaths_lines] ([XPathID],[line]) VALUES (@ID , @Line)";
                SqlCommand LCmd = new SqlCommand(mysql, conn);
                LCmd.Parameters.Add("@ID", SqlDbType.Int).Value = NewID;
                LCmd.Parameters.Add("@Line", SqlDbType.NVarChar, 250).Value = "";
                foreach (string OneLine in answer) {
                    LCmd.Parameters[1].Value = OneLine;
                    LCmd.ExecuteNonQuery();
                }
            }
            conn.Close();
            return pos;
        }



        private string FieldName(CountryIDType FieldType)
        {
            string Result;
            switch (FieldType)
            {
                case CountryIDType.CountryName:
                    Result = "Country";
                    break;
                case CountryIDType.CountryNo:
                    Result = "CountryNo";
                    break;
                case CountryIDType.ISO_1:
                    Result = "ISO3166_1";
                    break;
                case CountryIDType.ISO_2:
                    Result = "ISO3166_2";
                    break;
                case CountryIDType.ISO_3:
                    Result = "ISO3166_3";
                    break;
                case CountryIDType.VehicleCode:
                    Result = "CountryID";
                    break;
                case CountryIDType.UBLPartyScheme:
                    Result = "UBLPartyScheme";
                    break;
                case CountryIDType.UBLTaxScheme:
                    Result = "UBLTaxScheme";
                    break;
                default:
                    Result = "";
                    break;
            }
            return Result;
        }
        private string FieldName(LanguageIDType FieldType)
        { 
            string Result;
            switch (FieldType)
            {
                case LanguageIDType.ISO_1:
                    Result = "Iso639_1";
                    break;
                case LanguageIDType.ISO_2:
                    Result = "Iso639_2";
                    break;
                case LanguageIDType.LanguageName:
                    Result = "Name_en";
                    break;
                case LanguageIDType.NomDeLaLangue:
                    Result = "Name_fr";
                    break;
                case LanguageIDType.PreferredForCountryVC:
                    Result = "CountryID";
                    break;
                default:
                    Result = "";
                    break;
            }
            return Result;
         }
    }
}