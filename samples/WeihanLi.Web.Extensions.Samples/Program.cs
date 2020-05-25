﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.AspNetCore.Authentication;
using WeihanLi.AspNetCore.Authentication.QueryAuthentication;

namespace WeihanLi.Web.Extensions.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddAuthentication(QueryAuthenticationDefaults.AuthenticationSchema)
                        .AddQuery();

                    services.AddControllers();
                    services.AddHttpContextAccessor();
                    services.AddHttpContextUserIdProvider(options =>
                    {
                        options.UserIdFactory = context => $"{context.GetUserIP()}";
                    });
                })
                .Configure(app =>
                {
                    app.UseCustomExceptionHandler();
                    app.UseHealthCheck();
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                        endpoints.MapDefaultControllerRoute();
                    });
                })
                .Build();
            host.Run();
        }
    }
}
