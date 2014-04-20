using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Linq;

namespace vizzopWeb.vizzop
{
    /// <summary>
    /// Summary description for Socket
    /// </summary>
    [SessionState(System.Web.SessionState.SessionStateBehavior.Required)]
    public class PhantomSocket : IHttpHandler, IRequiresSessionState
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

                while (
                    (true) &&
                    ((socket.State == WebSocketState.Connecting) || (socket.State == WebSocketState.Open))
                    )
                {
                    //Thread.CurrentThread.Priority = ThreadPriority.Highest;
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                    WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                    if (socket.State == WebSocketState.Open)
                    {

                        receivedMessage += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        //string receivedMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                        if (result.EndOfMessage == true)
                        {

                            var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedMessage);

                            var UserName = dict["username"].ToString();
                            var Domain = dict["domain"].ToString();
                            var GUID = dict["guid"].ToString();
                            var WindowName = dict["windowname"].ToString();

                            if ((UserName == null) || (Domain == null))
                            {
                                return;
                            }

                            while (socket.State == WebSocketState.Open)
                            {

                                //DateTime start_time = DateTime.Now;
                                /*
                                while ((weblocation == null) && (DateTime.Now < start_time.AddSeconds(25)))
                                {
                                }
                                */

                                //List<WebLocation> WebLocations = new List<WebLocation>();

                                WebLocation weblocation = null;
                                string region = "weblocations";
                                object _result = SingletonCache.Instance.GetAllInRegion(region);
                                if (_result != null)
                                {
                                    IEnumerable<KeyValuePair<string, object>> WebLocations = (IEnumerable<KeyValuePair<string, object>>)result;

                                    //TimeZone localZone = TimeZone.CurrentTimeZone;
                                    //DateTime loctime = localZone.ToUniversalTime(DateTime.Now.AddSeconds(-30));

                                    var obj = (from m in WebLocations
                                               //where m.TimeStamp_Last > loctime &&
                                               where ((WebLocation)m.Value).UserName == UserName &&
                                               ((WebLocation)m.Value).Domain == Domain &&
                                               ((WebLocation)m.Value).WindowName == WindowName &&
                                               ((WebLocation)m.Value).ScreenCapture != null
                                               select m).OrderByDescending(m => ((WebLocation)m.Value).TimeStamp_Last).FirstOrDefault();
                                    weblocation = (WebLocation)obj.Value;
                                }

                                if (weblocation == null)
                                {
                                    continue;
                                }
                                if ((weblocation.ScreenCapture.GUID == GUID) || (weblocation.ScreenCapture.Blob == null))
                                {
                                    continue;
                                    //Thread.Sleep(TimeSpan.FromMilliseconds(250));
                                }

                                var CompleteHtml = utils.ScrubHTML(weblocation.CompleteHtml);
                                var toReturn = new
                                {
                                    Html = CompleteHtml,
                                    Blob = weblocation.ScreenCapture.Blob,
                                    UserName = UserName,
                                    Domain = Domain,
                                    GUID = weblocation.ScreenCapture.GUID,
                                    WindowName = weblocation.ScreenCapture.WindowName,
                                    Width = weblocation.ScreenCapture.Width,
                                    Height = weblocation.ScreenCapture.Height,
                                    ScrollTop = weblocation.ScreenCapture.ScrollTop,
                                    ScrollLeft = weblocation.ScreenCapture.ScrollLeft
                                };

                                GUID = weblocation.ScreenCapture.GUID;

                                /*
            #if DEBUG
                                utils.GrabaLog(Utils.NivelLog.info, "Lanzando imagen a Phantom");
            #endif
                                */
                                string SerializedResult = new JavaScriptSerializer().Serialize(toReturn);

                                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(SerializedResult));
                                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

                            }
                        }
                    }
                }
            }
        }
    }
}