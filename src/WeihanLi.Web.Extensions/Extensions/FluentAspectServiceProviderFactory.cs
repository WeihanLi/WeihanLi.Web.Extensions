using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeihanLi.Common.Aspect;

namespace WeihanLi.Web.Extensions
{
    internal sealed class FluentAspectsServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private readonly Action<FluentAspectOptions> _optionsAction;
        private readonly Action<IFluentAspectsBuilder> _aspectBuildAction;
        private readonly Func<Type, bool> _ignoreTypesPredict;

        public FluentAspectsServiceProviderFactory(
            Action<FluentAspectOptions> optionsAction,
            Action<IFluentAspectsBuilder> aspectBuildAction,
            Func<Type, bool> ignoreTypesPredict
            )
        {
            _optionsAction = optionsAction;
            _aspectBuildAction = aspectBuildAction;
            _ignoreTypesPredict = ignoreTypesPredict;
        }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return containerBuilder.BuildFluentAspectsProvider(_optionsAction, _aspectBuildAction, _ignoreTypesPredict);
        }
    }

    public static class FluentAspectServiceProviderFactoryExtensions
    {
        public static IHostBuilder UseFluentAspectsServiceProviderFactory(this IHostBuilder hostBuilder,
            Action<FluentAspectOptions> optionsAction,
            Action<IFluentAspectsBuilder> aspectBuildAction = null,
            Func<Type, bool> ignoreTypesPredict = null)
        {
            if (ignoreTypesPredict == null)
            {
                ignoreTypesPredict = t =>
                    t.Namespace?.StartsWith("Microsoft.") == true
                    || t.Namespace?.StartsWith("System.") == true
                    ;
            }
            hostBuilder.UseServiceProviderFactory(
                new FluentAspectsServiceProviderFactory(optionsAction, aspectBuildAction, ignoreTypesPredict)
                );
            return hostBuilder;
        }
    }
}
