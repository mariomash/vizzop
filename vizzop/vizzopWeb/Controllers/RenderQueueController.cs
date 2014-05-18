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
    public class RenderQueueController : Controller
    {
        //
        // GET: /RenderQueue/

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

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetRenderQueueJson()
        {
            Utils utils = new Utils();

            try
            {
                //Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (HttpContext.Session["converser"] == null)
                {
                    return null;
                }
                var converser = (Converser)HttpContext.Session["converser"];
                if (converser == null)
                {
                    return Json(null);
                }
                if (converser.Agent == null)
                {
                    return Json(null);
                }


                List<WebLocation> WebLocations = utils.GetRenderQueue();
                if (WebLocations != null)
                {

                    var aaData = WebLocations.Select(x => new[] { 
                        x.ConverserId.ToString(), 
                        x.ThumbNail,
                        x.ScreenCapture == null ? null : x.ScreenCapture.Width.ToString(),
                        x.ScreenCapture == null ? null : x.ScreenCapture.Height.ToString(),
                        x.Url,
                        x.Lang,
                        x.Ubication,
                        x.UserAgent,
                        x.Referrer, 
                        x.TimeStamp_First.ToString("o"),
                        utils.GetPrettyDate(x.TimeStamp_First),
                        x.TimeStamp_Last.ToString("o"),
                        utils.GetPrettyDate(x.TimeStamp_Last),
                        x.ScreenCapture == null ? null : x.ScreenCapture.CreatedOn.ToString("o"),
                        x.ScreenCapture == null ? null : utils.GetPrettyDate(x.ScreenCapture.CreatedOn),
                        x.ThumbNail == null ? null : x.ScreenCapture.PicturedOn.ToString("o"),
                        x.ThumbNail == null ? null : utils.GetPrettyDate(x.ScreenCapture.PicturedOn),
                        x.FullName,
                        x.UserName,
                        x.Domain,
                        x.Password,
                        x.WindowName
                        });

                    return Json(aaData, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(null);
                }

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(null);
            }
        }

    }
}
