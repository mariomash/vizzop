using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;
//using Microsoft.Web.Mvc.Resources;

namespace vizzopWeb.Controllers
{

    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class AuditController : Controller
    {

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Index()
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
                return View();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return View();
            }
        }


    }
}