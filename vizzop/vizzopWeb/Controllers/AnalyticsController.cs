using System;
using System.Collections.Generic;
//using Microsoft.Web.Mvc.Resources;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;

namespace vizzopWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class AnalyticsController : Controller
    {
        //
        // GET: /Analytics/

        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();


        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetAnalyticsGraphJson(string dimension1, string dimension2, string from, string to)
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

                //siempre dimension1 = "day"
                //Así que si dimension1 ya era day pasamos... 
                //dimension2 = dimension1;
                dimension2 = "day";
                dimension1 = "day";
                List<WebLocation_History> anal_list = GetAnalyticsData(from, to, converser);
                IEnumerable<AnalyticsGroup> groups = GroupAnalyticsData(dimension1, anal_list);

                List<Object> data = new List<Object>();
                List<Object> labels = new List<Object>();
                switch (dimension2.ToLowerInvariant())
                {
                    case "day":
                        labels.Add("Page Views");
                        labels.Add("Different IPs");
                        //labels.Add("Average Seconds");
                        List<Object> VisitsgroupToAdd = new List<Object>();
                        List<Object> IpsgroupToAdd = new List<Object>();
                        //List<Object> AveragegroupToAdd = new List<Object>();
                        foreach (var daygroup in groups.Reverse())
                        {
                            //Importante... es unixtime * 100
                            Int64 key = Convert.ToInt64(daygroup.key0) * 1000;

                            var VisitpointsToAdd = new Object[] {
                                    key, 
                                    daygroup.rows.Count()
                                };
                            VisitsgroupToAdd.Add(VisitpointsToAdd);

                            var IPpointsToAdd = new Object[] {
                                    key, 
                                    (from elem in daygroup.rows
                                     group elem by elem.IP 
                                     into ggg select ggg).Count()
                                };
                            IpsgroupToAdd.Add(IPpointsToAdd);
                            /*
                            var AveragepointsToAdd = new Object[] {
                                    key, 
                                    Convert.ToInt32(Math.Ceiling((from elem in daygroup.rows
                                                                  select Convert.ToInt32((elem.TimeStamp_Last - elem.TimeStamp_First).TotalSeconds)).Average()))
                                };
                            AveragegroupToAdd.Add(AveragepointsToAdd);
                             */
                        }
                        data.Add(VisitsgroupToAdd);
                        data.Add(IpsgroupToAdd);
                        //data.Add(AveragegroupToAdd);
                        break;
                    case "url":
                        foreach (var daygroup in groups)
                        {
                            //Importante... es unixtime * 100
                            Int64 key = Convert.ToInt64(daygroup.key0) * 1000;

                            var urlgroup = daygroup.rows.GroupBy(y => y.Url);
                            foreach (var urlelem in urlgroup)
                            {
                                List<Object> urlgroupToAdd = new List<Object>();
                                labels.Add(urlelem.Key);
                                foreach (var weblocation_historyelem in urlelem)
                                {
                                    var urlpointsToAdd = new Object[] {
                                       key, (from elem in urlelem group elem by elem.IP into ggg select ggg).Count()
                                    };
                                    urlgroupToAdd.Add(urlpointsToAdd);
                                }
                                data.Add(urlgroupToAdd);
                            }
                        }
                        break;
                    case "browser":
                        break;
                    case "referrer":
                        break;
                    case "language":
                        break;
                    case "ubication":
                        break;
                    case "visitor":
                        break;
                    case "visit duration":
                        break;
                    default:
                        break;
                }

                return Json(new
                {
                    data = data,
                    labels = labels

                },
                JsonRequestBehavior.AllowGet);

                /*
                return Content("{'data':[[[1377648000000,32],[1377907200000,14],[1378080000000,21],[1378252800000,6],[1377820800000,8],[1377734400000,4],[1378166400000,24],[1377993600000,5]],[[1377648000000,8],[1377907200000,4],[1378080000000,5],[1378252800000,2],[1377820800000,5],[1377734400000,2],[1378166400000,6],[1377993600000,2]]],'labels':['Page Views','Unique Visitors']}");
                 */

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(null);
            }
        }

        int[] DurationRanges = { 1001, 10000, 60000, 300000, 600000, 1200000, 2400000, 4800000 };
        private class AnalyticsGroup
        {
            public object key0 { get; set; }
            public object key1 { get; set; }
            public IGrouping<string, WebLocation_History> rows { get; set; }
            public int count { get; set; }
        }
        private class AnalyticsReturn
        {
            public object index0 { get; set; }
            public object index1 { get; set; }
            public object d0 { get; set; }
            public object d1 { get; set; }
            public int pageviews { get; set; }
            public int differentIPs { get; set; }
            public int milisecondsAverage { get; set; }
        }
        private Object EstandarizaIndex(AnalyticsGroup group, string dimension, int indice)
        {
            try
            {
                Object key = null;
                switch (indice)
                {
                    case 0:
                        key = group.key0;
                        break;
                    case 1:
                        key = group.key1;
                        break;
                    default:
                        return null;
                }

                if (dimension.ToLowerInvariant() == "day")
                {
                    return Convert.ToDateTime(key).Date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                }
                else
                {
                    return key;
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return null;
            }
        }
        private Object EstandarizaKey(AnalyticsGroup group, string dimension, int indice)
        {
            try
            {
                Object key = null;
                switch (indice)
                {
                    case 0:
                        key = group.key0;
                        break;
                    case 1:
                        key = group.key1;
                        break;
                    default:
                        return null;
                }

                if (dimension.ToLowerInvariant() == "day")
                {
                    //return utils.GetPrettyDate(Convert.ToDateTime(key));
                    return utils.GetTimeStampfromUnix(key.ToString()).ToString("d", DateTimeFormatInfo.InvariantInfo);
                }
                else if (dimension.ToLowerInvariant() == "visit duration")
                {
                    return utils.GetPrettyDurationRange(Convert.ToInt32(key), DurationRanges);
                }
                else if (dimension.ToLowerInvariant() == "visitor")
                {
                    int id = Convert.ToInt32(key);
                    var last = group.rows.LastOrDefault();
                    return key.ToString() + " / " + last.Lang + " / " + last.Ubication + " / " + last.IP;
                }
                else
                {
                    return key;
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return null;
            }
        }
        private List<WebLocation_History> GetAnalyticsData(string from, string to, Converser converser)
        {
            try
            {
                DateTime dtFrom = DateTime.Parse(from);
                DateTime dtTo = DateTime.Parse(to);
                dtTo = dtTo.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

                var anal_list = (from w in db.WebLocations_History.Include("converser")
                                 where ((w.TimeStamp_First <= dtFrom && w.TimeStamp_Last >= dtFrom) || (w.TimeStamp_First <= dtTo && w.TimeStamp_Last >= dtTo) ||
                                 (w.TimeStamp_First <= dtFrom && w.TimeStamp_Last >= dtTo) || (w.TimeStamp_First >= dtFrom && w.TimeStamp_Last <= dtTo))
                                 && (w.Headers.Contains("'DNT':'1'") == false)
                                 select w);
                if (converser.Business.Domain.ToLowerInvariant() != "vizzop")
                {
                    anal_list = (from w in anal_list
                                 where w.converser.Business.ID == converser.Business.ID
                                 select w);
                }
                return anal_list.ToList<WebLocation_History>();

            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return null;
            }
        }
        private IList<IEnumerable<AnalyticsGroup>> ReGroupAnalyticsData(string dimension, IEnumerable<AnalyticsGroup> groups, List<WebLocation_History> original_list)
        {
            try
            {
                IList<IEnumerable<AnalyticsGroup>> groups2 = new List<IEnumerable<AnalyticsGroup>>();
                switch (dimension.ToLowerInvariant())
                {
                    case "day":
                        foreach (var g in groups)
                        {
                            var tempgroups = from elem in g.rows
                                             orderby elem.TimeStamp_First descending
                                             group elem by utils.GetUnixTimeStamp(elem.TimeStamp_First.Date) into gg
                                             select new AnalyticsGroup()
                                             {
                                                 key0 = g.key0,
                                                 key1 = gg.Key,
                                                 rows = gg,
                                                 count = gg.Count()
                                             };
                            groups2.Add(tempgroups);
                        }
                        break;
                    case "url":
                        foreach (var g in groups)
                        {
                            var tempgroups = from elem in g.rows
                                             orderby elem.Url ascending
                                             group elem by elem.Url into gg
                                             select new AnalyticsGroup()
                                             {
                                                 key0 = g.key0,
                                                 key1 = gg.Key,
                                                 rows = gg,
                                                 count = gg.Count()
                                             };
                            groups2.Add(tempgroups);
                        }
                        break;
                    case "browser":
                        foreach (var g in groups)
                        {
                            var tempgroups = from elem in g.rows
                                             orderby elem.UserAgent ascending
                                             group elem by elem.UserAgent into gg
                                             select new AnalyticsGroup()
                                             {
                                                 key0 = g.key0,
                                                 key1 = gg.Key,
                                                 rows = gg,
                                                 count = gg.Count()
                                             };
                            groups2.Add(tempgroups);
                        }
                        break;

                    case "referrer":
                        foreach (var g in groups)
                        {
                            var tempgroups = from elem in g.rows
                                             orderby elem.Referrer ascending
                                             group elem by elem.Referrer into gg
                                             select new AnalyticsGroup()
                                             {
                                                 key0 = g.key0,
                                                 key1 = gg.Key,
                                                 rows = gg,
                                                 count = gg.Count()
                                             };
                            groups2.Add(tempgroups);
                        }
                        break;
                    case "language":
                        foreach (var g in groups)
                        {
                            var tempgroups = from elem in g.rows
                                             orderby elem.Lang ascending
                                             group elem by elem.Lang into gg
                                             select new AnalyticsGroup()
                                             {
                                                 key0 = g.key0,
                                                 key1 = gg.Key,
                                                 rows = gg,
                                                 count = gg.Count()
                                             };
                            groups2.Add(tempgroups);
                        }
                        break;
                    case "ubication":
                        foreach (var g in groups)
                        {
                            var tempgroups = from elem in g.rows
                                             orderby elem.Ubication ascending
                                             group elem by elem.Ubication into gg
                                             select new AnalyticsGroup()
                                             {
                                                 key0 = g.key0,
                                                 key1 = gg.Key,
                                                 rows = gg,
                                                 count = gg.Count()
                                             };
                            groups2.Add(tempgroups);
                        }
                        break;
                    case "visitor":
                        foreach (var g in groups)
                        {
                            var tempgroups = from elem in g.rows
                                             orderby elem.converser.ID descending
                                             group elem by elem.converser.ID.ToString() into gg
                                             select new AnalyticsGroup()
                                             {
                                                 key0 = g.key0,
                                                 key1 = gg.Key,
                                                 rows = gg,
                                                 count = gg.Count()
                                             };
                            groups2.Add(tempgroups);
                        }
                        break;
                    case "visit duration":
                        foreach (var g in groups)
                        {
                            var tempgroups = from elem in g.rows
                                             orderby (elem.TimeStamp_Last - elem.TimeStamp_First).TotalMilliseconds descending
                                             group elem by DurationRanges.Where(x => Convert.ToInt32((elem.TimeStamp_Last - elem.TimeStamp_First).TotalMilliseconds) >= x).DefaultIfEmpty().Last().ToString() into gg
                                             select new AnalyticsGroup()
                                             {
                                                 key0 = g.key0,
                                                 key1 = gg.Key,
                                                 rows = gg,
                                                 count = gg.Count()
                                             };
                            groups2.Add(tempgroups);
                        }
                        break;
                    default:
                        return null;
                }
                return groups2;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return null;
            }
        }
        private IEnumerable<AnalyticsGroup> GroupAnalyticsData(string dimension, List<WebLocation_History> original_list)
        {
            try
            {
                IEnumerable<AnalyticsGroup> groups = null;
                switch (dimension.ToLowerInvariant())
                {
                    case "day":
                        groups = from elem in original_list
                                 orderby elem.TimeStamp_First descending
                                 group elem by utils.GetUnixTimeStamp(elem.TimeStamp_First.Date) into g
                                 select new AnalyticsGroup()
                                 {
                                     key0 = g.Key,
                                     key1 = null,
                                     rows = g,
                                     count = g.Count()
                                 };
                        break;
                    case "url":
                        groups = from elem in original_list
                                 orderby elem.Url ascending
                                 group elem by elem.Url into g
                                 select new AnalyticsGroup()
                                 {
                                     key0 = g.Key,
                                     key1 = null,
                                     rows = g,
                                     count = g.Count()
                                 };
                        break;
                    case "browser":
                        groups = from elem in original_list
                                 orderby elem.UserAgent ascending
                                 group elem by elem.UserAgent into g
                                 select new AnalyticsGroup()
                                 {
                                     key0 = g.Key,
                                     key1 = null,
                                     rows = g,
                                     count = g.Count()
                                 };
                        break;
                    case "referrer":
                        groups = from elem in original_list
                                 orderby elem.Referrer ascending
                                 group elem by elem.Referrer into g
                                 select new AnalyticsGroup()
                                 {
                                     key0 = g.Key,
                                     key1 = null,
                                     rows = g,
                                     count = g.Count()
                                 };
                        break;
                    case "language":
                        groups = from elem in original_list
                                 orderby elem.Lang ascending
                                 group elem by elem.Lang into g
                                 select new AnalyticsGroup()
                                 {
                                     key0 = g.Key,
                                     key1 = null,
                                     rows = g,
                                     count = g.Count()
                                 };
                        break;
                    case "ubication":
                        groups = from elem in original_list
                                 orderby elem.Ubication ascending
                                 group elem by elem.Ubication into g
                                 select new AnalyticsGroup()
                                 {
                                     key0 = g.Key,
                                     key1 = null,
                                     rows = g,
                                     count = g.Count()
                                 };
                        break;
                    case "visitor":
                        groups = from elem in original_list
                                 orderby elem.converser.ID descending
                                 group elem by elem.converser.ID.ToString() into g
                                 select new AnalyticsGroup()
                                 {
                                     key0 = g.Key,
                                     key1 = null,
                                     rows = g,
                                     count = g.Count()
                                 };
                        break;
                    case "visit duration":
                        groups = from elem in original_list
                                 orderby (elem.TimeStamp_First - elem.TimeStamp_Last).Milliseconds descending
                                 group elem by DurationRanges.Where(x => Convert.ToInt32((elem.TimeStamp_Last - elem.TimeStamp_First).TotalMilliseconds) >= x).DefaultIfEmpty().Last().ToString() into g
                                 select new AnalyticsGroup()
                                 {
                                     key0 = g.Key,
                                     key1 = null,
                                     rows = g,
                                     count = g.Count()
                                 };
                        break;
                    default:
                        return null;
                }
                return groups;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return null;
            }
        }
        private List<AnalyticsReturn> ProcessAnalyticsData(string dimension1, string dimension2, IEnumerable<AnalyticsGroup> groups, List<WebLocation_History> original_list)
        {
            try
            {
                var ReturnList = new List<AnalyticsReturn>();

                if (groups == null)
                {
                    groups = GroupAnalyticsData(dimension1, original_list);
                    if ((dimension2 == null) || (dimension2 == ""))
                    {
                        int index0 = 0;
                        foreach (var g in groups)
                        {
                            AnalyticsReturn data = new AnalyticsReturn();
                            data.index0 = index0;
                            index0++;
                            data.index1 = EstandarizaIndex(g, dimension2, 1);
                            data.d0 = EstandarizaKey(g, dimension1, 0);
                            data.d1 = EstandarizaKey(g, dimension2, 1);
                            data.differentIPs = (from elem in g.rows
                                                 group elem by elem.IP into gg
                                                 select gg).Count();
                            data.pageviews = g.count;
                            data.milisecondsAverage = Convert.ToInt32(Math.Ceiling((from elem in g.rows
                                                                                    select Convert.ToInt32((elem.TimeStamp_Last - elem.TimeStamp_First).TotalMilliseconds)).Average()));
                            ReturnList.Add(data);
                        }
                        return ReturnList;
                    }
                    else
                    {
                        return ProcessAnalyticsData(dimension1, dimension2, groups, original_list);
                    }
                }
                else
                {
                    IList<IEnumerable<AnalyticsGroup>> groups2 = ReGroupAnalyticsData(dimension2, groups, original_list);
                    int index0 = 0;
                    foreach (var g in groups2)
                    {
                        foreach (var gg in g)
                        {
                            AnalyticsReturn data = new AnalyticsReturn();
                            data.index0 = index0;
                            index0++;
                            data.index1 = EstandarizaIndex(gg, dimension2, 1);
                            data.d0 = EstandarizaKey(gg, dimension1, 0);
                            data.d1 = EstandarizaKey(gg, dimension2, 1);
                            data.differentIPs = (from elem in gg.rows
                                                 group elem by elem.IP into ggg
                                                 select ggg).Count();
                            data.pageviews = gg.count;
                            data.milisecondsAverage = Convert.ToInt32(Math.Ceiling((from elem in gg.rows
                                                                                    select Convert.ToInt32((elem.TimeStamp_Last - elem.TimeStamp_First).TotalMilliseconds)).Average()));
                            ReturnList.Add(data);
                        }
                    }
                    return ReturnList;
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return null;
            }
        }

        [JsonpFilter]
#if DEBUG
#else
        [RequireHttps]
#endif
        public ActionResult GetAnalyticsJson(string dimension1, string dimension2, string from, string to)
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

                List<WebLocation_History> anal_list = GetAnalyticsData(from, to, converser);

                List<AnalyticsReturn> final_list = ProcessAnalyticsData(dimension1, dimension2, null, anal_list);

                return Json(new
                {
                    aaData = final_list.Select((x, index) => new Object[] {
                        x.index0,
                        x.d0,
                        x.index1,
                        x.d1,
                        x.pageviews,
                        x.differentIPs,
                        x.milisecondsAverage,
                        utils.GetPrettyTimespan(x.milisecondsAverage)
                    })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return Json(null);
            }
        }

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

    }
}
