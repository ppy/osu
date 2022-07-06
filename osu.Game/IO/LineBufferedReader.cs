// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;

namespace osu.Game.IO
{
    /// <summary>
    /// A <see cref="StreamReader"/>-like decorator (with more limited API) for <see cref="Stream"/>s
    /// that allows lines to be peeked without consuming.
    /// </summary>
    public class LineBufferedReader : IDisposable
    {
        private readonly StreamReader streamReader;

        private string? peekedLine;

        public LineBufferedReader(Stream stream, bool leaveOpen = false)
        {
            streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen);
        }

        /// <summary>
        /// Reads the next line from the stream without consuming it.
        /// Subsequent calls to <see cref="PeekLine"/> without a <see cref="ReadLine"/> will return the same string.
        /// </summary>
        public string? PeekLine() => peekedLine ??= streamReader.ReadLine();

        /// <summary>
        /// Reads the next line from the stream and consumes it.
        /// If a line was peeked, that same line will then be consumed and returned.
        /// </summary>
        public string? ReadLine()
        {
            try
            {
                return peekedLine ?? streamReader.ReadLine();
            }
            finally
            {
                peekedLine = null;
            }
        }

        /// <summary>
        /// Reads the stream to its end and returns the text read.
        /// Not compatible with calls to <see cref="PeekLine"/>.
        /// </summary>
        public string ReadToEnd()
        {
            if (peekedLine != null)
                throw new InvalidOperationException($"Do not use {nameof(ReadToEnd)} when also peeking for lines.");

            return streamReader.ReadToEnd();
        }

        public void Dispose()
        {
            streamReader.Dispose();
        }
    }
}
