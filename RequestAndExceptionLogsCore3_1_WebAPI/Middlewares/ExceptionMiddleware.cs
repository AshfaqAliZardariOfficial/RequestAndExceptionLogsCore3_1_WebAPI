using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using RequestAndExceptionLogsCore3_1_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RequestAndExceptionLogsCore3_1_WebAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                //httpContext.Response.Headers.Add("status", "FAILED");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            string ErrorID = context.Request.Headers["REQUEST-ID"];
            List<ExceptionLogModel> ExceptionLogs = new List<ExceptionLogModel>();
            if (exception is AggregateException)
            {
                foreach (var e in (exception as AggregateException).InnerExceptions)
                {
                    ExceptionLogs.Add(new ExceptionLogModel
                    {
                        ExceptionType = "AggregateException",
                        ExceptionName = e.GetType().FullName,
                        Message = e.Message,
                        StackTrace = exception.StackTrace,
                        //ErrorMethodName = new StackTrace(exception).GetFrame(0).GetMethod().Name,
                        //ErrorLineNo = new StackTrace(exception, true).GetFrame(0).GetFileLineNumber().ToString(),
                        //                        exception.StackTrace.Substring(exception.StackTrace.Length - 7, 7),
                        ExceptionURL = context.Request.GetDisplayUrl(),
                        ErrorLocation = e.Message.ToString(),
                        CreatedAt = DateTimeOffset.UtcNow,
                        RequestID = ErrorID
                    });
                }
            }
            //else if (exception is SqlException)
            //{
            //    SqlException sqlException = (SqlException)exception;

            //    ExceptionLogs.Add(new ExceptionLogModel
            //    {
            //        ExceptionType = "SqlException",
            //        ExceptionName = sqlException.GetType().ToString(),
            //        Message = sqlException.Message,
            //        StackTrace = sqlException.StackTrace,
            //        ErrorMethodName = new StackTrace(sqlException).GetFrame(0).GetMethod().Name,
            //        ErrorLineNo = sqlException.LineNumber.ToString(),
            //        ExceptionURL = !string.IsNullOrEmpty(context.Request.GetDisplayUrl()) ? context.Request.GetDisplayUrl() : null,
            //        ErrorLocation = sqlException.Message.ToString(),
            //        CreatedAt = DateTimeOffset.UtcNow,
            //        RequestID = ErrorID

            //    });
            //}
            else
            {
                ExceptionLogs.Add(new ExceptionLogModel
                {
                    ExceptionType = exception.GetType().FullName,
                    ExceptionName = exception.GetType().ToString(),
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    //ErrorMethodName = new StackTrace(exception).GetFrame(0).GetMethod().Name,
                    //ErrorLineNo = new StackTrace(exception, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionURL = context.Request.GetDisplayUrl(),
                    //ErrorLocation = exception.Message.ToString(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    RequestID = ErrorID

                });
            }
            new ExceptionLogModel().SaveExceptionLogs(ExceptionLogs);
            await context.Response.WriteAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Some error occurred on server.",
                ErrorID = ErrorID
            }.ToString());
        }
    }
}
