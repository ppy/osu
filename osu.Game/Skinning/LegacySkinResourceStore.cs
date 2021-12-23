// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Game.Database;
using osu.Game.Extensions;

namespace osu.Game.Skinning
{
    public class LegacySkinResourceStore : ResourceStore<byte[]>
    {
        private readonly IHasNamedFiles source;

        public LegacySkinResourceStore(IHasNamedFiles source, IResourceStore<byte[]> underlyingStore)
            : base(underlyingStore)
        {
            this.source = source;
        }

        protected override IEnumerable<string> GetFilenames(string name)
        {
            foreach (string filename in base.GetFilenames(name))
            {
                string path = getPathForFile(filename.ToStandardisedPath());
                if (path != null)
                    yield return path;
            }
        }

        private string getPathForFile(string filename) =>
            source.Files.FirstOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase))?.File.GetStoragePath();

        public override IEnumerable<string> GetAvailableResources() => source.Files.Select(f => f.Filename);
    }
}
