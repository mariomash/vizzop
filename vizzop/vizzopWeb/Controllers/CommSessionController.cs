using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.ServiceRuntime;
using vizzopWeb.Models;
using System.Threading.Tasks;

namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class CommSessionController : Controller
    {

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetCommSessionsJson(string CommSession_filter)
        {
            using (var db = new vizzopContext())
            {
                Utils utils = new Utils(db);

                Converser converser = new Converser();
                try
                {
                    if (HttpContext.Session == null)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    converser = utils.GetLoggedConverser(HttpContext.Session);
                    if (converser == null)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    converser = utils.GetConverserFromSystem(converser.UserName, converser.Password, converser.Business.Domain);
                    if (converser == null)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;

                    //0 == Pending Approval
                    //1 == Support approved, supporting Right Now
                    //2 == Support Denied
                    //3 == Support Ended

                    if (CommSession_filter == null)
                    {
                        CommSession_filter = "0";
                    }

                    List<CommSession> CommSessions = new List<CommSession>();
                    switch (CommSession_filter)
                    {
                        /*
                    case "0":
                        CommSessions = (from m in business.CommSessions
                                           where m.business.ID == business.ID &&
                                           (m.Status == 0) ||
                                           ((m.Status == 1) && (m.SessionType == "ticket") && (m.Messages.OrderByDescending(j => j.TimeStamp).FirstOrDefault().From.IsAgent == false)) ||
                                           ((m.Status == 1) && (m.Messages.Count == 0))
                                           select m).ToList();
                        break;
                         */
                        case "0":
                            CommSessions = (from m in converser.Business.CommSessions
                                            select m).ToList();
                            break;
                        case "1":
                            CommSessions = (from m in converser.Business.CommSessions
                                            where ((m.Status == 0) || (m.Status == 1))
                                            && m.SessionType == "ticket"
                                            select m).ToList();
                            break;
                        case "2":
                            CommSessions = (from m in converser.Business.CommSessions
                                            where ((m.Status == 2) || (m.Status == 3))
                                            && m.SessionType == "ticket"
                                            select m).ToList();
                            break;
                    }
                    List<CommSessionDataTables> DefSesList = new List<CommSessionDataTables>();
                    foreach (CommSession s in CommSessions)
                    {
                        try
                        {
                            CommSessionDataTables ses = new CommSessionDataTables();
                            ses.ID = s.ID;
                            switch (s.Status)
                            {
                                case 0:
                                    ses.Status = utils.LocalizeLang("waiting_approval", utils.GetLang(HttpContext), null);
                                    break;
                                case 1:
                                    ses.Status = utils.LocalizeLang("supporting", utils.GetLang(HttpContext), null);
                                    break;
                                case 2:
                                case 3:
                                    ses.Status = utils.LocalizeLang("closed_ticket", utils.GetLang(HttpContext), null);
                                    break;
                            }
                            if (s.SessionType.ToLowerInvariant() == "chat")
                            {
                                ses.Status = "chat";
                            }

                            if (s.Comments != null)
                            {
                                ses.Comments = s.Comments;
                            }
                            else
                            {
                                ses.Comments = "";
                            }
                            ses.WaitingFor = utils.GetPrettyDate(s.CreatedOn);
                            ses.LastAlive = s.CreatedOn;


                            if (s.Agents.Count() == 0)
                            {
                                ses.Agents = "-";
                            }

                            ArrayList found_agents = new ArrayList();
                            ses.LastMsgIsFromClient = false;
                            if (s.Messages.Count > 0)
                            {
                                foreach (Message msg in s.Messages.OrderByDescending(r => r.TimeStamp))
                                {
                                    if (msg.From.UserName != s.Client.UserName)
                                    {
                                        if (found_agents.Contains(msg.From.FullName) == false)
                                        {
                                            found_agents.Add(msg.From.FullName);
                                        }
                                    }
                                    if (ses.LastMessage == null)
                                    {
                                        if ((msg.Content != null) && (msg.Content != "") && (msg.Content != "null"))
                                        {
                                            TimeZone localZone = TimeZone.CurrentTimeZone;
                                            ses.LastAlive = localZone.ToLocalTime(msg.TimeStamp);

                                            ses.WaitingFor = utils.GetPrettyDate(msg.TimeStamp);
                                            //ses.LastMessage = "[<i>" + utils.GetPrettyDate(msg.TimeStamp) + "</i>] " + ses.LastMessage;

                                            ses.LastMessage = msg.From.FullName;
                                            if (msg.From.UserName == s.Client.UserName)
                                            {
                                                ses.LastMessage += " (client) ";
                                            }
                                            else
                                            {
                                                ses.LastMessage += " (agent) ";
                                            }
                                            ses.LastMessage += ": " + msg.Content;
                                            if (msg.From.UserName == s.Client.UserName)
                                            {
                                                ses.LastMsgIsFromClient = true;
                                            }
                                        }
                                    }
                                }
                                foreach (var found_agent in found_agents)
                                {
                                    ses.Agents += ", " + found_agent;
                                }
                                if (ses.Agents != null)
                                {
                                    ses.Agents = ses.Agents.TrimStart(',');
                                }
                                if (ses.Agents == null)
                                {
                                    ses.Agents = "-";
                                }
                            }
                            else
                            {
                                ses.LastMessage = "";
                            }
                            ses.SessionType = s.SessionType;
                            ses.ClientName = s.Client.FullName;
                            ses.NumberOfMessages = s.Messages.Count;

                            if (s.LockedBy == null)
                            {
                                ses.LockedBy = "-";
                            }
                            else
                            {
                                ses.LockedBy = s.LockedBy.FullName;
                            }

                            DefSesList.Add(ses);
                        }
                        catch (Exception _ex)
                        {
                            utils.GrabaLogExcepcion(_ex);
                        }
                    }
                    return Json(new
                    {
                        aaData = DefSesList.Select(x => new Object[] { 
                        x.ID.ToString(), 
                        x.SessionType,
                        x.Status, 
                        x.ClientName,
                        x.LockedBy,
                        x.Agents,
                        x.LastAlive.ToString("o"),
                        x.WaitingFor,
                        x.LastMessage,
                        x.Comments,
                        x.LastMsgIsFromClient,
                        x.NumberOfMessages
                })
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(null);
                }
            }
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Index(string CommSession_filter)
        {
            using (var db = new vizzopContext())
            {
                Utils utils = new Utils(db);

                ViewBag.LayoutPanelMode = true;
                Converser converser = new Converser();
                try
                {
                    if (HttpContext.Session == null)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    converser = utils.GetLoggedConverser(HttpContext.Session);
                    if (converser == null)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return RedirectToAction("ErrorPage", "Home");
                }

                try
                {

                    /*
                    business = (from m in db.businesss
                                      where m.ID == business.ID
                                      select m).FirstOrDefault();
                    */

                    //0 == Pending Approval
                    //1 == Support approved, supporting Right Now
                    //2 == Support Denied
                    //3 == Support Ended

                    List<SelectListItem> items = new List<SelectListItem>();
                    /*
                    items.Add(new SelectListItem
                    {
                        Text = "Sessions awaiting response",
                        Value = "0"
                    });
                     */
                    items.Add(new SelectListItem
                    {
                        Text = "All Chats & Tickets",
                        Value = "0"
                    });
                    items.Add(new SelectListItem
                    {
                        Text = "Only opened tickets",
                        Value = "1"
                    });
                    items.Add(new SelectListItem
                    {
                        Text = "Only closed tickets",
                        Value = "2"
                    });
                    if (CommSession_filter == null)
                    {
                        CommSession_filter = "1";
                    }
                    foreach (SelectListItem item in items)
                    {
                        if (item.Value == CommSession_filter)
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                    //items[Convert.ToInt16(CommSession_filter)].Selected = true;
                    ViewBag.filter_items = items;
                    return View();
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return View();
                }
            }
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Create()
        {
            return View();
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Check(int id, string username, string domain)
        {
            using (var db = new vizzopContext())
            {
                Utils utils = new Utils(db);

                //CommSession CommSession = db.CommSessions.Find(id);
                CommSession CommSession = (from m in db.CommSessions
                                               .Include("Client")
                                               .Include("Business")
                                               .Include("Client.Business")
                                           where m.ID == id &&
                                           m.Client.UserName == username
                                           select m).FirstOrDefault();
                ViewBag.business = CommSession.Business;
                ViewBag.ApiKey = CommSession.Business.ApiKey;
                ViewBag.BusinessName = CommSession.Business.BusinessName;
                return View(CommSession);
            }
        }

        //
        // POST: /CommSession/Comment/5

        [HttpPost]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult ClientComment(int id, string from_username, string from_domain, string content)
        {
            using (var db = new vizzopContext())
            {
                Utils utils = new Utils(db);

                try
                {
                    if (ModelState.IsValid)
                    {
                        CommSession CommSession = (from m in db.CommSessions
                                                       .Include("Client")
                                                       .Include("Business")
                                                   where m.ID == id &&
                                                   m.Client.UserName == from_username &&
                                                   m.Client.Business.Domain == from_domain
                                                   select m).FirstOrDefault();

                        /*
                        string usernames = "";
                        foreach (Agent agent in CommSession.Agents)
                        {
                            usernames += agent.Converser.UserName;
                        }
                        */
                        /*
                        var business_agents = (from m in CommSession.Business.Conversers
                                               where m.Agent != null
                                               select m);
                         */
                        var agent_from = CommSession.Agents.FirstOrDefault().Converser;

                        NewMessage newmessage = new NewMessage();
                        newmessage.From = CommSession.Client.UserName + '@' + CommSession.Client.Business.Domain;
                        newmessage.To = agent_from.UserName + '@' + agent_from.Business.Domain;
                        newmessage.Lang = utils.GetLang(HttpContext);
                        newmessage.Subject = "ticket";
                        newmessage.Content = content.Replace(Environment.NewLine, null);
                        newmessage.db = db;
                        newmessage.MessageType = "ticket";
                        Message message = new Message(newmessage);

                        Boolean result = message.Send();
                        if (result == true)
                        {
                            /*
                            message = (from m in db.Messages
                                       where m.ID == message.ID
                                       select m).FirstOrDefault();
                             * */
                            CommSession = (from m in db.CommSessions
                                           where m.ID == CommSession.ID
                                           select m).FirstOrDefault();

                            CommSession.Messages.Add(message);
                            CommSession.Status = 1;
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
                            catch (Exception ex) { utils.GrabaLogExcepcion(ex); }
                            string url_host_port = scheme + @"://" + domain + ":" + port;

                            /*
                            Business business_vizzop = (from m in db.Businesses
                                                             where m.Email == "customer.service@vizzop.com"
                                                             select m).FirstOrDefault();
                            */
                            Email _email2 = new Email();
                            string[] args2 = { CommSession.Client.Business.BusinessName, 
                                                     message.From.FullName, message.Content, 
                                                     url_host_port + Url.Action("Check", "CommSession", new { id = CommSession.ID, username = message.From.UserName }) 
                                                 };

                            string business_lang = agent_from.LangISO;
                            string _content2 = utils.LocalizeLang("email_ticket_newmessage_business_contents", business_lang, args2);
                            string _subject2 = utils.LocalizeLang("email_ticket_newmessage_business_subject", business_lang, args2);
                            _email2.message = new Message();
                            _email2.message.Content = _content2;
                            //TODO
                            _email2.message.From = CommSession.Agents.FirstOrDefault().Converser;
                            _email2.message.To = CommSession.Agents.FirstOrDefault().Converser;
                            _email2.message.Subject = _subject2;
                            _email2.send();
                        }

                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                }
                return RedirectToAction("Check", new { id = id, username = from_username, domain = from_domain });
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}