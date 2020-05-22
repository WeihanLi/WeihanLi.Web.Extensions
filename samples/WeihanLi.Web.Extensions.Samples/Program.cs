using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WeihanLi.Web.Extensions.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
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
