using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{
    public class CRMController : Controller
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();
        //
        // GET: /CRM/

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Index()
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
                if (converser.Business.Email == "customer.service@vizzop.com")
                {
                    return View(db.Businesses.ToList());
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


        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetCRMJson()
        {
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
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(null);
                }

                List<Business> businessList = (from m in db.Businesses
                                               select m).ToList<Business>();

                if (businessList.Count() > 0)
                {
                    return Json(new
                    {
                        aaData = businessList.Select((x, index) => new Object[] {
                                    x.ID,
                                    x.BusinessName,
                                    x.ApiKey,
                                    x.Email,
                                    x.Phone
                                })
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                //return Json(ScreenList);
            }
            catch (Exception ex)
            {
                //utils.GrabaLogExcepcion(ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
