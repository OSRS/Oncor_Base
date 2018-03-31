using Newtonsoft.Json.Linq;
using Osrs.Net.Http;
using System;
using System.IO;
using System.Text;

namespace Pnnl.Oncor.Rest
{
    public interface IServiceHandler : IHttpHandler
    {
        string BaseUrl
        {
            get;
        }
    }

    public enum JsonOpStatus
    {
        Unknown,
        Ok,
        Failed
    }


    /// <summary>
    /// Provides utility methods for dealing with the I/O of REST calls
    /// 
    /// To retreive the body from a POST request:
    ///     if Json is expected - use method in JsonUtils instead    /// 
    /// 
    ///     RestUtils.ReadBody(request)   will give the entire body parsed as a string
    /// 
    /// To send a body back to the browser:
    /// 
    ///     if sending a Json object standard as {status:JsonOpStatus, data:JsonData}...
    ///         RestUtils.Push(response, JsonOpStatus, JToken)   will properly format the output
    ///         RestUtils.Push(response, JsonOpStatus)  will properly format the output with no data property (just the status)
    ///     RestUtils.Push(response, string)    will just push the string directly to the body with no added formatting (still sets the MIME type to application/json)
    ///     RestUtils.PushRaw(response, string, string) will just push the string directly to the body with the provided MIME string or text/plain if mimetype is null)
    /// </summary>
    public static class RestUtils
    {
        public static string ReadBody(HttpRequest request)
        {
            try
            {
                return (new StreamReader(request.Body, Encoding.UTF8)).ReadToEnd();
            }
            catch
            { }
            return null;
        }

        public static string JsonOpStatus(JsonOpStatus status)
        {
            return JsonOpStatus(status, null);
        }

        public static string JsonOpStatus(JsonOpStatus status, string jsonDataPayload)
        {
            if (status == Rest.JsonOpStatus.Ok)
                return JsonOpStatus("ok", jsonDataPayload);
            else if (status == Rest.JsonOpStatus.Failed)
                return JsonOpStatus("failed", jsonDataPayload);
            return JsonOpStatus("unknown", jsonDataPayload);
        }

        public static string JsonOpStatus(string status)
        {
            return JsonOpStatus(status, null);
        }

        public static string JsonOpStatus(string status, string jsonDataPayload)
        {
            if (string.IsNullOrEmpty(jsonDataPayload))
            {
                if (!string.IsNullOrEmpty(status))
                    return "{\"status\":\"" + status + "\"}";
                return JsonOpStatus("unknown", null);
            }
            if (!string.IsNullOrEmpty(status))
                return "{\"status\":\"" + status + "\",\"data\":" + jsonDataPayload + "}";
            return JsonOpStatus("unknown", jsonDataPayload);
        }

        public static void Push(HttpResponse response)
        {
            Push(response, RestUtils.JsonOpStatus(Rest.JsonOpStatus.Ok));
        }

        public static void Push(HttpResponse response, JsonOpStatus status)
        {
            Push(response, RestUtils.JsonOpStatus(status));
        }

        public static void Push(HttpResponse response, JsonOpStatus status, JToken data)
        {
            if (data!=null)
                Push(response, RestUtils.JsonOpStatus(status, data.ToString()));
            else
                Push(response, RestUtils.JsonOpStatus(status));
        }

        public static void Push(HttpResponse response, string jsonPayload)
        {
            byte[] raw;
            if (jsonPayload != null)
                raw = Encoding.UTF8.GetBytes(jsonPayload);
            else
                raw = new byte[0];

            response.StatusCode = HttpStatusCodes.Status200OK;
            response.ContentType = "application/json";
            response.ContentLength = raw.Length;
            response.Body.Write(raw, 0, raw.Length);
        }

        public static void Push(HttpResponse response, string jsonPayload, string mimeType)
        {
            byte[] raw;
            if (jsonPayload != null)
                raw = Encoding.UTF8.GetBytes(jsonPayload);
            else
                raw = new byte[0];

            response.StatusCode = HttpStatusCodes.Status200OK;
            if (!string.IsNullOrEmpty(mimeType))
                response.ContentType = mimeType;
            else
                response.ContentType = "text/plain";
            response.ContentLength = raw.Length;
            response.Body.Write(raw, 0, raw.Length);
        }

        public static string LocalUrl(IServiceHandler handler, HttpRequest request)
        {
            return LocalUrl(handler.BaseUrl, request.Path);
        }

        public static string LocalUrl(string baseUrl, string encodedUrl)
        {
            return encodedUrl.Substring(encodedUrl.IndexOf(baseUrl, StringComparison.OrdinalIgnoreCase));
        }

        public static string StripLocal(string baseUrl, string localUrl)
        {
            return localUrl.Substring(baseUrl.Length);
        }
    }
}
