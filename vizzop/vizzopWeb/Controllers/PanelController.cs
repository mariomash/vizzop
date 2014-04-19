using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using vizzopWeb.Models;
//using Microsoft.Web.Mvc.Resources;

namespace vizzopWeb.Controllers
{
    public class PanelController : Controller
    {

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Index(string commsession_filter)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            ViewBag.LayoutPanelMode = true;
            Converser converser = new Converser();
            if (HttpContext.Session == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            try
            {
                converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                if (converser.Agent == null)
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
                ViewBag.Code = @"
<head>
    <!-- Start copying here -->
        <script type='text/javascript'>
            setTimeout(function(){
                var d = document; h = d.getElementsByTagName('head')[0];
                s = d.createElement('script'); s.type = 'text/javascript'; s.async = true; s.setAttribute('async', 'true');
                s.src = 'https://vizzop.com/vizzop/jsapi.ashx?mode=client&apikey=" + converser.Business.ApiKey + @"';
                h.appendChild(s);
            }, 50);
        </script>
    <!-- Stop copying here -->
</head>";
                if (commsession_filter == null)
                {
                    ViewBag.commsession_filter = "1";
                }
                else
                {
                    ViewBag.commsession_filter = commsession_filter;
                }

                ViewBag.ChangeLogs = new List<ChangeLog>();
                try
                {
                    ViewBag.ChangeLogs = (from m in db.ChangeLogs
                                          orderby m.TimeStamp descending
                                          select m).Take(4).ToList<ChangeLog>();
                }
                catch (Exception _ex)
                {
                    utils.GrabaLogExcepcion(_ex);
                }

                ViewBag.OpenedTickets = 0;
                ViewBag.TicketsInterval = 0;
                ViewBag.Chats1Week = 0;
                ViewBag.VisitsYesterday = 0;
                ViewBag.UniqueVisitsInterval = 0;
                try
                {
                    ViewBag.OpenedTickets = (from m in converser.Business.CommSessions
                                             where ((m.Status == 0) || (m.Status == 1))
                                             && m.SessionType == "ticket"
                                             select m).ToList().Count;
                    ViewBag.TicketsInterval = (from m in converser.Business.CommSessions
                                               where m.SessionType == "ticket"
                                               && m.CreatedOn.CompareTo(DateTime.Now.AddDays(-7)) > 0
                                               select m).ToList().Count;
                    ViewBag.Chats1Week = (from m in converser.Business.CommSessions
                                          where m.SessionType == "chat"
                                          && m.CreatedOn.CompareTo(DateTime.Now.AddDays(-7)) > 0
                                          select m).ToList().Count; ;
                    DateTime dtfrom = DateTime.Now.AddDays(-1);

                    ViewBag.VisitsYesterday = (from w in db.WebLocations_History.Include("converser")
                                               where w.converser.Business.ID == converser.Business.ID
                                               && (w.TimeStamp_First.CompareTo(dtfrom) > 0)
                                               select w).Count();
                    ViewBag.UniqueVisitsInterval = (from w in db.WebLocations_History.Include("converser")
                                                    where w.converser.Business.ID == converser.Business.ID
                                                    && (w.TimeStamp_First.CompareTo(dtfrom) > 0)
                                                    group w by w.converser into mgg
                                                    select mgg).Count();
                }
                catch (Exception _ex)
                {
                    utils.GrabaLogExcepcion(_ex);
                }

                return View();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return RedirectToAction("ErrorPage", "Home", new { panelmode = "true" });
            }
        }

        [HttpPost]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult SaveWidgetSettings(string WidgetBackgroundColor, string WidgetForegroundColor, string WidgetBorderColor, string WidgetText, string BusinessHours, string AllowJsApiLoading, string ShowHelpButton, string Controls)
        {
            
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            Converser converser = new Converser();
            try
            {
                if (HttpContext.Session == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                try
                {
                    converser = utils.GetLoggedConverser(HttpContext.Session);
                    if (converser == null)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }

                    var business = (from m in db.Businesses
                                    where m.ID == converser.Business.ID
                                    select m).FirstOrDefault();

                    if (WidgetBackgroundColor != null)
                    {
                        business.WidgetBackgroundColor = WidgetBackgroundColor;
                    }
                    if (WidgetForegroundColor != null)
                    {
                        business.WidgetForegroundColor = WidgetForegroundColor;
                    }
                    if (WidgetBorderColor != null)
                    {
                        business.WidgetBorderColor = WidgetBorderColor;
                    }
                    if (WidgetText != null)
                    {
                        business.WidgetText = WidgetText;
                    }
                    if (BusinessHours != null)
                    {
                        business.BusinessHours = BusinessHours;
                    }
                    if (AllowJsApiLoading != null)
                    {
                        business.AllowJsApiLoading = Convert.ToBoolean(AllowJsApiLoading);
                    }
                    if (ShowHelpButton != null)
                    {
                        business.ShowHelpButton = Convert.ToBoolean(ShowHelpButton);
                    }

                    business.Controls.Clear();
                    if (Controls != null)
                    {
                        var arrControls = new JavaScriptSerializer().Deserialize<List<string>>(Controls);
                        if (arrControls.Count > 0)
                        {
                            foreach (var str in arrControls)
                            {
                                var control = new FormControl();
                                control.Name = str;
                                business.Controls.Add(control);
                                /*
                                var existence = (from m in business.Controls
                                                 where m.Name == str
                                                 select m).FirstOrDefault();
                                if (existence == null)
                                {

                                }
                                 */
                            }
                        }
                    }

                    //db.Entry(converser).State = EntityState.Modified;
                    db.SaveChanges();

                    Session["converser"] = converser;

                    return Json(true);

                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
            }
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult EditBusiness()
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
                ViewBag.converser = converser;

                Newbusiness newbusiness = new Newbusiness();
                newbusiness.UserName = converser.UserName;
                newbusiness.Password = converser.Password;
                newbusiness.ConfirmPassword = converser.Password;
                newbusiness.FullName = converser.FullName;
                newbusiness.BusinessName = converser.Business.BusinessName;
                newbusiness.Email = converser.Business.Email;
                newbusiness.ID = converser.Business.ID;
                newbusiness.ServiceType = converser.Business.ServiceType;
                newbusiness.Address = converser.Business.Address;
                newbusiness.ApiKey = converser.Business.ApiKey;
                newbusiness.City = converser.Business.City;
                newbusiness.CountryId = converser.Business.CountryId;
                newbusiness.Phone = converser.Business.Phone;
                newbusiness.PostalCode = converser.Business.PostalCode;
                newbusiness.VAT = converser.Business.VAT;
                newbusiness.Url = converser.Business.Url;
                newbusiness.Domain = converser.Business.Domain;
                return View(newbusiness);
            }
            catch (Exception ex)
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
        public ActionResult EditBusiness(Newbusiness NewAccount)
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
                //vizzopContext _db = new vizzopContext();

                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                /*
                converser.Business.Conversers = new List<Converser>();
                ViewBag.converser = converser;
                */
                if (NewAccount.ServiceType != converser.Business.ServiceType)
                {
                    ViewBag.errors = "To change your account version please access <a href='" + Url.Action("UpgradeVersions", "vizzop") + "'>here</a>";
                    return View(NewAccount);
                }

                /*
                if (ModelState.IsValid)
                {
                }
                 */

                converser.FullName = NewAccount.FullName;
                converser.Business.BusinessName = NewAccount.BusinessName;
                converser.Business.Email = NewAccount.Email;
                converser.Password = NewAccount.Password;

                if (NewAccount.Address != null) { converser.Business.Address = NewAccount.Address; }
                if (NewAccount.City != null) { converser.Business.City = NewAccount.City; }
                if (NewAccount.CountryId != 0) { converser.Business.CountryId = NewAccount.CountryId; }
                if (NewAccount.Phone != null) { converser.Business.Phone = NewAccount.Phone; }
                if (NewAccount.PostalCode != null) { converser.Business.PostalCode = NewAccount.PostalCode; }
                if (NewAccount.ServiceType != null) { converser.Business.ServiceType = NewAccount.ServiceType; }
                if (NewAccount.Url != null) { converser.Business.Url = NewAccount.Url; }
                if (NewAccount.VAT != null) { converser.Business.VAT = NewAccount.VAT; }

                //db.Entry(converser).State = EntityState.Modified;
                db.SaveChanges();

                Session["converser"] = converser;

                return RedirectToAction("EditBusiness", "Panel", new { popupmsg = "The changes have been saved correctly" });


            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                ViewBag.errors = "Error. No changes were made to your Account.";
                return View(NewAccount);
            }
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Widget()
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

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
                return View(converser.Business);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return View();
            }
        }
    }
}

