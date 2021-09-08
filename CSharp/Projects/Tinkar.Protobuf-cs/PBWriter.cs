using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Tinkar.ProtoBuf.CS
{
    /// <summary>
    /// Write Protobuf Tinkar items to zip output file.
    /// </summary>
    public class PBWriter : PBIOBase
    {
        Stream stream;
        Int64 itemCount;
        MemoryStream msTemp = new MemoryStream();

        public PBWriter(Stream zipFile) : base(zipFile)
        {
            this.stream = this.CreateZipArchiveStream("export.pb");
            this.itemCount = 0;
        }

        public override void Dispose()
        {
            if (this.zipArchive == null)
                return;

            // write terminator to output stream and close.
            {
                this.WriteLen(-1);
                this.stream.Close();
                this.stream.Dispose();
            }

            // Write out Int64 item count to zip entry 'count'.
            using (Stream s = this.CreateZipArchiveStream("count"))
            {
                byte[] bytes = BitConverter.GetBytes(this.itemCount);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                s.Write(bytes);
            }

            this.zipArchive.Dispose();
            this.zipArchive = null;
            this.stream = null;
            this.zipFile = null;
        }

        Stream CreateZipArchiveStream(String name)
        {
            var zipEntry = this.zipArchive.CreateEntry(name, CompressionLevel.NoCompression);
            return zipEntry.Open();
        }

        void WriteLen(Int32 itemLen)
        {
            byte[] bytes = BitConverter.GetBytes(itemLen);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            this.stream.Write(bytes);
        }

        /// <summary>
        /// Write single protobuf message.
        /// We write the length of each message first (32 bits, network order), then the
        /// actual bytes of the message.
        /// </summary>
        /// <param name="msg"></param>
        public void Write(PBTinkarMsg msg)
        {
            this.msTemp.Position = 0;
            msg.WriteTo(this.msTemp);
            Int32 itemLen = (Int32)this.msTemp.Position;
            this.WriteLen(itemLen);
            this.stream.Write(this.msTemp.GetBuffer(), 0, itemLen);
            this.itemCount += 1;
        }
    }
}
