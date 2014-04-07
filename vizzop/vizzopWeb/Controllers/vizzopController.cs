using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.Web.Mvc.Resources;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{

    public class vizzopController : Controller
    {
        vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

#if DEBUG
#else
        [RequireHttps]
#endif
        [OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult En()
        {
            return View();
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        [OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult Es()
        {
            return View();
        }


#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Versions()
        {
            return RedirectToAction("Create", "vizzop");
            //return View();
        }

        [HttpPost]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Versions(string Option)
        {
            return RedirectToAction("Create", Option);
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Conditions()
        {
            return View();
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult UpgradeVersions()
        {
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

                if (converser.Business.ServiceType == "2")
                {
                    return RedirectToAction("Index", "Panel");
                }

                return View();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return RedirectToAction("ErrorPage", "Home", new { panelmode = "true" });
            }
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Upgrade(string SelectedOption)
        {
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

                var service = (from m in db.ServiceTypes
                               where m.CartName == SelectedOption
                               select m).FirstOrDefault();
                if (service == null)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }

                Sale sale = new Sale();
                sale.servicetype = service.CartName;
                sale.Total = service.Price;
                sale.Business = converser.Business;
                db.Sales.Add(sale);
                db.SaveChanges();

                string hash = vizzopWeb.Utils.GetMD5Hash(utils.secret + utils.vendornumber + sale.ID + sale.Total).ToUpperInvariant();
                sale.Hash = hash;

                db.SaveChanges();

                sale = (from m in db.Sales
                        where m.Hash == hash
                        select m).FirstOrDefault();

                if (sale == null)
                {
                    return RedirectToAction("ErrorPage", "Home");
                }

                return RedirectToAction("SendPay", new { id = sale.ID, panelmode = "true" });
            }
            catch (System.Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return RedirectToAction("ErrorPage", "Home", new { panelmode = "true" });
            }
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Create(string SelectedOption)
        {
            try
            {
                //Session["Option"] = SelectedOption;
                var ServiceTypes = from m in db.ServiceTypes
                                   where m.Type == "version"
                                   && m.Active == true
                                   select m;
                ServiceTypes = ServiceTypes.OrderBy(m => m.ZIndex);
                ViewBag.ServiceTypes = ServiceTypes;

                NewvizzopAccount newvizzopaccount = new NewvizzopAccount();
                if (SelectedOption != null)
                {
                    newvizzopaccount.ServiceType = (from m in db.ServiceTypes
                                                    where m.CartName == SelectedOption
                                                    select m).FirstOrDefault().CartName;
                }
                return View(newvizzopaccount);
            }
            catch (System.Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [HttpPost]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Create(NewvizzopAccount NewAccount)
        {
            try
            {
                Utils utils = new Utils();

                var ServiceTypes = from m in db.ServiceTypes
                                   where m.Type == "version"
                                   select m;
                ServiceTypes = ServiceTypes.OrderBy(m => m.ZIndex);
                ViewBag.ServiceTypes = ServiceTypes;

                if (ModelState.IsValid)
                {
                    //asegurémonos de que no hay emails repetidos...
                    var business_byemail = (from m in db.Businesses
                                            where m.Email == NewAccount.Email
                                            select m).FirstOrDefault();
                    if (business_byemail != null)
                    {
                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Create/Error/Repeated Email");
                        ViewBag.errors = "<p>There's already a client with this e-mail, please try with another one</p>";
                        return View(NewAccount);
                    }


                    /*
                    //Comprobamos que en ese dominio no haya un tipo igual!!
                    var converser_byuserame = (from m in db.Conversers
                                               where m.UserName == NewAccount.UserName
                                               && m.Business.Domain == NewAccount.Domain
                                               select m).FirstOrDefault();
                    if (converser_byuserame != null)
                    {
                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Create/Error - Repeated Client");
                        ViewBag.errors = "<p>There's already a client with this username, please try another one</p>";
                        return View(NewAccount);
                    }
                    */

                    Business business = new Business();
                    //Asegurémonos de que no hay Domains Repetidos
                    business.Domain = Guid.NewGuid().ToString();
                    while ((from m in db.Businesses
                            where m.Domain == business.Domain
                            select m).FirstOrDefault() != null)
                    {
                        business.Domain = Guid.NewGuid().ToString();
                    }
                    //Aseguremonos de que no hay APIKEYs repetidas...
                    business.ApiKey = Guid.NewGuid().ToString();
                    while ((from m in db.Businesses
                            where m.ApiKey == business.ApiKey
                            select m).FirstOrDefault() != null)
                    {
                        business.ApiKey = Guid.NewGuid().ToString();
                    }
                    business.CountryId = 0;
                    business.CreatedOn = DateTime.Now;
                    business.Email = NewAccount.Email;
                    if (NewAccount.Phone != null)
                    {
                        business.Phone = NewAccount.Phone;
                    }
                    business.BusinessName = NewAccount.BusinessName;
                    switch (NewAccount.ServiceType)
                    {
                        case "0":
                            business.MaxAgents = 1;
                            business.AccountType = "0";
                            break;
                        case "1":
                            business.MaxAgents = 3;
                            business.AccountType = "1";
                            break;
                    }
                    business.ServiceType = NewAccount.ServiceType;
                    business.Active = false;
                    db.Businesses.Add(business);
                    db.SaveChanges();

                    business = (from m in db.Businesses
                                where m.Email == business.Email
                                select m).FirstOrDefault();


                    Converser converser = new Converser();
                    converser.Email = NewAccount.Email;
                    converser.FullName = NewAccount.FullName;
                    //Asegurémonos de que no hay Usernames Repetidos
                    converser.UserName = Guid.NewGuid().ToString();
                    converser.Password = NewAccount.Password;
                    converser.Business = business;
                    //business.Conversers = new List<Converser>();
                    //business.Conversers.Add(converser);
                    db.Conversers.Add(converser);
                    db.SaveChanges();

                    converser = (from m in db.Conversers
                                 where m.UserName == converser.UserName
                                 && m.Password == converser.Password
                                 && m.Business.Domain == business.Domain
                                 select m).FirstOrDefault();

                    Agent agent = new Agent();
                    agent.Active = false;
                    agent.IsAdmin = true;
                    agent.Converser = converser;
                    db.Agents.Add(agent);

                    converser.Agent = agent;

                    db.SaveChanges();

                    business = (from m in db.Businesses.Include("Conversers").Include("Conversers.Agent")
                                where m.Email == business.Email
                                select m).FirstOrDefault();

                    if (business == null)
                    {
                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Create/Error/Data Error");
                        ViewBag.errors = "<p>There was an error with your data, please try again</p>";
                        return View(NewAccount);
                    }

                    var service = (from m in db.ServiceTypes
                                   where m.CartName == business.ServiceType
                                   select m).FirstOrDefault();

                    Sale sale = new Sale();
                    sale.servicetype = service.CartName;
                    sale.Total = service.Price;
                    sale.Business = business;
                    db.Sales.Add(sale);
                    db.SaveChanges();

                    string hash = vizzopWeb.Utils.GetMD5Hash(utils.secret + utils.vendornumber + sale.ID + sale.Total).ToUpperInvariant();
                    sale.Hash = hash;

                    db.SaveChanges();

                    sale = (from m in db.Sales
                            where m.Hash == hash
                            select m).FirstOrDefault();

                    if (sale == null)
                    {
                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Create/Error/No Sale/" + hash);
                        ViewBag.errors = "<p>There was an error with your data, please try again</p>";
                        return View(NewAccount);
                    }

                    //0 es la version gratis :)
                    if (sale.Total == "0.00")
                    {
                        business.Active = true;
                        agent.Active = true;
                        db.SaveChanges();
                        Session["converser"] = converser;

                        utils.AddZenSession(converser, Session.SessionID, db);
                        try
                        {
                            Sms sms = new Sms();
                            sms.message = "Nuevo business " + business.BusinessName;
                            sms.phonenumber = "34655778343";
#if DEBUG
#else
        sms.send();
#endif

                        }
                        catch (Exception _ex)
                        {
                            utils.GrabaLogExcepcion(_ex);
                        }

                        try
                        {
                            NewMessage newmessage = new NewMessage();
                            newmessage.From = "admin@vizzop";
                            newmessage.To = converser.UserName + "@" + converser.Business.Domain;
                            newmessage.Lang = utils.GetLang(HttpContext);
                            newmessage.Subject = utils.LocalizeLang("email_account_created_subject", newmessage.Lang, null);
                            /*
                            We have commented this code line because we want to send a email just with the name and email of the Business,
                            without UserName and Business' Domain
                            string[] args = { converser.Business.BusinessName, converser.FullName, converser.UserName + "@" + converser.Business.Domain };
                            */
                            string[] args = { converser.Business.BusinessName, converser.FullName, converser.Email };
                            newmessage.Content = utils.LocalizeLang("email_account_created_contents", newmessage.Lang, args);
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

                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Create/OK/Created - " + converser.UserName + "@" + converser.Business.Domain);
                        return RedirectToAction("Done");
                    }
                    // El resto no se tocan... son las de 2checkout e irán creciendo en lugar de sustituirse
                    else
                    {
                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Create/RedirectionToSendPay/" + converser.UserName + "@" + converser.Business.Domain);
                        return RedirectToAction("SendPay", new { id = sale.ID });
                    }
                }
                else
                {
                    return View(NewAccount);
                }
            }
            catch (System.Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Create/Error Saving User/");
                ViewBag.errors = "Error saving your user, please check your data and try again";
                return View(NewAccount);
            }
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult SendPay(string id, string panelmode)
        {
            try
            {
                Int32 _id = Convert.ToInt32(id);
                var sale = db.Sales.Find(_id);
                if (sale == null)
                {
                    Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/SendPay/Error/Sale == null/" + id);
                    return RedirectToAction("ErrorPage", "Home");
                }
                if (sale.Payed == true)
                {
                    Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/SendPay/Error/Sale payed already/" + id);
                    return RedirectToAction("ErrorPage", "Home");
                }
                ViewBag.Sale = sale;
                if (panelmode == "true")
                {
                    if (HttpContext.Session == null)
                    {
                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/SendPay/Error/session == null/" + id);
                        return RedirectToAction("LogOn", "Account");
                    }
                    //Primero que cargue el converser...
                    Utils utils = new Utils();
                    Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                    if (converser == null)
                    {
                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/SendPay/Error/Converser == null/" + id);
                        return RedirectToAction("LogOn", "Account");
                    }
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;
                    ViewBag.LayoutPanelMode = true;
                    ViewBag.LayoutSubPanelMode = true;
                }
                return View();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/SendPay/Error/Generic/" + id);
                return RedirectToAction("ErrorPage", "Home", new { panelmode = panelmode });
            }
        }

        //
        // GET: /Done/
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Done()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Done/Error");
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        //
        // GET: /Done/
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Ok()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/Done/Error");
                return RedirectToAction("ErrorPage", "Home");
            }
        }

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult ReturnPay()
        {
            try
            {
                utils.GrabaLog(Utils.NivelLog.error, utils.SacaParamsContext(HttpContext));
                if (Request["key"] == null)
                {
                    utils.GrabaLog(Utils.NivelLog.error, "No Key to continue after Payment...");
                    Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/ReturnPay/Error/No key to continue after payment/");
                    return RedirectToAction("ErrorPage", "Home");
                }

                var key = Request["key"].ToUpperInvariant();
                var id = Convert.ToInt32(Request["merchant_order_id"].ToUpperInvariant());
                var sale = (from m in db.Sales
                            where m.Hash == key
                            && m.ID == id
                            select m).FirstOrDefault();

                if ((HttpContext.IsDebuggingEnabled == true) && (sale == null))
                {
                    sale = (from m in db.Sales
                            where m.ID == id
                            select m).FirstOrDefault();
                }
                if (sale == null)
                {
                    utils.GrabaLog(Utils.NivelLog.error, "No Sale found after Payment...");
                    Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/ReturnPay/Error/No Sale found after Payment/" + id);
                    return RedirectToAction("ErrorPage", "Home");
                }

                switch (sale.servicetype)
                {
                    //Hemos dado de alta cuentas...
                    case "1":
                        sale.Payed = true;
                        sale.Business.Active = true;
                        sale.Business.MaxAgents = 3;
                        sale.Business.AccountType = "1";
                        foreach (Converser converser in sale.Business.Conversers)
                        {
                            if (converser.Agent != null)
                            {
                                converser.Agent.Active = true;
                            }
                        }
                        db.SaveChanges();

                        return RedirectToAction("done");
                    case "2":
                        sale.Payed = true;
                        sale.Business.Active = true;

                        sale.Business.MaxAgents = 10;
                        sale.Business.AccountType = "2";
                        foreach (Converser converser in sale.Business.Conversers)
                        {
                            if (converser.Agent != null)
                            {
                                converser.Agent.Active = true;
                            }
                        }
                        db.SaveChanges();

                        return RedirectToAction("done");
                    case "3":
                    case "4":
                        sale.Payed = true;
                        sale.Business.MaxAgents = sale.Business.MaxAgents + 1;
                        db.SaveChanges();
                        return RedirectToAction("ok");
                    case "5":
                        sale.Payed = true;
                        if (sale.Business.MaxAgents < 3)
                        {
                            sale.Business.MaxAgents = 3;
                        }
                        sale.Business.AccountType = "1";
                        db.SaveChanges();
                        return RedirectToAction("ok");
                }
                Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/ReturnPay/Error/No Case Error/" + id);
                return RedirectToAction("ErrorPage", "Home");
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/ReturnPay/Error/Generic/");
                return RedirectToAction("ErrorPage", "Home");
            }
        }
    }
}
