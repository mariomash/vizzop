using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.Web.Mvc.Resources;
//using Microsoft.Web.Mvc.Resources;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using Microsoft.WindowsAzure.ServiceRuntime;
using vizzopWeb.Models;
using Yahoo.Yui.Compressor;

namespace vizzopWeb
{
    /// <summary>
    /// Descripción breve de jsapi
    /// </summary>
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class jsapi : IHttpHandler, IReadOnlySessionState
    {
        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

        public void ProcessRequest(HttpContext context)
        {
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

                //string url_host_port = context.Request.Url.Scheme + @"://" + context.Request.Url.Host + ":" + context.Request.Url.Port.ToString();

                string scheme = context.Request.Url.Scheme;
                string wsscheme = context.Request.Url.Scheme;
                string domain = context.Request.Url.Host;
                int port = context.Request.Url.Port;

                try
                {
                    domain = RoleEnvironment.GetConfigurationSettingValue("Domain");
                    port = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("Port"));
                    scheme = RoleEnvironment.GetConfigurationSettingValue("Scheme");
                    wsscheme = RoleEnvironment.GetConfigurationSettingValue("WsScheme");
                }
                catch (Exception ex) { utils.GrabaLog(Utils.NivelLog.info, ex.Message); }
                string url_host_port = scheme + @"://" + domain + ":" + port;
                string ws_url_host_port = wsscheme + @"://" + domain + ":" + port;

                //comprobando apikeys...
                string ApiKey, Mode = null;
                try
                {
                    ApiKey = context.Request.Params["apikey"].ToLowerInvariant();
                    Mode = context.Request.Params["mode"].ToLowerInvariant();
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                    return;
                }

                var business = (from m in db.Businesses
                                where m.ApiKey == ApiKey
                                select m).FirstOrDefault();
                if (business == null)
                {
                    return;
                }

                string Me = "null";
                string webname = "vizzop";
                if (business.BusinessName != null) { webname = business.BusinessName; }
                Converser converser = null;
                string trackID = null;

                if (Mode == "agent")
                {
                    if ((context.Request.Params["username"].ToLowerInvariant() != null) && (context.Request.Params["password"].ToLowerInvariant() != null) && (context.Request.Params["domain"].ToLowerInvariant() != null))
                    {
                        var _username = context.Request.Params["username"].ToLowerInvariant();
                        var _password = context.Request.Params["password"].ToLowerInvariant();
                        var _domain = context.Request.Params["domain"].ToLowerInvariant();
                        converser = utils.GetConverserFromSystem(_username, _password, _domain);
                        if (converser == null)
                        {
                            return;
                        }
                        else
                        {
                            Converser conv_me = new Converser();
                            conv_me.UserName = converser.UserName;
                            conv_me.Password = converser.Password;
                            conv_me.ID = converser.ID;
                            conv_me.Email = converser.Email;
                            conv_me.FullName = converser.FullName;
                            conv_me.Business = new Business();
                            conv_me.Business.Domain = converser.Business.Domain;
                            Me = oSerializer.Serialize(conv_me);
                            //Utils.AddZenSession(converser.UserName, context.Session.SessionID);
                            //MsgCheck_Block = "true";
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else if (Mode == "client")
                {
                    string name_mecookie = ApiKey + "_me";
                    if (context.Request.Cookies[name_mecookie] != null)
                    {
                        HttpCookie cookie = context.Request.Cookies[name_mecookie];
                        string JSONvalue = HttpUtility.UrlDecode(cookie.Value);

                        JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                        var JSONdata = (IDictionary<string, object>)json_serializer.DeserializeObject(JSONvalue);
                        if (JSONdata != null)
                        {
                            if (JSONdata["ID"] != null)
                            {
                                Int32 ID = Convert.ToInt32(JSONdata["ID"]);
                                converser = (from m in db.Conversers
                                             where m.ID == ID
                                             select m).FirstOrDefault();
                            }
                        }
                    }
                    if (converser == null)
                    {
                        converser = new Converser();
                        converser = utils.CreateConverserinDB(converser);

                        if (converser.ID != 0)
                        {
                            converser = (from m in db.Conversers
                                             .Include("Business")
                                         where m.ID == converser.ID
                                         select m).FirstOrDefault();
                            if (converser != null)
                            {

                                converser.Business = business;
                                converser.LangISO = utils.GetLang(context);
                                converser.IP = utils.GetIP(context);
                                converser.UserAgent = context.Request.UserAgent;

                                db.SaveChanges();

                            }
                        }
                    }
                    if (converser != null)
                    {
                        string[] languages = context.Request.UserLanguages;
                        string language = null;
                        if (languages != null && languages.Length != 0) { language = languages[0].ToLowerInvariant().Trim(); }

                        // If you want it formated in some other way.
                        var headers = "{";
                        foreach (var key in context.Request.Headers.AllKeys)
                            headers += "'" + key + "':'" + context.Request.Headers[key] + "',";

                        headers = headers.TrimEnd(',') + "}";

                        string useragent = context.Request.UserAgent;

                        string sIP = context.Request.ServerVariables["HTTP_CLIENT_IP"];
                        if (string.IsNullOrEmpty(sIP) == true) { sIP = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]; }
                        if (string.IsNullOrEmpty(sIP) == true) { sIP = context.Request.ServerVariables["REMOTE_ADDR"]; }
                        if (string.IsNullOrEmpty(sIP) == true)
                        {
                            utils.GrabaLog(Utils.NivelLog.error, "No IP to Track");
                        }
                        else
                        {
                            sIP = sIP.Split(',')[0];
                            string urlreferrer = null;
                            if (context.Request.UrlReferrer != null)
                            {
                                urlreferrer = context.Request.UrlReferrer.AbsoluteUri;
                            }
                            //Y trackeamos la visita.. solo para clientes!! si es la primera vez no habrá trackID... anyway siempre lo traemos
                            Status returnStatus = utils.TrackPageView(trackID, converser, urlreferrer, "", language, useragent, sIP, headers, db);
                            var e = returnStatus;
                        }

                        Converser conv_me = new Converser();
                        conv_me.UserName = converser.UserName;
                        conv_me.Password = converser.Password;
                        conv_me.ID = converser.ID;
                        conv_me.Email = converser.Email;
                        conv_me.FullName = converser.FullName;
                        conv_me.Business = new Business();
                        conv_me.Business.Domain = converser.Business.Domain;
                        Me = oSerializer.Serialize(conv_me);
                    }
                }

                string serializedMeetingSession = "null";
                if (Mode == "meeting")
                {
                    if (context.Request.Params["meetingsessionid"].ToLowerInvariant() != null)
                    {
                        var _meetingsessionid = context.Request.Params["meetingsessionid"].ToLowerInvariant();

                        MeetingSession meetingSession = utils.GetMeetingSessionFromSystemSerializedProof(_meetingsessionid);

                        if (meetingSession == null)
                        {
                            return;
                        }
                        else
                        {
                            serializedMeetingSession = oSerializer.Serialize(meetingSession);
                        }
                    }
                    else
                    {
                        return;
                    }
                }


                //JS EXTERNOS
                string sJs = null;
                //sJs += "vizzoplib.LoadJS(\"https://static.opentok.com/v1.1/js/TB.min.js\");\n";
                //sJs += "vizzoplib.LoadJS(\"https://ajax.googleapis.com/ajax/libs/jVizzopui/1.8.16/jVizzop-ui.min.js\");\n";
                sContent += sJs;

                //JS LOCALES
                string sJsLocals = null;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop-1.8.3.js");
                sJsLocals += @"
                var jVizzop = jVizzop.noConflict();
                ";
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop-ui-1.9.2.js") + System.Environment.NewLine;
                sJsLocals += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop.class.js") + System.Environment.NewLine;

                string zenapi = "";
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap/js/bootstrap.js") + System.Environment.NewLine;
                //zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/tokbox/TB.min.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap-tooltip.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap-popover.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap-tab.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/swfobject/swfobject.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop.cookie.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop.json-2.3.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop.overlaps.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/jVizzop.h5validate.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/json2.js") + System.Environment.NewLine;
                //zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/html2canvas.js") + System.Environment.NewLine;
                //zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/bidelmanshot.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/diff_match_patch.js") + System.Environment.NewLine;
                //zenapi += utils.LeeArchivo(ROOT_PATH + "/Scripts/bootstrap-wysiwyg.js") + System.Environment.NewLine;
                /* Lo de arriba era para completar el obfuscate... */
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/jsapi.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.daemon.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.converser.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.message.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.box.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.messagebox.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.controlbox.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.clientmessagebox.js") + System.Environment.NewLine;
                zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.disclaimerbox.js") + System.Environment.NewLine;


                if (Mode == "agent")
                {
                    zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.agentmessagebox.js") + System.Environment.NewLine;
                    zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.ticketmessagebox.js") + System.Environment.NewLine;
                    zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.faqbox.js") + System.Environment.NewLine;
                    zenapi += utils.LeeArchivo(ROOT_PATH + "/vizzop/zen.localizebox.js") + System.Environment.NewLine;
                }

#if DEBUG
#else
                JavaScriptCompressor jsc = new JavaScriptCompressor();
                jsc.ObfuscateJavascript = true;
                zenapi = jsc.Compress(zenapi);
#endif

                sJsLocals += zenapi;
                sContent += sJsLocals;

                //Por Defecto siempre inglés
                HttpContextBase abstractContext = new System.Web.HttpContextWrapper(HttpContext.Current);
                string lang = utils.GetLang(abstractContext);

                string langiso = lang.Split('-')[0].ToLowerInvariant();
                if ((langiso != "en") && (langiso != "es"))
                {
                    langiso = "en";
                }

                var langStrings = (from m in db.TextStrings
                                   where m.IsoCode == langiso
                                   select m).ToList<TextString>();


                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = DateTime.Now.AddSeconds(-45);
                DateTime loctimeUTC = localZone.ToUniversalTime(loctime);
                var agents = (from m in db.Conversers
                              where m.Business.ID == converser.Business.ID
                              && m.Agent != null
                              && m.LastActive > loctimeUTC
                              && m.Active == true
                              select m).ToList<Converser>();
                string allowchat = "false";
                if (agents.Count > 0)
                {
                    allowchat = "true";
                }
                else
                {
                    allowchat = "false";
                }


                string UserAgent = context.Request.UserAgent;
                if (UserAgent.Contains("MSIE 6.0") == true)
                {
                    UserAgent = "MSIE 6.0";
                }
                else if (UserAgent.Contains("MSIE 7.0") == true)
                {
                    UserAgent = "MSIE 7.0";
                }
                else if (UserAgent.Contains("MSIE 8.0") == true)
                {
                    UserAgent = "MSIE 8.0";
                }
                else if (UserAgent.Contains("MSIE 8.0") == true)
                {
                    UserAgent = "MSIE 8.0";
                }
                else if (UserAgent.Contains("MSIE 9.0") == true)
                {
                    UserAgent = "MSIE 9.0";
                }
                else if (UserAgent.Contains("MSIE 10.0") == true)
                {
                    UserAgent = "MSIE 10.0";
                }

                var useragent_in_db = (from m in db.BrowserFeatures
                                       where m.UserAgent == UserAgent
                                       select m).FirstOrDefault();

                var allowjsapi = true;
                if (useragent_in_db != null)
                {
                    allowjsapi = useragent_in_db.AllowJsApi;
                }
                if (allowjsapi == false)
                {
                    return;
                }

                string allowchatsockets = business.AllowChatSockets.ToString().ToLowerInvariant();
                if (useragent_in_db != null)
                {
                    allowchatsockets = useragent_in_db.AllowChatSockets.ToString().ToLowerInvariant();
                }
                try
                {
                    if (context.Request.Params["allowchatsockets"] != null)
                    {
                        allowchatsockets = context.Request.Params["allowchatsockets"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                }

                string allowscreensockets = business.AllowScreenSockets.ToString().ToLowerInvariant();
                if (useragent_in_db != null)
                {
                    allowscreensockets = useragent_in_db.AllowScreenSockets.ToString().ToLowerInvariant();
                }
                try
                {
                    if (context.Request.Params["allowscreensockets"] != null)
                    {
                        allowscreensockets = context.Request.Params["allowscreensockets"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                }

                string allowscreencaptures = business.AllowScreenCaptures.ToString().ToLowerInvariant();
                if (useragent_in_db != null)
                {
                    allowscreencaptures = useragent_in_db.AllowScreenCaptures.ToString().ToLowerInvariant();
                }
                try
                {
                    if (context.Request.Params["allowscreencaptures"] != null)
                    {
                        allowscreencaptures = context.Request.Params["allowscreencaptures"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                }

                string showdisclaimer = business.ShowDisclaimer.ToString().ToLowerInvariant();
                try
                {
                    if (context.Request.Params["showdisclaimer"] != null)
                    {
                        showdisclaimer = context.Request.Params["showdisclaimer"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                }
                //Opentok Secret
                //0184561023487034817234012873410237510553
                //sessionid
                //1_MX4xMjMyMDgxfjcwLjQyLjQ3Ljc4fjIwMTItMDQtMTAgMTc6NTg6MjQuNjU1NDQ4KzAwOjAwfjAuMTIyMjUxMjA1NzI3fg
                //token
                //devtoken

                /*
    opentok_apiKey: '14117582',
    opentok_sessionId : '" + sessionId + @"',
    opentok_token: '" + token + @"',
                 */

                string allowcapturemouse = business.AllowCaptureMouse.ToString().ToLowerInvariant();
                try
                {
                    if (context.Request.Params["allowcapturemouse"] != null)
                    {
                        allowcapturemouse = context.Request.Params["allowcapturemouse"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                }

                string showhelpbutton = business.ShowHelpButton.ToString().ToLowerInvariant();
                try
                {
                    if (context.Request.Params["showhelpbutton"] != null)
                    {
                        showhelpbutton = context.Request.Params["showhelpbutton"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                }

                string auditmessages = business.AuditMessages.ToString().ToLowerInvariant();
                try
                {
                    if (context.Request.Params["auditmessages"] != null)
                    {
                        showhelpbutton = context.Request.Params["auditmessages"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                }

                string WidgetFg = "null";
                if (business.WidgetForegroundColor != null)
                {
                    WidgetFg = @"'" + business.WidgetForegroundColor + @"'";
                }
                string WidgetBg = "null";
                if (business.WidgetBackgroundColor != null)
                {
                    WidgetBg = @"'" + business.WidgetBackgroundColor + @"'";
                }
                string WidgetBorder = "null";
                if (business.WidgetBorderColor != null)
                {
                    WidgetBorder = @"'" + business.WidgetBorderColor + @"'";
                }
                string WidgetText = "null";
                if (business.WidgetText != null)
                {
                    WidgetText = @"'" + business.WidgetText + @"'";
                }
                string BusinessHours = "null";
                if (business.BusinessHours != null)
                {
                    BusinessHours = @"'" + business.BusinessHours + @"'";
                }

                string vizzop = @"
var vizzop = {
    lang: '" + langiso + @"',
    langStrings: " + oSerializer.Serialize(langStrings) + @",
    mainURL: '" + url_host_port + @"',
    wsURL: '" + ws_url_host_port + @"',
    trackHostName: null,
    trackPathName: null,
    isDebug: " + HttpContext.Current.IsDebuggingEnabled.ToString().ToLowerInvariant() + @",
    clientIP: '" + context.Request.UserHostAddress + @"',
    Boxes: [],
    Messages: [],
    ApiKey: '" + business.ApiKey + @"',
    DaemonTiming: 100,
    DaemonTiming_Steps: new Number(0),
    Daemon: null,
    store_engine: null,
    Tracking: null,
    ChatListCheck_InCourse: null,
    MsgCheck_InCourse: true,
    MsgCheckExternal_InCourse: true,
    MsgCue: [],
    MsgCueAudit: [],
    CommRequest_InCourse: null,
    MutationObserver: null,
    HtmlSend_InCourse: null,
    HtmlSend_Data: [],
    HtmlSend_Last: null,
    HtmlSend_LastHtml: null,
    HtmlSend_IsCapturing: false,
    mouseXPos: null,
    mouseYPos: null,
    Tracking_InCourse: null,
    webname: '" + webname + @"',
    me: " + Me + @",
    OriginalFavicon: null,
    mode: '" + Mode + @"',
    opentok_apiKey: '14117582',
    ShowDisclaimer: " + showdisclaimer + @",
    AllowCaptureMouse: " + allowcapturemouse + @",
    AllowChat: " + allowchat + @",
    AllowScreenCaptures: " + allowscreencaptures + @",
    AllowChatSockets: " + allowchatsockets + @",
    AllowScreenSockets: " + allowscreensockets + @",
    ShowHelpButton: " + showhelpbutton + @",
    AuditMessages: " + auditmessages + @",
    SessionID: '" + context.Session.SessionID + @"',
    MeetingSession: " + serializedMeetingSession + @",
    WidgetFg: " + WidgetFg + @",
    WidgetBg: " + WidgetFg + @",
    WidgetBorder: " + WidgetBorder + @",
    WidgetText: " + WidgetText + @",
    BusinessHours: " + BusinessHours + @",
    WSchat: null,
    WSscreen: null,
    IsInFrame: false
}

jVizzop(document).bind('ready.ashx', function () {
    vizzop.Daemon = new Daemon();
});
";
                sContent += vizzop;

                //CSS
                string sCss = null;
                //sCss += "vizzoplib.LoadCss(\"" + url_host_port + "/vizzop/css/zen-clean.css\");\n";
                sCss += "vizzoplib.LoadCss(\"" + url_host_port + "/vizzop/css/zen.css\");\n";
                sContent += sCss;

            }
            catch (Exception ex)
            {
                utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                sContent = null;
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