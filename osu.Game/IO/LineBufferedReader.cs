// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
        private readonly Queue<string> lineBuffer;

        public LineBufferedReader(Stream stream)
        {
            streamReader = new StreamReader(stream);
            lineBuffer = new Queue<string>();
        }

        /// <summary>
        /// Reads the next line from the stream without consuming it.
        /// Subsequent calls to <see cref="PeekLine"/> without a <see cref="ReadLine"/> will return the same string.
        /// </summary>
        public string PeekLine()
        {
            if (lineBuffer.Count > 0)
                return lineBuffer.Peek();

            var line = streamReader.ReadLine();
            if (line != null)
                lineBuffer.Enqueue(line);
            return line;
        }

        /// <summary>
        /// Reads the next line from the stream and consumes it.
        /// If a line was peeked, that same line will then be consumed and returned.
        /// </summary>
        public string ReadLine() => lineBuffer.Count > 0 ? lineBuffer.Dequeue() : streamReader.ReadLine();

        /// <summary>
        /// Reads the stream to its end and returns the text read.
        /// This includes any peeked but unconsumed lines.
        /// </summary>
        public string ReadToEnd()
        {
            var remainingText = streamReader.ReadToEnd();
            if (lineBuffer.Count == 0)
                return remainingText;

            var builder = new StringBuilder();

            // this might not be completely correct due to varying platform line endings
            while (lineBuffer.Count > 0)
                builder.AppendLine(lineBuffer.Dequeue());
            builder.Append(remainingText);

            return builder.ToString();
        }

        public void Dispose()
        {
            streamReader?.Dispose();
        }
    }
}
