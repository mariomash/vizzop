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

        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult TrackPageView(string url, string referrer, string windowname, string callback)
        {
            Utils utils = new Utils();

            try
            {
                if (url != null)
                {

                    if (HttpContext.Session["converser"] == null)
                    {
                        return null;
                    }
                    var converser = (Converser)HttpContext.Session["converser"];
                    if (converser == null)
                    {
                        return Json(false);
                    }

                    /*
                    Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }
                    */

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

                    Status returnstatus = utils.TrackPageView(converser, url, referrer, language, useragent, sIP, headers, windowname);
                    if (returnstatus.Success == true)
                    {
                        return Json(true);
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
            Utils utils = new Utils();

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
            Utils utils = new Utils();

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
            using (var db = new vizzopContext())
            {
                Utils utils = new Utils(db);

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
        }


        [ValidateInput(false)]
        [JsonpFilter]
        [AllowCrossSiteJson]
#if DEBUG
#else
        [RequireHttps]
#endif
        //public ActionResult TrackScreen(string data, string listeners)
        public ActionResult TrackScreen(string username, string password, string domain, string data, string listeners)
        {
            Utils utils = new Utils();

            try
            {

                /*
                if (HttpContext.Session["converser"] == null)
                {
                    return null;
                }
                var converser = (Converser)HttpContext.Session["converser"];
                if (converser == null)
                {
                    return Json(false);
                }
                */
                /*
                var wrapper = new HttpContextWrapper(HttpContext);
                */
                utils.TrackScreen(
                    username,
                    password,
                    domain,
                    data,
                    listeners,
                    false,
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

                //List<WebLocation> WebLocations = new List<WebLocation>();

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = localZone.ToUniversalTime(DateTime.Now.AddSeconds(-30));

                string region = "weblocations";
                /*
                List<DataCacheTag> Tags = new List<DataCacheTag>();
                Tags.Add(new DataCacheTag(tag));
                object result = SingletonCache.Instance.GetByTag(tag);*/

                object result = SingletonCache.Instance.GetAllInRegion(region);
                if (result != null)
                {

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

                    IEnumerable<KeyValuePair<string, object>> WebLocations = (IEnumerable<KeyValuePair<string, object>>)result;

                    WebLocations = (from m in WebLocations
                                    where ((WebLocation)m.Value).TimeStamp_Last > loctime
                                    select m);

                    if ((converser.Business.Domain.ToLowerInvariant() != "vizzop") || (VizzopGetsAllRealtimeWebLocations == false))
                    {
                        WebLocations = (from m in WebLocations
                                        where ((WebLocation)m.Value).Domain == converser.Business.Domain
                                        select m);
                    }

                    var GroupedWebLocations = WebLocations.GroupBy(m => ((WebLocation)m.Value).ConverserId);

                    if (GroupedWebLocations.Count() > 0)
                    {
                        List<WebLocation> _WebLocations = new List<WebLocation>();
                        foreach (var Group in GroupedWebLocations)
                        {
                            var m = Group.OrderByDescending(x => ((WebLocation)x.Value).TimeStamp_Last).FirstOrDefault();
                            _WebLocations.Add((WebLocation)m.Value);
                        }


                        var aaData = _WebLocations.Select(x => new[] { 
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
                        x.FullName,
                        x.UserName,
                        x.Domain,
                        x.Password,
                        x.WindowName
                        });

                        /*
                        foreach (var item in aaData)
                        {
                            ScreenCapture sc = utils.GetScreenCapture(item[14], item[15], item[17]);
                            if (sc != null)
                            {
                                item[2] = sc.Width.ToString();
                                item[3] = sc.Height.ToString();
                            }
                        }
                        */

                        return Json(aaData, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(null);
                    }
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
