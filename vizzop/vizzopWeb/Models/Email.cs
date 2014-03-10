using System;
//using Microsoft.Web.Mvc.Resources;
using System.Net.Mail;
using System.Threading;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Email
    {
        [NonSerialized]
        private vizzopContext db = new vizzopContext();

        public Message message;

        public bool withBcc = true;

        public Email()
        {
            /*
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            this.serverurl = appSettings["mailserverurl"];
             */
        }

        public Boolean send()
        {
            Utils utils = new Utils();
            try
            {
                email_otherthread emailthread = new email_otherthread(this.message);
                emailthread.withBcc = this.withBcc;
                Thread backgroundthread = new Thread(new ThreadStart(emailthread.send));
                backgroundthread.Priority = ThreadPriority.BelowNormal;
                backgroundthread.Start();
                return true;

            }
            catch (System.ApplicationException ex)
            {
                utils.GrabaLogExcepcion(ex);
                return false;
            }

        }

        internal class email_otherthread
        {
            private vizzopContext db = new vizzopContext();
            private Utils utils = new Utils();
            private MailMessage _mailmessage;
            private Message _message;
            private SmtpClient _smtpclient;
            public bool withBcc = true;
            //private Message _communication;
            public email_otherthread(Message message)
            {
                this._message = message;
                this._smtpclient = new SmtpClient("smtp.gmail.com", 587);
                System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential("customer.service@vizzop.com", "Q4b4s4bslc");
                this._smtpclient.UseDefaultCredentials = false;
                this._smtpclient.Credentials = SMTPUserInfo;
                //Enable SSL
                this._smtpclient.EnableSsl = true;
                this._mailmessage = new MailMessage();
                this._mailmessage.IsBodyHtml = true;
                this._mailmessage.From = new MailAddress(this._message.From.Email);
                this._mailmessage.To.Add(new MailAddress(this._message.To.Email));
                this._mailmessage.Bcc.Add(new MailAddress(this._message.From.Email));
                this._mailmessage.Subject = this._message.Subject;
                this._mailmessage.Body = this._message.Content + utils.LocalizeLang("email_footer", this._message.Lang, null);

                this._mailmessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
            }
            public void send()
            {
                try
                {
                    if (this.withBcc == true) this._mailmessage.Bcc.Add(new MailAddress(this._message.From.Email));
                    this._smtpclient.Send(this._mailmessage);                    
                    utils.GrabaLog(Utils.NivelLog.info, "E-Mail Sent to " + this._mailmessage.To[0].Address.ToString());
                    this._message.Sent = 1;
                    db.SaveChanges();
                }
                catch (System.Net.Mail.SmtpException ex)
                {
                    utils.GrabaLogExcepcion(ex);
                }
            }
        }

    }
}