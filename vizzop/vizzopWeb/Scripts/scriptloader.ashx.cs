using System;
using System.Web;
using System.Web.SessionState;
using Yahoo.Yui.Compressor;

namespace vizzopWeb.Scripts
{
    /// <summary>
    /// Descripción breve de scriptloader
    /// </summary>
    public class scriptloader : IHttpHandler, IReadOnlySessionState
    {

        public void ProcessRequest(HttpContext context)
        {

            TimeSpan expire = new TimeSpan(3, 0, 0);
            DateTime now = DateTime.Now;
            context.Response.Cache.SetExpires(now.Add(expire));
            context.Response.Cache.SetMaxAge(expire);
            context.Response.Cache.SetCacheability(HttpCacheability.Server);
            context.Response.Cache.SetValidUntilExpires(true);

            string sContent = null;
            try
            {
                System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                string APP_PATH = System.Web.HttpContext.Current.Request.ApplicationPath.ToLower();
                if (APP_PATH == "/")
                {
                    APP_PATH = "/";
                }
                else if (!APP_PATH.EndsWith("/"))
                {
                    APP_PATH += "/";
                }
                string ROOT_PATH = System.Web.HttpContext.Current.Server.MapPath(APP_PATH);
                if (!ROOT_PATH.EndsWith("\\"))
                {
                    ROOT_PATH += "\\";
                }
                Utils utils = new Utils();

                //JS LOCALES
                string sJsLocals = null;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/prettify.js") + System.Environment.NewLine;
                //sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop-1.7.1.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop-ui-1.8.17.js") + System.Environment.NewLine;
                //sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap/js/bootstrap.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap-tooltip.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap-popover.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap-carousel.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap-tab.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/DataTables-1.9.0/media/js/jVizzop.dataTables.js") + System.Environment.NewLine;
                //Ojo los JQplot asi como los datatables estan tocados!!!
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/jVizzop.jqplot.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.highlighter.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.bubbleRenderer.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.barRenderer.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.categoryAxisRenderer.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.dateAxisRenderer.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.canvasTextRenderer.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.canvasAxisLabelRenderer.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.canvasAxisTickRenderer.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jqplot/plugins/jqplot.pointLabels.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/DatePicker/js/datepicker.js") + System.Environment.NewLine;

#if DEBUG
#else
                JavaScriptCompressor jsc = new JavaScriptCompressor();
                jsc.ObfuscateJavascript = true;
                sJsLocals = jsc.Compress(sJsLocals);
#endif
                sContent += sJsLocals;

            }
            catch (Exception ex)
            {
                Utils utils = new Utils();
                utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                sContent = ex.Message;
            }
            context.Response.ContentType = "text/javascript";
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            context.Response.Write(sContent);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}