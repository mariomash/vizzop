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
        public ActionResult CheckExternal(string MsgLastID, string url, string referrer, string callback, string SessionID, string CommSessionID, string WindowName, string MsgCueAudit)
        {
            using (var db = new vizzopContext())
            {
                Utils utils = new Utils(db);
                try
                {
                    if ((MsgCueAudit != "") && (MsgCueAudit != null))
                    {
                        // Lanzamos el guardado de auditorias de mensaje en otro hilo...
                        Thread_AuditMsg oThread = new Thread_AuditMsg(MsgCueAudit);
                        Thread rCheck = new Thread(oThread.DoThings); //rCheck.Priority = ThreadPriority.Normal;
                        rCheck.Start();
                    }
                    //Y montamos la lista de mensajes que le vamos a devolver
                    List<Message> returnmessages = new List<Message>();


                    if (Session["converser"] == null)
                    {
                        return null;
                    }
                    var converser = (Converser)HttpContext.Session["converser"];
                    if (converser == null)
                    {
                        return Json(false);
                    }

                    /*
                    string tag = "agent";
                    List<DataCacheTag> Tags = new List<DataCacheTag>();
                    Tags.Add(new DataCacheTag(tag));
                    object result = SingletonCache.Instance.GetByTag(tag);
                    if (result != null)
                    {
                        IEnumerable<KeyValuePair<string, object>> Agents = (IEnumerable<KeyValuePair<string, object>>)result;
                        var agent = (Converser)(from m in Agents
                                                where ((Converser)m.Value).ID == converser.ID
                                                select m).FirstOrDefault().Value;
                    }
                    */

                    /*
                    converser.LastActive = DateTime.UtcNow;
                    Session["converser"] = converser;
                     * */
                    /*
                    Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }
                    */

                    //Solo Para agentes... permitimos una única sesion..
                    if ((converser.Agent != null) && (SessionID != null) && (SessionID != "null"))
                    {

                        /*
                         * Activamos el agente si es que tiene que estar activado...
                         */
                        string _key = converser.UserName + @"@" + converser.Business.Domain;
                        string tag = "agent";
                        List<DataCacheTag> Tags = new List<DataCacheTag>();
                        Tags.Add(new DataCacheTag(tag));
                        DataCacheLockHandle lockHandle;
                        object result = SingletonCache.Instance.GetWithLock(_key, out lockHandle);
                        if (result != null)
                        {
                            var agent = (Converser)result;
                            agent.LastActive = DateTime.UtcNow;
                            SingletonCache.Instance.InsertWithLockAndTags(_key, agent, lockHandle, Tags);
                        }

                        var ZenSession = (from m in db.ZenSessions
                                          where m.Converser.UserName == converser.UserName
                                          && m.Converser.Business.Domain == converser.Business.Domain
                                          && m.sessionID == SessionID
                                          select m).FirstOrDefault();
                        if (ZenSession == null)
                        {
                            Message returnmsg = new Message();
                            returnmsg.From = new Converser();
                            returnmsg.From.ID = 0;
                            returnmsg.From.UserName = "vizzop";
                            returnmsg.From.FullName = "";
                            returnmsg.From.Password = null;
                            returnmsg.From.Business = new Business();
                            returnmsg.From.Business.Domain = "vizzop";
                            returnmsg.To = new Converser();
                            returnmsg.To.ID = converser.ID;
                            returnmsg.To.UserName = converser.UserName;
                            returnmsg.To.FullName = converser.FullName;
                            returnmsg.To.Password = null;
                            returnmsg.To.Business = new Business();
                            returnmsg.To.Business.Domain = converser.Business.Domain;
                            returnmsg.Content = "";
                            returnmsg.Subject = "$#_forcestartsession";
                            returnmsg.db = null;
                            returnmsg.utils = null;
                            //returnmsg = utils.TransformMessageToSerializedProof(returnmsg);
                            returnmessages.Add(returnmsg);

                            return Json(returnmessages);
                        }
                    }

                    TimeZone localZone = TimeZone.CurrentTimeZone;
                    string[] languages = HttpContext.Request.UserLanguages;
                    string language = null;
                    if (languages != null && languages.Length != 0) { language = languages[0].ToLowerInvariant().Trim(); }

                    // If you want it formated in some other way.
                    var headers = "{";
                    foreach (var key in HttpContext.Request.Headers.AllKeys)
                        headers += "'" + key + "':'" + Request.Headers[key] + "',";

                    headers = headers.TrimEnd(',') + "}";

                    string useragent = HttpContext.Request.UserAgent;

                    string sIP = HttpContext.Request.ServerVariables["HTTP_CLIENT_IP"];
                    if (string.IsNullOrEmpty(sIP) == true) { sIP = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]; }
                    if (string.IsNullOrEmpty(sIP) == true) { sIP = HttpContext.Request.ServerVariables["REMOTE_ADDR"]; }
                    if (string.IsNullOrEmpty(sIP) == true)
                    {
                        utils.GrabaLog(Utils.NivelLog.error, "No IP to Track");
                    }
                    else
                    {
                        sIP = sIP.Split(',')[0];

                        //Y trackeamos la visita.. solo para clientes!!
                        if (converser.Agent == null)
                        {
                            try
                            {
                                if ((url != null) && (referrer != null) && (converser != null))
                                {
                                    Status returnStatus = utils.TrackPageView(converser, url, referrer, language, useragent, sIP, headers, WindowName);
                                    /*
                                    int number = 100;
                                    while (number > 0)
                                    {
                                        Status returnStatus = utils.TrackPageView(converser, url, referrer, language, useragent, sIP, headers, WindowName + number.ToString());
                                        number--;
                                    }
                                     */
                                }
                            }
                            catch (Exception ex)
                            {
                                utils.GrabaLogExcepcion(ex);
                            }
                        }

                    }

                    DateTime loctime = DateTime.Now.AddMinutes(-5);
                    DateTime loctimeUTC = localZone.ToUniversalTime(loctime);

                    DateTime loctime_AvailableTimeout = DateTime.Now.AddSeconds(-15);
                    DateTime loctimeUTC_AvailableTimeout = localZone.ToUniversalTime(loctime_AvailableTimeout);

                    try
                    {
                        if ((converser.Agent == null) && ((CommSessionID == null) || (CommSessionID == "null") || (CommSessionID == "")))
                        {
                            /*
                             * Encontrar todos los Agentes de este ApiKey...
                             * Si estan activos o no.. devuelvo un $#_activeagents o un $#_noactiveagents
                             */
                            /*
                            var agents = (from m in db.Conversers
                                          where m.Business.ID == converser.Business.ID
                                          && m.Agent != null
                                          && m.LastActive > loctimeUTC_AvailableTimeout
                                          && m.Active == true
                                          select m).ToList<Converser>();
                            */

                            Message agentsmsg = new Message();
                            agentsmsg.Content = null;
                            agentsmsg.From = new Converser();
                            agentsmsg.From.ID = 0;
                            agentsmsg.From.UserName = "vizzop";
                            agentsmsg.From.FullName = "";
                            agentsmsg.From.Password = null;
                            agentsmsg.From.Business = new Business();
                            agentsmsg.From.Business.Domain = "vizzop";
                            agentsmsg.ID = 0;
                            agentsmsg.To = new Converser();
                            agentsmsg.To.ID = converser.ID;
                            agentsmsg.To.UserName = converser.UserName;
                            agentsmsg.To.FullName = converser.FullName;
                            agentsmsg.To.Password = null;
                            agentsmsg.To.Business = new Business();
                            agentsmsg.To.Business.Domain = converser.Business.Domain;
                            agentsmsg.ID = 0;
                            agentsmsg.Status = 1;
                            agentsmsg.CommSession = new CommSession();
                            agentsmsg.CommSession.ID = 0;

                            string tag = "agent";
                            List<DataCacheTag> Tags = new List<DataCacheTag>();
                            Tags.Add(new DataCacheTag(tag));
                            object result = SingletonCache.Instance.GetByTag(tag);
                            if (result != null)
                            {
                                IEnumerable<KeyValuePair<string, object>> Agents = (IEnumerable<KeyValuePair<string, object>>)result;
                                var agents = (from m in Agents
                                              where ((Converser)m.Value).LastActive > loctimeUTC_AvailableTimeout &&
                                               ((Converser)m.Value).Business.ID > converser.Business.ID
                                              select m);
                                if (agents.Count() > 0)
                                {
                                    agentsmsg.Subject = "$#_activeagents";
                                }
                                else
                                {
                                    agentsmsg.Subject = "$#_noactiveagents";
                                }
                            }

                            //agentsmsg = utils.TransformMessageToSerializedProof(agentsmsg);
                            agentsmsg.db = null;
                            agentsmsg.utils = null;

                            returnmessages.Add(agentsmsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        utils.GrabaLogExcepcion(ex);
                    }

                    /* Encontrar todas las sesiones con este converser .. para available o no y dimensiones*/
                    List<CommSession> sessiones_de_este_converser = new List<CommSession>();
                    if (converser.Agent != null)
                    {
                        sessiones_de_este_converser = (from m in db.CommSessions
                                                           .Include("Client")
                                                           .Include("Client.Business")
                                                           .Include("Business")
                                                       where m.Agents.Any(j => j.Converser.UserName == converser.UserName && j.Converser.Business.Domain == converser.Business.Domain)
                                                       && (m.Status == 1) //m.Status == 0 ||
                                                       && m.Messages.Count > 0
                                                       && m.Messages.Any(j => j.TimeStamp > loctimeUTC)
                                                       select m).ToList<CommSession>();
                    }
                    else
                    {
                        if ((CommSessionID == null) || (CommSessionID == "null") || (CommSessionID == ""))
                        {
                            sessiones_de_este_converser = (from m in db.CommSessions
                                                               .Include("Client")
                                                               .Include("Client.Business")
                                                               .Include("Business")
                                                           where (m.Client.UserName == converser.UserName && m.Client.Business.Domain == converser.Business.Domain)
                                                           && (m.Status == 1) //m.Status == 0 || 
                                                           && m.Messages.Count > 0
                                                           && m.Messages.Any(j => j.TimeStamp > loctimeUTC)
                                                           select m).ToList<CommSession>();
                        }
                        else
                        {
                            int _CommSessionID = Convert.ToInt32(CommSessionID);
                            sessiones_de_este_converser = (from m in db.CommSessions
                                                           where m.ID == _CommSessionID
                                                           select m).ToList<CommSession>();
                        }
                    }

                    foreach (CommSession c in sessiones_de_este_converser)
                    {
                        string anon_client = utils.LocalizeLang("anon_client", language, null);

                        //Ahora enviar un available o _notavailable segun el interlocutor esté inactivo más de X segundos 
                        try
                        {
                            Converser interlocutor = new Converser();
                            if (converser.Agent != null)
                            {
                                interlocutor = c.Client;
                            }
                            else
                            {
                                if (c.Agents.Count > 0)
                                {
                                    interlocutor = c.Agents.FirstOrDefault().Converser;
                                }
                            }
                            if (interlocutor.UserName == null)
                            {
                                continue;
                            }
                            string fullname = anon_client;
                            if (interlocutor.FullName != null)
                            {
                                fullname = interlocutor.FullName;
                            }

                            Message returnmsg = new Message();
                            returnmsg.Content = null;
                            returnmsg.From = new Converser();
                            returnmsg.From.ID = interlocutor.ID;
                            returnmsg.From.UserName = interlocutor.UserName;
                            returnmsg.From.Business = new Business();
                            returnmsg.From.Business.Domain = interlocutor.Business.Domain;
                            returnmsg.From.FullName = fullname;
                            returnmsg.From.Password = null;
                            returnmsg.ID = 0;
                            returnmsg.To = new Converser();
                            returnmsg.To.ID = converser.ID;
                            returnmsg.To.UserName = converser.UserName;
                            returnmsg.To.Business = new Business();
                            returnmsg.To.Business.Domain = converser.Business.Domain;
                            returnmsg.To.FullName = converser.FullName;
                            returnmsg.To.Password = null;
                            returnmsg.ID = 0;
                            returnmsg.Status = 1;
                            returnmsg.CommSession = new CommSession();
                            returnmsg.CommSession.ID = c.ID;

                            if ((interlocutor.LastActive > loctimeUTC_AvailableTimeout) && (interlocutor.Active == true))
                            {
                                returnmsg.Subject = "$#_available";
                            }
                            else
                            {
                                returnmsg.Subject = "$#_notavailable";
                            }

                            returnmsg.db = null;
                            returnmsg.utils = null;
                            //returnmsg = utils.TransformMessageToSerializedProof(returnmsg);

                            returnmessages.Add(returnmsg);
                        }
                        catch (Exception ex)
                        {
                            utils.GrabaLogExcepcion(ex);
                        }
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
        public ActionResult CheckCaptureControl(string UserName, string Domain, string WindowName, string GUID, string callback)
        {
            Utils utils = new Utils();

            try
            {

                if ((UserName == null) || (Domain == null))
                {
                    return Json(false);
                }

                DateTime start_time = DateTime.Now;

                WebLocation weblocation = null;
                while ((weblocation == null) && (DateTime.Now < start_time.AddSeconds(25)))
                {

                    //List<WebLocation> WebLocations = new List<WebLocation>();

                    string region = "weblocations";
                    object result = SingletonCache.Instance.GetAllInRegion(region);
                    if (result != null)
                    {
                        IEnumerable<KeyValuePair<string, object>> WebLocations = (IEnumerable<KeyValuePair<string, object>>)result;

                        //TimeZone localZone = TimeZone.CurrentTimeZone;
                        //DateTime loctime = localZone.ToUniversalTime(DateTime.Now.AddSeconds(-30));

                        var obj = (from m in WebLocations
                                   //where m.TimeStamp_Last > loctime &&
                                   where ((WebLocation)m.Value).UserName == UserName &&
                                   ((WebLocation)m.Value).Domain == Domain &&
                                   ((WebLocation)m.Value).WindowName == WindowName &&
                                   ((WebLocation)m.Value).ScreenCapture != null
                                   select m).OrderByDescending(m => ((WebLocation)m.Value).TimeStamp_Last).FirstOrDefault();
                        weblocation = (WebLocation)obj.Value;
                    }

                    if (weblocation != null)
                    {
                        if ((weblocation.ScreenCapture.GUID == GUID) || (weblocation.ScreenCapture.Blob == null))
                        {
                            weblocation = null;
                            Thread.Sleep(TimeSpan.FromMilliseconds(250));
                        }
                    }

                }

                if (weblocation == null)
                {
                    return Json(false);
                }
                else
                {
                    var CompleteHtml = utils.ScrubHTML(weblocation.CompleteHtml);

                    var toReturn = new
                    {
                        Html = CompleteHtml,
                        Blob = weblocation.ScreenCapture.Blob,
                        UserName = UserName,
                        Domain = Domain,
                        GUID = weblocation.ScreenCapture.GUID,
                        WindowName = weblocation.ScreenCapture.WindowName,
                        Width = weblocation.ScreenCapture.Width,
                        Height = weblocation.ScreenCapture.Height,
                        ScrollTop = weblocation.ScreenCapture.ScrollTop,
                        ScrollLeft = weblocation.ScreenCapture.ScrollLeft
                    };

                    /*
#if DEBUG
                    utils.GrabaLog(Utils.NivelLog.info, "Lanzando imagen a Phantom");
#endif
                    */

                    return Json(toReturn);
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
                List<Message> Messages = new List<Message>();
                List<Message> messages = new List<Message>();
                List<Message> returnmessages = new List<Message>();

                string lang = utils.GetLang(HttpContext);
                DateTime start_time = DateTime.Now;
                while ((messages.Count == 0) && (DateTime.Now < start_time.AddSeconds(25)))
                {
                    try
                    {
                        try
                        {
                            string key = "messages_to_" + converser.UserName + "@" + converser.Business.Domain;


                            DataCacheLockHandle lockHandle;
                            object result = SingletonCache.Instance.GetWithLock(key, out lockHandle);
                            //object result = SingletonCache.Instance.Get(key);
                            if (result != null)
                            {
                                Messages = (List<Message>)result;
                            }

                            foreach (var m in Messages)
                            {
                                if (m.WindowNamesCollected == null)
                                {
                                    m.WindowNamesCollected = new List<string>();
                                }
                                if (m.WindowNamesCollected.Contains(WindowName) == false)
                                {
                                    messages.Add(m);
                                    m.WindowNamesCollected.Add(WindowName);
                                }
                            }
                            SingletonCache.Instance.InsertWithLock(key, Messages, lockHandle);
                        }
                        catch (Exception ex__)
                        {
                            utils.GrabaLogExcepcion(ex__);
                        }
                        foreach (Message m in messages)
                        {
                            try
                            {
                                if ((m.Content == null) && (m.Subject == null))
                                {
                                    continue;
                                }
                                if (m.From == null)
                                {
                                    if ((m.From_UserName != null) && (m.From_Domain != null))
                                    {
                                        m.From = new Converser();
                                        m.From.UserName = m.From_UserName;
                                        m.From.FullName = m.From_FullName;
                                        m.From.Business = new Business();
                                        m.From.Business.Domain = m.From_Domain;
                                    }
                                }
                                if (m.To == null)
                                {
                                    if ((m.To_UserName != null) && (m.To_Domain != null))
                                    {
                                        m.To = new Converser();
                                        m.To.UserName = m.To_UserName;
                                        m.To.Business = new Business();
                                        m.To.Business.Domain = m.To_Domain;
                                    }
                                }
                                if (m.From.FullName == null)
                                {
                                    string[] args = { m.From.IP };
                                    m.From.FullName = utils.LocalizeLang("client", lang, args);
                                }
                                m.From.Password = null;
                                m.To.Password = null;

                                //Ponemos esto en el momento de hoy...luego lo arrastraremos
                                m.TimeStampSrvSending = DateTime.Now.ToUniversalTime();

                                returnmessages.Add(utils.TransformMessageToSerializedProof(m));
                            }
                            catch (Exception ex__)
                            {
                                utils.GrabaLogExcepcion(ex__);
                            }
                        }
                    }
                    catch (Exception ex_)
                    {
                        utils.GrabaLogExcepcion(ex_);
                    }
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