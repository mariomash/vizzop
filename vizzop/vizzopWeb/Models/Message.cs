using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace vizzopWeb.Models
{

    [Serializable]
    public class Message //: ISerializable
    {
        [NonSerialized]
        public vizzopContext db = new vizzopContext();

        [NonSerialized]
        public Utils utils = new Utils();

        public Boolean DoAddToCache()
        {
            try
            {
                this.utils = new Utils();


                List<Message> Messages = new List<Message>();
                this.Sent = 1;
                this.utils = null;
                this.db = null;
                string key = "messages_to_" + this.To_UserName + "@" + this.To_Domain;

                DataCacheLockHandle lockHandle;
                object result = SingletonCache.Instance.GetWithLock(key, out lockHandle);
                //object result = SingletonCache.Instance.Get(key);
                if (result != null)
                {
                    Messages = (List<Message>)result;
                }
                Messages.Add(this);
                return SingletonCache.Instance.InsertWithLock(key, Messages, lockHandle);
                //return SingletonCache.Instance.Insert(key, Messages);

                /*
                if (HttpContext.Current.Application[key] != null)
                {
                    Messages = (List<Message>)HttpContext.Current.Application[key];
                    Messages.Add(this);
                }
                else
                {
                    Messages.Add(this);
                    HttpContext.Current.Application[key] = Messages;
                }
                */

                //return true;
            }
            catch (Exception ex)
            {
                this.utils = new Utils();
                this.utils.GrabaLogExcepcion(ex);
                this.utils.GrabaLog(Utils.NivelLog.error, "No se pudo grabar msg: " + this.Subject + ", " + this.Content);
                return false;
            }
        }

        public Boolean AddToCache()
        {
            try
            {

                /*
                #if DEBUG
                            foreach (var obj in cache.GetSystemRegions().SelectMany(cache.GetObjectsInRegion))
                            {
                                cache.ResetObjectTimeout(obj.Key, new TimeSpan(0, 0, 0, 0, 01)); //set to expire basically immediately
                            }
                #endif
                */

                Boolean result = false;
                int counter = 0;
                this.ID = this.utils.RandNumber(0, 1000000000);
                while ((result == false) && (counter < 10))
                {
                    result = this.DoAddToCache();
                }
                return result;
            }
            catch (Exception ex)
            {
                Utils utils = new Utils();
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public Boolean AddToDb()
        {
            try
            {

                if (this.db == null)
                {
                    this.db = new vizzopContext();
                }

                this.Sent = 1;
                this.Status = 1;

                this.loadConversersFromSpareData();

                db.Messages.Add(this);
                db.SaveChanges();

                return true;

            }
            catch (Exception ex)
            {
                try
                {
                    utils.GrabaLog(Utils.NivelLog.error, "Error Adding to DB: " + this.Content + " " + this.Subject);
                }
                catch (Exception ex_) { utils.GrabaLogExcepcion(ex_); }
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public Boolean Send()
        {
            Utils utils = new Utils();
            try
            {
                switch (this.MessageType)
                {
                    case "chat":
                        if (this.AddToCache() == true)
                        {
                            if (this.AddToDb() == true)
                            {
                                return true;
                            }
                        }
                        return false;
                    case "ticket":
                        if (this.AddToDb() == true)
                        {
                            return true;
                        }
                        return false;
                    case "email":
                        Email email = new Email();
                        email.message = this;
                        email.send();
                        this.AddToDb();
                        return true;
                    case null:
                        return false;
                }
                return false;
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
                return false;
            }
        }

        public void loadConversersFromSpareData()
        {
            var converser_from = (from m in db.Conversers
                          .Include("Business")
                                  where m.UserName == this.From_UserName
                                  && m.Business.Domain == this.From_Domain
                                  select m).FirstOrDefault();
            this.From = converser_from;
            var converser_to = (from m in db.Conversers
                        .Include("Business")
                                where m.UserName == this.To_UserName
                                 && m.Business.Domain == this.To_Domain
                                select m).FirstOrDefault();
            this.To = converser_to;
        }

        public Message()
        {
            this.TimeStamp = DateTime.Now.ToUniversalTime();
            this.TimeStampSenderSending = DateTime.Now.ToUniversalTime();
            this.TimeStampSrvAccepted = DateTime.Now.ToUniversalTime();
            this.TimeStampSrvSending = DateTime.Now.ToUniversalTime();
            this.MessageType = "chat";
            this.Subject = null;
            this.Lang = "en";
        }

        public Message(NewMessage newmessage)
        {
            if (newmessage.db != null)
            {
                db = newmessage.db;
            }

            try
            {
                if (newmessage.From.Length > 0)
                {
                    this.From_Domain = newmessage.From.Split('@')[1].ToString();
                    this.From_UserName = newmessage.From.Split('@')[0].ToString();
                    this.From_FullName = newmessage.From_FullName;
                }

                if (newmessage.To.Length > 0)
                {
                    this.To_Domain = newmessage.To.Split('@')[1].ToString();
                    this.To_UserName = newmessage.To.Split('@')[0].ToString();
                }

                if (newmessage.CC != null)
                {
                    if (newmessage.CC.Length > 0)
                    {
                        this.CC = newmessage.CC;
                    }
                }

                this.MessageType = newmessage.MessageType;

                /*
                if (this.MessageType == "ticket")
                {
                    this.loadConversersFromSpareData();
                }
                */

                this.Content = newmessage.Content;
                this.Subject = newmessage.Subject;
                this.ClientID = newmessage._clientid;
                this.Status = 0;

                this.TimeStamp = DateTime.Now.ToUniversalTime();
                this.TimeStampSenderSending = DateTime.Now.ToUniversalTime();
                this.TimeStampSrvAccepted = DateTime.Now.ToUniversalTime();
                this.TimeStampSrvSending = DateTime.Now.ToUniversalTime();

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = new DateTime();

                //Siempre se recibe como UTC/GMT cuando se creo el mensaje..
                if (DateTime.TryParse(newmessage.TimeStamp, out loctime) == true)
                {
                    this.TimeStamp = loctime.ToUniversalTime();
                }

                //Tambien se recibe como UTC el momento en que el mensaje se intentó enviar arriba
                if (DateTime.TryParse(newmessage.TimeStampSenderSending, out loctime) == true)
                {
                    this.TimeStampSenderSending = loctime.ToUniversalTime();
                }

                //Y se crea como UTC el momento en que el mensaje ya está en el servidor... 
                this.TimeStampSrvAccepted = newmessage.TimeStampSrvAccepted;

                this.Lang = newmessage.Lang;

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
        }

        /*
        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID", ID);
            info.AddValue("TimeStamp", TimeStamp);
            info.AddValue("Sent", Sent);
            info.AddValue("Status", Status);
            info.AddValue("MessageType", Status);
            info.AddValue("Ubication", Ubication);
            info.AddValue("From", From);
            info.AddValue("To", To);
            info.AddValue("Subject", Subject);
            info.AddValue("Lang", Lang);
            info.AddValue("Content", Content);
            info.AddValue("CommSession", CommSession);
            info.AddValue("MeetingSession", MeetingSession);
        }

        #endregion  
        */

        [Display(Name = "Message ID")]
        public int ID { get; set; }

        [Display(Name = "CreatedOn TimeStamp")]
        public DateTime TimeStamp { get; set; }

        [Display(Name = "ClientSending TimeStamp")]
        public DateTime TimeStampSenderSending { get; set; }

        [Display(Name = "SrvAccepted TimeStamp")]
        public DateTime TimeStampSrvAccepted { get; set; }

        [Display(Name = "SrvSending TimeStamp")]
        public DateTime TimeStampSrvSending { get; set; }

        //Sent Codes
        //0 == No enviado
        //1 == Enviado
        public int Sent { get; set; }

        //Status Codes
        //0 == No leído
        //1 == Leido
        public int Status { get; set; }

        //"chat", "ticket", "email" ... "facebook", etc etc etc por ahora defecto "chat"
        public string MessageType { get; set; }

        public virtual Ubication Ubication { get; set; }

        //[Required]
        public virtual Converser From { get; set; }

        public string From_UserName { get; set; }
        public string From_FullName { get; set; }
        public string From_Domain { get; set; }

        //[Required]
        public virtual Converser To { get; set; }

        public string CC { get; set; }

        public string ClientID { get; set; }

        public string To_UserName { get; set; }
        public string To_Domain { get; set; }

        public string Subject { get; set; }

        [Required]
        public string Lang { get; set; }

        /*[Required]*/
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        public virtual CommSession CommSession { get; set; }
        public virtual MeetingSession MeetingSession { get; set; }
        //public string CommSessionID { get; set; }
    }
}