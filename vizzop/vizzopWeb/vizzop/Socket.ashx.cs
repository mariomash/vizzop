using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using System.Web.WebSockets;
using vizzopWeb.Models;

namespace vizzopWeb.vizzop
{
    /// <summary>
    /// Summary description for Socket
    /// </summary>
    [SessionState(System.Web.SessionState.SessionStateBehavior.Required)]
    public class Socket : IHttpHandler, IRequiresSessionState
    {

        private string receivedMessage = null;

        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                //context.AcceptWebSocketRequest(new WebSocketHandler());
                context.AcceptWebSocketRequest(ProcessWSChat);
            }
        }

        public bool IsReusable { get { return false; } }

        private async Task ProcessWSChat(AspNetWebSocketContext context)
        {
            using (var db = new vizzopContext())
            {
                Utils utils = new Utils(db);
                WebSocket socket = context.WebSocket;
                string WindowName = null;
                Converser converser = null;
                HttpContextWrapper wrapper = null;


                while (
                    (true) &&
                    ((socket.State == WebSocketState.Connecting) || (socket.State == WebSocketState.Open))
                    )
                {
                    try
                    {
                        //Thread.CurrentThread.Priority = ThreadPriority.Highest;
                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                        WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                        if (socket.State == WebSocketState.Open)
                        {
                            receivedMessage += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                            //string receivedMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                            List<Object> returnList = new List<object>();
                            var newmessages = utils.CheckNew(wrapper, converser, WindowName);
                            if (newmessages.Count > 0)
                            {
                                returnList = new List<Object>();
                                returnList.Add(new
                                {
                                    type = "CheckNew",
                                    data = newmessages
                                });
                            }

                            if (result.EndOfMessage == true)
                            {
                                var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedMessage);

                                wrapper = new HttpContextWrapper(HttpContext.Current);
                                receivedMessage = null;

                                //string seriuserMessage = null;

                                string MessageType = dict.ContainsKey("messagetype") == false ? null : dict["messagetype"] == null ? null : dict["messagetype"].ToString();
                                switch (MessageType)
                                {
                                    case "TrackPageView":
                                        if ((dict["username"] != null) && (dict["password"] != null) && (dict["domain"] != null))
                                        {

                                            converser = utils.GetConverserFromSystem(
                                                dict["username"].ToString(),
                                                dict["password"].ToString(),
                                                dict["domain"].ToString());

                                            var headers = "{";
                                            foreach (var key in wrapper.Request.Headers.AllKeys)
                                                headers += "'" + key + "':'" + wrapper.Request.Headers[key] + "',";

                                            headers = headers.TrimEnd(',') + "}";

                                            string sIP = wrapper.Request.ServerVariables["HTTP_CLIENT_IP"];
                                            if (string.IsNullOrEmpty(sIP) == true) { sIP = wrapper.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]; }
                                            if (string.IsNullOrEmpty(sIP) == true) { sIP = wrapper.Request.ServerVariables["REMOTE_ADDR"]; }
                                            if (string.IsNullOrEmpty(sIP) == true) { utils.GrabaLog(Utils.NivelLog.error, "No IP to Track"); sIP = "0"; }
                                            sIP = sIP.Split(',')[0];

                                            string[] languages = wrapper.Request.UserLanguages;
                                            string language = null;
                                            if (languages != null && languages.Length != 0) { language = languages[0].ToLowerInvariant().Trim(); }

                                            string useragent = wrapper.Request.UserAgent;

                                            utils.TrackPageView(
                                                converser,
                                                dict["url"].ToString(),
                                                dict["referrer"].ToString(),
                                                language,
                                                useragent,
                                                sIP,
                                                headers,
                                                dict["windowname"].ToString());

                                        }
                                        break;
                                    case "GetWebLocations":
                                        List<WebLocation> _WebLocations = new List<WebLocation>();
                                        if (converser != null)
                                        {
                                            _WebLocations = utils.GetWebLocations(converser);
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

                                        returnList.Add(
                                            new
                                            {
                                                type = "GetWebLocations",
                                                data = aaData
                                            });

                                        break;
                                    case "CheckExternal":

                                        List<Message> CheckExternalMessages = new List<Message>();

                                        if ((dict["username"] != null) && (dict["password"] != null) && (dict["domain"] != null))
                                        {

                                            converser = utils.GetConverserFromSystem(
                                                dict["username"].ToString(),
                                                dict["password"].ToString(),
                                                dict["domain"].ToString());

                                            string referrer = null;
                                            if (dict["referrer"] != null)
                                            {
                                                referrer = dict["referrer"].ToString();
                                            }
                                            string CommSessionID = null;
                                            if (dict["CommSessionID"] != null)
                                            {
                                                CommSessionID = dict["CommSessionID"].ToString();
                                            }
                                            string SessionID = null;
                                            if (dict["SessionID"] != null)
                                            {
                                                SessionID = dict["SessionID"].ToString();
                                            }
                                            if (dict["WindowName"] != null)
                                            {
                                                WindowName = dict["WindowName"].ToString();
                                            }

                                            CheckExternalMessages = utils.CheckExternal(
                                                wrapper,
                                                dict["username"].ToString(),
                                                dict["password"].ToString(),
                                                dict["domain"].ToString(),
                                                dict["url"].ToString(),
                                                referrer,
                                                CommSessionID,
                                                SessionID,
                                                WindowName,
                                                dict["MsgCueAudit"]
                                                );

                                        }

                                        if (CheckExternalMessages.Count > 0)
                                        {
                                            returnList.Add(
                                                new
                                                {
                                                    type = "CheckExternal",
                                                    data = CheckExternalMessages
                                                });
                                        }

                                        break;
                                    case "Screen":
                                        //vizzopWeb.Controllers.RealTimeController rt = new vizzopWeb.Controllers.RealTimeController();
                                        //var serializedData = new JavaScriptSerializer().Serialize(dict["data"]);

                                        utils.TrackScreen(
                                            dict["username"].ToString(),
                                            dict["password"].ToString(),
                                            dict["domain"].ToString(),
                                            dict["data"],
                                            dict["listeners"].ToString(),
                                            false,
                                            wrapper
                                            );

                                        break;

                                    case "Plain":

                                        string ToArr = dict.ContainsKey("To") == false ? "" : dict["To"] == null ? null : dict["To"].ToString();
                                        foreach (var ToComplete in ToArr.Split(','))
                                        {
                                            string To = ToComplete;
                                            if (To.Contains("::"))
                                            {
                                                To = ToComplete.Split(new string[] { "::" }, StringSplitOptions.None)[1].ToString();
                                            }
                                            string From = dict.ContainsKey("From") == false ? "" : dict["From"] == null ? null : dict["From"].ToString();

                                            string From_FullName = dict.ContainsKey("From_FullName") == false ? "" : dict["From_FullName"] == null ? null : dict["From_FullName"].ToString();
                                            if ((From_FullName == "null") || (From_FullName == ""))
                                            {
                                                From_FullName = null;
                                            }

                                            string Subject = dict.ContainsKey("Subject") == false ? "" : dict["Subject"] == null ? null : dict["Subject"].ToString();
                                            string Content = dict.ContainsKey("Content") == false ? "" : dict["Content"] == null ? null : dict["Content"].ToString();
                                            string _clientid = dict.ContainsKey("_clientid") == false ? "" : dict["_clientid"] == null ? null : dict["_clientid"].ToString();

                                            if ((_clientid == null) || (_clientid == "null") && (_clientid == ""))
                                            {
                                                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                                TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
                                                _clientid = Math.Floor(diff.TotalSeconds).ToString();
                                            }

                                            string _status = dict.ContainsKey("_status") == false ? "" : dict["_status"] == null ? null : dict["_status"].ToString();

                                            string TimeStamp = DateTime.UtcNow.ToString("o");
                                            /*
                                            dict.ContainsKey("TimeStamp") == false ? "" : dict["TimeStamp"] == null ? null : dict["TimeStamp"].ToString();
                                        if ((TimeStamp == null) || (TimeStamp == "null") && (TimeStamp == ""))
                                        {
                                            TimeStamp = DateTime.UtcNow.ToString("o");
                                        }
                                            */

                                            string TimeStampSenderSending = dict.ContainsKey("TimeStampSenderSending") == false ? "" : dict["TimeStampSenderSending"] == null ? null : dict["TimeStampSenderSending"].ToString();
                                            if ((TimeStampSenderSending == null) || (TimeStampSenderSending == "null") && (TimeStampSenderSending == ""))
                                            {
                                                TimeStampSenderSending = DateTime.UtcNow.ToString("o");
                                            }

                                            string commsessionid = dict.ContainsKey("commsessionid") == false ? "" : dict["commsessionid"] == null ? null : dict["commsessionid"].ToString();
                                            string SetTicketState = dict.ContainsKey("SetTicketState") == false ? "" : dict["SetTicketState"] == null ? null : dict["SetTicketState"].ToString();

                                            string lang = utils.GetLang(context);

                                            Content = Content.Replace(Environment.NewLine, null);

                                            NewMessage newmessage = new NewMessage();
                                            newmessage.From = From;
                                            newmessage.From_FullName = From_FullName;
                                            newmessage.To = To;
                                            newmessage.CC = ToArr;
                                            newmessage.Subject = Subject;
                                            newmessage.Content = Content;
                                            newmessage._clientid = _clientid;
                                            newmessage._status = _status;
                                            newmessage.TimeStamp = TimeStamp;
                                            newmessage.TimeStampSenderSending = TimeStampSenderSending;
                                            newmessage.TimeStampSrvAccepted = DateTime.Now.ToUniversalTime();
                                            newmessage.commsessionid = commsessionid;
                                            newmessage.Lang = lang;
                                            newmessage.MessageType = "chat";

                                            bool sent = utils.SendMessage(newmessage, SetTicketState);

                                            if (sent == false)
                                            {
                                                utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, "Msg Not Sent : " + newmessage.Subject + "," + newmessage.Content);
                                            }
                                        }
                                        break;
                                }
                            }
                            if (returnList.Count > 0)
                            {
                                var seriuserMessage = new JavaScriptSerializer().Serialize(returnList);
                                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(seriuserMessage));
                                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        utils.GrabaLogExcepcion(ex);
                        receivedMessage = null;
                    }
                }
            }
        }

    }
}