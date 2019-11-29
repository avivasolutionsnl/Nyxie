using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions
{
    /// <summary>
    /// https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
    /// </summary>
    internal static class AsyncHelper
    {
        private static readonly TaskFactory MyTaskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;

            return MyTaskFactory
                .StartNew(() =>
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = cultureUi;
                    return func();
                })
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}