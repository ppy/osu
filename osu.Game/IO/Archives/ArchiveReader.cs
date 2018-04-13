﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.IO.Stores;

namespace osu.Game.IO.Archives
{
    public abstract class ArchiveReader : IDisposable, IResourceStore<byte[]>
    {
        /// <summary>
        /// Opens a stream for reading a specific file from this archive.
        /// </summary>
        public abstract Stream GetStream(string name);

        public abstract void Dispose();

        /// <summary>
        /// The name of this archive (usually the containing filename).
        /// </summary>
        public readonly string Name;

        protected ArchiveReader(string name)
        {
            Name = name;
        }

        public abstract IEnumerable<string> Filenames { get; }

        public virtual byte[] Get(string name)
        {
            using (Stream input = GetStream(name))
            {
                if (input == null)
                    return null;

                byte[] buffer = new byte[input.Length];
                input.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public abstract Stream GetUnderlyingStream();
    }
}
