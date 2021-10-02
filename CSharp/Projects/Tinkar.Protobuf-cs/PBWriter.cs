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
        /// <summary>
        /// Current zip entry stream
        /// </summary>
        Stream stream;

        /// <summary>
        /// Total number of tinkar items written
        /// </summary>
        Int64 itemCount;

        /// <summary>
        /// Temporary memory stream to write items to before writing bytes to zip stream.
        /// </summary>
        MemoryStream msTemp = new MemoryStream();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="zipFile">Outputr zip stream</param>
        public PBWriter(Stream zipFile) : base(zipFile)
        {
            this.zipArchive = new ZipArchive(this.zipFile, ZipArchiveMode.Create);
            this.stream = this.CreateZipArchiveStream("export.pb");
            this.itemCount = 0;
        }

        /// <summary>
        /// Dispose. Close all streams.
        /// </summary>
        public override void Dispose()
        {
            if (this.zipArchive == null)
                return;

            if (this.stream != null)
            {
                this.WriteLen(-1);
                this.stream.Close();
                this.stream.Dispose();
                this.stream = null;
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
            this.zipFile = null;
        }

        Stream CreateZipArchiveStream(String name)
        {
            var zipEntry = this.zipArchive.CreateEntry(name, CompressionLevel.Optimal);
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
