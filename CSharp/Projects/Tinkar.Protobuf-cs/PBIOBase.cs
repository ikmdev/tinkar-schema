using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Tinkar.ProtoBuf.CS
{
    /// <summary>
    /// Base class for all protobof file input/output.
    /// A protobuf file is a specially formatted zip file with the following characteristics.
    /// a) It will have a zip archive entry called "count" which will contain a 8 byte long (network order - big endian)
    ///    count fo the number of distinct protobuf items stored in the file.
    /// b) It will have a zip archive entry called "export.pb" which will contain the protobuf items.
    ///    Multiple protobuf items may be written to this archive.
    ///    Each protobuf item written is formatted in the following manner.
    ///    i. A 4 byte (Int32) length field (Network order / big endian) that identifies the number of bytes in the
    ///       serialized protobuf item.
    ///       End of file is marked by a -1 in this field.
    ///    ii. the serialized protobuf items.
    /// </summary>
    public abstract class PBIOBase : IDisposable
    {
        protected Stream zipFile;
        protected ZipArchive zipArchive;

        protected PBIOBase(Stream zipFile)
        {
            this.zipFile = zipFile;
        }
        public abstract void Dispose();
    }
}
