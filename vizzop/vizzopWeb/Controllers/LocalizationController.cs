using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;
//using Microsoft.Web.Mvc.Resources;

namespace vizzopWeb.Controllers
{
    public class LocalizationController : Controller
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();
        //
        // GET: /Localization/
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
                Converser converser = utils.GetLoggedConverser(HttpContext.Session, db);
                if (converser == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                converser.Business.Conversers = new List<Converser>();
                ViewBag.converser = converser;
                if (converser.Business.Email == "customer.service@vizzop.com")
                {
                    return View(db.TextStrings.ToList());
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
        public ActionResult GetLocalizationJson()
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
                    converser = utils.GetLoggedConverser(HttpContext.Session, db);
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

                List<TextString> locList = (from m in db.TextStrings
                                            select m).ToList<TextString>();

                if (locList.Count() > 0)
                {
                    return Json(new
                    {
                        aaData = locList.Select((x, index) => new Object[] {
                                    x.ID,
                                    x.IsoCode,
                                    x.Ref,
                                    x.Text
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


        [JsonpFilter]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult GetTextStringDetails(string id)
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
                    converser = utils.GetLoggedConverser(HttpContext.Session, db);
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

                Int32 _id = Convert.ToInt32(id);
                TextString ts = (from m in db.TextStrings
                                 where m.ID == _id
                                 select m).FirstOrDefault();

                if (ts != null)
                {
                    return Json(ts, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null);
                }

            }
            catch (Exception _ex)
            {
                utils.GrabaLogExcepcion(_ex);
                return Json(null);
            }

        }


        [JsonpFilter]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult SaveTextString(string reference, string text, string langisocode, string id)
        {
            Converser converser = new Converser();
            try
            {
                if (HttpContext.Session == null)
                {
                    return Json(false);
                }
                try
                {
                    converser = utils.GetLoggedConverser(HttpContext.Session, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(false);
                }

                TextString loc = null;
                if ((id != null) && (id != "null") & (id != ""))
                {
                    var _id = Convert.ToInt32(id);

                    loc = (from m in db.TextStrings
                           where m.ID == _id
                           select m).FirstOrDefault();

                    if (loc != null)
                    {
                        loc.Ref = reference;
                        loc.Text = text;
                        loc.IsoCode = langisocode;
                    }

                    db.SaveChanges();
                }
                else
                {

                    loc = new TextString();
                    loc.IsoCode = langisocode;
                    loc.Ref = reference;
                    loc.Text = text;

                    db.TextStrings.Add(loc);
                    db.SaveChanges();
                }

                return Json(loc.ID);
            }
            catch (Exception _ex)
            {
                utils.GrabaLogExcepcion(_ex);
                return Json(false);
            }

        }


        [JsonpFilter]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult DeleteTextString(string id)
        {
            Converser converser = new Converser();
            try
            {
                if (HttpContext.Session == null)
                {
                    return Json(false);
                }
                try
                {
                    converser = utils.GetLoggedConverser(HttpContext.Session, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(false);
                }

                if ((id != null) && (id != "null"))
                {
                    var _id = Convert.ToInt32(id);
                    var loc = (from m in db.TextStrings
                               where m.ID == _id
                               select m).FirstOrDefault();
                    if (loc != null)
                    {
                        db.TextStrings.Remove(db.TextStrings.Single<TextString>(m => m.ID == _id));
                        db.SaveChanges();
                        return Json(true);
                    }
                    else
                    {
                        db.SaveChanges();
                        return Json(id);
                    }
                }
                else
                {
                    return Json(false);
                }

            }
            catch (Exception _ex)
            {
                utils.GrabaLogExcepcion(_ex);
                return Json(false);
            }

        }


        [JsonpFilter]
        [AllowCrossSiteJson]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult GetIsoCodes()
        {
            try
            {
                var isocodes = (from m in db.Isocodes
                                select m).ToList();
                return Json(isocodes);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(null);
            }
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}