namespace Sitecore.Commerce.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public class SampleScenarioScope : IDisposable
    {
        private Stopwatch _watch = new Stopwatch();
        private string _scenarioName;
        bool _disposed = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public SampleScenarioScope(string scenarioName)
        {
            this._scenarioName = scenarioName;
            this._watch.Start();
            Console.WriteLine($"[Begin Scenario] {this._scenarioName}");
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this._watch.Stop();
                Console.WriteLine($"[End Scenario] {this._scenarioName} : {this._watch.Elapsed}");
                this._disposed = true;
            }
        }
    }
}
