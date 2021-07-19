using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tinkar.Protobuf.XUnitTests
{
    class StopWatch : IDisposable
    {
        string msg;
        DateTime start;

        public StopWatch(String msg)
        {
            this.msg = msg;
            this.start = DateTime.Now;
        }

        public void Dispose()
        {
            TimeSpan s = DateTime.Now - start;
            Trace.WriteLine($"{this.msg} {s.ToString()}");
        }
    }
}
