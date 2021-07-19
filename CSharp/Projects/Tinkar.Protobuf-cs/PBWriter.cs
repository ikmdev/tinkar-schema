using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tinkar.ProtoBuf.CS
{
    public class PBWriter
    {
        Stream outputStream;
        public PBWriter(Stream outputStream)
        {
            this.outputStream = outputStream;
        }

        public void Write(PBTinkarMsg msg) =>
            msg.WriteDelimitedTo(outputStream);
    }
}
