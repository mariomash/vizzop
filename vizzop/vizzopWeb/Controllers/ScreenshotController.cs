using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;

namespace vizzopWeb
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class ScreenshotController : Controller
    {
        // GET: /Screenshot/

#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult Index(string videolength_filter)
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

                List<SelectListItem> items = new List<SelectListItem>();

                items.Add(new SelectListItem
                {
                    Text = "All Videos",
                    Value = "0"
                });
                items.Add(new SelectListItem
                {
                    Text = "More than 10 seconds",
                    Value = "10"
                });
                items.Add(new SelectListItem
                {
                    Text = "More than 30 seconds",
                    Value = "30"
                });
                items.Add(new SelectListItem
                {
                    Text = "More than 1 minute",
                    Value = "60"
                });
                if (videolength_filter == null)
                {
                    videolength_filter = "10";
                }
                foreach (SelectListItem item in items)
                {
                    if (item.Value == videolength_filter)
                    {
                        item.Selected = true;
                        break;
                    }
                }

                ViewBag.videolenghtfilter_items = items;
                return View();
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return View();
            }

        }

        private class ScreenMovieDTO
        {
            public int ID { get; set; }
            public Converser Converser { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public DateTime FirstScreenCapture { get; set; }
            public DateTime LastScreenCapture { get; set; }
        }


        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetScreenshotJsonByConverser(string username, string domain)
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

                var ScreenList = (from w in db.ScreenCaptures.Include("converser")
                                  where (w.converser.UserName == username) && (w.converser.Business.Domain == domain)
                                  orderby (w.CreatedOn)
                                  select w).ToList<ScreenCapture>();
                if (ScreenList.Count() > 0)
                {
                    var obj = ScreenList.Select((x, index) => new
                    {
                        GUID = x.GUID,
                        CreatedOn = x.CreatedOn.ToLocalTime().ToString("G"),
                        Url = x.Url
                    });
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(null);
            }
        }

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetScreenshotJson(string dimension1, string dimension2, string from, string to, string secondvideolimit)
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

                /*
                var movies = from p in List
                             group p by p.converser.ID into g
                             select new ScreenMovieDTO
                             {
                                 ID = g.FirstOrDefault().ID,
                                 Converser = g.FirstOrDefault().converser,
                                 Width = g.FirstOrDefault().LastFrameWidth,
                                 Height = g.FirstOrDefault().LastFrameHeight,
                                 FirstScreenCapture = (DateTime)g.Min(x => x.CreatedOn),
                                 LastScreenCapture = (DateTime)g.Max(x => x.CreatedOn)
                             };
                */

                List<ScreenMovie> MovieList = GetListOfMovies(from, to, secondvideolimit, converser);

                if (MovieList.Count() > 0)
                {
                    return Json(new
                    {
                        aaData = MovieList.Select((x, index) => new Object[] {
                                    x.ID,
                                    x.converser.ID,
                                    x.LastFrameUrl,
                                    x.converser.LangISO,
                                    utils.GetUbicationFromIP(x.converser.IP),
                                    x.LastFrameWidth,
                                    x.LastFrameHeight,
                                    x.ModifiedOn.ToString("o"),
                                    utils.GetPrettyDate(x.ModifiedOn),
                                    (x.LastFrameTimeStamp - x.FirstFrameTimeStamp).TotalMilliseconds,
                                    utils.GetPrettyTimespan(Convert.ToInt32((x.LastFrameTimeStamp - x.FirstFrameTimeStamp).TotalMilliseconds)),
                                    @"https://vizzop.blob.core.windows.net/videos/" + x.converser.ID + @".mp4"
                                })
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                //return Json(ScreenList);
            }
            catch (Exception)
            {
                //utils.GrabaLogExcepcion(ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ScreenMovie> GetListOfMovies(string from, string to, string secondVideoLimit, Converser converser)
        {
            vizzopContext db = new vizzopContext();
            Utils utils = new Utils(db);

            List<ScreenMovie> List = new List<ScreenMovie>();
            try
            {

                int secondVideoLimit_int = Convert.ToInt16(secondVideoLimit);
                DateTime dtFrom = DateTime.Parse(from);
                DateTime dtTo = DateTime.Parse(to);
                dtTo = dtTo.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

                var tempList = (from w in db.ScreenMovies.Include("converser")
                                where w.CreatedOn >= dtFrom &&
                                w.CreatedOn <= dtTo &&
                                EntityFunctions.DiffSeconds(w.LastFrameTimeStamp, w.FirstFrameTimeStamp) > secondVideoLimit_int
                                select w);

                //&& (w.Headers.Contains("'DNT':'1'") == false)

                if (converser.Business.Domain.ToLowerInvariant() != "vizzop")
                {
                    tempList = (from w in tempList
                                where w.converser.Business.ID == converser.Business.ID
                                select w);
                }

                /*
                var movies = from p in List
                             group p by p.converser.ID into g
                             select new ScreenMovieDTO
                             {
                                 ID = g.FirstOrDefault().ID,
                                 Converser = g.FirstOrDefault().converser,
                                 Width = g.FirstOrDefault().LastFrameWidth,
                                 Height = g.FirstOrDefault().LastFrameHeight,
                                 FirstScreenCapture = (DateTime)g.Min(x => x.CreatedOn),
                                 LastScreenCapture = (DateTime)g.Max(x => x.CreatedOn)
                             };
                */

                if (tempList.Count() > 0)
                {
                    List = tempList.ToList();
                }

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }

            return List;

        }
    }
}
