using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.ApplicationServer.Caching;
//using Microsoft.Web.Mvc.Resources;
using Microsoft.WindowsAzure.ServiceRuntime;
using vizzopWeb.Models;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.ComponentModel;

namespace vizzopWeb
{

    public class Thread_SendMsg
    {


        public Message message { get; set; }
        public NewMessage newmessage { get; set; }
        public string setticketstate { get; set; }
        public string mainUrl { get; set; }

        public Thread_SendMsg(NewMessage newmessage, string setticketstate, string mainUrl)
        {
            this.newmessage = newmessage;
            this.setticketstate = setticketstate;
            this.mainUrl = mainUrl;

            if ((this.mainUrl == null) || (this.mainUrl == "null"))
            {
                string scheme = HttpContext.Current.Request.Url.Scheme;
                string domain = HttpContext.Current.Request.Url.Host;
                int port = HttpContext.Current.Request.Url.Port;
                try
                {
                    domain = RoleEnvironment.GetConfigurationSettingValue("Domain");
                    port = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("Port"));
                    scheme = RoleEnvironment.GetConfigurationSettingValue("Scheme");
                }
                catch (Exception) { }
                this.mainUrl = scheme + @"://" + domain + ":" + port;
            }
        }

        public void DoThings()
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            try
            {
                this.message = new Message(newmessage);
                this.message.db = db;
                if (newmessage.commsessionid != null)
                {

                    int _commsessionid = Convert.ToInt32(newmessage.commsessionid);
                    var commsession = (from m in db.CommSessions
                                           .Include("Business")
                                           .Include("Client")
                                           .Include("Client.Business")
                                       where m.ID == _commsessionid
                                       select m).FirstOrDefault();

                    if (commsession != null)
                    {

                        if (message.Subject != "$#_closesession")
                        {
                            commsession.Status = 1;
                        }

                        message.CommSession = commsession;

                        if ((setticketstate != null) && (setticketstate != "null"))
                        {
                            //int _commsessionid = Convert.ToInt32(newmessage.commsessionid);
                            switch (setticketstate)
                            {
                                case "close":
                                    //db.Database.ExecuteSqlCommand("UPDATE CommSessionS SET STATUS=3 WHERE ID=" + _commsessionid);
                                    commsession.Status = 3;
                                    break;
                                case "open":
                                    //db.Database.ExecuteSqlCommand("UPDATE CommSessionS SET STATUS=1 WHERE ID=" + _commsessionid);
                                    commsession.Status = 1;
                                    break;
                            }
                        }

                        message.AddToDb();

                        if (message.MessageType == "ticket")
                        {
                            if ((newmessage.Content != null) && (newmessage.Content != "null"))
                            {
                                string checkurl = mainUrl + "/CommSession/Check/?id=" + commsession.ID + "&username=" + commsession.Client.UserName;

                                Email _email = new Email();

                                Message firstMessage = commsession.Messages.OrderBy(messages => messages.TimeStamp).FirstOrDefault();
                                string _content;

                                if (firstMessage != null)
                                {
                                    string[] args = { commsession.Client.Business.BusinessName, commsession.Client.FullName, firstMessage.Content, message.Content, checkurl };
                                    _content = utils.LocalizeLang("email_ticket_response_contents", newmessage.Lang, args);
                                }
                                else
                                {
                                    string[] args = { commsession.Client.Business.BusinessName, commsession.Client.FullName, "", message.Content, checkurl };
                                    _content = utils.LocalizeLang("email_ticket_response_contents", newmessage.Lang, args);
                                }
                                string[] args_ = { commsession.Client.Business.BusinessName };
                                string _subject = utils.LocalizeLang("email_ticket_response_subject", newmessage.Lang, args_);

                                _email.message = new Message();
                                _email.message.Lang = newmessage.Lang;
                                _email.message.Content = _content;
                                _email.message.From = commsession.Agents.FirstOrDefault().Converser;
                                _email.message.To = commsession.Client;
                                _email.message.To.Business = new Business();
                                _email.message.To.Business.Domain = commsession.Client.Business.Domain;
                                _email.message.Subject = _subject;
                                _email.withBcc = false;
                                _email.send();

                            }

                        }


                    }

                }
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
                return;
            }
        }

    }

    public class Thread_AuditMsg
    {

        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();
        private List<Dictionary<string, object>> arrDict = new List<Dictionary<string, object>>();

        public Thread_AuditMsg(string MsgCueAudit)
        {
            try
            {
                if ((MsgCueAudit == null) || (MsgCueAudit == ""))
                {
                    return;
                }

                this.arrDict = new JavaScriptSerializer().Deserialize<List<Dictionary<string, object>>>(MsgCueAudit);
            }
            catch (Exception ex)
            {
                utils.GrabaLog(Utils.NivelLog.info, ex.Message);
            }
        }

        public void DoThings()
        {
            try
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = new DateTime();
                foreach (Dictionary<string, object> dict in this.arrDict)
                {
                    /*
                        'timestamp': parseJsonDate(v.TimeStamp),
                        'timestampsendersending': parseJsonDate(v.TimeStampSenderSending),
                        'timestampsrvaccepted': parseJsonDate(v.TimeStampSrvAccepted),
                        'timestampsrcsending': parseJsonDate(v.TimeStampSrvSending),
                        'timestamprecipientaccepted': timestamprecipientaccepted.toJSON(),
                        'from': v.From.UserName + '@' + v.From.Business.Domain,
                        'to': v.To.UserName + '@' + v.From.Business.Domain,
                        'mainurl': vizzop.mainURL,
                        'messagetype': v.MessageType,
                        'commsessionid': v.CommSession.ID,
                        'subject': v.Subject,
                        'content': v.Content
                     */
                    MessageAudit messageaudit = new MessageAudit();
                    if (dict.ContainsKey("from"))
                    {
                        try
                        {
                            if (dict["timestamp"] != null)
                            {
                                //Siempre se recibe como UTC/GMT cuando se creo el mensaje..
                                if (DateTime.TryParse(dict["timestamp"].ToString(), out loctime) == true)
                                {
                                    messageaudit.TimeStamp = loctime.ToUniversalTime();
                                }
                            }
                            if (dict["timestampsendersending"] != null)
                            {
                                if (DateTime.TryParse(dict["timestampsendersending"].ToString(), out loctime) == true)
                                {
                                    messageaudit.TimeStampSenderSending = loctime.ToUniversalTime();
                                }
                            }
                            if (dict["timestampsrvaccepted"] != null)
                            {
                                if (DateTime.TryParse(dict["timestampsrvaccepted"].ToString(), out loctime) == true)
                                {
                                    messageaudit.TimeStampSrvAccepted = loctime.ToUniversalTime();
                                }
                            }
                            if (dict["timestampsrvsending"] != null)
                            {
                                if (DateTime.TryParse(dict["timestampsrvsending"].ToString(), out loctime) == true)
                                {
                                    messageaudit.TimeStampSrvSending = loctime.ToUniversalTime();
                                }
                            }
                            if (dict["timestamprecipientaccepted"] != null)
                            {
                                if (DateTime.TryParse(dict["timestamprecipientaccepted"].ToString(), out loctime) == true)
                                {
                                    messageaudit.TimeStampRecipientAccepted = loctime.ToUniversalTime();
                                }
                            }
                            if (dict["from"] != null)
                            {
                                var UserName = dict["from"].ToString().Split('@')[0].ToString();
                                var Domain = dict["from"].ToString().Split('@')[1].ToString();
                                var converser = (from m in db.Conversers.Include("Business").Include("Agent")
                                                 where m.UserName == UserName
                                                 && m.Business.Domain == Domain
                                                 select m).FirstOrDefault();
                                messageaudit.From = converser;
                            }
                            if (dict["to"] != null)
                            {
                                var UserName = dict["to"].ToString().Split('@')[0].ToString();
                                var Domain = dict["to"].ToString().Split('@')[1].ToString();
                                var converser = (from m in db.Conversers.Include("Business").Include("Agent")
                                                 where m.UserName == UserName
                                                 && m.Business.Domain == Domain
                                                 select m).FirstOrDefault();
                                messageaudit.To = converser;
                            }
                            if (dict["from"] != null)
                            {
                                var UserName = dict["from"].ToString().Split('@')[0].ToString();
                                var Domain = dict["from"].ToString().Split('@')[1].ToString();
                                var converser = (from m in db.Conversers.Include("Business").Include("Agent")
                                                 where m.UserName == UserName
                                                 && m.Business.Domain == Domain
                                                 select m).FirstOrDefault();
                                messageaudit.From = converser;
                            }
                            if (dict["mainurl"] != null)
                            {
                                messageaudit.MainURL = dict["mainurl"].ToString();
                            }
                            if (dict["commsessionid"] != null)
                            {
                                var commsessionid = Convert.ToInt32(dict["commsessionid"].ToString());
                                messageaudit.CommSession = (from m in db.CommSessions
                                                            where m.ID == commsessionid
                                                            select m).FirstOrDefault();
                            }
                            if (dict["subject"] != null)
                            {
                                messageaudit.Subject = dict["subject"].ToString();
                            }
                            if (dict["content"] != null)
                            {
                                messageaudit.Content = dict["content"].ToString();
                            }
                            if (dict["messagetype"] != null)
                            {
                                messageaudit.MessageType = dict["messagetype"].ToString();
                            }

                            if (messageaudit == null)
                            {
                                return;
                            }

                            try
                            {
                                if (messageaudit.Subject.StartsWith("$#_"))
                                {
                                    return;
                                }

                                db.MessageAudits.Add(messageaudit);
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException ex)
                            {
                                foreach (DbEntityValidationResult result in ex.EntityValidationErrors)
                                {
                                    utils.GrabaLog(Utils.NivelLog.error, result.ToString());
                                }
                            }
                        }
                        catch (Exception _ex)
                        {
                            utils.GrabaLog(Utils.NivelLog.info, _ex.Message);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
            }
        }

    }

}

namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class MessagesController : Controller
    {
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetAllMessagesFromCommSession(string UserName, string Password, string domain, string commsessionid, string callback)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            try
            {

                Converser converser = utils.GetConverserFromSystem(UserName, Password, domain);
                if (converser == null)
                {
                    return Json(false);
                }


                int _commsessionid = Convert.ToInt32(commsessionid);

                var session = (from m in db.CommSessions
                               where (m.Agents.Any(j => j.Converser.UserName == UserName && j.Converser.Business.Domain == domain))
                               && m.ID == _commsessionid
                               select m).FirstOrDefault();

                List<Message> returnmessages = new List<Message>();

                //Tengo que hacerlo asi porque si no entity me da referencias circulares... que mierda es esta?
                foreach (Message oldmsg in session.Messages.OrderBy(m => m.TimeStamp).ToList<Message>())
                {
                    try
                    {
                        if (oldmsg.Subject.StartsWith("$#_") == true)
                        {
                            continue;
                        }
                        Message returnmsg = utils.TransformMessageToSerializedProof(oldmsg);
                        returnmessages.Add(returnmsg);
                    }
                    catch (Exception ex)
                    {
                        utils.GrabaLogExcepcion(ex);
                    }
                }
                if (returnmessages.Count > 0)
                {

                    //session.Messages.ToList<Message>().ForEach(m => m.Status = 1);

                    db.Database.ExecuteSqlCommand("UPDATE MESSAGES SET STATUS=1 WHERE COMMSESSION_ID=" + session.ID);
                    //db.SaveChanges();

                    return Json(returnmessages);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
                return Json(false);
            }
        }

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetAllMessagesFromInterlocutor(string UserName, string Password, string Domain, string Interlocutor_UserName, string Interlocutor_Domain, string callback)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            try
            {
                /*
                var converser = (from m in db.Conversers
                                 where m.UserName == UserName
                                 && m.Password == Password
                                 && m.Active == true
                                 select m).FirstOrDefault();
                */

                Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain);
                if (converser == null)
                {
                    return Json(false);
                }

                var interlocutor = (from m in db.Conversers
                                    where m.UserName == Interlocutor_UserName && m.Business.Domain == Interlocutor_Domain
                                    select m).FirstOrDefault();
                if (interlocutor == null)
                {
                    return Json(false);
                }

                TimeZone localZone = TimeZone.CurrentTimeZone;
                converser.LastActive = localZone.ToUniversalTime(DateTime.Now);
                db.SaveChanges();

                var orig_messages = (from m in db.Messages
                                     where ((m.From.UserName == converser.UserName) && (m.To.UserName == interlocutor.UserName)) || ((m.From.UserName == interlocutor.UserName) && (m.To.UserName == converser.UserName))
                                     select m).OrderBy(m => m.TimeStamp).ToList();

                List<Message> messages = new List<Message>();
                if (orig_messages.Count > 0)
                {
                    messages = orig_messages.ToList<Message>();
                }

                List<Message> returnmessages = new List<Message>();
                //Tengo que hacerlo asi porque si no entity me da referencias circulares... que mierda es esta?
                foreach (Message oldmsg in messages)
                {
                    if (oldmsg.Content == "null")
                    {
                        continue;
                    }
                    if (oldmsg.Subject.StartsWith("$#_"))
                    {
                        continue;
                    }
                    Message returnmsg = utils.TransformMessageToSerializedProof(oldmsg);
                    returnmessages.Add(returnmsg);

                }

                if (returnmessages.Count > 0)
                {

                    messages.ForEach(m => m.Status = 1);
                    db.SaveChanges();
                    /*
                    foreach (var m in returnmessages)
                    {
                        m.Status = 1;
                        m.From.Password = null;
                        m.To.Password = null;
                    }*/

                    return Json(returnmessages);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
                return Json(false);
            }
        }


        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult CheckExternal(string username, string domain, string password, string url, string referrer, string callback, string SessionID, string CommSessionID, string WindowName, string MsgCueAudit)
        {
            using (var db = new vizzopContext())
            {
                Utils utils = new Utils(db);
                try
                {
                    List<Message> returnmessages = utils.CheckExternal(HttpContext, username, domain, password, url, referrer, SessionID, CommSessionID, WindowName, MsgCueAudit);

                    if (returnmessages.Count > 0)
                    {
                        return Json(returnmessages);
                    }
                    else
                    {
                        return Json(false);
                    }

                }
                catch (Exception e)
                {
                    utils.GrabaLogExcepcion(e);
                    return Json(false);
                }
            }
        }

        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetScreenCaptureByGuid(string UserName, string Domain, string Password, string GUID, string callback)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            try
            {

                if ((UserName == null) || (Domain == null))
                {
                    return Json(false);
                }

                Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain);
                if (converser == null)
                {
                    return Json(false);
                }

                ScreenCapture sc = null;

                sc = (from m in db.ScreenCaptures
                      where m.converser.ID == converser.ID &&
                      m.GUID == GUID
                      select m).FirstOrDefault();

                if (sc != null)
                {
                    var toReturn = new
                    {
                        Html = utils.ScrubHTML(sc.Blob),
                        Width = sc.Width,
                        Height = sc.Height,
                        ScrollTop = sc.ScrollTop,
                        ScrollLeft = sc.ScrollLeft,
                        GUID = sc.GUID
                    };

#if DEBUG
                    utils.GrabaLog(Utils.NivelLog.info, "Lanzando imagen a Phantom");
#endif

                    return Json(toReturn);

                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
            }
        }


        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult CheckNew(string WindowName, string callback)
        {
            Utils utils = new Utils();

            try
            {

                if (HttpContext.Session["converser"] == null)
                {
                    return null;
                }
                var converser = (Converser)HttpContext.Session["converser"];
                if (converser == null)
                {
                    return Json(false);
                }

                /*
                if ((UserName == null) || (Password == null) || (Domain == null))
                {
                    return Json(false);
                }

                Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain, db);
                if (converser == null) { return Json(false); }
                */


                //Y montamos la lista de mensajes que le vamos a devolver
                List<Message> returnmessages = new List<Message>();

                DateTime start_time = DateTime.Now;
                while ((returnmessages.Count == 0) && (DateTime.Now < start_time.AddSeconds(25)))
                {
                    returnmessages = utils.CheckNew(HttpContext, converser, WindowName);
                    Thread.Sleep(TimeSpan.FromMilliseconds(50));
                }

                if (returnmessages.Count > 0)
                {
                    return Json(returnmessages);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
            }
        }

        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult SendNewReview(string UserName, string Password, string Domain, string newReviewComment, string newReviewRating, string callback)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            try
            {
                if ((UserName == null) || (Password == null) || (Domain == null) || (newReviewComment == null) || (newReviewRating == null))
                {
                    return Json(false);
                }

                Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain);

                if (converser == null) { return Json(false); }

                Review newreview = new Review();
                newreview.Converser = converser;
                newreview.Comment = newReviewComment;
                newreview.Rating = Convert.ToInt16(newReviewRating);
                db.Reviews.Add(newreview);
                db.SaveChanges();

                try
                {
                    NewMessage newmessage = new NewMessage();
                    newmessage.From = "admin@vizzop";
                    newmessage.To = converser.UserName + "@" + converser.Business.Domain;
                    newmessage.Lang = utils.GetLang(HttpContext);
                    string[] args = { "Zentralized Ltd.", converser.FullName, newreview.Comment };
                    newmessage.Subject = utils.LocalizeLang("email_review_created_subject", newmessage.Lang, args);
                    newmessage.Content = utils.LocalizeLang("email_review_created_contents", newmessage.Lang, args);
                    newmessage.db = db;
                    newmessage.MessageType = "email";
                    Message message = new Message(newmessage);

                    var _conv = (from m in db.Conversers
                                 where m.UserName == "admin" && m.Business.Domain == "vizzop"
                                 select m).FirstOrDefault();

                    message.From = _conv;
                    message.To = converser;

                    Boolean result = message.Send();
                }
                catch (Exception _ex)
                {
                    utils.GrabaLogExcepcion(_ex);
                }

                try
                {
                    Sms sms = new Sms();
                    sms.message = "Nuevo Review " + converser.Business.BusinessName;
                    sms.phonenumber = "34655778343";
                    sms.send();
                }
                catch (Exception _ex)
                {
                    utils.GrabaLogExcepcion(_ex);
                }


                return Json(true);

            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);

                try
                {
                    string messageLog = "username: " + UserName + " password: " + Password + " domain: " + Domain + " newReviewComment: " + newReviewComment + " newReviewRating: " + newReviewRating;
                    utils.GrabaLog(Utils.NivelLog.error, messageLog);
                }
                catch (Exception ex_)
                {
                    utils.GrabaLogExcepcion(ex_);
                }

                return Json(false);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}