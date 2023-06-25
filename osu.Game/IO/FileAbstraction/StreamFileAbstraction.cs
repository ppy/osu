// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;

namespace osu.Game.IO.FileAbstraction
{
    public class StreamFileAbstraction : TagLib.File.IFileAbstraction
    {
        public StreamFileAbstraction(string filename, Stream fileStream)
        {
            ReadStream = fileStream;
            Name = filename;
        }

        public string Name { get; }

        public Stream ReadStream { get; }
        public Stream WriteStream => ReadStream;

        public void CloseStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            stream.Close();
        }
    }
}
