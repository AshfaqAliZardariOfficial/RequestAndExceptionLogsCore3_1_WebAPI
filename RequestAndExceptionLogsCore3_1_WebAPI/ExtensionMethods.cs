using Microsoft.AspNetCore.Builder;
using RequestAndExceptionLogsCore3_1_WebAPI.Middlewares;

namespace RequestAndExceptionLogsCore3_1_WebAPI
{

    public static class ExtensionMethods
    {
        //public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        //{
        //    app.UseExceptionHandler(appError =>
        //    {
        //        appError.Run(async context =>
        //        {
        //            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //            context.Response.ContentType = "application/json";
        //            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        //            if (contextFeature != null)
        //            {
        //                await context.Response.WriteAsync(new
        //                {
        //                    StatusCode = context.Response.StatusCode,
        //                    Message = "Internal Server Error."
        //                }.ToString());
        //            }
        //        });
        //    });
        //}

        public static void UseExceptionLogs(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
