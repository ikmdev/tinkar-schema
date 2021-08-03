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
    public class PBReader : IDisposable
    {
        Stream zipFile;
        ZipArchive zipArchive;
        ZipArchiveEntry zipEntry;
        Stream stream;

        public PBReader(Stream inputStream)
        {
            this.zipFile = inputStream;
            this.zipArchive = new ZipArchive(this.zipFile);
            this.zipEntry = this.zipArchive.GetEntry("export.pb");
            this.stream = this.zipEntry.Open();
        }

        public void Dispose()
        {
            if (this.zipArchive != null)
            {
                this.zipArchive.Dispose();
                this.zipArchive = null;
                this.zipFile = null;
                this.zipEntry = null;
                this.stream = null;
            }
        }

        /// <summary>
        /// Read single protobuf message.
        /// </summary>
        public PBTinkarMsg Read() =>
            PBTinkarMsg.Parser.ParseDelimitedFrom(this.stream);
    }

}
