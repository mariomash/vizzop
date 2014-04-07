using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Web.Mvc;
using vizzopWeb.Models;
using System.Web.Script.Serialization;
using System.Text;
using System.Security.Cryptography;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Collections;
//using Microsoft.Security.Application;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using System.Linq.Expressions;
using Microsoft.ApplicationServer.Caching;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web.WebSockets;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.ComponentModel;
using Newtonsoft.Json;


public class LZString
{

    private class Context_Compress
    {
        public Dictionary<string, int> dictionary { get; set; }
        public Dictionary<string, bool> dictionaryToCreate { get; set; }
        public string c { get; set; }
        public string wc { get; set; }
        public string w { get; set; }
        public int enlargeIn { get; set; }
        public int dictSize { get; set; }
        public int numBits { get; set; }
        public Context_Compress_Data data { get; set; }
    }

    private class Context_Compress_Data
    {
        public string str { get; set; }
        public int val { get; set; }
        public int position { get; set; }
    }

    private class Decompress_Data
    {
        public string str { get; set; }
        public int val { get; set; }
        public int position { get; set; }
        public int index { get; set; }
    }

    private static Context_Compress_Data writeBit(int value, Context_Compress_Data data)
    {
        data.val = (data.val << 1) | value;

        if (data.position == 15)
        {
            data.position = 0;
            data.str += (char)data.val;
            data.val = 0;
        }
        else
            data.position++;

        return data;
    }

    private static Context_Compress_Data writeBits(int numbits, int value, Context_Compress_Data data)
    {

        for (var i = 0; i < numbits; i++)
        {
            data = writeBit(value & 1, data);
            value = value >> 1;
        }

        return data;
    }

    private static Context_Compress produceW(Context_Compress context)
    {

        if (context.dictionaryToCreate.ContainsKey(context.w))
        {
            if (context.w[0] < 256)
            {
                context.data = writeBits(context.numBits, 0, context.data);
                context.data = writeBits(8, context.w[0], context.data);
            }
            else
            {
                context.data = writeBits(context.numBits, 1, context.data);
                context.data = writeBits(16, context.w[0], context.data);
            }

            context = decrementEnlargeIn(context);
            context.dictionaryToCreate.Remove(context.w);
        }
        else
        {
            context.data = writeBits(context.numBits, context.dictionary[context.w], context.data);
        }

        return context;
    }

    private static Context_Compress decrementEnlargeIn(Context_Compress context)
    {

        context.enlargeIn--;
        if (context.enlargeIn == 0)
        {
            context.enlargeIn = (int)Math.Pow(2, context.numBits);
            context.numBits++;
        }
        return context;
    }

    public static string compress(string uncompressed)
    {

        Context_Compress context = new Context_Compress();
        Context_Compress_Data data = new Context_Compress_Data();

        context.dictionary = new Dictionary<string, int>();
        context.dictionaryToCreate = new Dictionary<string, bool>();
        context.c = "";
        context.wc = "";
        context.w = "";
        context.enlargeIn = 2;
        context.dictSize = 3;
        context.numBits = 2;

        data.str = "";
        data.val = 0;
        data.position = 0;

        context.data = data;

        try
        {
            for (int i = 0; i < uncompressed.Length; i++)
            {
                context.c = uncompressed[i].ToString();

                if (!context.dictionary.ContainsKey(context.c))
                {
                    context.dictionary[context.c] = context.dictSize++;
                    context.dictionaryToCreate[context.c] = true;
                };

                context.wc = context.w + context.c;

                if (context.dictionary.ContainsKey(context.wc))
                {
                    context.w = context.wc;
                }
                else
                {
                    context = produceW(context);
                    context = decrementEnlargeIn(context);
                    context.dictionary[context.wc] = context.dictSize++;
                    context.w = context.c;
                }
            }

            if (context.w != "")
            {
                context = produceW(context);
            }

            // Mark the end of the stream
            context.data = writeBits(context.numBits, 2, context.data);

            // Flush the last char
            while (true)
            {
                context.data.val = (context.data.val << 1);
                if (context.data.position == 15)
                {
                    context.data.str += (char)context.data.val;
                    break;
                }
                else
                    context.data.position++;
            }

        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return context.data.str;
    }

    private static int readBit(Decompress_Data data)
    {

        var res = data.val & data.position;

        data.position >>= 1;

        if (data.position == 0)
        {
            data.position = 32768;

            // This 'if' check doesn't appear in the orginal lz-string javascript code.
            // Added as a check to make sure we don't exceed the length of data.str
            // The javascript charCodeAt will return a NaN if it exceeds the index but will not error out
            if (data.index < data.str.Length)
            {
                data.val = data.str[data.index++]; // data.val = data.string.charCodeAt(data.index++); <---javascript equivilant
            }
        }

        return res > 0 ? 1 : 0;
    }

    private static int readBits(int numBits, Decompress_Data data)
    {

        int res = 0;
        int maxpower = (int)Math.Pow(2, numBits);
        int power = 1;

        while (power != maxpower)
        {
            res |= readBit(data) * power;
            power <<= 1;
        }

        return res;
    }

    public static string decompress(string compressed)
    {

        Decompress_Data data = new Decompress_Data();

        List<string> dictionary = new List<string>();
        int next = 0;
        int enlargeIn = 4;
        int numBits = 3;
        string entry = "";
        string result = "";
        int i = 0;
        dynamic w = "";
        dynamic c = "";
        int errorCount = 0;

        data.str = compressed;
        data.val = (int)compressed[0];
        data.position = 32768;
        data.index = 1;

        try
        {
            for (i = 0; i < 3; i++)
            {
                dictionary.Add(i.ToString());
            }

            next = readBits(2, data);

            switch (next)
            {
                case 0:
                    c = Convert.ToChar(readBits(8, data)).ToString();
                    break;
                case 1:
                    c = Convert.ToChar(readBits(16, data)).ToString();
                    break;
                case 2:
                    return "";
            }

            dictionary.Add(c);
            w = result = c;

            while (true)
            {
                c = readBits(numBits, data);
                int cc = (int)(c);

                switch (cc)
                {
                    case 0:
                        if (errorCount++ > 10000)
                            throw new Exception("To many errors");

                        c = Convert.ToChar(readBits(8, data)).ToString();
                        dictionary.Add(c);
                        c = dictionary.Count - 1;
                        enlargeIn--;

                        break;
                    case 1:
                        c = Convert.ToChar(readBits(16, data)).ToString();
                        dictionary.Add(c);
                        c = dictionary.Count - 1;
                        enlargeIn--;

                        break;
                    case 2:
                        return result;
                }

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }


                if (dictionary.ElementAtOrDefault((int)c) != null) // if (dictionary[c] ) <------- original Javascript Equivalant
                {
                    entry = dictionary[c];
                }
                else
                {
                    if (c == dictionary.Count)
                    {
                        entry = w + w[0];
                    }
                    else
                    {
                        return null;
                    }
                }

                result += entry;
                dictionary.Add(w + entry[0]);
                enlargeIn--;
                w = entry;

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public static string compressToUTF16(string input)
    {

        string output = "";
        int status = 0;
        int current = 0;

        try
        {
            if (input == null)
                throw new Exception("Input is Null");

            input = compress(input);
            if (input.Length == 0)
                return input;

            for (int i = 0; i < input.Length; i++)
            {
                int c = (int)input[i];
                switch (status++)
                {
                    case 0:
                        output += (char)((c >> 1) + 32);
                        current = (c & 1) << 14;
                        break;
                    case 1:
                        output += (char)((current + (c >> 2)) + 32);
                        current = (c & 3) << 13;
                        break;
                    case 2:
                        output += (char)((current + (c >> 3)) + 32);
                        current = (c & 7) << 12;
                        break;
                    case 3:
                        output += (char)((current + (c >> 4)) + 32);
                        current = (c & 15) << 11;
                        break;
                    case 4:
                        output += (char)((current + (c >> 5)) + 32);
                        current = (c & 31) << 10;
                        break;
                    case 5:
                        output += (char)((current + (c >> 6)) + 32);
                        current = (c & 63) << 9;
                        break;
                    case 6:
                        output += (char)((current + (c >> 7)) + 32);
                        current = (c & 127) << 8;
                        break;
                    case 7:
                        output += (char)((current + (c >> 8)) + 32);
                        current = (c & 255) << 7;
                        break;
                    case 8:
                        output += (char)((current + (c >> 9)) + 32);
                        current = (c & 511) << 6;
                        break;
                    case 9:
                        output += (char)((current + (c >> 10)) + 32);
                        current = (c & 1023) << 5;
                        break;
                    case 10:
                        output += (char)((current + (c >> 11)) + 32);
                        current = (c & 2047) << 4;
                        break;
                    case 11:
                        output += (char)((current + (c >> 12)) + 32);
                        current = (c & 4095) << 3;
                        break;
                    case 12:
                        output += (char)((current + (c >> 13)) + 32);
                        current = (c & 8191) << 2;
                        break;
                    case 13:
                        output += (char)((current + (c >> 14)) + 32);
                        current = (c & 16383) << 1;
                        break;
                    case 14:
                        output += (char)((current + (c >> 15)) + 32);
                        output += (char)((c & 32767) + 32);
                        status = 0;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return output + (char)(current + 32);
    }

    public static string decompressFromUTF16(string input)
    {

        string output = "";
        int status = 0;
        int current = 0;
        int i = 0;

        try
        {
            if (input == null)
                throw new Exception("input is Null");

            while (i < input.Length)
            {
                int c = ((int)input[i]) - 32;

                switch (status++)
                {
                    case 0:
                        current = c << 1;
                        break;
                    case 1:
                        output += (char)(current | (c >> 14));
                        current = (c & 16383) << 2;
                        break;
                    case 2:
                        output += (char)(current | (c >> 13));
                        current = (c & 8191) << 3;
                        break;
                    case 3:
                        output += (char)(current | (c >> 12));
                        current = (c & 4095) << 4;
                        break;
                    case 4:
                        output += (char)(current | (c >> 11));
                        current = (c & 2047) << 5;
                        break;
                    case 5:
                        output += (char)(current | (c >> 10));
                        current = (c & 1023) << 6;
                        break;
                    case 6:
                        output += (char)(current | (c >> 9));
                        current = (c & 511) << 7;
                        break;
                    case 7:
                        output += (char)(current | (c >> 8));
                        current = (c & 255) << 8;
                        break;
                    case 8:
                        output += (char)(current | (c >> 7));
                        current = (c & 127) << 9;
                        break;
                    case 9:
                        output += (char)(current | (c >> 6));
                        current = (c & 63) << 10;
                        break;
                    case 10:
                        output += (char)(current | (c >> 5));
                        current = (c & 31) << 11;
                        break;
                    case 11:
                        output += (char)(current | (c >> 4));
                        current = (c & 15) << 12;
                        break;
                    case 12:
                        output += (char)(current | (c >> 3));
                        current = (c & 7) << 13;
                        break;
                    case 13:
                        output += (char)(current | (c >> 2));
                        current = (c & 3) << 14;
                        break;
                    case 14:
                        output += (char)(current | (c >> 1));
                        current = (c & 1) << 15;
                        break;
                    case 15:
                        output += (char)(current | c);
                        status = 0;
                        break;
                }

                i++;
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return decompress(output);
    }

    public static string compressToBase64(string input)
    {

        string _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
        string output = "";

        // Using the data type 'double' for these so that the .Net double.NaN & double.IsNaN functions can be used
        // later in the function.  .Net doesn't have a similar function for regular integers.
        double chr1, chr2, chr3 = 0.0;

        int enc1 = 0;
        int enc2 = 0;
        int enc3 = 0;
        int enc4 = 0;
        int i = 0;

        try
        {
            if (input == null)
                throw new Exception("input is Null");

            input = compress(input);

            while (i < input.Length * 2)
            {
                if (i % 2 == 0)
                {
                    chr1 = (int)input[i / 2] >> 8;
                    chr2 = (int)input[i / 2] & 255;
                    if (i / 2 + 1 < input.Length)
                        chr3 = (int)input[i / 2 + 1] >> 8;
                    else
                        chr3 = double.NaN;//chr3 = NaN; <------ original Javascript Equivalent
                }
                else
                {
                    chr1 = (int)input[(i - 1) / 2] & 255;
                    if ((i + 1) / 2 < input.Length)
                    {
                        chr2 = (int)input[(i + 1) / 2] >> 8;
                        chr3 = (int)input[(i + 1) / 2] & 255;
                    }
                    else
                    {
                        chr2 = chr3 = double.NaN; // chr2 = chr3 = NaN; <------ original Javascript Equivalent
                    }
                }
                i += 3;


                enc1 = (int)(Math.Round(chr1)) >> 2;

                // The next three 'if' statements are there to make sure we are not trying to calculate a value that has been 
                // assigned to 'double.NaN' above.  The orginal Javascript functions didn't need these checks due to how
                // Javascript functions.
                // Also, due to the fact that some of the variables are of the data type 'double', we have to do some type 
                // conversion to get the 'enc' variables to be the correct value.
                if (!double.IsNaN(chr2))
                {
                    enc2 = (((int)(Math.Round(chr1)) & 3) << 4) | ((int)(Math.Round(chr2)) >> 4);
                }

                if (!double.IsNaN(chr2) && !double.IsNaN(chr3))
                {
                    enc3 = (((int)(Math.Round(chr2)) & 15) << 2) | ((int)(Math.Round(chr3)) >> 6);
                }

                if (!double.IsNaN(chr3))
                {

                    enc4 = (int)(Math.Round(chr3)) & 63;
                }

                if (double.IsNaN(chr2)) //if (isNaN(chr2)) <------ original Javascript Equivalent
                {
                    enc3 = enc4 = 64;
                }
                else if (double.IsNaN(chr3)) //else if (isNaN(chr3)) <------ original Javascript Equivalent
                {
                    enc4 = 64;
                }

                output = output + _keyStr[enc1] + _keyStr[enc2] + _keyStr[enc3] + _keyStr[enc4];
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return output;
    }

    public static string decompressFromBase64(string input)
    {

        string _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        string output = "";
        int output_ = 0;
        int ol = 0;
        int chr1, chr2, chr3 = 0;
        int enc1, enc2, enc3, enc4 = 0;
        int i = 0;

        try
        {
            if (input == null)
                throw new Exception("input is Null");

            var regex = new Regex(@"[^A-Za-z0-9-\+\/\=]");
            input = regex.Replace(input, "");

            while (i < input.Length)
            {
                enc1 = _keyStr.IndexOf(input[i++]);
                enc2 = _keyStr.IndexOf(input[i++]);
                enc3 = _keyStr.IndexOf(input[i++]);
                enc4 = _keyStr.IndexOf(input[i++]);

                chr1 = (enc1 << 2) | (enc2 >> 4);
                chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
                chr3 = ((enc3 & 3) << 6) | enc4;

                if (ol % 2 == 0)
                {
                    output_ = chr1 << 8;

                    if (enc3 != 64)
                    {
                        output += (char)(output_ | chr2);
                    }

                    if (enc4 != 64)
                    {
                        output_ = chr3 << 8;
                    }
                }
                else
                {
                    output = output + (char)(output_ | chr1);

                    if (enc3 != 64)
                    {
                        output_ = chr2 << 8;
                    }
                    if (enc4 != 64)
                    {
                        output += (char)(output_ | chr3);
                    }
                }
                ol += 3;
            }

            // Send the output out to the main decompress function
            output = decompress(output);
        }
        catch (Exception)
        {
            return null;
        }

        return output;
    }

}

public static class LabelExtensions
{
    public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
    {
        return html.LabelFor(expression, null, htmlAttributes);
    }

    public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText, object htmlAttributes)
    {
        return html.LabelHelper(
            ModelMetadata.FromLambdaExpression(expression, html.ViewData),
            ExpressionHelper.GetExpressionText(expression),
            HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes),
            labelText);
    }

    private static MvcHtmlString LabelHelper(this HtmlHelper html, ModelMetadata metadata, string htmlFieldName, IDictionary<string, object> htmlAttributes, string labelText = null)
    {
        var str = labelText
            ?? (metadata.DisplayName
            ?? (metadata.PropertyName
            ?? htmlFieldName.Split(new[] { '.' }).Last()));

        if (string.IsNullOrEmpty(str))
            return MvcHtmlString.Empty;

        var tagBuilder = new TagBuilder("label");
        tagBuilder.MergeAttributes(htmlAttributes);
        tagBuilder.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
        tagBuilder.SetInnerText(str);

        return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
    }

    private static MvcHtmlString ToMvcHtmlString(this TagBuilder tagBuilder, TagRenderMode renderMode)
    {
        return new MvcHtmlString(tagBuilder.ToString(renderMode));
    }
}

public class BooleanRequired : RequiredAttribute, IClientValidatable
{
    public BooleanRequired() { }

    public override bool IsValid(object value)
    {
        return value != null && (bool)value == true;
    }

    public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
    {
        return new ModelClientValidationRule[] { new ModelClientValidationRule() { ValidationType = "brequired", ErrorMessage = this.ErrorMessage } };
    }
}

/// <summary>
/// Renders result as JSON and also wraps the JSON in a call
/// to the callback function specified in "JsonpResult.Callback".
/// </summary>
public class JsonpResult : JsonResult
{
    private vizzopWeb.Utils utils = new vizzopWeb.Utils();
    /// <summary>
    /// Gets or sets the javascript callback function that is
    /// to be invoked in the resulting script output.
    /// </summary>
    /// <value>The callback function name.</value>
    public string Callback { get; set; }

    /// <summary>
    /// Enables processing of the result of an action method by a
    /// custom type that inherits from <see cref="T:System.Web.Mvc.ActionResult"/>.
    /// </summary>
    /// <param name="context">The context within which the
    /// result is executed.</param>
    public override void ExecuteResult(ControllerContext context)
    {
        if (context == null)
            throw new ArgumentNullException("context");

        HttpResponseBase response = context.HttpContext.Response;
        if (!String.IsNullOrEmpty(ContentType))
            response.ContentType = "application/json";
        else
            response.ContentType = "application/javascript";

        if (ContentEncoding != null)
            response.ContentEncoding = ContentEncoding;

        if (Callback == null || Callback.Length == 0)
            Callback = context.HttpContext.Request.QueryString["callback"];

        if (Data != null)
        {
            // The JavaScriptSerializer type was marked as obsolete
            // prior to .NET Framework 3.5 SP1 
#pragma warning disable 0618
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string ser = "";
            try
            {
                ser = serializer.Serialize(Data);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                utils.GrabaLog(vizzopWeb.Utils.NivelLog.error, context.HttpContext.Request.Url.AbsoluteUri);
            }
            if (Callback != null)
            {
                response.Write(Callback + "(" + ser + ");");
            }
            else
            {
                response.Write(ser);
            }
#pragma warning restore 0618
        }
    }
}

public class JsonpFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        if (filterContext == null)
            throw new ArgumentNullException("filterContext");

        //
        // see if this request included a "callback" querystring parameter
        //
        string callback = filterContext.HttpContext.Request.QueryString["callback"];
        //if (callback != null && callback.Length > 0)
        //{
        //
        // ensure that the result is a "JsonResult"
        //
        JsonResult result = filterContext.Result as JsonResult;
        if (result == null)
        {
            return;
            /*
            throw new InvalidOperationException("JsonpFilterAttribute must be applied only " +
                "on controllers and actions that return a JsonResult object.");
             * */
        }

        filterContext.Result = new JsonpResult
        {
            ContentEncoding = result.ContentEncoding,
            ContentType = result.ContentType,
            Data = result.Data,
            Callback = callback
        };
        //}
    }
}

public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        /*
        if (filterContext.RequestContext.HttpContext.Response.Headers.AllKeys.Contains("Access-Control-Allow-Origin") == false)
        {
            // If you want it formated in some other way.
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
        {
            filterContext.RequestContext.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
        }
         */
        base.OnActionExecuting(filterContext);
    }
}

namespace vizzopWeb
{

    public class Sms
    {
        private Utils utils = new Utils();

        private string _message = null;

        private string _phonenumber = null;

        public string message
        {
            get { return _message; }
            set { _message = value; }
        }

        public string phonenumber
        {
            get { return _phonenumber; }
            set { _phonenumber = value; }
        }

        public string stringToHex(string s)
        {
            string hex = "";
            foreach (char c in s)
            {
                int tmp = c;
                hex += String.Format("{0:x4}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }

        public Sms() { }

        public Boolean send()
        {
            try
            {
                UriBuilder urlBuilder = new UriBuilder();
                urlBuilder.Scheme = "http";
                urlBuilder.Host = "www.bulksms.co.uk";
                urlBuilder.Port = 5567;
                urlBuilder.Path = "/eapi/submission/send_sms/2/2.0";
                /*
                * If your firewall blocks access to port 5567, you can fall back to port 80:
                * In other words you do not need to set 'urlBuilder.Port' to anything, you can
                * comment it out
                * //urlBuilder.Port = 5576;
                * (See FAQ for more details.)
                */

                /*
                * Construct Http Post body/data
                */
                /*
                * Note the suggested encoding for certain parameters, notably
                * the username, password and especially the message.  ISO-8859-1
                * is essentially the character set that we use for message bodies,
                * with a few exceptions for (e.g.) Greek characters. For a full list,
                * see: http://www.bulksms.co.uk/docs/eapi/submission/character_encoding/
                */
                string data = "";
                data += "username=" + HttpUtility.UrlEncode("foodmovin", System.Text.Encoding.GetEncoding("ISO-8859-1"));
                data += "&password=" + HttpUtility.UrlEncode("Q4b4s4bslc", System.Text.Encoding.GetEncoding("ISO-8859-1"));
                data += "&message=" + stringToHex(this.message);
                data += "&dca=16bit";
                data += "&want_report=1";
                data += "&msisdn=" + this.phonenumber;

                /*
                * Make Http Post request to server
                */
                urlBuilder.Query = string.Format(data); ;

                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString()));
                httpReq.Method = "POST";
                HttpWebResponse httpResponse = (HttpWebResponse)(httpReq.GetResponse());

                StreamReader input = new StreamReader(httpResponse.GetResponseStream());

                String res = input.ReadToEnd();

                String[] parts = res.Split('|');

                String statusCode = parts[0];
                String statusString = parts[1];

                httpResponse.Close();

                if (!statusCode.Equals("0"))
                {
                    //Console.WriteLine("Error: " + statusCode + ": " + statusString);
                    utils.GrabaLog(Utils.NivelLog.error, "Error sending SMS: " + statusCode + ": " + statusString + ": " + this.phonenumber + "/" + this.message);
                    return false;
                }
                else
                {
                    //Console.WriteLine("Success: batch ID " + parts[2]);
                    utils.GrabaLog(Utils.NivelLog.info, "SMS sent to: " + this.phonenumber + "/" + this.message);
                    return true;
                }


            }
            catch (Exception ex)
            {
                //Console.WriteLine("{0} Exception caught.", e);
                utils.GrabaLogExcepcion(ex);
                return false;

            }
        }
    }

    public class Utils
    {
        public vizzopContext db = null;

        public static string google_analytics_key = "UA-29363897-1";

        public Utils(vizzopContext _db)
        {
            if (_db != null)
            {
                db = _db;
            }
            else
            {
                db = new vizzopContext();
            }
        }

        public Utils()
        {
            db = new vizzopContext();
        }

        public string vendornumber = "2084568";

        public string secret = "tortuga";

        public string GetIP(HttpContext Context)
        {
            HttpContextBase abstractContext = new System.Web.HttpContextWrapper(Context);
            return GetIP(abstractContext);
        }

        public string GetIP(HttpContextBase Context)
        {
            string sIP = "127.0.0.1";
            try
            {
                sIP = Context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(sIP) == true) { sIP = Context.Request.ServerVariables["REMOTE_ADDR"]; }
                else { sIP = sIP.Split(',')[0]; }
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
            }
            return sIP;
        }

        public string GetLang(AspNetWebSocketContext Context)
        {
            string lang = "en";
            try
            {
                string[] languages = Context.UserLanguages;
                if (languages != null && languages.Length != 0) { lang = languages[0].ToLowerInvariant().Trim(); }
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
            }
            return lang;
        }

        public string GetLang(HttpContext Context)
        {
            HttpContextBase abstractContext = new System.Web.HttpContextWrapper(Context);
            return GetLang(abstractContext);
        }

        public string GetLang(HttpContextBase Context)
        {
            string lang = "en";
            try
            {
                string[] languages = Context.Request.UserLanguages;
                if (languages != null && languages.Length != 0) { lang = languages[0].ToLowerInvariant().Trim(); }
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
            }
            return lang;
        }

        public Converser CreateConverserinDB(Converser oConverser)
        {
            try
            {
                vizzopContext db = new vizzopContext();

                //Aseguremonos de que no hay APIKEYs repetidas...
                var guid = Guid.NewGuid().ToString();
                while ((from m in db.Conversers
                        where m.UserName == guid
                        select m).FirstOrDefault() != null)
                {
                    guid = Guid.NewGuid().ToString();
                }

                oConverser.UserName = guid;
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = localZone.ToUniversalTime(DateTime.Now);
                oConverser.LastActive = loctime;
                oConverser.CreatedOn = loctime;
                oConverser.Password = Guid.NewGuid().ToString();
                oConverser.Active = true;
                db.Conversers.Add(oConverser);
                db.SaveChanges();
                return oConverser;
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public String decompress_LZW(String[] compressed)
        {
            int j = 1;
            try
            {
                // Build the dictionary.
                int dictSize = 256;
                Dictionary<Int32, String> dictionary = new Dictionary<Int32, String>();
                for (int i = 0; i < 256; i++)
                {
                    dictionary.Add(i, "" + (char)i);
                }

                String w = "" + (char)Convert.ToInt32(compressed[0]);
                String result = w;

                for (j = 1; j < compressed.Length; j += 1)
                {
                    int k = 0;
                    try
                    {
                        k = Convert.ToInt32(compressed[j]);
                    }
                    catch (Exception ex)
                    {
                        GrabaLog(Utils.NivelLog.error, ex.Message);
                    }
                    String entry;
                    if (dictionary.ContainsKey(k))
                    {
                        entry = (from m in dictionary
                                 where m.Key == k
                                 select m).FirstOrDefault().Value;
                    }
                    else
                    {
                        if (k == dictSize)
                        {
                            entry = w + w[0];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    result += entry;

                    // Add w+entry[0] to the dictionary.
                    dictionary.Add(dictSize++, w + entry[0]);

                    w = entry;
                }
                return result;
            }
            catch (Exception e)
            {
                GrabaLog(Utils.NivelLog.error, j + " " + e.Message);
                return null;
            }
        }

        public MeetingSession GetMeetingSessionFromSystemSerializedProof(string MeetingSessionID)
        {
            try
            {
                MeetingSession meetingSession = GetMeetingSessionFromSystem(MeetingSessionID);
                MeetingSession returnMeetingSession = new MeetingSession();
                if (meetingSession == null)
                {
                    return null;
                }
                else
                {
                    returnMeetingSession.Business = new Business();
                    returnMeetingSession.Business.Domain = meetingSession.Business.Domain;
                    returnMeetingSession.Comments = meetingSession.Comments;

                    List<Converser> conversers = new List<Converser>();
                    foreach (Converser originalConverser in meetingSession.Conversers)
                    {
                        Converser newConverser = new Converser();
                        newConverser.Email = originalConverser.Email;
                        newConverser.FullName = originalConverser.FullName;
                        newConverser.ID = originalConverser.ID;
                        newConverser.LangISO = originalConverser.LangISO;
                        newConverser.UserName = originalConverser.UserName;
                        newConverser.UserAgent = originalConverser.UserAgent;
                        newConverser.Business = new Business();
                        newConverser.Business.Domain = originalConverser.Business.Domain;
                        conversers.Add(newConverser);
                    }

                    returnMeetingSession.Conversers = conversers;
                    returnMeetingSession.CreatedOn = meetingSession.CreatedOn;
                    returnMeetingSession.ID = meetingSession.ID;

                    List<Message> messages = new List<Message>();
                    foreach (Message originalMessage in meetingSession.Messages)
                    {
                        Message newMessage = new Message();
                        newMessage.MeetingSession = new MeetingSession();
                        newMessage.MeetingSession.ID = originalMessage.MeetingSession.ID;
                        newMessage.Content = originalMessage.Content;
                        newMessage.From = new Converser();
                        newMessage.From.ID = originalMessage.From.ID;
                        newMessage.From.UserName = originalMessage.From.UserName;
                        newMessage.From.Business = new Business();
                        newMessage.From.Business.Domain = originalMessage.From.Business.Domain;
                        newMessage.To = new Converser();
                        newMessage.To.ID = originalMessage.To.ID;
                        newMessage.To.UserName = originalMessage.To.UserName;
                        newMessage.To.Business = new Business();
                        newMessage.To.Business.Domain = originalMessage.To.Business.Domain;
                        newMessage.ID = originalMessage.ID;
                        newMessage.Lang = originalMessage.Lang;
                        newMessage.MessageType = originalMessage.MessageType;
                        newMessage.Sent = originalMessage.Sent;
                        newMessage.Status = originalMessage.Status;
                        newMessage.Subject = originalMessage.Subject;
                        newMessage.TimeStamp = originalMessage.TimeStamp;
                        newMessage.Ubication = originalMessage.Ubication;
                    }

                    returnMeetingSession.AssociatedConverser = new Converser();
                    returnMeetingSession.AssociatedConverser.UserName = meetingSession.AssociatedConverser.UserName;
                    returnMeetingSession.AssociatedConverser.Business = new Business();
                    returnMeetingSession.AssociatedConverser.Business.Domain = meetingSession.AssociatedConverser.Business.Domain;

                }
                return returnMeetingSession;
            }
            catch (System.Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public Message TransformMessageToSerializedProof(Message oldmsg)
        {
            Message returnmsg = new Message();
            try
            {
                returnmsg.Content = oldmsg.Content;
                returnmsg.From = new Converser();
                returnmsg.From.ID = oldmsg.From.ID;
                returnmsg.From.UserName = oldmsg.From.UserName;
                returnmsg.From.Password = oldmsg.From.Password;
                returnmsg.From.FullName = oldmsg.From.FullName;
                returnmsg.From.LangISO = oldmsg.From.LangISO;
                returnmsg.From.IP = oldmsg.From.IP;
                returnmsg.From.UserAgent = oldmsg.From.UserAgent;
                returnmsg.From.Business = new Business();
                returnmsg.From.Business.Domain = oldmsg.From.Business.Domain;
                returnmsg.CC = oldmsg.CC;
                returnmsg.ID = oldmsg.ID;
                returnmsg.To = new Converser();
                returnmsg.To.ID = oldmsg.To.ID;
                returnmsg.To.UserName = oldmsg.To.UserName;
                returnmsg.To.Password = oldmsg.To.Password;
                returnmsg.To.FullName = oldmsg.To.FullName;
                returnmsg.To.Business = new Business();
                returnmsg.To.Business.Domain = oldmsg.To.Business.Domain;
                returnmsg.To.LangISO = oldmsg.To.LangISO;
                returnmsg.To.IP = oldmsg.To.IP;
                returnmsg.To.UserAgent = oldmsg.To.UserAgent;
                returnmsg.ID = oldmsg.ID;
                returnmsg.Status = oldmsg.Status;
                returnmsg.TimeStamp = oldmsg.TimeStamp;
                returnmsg.TimeStampSenderSending = oldmsg.TimeStampSenderSending;
                returnmsg.TimeStampSrvAccepted = oldmsg.TimeStampSrvAccepted;
                returnmsg.TimeStampSrvSending = oldmsg.TimeStampSrvSending;
                returnmsg.Subject = oldmsg.Subject;
                returnmsg.Status = oldmsg.Status;
                returnmsg.ClientID = oldmsg.ClientID;
                if (oldmsg.CommSession != null)
                {
                    returnmsg.CommSession = new CommSession();
                    returnmsg.CommSession.ID = oldmsg.CommSession.ID;
                    returnmsg.CommSession.Status = oldmsg.CommSession.Status;
                }
                if (oldmsg.MeetingSession != null)
                {
                    returnmsg.MeetingSession = new MeetingSession();
                    returnmsg.MeetingSession.ID = oldmsg.MeetingSession.ID;
                }
                returnmsg.db = null;
                returnmsg.utils = null;

                // Y un poquito de censura de passwords
                returnmsg.From.Password = null;
                returnmsg.To.Password = null;
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
            return returnmsg;
        }

        public MeetingSession GetMeetingSessionFromSystem(string MeetingSessionID)
        {
            try
            {
                int convertedMeetingSessionID = Convert.ToInt16(MeetingSessionID);
                MeetingSession meetingSession = (from m in db.MeetingSessions
                                                     .Include("Business")
                                                     .Include("Conversers")
                                                     .Include("Messages")
                                                 where m.ID == convertedMeetingSessionID
                                                 select m).FirstOrDefault();

                return meetingSession;
            }
            catch (System.Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public Converser GetLoggedConverser(HttpSessionStateBase session)
        {
            vizzopContext _db = new vizzopContext();
            try
            {
                if (session["converser"] == null)
                {
                    return null;
                }
                var _converser = (Converser)session["converser"];
                Converser converser = GetConverserFromSystem(_converser.UserName, _converser.Password, _converser.Business.Domain);
                return converser;
            }
            catch (System.Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public Converser GetLoggedConverser(HttpSessionStateBase session, vizzopContext DBContext)
        {
            vizzopContext _db = new vizzopContext();
            try
            {
                if (session["converser"] == null)
                {
                    return null;
                }
                var _converser = (Converser)session["converser"];
                Converser converser = GetConverserFromSystem(_converser.UserName, _converser.Password, _converser.Business.Domain, DBContext);
                return converser;
            }
            catch (System.Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public Converser GetConverserFromSystem(string Email, string Password, vizzopContext db)
        {
            Converser converser = null;

            try
            {
                if (db == null)
                {
                    db = new vizzopContext();
                }


                converser = (from m in db.Conversers.Include("Business").Include("Agent")
                             where m.Email == Email
                             && m.Password == Password
                             select m).FirstOrDefault();

                if (converser == null)
                {
                    return null;
                }

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = DateTime.Now;
                DateTime loctimeUTC = localZone.ToUniversalTime(loctime);
                converser.LastActive = loctimeUTC;

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                //Utils utils = new Utils();
                GrabaLogExcepcion(ex);

            }
            return converser;
        }

        public Converser GetConverserFromSystem(string UserName, string Password, string Domain)
        {
            return GetConverserFromSystem(UserName, Password, Domain, null);
        }

        public Converser GetConverserFromSystem(string UserName, string Password, string Domain, vizzopContext _db)
        {
            Converser converser = null;

            try
            {
                if (_db == null)
                {
                    _db = db;
                }

                converser = (from m in _db.Conversers.Include("Business").Include("Agent")
                             where m.UserName == UserName
                             && m.Password == Password
                             && m.Business.Domain == Domain
                             select m).FirstOrDefault();

                if (converser == null)
                {
                    return null;
                }

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = DateTime.Now;
                DateTime loctimeUTC = localZone.ToUniversalTime(loctime);
                converser.LastActive = loctimeUTC;

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                //Utils utils = new Utils();
                GrabaLogExcepcion(ex);
            }
            return converser;
        }

        static string ParamName<T>(Expression<Func<T>> expr)
        {
            var body = ((MemberExpression)expr.Body);
            return body.Member.Name.ToString();
        }

        public ScreenCapture GetScreenCaptureByGUID(string UserName, string Domain, string GUID)
        {
            ScreenCapture return_sc = null;

            try
            {
                //Esto es rápido ;)
                return_sc = (from m in db.ScreenCaptures.Include("converser").Include("converser.Business")
                             where m.converser.UserName == UserName &&
                             m.converser.Business.Domain == Domain &&
                             m.GUID == GUID
                             select m).FirstOrDefault();
            }
            catch (Exception e)
            {
                GrabaLogExcepcion(e);
                return null;
            }
            return return_sc;
        }

        public Bitmap PrepareScreenToReturn(ScreenCapture sc, string width, string height, bool withdata)
        {
            Bitmap bitImage = null;
            try
            {
                //Formateando la imagen...
                if ((sc.Data == null) || (sc.Data == ""))
                {
                    return null;
                }
                Byte[] bitmapData = new Byte[sc.Data.Length];
                bitmapData = Convert.FromBase64String(FixBase64ForImage(sc.Data.Substring(sc.Data.IndexOf(',') + 1)));
                if (bitmapData == null)
                {
                    return null;
                }
                System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
                bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

                //Ya no no interesa toda la pantalla.. ya la tenemos mas cortica ;-)
                Rectangle cropRect = new Rectangle(0, 0, sc.Width, sc.Height);
                int headerheight = 0;
                int fontsize = Convert.ToInt32((sc.Height * 18) / 768);
                if (fontsize == 0)
                {
                    return null;
                }
                Font MyFont = new Font(FontFamily.GenericSansSerif, fontsize, FontStyle.Regular, GraphicsUnit.World);

                string text = "DATE: " + sc.CreatedOn.ToLocalTime().ToString("o") + " · URL: " + sc.Url;
                Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
                //Si no pasamos parametros queremos todos los datos... si no le quitamos el header
                if (withdata == true)
                {
                    using (Graphics g = Graphics.FromImage(target))
                    {
                        headerheight = Convert.ToInt32(g.MeasureString(text, MyFont).Height) + 5;
                    }
                }
                target = new Bitmap(cropRect.Width, cropRect.Height + headerheight);

                using (Graphics g = Graphics.FromImage(target))
                {

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                    //La Imagen
                    g.DrawImage(bitImage,
                        new Rectangle(0, headerheight, target.Width, target.Height - headerheight),
                        cropRect,
                        GraphicsUnit.Pixel);

                    if (withdata == true)
                    {
                        //El Raton
                        if ((sc.MouseX != 0) && (sc.MouseY != 0))
                        {
                            SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(200, 255, 0, 0));
                            g.FillEllipse(semiTransBrush, sc.MouseX - 10 - sc.ScrollLeft, sc.MouseY - sc.ScrollTop + headerheight - 10, 20, 20);

                        }

                        //Y ahora el texto informativo
                        SolidBrush MyBrush = new SolidBrush(Color.Black);
                        g.DrawString(text, MyFont, MyBrush, 5, 5);

                    }
                }

                if ((height == null) && (width == null))
                {
                    bitImage = target;
                }
                else
                {
                    ThumbCreator thumb = new ThumbCreator();
                    bitImage = thumb.Resize(target, Convert.ToInt16(height), Convert.ToInt16(width), ThumbCreator.VerticalAlign.Top, ThumbCreator.HorizontalAlign.Middle);
                }
            }
            catch (Exception e)
            {
                GrabaLogExcepcion(e);
                return null;
            }
            return bitImage;
        }

        public void CheckIfCaptureProcessMustBeAddedToSc_Control(string UserName, string Domain, string WindowName)
        {
            try
            {
                /*
                 * Miramos (en background) La lista de Procesos de Captura a ver si este converser está ahi o no...
                 * Y en caso de que no esté lo añadimos para que empiecen a sacar "fotos" desde el worker
                 */
                Task.Factory.StartNew(() =>
                {
                    string key = "screenshot_control_list";
                    var item = UserName + "@" + Domain + "@" + WindowName;
                    Dictionary<string, string> sc_control_list = null;

                    //object result = SingletonCache.Instance.Get(key);
                    DataCacheLockHandle lockHandle;
                    object result = SingletonCache.Instance.GetWithLock(key, out lockHandle);

                    if (result != null)
                    {
                        sc_control_list = (Dictionary<string, string>)result;
                    }

                    if (sc_control_list == null)
                    {
                        sc_control_list = new Dictionary<string, string>();
                    }

                    //Si el proceso se hubiera muerto lo hubieramos Quitado del cache monitorizando en cada worker...
                    if (sc_control_list.ContainsKey(item) == false)
                    {
                        sc_control_list.Add(item, null);
                        //SingletonCache.Instance.Insert(key, sc_control_list);
                        SingletonCache.Instance.InsertWithLock(key, sc_control_list, lockHandle);
                        //SingletonCache.Instance.Insert(key, sc_control_list);
                    }
                    else
                    {
                        SingletonCache.Instance.UnLock(key, lockHandle);
                    }
                });
            }
            catch (Exception ex)
            {
                GrabaLog(NivelLog.error, ex.Message);
            }
        }

        public ScreenCapture GetScreenCapture(string UserName, string Domain, string WindowName)
        {
            ScreenCapture sc = null;

            try
            {
                CheckIfCaptureProcessMustBeAddedToSc_Control(UserName, Domain, WindowName);

                /*
                 * Nos traemos la fotico (si la hay)
                 */
                string key = "screenshot_from_" + UserName + "@" + Domain + "@" + WindowName;
                object result = SingletonCache.Instance.Get(key);
                //object result = SingletonCache.Instance.GetWithLock(key, out lockHandle);

                if (result != null)
                {
                    sc = (ScreenCapture)result;
                }

            }
            catch (Exception e)
            {
                GrabaLogExcepcion(e);
            }

            return sc;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        public string ImageToJpegBase64(Image image, long Q)
        {
            string base64String = "";
            try
            {
                if (image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Convert Image to byte[] formato jpeg
                        EncoderParameters eps = new EncoderParameters(1);
                        eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Q);
                        ImageCodecInfo ici = GetEncoderInfo("image/jpeg");
                        image.Save(ms, ici, eps);

                        byte[] imageBytes = ms.ToArray();

                        // Convert byte[] to Base64 String
                        base64String = Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch (Exception)
            {
            }
            return base64String;
        }

        public string FixBase64ForImage(string Image)
        {
            try
            {
                System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
                sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
                return sbText.ToString();
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return "";
            }
        }

        public string SafeSqlLiteral(System.Object theValue, System.Object theLevel)
        {

            // Written by user CWA, CoolWebAwards.com Forums. 2 February 2010
            // http://forum.coolwebawards.com/threads/12-Preventing-SQL-injection-attacks-using-C-NET

            // intLevel represent how thorough the value will be checked for dangerous code
            // intLevel (1) - Do just the basic. This level will already counter most of the SQL injection attacks
            // intLevel (2) -   (non breaking space) will be added to most words used in SQL queries to prevent unauthorized access to the database. Safe to be printed back into HTML code. Don't use for usernames or passwords

            string strValue = (string)theValue;
            int intLevel = (int)theLevel;

            if (strValue != null)
            {
                if (intLevel > 0)
                {
                    strValue = strValue.Replace("'", "''"); // Most important one! This line alone can prevent most injection attacks
                    strValue = strValue.Replace("--", "");
                    strValue = strValue.Replace("[", "[[]");
                    strValue = strValue.Replace("%", "[%]");
                }
                if (intLevel > 1)
                {
                    string[] myArray = new string[] { "xp_ ", "update ", "insert ", "select ", "drop ", "alter ", "create ", "rename ", "delete ", "replace " };
                    int i = 0;
                    int i2 = 0;
                    int intLenghtLeft = 0;
                    for (i = 0; i < myArray.Length; i++)
                    {
                        string strWord = myArray[i];
                        Regex rx = new Regex(strWord, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        MatchCollection matches = rx.Matches(strValue);
                        i2 = 0;
                        foreach (Match match in matches)
                        {
                            GroupCollection groups = match.Groups;
                            intLenghtLeft = groups[0].Index + myArray[i].Length + i2;
                            strValue = strValue.Substring(0, intLenghtLeft - 1) + "&nbsp;" + strValue.Substring(strValue.Length - (strValue.Length - intLenghtLeft), strValue.Length - intLenghtLeft);
                            i2 += 5;
                        }
                    }
                }
                return strValue;
            }
            else
            {
                return strValue;
            }
        }

        public string ScrubHTML(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            //Remove potentially harmful elements
            HtmlNodeCollection nc = doc.DocumentNode.SelectNodes("//script|//iframe|//frameset|//frame|//applet|//object");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.ParentNode.RemoveChild(node, false);

                }
            }

            /*
            //remove hrefs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'javascript')]|//a[starts-with(@href, 'jscript')]|//a[starts-with(@href, 'vbscript')]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.SetAttributeValue("href", "protected");
                }
            }
            */

            //remove hrefs
            nc = doc.DocumentNode.SelectNodes("//a");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    //node.SetAttributeValue("href", "#");
                    node.Attributes.Remove("href");
                }
            }

            nc = doc.DocumentNode.SelectNodes("//form");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    //node.SetAttributeValue("href", "#");
                    node.Attributes.Remove("action");
                }
            }

            nc = doc.DocumentNode.SelectNodes("//input");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    if (node.Attributes["type"].Value.ToString().ToLowerInvariant() == "submit")
                    {
                        //node.SetAttributeValue("href", "#");
                        node.Attributes.Remove("type");
                    }
                }
            }

            //remove img with refs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//img[starts-with(@src, 'javascript')]|//img[starts-with(@src, 'jscript')]|//img[starts-with(@src, 'vbscript')]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.SetAttributeValue("src", "protected");
                }
            }

            //remove on<Event> handlers from all tags
            nc = doc.DocumentNode.SelectNodes("//*[@onclick or @onmouseover or @onfocus or @onblur or @onmouseout or @ondoubleclick or @onload or @onunload]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.Attributes.Remove("onFocus");
                    node.Attributes.Remove("onBlur");
                    node.Attributes.Remove("onClick");
                    node.Attributes.Remove("onMouseOver");
                    node.Attributes.Remove("onMouseOut");
                    node.Attributes.Remove("onDoubleClick");
                    node.Attributes.Remove("onLoad");
                    node.Attributes.Remove("onUnload");
                }
            }

            return doc.DocumentNode.WriteTo();
        }

        public bool TrackScreen(string username, string password, string domain, string data, string listeners, HttpContextBase context)
        {
            try
            {

                Converser converser = GetConverserFromSystem(username, password, domain, db);
                if (converser == null)
                {
                    return false;
                }

                if ((data == null) || (data == ""))
                {
                    return false;
                }

                /*
                 * Hay dos opciones... o enviamos un array de data con sus respectivos difff
                 * O bien enviamos un solo diff sin array
                 */

                List<Dictionary<string, object>> arrDict = null;

                if (converser.Business.CompressHtmlData == true)
                {
                    data = LZString.decompressFromBase64(data);
                }

                if ((data == null) || (data == ""))
                {
                    return false;
                }

                arrDict = new JavaScriptSerializer().Deserialize<List<Dictionary<string, object>>>(data);
                if (arrDict.Count == 0)
                {
                    Dictionary<string, object> dict = null;
                    dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(data);
                    if (dict != null)
                    {
                        arrDict.Add(dict);
                    }
                }

                if (listeners != null)
                {
                    if ((listeners == "null") || (listeners.IndexOf("@") < 0))
                    {
                        listeners = null;
                    }
                }
                if (arrDict == null)
                {
                    GrabaLog(Utils.NivelLog.error, "Error serializing TrackScreen: " + data);
                    return false;
                }

                foreach (Dictionary<string, object> dict in arrDict)
                {
                    try
                    {
                        string sIP = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (string.IsNullOrEmpty(sIP) == true) { sIP = context.Request.ServerVariables["REMOTE_ADDR"]; }
                        else { sIP = sIP.Split(',')[0]; }
                        string[] languages = context.Request.UserLanguages;
                        string useragent = context.Request.UserAgent;

                        // If you want it formated in some other way.
                        var headers = "{";
                        foreach (var key in context.Request.Headers.AllKeys)
                            headers += "'" + key + "':'" + context.Request.Headers[key] + "',";
                        headers = headers.TrimEnd(',') + "}";

                        ScreenCapture new_screencapture = new ScreenCapture();

                        new_screencapture.converser = converser;
                        new_screencapture.Headers = headers;

                        if (dict.ContainsKey("st"))
                        {
                            if (dict["st"] != null)
                            {
                                if (dict["st"].ToString() != "null")
                                {
                                    new_screencapture.ScrollTop = Convert.ToInt16(dict["st"].ToString());
                                }
                            }
                        }
                        if (dict.ContainsKey("sl"))
                        {
                            if (dict["sl"] != null)
                            {
                                if (dict["sl"].ToString() != "null")
                                {
                                    new_screencapture.ScrollLeft = Convert.ToInt16(dict["sl"].ToString());
                                }
                            }
                        }
                        if (dict.ContainsKey("mx"))
                        {
                            if (dict["mx"] != null)
                            {
                                if (dict["mx"].ToString() != "null")
                                {
                                    string strmouseX = "0";
                                    if (dict["mx"].ToString().IndexOf('.') > -1)
                                    {
                                        strmouseX = dict["mx"].ToString().Split('.')[0];
                                    }
                                    else if (dict["mx"].ToString().IndexOf(',') > -1)
                                    {
                                        strmouseX = dict["mx"].ToString().Split(',')[0];
                                    }
                                    else
                                    {
                                        strmouseX = dict["mx"].ToString();
                                    }
                                    new_screencapture.MouseX = Convert.ToInt16(strmouseX);
                                }
                            }
                        }
                        if (dict.ContainsKey("my"))
                        {
                            if (dict["my"] != null)
                            {
                                if (dict["my"].ToString() != "null")
                                {
                                    string strmouseY = "0";
                                    if (dict["my"].ToString().IndexOf('.') > -1)
                                    {
                                        strmouseY = dict["my"].ToString().Split('.')[0];
                                    }
                                    else if (dict["my"].ToString().IndexOf(',') > -1)
                                    {
                                        strmouseY = dict["my"].ToString().Split(',')[0];
                                    }
                                    else
                                    {
                                        strmouseY = dict["my"].ToString();
                                    }
                                    new_screencapture.MouseY = Convert.ToInt16(strmouseY);
                                }
                            }
                        }
                        if (dict.ContainsKey("w"))
                        {
                            if (dict["w"] != null)
                            {
                                if (dict["w"].ToString() != "null")
                                {
                                    new_screencapture.Width = Convert.ToInt16(dict["w"].ToString().Split('.')[0]);
                                }
                            }
                        }
                        if (dict.ContainsKey("h"))
                        {
                            if (dict["h"] != null)
                            {
                                if (dict["h"].ToString() != "null")
                                {
                                    new_screencapture.Height = Convert.ToInt16(dict["h"].ToString());
                                }
                            }
                        }
                        if (dict.ContainsKey("date"))
                        {
                            if (dict["date"] != null)
                            {
                                if (dict["date"].ToString() != "null")
                                {
                                    //Se guarda como UTC
                                    new_screencapture.CreatedOn = DateTime.Parse(dict["date"].ToString());
                                }
                            }
                        }
                        if (dict.ContainsKey("url"))
                        {
                            if (dict["url"] != null)
                            {
                                if (dict["url"].ToString() != "null")
                                {
                                    new_screencapture.Url = dict["url"].ToString();
                                }
                            }
                        }

                        /*
                         * hacemos lo que sea segun es una imagen (blob o img) o solo la posicion, o solo el mouse etc
                         * 1: Si es una imagen lo mandamos a la cola de proceso, donde ademas sustituimos el capturelastcomplete_usernamedomain de la cache
                         * 2: Si es un pos o un mut tomamos el capturelastcomplete_usernamedomain de la cache, lo mutamos o movemos y lo sustituimos
                         *    y entonces lo mandamos a la cola de proceso
                         * 3: Si es un raton solo enviamos el punto al que se movió  como mensaje por caché, y lo guardamos en DB para luego hacer los videos :)
                         */

                        if (dict.ContainsKey("img"))
                        {
                            if (dict["img"] != null)
                            {
                                if (dict["img"].ToString() != "null")
                                {
                                    new_screencapture.Data = dict["img"].ToString();
                                }
                            }
                        }

                        List<String> listenersArr = new List<String>();
                        if (listeners != null)
                        {
                            if ((listeners != "") && (listeners != "null"))
                            {
                                listenersArr = listeners.Split(',').ToList<String>();
                            }
                        }
                        if (listenersArr.Count == 0)
                        {
                            foreach (Agent agent in GetActiveAgents(converser))
                            {
                                listenersArr.Add(agent.Converser.UserName + "@" + agent.Converser.Business.Domain);
                            }
                        }
                        foreach (string listenerUD in listenersArr)
                        {
                            var newmessage = new NewMessage();
                            newmessage.From = username + @"@" + domain;
                            newmessage.To = listenerUD;
                            newmessage.Subject = "$#_mousemove";
                            newmessage.Content = new_screencapture.Width + "," + new_screencapture.Height + "@" + (new_screencapture.MouseX - new_screencapture.ScrollLeft) + "," + (new_screencapture.MouseY - new_screencapture.ScrollTop);
                            newmessage.MessageType = "chat";
                            var message = new Message(newmessage);
                            message.AddToCache();
                        }

                        if (dict.ContainsKey("windowname"))
                        {
                            if ((dict["windowname"] != null) && (dict["windowname"].ToString() != "null"))
                            {
                                new_screencapture.WindowName = dict["windowname"].ToString();
                            }
                        }

                        if (dict.ContainsKey("blob"))
                        {
                            if ((dict["blob"] != null) && (dict["blob"].ToString() != "null"))
                            {

                                string SerializedBlob = new JavaScriptSerializer().Serialize(dict["blob"]);
                                new_screencapture.Blob = SerializedBlob;

                                bool sent = AddScreenCaptureToProcessQueue(new_screencapture);

                                continue;
                            }
                        }

                        if (new_screencapture.Data == null)
                        {
                            new_screencapture.Data = "";
                        }
                        SaveScreenCapture(username, password, domain, new_screencapture, listeners);

                    }
                    catch (Exception _ex)
                    {
                        GrabaLogExcepcion(_ex);
                    }

                }

                return true;

            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return false;
            }
        }

        private bool AddScreenCaptureToProcessQueue(ScreenCapture new_screencapture)
        {
            DataCacheLockHandle lockHandle = null;
            string key = "";
            try
            {
                /*
                 * Vamos a preparar el control para que siempre haya una SC que generar con los ultimos cambios
                 * Y aplicaremos esos cambios para que siempre haya un LastHtml
                 * TODO meter mutations y POS... por ahora va a cargar siempre todo el HTML entero
                 */

                var UserName = new_screencapture.converser.UserName;
                var Domain = new_screencapture.converser.Business.Domain;
                var Password = new_screencapture.converser.Password;
                var WindowName = new_screencapture.WindowName;

                key = "screenshot_control_from_" + UserName + "@" + Domain + "@" + WindowName;

                object result = SingletonCache.Instance.GetWithLock(key, out lockHandle);
                //object result = SingletonCache.Instance.Get(key);

                ScreenCaptureControl sc_control = null;
                if (result != null)
                {
                    sc_control = (ScreenCaptureControl)result;
                }
                if (sc_control == null)
                {
                    sc_control = new ScreenCaptureControl();
                    sc_control.UserName = UserName;
                    sc_control.Domain = Domain;
                    sc_control.Password = Password;
                }

                ArrayList ArrBlob = new ArrayList();
                //ArrBlob = new JavaScriptSerializer().Deserialize<ArrayList>(new_screencapture.Blob);
                ArrBlob = JsonConvert.DeserializeObject<ArrayList>(new_screencapture.Blob);

                //Casos especiales... o la primera vez que se carga o no ha cambiado en total...
                if (ArrBlob.Count == 1)
                {
                    var elem = (Newtonsoft.Json.Linq.JArray)ArrBlob[0];
                    if (elem[0].ToString() == "1")
                    {
                        //Si es la primera vez que carga la página... es que siempre es nueva
                        //así que lo ponemos todo a cero
                        sc_control.CompleteHtml = "";
                    }
                    else if (elem[0].ToString() == "0")
                    {
                        if (sc_control.ScreenCapture != null)
                        {
                            //si no ha cambiado naaaada no renderizamos
                            if ((new_screencapture.Height == sc_control.ScreenCapture.Height) &&
                                (new_screencapture.Width == sc_control.ScreenCapture.Width) &&
                                (new_screencapture.ScrollLeft == sc_control.ScreenCapture.ScrollLeft) &&
                                (new_screencapture.ScrollTop == sc_control.ScreenCapture.ScrollTop))
                            {
                                // && (new_screencapture.Blob != sc_control.ScreenCapture.Blob)
                                SingletonCache.Instance.UnLock(key, lockHandle);
                                return true;
                            }
                        }
                    }
                }

                string processedHtml = "";
                foreach (Newtonsoft.Json.Linq.JArray elem in ArrBlob)
                {
                    var type = elem[0].ToString();
                    var contents = elem[1].ToString();
                    if (type == "1")
                    {
                        processedHtml += contents;
                    }
                    else if (type == "-1")
                    {
                        if (sc_control.CompleteHtml != null)
                        {
                            if (Convert.ToInt32(contents) > sc_control.CompleteHtml.Length)
                            {
                                contents = sc_control.CompleteHtml.Length.ToString();
                            }
                            sc_control.CompleteHtml = sc_control.CompleteHtml.Remove(0, Convert.ToInt32(contents));
                        }
                    }
                    else if (type == "0")
                    {
                        if (sc_control.CompleteHtml != null)
                        {
                            if (Convert.ToInt32(contents) > sc_control.CompleteHtml.Length)
                            {
                                return false;
                            }
                            processedHtml += sc_control.CompleteHtml.Substring(0, Convert.ToInt32(contents));
                            sc_control.CompleteHtml = sc_control.CompleteHtml.Remove(0, Convert.ToInt32(contents));
                        }
                    }
                }
                sc_control.CompleteHtml = processedHtml;

                new_screencapture.converser = null;
                sc_control.ScreenCapture = new_screencapture;

                SingletonCache.Instance.InsertWithLock(key, sc_control, lockHandle);
                //SingletonCache.Instance.Insert(key, sc_control);

                //Y no tenemos claro si poner a funcionar el worker... a ver si no se resiente mucho la CPU
                /*
                GetScreenCapture(UserName, Domain);
#if DEBUG
#else
#endif
                */
                /* 
                 * Lanzamos el Save en otro hilo... pero antes le mentemos el HTML que toca ya procesado.. 
                 * por aquello de generar luego los videos en otro proceso mañana o pasado ;-)
                 */

                new_screencapture.Blob = sc_control.CompleteHtml;

                string SaveScreenshotsInDbSetting = "SaveScreenshotsInDbInRelease";
#if DEBUG
                SaveScreenshotsInDbSetting = "SaveScreenshotsInDbInDebug";
#endif
                bool SaveScreenshotsInDb = false;
                SaveScreenshotsInDb = Convert.ToBoolean((from m in db.Settings
                                                         where m.Name == SaveScreenshotsInDbSetting
                                                         select m).FirstOrDefault().Value);
                if (SaveScreenshotsInDb == true)
                {
                    Task TaskLog = Task.Factory.StartNew(() =>
                    {
                        AddScreenCapturetoDb(UserName, Password, Domain, new_screencapture);
                    });
                }

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ErrorCode<ERRCA0012>:SubStatus<ES0001>:") == false)
                {
                    GrabaLogExcepcion(ex);
                }
                if (lockHandle != null)
                {
                    SingletonCache.Instance.UnLock(key, lockHandle);
                }
            }

            return true;
        }

        public void AddScreenCapturetoDb(string UserName, string Password, string Domain, ScreenCapture new_screencapture)
        {
            try
            {

                //Si ya existe la actualizamos
                ScreenCapture sc = null;
                sc = (from m in db.ScreenCaptures
                      where m.GUID == new_screencapture.GUID
                      select m).FirstOrDefault();

                if (sc == null)
                {
                    Converser converser = GetConverserFromSystem(UserName, Password, Domain, db);
                    if (converser == null)
                    {
                        return;
                    }
                    else
                    {
                        new_screencapture.converser = converser;
                    }
                    db.ScreenCaptures.Add(new_screencapture);
                }
                else
                {
                    if (sc.converser == null)
                    {
                        Converser converser = GetConverserFromSystem(UserName, Password, Domain, db);
                        if (converser == null)
                        {
                            return;
                        }
                        else
                        {
                            sc.converser = converser;
                        }
                    }

                    sc.GUID = new_screencapture.GUID;
                    sc.Headers = new_screencapture.Headers;
                    sc.Height = new_screencapture.Height;
                    sc.MouseX = new_screencapture.MouseX;
                    sc.MouseY = new_screencapture.MouseY;
                    sc.ScrollLeft = new_screencapture.ScrollLeft;
                    sc.ScrollTop = new_screencapture.ScrollTop;
                    sc.Url = new_screencapture.Url;
                    sc.Width = new_screencapture.Width;
                    sc.Blob = new_screencapture.Blob;
                    sc.Data = new_screencapture.Data;
                    sc.WindowName = new_screencapture.WindowName;
                }
                db.SaveChanges();

                //string strSQL = "DELETE FROM SCREENCAPTURES WHERE createdon < DATEADD(minute,-10,CURRENT_TIMESTAMP);";

                //Tendremos screeenshots de hasta hace dos días...
                //string strSQL = "DELETE FROM SCREENCAPTURES WHERE createdon < DATEADD(day,-7,CURRENT_TIMESTAMP);";
                //db.Database.ExecuteSqlCommand(strSQL);

            }
            catch (Exception _ex)
            {
                GrabaLogExcepcion(_ex);
            }
        }

        public string unescape(string processedHtml)
        {
            try
            {
                processedHtml = processedHtml.Replace(@"&amp;", @"&");
                processedHtml = processedHtml.Replace(@"&lt;", @"<");
                processedHtml = processedHtml.Replace(@"&gt;", @">");
                processedHtml = processedHtml.Replace(@"&quot;", "\"");
                return processedHtml;
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public Boolean SaveScreenCapture(string username, string password, string domain, ScreenCapture new_screencapture, string listeners)
        {
            try
            {
                if (new_screencapture == null)
                {
                    return false;
                }

                Converser converser = GetConverserFromSystem(username, password, domain);
                if (converser == null)
                {
                    return false;
                }

                new_screencapture.converser = converser;
                new_screencapture.AddToCache();

                return true;

            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return false;
            }
        }

        public List<Agent> GetActiveAgents(Converser converser)
        {
            try
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = DateTime.Now.AddSeconds(-20);
                DateTime loctimeUTC = localZone.ToUniversalTime(loctime);
                var agents = (from m in db.Agents
                              where m.Converser.Business.ID == converser.Business.ID
                              where m.Converser.Active == true
                              && m.Converser.LastActive > loctimeUTC
                              select m).ToList<Agent>();
                return agents;
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public Status TrackPageView(string trackID, Converser converser, string url, string referrer, string language, string useragent, string sIP, string headers, string windowname)
        {
            /*
            if (db == null)
            {
                db = new vizzopContext();
            }
            */
            /*
            Task.Factory.StartNew(() =>
            {
                LimpiaWebLocations();
            });
            */
            List<WebLocation> WebLocations = new List<WebLocation>();

            string tag = "weblocation";
            List<DataCacheTag> Tags = new List<DataCacheTag>();
            Tags.Add(new DataCacheTag(tag));
            object result = SingletonCache.Instance.GetByTag(tag);
            if (result != null)
            {
                IEnumerable<KeyValuePair<string, object>> ObjectList = (IEnumerable<KeyValuePair<string, object>>)result;

                foreach (var e in ObjectList)
                {
                    WebLocations.Add((WebLocation)e.Value);
                }

            }
            /*
            DataCacheLockHandle lockHandle;
            object result = SingletonCache.Instance.GetWithLock(key, out lockHandle);
            if (result != null)
            {
                WebLocations = (List<WebLocation>)result;
            }
            */

            try
            {
                if (url.Length > 3999)
                {
                    url = url.Substring(0, 3999);
                }
                if (referrer == null)
                {
                    referrer = "";
                }
                if (referrer.Length > 3999)
                {
                    referrer = referrer.Substring(0, 3999);
                }
                if (useragent.Length > 3999)
                {
                    useragent = useragent.Substring(0, 3999);
                }
                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = localZone.ToUniversalTime(DateTime.Now);
                if ((url != null) && (converser != null))
                {
                    /*
                    Utils utils = new Utils();
                    */
                    /*
                    var pageview = (from m in DBContext.WebLocations.Include("Converser").Include("Converser.Business")
                                    where m.Converser.ID == converser.ID &&
                                    m.Url == url
                                    select m).FirstOrDefault();
                     */
                    WebLocation weblocation = null;

                    if ((trackID != null) && (trackID != "") && (trackID != "null"))
                    {
                        int _trackID = Convert.ToInt32(trackID);
                        if (_trackID != 0)
                        {
                            weblocation = (from m in WebLocations
                                           where m.ID == _trackID &&
                                           m.Url == url
                                           select m).FirstOrDefault();
                        }
                    }
                    /*
                     * Si la weblocation Existe pero no ha sido 
                     * 
                     */
                    if (weblocation != null)
                    {
                        weblocation.TimeStamp_Last = loctime;
                        //db.SaveChanges();
                    }
                    else
                    {

                        string slang = "-";
                        if (language != null) { slang = language; }

                        weblocation = new WebLocation();
                        weblocation.ID = RandNumber(0, 9999999);
                        weblocation.ConverserId = converser.ID;
                        weblocation.UserName = converser.UserName;
                        weblocation.Password = converser.Password;
                        weblocation.FullName = converser.FullName;
                        weblocation.Domain = converser.Business.Domain;
                        weblocation.Referrer = referrer;
                        weblocation.TimeStamp_First = loctime;
                        weblocation.TimeStamp_Last = loctime.AddMilliseconds(1000);
                        weblocation.UserAgent = useragent;
                        weblocation.IP = sIP;
                        weblocation.Lang = slang;
                        weblocation.Url = url;
                        weblocation.Ubication = GetUbicationFromIP(sIP);
                        weblocation.Headers = headers;
                        weblocation.WindowName = windowname;
                        //WebLocations.Add(weblocation);
                        //db.SaveChanges();

                    }
                    //return weblocation.ID;
                    //SingletonCache.Instance.InsertWithLock(key, WebLocations, lockHandle);
                    string key = tag + "_" + converser.UserName + @"@" + converser.Business.Domain + @"@" + windowname;
                    SingletonCache.Instance.InsertWithTags(key, weblocation, Tags);
                    return new Status(true, weblocation.ID);
                }
                else
                {
                    //SingletonCache.Instance.UnLock(key, lockHandle);
                    return new Status(false, null);
                    //return null;
                }
            }
            catch (Exception e)
            {
                //SingletonCache.Instance.UnLock(key, lockHandle);
                GrabaLogExcepcion(e);
                GrabaLog(Utils.NivelLog.error, converser.UserName + @"_" + converser.Password + @"_" + converser.Business.Domain + @"_" + url + @"_" + referrer + @"_" + language + @"_" + useragent + @"_" + sIP);
                //return null;
                return new Status(false, null);
            }
        }

        public String GetUbicationFromIP(string sIP)
        {
            string ubication = "unknown";
            try
            {
                if (sIP == null)
                {
                    GrabaLog(NivelLog.error, "No IP to get Ubication from");
                }
                else
                {
                    if (sIP == "127.0.0.1")
                    {
                        ubication = "localhost";
                    }
                    else
                    {
                        //string filePath = AppDomain.CurrentDomain.BaseDirectory + "Content\\GeoLiteCity.dat";
                        string filePath = System.Web.HttpContext.Current.Request.MapPath("/Content/GeoLiteCity.dat");
                        LookupService ls = new LookupService(filePath, LookupService.GEOIP_MEMORY_CACHE);
                        Location l = ls.getLocation(sIP);
                        if (l != null)
                        {
                            ubication = l.countryName + ", " + l.city;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
            }
            return ubication;
        }

        public String GetPrettyTimespan(int ms)
        {
            try
            {
                int seconds = (int)((ms / 1000) % 60);
                int minutes = (int)(((ms / 1000) / 60) % 60);
                int hours = (int)((((ms / 1000) / 60) / 60) % 24);

                String sec, min, hrs;
                if (seconds < 10) sec = "0" + seconds;
                else sec = "" + seconds;
                if (minutes < 10) min = "0" + minutes;
                else min = "" + minutes;
                if (hours < 10) hrs = "0" + hours;
                else hrs = "" + hours;

                return hrs + ":" + min + ":" + sec;
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public string GetPrettyDate(DateTime d)
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime loctime = localZone.ToUniversalTime(DateTime.Now);
            //d = localZone.ToUniversalTime(d);
            // 1.
            // Get time span elapsed since the date.
            TimeSpan s = loctime.Subtract(d);

            // 2.
            // Get total number of days elapsed.
            int dayDiff = (int)s.TotalDays;

            // 3.
            // Get total number of seconds elapsed.
            int secDiff = (int)s.TotalSeconds;

            // 4.
            // Don't allow out of range values.
            if (dayDiff < 0 || dayDiff >= 31)
            {
                return d.ToShortDateString() + " " + d.ToShortTimeString();
            }

            // 5.
            // Handle same-day times.
            if (dayDiff == 0)
            {
                // AAA.
                // 0 Secs
                if (secDiff < 2)
                {
                    return "just now";
                }
                // A.
                // LES THAN 20 SECS AGO;
                if (secDiff < 60)
                {
                    return secDiff.ToString() + " seconds ago";
                }
                // B.
                // Less than 2 minutes ago.
                if (secDiff < 120)
                {
                    return "1 minute ago";
                }
                // C.
                // Less than one hour ago.
                if (secDiff < 3600)
                {
                    return string.Format("{0} minutes ago",
                        Math.Floor((double)secDiff / 60));
                }
                // D.
                // Less than 2 hours ago.
                if (secDiff < 7200)
                {
                    return "1 hour ago";
                }
                // E.
                // Less than one day ago.
                if (secDiff < 86400)
                {
                    return string.Format("{0} hours ago",
                        Math.Floor((double)secDiff / 3600));
                }
            }
            // 6.
            // Handle previous days.
            if (dayDiff == 1)
            {
                return "yesterday";
            }
            if (dayDiff < 7)
            {
                return string.Format("{0} days ago",
                dayDiff);
            }
            if (dayDiff < 31)
            {
                return string.Format("{0} weeks ago",
                Math.Ceiling((double)dayDiff / 7));
            }
            return d.ToString();
        }

        public string SacaParamsContext(HttpContextBase Httpcontext)
        {
            try
            {

                StringBuilder sb = new StringBuilder();

                /*
                System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                

                sb.AppendLine(oSerializer.Serialize(Httpcontext.Request.QueryString));
                sb.AppendLine(oSerializer.Serialize(Httpcontext.Request.Form));
                */
                for (int i = 0; i < Httpcontext.Request.QueryString.Count; i++)
                {
                    sb.AppendFormat("{0} : {1}" + Environment.NewLine, Httpcontext.Request.QueryString.Keys[i], Httpcontext.Request.QueryString[i]);
                }
                for (int i = 0; i < Httpcontext.Request.Form.Count; i++)
                {
                    sb.AppendFormat("{0} : {1}" + Environment.NewLine, Httpcontext.Request.Form.Keys[i], Httpcontext.Request.Form[i]);
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public static string GetMD5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        public string LeeArchivo(string sNombreCompleto)
        {
            try
            {
                //Objetivo; Leer un archivo completo y traerlo al frente. 
                string sLeeArchivo = null;
                //Crea un archivo con el nombre especificado en el path.. Con el contenido asignado
                //Dim oArchivo As File
                string strNombre = sNombreCompleto;
                //Se puede producir un error si quieren grabar simultáneamente en el fichero
                if (File.Exists(strNombre))
                {
                    StreamReader sr = File.OpenText(sNombreCompleto);
                    sLeeArchivo = sr.ReadToEnd();
                }

                return sLeeArchivo;
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }

        public enum NivelLog
        {
            info = 0,
            error = 1
        }

        public string LocalizeLang(string Ref, string LangISO, string[] Args)
        {
            vizzopContext db = new vizzopContext();
            string returnValue = "";
            if (LangISO.Split('-').Length > 0)
            {
                LangISO = LangISO.Split('-')[0];
            }
            try
            {
                var text = (from m in db.TextStrings
                            where m.Ref == Ref
                            && m.IsoCode == LangISO
                            select m).FirstOrDefault();

                if (text == null)
                {
                    text = (from m in db.TextStrings
                            where m.Ref == Ref
                            && m.IsoCode == "en"
                            select m).FirstOrDefault();
                }
                returnValue = text.Text;
                if (Args != null)
                {
                    for (int i = 0; i < Args.Length; i++)
                    {
                        returnValue = returnValue.Replace("%" + i, Args[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
            }
            return returnValue;
        }

        public void GrabaLogExcepcion(Exception ex)
        {
            string msg = ex.Message.ToString();
            if (ex.StackTrace != null)
            {
                msg += System.Environment.NewLine + ex.StackTrace;
            }
            if (ex.InnerException != null)
            {
                msg += System.Environment.NewLine + ex.InnerException.Message;
                if (ex.InnerException.InnerException != null)
                {
                    msg += System.Environment.NewLine + ex.InnerException.InnerException.Message;
                }
            }
            GrabaLog(Utils.NivelLog.error, msg);
        }

        public void GrabaLog(NivelLog NLog, string strLog)
        {
            try
            {
                Task TaskLog = Task.Factory.StartNew(() =>
                {
                    db = new vizzopContext();

                    string CreateLogsSetting = "CreateLogsInRelease";
#if DEBUG
                    CreateLogsSetting = "CreateLogsInDebug";
#endif
                    bool CreateLogs = false;
                    CreateLogs = Convert.ToBoolean((from m in db.Settings
                                                    where m.Name == CreateLogsSetting
                                                    select m).FirstOrDefault().Value);
                    if (CreateLogs == false)
                    {
                        return;
                    }

                    //Asi pillamos mas info de donde estamos realmente :)
                    System.Diagnostics.StackFrame Frame = new System.Diagnostics.StackFrame(1, false);
                    System.Reflection.MethodBase Method;
                    Method = Frame.GetMethod();

                    String strLog_WithRoute = Method.DeclaringType.FullName + "/" + Method.Name + "/" + strLog;

                    switch (NLog)
                    {
                        case NivelLog.info:
                            //GrabaAnalyticsLog(NLog, strLog_WithRoute);
                            //Lo importante va a Syslog y a BD para posterior búsqueda-seguimiento
                            GrabaDBLog(NLog, strLog_WithRoute);
                            break;
                        case NivelLog.error:
                            //Los errores van a Syslog y a BD para posterior búsqueda-seguimiento
                            GrabaDBLog(NLog, strLog_WithRoute);
                            GrabaAnalyticsLog(NLog, strLog_WithRoute);
                            break;
                    }
                    //Tó va a Syslog, y en caso de que no sea posible, Syslog se encarga de mandarlo a un TXT
                    GrabaSYSLog(NLog, strLog_WithRoute);
                });
            }
            catch (Exception)
            {

            }
        }

        public void GrabaLogJavascript(string strLog)
        {
            try
            {
                Task TaskLog = Task.Factory.StartNew(() =>
                {
                    db = new vizzopContext();

                    string CreateLogsSetting = "CreateLogsInRelease";
#if DEBUG
                    CreateLogsSetting = "CreateLogsInDebug";
#endif
                    bool CreateLogs = false;
                    CreateLogs = Convert.ToBoolean((from m in db.Settings
                                                    where m.Name == CreateLogsSetting
                                                    select m).FirstOrDefault().Value);
                    if (CreateLogs == false)
                    {
                        return;
                    }

                    String strLog_WithRoute = "javascript/" + strLog;
                    NivelLog NLog = NivelLog.error;

                    switch (NLog)
                    {
                        case NivelLog.info:
                            GrabaAnalyticsLog(NLog, strLog_WithRoute);
                            //Lo importante va a Syslog y a BD para posterior búsqueda-seguimiento
                            GrabaDBLog(NLog, strLog_WithRoute);
                            break;
                        case NivelLog.error:
                            GrabaAnalyticsLog(NLog, strLog_WithRoute);
                            //Los errores van a Syslog y a BD para posterior búsqueda-seguimiento
                            GrabaDBLog(NLog, strLog_WithRoute);
                            break;
                    }
                    //Tó va a Syslog, y en caso de que no sea posible, Syslog se encarga de mandarlo a un TXT
                    GrabaSYSLog(NLog, strLog_WithRoute);
                });
            }
            catch (Exception)
            {

            }
        }

        public void AddZenSession(Converser Converser, string SessionID, vizzopContext _db)
        {
            if (_db == null)
            {
                _db = db;
            }

            try
            {
                var newconverser = (from m in _db.Conversers
                                    where m.ID == Converser.ID
                                    select m).FirstOrDefault();
                if (newconverser != null)
                {
                    var newLoginAction = new LoginAction();
                    newLoginAction.Converser = newconverser;
                    _db.LoginActions.Add(newLoginAction);
                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                GrabaLogExcepcion(e);
            }
            try
            {
                var ses = (from m in _db.ZenSessions
                           where m.Converser.ID == Converser.ID
                           select m).FirstOrDefault();
                if (ses != null)
                {
                    ses.sessionID = SessionID;
                    ses.TimeStamp = DateTime.Now;
                    _db.SaveChanges();
                }
                else
                {
                    var newconverser = (from m in _db.Conversers
                                        where m.ID == Converser.ID
                                        select m).FirstOrDefault();
                    if (newconverser != null)
                    {
                        ZenSession newses = new ZenSession();
                        newses.sessionID = SessionID;
                        newses.Converser = newconverser;
                        newses.TimeStamp = DateTime.Now;
                        _db.ZenSessions.Add(newses);
                        _db.SaveChanges();
                    }
                }
            }
            catch (Exception _ex)
            {
                GrabaLogExcepcion(_ex);
            }
        }

        public static void GrabaAnalyticsLog(NivelLog NLog, string message)
        {
            try
            {
                string logtype = "error";
                switch (NLog)
                {
                    case NivelLog.info:
                        logtype = "info";
                        Trace.TraceInformation(message);
                        break;
                }
                message = logtype + "/" + message;
                GoogleAnalyticsDotNet.GoogleAnalyticsDotNet.SendTrackingRequest(google_analytics_key, message, null);
            }
            catch (Exception)
            {
                //Console.Write(ex.Message);
            }
        }

        private void GrabaSYSLog(NivelLog NLog, string strLog)
        {
            try
            {
                if (strLog == null)
                {
                    return;
                }
                string sToLog = System.DateTime.Now.ToString() + "[" + strLog + "]";
                switch (NLog)
                {
                    case NivelLog.error:
                        Trace.TraceError(sToLog);
                        break;
                    case NivelLog.info:
                        Trace.TraceInformation(sToLog);
                        break;
                }
#if DEBUG
                Debug.Write(strLog);
#endif
            }
            catch (Exception)
            {

                //String message = ex.Message;
            }
        }

        private void GrabaDBLog(NivelLog NLog, string strLog)
        {
            try
            {
                vizzopContext db = new vizzopContext();

                Log log = new Log();
                log.Text = strLog;
                db.Logs.Add(log);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GrabaSYSLog(Utils.NivelLog.error, ex.Message);
            }
            //throw new Exception("The method or operation is not implemented.");
        }

        public int RandNumber(int Low, int High)
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));

            int rnd = rndNum.Next(Low, High);

            return rnd;
        }

        /*
        //Function to get random number
        private static readonly Random getrandom = new Random();
        private static readonly object syncLock = new object();
        public static int RandNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return getrandom.Next(min, max);
            }
        }
         * */

        public string GetUnixTimeStamp(DateTime target)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixTimestamp = System.Convert.ToInt64((target - dtDateTime).TotalSeconds);

            return unixTimestamp.ToString();
        }

        public DateTime GetTimeStampfromUnix(string unixTimeStamp)
        {
            double dtimestamp = Convert.ToDouble(unixTimeStamp);
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(dtimestamp).ToLocalTime();
            return dtDateTime;
        }

        internal string GetPrettyDurationRange(int duration, int[] DurationRanges)
        {
            try
            {
                int Duration_min = (from j in DurationRanges
                                    where j <= duration
                                    select j).LastOrDefault();
                int Duration_max = (from j in DurationRanges
                                    where j > duration
                                    select j).FirstOrDefault();
                if (Duration_min == 0)
                {
                    //return "less than " + GetPrettyTimespan(Duration_max);
                    return "less than a second";
                }
                else if (Duration_max == 0)
                {
                    return "more than " + GetPrettyTimespan(Duration_min);
                }
                return GetPrettyTimespan(Duration_min) + " to " + GetPrettyTimespan(Duration_max);
            }
            catch (Exception ex)
            {
                GrabaLog(Utils.NivelLog.error, ex.Message);
                return null;
            }
        }

        internal bool SendMessage(NewMessage newmessage, string SetTicketState)
        {
            try
            {

                Message message = new Message(newmessage);
                message.CommSession = new CommSession();
                message.CommSession.ID = Convert.ToInt32(newmessage.commsessionid);


                if ((message.Content == null) && (message.Subject == null))
                {
                    return false;
                }

                if ((SetTicketState != null) && (SetTicketState != "null") && (SetTicketState != ""))
                {
                    newmessage.MessageType = "ticket";
                    Thread_SendMsg oThread = new Thread_SendMsg(newmessage, SetTicketState, null);
                    oThread.DoThings();
                    return true;
                }
                else
                {
                    if (message.AddToCache() == true)
                    {
                        // Lanzamos el msg en otro hilo...
                        Thread_SendMsg oThread = new Thread_SendMsg(newmessage, SetTicketState, null);
                        Thread rSend = new Thread(oThread.DoThings);
                        rSend.Priority = ThreadPriority.BelowNormal;
                        rSend.Start();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                GrabaLog(Utils.NivelLog.error, ex.Message);
                return false;
            }
        }

        public void LaunchScreenShotsFileControl()
        {

            //GrabaLog(Utils.NivelLog.info, "Iniciando LaunchScreenShotsFileControl");
            while (true)
            {
                try
                {
                    var pathtosearch = AppDomain.CurrentDomain.BaseDirectory + @"\img\captures";
                    foreach (var file in Directory.GetFiles(pathtosearch))
                    {
                        if (file.EndsWith("png") == false)
                        {
                            File.Delete(file);
                            continue;
                        }
                        string strBase64 = null;
                        using (var stream = File.OpenRead(file))
                        using (var image = Image.FromStream(stream))
                        {
                            strBase64 = ImageToJpegBase64(image, 100L);
                        }

                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }

                        if (strBase64 != null)
                        {

                            string FileName = file.Split('\\')[file.Split('\\').Length - 1];
                            string UserName = FileName.Split('_')[0];
                            string Domain = FileName.Split('_')[1];
                            string WindowName = FileName.Split('_')[2];
                            string key = "screenshot_control_from_" + UserName + "@" + Domain + "@" + WindowName;
                            ScreenCaptureControl sc_control = null;
                            //object result = SingletonCache.Instance.Get(key);
                            DataCacheLockHandle lockHandle;
                            object result = SingletonCache.Instance.GetWithLock(key, out lockHandle);
                            if (result != null)
                            {
                                sc_control = (ScreenCaptureControl)result;
                            }

                            if (sc_control == null)
                            {
                                //GrabaLog(Utils.NivelLog.error, "sc_control is null: " + UserName + '@' + Domain + '@' + WindowName);
                                continue;
                            }

                            ScreenCapture sc = sc_control.ScreenCapture;

                            if (sc == null)
                            {
                                GrabaLog(Utils.NivelLog.error, "sc is null: ");
                                continue;
                            }

                            //sc.GUID = Guid.NewGuid().ToString();
                            /*
                            if (sc.GUID == Guid)
                            {
                                sc.Data = strBase64;
                                sc.Blob = sc_control.CompleteHtml;
                            }
                            */

                            /*
                            #if DEBUG
                                                        GrabaLog(Utils.NivelLog.info, DateTime.Now.ToLongTimeString() + " Vamos a guardar la imagen");
                            #endif
                            */

                            //SingletonCache.Instance.Insert(key, sc_control);
                            SingletonCache.Instance.InsertWithLock(key, sc_control, lockHandle);

                            sc.Data = strBase64;
                            sc.Blob = sc_control.CompleteHtml;
                            sc.GUID = Guid.NewGuid().ToString();
                            SaveScreenCapture(sc_control.UserName, sc_control.Password, sc_control.Domain, sc, null);

                        }
                    }
                }
                catch (Exception)
                {
                    //utils.GrabaLog(Utils.NivelLog.error, ex.Message);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(5));
            }
        }

        public void CheckIfStartedCaptureProcessesAreStillRunning()
        {
            try
            {
                string instanceId = RoleEnvironment.CurrentRoleInstance.Id;
                int instanceIndex = 0;
                if (int.TryParse(instanceId.Substring(instanceId.LastIndexOf(".") + 1), out instanceIndex)) // On cloud.
                {
                    int.TryParse(instanceId.Substring(instanceId.LastIndexOf("_") + 1), out instanceIndex); // On compute emulator.
                }

                string key = "screenshot_control_list";
                Dictionary<string, string> sc_control_list = null;
                object result = SingletonCache.Instance.Get(key);
                if (result != null)
                {
                    sc_control_list = (Dictionary<string, string>)result;
                }
                if (sc_control_list == null)
                {
                    sc_control_list = new Dictionary<string, string>();
                }

                var Items_Running = (from m in sc_control_list
                                     where m.Value != null
                                     select m).ToList();
                foreach (var item in Items_Running)
                {
                    //Si el proceso esta en este worker... vamos a ver si sigue vivo o murió
                    var procInstance = sc_control_list[item.Key].Split('_')[0];
                    var procId = Convert.ToInt32(sc_control_list[item.Key].Split('_')[1]);
                    if (procInstance == instanceIndex.ToString())
                    {
                        Process proc = null;
                        try
                        {
                            proc = Process.GetProcessById(procId);
                        }
                        catch (ArgumentException)
                        {
                        }
                        if (proc == null)
                        {
                            sc_control_list.Remove(item.Key);
                        }
                    }
                }

                SingletonCache.Instance.Insert(key, sc_control_list);

            }
            catch (Exception ex)
            {
                GrabaLog(Utils.NivelLog.error, ex.Message);
            }
        }

        public void CheckIfCaptureProcessesHaveToBeStarted()
        {
            try
            {
                string key = "screenshot_control_list";

                Dictionary<string, string> sc_control_list = null;
                object result = SingletonCache.Instance.Get(key);
                if (result != null)
                {
                    sc_control_list = (Dictionary<string, string>)result;
                }
                if (sc_control_list == null)
                {
                    sc_control_list = new Dictionary<string, string>();
                }

                var Items_Not_Running = (from m in sc_control_list
                                         where m.Value == null
                                         select m).ToList();
                foreach (var item in Items_Not_Running)
                {
                    var username = item.Key.Split('@')[0];
                    var domain = item.Key.Split('@')[1];
                    var windowname = item.Key.Split('@')[2];
                    var ProcessID = LaunchCaptureProcess(username, domain, windowname);
                    sc_control_list[item.Key] = ProcessID;
                }

                SingletonCache.Instance.Insert(key, sc_control_list);
            }
            catch (Exception ex)
            {
                GrabaLog(Utils.NivelLog.error, ex.Message);
            }
        }

        public void LaunchCaptureProcesses()
        {
            //GrabaLog(Utils.NivelLog.info, "Iniciando LaunchCaptureProcesses");
            while (true)
            {
                CheckIfStartedCaptureProcessesAreStillRunning();

                CheckIfCaptureProcessesHaveToBeStarted();

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        public string LaunchCaptureProcess(string username, string domain, string windowname)
        {
            try
            {

                string instanceId = RoleEnvironment.CurrentRoleInstance.Id;
                int instanceIndex = 0;
                if (int.TryParse(instanceId.Substring(instanceId.LastIndexOf(".") + 1), out instanceIndex)) // On cloud.
                {
                    int.TryParse(instanceId.Substring(instanceId.LastIndexOf("_") + 1), out instanceIndex); // On compute emulator.
                }

                // instanceIndex is begin from 0. The instanceIndex of the first instance is 0. 
                // Response.Write(instanceIndex);

                string pathjs = "phantom.js";

                Process proc = DoLaunchCaptureProcess(pathjs, username, domain, "", "", windowname);
                if (proc != null)
                {
                    return instanceIndex + "_" + proc.Id.ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                GrabaLog(Utils.NivelLog.error, ex.Message);
                return null;
            }
        }

        public Process DoLaunchCaptureProcess(string pathjs, string username, string domain, string password, string GUID, string windowname)
        {
            try
            {

                string LogPhantomSetting = "LogPhantomInRelease";

#if DEBUG
                LogPhantomSetting = "LogPhantomInDebug";
#endif

                bool LogPhantom = false;
                LogPhantom = Convert.ToBoolean((from m in db.Settings
                                                where m.Name == LogPhantomSetting
                                                select m).FirstOrDefault().Value);

                //GrabaLog(Utils.NivelLog.error, "Iniciando CreateAndAddHeadlessProcessToPool: " + QueueId.ToString());

                string strPath = AppDomain.CurrentDomain.BaseDirectory + @"\img\";
                string phantomjs_filename = strPath + "phantomjs.exe";
                string scheme = null;
                string strdomain = null;
                int port = 0;
                try
                {
                    strdomain = RoleEnvironment.GetConfigurationSettingValue("Domain");
                    port = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("Port"));
                    scheme = RoleEnvironment.GetConfigurationSettingValue("Scheme");
                }
                catch (Exception _ex)
                {
                    GrabaLog(Utils.NivelLog.info, _ex.Message);
                }
                string mainURL = scheme + @"://" + strdomain + ":" + port;

                string debug_args = "";
#if DEBUG
                debug_args = " --remote-debugger-port=9000 --remote-debugger-autorun=yes ";
#endif

                string strlogPhantom = "false";
                if (LogPhantom == true)
                {
                    strlogPhantom = "true";
                }

                var psi = new ProcessStartInfo(phantomjs_filename)
                {
                    UseShellExecute = false,
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = strPath,
                    Arguments = @" --proxy-type=none --disk-cache=yes  --web-security=no --ignore-ssl-errors=yes " + debug_args + pathjs + @" " + mainURL + @" " + username + @" " + domain + @" " + password + @" " + GUID + @" " + windowname + @" " + strlogPhantom,
                    ErrorDialog = false
                };

#if DEBUG
                psi.CreateNoWindow = false;
                psi.WindowStyle = ProcessWindowStyle.Normal;
#endif

                if (LogPhantom == true)
                {
                    psi.RedirectStandardError = true;
                    psi.RedirectStandardOutput = true;
                }
                else
                {
                    psi.RedirectStandardError = false;
                    psi.RedirectStandardOutput = false;
                }

                var process = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo = psi
                };

                string Logged = "";
                Action<object, DataReceivedEventArgs> actionWrite = (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Logged += e.Data + System.Environment.NewLine;
                    }
                };

                process.ErrorDataReceived += (sender, e) => actionWrite(sender, e);
                process.OutputDataReceived += (sender, e) => actionWrite(sender, e);
                process.Exited += (sender, e) =>
                {
                    //Debug.WriteLine("Process exited with exit code " + process.ExitCode.ToString());
                    if (Logged != "")
                    {
                        GrabaLog(Utils.NivelLog.info, Logged);
                    }
                };

                process.Start();

                if (LogPhantom == true)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                //process.WaitForExit();
                return process;
            }
            catch (Exception)
            {
                //GrabaLog(Utils.NivelLog.error, ex.Message);
                return null;
            }
        }

        internal Converser GetConverserFromSystemWithEmailAndBusinessId(string Email, int id, vizzopContext db)
        {

            Converser converser = null;
            try
            {
                if (db == null)
                {
                    db = new vizzopContext();
                }

                converser = (from m in db.Conversers.Include("Business").Include("Agent")
                             where m.Email == Email
                             && m.Business.ID == id
                             select m).FirstOrDefault();

                if (converser == null)
                {
                    return null;
                }

                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = DateTime.Now;
                DateTime loctimeUTC = localZone.ToUniversalTime(loctime);
                converser.LastActive = loctimeUTC;

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
            }
            return converser;
        }

        internal BrowserFeature FindBrowserFeaturesBasedOnUserAgent(string UserAgent)
        {
            try
            {
                if (UserAgent.ToUpperInvariant().Contains("GOOGLEBOT") == true)
                {
                    UserAgent = "GOOGLEBOT";
                }
                else if (UserAgent.ToUpperInvariant().Contains("MSIE 6.0") == true)
                {
                    UserAgent = "MSIE 6.0";
                }
                else if (UserAgent.ToUpperInvariant().Contains("MSIE 7.0") == true)
                {
                    UserAgent = "MSIE 7.0";
                }
                else if (UserAgent.ToUpperInvariant().Contains("MSIE 8.0") == true)
                {
                    UserAgent = "MSIE 8.0";
                }
                else if (UserAgent.ToUpperInvariant().Contains("MSIE 8.0") == true)
                {
                    UserAgent = "MSIE 8.0";
                }
                else if (UserAgent.ToUpperInvariant().Contains("MSIE 9.0") == true)
                {
                    UserAgent = "MSIE 9.0";
                }
                else if (UserAgent.ToUpperInvariant().Contains("MSIE 10.0") == true)
                {
                    UserAgent = "MSIE 10.0";
                }
                else if (UserAgent.ToUpperInvariant().Contains("ANDROID") == true)
                {
                    UserAgent = "ANDROID";
                }
                else if (UserAgent.ToUpperInvariant().Contains("IPAD") == true)
                {
                    UserAgent = "IPAD";
                }
                else if (UserAgent.ToUpperInvariant().Contains("IPHONE") == true)
                {
                    UserAgent = "IPHONE";
                }
                else if (UserAgent.ToUpperInvariant().Contains("IPOD") == true)
                {
                    UserAgent = "IPOD";
                }
                else if (UserAgent.ToUpperInvariant().Contains("FIREFOX") == true)
                {
                    UserAgent = "FIREFOX";
                }
                else if (UserAgent.ToUpperInvariant().Contains("SAFARI") == true)
                {
                    UserAgent = "SAFARI";
                }


                var useragent_in_db = (from m in db.BrowserFeatures
                                       where m.UserAgent == UserAgent
                                       select m).FirstOrDefault();
                return useragent_in_db;
            }
            catch (Exception ex)
            {
                GrabaLogExcepcion(ex);
                return null;
            }
        }


        public void LimpiaWebLocations()
        {
            List<WebLocation> WebLocations = new List<WebLocation>();

            string tag = "weblocation";
            List<DataCacheTag> Tags = new List<DataCacheTag>();
            Tags.Add(new DataCacheTag(tag));
            object result = SingletonCache.Instance.GetByTag(tag);
            if (result != null)
            {
                IEnumerable<KeyValuePair<string, object>> ObjectList = (IEnumerable<KeyValuePair<string, object>>)result;
                foreach (var e in ObjectList)
                {
                    WebLocations.Add((WebLocation)e.Value);
                }

            }

            /*
            string key = "weblocations";
            DataCacheLockHandle lockHandle;
            object result = SingletonCache.Instance.GetWithLock(key, out lockHandle);
            if (result != null)
            {
                WebLocations = (List<WebLocation>)result;
            }
            */

            try
            {
                vizzopContext db = new vizzopContext();


                /* Primero miramos si vamos limpiando...
                 * 
                 */
                string LimpiaWebLocationsSetting = "LimpiaWebLocationsInRelease";
#if DEBUG
                LimpiaWebLocationsSetting = "LimpiaWebLocationsInDebug";
#endif
                bool LimpiaWebLocations = false;
                LimpiaWebLocations = Convert.ToBoolean((from m in db.Settings
                                                        where m.Name == LimpiaWebLocationsSetting
                                                        select m).FirstOrDefault().Value);
                if (LimpiaWebLocations == false)
                {
                    return;
                }


                TimeZone localZone = TimeZone.CurrentTimeZone;
                DateTime loctime = localZone.ToUniversalTime(DateTime.Now.AddSeconds(-1));
                var to_move = (from m in WebLocations
                               where m.TimeStamp_Last < loctime
                               select m).ToList();
                if (to_move.Count > 0)
                {
                    for (int i = to_move.Count() - 1; i >= 0; i--)
                    {
                        try
                        {
                            WebLocation m = to_move[i];
                            WebLocation_History newloc = null;
                            newloc = (from wl in db.WebLocations_History
                                      where wl.converser.ID == m.ConverserId
                                      && wl.Referrer == m.Referrer
                                      && wl.Url == m.Url
                                      select wl).FirstOrDefault();

                            if (newloc == null)
                            {
                                newloc = new WebLocation_History();

                                newloc.converser = (from j in db.Conversers
                                                    where j.ID == m.ConverserId
                                                    select j).FirstOrDefault();
                                newloc.Referrer = m.Referrer;
                                newloc.TimeStamp_First = m.TimeStamp_First;
                                newloc.TimeStamp_Last = m.TimeStamp_Last;
                                newloc.IP = m.IP;
                                newloc.Lang = m.Lang;
                                newloc.UserAgent = m.UserAgent;
                                newloc.Url = m.Url;
                                newloc.Ubication = m.Ubication;
                                newloc.Headers = m.Headers;
                                newloc.WindowName = m.WindowName;

                                if (newloc.converser != null)
                                {
                                    if (newloc.converser.Business.SaveWebLocationHistory == true)
                                    {
                                        db.WebLocations_History.Add(newloc);
                                    }
                                }
                            }
                            else
                            {
                                newloc.converser = (from j in db.Conversers
                                                    where j.ID == m.ConverserId
                                                    select j).FirstOrDefault();
                                newloc.TimeStamp_Last = m.TimeStamp_Last;
                            }
                            string key = @"weblocation" + "_" + m.UserName + @"@" + m.Domain + @"@" + m.WindowName; ;
                            SingletonCache.Instance.Remove(key);
                        }
                        catch (Exception)
                        {
                            //GrabaLogExcepcion(_ex);
                        }
                    }
                    db.SaveChanges();
                    //SingletonCache.Instance.InsertWithLock(key, WebLocations, lockHandle);
                }
            }
            catch (Exception ex)
            {
                //SingletonCache.Instance.UnLock(key, lockHandle);
                GrabaLogExcepcion(ex);
            }
        }

    }

    public static class ProcessExtensions
    {
        public static bool IsRunning(this Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            try
            {
                Process.GetProcessById(process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }
    }
}