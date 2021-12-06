// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Game.Database;
using osu.Game.Extensions;
using Realms;

namespace osu.Game.Skinning
{
    public class LegacyDatabasedSkinResourceStore : ResourceStore<byte[]>
    {
        private readonly ILive<SkinInfo> source;

        public LegacyDatabasedSkinResourceStore(SkinInfo source, IResourceStore<byte[]> underlyingStore)
            : base(underlyingStore)
        {
            this.source = source.ToLive();
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
            source.PerformRead(s =>
            {
                if (s.IsManaged)
                {
                    // avoid enumerating all files if this is a managed realm instance.
                    return s.Files.Filter(@"Filename ==[c] $0", filename).FirstOrDefault()?.File.GetStoragePath();
                }

                return s.Files.FirstOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase))?.File.GetStoragePath();
            });

        public override IEnumerable<string> GetAvailableResources() => source.PerformRead(s => s.Files.Select(f => f.Filename));
    }
}
