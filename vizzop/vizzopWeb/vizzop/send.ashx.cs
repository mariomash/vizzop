using System;
//using System.Web.Mvc.Resources;
//using Microsoft.Web.Mvc.Resources;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using vizzopWeb.Models;
//using Microsoft.ApplicationServer.Caching;

namespace vizzopWeb
{
    /// <summary>
    /// Descripción breve de msgapi
    /// </summary>
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    public class send : IHttpHandler
    {

        //private vizzopContext db = new vizzopContext();
        private Utils utils = new Utils();

        public void ProcessRequest(HttpContext context)
        {

            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            //context.Response.Write("true");
            //context.Response.End();

            bool result = false;
            string Callback = context.Request.QueryString["callback"];
            context.Response.ContentType = "text/javascript";

            /*
            var found_origin = false;
            foreach (var item in context.Response.Headers.AllKeys)
            {
                if (item == "Access-Control-Allow-Origin")
                {
                    found_origin = true;
                    break;
                }
            }
            if (found_origin == false)
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            }
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            }
            */

            string ToArr = context.Request.Params["To"];
            foreach (var ToComplete in ToArr.Split(','))
            {
                try
                {
                    string To = ToComplete;
                    if (To.Contains("::"))
                    {
                        To = ToComplete.Split(new string[] { "::" }, StringSplitOptions.None)[1].ToString();
                    }

                    string From = context.Request.Params["From"];
                    string From_FullName = context.Request.Params["From_FullName"];
                    if ((From_FullName == "null") || (From_FullName == ""))
                    {
                        From_FullName = null;
                    }
                    string Subject = context.Request.Params["Subject"];
                    string Content = context.Request.Params["Content"];
                    string _clientid = context.Request.Params["_clientid"];
                    if ((_clientid == null) || (_clientid == "null") && (_clientid == ""))
                    {
                        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
                        _clientid = Math.Floor(diff.TotalSeconds).ToString();
                    }
                    string _status = context.Request.Params["_status"];

                    string TimeStamp = DateTime.UtcNow.ToString("o");
                    /*
                    string TimeStamp = context.Request.Params["TimeStamp"];
                    if ((TimeStamp == null) || (TimeStamp == "null") && (TimeStamp == ""))
                    {
                        TimeStamp = DateTime.UtcNow.ToString("o");
                    }
                    */

                    string TimeStampSenderSending = context.Request.Params["TimeStampSenderSending"];
                    if ((TimeStampSenderSending == null) || (TimeStampSenderSending == "null") && (TimeStampSenderSending == ""))
                    {
                        TimeStampSenderSending = DateTime.UtcNow.ToString("o");
                    }

                    string commsessionid = context.Request.Params["commsessionid"];
                    string SetTicketState = context.Request.Params["SetTicketState"];

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

                    result = utils.SendMessage(newmessage, SetTicketState);
                }
                catch (Exception ex)
                {
                    utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, ex.Message);
                }
            }
            if (Callback != null)
            {
                context.Response.Write(Callback + "(" + result.ToString().ToLowerInvariant() + ");");
            }
            else
            {
                context.Response.Write(result.ToString().ToLowerInvariant());
            }

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