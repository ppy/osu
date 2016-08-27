//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OpenTK;

namespace osu.Framework.IO
{
    public class AsyncBufferStream : Stream
    {
        const int block_size = 32768;

        #region Concurrent access
        readonly byte[] data;

        readonly bool[] blockLoadedStatus;

        volatile bool isClosed;
        volatile bool isLoaded;

        volatile int position;
        volatile int amountBytesToRead;
        #endregion

        readonly int blocksToReadAhead;

        readonly Stream underlyingStream;

        private Thread loadThread;

        /// <summary>
        /// A stream that buffers the underlying stream to contiguous memory, reading until the whole file is eventually memory-backed.
        /// </summary>
        /// <param name="stream">The underlying stream to read from.</param>
        /// <param name="blocksToReadAhead">The amount of blocks to read ahead of the read position.</param>
        /// <param name="shared">Another AsyncBufferStream which is backing the same underlying stream. Allows shared usage of memory-backing.</param>
        public AsyncBufferStream(Stream stream, int blocksToReadAhead, AsyncBufferStream shared = null)
        {
            Debug.Assert(stream != null);

            this.blocksToReadAhead = blocksToReadAhead;
            underlyingStream = stream;

            if (underlyingStream.CanSeek)
                underlyingStream.Seek(0, SeekOrigin.Begin);

            if (shared?.Length != stream.Length)
            {
                data = new byte[underlyingStream.Length];
                blockLoadedStatus = new bool[data.Length / block_size + 1];
            }
            else
            {
                data = shared.data;
                blockLoadedStatus = shared.blockLoadedStatus;
                isLoaded = shared.isLoaded;
            }


            loadThread = new Thread(loadRequiredBlocks) { IsBackground = true };
            loadThread.Start();
        }

        ~AsyncBufferStream()
        {
            Dispose(false);
        }

        private void loadRequiredBlocks()
        {
            try
            {
                if (isLoaded)
                    return;

                int last = -1;
                while (!isLoaded && !isClosed)
                {
                    int curr = nextBlockToLoad;
                    if (curr < 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    int readStart = curr * block_size;

                    if (last + 1 != curr)
                    {
                        //follow along with a seek.
                        Debug.Assert(underlyingStream.CanSeek);
                        underlyingStream.Seek(readStart, SeekOrigin.Begin);
                    }

                    Debug.Assert(underlyingStream.Position == readStart);

                    int readSize = Math.Min(data.Length - readStart, block_size);
                    int read = underlyingStream.Read(data, readStart, readSize);

                    Debug.Assert(read == readSize);

                    blockLoadedStatus[curr] = true;
                    last = curr;

                    isLoaded |= blockLoadedStatus.All((bool loaded) => loaded);
                }

                isLoaded = true;
            }
            catch (ThreadAbortException) { }

            if (!isClosed) underlyingStream?.Close();
        }

        private int nextBlockToLoad
        {
            get
            {
                if (isClosed) return -1;

                int start = underlyingStream.CanSeek ? position / block_size : 0;

                int end = blockLoadedStatus.Length;
                if (blocksToReadAhead > -1)
                    end = Math.Min(end, (position + amountBytesToRead) / block_size + blocksToReadAhead + 1);

                for (int i = start; i < end; i++)
                    if (!blockLoadedStatus[i]) return i;

                return -1;
            }
        }

        protected override void Dispose(bool disposing)
        {
            loadThread?.Abort();

            if (!isClosed) Close();
            base.Dispose(disposing);
        }

        public override void Close()
        {
            isClosed = true;

            underlyingStream?.Close();

            base.Close();
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => data.Length;

        public override long Position
        {
            get
            {
                return position;
            }

            set
            {
                position = MathHelper.Clamp((int)value, 0, data.Length);
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Debug.Assert(count <= buffer.Length - offset);

            amountBytesToRead = Math.Min(count, data.Length - position);

            int startBlock = position / block_size;
            int endBlock = (position + amountBytesToRead) / block_size;

            //ensure all required buffers are loaded
            for (int i = startBlock; i <= endBlock; i++)
            {
                while (!blockLoadedStatus[i])
                    Thread.Sleep(1);
            }

            Array.Copy(data, position, buffer, offset, amountBytesToRead);

            int bytesRead = amountBytesToRead;

            amountBytesToRead = 0;
            position += bytesRead;

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = data.Length + offset;
                    break;
            }

            return position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
