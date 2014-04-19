using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{

    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class MailingController : Controller
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

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult GetQueueJson()
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

                List<Message> ReturnList = new List<Message>();

                ReturnList = (from m in db.Messages
                              where m.Sent == 0 && m.MessageType == "email"
                              select m).ToList<Message>();

                return Json(new
                {
                    aaData = ReturnList.Select((x, index) => new Object[] { 
                                x.TimeStamp,
                                x.Sent,
                                x.Status,
                                x.MessageType,
                                x.Subject,
                                x.Lang,
                                x.Content
                            })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception _ex)
            {
                utils.GrabaLogExcepcion(_ex);
                return Json(null);
            }

        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult SendMail(int id)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            try
            {
                var message = (from m in db.Messages
                               where m.ID == id
                               select m).FirstOrDefault();

                //Message message = new Message(newmessage);
                Email email = new Email();
                email.message = message;
                if ((email.send()) == true)
                {
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }

            }
            catch (Exception _ex)
            {
                utils.GrabaLogExcepcion(_ex);
                return Json(true);
            }

        }


    }
}
