using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.Web.Mvc.Resources;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class AgentController : Controller
    {

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Index()
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);
            ViewBag.LayoutPanelMode = true;
            ViewBag.LayoutSubPanelMode = true;
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }

                return View(converser);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return View();
            }
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Panel()
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);
            ViewBag.LayoutPanelMode = true;
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                converser.Business.Conversers = new List<Converser>();
                ViewBag.converser = converser;
                var list = (from m in db.Agents
                            where m.Converser.Business.ID == converser.Business.ID
                            select m);
                return View(list);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                ViewBag.errors = "Error loading users, please check your data and try again";
                return View();
            }
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult BuyAgent()
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);
            ViewBag.LayoutPanelMode = true;
            ViewBag.LayoutSubPanelMode = true;
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                converser.Business.Conversers = new List<Converser>();
                ViewBag.converser = converser; ;
                ServiceType service = new ServiceType();
                switch (converser.Business.ServiceType)
                {
                    case "0":
                        return RedirectToAction("UpgradeVersions", "vizzop");
                    case "1":
                    case "2":
                        //Standard Agent
                        service = (from m in db.ServiceTypes
                                   where m.CartName == "2"
                                   select m).FirstOrDefault();
                        break;
                    /*
                case "2":
                    //Premium Agent
                    service = (from m in db.ServiceTypes
                               where m.CartName == "4"
                               select m).FirstOrDefault();
                    break;
                    */
                }

                Sale sale = new Sale();
                sale.servicetype = service.CartName;
                sale.Total = service.Price;
                sale.Business = converser.Business;
                db.Sales.Add(sale);
                db.SaveChanges();

                string hash = Utils.GetMD5Hash(utils.secret + utils.vendornumber + sale.ID + sale.Total).ToUpperInvariant();
                sale.Hash = hash;

                db.SaveChanges();

                sale = (from m in db.Sales
                        where m.Hash == hash
                        select m).FirstOrDefault();

                if (sale == null)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }

                return RedirectToAction("SendPay", "vizzop", new { id = sale.ID });

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return RedirectToAction("Index");
            }
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Create()
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);
            ViewBag.LayoutPanelMode = true;
            ViewBag.LayoutSubPanelMode = true;
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                /*
                converser.Business.Conversers = new List<Converser>();
                ViewBag.converser = converser;
                */
                var list = (from m in db.Agents
                            where m.Converser.Business.ID == converser.Business.ID
                            select m);

                if (list.Count() >= converser.Business.MaxAgents)
                {
                    //TODO
                    string href = Url.Action("BuyAgent");
                    ViewBag.errors = "<p>You have reached your maximun number of agents. <a href=" + href + " class='btn'>Buy another agent</a></p>";
                    return View();
                }
                return View();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                ViewBag.errors = "Error loading users, please check your data and try again";
                return View();
            }
        }

        [HttpPost]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Create(NewAgent newAgent)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);
            ViewBag.LayoutPanelMode = true;
            ViewBag.LayoutSubPanelMode = true;
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                /*
                converser.Business.Conversers = new List<Converser>();
                ViewBag.converser = converser;
                */

                if (ModelState.IsValid)
                {

                    var list = (from m in db.Agents
                                where m.Converser.Business.ID == converser.Business.ID
                                select m);

                    if (list.Count() >= converser.Business.MaxAgents)
                    {
                        //TODO
                        string href = Url.Action("BuyAgent");
                        ViewBag.errors = "<p>You have reached your maximun number of agents. <a href=" + href + " class='btn'>Buy another agent</a></p>";
                        return View(newAgent);
                    }

                    //asegurémonos de que no hay Agents repetidos...
                    var agent_byemail = (from m in db.Agents
                                         where m.Converser.Email == newAgent.Email
                                         select m).FirstOrDefault();
                    if (agent_byemail != null)
                    {
                        ViewBag.errors = "<p>There's already a Support Agent with this e-mail, please try another one</p>";
                        return View(newAgent);
                    }


                    var business = (from m in db.Businesses
                                    where m.ID == converser.Business.ID
                                    select m).FirstOrDefault();

                    Converser _converser = new Converser();
                    _converser.Email = newAgent.Email;
                    _converser.FullName = newAgent.FullName;

                    //Asegurémonos de que no hay UserNames Repetidos
                    _converser.UserName = Guid.NewGuid().ToString();
                    while ((from m in db.Conversers
                            where m.UserName == _converser.UserName
                            select m).FirstOrDefault() != null)
                    {
                        _converser.UserName = Guid.NewGuid().ToString();
                    }
                    _converser.Password = newAgent.Password;
                    _converser.LastActive = DateTime.Now.ToUniversalTime();
                    db.Conversers.Add(_converser);
                    db.SaveChanges();

                    _converser = (from m in db.Conversers
                                  where m.UserName == _converser.UserName && m.Password == _converser.Password
                                  select m).FirstOrDefault();
                    if (_converser == null)
                    {
                        ViewBag.errors = "Error saving, please check your data and try again";
                        return View(newAgent);
                    }

                    _converser.Business = business;

                    //business.Conversers.Add(_converser);

                    Agent _agent = new Agent();
                    _agent.Active = true;
                    _agent.IsAdmin = false;
                    _agent.Converser = _converser;
                    db.Agents.Add(_agent);

                    _converser.Agent = _agent;

                    //converser.Business.Conversers.Add(_converser);

                    db.SaveChanges();

                    try
                    {
                        NewMessage newmessage = new NewMessage();
                        newmessage.From = "admin@vizzop";
                        newmessage.To = _converser.UserName + "@" + _converser.Business.Domain;
                        newmessage.Lang = utils.GetLang(HttpContext);
                        newmessage.Subject = utils.LocalizeLang("email_account_created_subject", newmessage.Lang, null);
                        string[] args = { _converser.Business.BusinessName, _converser.FullName, _converser.UserName + "@" + _converser.Business.Domain };
                        newmessage.Content = utils.LocalizeLang("email_account_created_contents", newmessage.Lang, args);
                        newmessage.db = db;
                        newmessage.MessageType = "email";
                        Message message = new Message(newmessage);
                        message.From = business.Conversers.FirstOrDefault();
                        message.To = _converser;
                        Boolean result = message.Send();
                    }
                    catch (Exception ex)
                    {
                        utils.GrabaLogExcepcion(ex);
                    }

                    try
                    {
                        Sms sms = new Sms();
                        sms.message = "Nuevo agent " + business.BusinessName;
                        sms.phonenumber = "34655778343";
                        sms.send();
                    }
                    catch (Exception _ex)
                    {
                        utils.GrabaLogExcepcion(_ex);
                    }

                    return RedirectToAction("Index", "Agent", new { popupmsg = "The Agent has been created successfully" });
                    //return RedirectToAction("Index", "Agent");
                }
                else
                {
                    ViewBag.errors = "Error saving, please check your data and try again";
                    return View(newAgent);
                }

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                ViewBag.errors = "Error saving, please check your data and try again";
                return View(newAgent);
            }
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Edit(int id)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);
            ViewBag.LayoutPanelMode = true;
            ViewBag.LayoutSubPanelMode = true;
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                /*
                converser.Business.Conversers = new List<Converser>();
                */
                ViewBag.converser = converser;

                Agent agent = db.Agents.Find(id);
                Boolean contains = false;

                var list = (from m in db.Agents
                            where m.Converser.Business.ID == converser.Business.ID
                            select m);
                foreach (Agent _Agent in list)
                {
                    if (_Agent.ID == agent.ID)
                    {
                        contains = true;
                    }
                }
                if (contains == false)
                {
                    return RedirectToAction("LogOn", "Account");
                }

                /*
            if (agent.IsDefault == true)
            {
                RedirectToAction("EditBusiness", "Panel");
            }
                 * */

                NewAgent newagent = new NewAgent();
                newagent.Password = agent.Converser.Password;
                newagent.ConfirmPassword = agent.Converser.Password;
                newagent.FullName = agent.Converser.FullName;
                newagent.Email = agent.Converser.Email;
                newagent.ID = agent.ID;
                return View(newagent);
            }
            catch (System.Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return RedirectToAction("Index", "Agent", new { popupmsg = "There was some error while loading the agent" });
            }
        }

        [HttpPost]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Edit(NewAgent newAgent)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);
            ViewBag.LayoutPanelMode = true;
            ViewBag.LayoutSubPanelMode = true;
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                //converser.Business.Conversers = new List<Converser>();
                ViewBag.converser = converser;

                if (ModelState.IsValid)
                {
                    Agent agent = db.Agents.Find(newAgent.ID);
                    Boolean contains = false;

                    var list = (from m in db.Agents
                                where m.Converser.Business.ID == converser.Business.ID
                                select m);
                    foreach (Agent _Agent in list)
                    {
                        if (_Agent.ID == agent.ID)
                        {
                            contains = true;
                        }
                    }
                    if (contains == false)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    /*
                    if (agent.IsDefault == true)
                    {
                        //string href = Url.Action("EditBusiness", "Panel");
                        ViewBag.errors = "<p>You Cannot edit your default agent from here, please go to Settings -> Account Details</p>";
                        return View(NewAccount);
                    }
                     * */
                    agent.Converser.Password = newAgent.Password;
                    agent.Converser.Email = newAgent.Email;
                    agent.Converser.FullName = newAgent.FullName;
                    //db.Entry(agent).State = EntityState.Modified;
                    db.SaveChanges();
                    /*
                    return Redirect(Url.RouteUrl(new { controller = "Panel", action = "Index" }) + "#agents");
                     * */
                    utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/EditedAgentOK/" + agent.Converser.UserName + "@" + agent.Converser.Business.Domain);
                    string data_saved = utils.LocalizeLang("data_saved", utils.GetLang(HttpContext), null);
                    return RedirectToAction("Index", "Agent", new { popupmsg = data_saved });
                }

                ViewBag.errors = "Error saving, please check your data and try again";
                return View(newAgent);
            }
            catch (System.Exception ex)
            {
                ViewBag.errors = "Error saving your user, please check your data and try again";
                utils.GrabaLogExcepcion(ex);
                return View(newAgent);
            }
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult DeleteConfirmed(int id)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);
            ViewBag.LayoutPanelMode = true;
            ViewBag.LayoutSubPanelMode = true;
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }

                if (converser.Agent.IsAdmin == false)
                {
                    ViewBag.errors = "<p>Not allowed to delete Agent</p>";
                    return RedirectToAction("Index", "Agent");
                }

                //(item.Converser.ID != Model.ID)

                Agent agent = db.Agents.Find(id);

                if (agent == null)
                {
                    ViewBag.errors = "<p>No agent found</p>";
                    return RedirectToAction("Index", "Agent");
                }

                if (converser.Agent.ID == agent.ID)
                {
                    ViewBag.errors = "<p>You Cannot delete your own agent</p>";
                    return RedirectToAction("Index", "Agent");
                }

                Boolean contains = false;

                var list = (from m in db.Agents
                            where m.Converser.Business.ID == converser.Business.ID
                            select m);

                foreach (Agent _Agent in list)
                {
                    if (_Agent.ID == agent.ID)
                    {
                        contains = true;
                    }
                }

                if (contains == false)
                {
                    ViewBag.errors = "<p>No agent found</p>";
                    return RedirectToAction("Index", "Agent");
                }

                db.Conversers.Remove(agent.Converser);
                db.Agents.Remove(agent);
                db.SaveChanges();

                //return RedirectToAction("Index", "Panel");
                return Redirect(Url.RouteUrl(new { controller = "Agent", action = "Index" }) + "?popupmsg=Agent deleted successfully");

            }
            catch (System.Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Redirect(Url.RouteUrl(new { controller = "Agent", action = "Index" }) + "?popupmsg=There was an error deleting the Agent");
                //return RedirectToAction("ErrorPage", "Home");
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}