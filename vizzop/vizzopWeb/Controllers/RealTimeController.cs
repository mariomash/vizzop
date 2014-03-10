using System;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.Web.Mvc.Resources;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using vizzopWeb.Models;
//using Microsoft.ApplicationServer.Caching;


namespace vizzopWeb
{

    public class Thread_GetWebLocationsHelper
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();
        public Converser _Converser = new Converser();

        public Thread_GetWebLocationsHelper(Converser oConverser)
        {
            this._Converser = oConverser;
        }

        public void DoThings()
        {
            try
            {

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = localZone.ToUniversalTime(DateTime.Now.AddSeconds(-30));
                var to_move = (from m in db.WebLocations.Include("Converser").Include("Converser.Business")
                               where m.TimeStamp_Last < loctime
                               //&& m.Converser.Business.ID == _Converser.Business.ID
                               select m);
                if (to_move != null)
                {
                    foreach (var m in to_move)
                    {
                        WebLocation_History newloc = new WebLocation_History();
                        newloc.converser = m.Converser;
                        newloc.Referrer = m.Referrer;
                        newloc.TimeStamp_First = m.TimeStamp_First;
                        newloc.TimeStamp_Last = m.TimeStamp_Last;
                        newloc.IP = m.IP;
                        newloc.Lang = m.Lang;
                        newloc.UserAgent = m.UserAgent;
                        newloc.Url = m.Url;
                        newloc.Ubication = m.Ubication;
                        newloc.Headers = m.Headers;
                        if (newloc.converser != null)
                        {
                            db.WebLocations_History.Add(newloc);
                        }
                        db.WebLocations.Remove(m);
                    }
                    if (to_move.Count() > 0)
                    {
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception _ex)
            {
                utils.GrabaLogExcepcion(_ex);
            }
        }
    }

    public class Thread_TrackScreenHelper
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

        public string _UserName { get; set; }
        public string _Password { get; set; }
        public string _Domain { get; set; }
        public ScreenCapture _ScreenCapture { get; set; }

        public Thread_TrackScreenHelper(string UserName, string Password, string Domain, ScreenCapture sc)
        {
            this._UserName = UserName;
            this._Password = Password;
            this._Domain = Domain;
            this._ScreenCapture = sc;

        }

        public void DoThings()
        {
            try
            {

                //Si ya existe la actualizamos
                ScreenCapture sc = null;
                sc = (from m in db.ScreenCaptures
                      where m.GUID == this._ScreenCapture.GUID
                      select m).FirstOrDefault();

                if (sc == null)
                {
                    Converser converser = utils.GetConverserFromSystem(this._UserName, this._Password, this._Domain, db);
                    if (converser == null)
                    {
                        return;
                    }
                    else
                    {
                        this._ScreenCapture.converser = converser;
                    }

                    /*
                    if ((this._ScreenCapture.Data == null) || (this._ScreenCapture.Data == ""))
                    {
                        var penul_SC = (from m in db.ScreenCaptures
                                        where m.converser.ID == converser.ID && m.Data != null
                                        orderby m.ID descending
                                        select m).FirstOrDefault();
                        if (penul_SC != null)
                        {
                            if (penul_SC.Data != null)
                            {
                                this._ScreenCapture.Data = penul_SC.Data;
                            }
                        }
                    }

                    if (this._ScreenCapture.Data == null)
                    {
                        return;
                    }
                    */

                    db.ScreenCaptures.Add(this._ScreenCapture);
                }
                else
                {
                    if (sc.converser == null)
                    {
                        Converser converser = utils.GetConverserFromSystem(this._UserName, this._Password, this._Domain, db);
                        if (converser == null)
                        {
                            return;
                        }
                        else
                        {
                            sc.converser = converser;
                        }
                    }

                    sc.GUID = this._ScreenCapture.GUID;
                    sc.Headers = this._ScreenCapture.Headers;
                    sc.Height = this._ScreenCapture.Height;
                    sc.MouseX = this._ScreenCapture.MouseX;
                    sc.MouseY = this._ScreenCapture.MouseY;
                    sc.ScrollLeft = this._ScreenCapture.ScrollLeft;
                    sc.ScrollTop = this._ScreenCapture.ScrollTop;
                    sc.Url = this._ScreenCapture.Url;
                    sc.Width = this._ScreenCapture.Width;
                    sc.Blob = this._ScreenCapture.Blob;
                    sc.Data = this._ScreenCapture.Data;
                }
                db.SaveChanges();

                //string strSQL = "DELETE FROM SCREENCAPTURES WHERE createdon < DATEADD(minute,-10,CURRENT_TIMESTAMP);";

                //Tendremos screeenshots de hasta hace dos días...
                //string strSQL = "DELETE FROM SCREENCAPTURES WHERE createdon < DATEADD(day,-7,CURRENT_TIMESTAMP);";
                //db.Database.ExecuteSqlCommand(strSQL);

            }
            catch (Exception _ex)
            {
                utils.GrabaLogExcepcion(_ex);
            }
        }
    }
}

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
        public ActionResult TrackPageExit(string username, string password, string domain, string url, string callback)
        {
            try
            {
                if ((Url != null) && (username != null) && (password != null) && (domain != null))
                {

                    if (url.Length > 3999)
                    {
                        url = url.Substring(0, 3999);
                    }

                    Converser converser = utils.GetConverserFromSystem(username, password, domain, db);
                    if (converser == null)
                    {
                        return Json(false);
                    }

                    var pageview = (from m in db.WebLocations.Include("Converser").Include("Converser.Business")
                                    where m.Converser.ID == converser.ID &&
                                    m.Url == url
                                    select m).FirstOrDefault();

                    if (pageview != null)
                    {
                        WebLocation_History newloc = new WebLocation_History();
                        newloc.converser = converser;
                        newloc.Referrer = pageview.Referrer;
                        newloc.TimeStamp_First = pageview.TimeStamp_First;
                        newloc.TimeStamp_Last = pageview.TimeStamp_Last;
                        newloc.UserAgent = pageview.UserAgent;
                        newloc.IP = pageview.IP; //HttpContext.Request.ServerVariables("REMOTE_ADDR")
                        newloc.Lang = pageview.Lang;
                        newloc.Url = pageview.Url;
                        newloc.Ubication = pageview.Ubication;
                        newloc.Headers = pageview.Headers;


                        db.WebLocations.Remove(pageview);
                        db.SaveChanges();


                        db.WebLocations_History.Add(newloc);
                        db.SaveChanges();
                    }
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception e)
            {
                utils.GrabaLogExcepcion(e);
                return Json(false);
            }
        }

        [ValidateInput(false)]
        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult TrackPageView(string trackID, string username, string password, string domain, string url, string referrer, string callback)
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
                    string useragent = HttpContext.Request.UserAgent;
                    Status returnstatus = utils.TrackPageView(trackID, converser, url, referrer, languages[0], useragent, sIP, headers, db);
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

        /*
        [JsonpFilter]
        [AllowCrossSiteJson]
#if DEBUG
#else                    
        [RequireHttps]
#endif
        public FileResult GetScreenImage(string UserName, string Domain, string height, string width)
        {
            try
            {
                ScreenCapture sc = null;
                ScreenCapture penul_sc = null;
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime start_time = DateTime.Now;
                Boolean CaptureCheck = false;
                while (CaptureCheck == false)
                {
                    sc = utils.GetScreenCapture(UserName, Domain, false);
                    if (sc == null)
                    {
                        sc = new ScreenCapture();
                    }

                    penul_sc = utils.GetScreenCapture(UserName, Domain, true);
                    if (penul_sc == null)
                    {
                        penul_sc = new ScreenCapture();
                    }

                    if (((sc.Data != null) && (penul_sc.Data != sc.Data)) || (DateTime.Now > start_time.AddSeconds(20)))
                    {
                        CaptureCheck = true;
                    }

                    Thread.Sleep(TimeSpan.FromMilliseconds(50));
                }


                if (sc == null)
                {
                    string mime_type = "image/jpeg";
                    MemoryStream ms = new MemoryStream();
                    byte[] imageBytes = ms.ToArray();
                    return base.File(imageBytes, mime_type);
                }
                else
                {

                    Byte[] bitmapData = new Byte[sc.Data.Length];
                    bitmapData = Convert.FromBase64String(utils.FixBase64ForImage(sc.Data.Substring(sc.Data.IndexOf(',') + 1)));
                    System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
                    Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

                    Rectangle cropRect = new Rectangle(sc.ScrollLeft, sc.ScrollTop, sc.Width, sc.Height);
                    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(bitImage,
                            new Rectangle(0, 0, target.Width, target.Height),
                            cropRect,
                            GraphicsUnit.Pixel);
                    }

                    if ((height == null) || (width == null))
                    {
                        bitImage = target;
                    }
                    else
                    {
                        ThumbCreator thumb = new ThumbCreator();
                        bitImage = thumb.Resize(target, Convert.ToInt16(height), Convert.ToInt16(width), ThumbCreator.VerticalAlign.Top, ThumbCreator.HorizontalAlign.Left);
                    }

                    string mime_type = "image/jpeg";
                    MemoryStream ms = new MemoryStream();
                    bitImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] imageBytes = ms.ToArray();

                    return File(imageBytes, mime_type);

                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                string mime_type = "image/jpeg";
                MemoryStream ms = new MemoryStream();
                byte[] imageBytes = ms.ToArray();
                return base.File(imageBytes, mime_type);
                //return null;
            }
        }
        */

        [JsonpFilter]
        [AllowCrossSiteJson]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetScreen(string UserName, string Domain, string height, string width, string guid)
        {
            try
            {

                ScreenCapture sc = null;
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime start_time = DateTime.Now;
                Boolean CaptureCheck = false;
                while (CaptureCheck == false)
                {
                    sc = utils.GetScreenCapture(UserName, Domain);
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
                        GUID = sc.GUID,
                        CreatedOn = sc.CreatedOn.ToLocalTime().ToString("G"),
                        Url = sc.Url,
                        Data = "data:image/jpg;base64," + utils.ImageToJpegBase64(utils.PrepareScreenToReturn(sc, height, width, false), 90L)
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
                        Data = "data:image/jpg;base64," + utils.ImageToJpegBase64(utils.PrepareScreenToReturn(sc, height, width, false), 75L)
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
                Converser converser = utils.GetLoggedConverser(HttpContext.Session);
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

                Thread_GetWebLocationsHelper oThread_GetWebLocationsHelper = new Thread_GetWebLocationsHelper(converser);
                oThread_GetWebLocationsHelper.DoThings();
                //Thread.Sleep(new TimeSpan(0, 0, 15));

                List<WebLocation> weblocations = new List<WebLocation>();
                List<WebLocationDataTables> DefLocList = new List<WebLocationDataTables>();
                if (converser.Business.Domain.ToLowerInvariant() == "vizzop")
                {
                    weblocations = (from m in db.WebLocations
                                    select m).OrderByDescending(z => z.TimeStamp_Last).ToList<WebLocation>();
                }
                else
                {
                    weblocations = (from m in db.WebLocations
                                    where m.Converser.Business.ID == converser.Business.ID &&
                                    m.Converser.Agent == null
                                    select m).OrderByDescending(z => z.TimeStamp_Last).ToList<WebLocation>();
                }

                foreach (WebLocation wl in weblocations)
                {
                    try
                    {
                        if (wl.Converser == null)
                        {
                            continue;
                        }
                        //Solo añadimos los no duplicados...
                        if (DefLocList.Any(m => m.UserName == wl.Converser.UserName) == true)
                        { }
                        else
                        {
                            if (wl.Converser != null)
                            {
                                WebLocationDataTables loc = new WebLocationDataTables();
                                loc.ID = wl.ID;
                                loc.Url = wl.Url;
                                loc.Referrer = wl.Referrer;
                                loc.IP = wl.IP;
                                loc.Lang = wl.Lang;
                                loc.Ubication = wl.Ubication;
                                loc.UserAgent = wl.UserAgent;
                                loc.TimeStamp = wl.TimeStamp_Last;
                                loc.LastViewed = utils.GetPrettyDate(wl.TimeStamp_Last).ToString();
                                loc.UserName = wl.Converser.UserName;
                                loc.Domain = wl.Converser.Business.Domain;
                                loc.Password = wl.Converser.Password;
                                loc.FullName = wl.Converser.FullName != null ? wl.Converser.FullName : "Anonymous";
                                DefLocList.Add(loc);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        utils.GrabaLogExcepcion(e);
                    }
                }

                /*
                List<WebLocation> weblocations = new List<WebLocation>();
                List<WebLocationDataTables> DefLocList = new List<WebLocationDataTables>();
                List<WebLocationDataTables> Old_DefLocList = (List<WebLocationDataTables>)HttpContext.Session["DefLocList"];
                Boolean DifferentLists = false;
                DateTime start_time = DateTime.Now;
                while ((DifferentLists == false) && (DateTime.Now < start_time.AddSeconds(15)))
                {
                    Thread_GetWebLocationsHelper oThread_GetWebLocationsHelper = new Thread_GetWebLocationsHelper(converser);
                    oThread_GetWebLocationsHelper.DoThings();

                    if (converser.Business.Domain.ToLowerInvariant() == "vizzop")
                    {
                        weblocations = (from m in db.WebLocations
                                        select m).OrderByDescending(z => z.TimeStamp_Last).ToList<WebLocation>();
                    }
                    else
                    {
                        weblocations = (from m in db.WebLocations
                                        where m.Converser.Business.ID == converser.Business.ID &&
                                        m.Converser.Agent == null
                                        select m).OrderByDescending(z => z.TimeStamp_Last).ToList<WebLocation>();
                    }

                    foreach (WebLocation wl in weblocations)
                    {
                        try
                        {
                            //Solo añadimos los no duplicados...
                            if (DefLocList.Any(m => m.UserName == wl.Converser.UserName) == true)
                            { }
                            else
                            {
                                WebLocationDataTables loc = new WebLocationDataTables();
                                loc.ID = wl.ID;
                                loc.Url = wl.Url;
                                loc.Referrer = wl.Referrer;
                                loc.IP = wl.IP;
                                loc.Lang = wl.Lang;
                                loc.Ubication = wl.Ubication;
                                loc.UserAgent = wl.UserAgent;
                                loc.TimeStamp = wl.TimeStamp_Last;
                                loc.LastViewed = utils.GetPrettyDate(wl.TimeStamp_Last).ToString();
                                loc.UserName = wl.Converser.UserName;
                                loc.Domain = wl.Converser.Business.Domain;
                                loc.FullName = wl.Converser.FullName != null ? wl.Converser.FullName : "Anonymous";
                                DefLocList.Add(loc);
                            }
                        }
                        catch (Exception e)
                        {
                            utils.GrabaLogExcepcion(e);
                        }
                    }

                    if (Old_DefLocList == null)
                    {
                        Old_DefLocList = new List<WebLocationDataTables>();
                    }
                    
                      //Muy importante... no comparo el timestamp y lastviewed!!!
                      //Dado que ya me quito de enmedio a los que hace 30 segundos 
                      //que no dan señales de vida en segundo plano en "Do Things"
                     
                    var firstNotSecond = Old_DefLocList.Except(DefLocList).ToList();
                    var secondNotFirst = DefLocList.Except(Old_DefLocList).ToList();
                    if ((firstNotSecond.Count > 0) || (secondNotFirst.Count > 0))
                    {
                        DifferentLists = true;
                    }
                }

                HttpContext.Session["DefLocList"] = DefLocList;
                */

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
                        x.TimeStamp.ToString(),
                        x.LastViewed,
                        x.FullName,
                        x.UserName,
                        x.Domain,
                        x.Password,
                        ""
                        }).ToList();

                    foreach (var item in aaData)
                    {
                        ScreenCapture sc = utils.GetScreenCapture(item[12], item[13]);
                        if (sc != null)
                        {
                            item[1] = "data:image/jpg;base64," + utils.ImageToJpegBase64(
                                utils.PrepareScreenToReturn(sc, "90", "90", false), 90L
                                );
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
