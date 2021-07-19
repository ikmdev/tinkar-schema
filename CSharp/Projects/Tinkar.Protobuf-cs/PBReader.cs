using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tinkar.ProtoBuf.CS;

namespace Tinkar.ProtoBuf.CS
{
    public class PBReader
    {
        Stream inputStream;
        public PBReader(Stream inputStream)
        {
            this.inputStream = inputStream;
        }

        public PBTinkarMsg Read() =>
            PBTinkarMsg.Parser.ParseDelimitedFrom(inputStream);
    }

}
