using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeihanLi.Common.Aspect;
using WeihanLi.Web.Authentication;
using WeihanLi.Web.Authentication.HeaderAuthentication;

namespace WeihanLi.Web.Extensions.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .ConfigureServices(services =>
                        {
                            services.AddAuthentication(HeaderAuthenticationDefaults.AuthenticationSchema)
                                .AddQuery(options =>
                                {
                                    options.UserIdQueryKey = "uid";
                                })
                                .AddHeader(options =>
                                {
                                    options.UserIdHeaderName = "X-UserId";
                                    options.UserNameHeaderName = "X-UserName";
                                    options.UserRolesHeaderName = "X-UserRoles";
                                });

                            services.AddControllers();
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
                            });
                        })
                        ;
                })
                 .UseFluentAspectsServiceProviderFactory(options =>
                 {
                     options.InterceptAll()
                       .With<EventPublishLogInterceptor>();
                 })
                .Build();
            host.Run();
        }
    }
}
