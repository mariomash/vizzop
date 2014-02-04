using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using System.Web.WebSockets;
using vizzopWeb.Models;

namespace vizzopWeb.vizzop
{
    /// <summary>
    /// Summary description for Audit
    /// </summary>

    public class Audit : IHttpHandler, IReadOnlySessionState
    {

        private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (context.Session != null)
                {
                    HttpSessionStateBase session = new HttpSessionStateWrapper(context.Session);
                    Converser converser = utils.GetLoggedConverser(session);
                    if (converser != null)
                    {
                        if (context.IsWebSocketRequest)
                        {
                            context.AcceptWebSocketRequest(Process);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
        }

        public bool IsReusable { get { return false; } }

        private async Task Process(AspNetWebSocketContext context)
        {
            try
            {
                WebSocket socket = context.WebSocket;

                int lastID = 0;
                int LastLogID = 0;
                while (true)
                {
                    Thread.Sleep(100);

                    if (socket.State != WebSocketState.Open)
                    {
                        break;
                    }
                    var DataLog = (from m in db.Logs
                                   where m.ID > LastLogID
                                   select m).OrderBy(m => m.ID).Take(10).ToList<Log>();

                    if (DataLog.Count > 0)
                    {
                        DataLog.Reverse();
                        LastLogID = (from m in DataLog select m).Max(m => m.ID);
                    }

                    //Solo tomamos los mensajes reales... porque los otros se crean con un timestamp del server
                    var DataAudit = (from m in db.MessageAudits
                                     orderby m.ID descending
                                     where m.ID > lastID && m.From != null && m.Subject.StartsWith("$#_") == false
                                     select m).Take(27).ToList<MessageAudit>();

                    if (DataAudit.Count > 0)
                    {
                        DataAudit.Reverse();
                        lastID = (from m in DataAudit select m).Max(m => m.ID);
                    }


                    //Y a montarlo
                    var Logdata = DataLog.Select((x, index) => new
                    {
                        ID = x.ID,
                        TimeStamp = x.TimeStamp,
                        Text = x.Text
                    });

                    var Auditdata = DataAudit.Select((x, index) => new
                    {
                        ID = x.ID,
                        Total = (x.TimeStampRecipientAccepted - x.TimeStamp).TotalMilliseconds,
                        InSendingClient = (x.TimeStampSenderSending - x.TimeStamp).TotalMilliseconds,
                        InServer = (x.TimeStampSrvSending - x.TimeStampSrvAccepted).TotalMilliseconds,
                        SrvName = x.MainURL,
                        UserAgent = x.From.UserAgent,
                        From = utils.GetUbicationFromIP(x.From.IP),
                        To = utils.GetUbicationFromIP(x.To.IP),
                        Subject = x.Subject
                    });

                    if ((Auditdata.Count() > 0) || (Logdata.Count() > 0))
                    {
                        var defdata = new
                        {
                            Audit = Auditdata,
                            Log = Logdata
                        };

                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        string ser = null;
                        try
                        {
                            ser = serializer.Serialize(defdata);
                        }
                        catch (Exception ex)
                        {
                            utils.GrabaLogExcepcion(ex);
                        }

                        if (ser != null)
                        {
                            ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(ser));
                            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
        }
    }
}