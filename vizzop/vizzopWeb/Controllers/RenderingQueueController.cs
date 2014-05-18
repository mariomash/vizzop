using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using vizzopWeb.Models;


namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class RenderingQueueController : Controller
    {
        //
        // GET: /RenderingQueue/

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Index()
        {
            using (var db = new vizzopContext())
            {
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
}
