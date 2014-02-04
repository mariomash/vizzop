using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;
//using Microsoft.Web.Mvc.Resources;

namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class ConverserController : Controller
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

        [JsonpFilter]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult checkLogin(string username, string password, string callback)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (HttpContext.Session == null)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                    if (converser == null)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;

                    if (converser != null)
                    {
                        //return Json(converser);
                        return Json(converser);
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
        public ActionResult GetNew(string FullName, string apikey)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var business = (from m in db.Businesses
                                    where m.ApiKey == apikey
                                    select m).FirstOrDefault();
                    if (business == null)
                    {
                        return null;
                    }

                    Converser converser = new Converser();
                    converser = utils.CreateConverserinDB(converser);

                    if (converser.ID != 0)
                    {
                        converser = (from m in db.Conversers
                                         .Include("Business")
                                     where m.ID == converser.ID
                                     select m).FirstOrDefault();
                        if (converser == null)
                        {
                            return null;
                        }

                        if (FullName != null)
                        {
                            converser.FullName = FullName;
                        }
                        converser.Business = business;
                        converser.LangISO = utils.GetLang(HttpContext);
                        converser.IP = utils.GetIP(HttpContext);
                        converser.UserAgent = HttpContext.Request.UserAgent;

                        db.SaveChanges();

                        Converser return_conv = new Converser();
                        return_conv.FullName = converser.FullName;
                        return_conv.UserName = converser.UserName;
                        return_conv.Password = converser.Password;
                        return_conv.ID = converser.ID;
                        return_conv.Business = new Business();
                        return_conv.Business.Domain = converser.Business.Domain;
                        return Json(return_conv);
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
                return null;
            }
        }

        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult ChangeStatus(string UserName, string Status, string Password, string Domain, string callback)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }

                    bool newStatus = Convert.ToBoolean(Status);
                    converser.Active = newStatus;

                    TimeZone localZone = TimeZone.CurrentTimeZone;
                    DateTime loctime = DateTime.Now;
                    DateTime loctimeUTC = localZone.ToUniversalTime(loctime);
                    converser.LastActive = loctimeUTC;
                    db.SaveChanges();

                    Converser return_conv = new Converser();
                    return_conv.Active = converser.Active;
                    return_conv.FullName = converser.FullName;
                    return_conv.UserName = converser.UserName;
                    return_conv.Password = converser.Password;
                    return_conv.Business = new Business();
                    return_conv.Business.Domain = converser.Business.Domain;
                    return Json(return_conv);
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
        public ActionResult ChangeName(string UserName, string FullName, string Password, string Domain, string callback)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Converser converser = utils.GetConverserFromSystem(UserName, Password, Domain, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }

                    if (FullName == "null") { FullName = null; }
                    converser.FullName = FullName;
                    converser.LastActive = DateTime.Now.ToUniversalTime();

                    //db.Entry(converser).State = EntityState.Modified;
                    db.SaveChanges();

                    Converser return_conv = new Converser();
                    return_conv.Active = converser.Active;
                    return_conv.FullName = converser.FullName;
                    return_conv.UserName = converser.UserName;
                    return_conv.Password = converser.Password;
                    return_conv.Business = new Business();
                    return_conv.Business.Domain = converser.Business.Domain;
                    return Json(return_conv);
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


        // POST: /GetCommSessions/
        /*
        [WebApiEnabled]
         */
        [JsonpFilter]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult GetConversersToChat(string password, string username, string domain, string callback)
        {
            try
            {

                Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                if (converser == null)
                {
                    return Json(false);
                }

                var orig_conversers = (from m in db.Conversers
                                       where m.Business.ID == converser.Business.ID
                                       && m.Agent != null
                                       select m);

                List<Converser> return_conversers = new List<Converser>();

                foreach (Converser _converser in orig_conversers)
                {
                    Converser oConverser = new Converser();
                    oConverser.UserName = _converser.UserName;
                    oConverser.FullName = _converser.FullName;
                    //oConverser.Password = _converser.Password;
                    oConverser.Business = new Business();
                    oConverser.Business.Domain = _converser.Business.Domain;
                    oConverser.LastActive = _converser.LastActive;
                    oConverser.Active = _converser.Active;

                    TimeZone localZone = TimeZone.CurrentTimeZone;
                    if (oConverser.Active == true)
                    {
                        if (_converser.LastActive > localZone.ToUniversalTime(DateTime.Now).AddSeconds(-20))
                        {
                            oConverser.Active = true;
                        }
                        else
                        {
                            oConverser.Active = false;
                        }
                    }

                    return_conversers.Add(oConverser);
                }

                if (return_conversers.Count > 0)
                {
                    return Json(return_conversers);
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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}