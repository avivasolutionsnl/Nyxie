namespace Sitecore.Commerce.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public class SampleMethodScope : IDisposable
    {
        private static int TabCount = 1;
        private Stopwatch _watch = new Stopwatch();
        private string _methodName;
        private bool _disposed = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public SampleMethodScope()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            if (sf.GetMethod().Name.Equals("MoveNext"))
            {
                // The method is an async task
                sf = st.GetFrame(3);
            }

            this._methodName = sf.GetMethod().Name;
            this._watch.Start();

            Console.WriteLine($"{new string('>', (TabCount++) * 2)} [Begin Method] {this._methodName}");
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this._watch.Stop();
                Console.WriteLine($"{new string('<', (--TabCount) * 2)} [End Method] {this._methodName} : {this._watch.Elapsed}");
                this._disposed = true;
            }
        }
    }
}
