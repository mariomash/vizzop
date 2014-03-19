using System;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.Web.Mvc.Resources;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.ServiceRuntime;
using OpenTok;
using vizzopWeb.Models;
using System.Threading.Tasks;

namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class CommController : Controller
    {

        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

        /*
        [WebApiEnabled]
         */

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetCommSessions(string password, string username, string domain, string callback)
        {
            try
            {
                Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                if (converser == null)
                {
                    return Json(false);
                }

                List<CommSession> sessions = new List<CommSession>();

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime start_time = DateTime.Now;

                while ((sessions.Count == 0) && (DateTime.Now < start_time.AddSeconds(15)))
                {
                    TimeZone localZoneIn = TimeZone.CurrentTimeZone;
                    DateTime localTimeUTCIn = localZone.ToUniversalTime(DateTime.Now.AddMinutes(-60));
                    if (converser.Agent != null)
                    {
                        var orig_sessions = (from m in db.CommSessions
                                                 .Include("Client")
                                                 .Include("Client.Business")
                                                 .Include("Business")
                                                 .Include("Messages.From")
                                                 .Include("Messages.From.Business")
                                                 .Include("Messages.To")
                                                 .Include("Messages.To.Business")
                                                 .Include("Messages.CommSession")
                                             where m.Client.Business.ID == converser.Business.ID
                                             where m.Agents.Any(o => o.Converser.UserName == converser.UserName)
                                                 //&& ((m.Status == 0) || (m.Status == 1))
                                             && m.Status == 0
                                             && m.SessionType == "chat"
                                             select m);

                        if (orig_sessions.Count() > 0)
                        {
                            sessions = orig_sessions.ToList<CommSession>();
                        }
                    }
                    else
                    {
                        var orig_sessions = (from m in db.CommSessions
                                                 .Include("Client")
                                                 .Include("Client.Business")
                                                 .Include("Business")
                                                 .Include("Messages.From")
                                                 .Include("Messages.From.Business")
                                                 .Include("Messages.To")
                                                 .Include("Messages.To.Business")
                                                 .Include("Messages.CommSession")
                                             where m.Client.UserName == converser.UserName
                                             && ((m.Status == 0) || (m.Status == 1))
                                             && m.SessionType == "chat"
                                             && m.Messages.Any(j => j.TimeStamp > localTimeUTCIn)
                                             select m);

                        if (orig_sessions.Count() > 0)
                        {
                            sessions = orig_sessions.ToList<CommSession>();
                        }
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }

                List<CommSession> returnsessions = new List<CommSession>();

                if (sessions.Count > 0)
                {
                    //Tengo que hacerlo asi porque si no entity me da referencias circulares... que mierda es esta?
                    foreach (CommSession selected_session in sessions)
                    {

                        CommSession return_session = new CommSession();
                        return_session.ID = selected_session.ID;
                        return_session.Status = selected_session.Status;
                        return_session.SessionType = selected_session.SessionType;
                        return_session.Client = new Converser();
                        Converser conv = new Converser();
                        conv.UserName = selected_session.Client.UserName;
                        conv.LastActive = DateTime.Now.ToUniversalTime();
                        conv.ID = selected_session.Client.ID;
                        conv.FullName = selected_session.Client.FullName;
                        conv.IP = selected_session.Client.IP;
                        conv.LangISO = selected_session.Client.LangISO;
                        conv.UserAgent = selected_session.Client.UserAgent;
                        conv.Business = new Business();
                        conv.Business.Domain = selected_session.Business.Domain;
                        return_session.Client = conv;

                        return_session.Messages = new List<Message>();

                        //Tengo que hacerlo asi porque si no entity me da referencias circulares... que mierda es esta?
                        foreach (Message oldmsg in selected_session.Messages.OrderBy(m => m.TimeStamp).ToList<Message>())
                        {
                            try
                            {
                                if (oldmsg.Subject.StartsWith("$#_") == true)
                                {
                                    continue;
                                }

                                if ((from m in return_session.Messages
                                     where m.ClientID == oldmsg.ClientID
                                     select m).Count() > 0)
                                {
                                    continue;
                                }

                                oldmsg.From.Password = null;
                                oldmsg.To.Password = null;
                                return_session.Messages.Add(utils.TransformMessageToSerializedProof(oldmsg));
                            }
                            catch (Exception ex)
                            {
                                utils.GrabaLogExcepcion(ex);
                            }
                        }
                        /*
                        if (return_session.Messages.Count > 0)
                        {
                            db.Database.ExecuteSqlCommand("UPDATE MESSAGES SET STATUS=1 WHERE COMMSESSION_ID=" + return_session.ID);
                        }
                         * */
                        returnsessions.Add(return_session);
                    }

                    if (returnsessions.Count > 0)
                    {
                        return Json(returnsessions);
                    }
                    else
                    {
                        return Json(false);
                    }

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

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetOpenTokSessionID(string UserName, string Password, string Domain, string commsessionid, string callback)
        {
            try
            {
                Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain, db);
                if (converser == null)
                {
                    return Json(false);
                }

                string opentoksessionID = null;

                OpenTokSDK opentok = new OpenTokSDK();

                CommSession session = new CommSession();

                if ((commsessionid != null) && (commsessionid != "null"))
                {
                    int _commsessionid = Convert.ToInt32(commsessionid);
                    session = (CommSession)(from m in db.CommSessions
                                                .Include("Client")
                                                .Include("Client.Business")
                                                .Include("Business")
                                                .Include("Messages")
                                                .Include("Agents")
                                                .Include("Messages.To")
                                            //where ((m.Agents.Any(j => j.Converser.UserName == UserName && j.Converser.Password == Password)) || ((m.Client.UserName == UserName) && (m.Client.Password == Password)))
                                            where m.ID == _commsessionid
                                            select m).FirstOrDefault();
                    if (session != null)
                    {
                        opentoksessionID = session.OpenTokSessionID;
                    }
                }

                if (session == null)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

                if (opentoksessionID == null)
                {
                    Dictionary<string, object> options = new Dictionary<string, object>();
                    options.Add(SessionPropertyConstants.P2P_PREFERENCE, "enabled");
                    opentoksessionID = opentok.CreateSession(HttpContext.Request.ServerVariables["REMOTE_ADDR"], options);
                    session.OpenTokSessionID = opentoksessionID;
                    db.SaveChanges();
                }

                string token = opentok.GenerateToken(opentoksessionID);

                return Json(new
                {
                    OpenTokSessionID = opentoksessionID,
                    OpenTokToken = token
                }, JsonRequestBehavior.AllowGet);

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
        public ActionResult GetAllDetailsFromCommSession(string UserName, string Password, string Domain, string commsessionid, string callback)
        {
            try
            {
                Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain, db);
                if (converser == null)
                {
                    return Json(false);
                }

                int _commsessionid = Convert.ToInt32(commsessionid);

                CommSession session = null;
                if (converser.Business.Domain.ToLowerInvariant() == "vizzop")
                {
                    session = (from m in db.CommSessions
                                         .Include("Client")
                                         .Include("Client.Business")
                                         .Include("Business")
                                         .Include("Messages")
                                         .Include("Agents")
                                         .Include("Messages.To")
                                         .Include("Messages.To.Business")
                                         .Include("Messages.From")
                                         .Include("Messages.From.Business")
                               where m.ID == _commsessionid
                               select m).FirstOrDefault();
                }
                else
                {
                    session = (from m in db.CommSessions
                                      .Include("Client")
                                      .Include("Client.Business")
                                      .Include("Business")
                                      .Include("Messages")
                                      .Include("Agents")
                                      .Include("Messages.To")
                                      .Include("Messages.To.Business")
                                      .Include("Messages.From")
                                      .Include("Messages.From.Business")
                               where m.Business.ID == converser.Business.ID
                               && m.ID == _commsessionid
                               select m).FirstOrDefault();
                }
                if (session == null)
                {
                    return Json(false);
                }

                List<Message> returnmessages = new List<Message>();

                if (session != null)
                {
                    if (session.Messages != null)
                    {
                        //Tengo que hacerlo asi porque si no entity me da referencias circulares... que mierda es esta?
                        foreach (Message oldmsg in session.Messages.OrderBy(m => m.TimeStamp).ToList<Message>())
                        {
                            if (oldmsg.Subject.StartsWith("$#_") == false)
                            {
                                oldmsg.From.Password = null;
                                oldmsg.To.Password = null;
                                returnmessages.Add(utils.TransformMessageToSerializedProof(oldmsg));
                            }
                        }
                    }
                }
                if (returnmessages.Count > 0)
                {
                    db.Database.ExecuteSqlCommand("UPDATE MESSAGES SET STATUS=1 WHERE COMMSESSION_ID=" + session.ID);
                }

                CommSession returnsession = new CommSession();
                returnsession.Messages = returnmessages;
                returnsession.ID = session.ID;

                returnsession.Client = new Converser();
                returnsession.Client.ID = session.Client.ID;
                returnsession.Client.UserName = session.Client.UserName;
                returnsession.Client.FullName = session.Client.FullName;
                returnsession.Client.Business = new Business();
                returnsession.Client.Business.Domain = session.Client.Business.Domain;
                returnsession.Client.LangISO = session.Client.LangISO;
                returnsession.Client.IP = session.Client.IP;
                returnsession.Client.UserAgent = session.Client.UserAgent;

                returnsession.LockedBy = new Converser();
                if (session.LockedBy == null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE COMMSESSIONS SET LOCKEDBY_ID=" + converser.ID + " WHERE ID=" + session.ID);
                    returnsession.LockedBy.ID = converser.ID;
                    returnsession.LockedBy.UserName = converser.UserName;
                    returnsession.LockedBy.FullName = converser.FullName;
                }
                else
                {
                    returnsession.LockedBy.ID = session.LockedBy.ID;
                    returnsession.LockedBy.UserName = session.LockedBy.UserName;
                    returnsession.LockedBy.FullName = session.LockedBy.FullName;
                }

                returnsession.Comments = session.Comments;
                returnsession.CreatedOn = session.CreatedOn;
                returnsession.SessionType = session.SessionType;
                returnsession.Status = session.Status;

                WebLocationDataTables returnlocation = new WebLocationDataTables();
                try
                {

                    var current_location = (from m in db.WebLocations
                                            where m.Converser.ID == returnsession.Client.ID
                                            orderby m.ID
                                            select m).FirstOrDefault();

                    if (current_location != null)
                    {
                        returnlocation.ID = current_location.ID;
                        returnlocation.Url = current_location.Url;
                        returnlocation.Referrer = current_location.Referrer;
                        returnlocation.IP = current_location.IP;
                        returnlocation.Lang = current_location.Lang;
                        returnlocation.Ubication = current_location.Ubication;
                        returnlocation.UserAgent = current_location.UserAgent;
                        returnlocation.TimeStamp = current_location.TimeStamp_Last;
                        returnlocation.LastViewed = utils.GetPrettyDate(current_location.TimeStamp_Last).ToString();
                        returnlocation.UserName = current_location.Converser.UserName;
                        returnlocation.Domain = current_location.Converser.Business.Domain;
                        returnlocation.Password = current_location.Converser.Password;
                        returnlocation.FullName = current_location.Converser.FullName != null ? current_location.Converser.FullName : "Anonymous";
                    }

                }
                catch (Exception _e)
                {
                    utils.GrabaLogExcepcion(_e);
                }


                //return Json(returnsession);


                return Json(new
                {
                    Session = returnsession,
                    Location = returnlocation
                }, JsonRequestBehavior.AllowGet);
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
        public ActionResult ApproveCommRequest(string password, string username, string domain, string commsessionid, string callback)
        {
            try
            {
                Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                if (converser == null)
                {
                    return Json(false);
                }

                int _commsessionid = Convert.ToInt32(commsessionid);
                var commsession = (from m in db.CommSessions
                                       .Include("Business")
                                       .Include("Client")
                                       .Include("Client.Business")
                                   where m.ID == _commsessionid
                                   && m.Client.Business.ID == converser.Business.ID
                                   //&& m.Agents.Any(o => o.Converser.ID == converser.ID)
                                   select m).FirstOrDefault();

                if (commsession == null)
                {
                    //Esto es que ya la habían aprobado antes... se nos han adelantado!! nos quitaron de la lista..
                    return Json(false);
                }

                //Primero creamos la commsession con todos los agentes... 
                // - conforme van denegando se quitan de la lista, cuando queda solo uno además al dejar la lista a 0 se cambia el status de la sesion a 2 (denegada)
                // - Y si alguno aprueba... se deja en la lista solo a ese y se marca como 1 (aprobada)

                commsession.Agents = new List<Agent>();
                commsession.Agents.Add(converser.Agent);
                commsession.Status = 1;
                db.SaveChanges();

                //db.Database.ExecuteSqlCommand("UPDATE COMMSESSIONS SET STATUS=1 WHERE ID=" + commsession.ID);

                return Json(true);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
                //return null;
            }
        }

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult DenyCommRequest(string password, string username, string domain, string commsessionid, string callback)
        {
            try
            {
                Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                if (converser == null)
                {
                    return Json(false);
                }

                int _commsessionid = Convert.ToInt32(commsessionid);
                var commsession = (from m in db.CommSessions
                                       .Include("Business")
                                       .Include("Client")
                                       .Include("Client.Business")
                                   where m.ID == _commsessionid
                                   && m.Client.Business.ID == converser.Business.ID
                                   //&& m.Agents.Any(o => o.Converser.ID == converser.ID)
                                   select m).FirstOrDefault();

                if (commsession == null)
                {
                    return Json(true);
                }

                var agent_to_pop = (from m in db.Agents.Include("Converser") //.Include("Converser.Business")
                                    where m.Converser.ID == converser.ID
                                    select m).FirstOrDefault();

                commsession.Agents = commsession.Agents.Where(x => x.Converser.UserName != converser.UserName).ToList();
                if (commsession.Agents.Count == 0)
                {
                    commsession.Status = 2;
                }
                db.SaveChanges();
                return Json(true);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(true);
            }
        }

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult FinalizeCommRequest(string password, string username, string domain, string commsessionid, string callback)
        {
            try
            {
                Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                if (converser == null)
                {
                    return Json(false);
                }

                int _commsessionid = Convert.ToInt32(commsessionid);
                CommSession commsession = (from m in db.CommSessions
                                               .Include("Business")
                                               .Include("Client")
                                               .Include("Client.Business")
                                           where m.ID == _commsessionid
                                           && (m.Client.ID == converser.ID || m.Agents.Any(o => o.Converser.ID == converser.ID))
                                           //&& ((m.Client.UserName == converser.UserName && m.Client.Password == converser.Password) || m.Agents.Any(o => o.Converser.UserName == converser.UserName && o.Converser.Password == converser.Password))
                                           select m).FirstOrDefault();
                //commsession.Status = 3;
                //db.SaveChanges();

                if (commsession.ID == _commsessionid)
                {
                    db.Database.ExecuteSqlCommand("UPDATE COMMSESSIONS SET STATUS=3 WHERE ID=" + commsession.ID);

                    //Al cliente
                    NewMessage newmessage = new NewMessage();
                    Converser agent = commsession.Agents.FirstOrDefault().Converser;
                    newmessage.From = agent.UserName + "@" + agent.Business.Domain;
                    Converser client = commsession.Client;
                    newmessage.From_FullName = agent.FullName;
                    newmessage.To = client.UserName + "@" + client.Business.Domain;
                    newmessage.Subject = "$#_closesession";
                    newmessage.Content = commsession.ID.ToString();
                    newmessage._clientid = null;
                    newmessage._status = null;
                    newmessage.TimeStamp = null;
                    newmessage.commsessionid = commsession.ID.ToString();
                    newmessage.Lang = utils.GetLang(HttpContext);
                    newmessage.MessageType = "chat";
                    // Lanzamos el msg en otro hilo...
                    Message message = new Message(newmessage);
                    message.CommSession = new CommSession();
                    message.CommSession.ID = Convert.ToInt32(newmessage.commsessionid);
                    if (message.AddToCache() == true)
                    {
                        // Lanzamos el msg en otro hilo...
                        Thread_SendMsg oThread = new Thread_SendMsg(newmessage, null, null);
                        Thread rSend = new Thread(oThread.DoThings);
                        //rSend.Priority = ThreadPriority.BelowNormal;
                        rSend.Start();
                    }

                    NewMessage newmessage2 = new NewMessage();
                    newmessage2.From = client.UserName + "@" + client.Business.Domain;
                    newmessage2.From_FullName = client.FullName;
                    newmessage2.To = agent.UserName + "@" + agent.Business.Domain;
                    newmessage2.Subject = "$#_closesession";
                    newmessage2.Content = commsession.ID.ToString();
                    newmessage2._clientid = null;
                    newmessage2._status = null;
                    newmessage2.TimeStamp = null;
                    newmessage2.commsessionid = commsession.ID.ToString();
                    newmessage2.Lang = utils.GetLang(HttpContext);
                    newmessage2.MessageType = "chat";
                    Message message2 = new Message(newmessage2);
                    message2.CommSession = new CommSession();
                    message2.CommSession.ID = Convert.ToInt32(newmessage2.commsessionid);
                    if (message2.AddToCache() == true)
                    {
                        // Lanzamos el msg en otro hilo...
                        Thread_SendMsg oThread = new Thread_SendMsg(newmessage2, null, null);
                        Thread rSend = new Thread(oThread.DoThings);
                        //rSend.Priority = ThreadPriority.BelowNormal;
                        rSend.Start();
                    }

                    return Json(true);

                }
                else
                {
                    return Json(true);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
                //return null;
            }
        }

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Requestcommsessionid(string callback, string username, string password, string domain, string agent_password, string agent_username, string agent_domain, string fullname)
        {
            try
            {

                string sIP = utils.GetIP(HttpContext);
                string language = utils.GetLang(HttpContext);
                string useragent = HttpContext.Request.UserAgent;

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = DateTime.Now;
                DateTime loctimeUTC = localZone.ToUniversalTime(loctime);

                CommSession commsession = null;

                //Si ya sabemos todos los implicados...o sea que esto la inicia un agente... la intentamos buscar primero
                if ((agent_username != null) && (agent_password != null) && (agent_domain != null))
                {
                    commsession = (from m in db.CommSessions
                                       .Include("Client")
                                       .Include("Business")
                                       .Include("Client.Business")
                                   where (m.Client.UserName == username) &&
                                   m.Agents.Any(o => o.Converser.UserName == agent_username &&
                                       o.Converser.Password == agent_password &&
                                       o.Converser.Business.Domain == agent_domain)
                                   select m).FirstOrDefault();
                }

                if (commsession == null)
                {
                    //Si no ha sido fructifera la búsqueda (porque no era un agente) buscamos una que ya exista para entrar ahí
                    commsession = (from m in db.CommSessions
                                       .Include("Client")
                                       .Include("Business")
                                       .Include("Client.Business")
                                   where (m.Client.UserName == username) &&
                                   (m.Client.Password == password) &&
                                   (m.Client.Business.Domain == domain)
                                   select m).FirstOrDefault();
                }

                if (commsession != null)
                {
                    if ((commsession.Status == 2) || ((agent_username == null) && (agent_password == null) && (agent_domain == null)))
                    {
                        commsession.Status = 0;
                        db.SaveChanges();
                    }
                }
                else
                {
                    //Si aun asi no encontramos nada....  creamos la sesion
                    commsession = new CommSession();

                    Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }
                    converser.LastActive = DateTime.Now.ToUniversalTime();
                    converser.IP = sIP;
                    converser.LangISO = language;
                    converser.UserAgent = useragent;

                    //Añadimos una nueva CommSession como pending approval...
                    //Permitimos un solo cliente... ya veremos si podríamos añadir más.. (multisoporte wow!)

                    commsession.Business = converser.Business;
                    commsession.Client = converser;
                    commsession.SessionType = "chat";
                    commsession.CreatedOn = DateTime.Now.ToUniversalTime();

                    commsession.Status = 0;

                    /*
                     * Si la sesion la ha iniciado un agente... 
                     * la ponemos como funcionando desde ya
                     * y ponemos ese agente como agente ;-)
                     * Dado que no hay que empezar la rueda de "aceptamientos"
                     */

                    if ((agent_username != null) && (agent_password != null) && (agent_domain != null))
                    {
                        commsession.Status = 1;
                        commsession.Agents = new List<Agent>();
                        var agent_to_put = (from m in db.Agents
                                            //where m.Converser.Business.ID == converser.Business.ID
                                            where m.Converser.UserName == agent_username &&
                                            m.Converser.Password == agent_password &&
                                            m.Converser.Business.Domain == agent_domain
                                            select m).FirstOrDefault();
                        commsession.Agents.Add(agent_to_put);
                    }
                    else
                    {

                        /* 
                         * si es iniciada por cliente... 
                         * metemos todos los agentes de este business que estén logados ahora mismo 
                         * para ir eliminando conforme deniegan soporte
                         */

                        commsession.Agents = utils.GetActiveAgents(converser);
                    }

                    db.CommSessions.Add(commsession);
                    //db.Entry(commsession).State = EntityState.Modified;
                    db.SaveChanges();


                    Task.Factory.StartNew(() =>
                    {
                        OpenTokSDK opentok = new OpenTokSDK();
                        Dictionary<string, object> options = new Dictionary<string, object>();
                        options.Add(SessionPropertyConstants.P2P_PREFERENCE, "enabled");
                        string OpenTokSessionID = opentok.CreateSession(sIP, options);
                        commsession.OpenTokSessionID = OpenTokSessionID;
                        db.SaveChanges();
                    });
                }
                return Json(commsession.ID);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
            }
        }

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult RequestcommsessionidWithMessage(string apikey, string callback, string fullname, string email, string content)
        {
            try
            {
                string sIP = utils.GetIP(HttpContext);
                string language = utils.GetLang(HttpContext);
                string useragent = HttpContext.Request.UserAgent;

                var prayer = (from m in db.Conversers
                                  .Include("Business")
                              where m.Email == email
                              select m).FirstOrDefault();

                var business = (from m in db.Businesses
                                where m.ApiKey == apikey
                                select m).FirstOrDefault();

                if (prayer == null)
                {
                    prayer = new Converser();
                    prayer = utils.CreateConverserinDB(prayer);
                    prayer = (from m in db.Conversers
                              where m.ID == prayer.ID
                              select m).FirstOrDefault();
                    prayer.Business = business;
                    prayer.LastActive = DateTime.Now.ToUniversalTime();
                }
                prayer.FullName = fullname;
                prayer.Email = email;
                prayer.LangISO = language;
                prayer.IP = sIP;
                prayer.UserAgent = useragent;
                db.SaveChanges();




                var business_agents = (from m in db.Conversers
                                       where m.Agent != null && m.Business.ID == business.ID
                                       select m);


                List<Agent> agents = new List<Agent>();
                foreach (Converser sup_op in business_agents)
                {
                    agents.Add(sup_op.Agent);
                }

                //Añadimos una nueva CommSession como pending approval...
                //Por ahora permitimos un solo prayer pero podríamos añadir más.. (multisoporte wow!)
                CommSession commsession = new CommSession();
                commsession.CreatedOn = DateTime.Now.ToUniversalTime();
                commsession.Business = business;
                commsession.Client = prayer;
                commsession.Agents = new List<Agent>();
                commsession.Agents = agents;
                commsession.Status = 0;
                commsession.SessionType = "ticket";
                db.CommSessions.Add(commsession);
                db.SaveChanges();

                /*
                string usernames = "";
                foreach (Agent agent in agent
                {
                    usernames += agent.Converser.UserName + '@' + agent.Converser.Business.Domain, usernames + ",";
                }
                */

                /*
                var agent = (from m in business.Conversers
                             where m.Agent != null
                             select m).FirstOrDefault();
                */

                var agent = (from m in db.Conversers
                             where m.Agent != null && m.Business.ApiKey == business.ApiKey
                             select m).FirstOrDefault();

                NewMessage newmessage = new NewMessage();
                newmessage.From = prayer.UserName + '@' + prayer.Business.Domain;
                newmessage.To = agent.UserName + '@' + business.Domain;
                newmessage.Lang = utils.GetLang(HttpContext);
                newmessage.Subject = "ticket";
                newmessage.Content = content.Replace(Environment.NewLine, null);
                newmessage.db = db;
                newmessage.MessageType = "ticket";
                Message message = new Message(newmessage);

                if (message.Content == null)
                {
                    return Json(false);
                }

                Boolean result = message.Send();
                if (result == true)
                {
                    /*
                    message = (from m in db.Messages
                               where m.ID == message.ID
                               select m).FirstOrDefault();
                     */
                    commsession = (from m in db.CommSessions
                                   where m.ID == commsession.ID
                                   select m).FirstOrDefault();

                    commsession.Messages.Add(message);
                    db.SaveChanges();

                    string scheme = HttpContext.Request.Url.Scheme;
                    string domain = HttpContext.Request.Url.Host;
                    int port = HttpContext.Request.Url.Port;
                    try
                    {
                        domain = RoleEnvironment.GetConfigurationSettingValue("Domain");
                        port = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("Port"));
                        scheme = RoleEnvironment.GetConfigurationSettingValue("Scheme");
                    }
                    catch (Exception ex) { utils.GrabaLog(Utils.NivelLog.info, ex.Message); }
                    string url_host_port = scheme + @"://" + domain + ":" + port;

                    Email _email = new Email();
                    string[] args = { business.BusinessName, prayer.FullName, message.Content, url_host_port + Url.Action("Check", "CommSession", new { id = commsession.ID, username = prayer.UserName }) };
                    string _content = utils.LocalizeLang("email_ticket_created_contents", message.Lang, args);
                    string _subject = utils.LocalizeLang("email_ticket_created_subject", message.Lang, args);
                    _email.message = new Message();
                    _email.message.Content = _content;
                    _email.message.From = agent;
                    _email.message.To = prayer;
                    _email.message.Subject = _subject;
                    _email.message.Lang = message.Lang;
                    _email.withBcc = false;
                    _email.send();

                    var vizzop_converser_agent = (from m in db.Conversers
                                                  where m.Business.Email == "customer.service@vizzop.com" && m.Agent != null
                                                  select m).FirstOrDefault();

                    Email _email2 = new Email();
                    //string[] args2 = { commsession.ID.ToString(), prayer.UserName, business.BusinessName };
                    string[] args2 = { business.BusinessName, message.From.FullName, message.Content, url_host_port + Url.Action("Check", "CommSession", new { id = commsession.ID, username = prayer.UserName }) };
                    string business_lang = vizzop_converser_agent.LangISO;
                    string _content2 = utils.LocalizeLang("email_ticket_newmessage_business_contents", business_lang, args2);
                    string _subject2 = utils.LocalizeLang("email_ticket_newmessage_business_subject", business_lang, args2);
                    _email2.message = new Message();
                    _email2.message.Content = _content2;
                    _email2.message.From = vizzop_converser_agent;
                    _email2.message.To = vizzop_converser_agent;
                    _email2.message.Subject = _subject2;
                    _email2.send();
                }
                else
                {
                    return Json(false);
                }
                return Json(commsession.ID);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
            }
        }

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult AskForAgentsByCommSessionId(string requestcommsessionid, string username, string password, string domain, string callback)
        {
            try
            {
                utils = new Utils(db);

                if ((requestcommsessionid == null) || (requestcommsessionid == "") || (username == null) || (password == null) || (domain == null))
                {
                    string messageLog = "Faltan Datos -> username: " + username + " password: " + password + " domain: " + domain + " requestcommsessionid: " + requestcommsessionid;
                    utils.GrabaLog(Utils.NivelLog.error, messageLog);
                    return Json(false);
                }

                var requestcommsessionid_converted = Convert.ToInt16(requestcommsessionid);

                Converser converser = utils.GetConverserFromSystem(username, password, domain);

                if (converser != null)
                {
                    converser.LastActive = DateTime.Now.ToUniversalTime();
                    db.SaveChanges();
                }
                else
                {
                    return Json(false);
                }

                /*
                 * Antes que nada... todas las sesiones viejas por aprobar se ponen a 2 (denegadas)
                 */
                string strSQL = "UPDATE COMMSESSIONS SET STATUS=2 WHERE createdon < DATEADD(day,-1,CURRENT_TIMESTAMP);";
                db.Database.ExecuteSqlCommand(strSQL);

                //Buscamos la sesion que toca
                var selected_session = (from m in db.CommSessions
                                            .Include("Business")
                                            .Include("Client")
                                            .Include("Client.Business")
                                        where m.ID == requestcommsessionid_converted
                                        select m).FirstOrDefault();
                if (selected_session != null)
                {
                    //Chequeando que hacer con la sesion...
                    if (selected_session.Status == 1)
                    {
                        /*
                        selected_session.Status = 0;

                        selected_session.Agents = utils.GetActiveAgents(converser);
                        db.SaveChanges();
                        return Json(false);
                        */
                        //la sesion esta aceptada así que la devolvemos... siempre que tenga agentes,
                        //si no la ponemos a 0 para que otro operador la acepte o no..
                        //y devolvemos false para que el cliente sepa que la session sigue por aceptar... y siga intentando ;)
                        if (selected_session.Agents.Count > 0)
                        {
                            Converser agent_conv = new Converser();
                            Agent agent = selected_session.Agents.FirstOrDefault();
                            agent_conv.FullName = agent.Converser.FullName;
                            agent_conv.UserName = agent.Converser.UserName;
                            agent_conv.Business = new Business();
                            agent_conv.Business.Domain = selected_session.Business.Domain;
                            return Json(agent_conv);
                        }
                        else
                        {
                            //Esto es raro que pase... si esta aprobada pero no tiene agentes 
                            //la ponemos a 0 para vuelva al juego 
                            //y añadimos los agentes logados actualmente
                            //para que vayan aceptando o denegando...

                            selected_session.Status = 0;

                            selected_session.Agents = utils.GetActiveAgents(converser);
                            db.SaveChanges();
                            return Json(false);
                        }
                    }
                    else if (selected_session.Status == 2)
                    {
                        //La sesion esta denegada asi que les decimos que devuelvan un mensaje...
                        return Json("leave_message");
                    }
                    else
                    {
                        /*
                         * La sesion se mantiene 30 segundos en el candelero... si ningun operador la acepta
                         * se devuelve un leave_message durante 15 segundos.. y si se vuelve a pedir se vuelve a poner
                         * a 0
                         */
                        if (selected_session.CreatedOn.AddSeconds(45) < DateTime.Now.ToUniversalTime())
                        {

                            /*
                             * La sesion esta sin aprobar y han pasado mas de 45 segundos...
                             * la ponemos a 0 para que siga en el juego
                             * y ponemos todos los agentes logados actualmente para que les avise
                             */

                            selected_session.Status = 0;
                            selected_session.CreatedOn = DateTime.Now.ToUniversalTime();

                            selected_session.Agents = utils.GetActiveAgents(converser);

                            db.SaveChanges();
                            return Json(false);
                        }
                        else
                        {
                            if (selected_session.CreatedOn.AddSeconds(30) < DateTime.Now.ToUniversalTime())
                            {
                                // La sesion esta sin aprobar y han pasado mas de 30 segundos pero menos de 45
                                // devolvemos un leave mesage, la dejamos como denegada
                                // Y deberíamos dar 100 latigazos a los del Call Center
                                selected_session.Status = 2;
                                selected_session.Agents = new List<Agent>();
                                db.SaveChanges();
                                return Json("leave_message");
                            }
                            else
                            {
                                /*
                                 * La sesion esta sin aprobar y han pasado menos de 30 segundos...
                                 * la ponemos a 0 para que vuelva al juego
                                 * Y en teoría todavía quedan agentes por aprobar...
                                 */

                                selected_session.Status = 0;
                                db.SaveChanges();
                                return Json(false);
                            }
                        }
                    }
                }
                else
                {
                    return Json(false);
                }

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);

                try
                {
                    string messageLog = "username: " + username + " password: " + password + " domain: " + domain + " requestcommsessionid: " + requestcommsessionid;
                    utils.GrabaLog(Utils.NivelLog.error, messageLog);
                }
                catch (Exception ex_)
                {
                    utils.GrabaLogExcepcion(ex_);
                }

                return Json(false);
            }
        }

    }
}
