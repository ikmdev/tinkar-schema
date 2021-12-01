using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        SubStream subStream;
        byte[] lengthBytes = new byte[4];

        public Int64 Count { get; private set; }


        public PBReader(Stream inputStream) : base(inputStream)
        {
            this.zipArchive = new ZipArchive(this.zipFile, ZipArchiveMode.Read);
            this.stream = this.OpenZipArchiveStream("export.pb");
            this.subStream = new SubStream(this.stream);

            using var countStream = this.OpenZipArchiveStream("count");
            byte[] countBytes = new byte[8];
            ReadBytes(countStream, countBytes, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(countBytes, 0, 8);
            this.Count = BitConverter.ToInt64(countBytes);
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

        Stream OpenZipArchiveStream(String name)
        {
            var zipEntry = this.zipArchive.GetEntry(name);
            return zipEntry.Open();
        }

        void ReadBytes(Stream s, byte[] bytes, Int32 length)
        {
            Int32 bytesReadSoFar = 0;
            while (length > 0)
            {
                Int32 count = s.Read(bytes, bytesReadSoFar, length);
                length -= count;
                bytesReadSoFar += count;
            }
        }


        /// <summary>
        /// Read single protobuf message.
        /// </summary>
        public PBTinkarMsg Read()
        {
            if (this.eof == true)
                return null;

            ReadBytes(this.stream, this.lengthBytes, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes, 0, 4);
            Int32 itemLength = BitConverter.ToInt32(lengthBytes);

            if (itemLength == -1)
            {
                this.eof = true;
                return null;
            }
            this.subStream.Init(itemLength);
            return PBTinkarMsg.Parser.ParseFrom(this.subStream);
        }
    }
}
