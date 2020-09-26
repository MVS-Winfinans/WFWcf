using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Implementing web class, which performs actual database calls
/// see other files named web*, which implements grouped functions for readability
/// </summary>

namespace wfws
{
    public partial class web
    {
        protected int compID;
        protected string conn_str;

        public web(ref DBUser DBUser)
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

        public bool ValidateSeller(ref CompanySeller MySeller)
        {
            bool Succes = false;
            int Result = 0;
                    SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "SELECT @Result = min(SellerID) FROM ac_Companies_Sellers WHERE Compid = @CompID and ((SellerID = @ID) OR (SellerName = @Name) OR (SellerEAN = @EAN) OR (CompanyNo = @No))";
            //try
        {
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@ID", SqlDbType.Int).Value = MySeller.SellerID;
                comm.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(MySeller.SellerName) ? "XXXXXXXXXX" : (MySeller.SellerName).Substring(0, Math.Min(50, MySeller.SellerName.Length)));
                comm.Parameters.Add("@EAN", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(MySeller.SellerEAN) ? "XXXXXXXXXX" : MySeller.SellerEAN);
                comm.Parameters.Add("@No", SqlDbType.NVarChar, 50).Value = (string.IsNullOrEmpty(MySeller.CompanyNo) ? "XXXXXXXXXX" : MySeller.CompanyNo);
                SqlParameter MyResult = comm.Parameters.Add("@Result", SqlDbType.Int);
                MyResult.Direction = ParameterDirection.Output;
                conn.Open();
            comm.ExecuteNonQuery();
                Result = (int)MyResult.Value;
            conn.Close();
       }
            //catch (Exception e) { errStr = e.Message; }
            if (Result != 0)
            {
                MySeller.SellerID = Result;
                Succes = true;
            }
                else
                Succes = false;
            return Succes;
        }


        public void Order_AddDim(string Dim1, string Dim2, string Dim3, string Dim4)
        {
             SqlConnection conn = new SqlConnection(conn_str);
            string mysql = "if not exists (SELECT * FROM fi_Dimensions WHERE CompID = @CompID AND DimNo = @DimNo AND DimID = @DimID)  INSERT fi_Dimensions (CompID,DimNo,DimID,DimText,EnterDate,Closed) values (@CompID, @DimNo,@DimID,@DimID,getdate(),0) ";
            SqlCommand comm = new SqlCommand(mysql, conn);
            comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
            comm.Parameters.Add("@DimNo", SqlDbType.Int).Value = 1;
            comm.Parameters.Add("@DimID", SqlDbType.NVarChar, 20).Value = DBNull.Value;
            conn.Open();
            if (!string.IsNullOrEmpty(Dim1))
            {
                comm.Parameters["@DimNo"].Value = 1;
                comm.Parameters["@DimID"].Value = Dim1;
                comm.ExecuteNonQuery();
            }
            if (!string.IsNullOrEmpty(Dim2))
            {
                comm.Parameters["@DimNo"].Value = 2;
                comm.Parameters["@DimID"].Value = Dim2;
                comm.ExecuteNonQuery();
            }
            if (!string.IsNullOrEmpty(Dim3))
            {
                comm.Parameters["@DimNo"].Value = 3;
                comm.Parameters["@DimID"].Value = Dim3;
                comm.ExecuteNonQuery();
            }
            if (!string.IsNullOrEmpty(Dim4))
            {
                comm.Parameters["@DimNo"].Value = 4;
                comm.Parameters["@DimID"].Value = Dim4;
                comm.ExecuteNonQuery();
       }
                conn.Close();
        }


        public Guid Address_get_basket_Guid(int AddressID,ref string retstr)
        {
            int BasketID = 0;
            Guid BasketGuid = Guid.Empty;
            if (AddressID > 0)
            {
                string mysql = "SELECT isnull(max(BasketID),0) FROM web_basket WHERE CompID = @CompID AND (AddressID <> 0 AND AddressID = @AddressID) OR (sh_AddressID <> 0 AND sh_AddressID = @AddressID) AND Class = 0";
                SqlConnection conn = new SqlConnection(conn_str);
                SqlCommand comm = new SqlCommand(mysql, conn);
                comm.Parameters.Add("@CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("@AddressID", SqlDbType.Int).Value = AddressID;
                comm.Parameters.Add("@BasketID", SqlDbType.Int).Value = 0;
                conn.Open();
                BasketID = (int)comm.ExecuteScalar();
                if (BasketID > 0)
                {
                    retstr = BasketID.ToString();
                    comm.Parameters["@BasketID"].Value = BasketID;
                    comm.CommandText = "SELECT Basketguid FROM web_basket WHERE CompID = @CompID AND BasketID = @BasketID";
                    BasketGuid = (Guid)comm.ExecuteScalar();
                }
                conn.Close();
            }
            return BasketGuid;
        }




    
    }
}