// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;
using WeihanLi.Common.Aspect;
using WeihanLi.Web.Authentication;
using WeihanLi.Web.Authentication.ApiKeyAuthentication;
using WeihanLi.Web.Authentication.HeaderAuthentication;
using WeihanLi.Web.Extensions;
using WeihanLi.Web.Extensions.Samples;

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
                    })
                    .AddApiKey(options =>
                    {
                        options.ClaimsIssuer = "https://id.weihanli.xyz";
                        options.ApiKey = "123456";
                        options.ApiKeyName = "X-ApiKey";
                        options.KeyLocation = KeyLocation.HeaderOrQuery;
                    })
                    ;

                services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
                services.AddHttpContextUserIdProvider(options =>
                {
                    options.UserIdFactory = context => $"{context.GetUserIP()}";
                });
            })
            .Configure(app =>
            {
                // app.UseCustomExceptionHandler();
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
