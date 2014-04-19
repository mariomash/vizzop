using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;


namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class FaqsController : Controller
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

        [JsonpFilter]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult GetFaqsJson()
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
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(null);
                }

                List<Faq> ReturnList = new List<Faq>();

                ReturnList = (from m in db.Faqs
                              where m.Business.ID == converser.Business.ID
                              select m).ToList<Faq>();

                return Json(new
                {
                    aaData = ReturnList.Select((x, index) => new Object[] { 
                        x.ID,
                        x.TimeStamp,
                        utils.GetPrettyDate(x.TimeStamp),
                        string.Join(",", (from xx in db.FaqDetails
                                              where xx.Faq.ID == x.ID
                                              select xx.LangISOCode).ToList()),
                        x.FaqDetails.FirstOrDefault().Question,
                        x.FaqDetails.FirstOrDefault().Answer
                    })
                }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetFaqDetails(string id)
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
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(null);
                }

                Int32 _id = Convert.ToInt32(id);
                Faq faq = (from m in db.Faqs
                           where m.ID == _id
                           select m).FirstOrDefault();

                return Json(new Faq()
                {
                    ID = faq.ID,
                    Business = null,
                    TimeStamp = faq.TimeStamp,
                    FaqDetails = (from mm in faq.FaqDetails
                                  select new FaqDetails()
                                  {
                                      ID = mm.ID,
                                      Answer = mm.Answer,
                                      Question = mm.Question,
                                      TimeStamp = mm.TimeStamp,
                                      LangISOCode = mm.LangISOCode,
                                      Faq = null
                                  }).ToList<FaqDetails>()
                }, JsonRequestBehavior.AllowGet);

                /*
                return Json(new
                {
                    aaData = ReturnList.Select((x, index) => new Object[] { 
                        x.ID,
                        x.TimeStamp,
                        utils.GetPrettyDate(x.TimeStamp),
                        string.Join(",", (from xx in db.FaqDetails
                                              where xx.Faq.ID == x.ID
                                              select xx.LangISOCode).ToList()),
                        x.FaqDetails.FirstOrDefault().Question,
                        x.FaqDetails.FirstOrDefault().Answer
                    })
                }, JsonRequestBehavior.AllowGet);
                 */

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
        public ActionResult SaveFaq(string question, string answer, string langisocode, string id)
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
                    converser.Business.Conversers = new List<Converser>();
                    ViewBag.converser = converser;
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(false);
                }

                Faq faq = new Faq();
                if ((id != null) && (id != "null"))
                {
                    var _id = Convert.ToInt32(id);

                    faq = (from m in db.Faqs
                           where m.ID == _id
                           select m).FirstOrDefault();

                    var found = false;
                    foreach (FaqDetails details in faq.FaqDetails)
                    {
                        if (details.LangISOCode == langisocode)
                        {
                            found = true;
                            details.Answer = answer;
                            details.Question = question;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        FaqDetails details = new FaqDetails();
                        details.Answer = answer;
                        details.Question = question;
                        details.LangISOCode = langisocode;
                        faq.FaqDetails.Add(details);
                    }

                    db.SaveChanges();
                }
                else
                {

                    var business = (from m in db.Businesses
                                    where m.ID == converser.Business.ID
                                    select m).FirstOrDefault();
                    faq.Business = business;

                    FaqDetails details = new FaqDetails();
                    details.Answer = answer;
                    details.Question = question;
                    details.LangISOCode = langisocode;

                    faq.FaqDetails = new List<FaqDetails>();
                    faq.FaqDetails.Add(details);
                    db.Faqs.Add(faq);
                    db.SaveChanges();
                }

                return Json(faq.ID);
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
        public ActionResult DeleteFaqDetails(string id, string lang)
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
                    var faq = (from m in db.Faqs
                               where m.ID == _id
                               select m).FirstOrDefault();
                    faq.FaqDetails.Remove(faq.FaqDetails.Single<FaqDetails>(m => m.LangISOCode == lang));
                    if (faq.FaqDetails.Count == 0)
                    {
                        db.Faqs.Remove(db.Faqs.Single<Faq>(m => m.ID == _id));
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

    }
}
