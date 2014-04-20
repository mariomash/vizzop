using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace vizzopWeb
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            /*
            Utils utils = new Utils();
            utils.GrabaLog(Utils.NivelLog.info, "Application_Start");
             */
#if DEBUG
#else
#endif
        }


        protected void Application_PostMapRequestHandler(object sender, EventArgs e)
        {

            if (Request.Cookies["ASP.NET_SessionIdTemp"] != null)
            {
                if (Request.Cookies["ASP.NET_SessionId"] == null)
                    Request.Cookies.Add(new HttpCookie("ASP.NET_SessionId", Request.Cookies["ASP.NET_SessionIdTemp"].Value));
                else
                    Request.Cookies["ASP.NET_SessionId"].Value = Request.Cookies["ASP.NET_SessionIdTemp"].Value;
            }

        }

        protected void Application_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            Utils utils = new Utils();
            try
            {
                if (Context.Session != null)
                {
                    var cookie = new HttpCookie("ASP.NET_SessionIdTemp", Session.SessionID);
                    cookie.Expires = DateTime.Now.AddMonths(1);
                    Response.Cookies.Add(cookie);
                }
            }
            catch (Exception)
            {
            }
        }

        /*
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

            HttpApplication app = (HttpApplication)sender;

            string acceptEncoding = app.Request.Headers["Accept-Encoding"];
            Stream prevUncompressedStream = app.Response.Filter;

            if (acceptEncoding == null || acceptEncoding.Length == 0)
            {
                return;
            }

            acceptEncoding = acceptEncoding.ToLower();

            if (app.Context.Request.FilePath.EndsWith(".ashx") == true)
            {
                if (acceptEncoding.Contains("gzip"))
                { // gzip
                    app.Response.Filter = new GZipStream(prevUncompressedStream, CompressionMode.Compress);
                    app.Response.AppendHeader("Content-Encoding", "gzip");
                }
                else if (acceptEncoding.Contains("deflate"))
                { // defalte
                    app.Response.Filter = new DeflateStream(prevUncompressedStream, CompressionMode.Compress);
                    app.Response.AppendHeader("Content-Encoding", "deflate");
                }
            }
        }
        */

    }
}