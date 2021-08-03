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
    public class PBWriter : IDisposable
    {
        Stream zipFile;
        ZipArchive zipArchive;
        ZipArchiveEntry zipEntry;
        Stream stream;

        public PBWriter(Stream zipFile)
        {
            this.zipFile = zipFile;
            this.zipArchive = new ZipArchive(this.zipFile, ZipArchiveMode.Create);
            this.zipEntry = this.zipArchive.CreateEntry("export.pb");
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
        /// Write single protobuf message.
        /// </summary>
        /// <param name="msg"></param>
        public void Write(PBTinkarMsg msg) =>
            msg.WriteDelimitedTo(stream);
    }
}
