using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WeihanLi.Web.Extensions.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services=> { })
                .Build() ;
            host.Run();
        }
    }
}
