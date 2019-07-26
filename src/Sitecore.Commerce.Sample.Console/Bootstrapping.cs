namespace Sitecore.Commerce.Sample.Console
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Contexts;
    using FluentAssertions;

    using Sitecore.Commerce.ServiceProxy;
    using CommerceOps = CommerceOps.Sitecore.Commerce.Engine;

    public static class Bootstrapping
    {
        public static void RunScenarios()
        {
            var watch = new Stopwatch();
            watch.Start();

            var devOp = new DevOpAndre();
            var container = devOp.Context.OpsContainer();

            Console.WriteLine("---------------------------------------------------");
            Bootstrap(container);
            CleanEnvironment(container, "AdventureWorksAuthoring");
            CleanEnvironment(container, "HabitatAuthoring");
            InitializeEnvironment(container, "AdventureWorksAuthoring");
            InitializeEnvironment(container, "HabitatAuthoring");

            Console.WriteLine("---------------------------------------------------");

            GetPipelines(container);
            Console.WriteLine("---------------------------------------------------");

            watch.Stop();

            Console.WriteLine($"End Bootstrapping: {watch.ElapsedMilliseconds} ms");
        }

        private static void Bootstrap(CommerceOps.Container container)
        {
            Console.WriteLine("Begin>> Bootstrap");
            var result = Proxy.GetValue(container.Bootstrap());
            Console.WriteLine($"End>> Bootstrap: {result.ResponseCode}");
            result.ResponseCode.Should().Be("Ok");
        }

        private static void CleanEnvironment(CommerceOps.Container container, string environment)
        {
            Console.WriteLine($"Begin>> Clean Environment: {environment}");
            var result = Proxy.GetValue(container.CleanEnvironment(environment));
            Console.WriteLine($"End>> Clean Environment: {result.ResponseCode}");
            result.ResponseCode.Should().Be("Ok");
        }

        public static void InitializeEnvironment(CommerceOps.Container container, string environmentName)
        {
            Console.WriteLine($"Begin>> Initialize Environment:{environmentName}");
            var result = Proxy.GetValue(container.InitializeEnvironment(environmentName));
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
            Console.WriteLine($"End>> Initialize Environment: {longRunningCommand.ResponseCode}");
        }

        private static void GetPipelines(CommerceOps.Container container)
        {
            var pipelineConfiguration = Proxy.GetValue(container.GetPipelines());

            var localPath = AppDomain.CurrentDomain.BaseDirectory;

            var pipelineFile = $"{localPath}/logs/ConfiguredPipelines.log";

            if (!System.IO.Directory.Exists($"{localPath}/logs"))
            {
                System.IO.Directory.CreateDirectory($"{localPath}/logs");
            }

            if (System.IO.File.Exists(pipelineFile))
            {
                System.IO.File.Delete(pipelineFile);
            }

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(pipelineFile))
            {
                file.WriteLine("Current Pipeline Configuration");
                file.WriteLine("-----------------------------------------------------------------");
                foreach (var pipeline in pipelineConfiguration.List)
                {
                    file.WriteLine($"{pipeline.Namespace}");
                    file.WriteLine($"{pipeline.Name} ({pipeline.Receives} => {pipeline.Returns})");
                    foreach (var block in pipeline.Blocks)
                    {
                        var computedNamespace = block.Namespace.Replace("Sitecore.Commerce.","");
                        file.WriteLine($"     {computedNamespace}.{block.Name} ({block.Receives} => {block.Returns})");
                    }

                    if (!string.IsNullOrEmpty(pipeline.Comment))
                    {
                        file.WriteLine("     ------------------------------------------------------------");
                        file.WriteLine($"     Comment: {pipeline.Comment}");
                    }

                    file.WriteLine("-----------------------------------------------------------------");
                }
            }
        }
    }
}
