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
//using System.Web.WebSockets;
using Microsoft.Web.WebSockets;
using vizzopWeb.Models;
using vizzopWeb;

/// <summary>
/// Summary description for Socket
/// </summary>
public class VizzopWebSocket : WebSocketHandler
{
    private HttpContext context;
    //private vizzopContext db = new vizzopContext();
    public VizzopWebSocket(HttpContext _context)
    {
        context = _context;
    }

    public override void OnOpen()
    {
        //base.Send("You connected to a WebSocket!"));
    }

    public override void OnMessage(string receivedMessage)
    {
        // Echo message
        //base.OnMessage(message);

        using (var db = new vizzopContext())
        {
            Utils utils = new Utils(db);

            try
            {
                var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedMessage);

                Converser converser = null;

                if (context.Session != null)
                {
                    if (context.Session["converser"] != null)
                    {
                        converser = (Converser)context.Session["converser"];
                    }
                }
                if (converser == null)
                {
                    converser = utils.GetConverserFromSystem(
                        dict["username"].ToString(),
                        dict["password"].ToString(),
                        dict["domain"].ToString());
                }

                if (converser == null)
                {
                    return;
                }

                string MessageType = dict.ContainsKey("messagetype") == false ? null : dict["messagetype"] == null ? null : dict["messagetype"].ToString();
                switch (MessageType)
                {
                    case "Screen":
                        vizzopWeb.Controllers.RealTimeController rt = new vizzopWeb.Controllers.RealTimeController();


                        //var serializedData = new JavaScriptSerializer().Serialize(dict["data"]);

                        var wrapper = new HttpContextWrapper(context);

                        utils.TrackScreen(
                            converser.UserName,
                            converser.Password,
                            converser.Business.Domain,
                            dict["data"].ToString(),
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

                            string lang = utils.GetLang(HttpContext.Current);

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
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
        }
    }

    public override void OnClose()
    {
        // Free resources, close connections, etc.
        base.OnClose();
    }


}
