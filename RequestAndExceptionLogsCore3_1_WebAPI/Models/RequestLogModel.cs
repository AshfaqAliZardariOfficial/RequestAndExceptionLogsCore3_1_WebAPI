using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace RequestAndExceptionLogsCore3_1_WebAPI.Models
{
    public class RequestLogModel
    {
        public int? RequestLogID { get; set; }
        public string RequestID { get; set; }
        public string SessionID { get; set; }
        public string IpAddress { get; set; }
        public string RequestURL { get; set; }
        public string ReferrerURL { get; set; }
        public string Scheme { get; set; }
        public string Method { get; set; }
        public long? ContentLength { get; set; }
        public string QueryString { get; set; }
        public string RequestBody { get; set; }
        public string RequestFormData { get; set; }
        public string RequestHeaders { get; set; }
        public int ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string ResponseHeaders { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public void SaveRequestLogs(RequestLogModel RequestLog)
        {
            string query = @"INSERT INTO RequestLogs (RequestID,
SessionID,
IpAddress,
RequestURL,
ReferrerURL,
Scheme,
Method,
ContentLength,
QueryString,
RequestBody,
RequestFormData,
RequestHeaders,
ResponseCode,
ResponseText,
ResponseHeaders,
CreatedAt)
  VALUES (@RequestID, @SessionID, @IpAddress, @RequestURL, @ReferrerURL, @Scheme, @Method, @ContentLength, @QueryString, @RequestBody, @RequestFormData, @RequestHeaders, @ResponseCode, @ResponseText, @ResponseHeaders, @CreatedAt);";
            new DatabaseUtil().SaveData(query, RequestLogModelToDictionary(RequestLog));
        }

        private Dictionary<string, object> RequestLogModelToDictionary(RequestLogModel RequestLog)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            PropertyInfo[] props = typeof(RequestLogModel).GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].CanRead)
                {
                    res.Add(props[i].Name, RequestLog != null ? props[i].GetValue(RequestLog, null) : null);
                }
            }
            return res;
        }
    }
}
