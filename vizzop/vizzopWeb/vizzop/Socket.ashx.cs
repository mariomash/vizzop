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
                            /*

                             * */
                            if ((WindowName != null) && (converser != null) && (wrapper != null))
                            {
                                List<Message> returnmessages = utils.CheckNew(wrapper, converser, WindowName);
                                if (returnmessages.Count > 0)
                                {
                                    var userMessage = returnmessages;//DateTime.Now.ToLongTimeString();
                                    var seriuserMessage = new JavaScriptSerializer().Serialize(userMessage);
                                    buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(seriuserMessage));
                                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                            }

                            receivedMessage += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                            //string receivedMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                            if (result.EndOfMessage == true)
                            {

                                var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedMessage);

                                wrapper = new HttpContextWrapper(HttpContext.Current);
                                receivedMessage = null;

                                string MessageType = dict.ContainsKey("messagetype") == false ? null : dict["messagetype"] == null ? null : dict["messagetype"].ToString();
                                switch (MessageType)
                                {
                                    case "GetWebLocations":

                                        if (converser != null)
                                        {
                                            string seriuserMessage = null;
                                            List<WebLocation> _WebLocations = utils.GetWebLocations(converser);
                                            if (_WebLocations != null)
                                            {

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
                                                seriuserMessage = new JavaScriptSerializer().Serialize(aaData);
                                            }
                                            if (seriuserMessage == null)
                                            {
                                                seriuserMessage = new JavaScriptSerializer().Serialize(false);
                                            }
                                            buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(seriuserMessage));
                                            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                        }
                                        break;

                                    case "CheckExternal":

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

                                        List<Message> returnmessages = utils.CheckExternal(
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

                                        if (returnmessages.Count > 0)
                                        {
                                            var userMessage = returnmessages;//DateTime.Now.ToLongTimeString();
                                            var seriuserMessage = new JavaScriptSerializer().Serialize(userMessage);
                                            buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(seriuserMessage));
                                            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

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