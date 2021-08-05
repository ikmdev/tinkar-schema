using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Tinkar.ProtoBuf.CS;

namespace Tinkar.ProtoBuf.CS
{
    /// <summary>
    /// Read protobuf tinkar items from protobuf zip input stream.
    /// </summary>
    public class PBReader : PBIOBase
    {
        bool eof = false;
        Stream stream;

        public PBReader(Stream inputStream) : base(inputStream)
        {
            this.stream = this.OpenZipArchiveStream("export.pb");
        }

        public override void Dispose()
        {
            if (this.zipArchive != null)
            {
                this.zipArchive.Dispose();
                this.zipArchive = null;
                this.zipFile = null;
                this.stream = null;
            }
        }

        public Stream OpenZipArchiveStream(String name)
        {
            var zipEntry = this.zipArchive.GetEntry(name);
            return zipEntry.Open();
        }

        /// <summary>
        /// Read single protobuf message.
        /// </summary>
        public PBTinkarMsg Read()
        {
            if (this.eof == true)
                return null;
            byte[] lengthBytes = new byte[4];
            this.stream.Read(lengthBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes, 0, 4);
            Int32 itemLength = BitConverter.ToInt32(lengthBytes);
            if (itemLength == -1)
            {
                this.eof = true;
                return null;
            }
            byte[] itemBytes = new byte[itemLength];
            this.stream.Read(itemBytes, 0, itemLength);
            return PBTinkarMsg.Parser.ParseFrom(itemBytes);
        }
    }

}
