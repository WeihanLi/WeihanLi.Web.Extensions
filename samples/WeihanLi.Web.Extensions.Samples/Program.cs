using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace WeihanLi.Web.Extensions.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
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
