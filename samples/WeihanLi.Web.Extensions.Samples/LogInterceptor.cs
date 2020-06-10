using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WeihanLi.Common.Aspect;
using WeihanLi.Extensions;

namespace WeihanLi.Web.Extensions.Samples
{
    public class EventPublishLogInterceptor : AbstractInterceptor
    {
        public override async Task Invoke(IInvocation invocation, Func<Task> next)
        {
            Console.WriteLine("-------------------------------");
            Console.WriteLine($"Method {invocation.Method?.Name} invoke begin, eventData:{invocation.Arguments.ToJson()}");
            var watch = Stopwatch.StartNew();
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Method {invocation.Method?.Name} invoke exception({ex})");
            }
            finally
            {
                watch.Stop();
                Console.WriteLine($"Method {invocation.Method?.Name} invoke complete, elasped:{watch.ElapsedMilliseconds} ms");
            }
            Console.WriteLine("-------------------------------");
        }
    }

    public class EventHandleLogInterceptor : IInterceptor
    {
        public async Task Invoke(IInvocation invocation, Func<Task> next)
        {
            Console.WriteLine("-------------------------------");
            Console.WriteLine($"Event handle begin, eventData:{invocation.Arguments.ToJson()}");
            var watch = Stopwatch.StartNew();
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Event handle exception({ex})");
            }
            finally
            {
                watch.Stop();
                Console.WriteLine($"Event handle complete, elasped:{watch.ElapsedMilliseconds} ms");
            }
            Console.WriteLine("-------------------------------");
        }
    }
}
