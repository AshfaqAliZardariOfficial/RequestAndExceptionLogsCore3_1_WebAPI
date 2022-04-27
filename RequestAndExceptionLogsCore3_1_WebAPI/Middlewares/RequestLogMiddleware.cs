using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using RequestAndExceptionLogsCore3_1_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RequestAndExceptionLogsCore3_1_WebAPI.Middlewares
{
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string REQUESTID = Guid.NewGuid().ToString("N").ToUpper();

            RequestLogModel RequestLog = await PrepareRequestLog(httpContext, REQUESTID);


            var originalBody = httpContext.Response.Body;
            using var newBody = new MemoryStream();
            httpContext.Response.Body = newBody;
            httpContext.Response.OnStarting(() =>
            {
                httpContext.Response.Headers.Add("RESPONSE-ID", REQUESTID);

                httpContext.Response.Headers.Add("RESPONSE-DATETIME", DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz"));
                string status = httpContext.Response.StatusCode == 200 || httpContext.Response.StatusCode == 201 ? "SUCCESS" : "Failed";
                httpContext.Response.Headers.Add("status", status);

                return Task.CompletedTask;
            });
            //try
            //{
            await _next(httpContext);

            // ReadResponseBody
            newBody.Seek(0, SeekOrigin.Begin);
            string bodyText = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();

            newBody.Seek(0, SeekOrigin.Begin);
            await newBody.CopyToAsync(originalBody);
            // Get Headers
            Dictionary<string, object> ResponseHeaders = new Dictionary<string, object>();
            if (httpContext.Response.Headers != null && httpContext.Response.Headers.Count > 0)
            {
                foreach (var header in httpContext.Response.Headers)
                {
                    ResponseHeaders.Add(header.Key, header.Value);
                }

            }
            RequestLog.ResponseHeaders = ResponseHeaders != null && ResponseHeaders.Count > 0 ? JsonConvert.SerializeObject(ResponseHeaders, Formatting.Indented) : null;
            RequestLog.ResponseText = string.IsNullOrEmpty(bodyText) ? null : bodyText;
            RequestLog.ResponseCode = httpContext.Response.StatusCode;
            RequestLog.CreatedAt = DateTimeOffset.UtcNow;
            new RequestLogModel().SaveRequestLogs(RequestLog);
            //}
            //catch (Exception)
            //{
            //    //add additional exception handling logic here 
            //    httpContext.Response.StatusCode = 500;
            //}

        }

        private static async Task<RequestLogModel> PrepareRequestLog(HttpContext httpContext, string REQUESTID)
        {
            HttpRequest Request = httpContext.Request;
            RequestLogModel RequestLog = new RequestLogModel();
            // All request data to be saved!
            Request.Headers.Add("REQUEST-ID", REQUESTID);
            // DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            Request.Headers.Add("REQUEST-DATETIME", DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz"));
            RequestLog.RequestID = REQUESTID;
            RequestLog.IpAddress = httpContext.Connection?.RemoteIpAddress?.ToString();
            RequestLog.RequestURL = Request.GetDisplayUrl();
            RequestLog.ReferrerURL = Request.GetTypedHeaders()?.Referer?.ToString();
            RequestLog.Scheme = Request.Scheme;
            RequestLog.Method = Request.Method;
            RequestLog.ContentLength = Request.ContentLength != null && Request.ContentLength > 0 ? Request.ContentLength : null;
            RequestLog.QueryString = Request.Query?.Count > 0 ? Request.QueryString.ToString() : null;

            // Get Headers, FormData
            Dictionary<string, object> RequestHeaders = new Dictionary<string, object>();
            Dictionary<string, object> RequestFormData = new Dictionary<string, object>();
            string requestBodyContent = null;
            if (Request.Headers != null && Request.Headers.Count > 0)
            {
                foreach (var header in Request.Headers)
                {
                    RequestHeaders.Add(header.Key, header.Value);
                }

            }
            RequestLog.RequestHeaders = RequestHeaders != null && RequestHeaders.Count > 0 ? JsonConvert.SerializeObject(RequestHeaders, Formatting.Indented) : null;
            // Get Form Params
            if (Request.HasFormContentType && Request.Form != null && Request.Form.Count > 0)
            {
                foreach (var form in Request.Form)
                {

                    RequestFormData.Add(form.Key, form.Value);
                }
            }
            RequestLog.RequestFormData = RequestFormData != null && RequestFormData.Count > 0 ? JsonConvert.SerializeObject(RequestFormData, Formatting.Indented) : null;

            // Get Body Params
            if (Request.Body.CanRead)
            {
                Request.EnableBuffering();
                var body = await new StreamReader(Request.Body).ReadToEndAsync();
                requestBodyContent = string.IsNullOrEmpty(body) ? null : body;
                Request.Body.Seek(0, SeekOrigin.Begin);
            }
            RequestLog.RequestBody = requestBodyContent;
            return RequestLog;
        }
    }

}
