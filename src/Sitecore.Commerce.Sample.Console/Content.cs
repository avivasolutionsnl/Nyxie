
namespace Sitecore.Commerce.Sample.Console
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Contexts;
    using FluentAssertions;

    using Sitecore.Commerce.ServiceProxy;
    using CommerceOps.Sitecore.Commerce.Engine;

    public static class Content
    {
        public static void RunScenarios()
        {
            var watch = new Stopwatch();
            watch.Start();
            Console.WriteLine("Begin Content");

            var devOp = new DevOpAndre();
            var container = devOp.Context.OpsContainer();

            Console.WriteLine("---------------------------------------------------");
            EnsureSyncDefaultContentPaths(container, "AdventureWorksAuthoring", devOp.Context.Shop);
            EnsureSyncDefaultContentPaths(container, "HabitatAuthoring", devOp.Context.Shop);
            Console.WriteLine("---------------------------------------------------");

            watch.Stop();

            Console.WriteLine($"End Content: {watch.ElapsedMilliseconds} ms");
        }

        private static void EnsureSyncDefaultContentPaths(Container container, string environmentName, string shopName)
        {
            Console.WriteLine($"Begin>> Ensure/Sync Content Path {environmentName}");
            var result = Proxy.GetValue(container.EnsureSyncDefaultContentPaths(environmentName, shopName));
            result.ResponseCode.Should().Be("Ok");

            var longRunningCommand = result;
            longRunningCommand.Should().NotBeNull();
            var waitingTime = new Stopwatch();
            waitingTime.Start();
            while (!longRunningCommand.Status.Equals("RanToCompletion") && waitingTime.Elapsed <= TimeSpan.FromMinutes(10))
            {
                Thread.Sleep(15000);
                longRunningCommand = Proxy.GetValue(container.CheckCommandStatus(longRunningCommand.TaskId));
                longRunningCommand.Should().NotBeNull();
            }

            waitingTime.Stop();
            waitingTime.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromMinutes(10));
            longRunningCommand.ResponseCode.Should().Be("Ok");
            Console.WriteLine($"End>> Ensure/Sync Content Path {environmentName}: {longRunningCommand.ResponseCode}");
        }
    }
}
