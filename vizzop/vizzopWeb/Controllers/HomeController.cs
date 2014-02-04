using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
//using System.Web.Mvc.Resources;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{
    public class HomeController : Controller
    {
        vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

#if DEBUG
#else                    
        [RequireHttps]
#endif
        //[OutputCache(Duration = 10, VaryByParam = "none")]
        [OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult Index(bool force = false)
        {
            /*
            try
            {
                var list = (from m in db.WebLocations_History.Include("Converser")
                            select m);
                foreach (WebLocation_History loc in list)
                {
                    loc.Ubication = utils.GetUbicationFromIP(loc.IP);
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
            */

            try
            {
                if ((Session["converser"] != null) && (force == false))
                {
                    Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                    if (converser != null)
                    {
                        return RedirectToAction("Index", "Panel");
                    }
                }
                //return RedirectToAction("LogOn", "Account");
                string langISO = utils.GetLang(HttpContext);
                if (langISO.IndexOf("-") > -1)
                {
                    langISO = langISO.Split('-')[0];
                }
                switch (langISO.ToLowerInvariant())
                {
                    case "es":
                        return RedirectToAction("Es", "vizzop");
                    default:
                        return RedirectToAction("En", "vizzop");
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
            return View();
            //return null;
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult ErrorPage(string panelmode)
        {
            if (panelmode == "true")
            {
                try
                {
                    Converser converser = utils.GetLoggedConverser(HttpContext.Session);
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
                }
                ViewBag.LayoutPanelMode = true;
                ViewBag.LayoutSubPanelMode = true;
            }
            return View();
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult About()
        {
            return View();
        }

        /*
        [RequireHttps]
        public ActionResult vizzop()
        {
            return View();
        }
        */

    }
}
