using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tinkar.ProtoBuf.CS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Assert = Xunit.Assert;

namespace Tinkar.Protobuf.XUnitTests
{
    public class Timings
    {
        const Int32 BlockSize = 100000;
        const String ProtobufFile = @"C:\Development\Tinkar\tinkar-solor-us-export.pbin";


        [DoNotParallelize]
        [Fact]
        public void ReadConcepts()
        {
            PBTinkarMsg[] msgs = new PBTinkarMsg[8000000];
            DateTime start = DateTime.Now;
            DateTime current = start;
            TimeSpan elapsed;
            using FileStream input = File.OpenRead(ProtobufFile);
            Int32 i = 0;
            PBReader pbReader = new PBReader(input);
            NativeMethods.PreventSleep();
            while (input.Position < input.Length)
            {
                msgs[i] = pbReader.Read();
                //msgs[i] = null;
                if ((i % BlockSize) == 0)
                {
                    elapsed = DateTime.Now - current;
                    current = DateTime.Now;
                    float itemsPerSec = (float)BlockSize / (float)elapsed.TotalSeconds;
                    Trace.WriteLine($"{i} - Read time {elapsed.TotalSeconds} / {itemsPerSec.ToString("F2")}");
                    GC.Collect(2, GCCollectionMode.Forced);
                }
                i += 1;
            }
            elapsed = DateTime.Now - start;
            NativeMethods.AllowSleep();
            Trace.WriteLine($"AProtobufReadConcepts time {elapsed}");
        }
    }
}
