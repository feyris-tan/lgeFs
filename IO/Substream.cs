using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LgeFs
{
    internal class FileSection
    {
        public long length, offset;
    }

    public class Substream : Stream
    {
        Stream str;
        FileSection section;
        long internalPosition;

        private long endPosition { get { return section.offset + section.length; } }

        public Substream(long offset, long length, Stream str)
        {
            section = new FileSection();
            section.length = length;
            section.offset = offset;
            this.str = str;
        }
        
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { return section.length; }
        }

        public int Length32
        {
            get
            {
                return (int)Length;
            }
        }

        public override long Position
        {
            get
            {
                return internalPosition;
            }
            set
            {
                internalPosition = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            str.Position = (section.offset + internalPosition);
            while ((internalPosition + count) > section.length)
                count--;
            if (count == 0)
                return 0;
            if (count < 0)
                throw new IndexOutOfRangeException();
            str.Read(buffer, offset, count);
            internalPosition += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset > section.length)
                        throw new IndexOutOfRangeException();
                    internalPosition = offset;
                    break;
                case SeekOrigin.Current:
                    internalPosition += offset;
                    if (internalPosition > section.length)
                        internalPosition = section.length;
                    break;
                case SeekOrigin.End:
                    internalPosition = section.length - offset;
                    if (internalPosition < 0)
                        internalPosition = 0;
                    break;
            }
            return internalPosition;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
