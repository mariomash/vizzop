using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
//using Microsoft.Web.Mvc.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using vizzopWeb.Models;
//using Microsoft.ApplicationServer.Caching;

namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class RealTimeController : Controller
    {

        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Index()
        {
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

        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult TrackPageView(string trackID, string username, string password, string domain, string url, string referrer, string windowname, string callback)
        {
            try
            {
                if ((url != null) && (username != null) && (password != null) && (domain != null))
                {

                    Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }

                    // If you want it formated in some other way.
                    var headers = "{";
                    foreach (var key in HttpContext.Request.Headers.AllKeys)
                        headers += "'" + key + "':'" + Request.Headers[key] + "',";

                    headers = headers.TrimEnd(',') + "}";

                    string sIP = HttpContext.Request.ServerVariables["HTTP_CLIENT_IP"];
                    if (string.IsNullOrEmpty(sIP) == true) { sIP = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]; }
                    if (string.IsNullOrEmpty(sIP) == true) { sIP = HttpContext.Request.ServerVariables["REMOTE_ADDR"]; }
                    if (string.IsNullOrEmpty(sIP) == true) { utils.GrabaLog(Utils.NivelLog.error, "No IP to Track"); return Json(false); }
                    sIP = sIP.Split(',')[0];

                    string[] languages = HttpContext.Request.UserLanguages;
                    string language = null;
                    if (languages != null && languages.Length != 0) { language = languages[0].ToLowerInvariant().Trim(); }

                    string useragent = HttpContext.Request.UserAgent;
                    if (windowname == null)
                    {
                        windowname = "";
                    }

                    Status returnstatus = utils.TrackPageView(trackID, converser, url, referrer, language, useragent, sIP, headers, windowname);
                    if (returnstatus.Success == true)
                    {
                        return Json(returnstatus.Value.ToString());
                    }
                    else
                    {
                        return Json(false);
                    }
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
            }
        }

        [JsonpFilter]
        [AllowCrossSiteJson]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetScreenHtml(string UserName, string Domain, string WindowName, string guid)
        {
            try
            {

                ScreenCapture sc = null;
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime start_time = DateTime.Now;
                Boolean CaptureCheck = false;
                while (CaptureCheck == false)
                {
                    sc = utils.GetScreenCaptureHtml(UserName, Domain, WindowName);
                    if (sc != null)
                    {
                        if ((sc.GUID != guid) || (DateTime.Now > start_time.AddSeconds(20)))
                        {
                            CaptureCheck = true;
                        }
                        else
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(5));
                        }
                    }
                    else
                    {
                        if (DateTime.Now > start_time.AddSeconds(20))
                        {
                            CaptureCheck = true;
                        }
                        else
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(5));
                        }
                    }
                }
                if (sc == null)
                {
                    return Json(false);
                }
                else
                {
                    return Json(new
                    {
                        UserName = UserName,
                        Domain = Domain,
                        WindowName = sc.WindowName,
                        GUID = sc.GUID,
                        CreatedOn = sc.CreatedOn.ToString("HH:mm:ss:ff"),
                        ReceivedOn = sc.ReceivedOn.ToString("HH:mm:ss:ff"),
                        LatencyInMs = (DateTime.UtcNow - sc.CreatedOn).TotalSeconds,
                        Url = sc.Url,
                        Data = sc.Blob,
                        ScrollLeft = sc.ScrollLeft,
                        ScrollTop = sc.ScrollTop,
                        Width = sc.Width,
                        Height = sc.Height
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(null);
            }
        }

        [JsonpFilter]
        [AllowCrossSiteJson]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetScreen(string UserName, string Domain, string WindowName, string height, string width, string guid)
        {
            try
            {

                ScreenCapture sc = null;
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime start_time = DateTime.Now;
                Boolean CaptureCheck = false;
                while (CaptureCheck == false)
                {
                    sc = utils.GetScreenCapture(UserName, Domain, WindowName);
                    if (sc != null)
                    {
                        if ((sc.GUID != guid) || (DateTime.Now > start_time.AddSeconds(20)))
                        {
                            CaptureCheck = true;
                        }
                        else
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(5));
                        }
                    }
                    else
                    {
                        if (DateTime.Now > start_time.AddSeconds(20))
                        {
                            CaptureCheck = true;
                        }
                        else
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(5));
                        }
                    }
                }
                if (sc == null)
                {
                    return Json(false);
                }
                else
                {
                    return Json(new
                    {
                        UserName = sc.converser.UserName,
                        Domain = sc.converser.Business.Domain,
                        WindowName = sc.WindowName,
                        GUID = sc.GUID,
                        CreatedOn = sc.CreatedOn.ToString("HH:mm:ss:ff"),
                        ReceivedOn = sc.ReceivedOn.ToString("HH:mm:ss:ff"),
                        PicturedOn = sc.PicturedOn.ToString("HH:mm:ss:ff"),
                        ServedOn = DateTime.UtcNow.ToString("HH:mm:ss:ff"),
                        LatencyInMs = (DateTime.UtcNow - sc.CreatedOn).TotalSeconds,
                        Url = sc.Url,
                        Data = "data:image/jpg;base64," + sc.Data,
                        //Data = "data:image/jpg;base64," + utils.ImageToJpegBase64(utils.PrepareScreenToReturn(sc, width, height, false), 60L),
                        ScrollLeft = sc.ScrollLeft,
                        ScrollTop = sc.ScrollTop,
                        Width = sc.Width,
                        Height = sc.Height
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(null);
            }
        }


        [JsonpFilter]
        [AllowCrossSiteJson]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetScreenByGUID(string UserName, string Domain, string height, string width, string guid)
        {
            try
            {

                ScreenCapture sc = null;
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime start_time = DateTime.Now;
                sc = utils.GetScreenCaptureByGUID(UserName, Domain, guid);
                if (sc == null)
                {
                    return Json(false);
                }
                else
                {

                    return Json(new
                    {
                        UserName = sc.converser.UserName,
                        Domain = sc.converser.Business.Domain,
                        GUID = sc.GUID,
                        CreatedOn = sc.CreatedOn.ToLocalTime().ToString("G"),
                        Url = sc.Url,
                        Data = "data:image/jpg;base64," + utils.ImageToJpegBase64(utils.PrepareScreenToReturn(sc, width, height, false), 75L)
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(null);
            }
        }


        [ValidateInput(false)]
        [JsonpFilter]
        [AllowCrossSiteJson]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult TrackScreen(string username, string password, string domain, string data, string listeners)
        {
            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                /*
                var wrapper = new HttpContextWrapper(HttpContext);
                */
                utils.TrackScreen(
                    username,
                    password,
                    domain,
                    data,
                    listeners,
                    HttpContext
                    );
                return Json(true);

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(false);
            }
        }



        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetWebLocationsJson()
        {
            try
            {
                //Converser converser = utils.GetLoggedConverser(HttpContext.Session);
                if (HttpContext.Session["converser"] == null)
                {
                    return null;
                }
                var converser = (Converser)HttpContext.Session["converser"];
                try
                {
                    if (converser == null)
                    {
                        return Json(null);
                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return Json(null);
                }

                /*
                    Thread_GetWebLocationsHelper oThread_GetWebLocationsHelper = new Thread_GetWebLocationsHelper(converser);
                    oThread_GetWebLocationsHelper.DoThings();
                 */


                List<WebLocation> WebLocations = new List<WebLocation>();

                string tag = "weblocation";
                List<DataCacheTag> Tags = new List<DataCacheTag>();
                Tags.Add(new DataCacheTag(tag));
                object result = SingletonCache.Instance.GetByTag(tag);
                if (result != null)
                {
                    IEnumerable<KeyValuePair<string, object>> ObjectList = (IEnumerable<KeyValuePair<string, object>>)result;

                    foreach (var e in ObjectList)
                    {
                        WebLocations.Add((WebLocation)e.Value);
                    }
                }

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = localZone.ToUniversalTime(DateTime.Now.AddSeconds(-20));
                WebLocations = (from m in WebLocations
                                where m.TimeStamp_Last > loctime
                                select m).OrderByDescending(z => z.TimeStamp_Last).ToList();

                List<WebLocationDataTables> DefLocList = new List<WebLocationDataTables>();

                /*
                string VizzopGetsAllRealtimeWebLocationsSetting = "VizzopGetsAllRealtimeWebLocationsInRelease";
#if DEBUG
                VizzopGetsAllRealtimeWebLocationsSetting = "VizzopGetsAllRealtimeWebLocationsInDebug";
#endif
                bool VizzopGetsAllRealtimeWebLocations = Convert.ToBoolean((from m in db.Settings
                                                                            where m.Name == VizzopGetsAllRealtimeWebLocationsSetting
                                                                            select m).FirstOrDefault().Value);
                */
                var VizzopGetsAllRealtimeWebLocations = true;

                foreach (WebLocation wl in WebLocations)
                {
                    try
                    {

                        //solo seguimos palante si es vizzop y el setting 
                        if ((converser.Business.Domain.ToLowerInvariant() != "vizzop") || (VizzopGetsAllRealtimeWebLocations == false))
                        {
                            if (wl.Domain != converser.Business.Domain)
                            {
                                continue;
                            }
                        }

                        //Solo añadimos los no duplicados...
                        if (DefLocList.Any(m => m.WindowName == wl.WindowName) == false)
                        {
                            WebLocationDataTables loc = new WebLocationDataTables();
                            loc.ID = wl.ConverserId;
                            loc.Url = wl.Url;
                            loc.Referrer = wl.Referrer;
                            loc.IP = wl.IP;
                            loc.Lang = wl.Lang;
                            loc.Ubication = wl.Ubication;
                            loc.UserAgent = wl.UserAgent;
                            loc.FirstViewed = wl.TimeStamp_First;
                            loc.FirstViewedHuman = utils.GetPrettyDate(wl.TimeStamp_First);
                            loc.LastViewed = wl.TimeStamp_Last;
                            loc.LastViewedHuman = utils.GetPrettyDate(wl.TimeStamp_Last).ToString();
                            loc.UserName = wl.UserName;
                            loc.Domain = wl.Domain;
                            loc.Password = wl.Password;
                            loc.FullName = wl.FullName != null ? wl.FullName : "Anonymous";
                            loc.WindowName = wl.WindowName;
                            DefLocList.Add(loc);
                        }
                    }
                    catch (Exception e)
                    {
                        utils.GrabaLogExcepcion(e);
                    }
                }

                if (DefLocList.Count > 0)
                {
                    var aaData = DefLocList.Select(x => new[] { 
                        x.ID.ToString(), 
                        "",
                        "",
                        "",
                        x.Url,
                        x.Lang,
                        x.Ubication,
                        x.UserAgent,
                        x.Referrer, 
                        x.FirstViewed.ToString("o"),
                        x.FirstViewedHuman,
                        x.LastViewed.ToString("o"),
                        x.LastViewedHuman,
                        x.FullName,
                        x.UserName,
                        x.Domain,
                        x.Password,
                        x.WindowName,
                        ""
                        }).ToList();

                    foreach (var item in aaData)
                    {
                        string thumb = utils.GetScreenCaptureThumbnail(item[14], item[15], item[17]);
                        if (thumb != null)
                        {
                            item[1] = thumb;
                        }
                        ScreenCapture sc = utils.GetScreenCapture(item[14], item[15], item[17]);
                        if (sc != null)
                        {
                            /*
                            item[1] = "data:image/jpg;base64," + utils.ImageToJpegBase64(
                                utils.PrepareScreenToReturn(sc, "140", "90", false), 90L
                                );
                             * */
                            item[2] = sc.Width.ToString();
                            item[3] = sc.Height.ToString();
                        }
                    }

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
