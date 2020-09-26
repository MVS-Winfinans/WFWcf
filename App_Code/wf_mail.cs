using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.IO;
// Imports System.Net.Mail
// Imports System.Data
// Imports System.Net



/// <summary>
/// Summary description for wf_mail
/// </summary>
/// 

namespace wfws
{

    public class wf_mail
    {

        int compID;
        string conn_str;


        public string mailTo;
        public string mailCC;
        public string mailSubject;
        public string mailAtt_1;
        public string mailAtt_2;
        public byte[] TheData;
        public Boolean isHtmlBody;
        public string errstr;
      
        //private string userEmail;
        //private Boolean SmtpUseSSL;
        //private string SmtpServer;
        public string MailFrom;
        public string MailBcc;
        public string HostName;
        public string userName;
        public string userPassword;
        public Boolean EnableSsl;
        public string mailbody;

        //private int ClientID;
        public int emailPort;
        //private Boolean isAttashment;
        //private string AttString;

        public wf_mail(ref DBUser DBUser)
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
            //if (DBUser.PublicConnection == false) DBUser.ConnectionString = "Not public";
         
            //ClientID = 0;
            MailFrom = "support@Winfinans.dk";
            HostName = "smtp.securenetwork.dk";

            userName = String.Empty;
            userPassword = String.Empty;
            EnableSsl = false;
            emailPort = 25;
            isHtmlBody = true;

        }

    

        public void get_Client(int ClientID)
        {
              SqlConnection conn = new SqlConnection(conn_str);
            string mysql  = "SELECT * FROM ac_Companies_mail_clients  WHERE CompID = @CompID AND ClientID = @ClientID ";
             SqlCommand comm = new SqlCommand(mysql, conn);

            MailFrom = "wf@Winfinans.dk";
            HostName = "smtp.securenetwork.dk";
            if (ClientID > 0)
            {

                comm.Parameters.Add("CompID", SqlDbType.Int).Value = compID;
                comm.Parameters.Add("ClientID", SqlDbType.Int).Value = ClientID;
                conn.Open();
                   SqlDataReader myr  = comm.ExecuteReader();
                 if  (myr.Read()) {

                    MailFrom = myr["MailFrom"].ToString();
                    MailBcc = myr["MailBcc"].ToString();
                    HostName = myr["HostName"].ToString();
                    userName = myr["userName"].ToString();
                    userPassword = myr["userPassword"].ToString();
                    EnableSsl = (Boolean) myr["EnableSsl"];
                 }
                conn.Close();
            } // if ClientID

         
            int pos = HostName.IndexOf(";");
            if (pos > 1)
            {
                var arhost = HostName.Split(';');
                HostName = arhost[0];
                int.TryParse(arhost[1], out emailPort);
            }

            if (emailPort == 0)
            {
                if (EnableSsl) emailPort = 587; else emailPort = 25;
            }

            if (string.IsNullOrEmpty(HostName)) HostName = "smtp.securenetwork.dk";
            

            if (HostName == "smtp.securenetwork.dk")
            {
                emailPort = 25;
                EnableSsl = false;
            }
        }




         public string SendMail() 
         {
            var Msg = new MailMessage();
            string errstr = "OK";
            string sto = string.Empty;
            try
             {
                 //isAttashment = false;
                  // if (Attach.Length > 0) {
                    // Dim myAttachment As System.Net.Mail.Attachment = New System.Net.Mail.Attachment(Attach, "Faktura.pdf")

                    // var myAttachment = new System.Net.Mail.Attachment(Attach, "Faktura.pdf");

                  //  myAttachment.ContentType.MediaType = MediaTypeNames.Application.Pdf;
                  //  Msg.Attachments.Add(myAttachment);
                 //  isAttashment = true;
                 //  }


             
                   var ms = new MemoryStream(TheData);
                    var myAttachment = new  System.Net.Mail.Attachment(ms,"Faktura.pdf");
                    myAttachment.ContentType.MediaType = System.Net.Mime.MediaTypeNames.Application.Pdf;
                    Msg.Attachments.Add(myAttachment);
               


             
                int tocount  = 0;
                Msg.IsBodyHtml = true;
                string[] tosplit  = mailTo.Split(';');   // '(New [Char]() {" "c, ","c, "."c, ":"c})
               
       
                 foreach (string sto1 in tosplit) {
                    if (sto1.Trim() != "") {
                        Msg.To.Add(new MailAddress(sto1));
                        tocount = tocount + 1;
                    }
                 }
                Msg.From = new MailAddress(MailFrom);
                Msg.Subject = mailSubject;
                Msg.Body = mailbody;
                            
                if (!string.IsNullOrEmpty(mailCC)) {
                    var ccsplit  = mailCC.Split(';');
                    foreach( string sto_1 in ccsplit) {
                        
                        if (sto_1.Trim() != "") {
                            Msg.CC.Add(new MailAddress(sto_1));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(MailBcc)) {
                    var bccsplit  = MailBcc.Split(';');
                    foreach (string sto_2 in bccsplit) {
                        if (sto_2.Trim() != "") {
                            Msg.Bcc.Add(new MailAddress(sto_2));
                        }
                    }
                }
                try
                    {
                    var client  = new System.Net.Mail.SmtpClient(HostName);
                    client.EnableSsl = EnableSsl;
                    client.Port = emailPort;

                    if  (!string.IsNullOrEmpty(userName)) {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(userName, userPassword);
                    } else {
                        client.UseDefaultCredentials = true;
                    }
                    client.Send(Msg);
                    Msg.Attachments.Clear();
                    Msg.Dispose();
                }

                catch (System.Web.HttpException ehttp) { errstr = String.Concat(ehttp.Message, " -##- ", ehttp.ToString()); }
            }  // try

            catch (Exception e) { errstr = e.Message; }
            return errstr;
       }


      






    }

}