using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;
//using Microsoft.Web.Mvc.Resources;

namespace vizzopWeb.Controllers
{
    public class AccountController : Controller
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult ReminderSent()
        {

            return View();
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Reminder()
        {

            return View();
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult PasswordReset(string key)
        {
            try
            {
                var passwordreset = (from m in db.PasswordResets
                                     where m.Key == key
                                     select m).FirstOrDefault();
                if (passwordreset != null)
                {
                    return View(passwordreset);
                }
                else
                {
                    return RedirectToAction("ErrorPage", "Home");
                }
            }
            catch (System.Exception ex)
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
        public ActionResult PasswordReset(PasswordReset newpasswordreset)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var passwordreset = (from m in db.PasswordResets
                                         where m.Key == newpasswordreset.Key
                                         select m).FirstOrDefault();
                    if (passwordreset == null)
                    {
                        return RedirectToAction("ErrorPage", "Home");
                    }

                    /*
                     When the Email attribute has been created,
                     we should change passwordreset.UserName to passwordreset.UserName
                     */
                    Converser converser = utils.GetConverserFromSystemWithEmailAndBusinessId(passwordreset.UserName, passwordreset.Business.ID, db);

                    if (converser != null)
                    {
                        converser.Password = newpasswordreset.Password;
                        db.SaveChanges();

                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/PasswordResetOK/" + passwordreset.UserName);
                    }
                    else
                    {

                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/PasswordResetNotOK/" + passwordreset.UserName);
                    }

                    LogOn logon = new LogOn();
                    logon.Email = converser.Email;
                    logon.Password = newpasswordreset.Password;

                    db.PasswordResets.Remove(passwordreset);
                    db.SaveChanges();

                    Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/PasswordResetOK/" + passwordreset.UserName);

                    return this.LogOn(logon, null, null, null);
                }
                else
                {
                    return View(newpasswordreset);
                }
            }
            catch (System.Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                ViewBag.errors = "Error resetting your password, please check your data and try again";
                return View(newpasswordreset);
            }
        }

        [HttpPost]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult Reminder(string email)
        {
            try
            {

                Converser converser = (from m in db.Conversers.Include("Business")
                                       where m.Email == email
                                       select m).FirstOrDefault();

                if (converser == null)
                {
                    Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/ReminderNotOK/" + email);

                    ViewBag.errors = "No account associated with this e-mail";
                    return View();
                }

                PasswordReset passwordreset = new PasswordReset();
                /*
                 We should create the attribute Email on PasswordReset. 
                 Momentarily, we put the Email on passwordreset.UserName
                 */
                passwordreset.UserName = converser.Email;
                passwordreset.Business = converser.Business;
                passwordreset.Password = converser.Password;
                passwordreset.ConfirmPassword = converser.Password;
                db.PasswordResets.Add(passwordreset);
                db.SaveChanges();

                NewMessage newmessage = new NewMessage();
                newmessage.From = "admin@vizzop";
                newmessage.To = converser.UserName + "@" + converser.Business.Domain;
                newmessage.Lang = utils.GetLang(HttpContext);
                newmessage.Subject = utils.LocalizeLang("email_reminder_subject", newmessage.Lang, null);
                /*
                change UserName to Email, we don't want to send the UserName to the Business(our client), just the Email
                string[] args = { converser.UserName, "https://vizzop.com/" + Url.Action("PasswordReset", new { key = passwordreset.Key }) };
                */
                string[] args = { converser.Email, "https://vizzop.com/" + Url.Action("PasswordReset", new { key = passwordreset.Key }) };
                newmessage.Content = utils.LocalizeLang("email_reminder_contents", newmessage.Lang, args);
                newmessage.db = db;
                newmessage.MessageType = "email";
                Message message = new Message(newmessage);

                var _conv = (from m in db.Conversers
                             where m.UserName == "admin" && m.Business.Domain == "vizzop"
                             select m).FirstOrDefault();

                message.From = _conv;
                message.To = converser;
                Boolean result = message.Send();
                if (result == false)
                {
                    ViewBag.errors = "Error. Please try again";
                    Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/ReminderOK/" + email);
                    return View();
                }
                else
                {
                    return RedirectToAction("reminderSent");
                }
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
                return View();
            }
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult LogOff()
        {
            try
            {
                Converser converser = utils.GetLoggedConverser(HttpContext.Session, db);
                if (converser != null)
                {
                    Session["converser"] = null;
                }

            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
            }
            return View();
        }

#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult LogOn()
        {
            if (Session["converser"] != null)
            {
                Session["converser"] = null;
                return RedirectToAction("LogOn");
            }
            ViewBag.nologinbox = true;

            return View();
        }

        [HttpPost]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public ActionResult LogOn(LogOn logon, string returnUrl, string Email, string Password)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (Session["converser"] != null)
                    {
                        Session["converser"] = null;
                        return RedirectToAction("LogOn");
                    }

                    if ((logon.Email == null) || (logon.Password == null))
                    {
                        if ((Email != null) && (Password != null))
                        {
                            logon = new LogOn();
                            logon.Email = Email;
                            logon.Password = Password;
                        }
                    }

                    if ((logon.Email == null) || (logon.Password == null))
                    {
                        return RedirectToAction("LogOn");
                    }


                    logon.Email = logon.Email.Trim();
                    logon.Password = logon.Password.Trim();

                    Converser converser = utils.GetConverserFromSystem(logon.Email, logon.Password, db);

                    if (converser == null)
                    {
                        Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/LogOnNotOK/" + logon.Email);
                        ViewBag.errors = "Combination of email/password not found";
                        return View();
                    }

                    Utils.GrabaAnalyticsLog(Utils.NivelLog.info, "Account/LogOnOK/" + logon.Email);

                    Session["converser"] = converser;
                    utils.AddZenSession(converser, Session.SessionID);

                    try
                    {
                        converser.Active = true;
                        converser.LastActive = DateTime.Now.ToUniversalTime();
                        db.SaveChanges();
                    }
                    catch (Exception _e)
                    {
                        utils.GrabaLogExcepcion(_e);
                    }

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        if (converser.Agent != null)
                        {
                            return RedirectToAction("Index", "Panel");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }

                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
                return View();
            }
        }

    }
}
