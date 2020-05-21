using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Services;
using WeihanLi.Web.Services;

namespace WeihanLi.Web.Extensions.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHttpContextAccessor();
                    services.AddSingleton<IUserIdProvider, HttpContextUserIdProvider>();
                })
                .Configure(app =>
                {
                    app.UseCustomExceptionHandler();
                    app.UseHealthCheck();

                    app.UseAuthentication();
                    app.UseMvc();
                })
                .Build();
            host.Run();
        }
    }
}
