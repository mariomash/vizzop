using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class LogController : Controller
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
                if (converser.Business.Email == "customer.service@vizzop.com")
                {
                    return View(db.Logs.ToList());
                }
                else
                {
                    return RedirectToAction("LogOn", "Account");
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        // POST: /Messages/Send
        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult SaveFromAjax(String text, string callback)
        {
            Utils utils = new Utils();
            //text = Request.ServerVariables["REMOTE_ADDR"] + " : " + text;
            utils.GrabaLogJavascript(text);
            return Json(true);
        }

        //
        // GET: /Log/Details/5
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Details(int id)
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
                if (converser.Business.Email == "customer.service@vizzop.com")
                {
                    Log log = db.Logs.Find(id);
                    return View(log);
                }
                else
                {
                    return RedirectToAction("LogOn", "Account");
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}