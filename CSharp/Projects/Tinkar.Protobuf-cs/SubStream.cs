using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tinkar.ProtoBuf.CS
{
    /// <summary>
    /// Create a sub stream of a stream. Reads only length bytes of a defined stream before returning
    /// that stream is empty.
    /// </summary>
    class SubStream : Stream
    {
        private Stream baseStream;
        private Int64 length;
        private Int64 position;

        public SubStream(Stream baseStream)
        {
            if (baseStream == null)
                throw new ArgumentNullException("baseStream");
            if (baseStream.CanRead == false)
                throw new ArgumentException("can't read base stream");
            this.baseStream = baseStream;
        }

        public void Init(Int64 length)
        {
            this.length = length;
            this.position = 0;
        }

        public override Int32 Read(byte[] buffer, Int32 offset, Int32 count)
        {
            Int64 remaining = length - position;
            if (remaining <= 0)
                return 0;
            if (remaining < count)
                count = (Int32)remaining;
            Int32 read = baseStream.Read(buffer, offset, count);
            position += read;
            return read;
        }

        public override long Length => length;
        public override bool CanRead => baseStream.CanRead;
        public override bool CanWrite => false;
        public override bool CanSeek => baseStream.CanSeek;
        public override Int64 Seek(Int64 offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(Int64 value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, Int32 offset, Int32 count) => throw new NotImplementedException();

        public override Int64 Position
        {
            get => position;
            set => throw new NotSupportedException();
        }

        public override void Flush() => baseStream.Flush();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (baseStream != null)
            {
                baseStream.Dispose();
                baseStream = null;
            }
        }
    }
}
