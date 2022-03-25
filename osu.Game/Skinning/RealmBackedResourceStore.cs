// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Game.Database;
using osu.Game.Extensions;

namespace osu.Game.Skinning
{
    public class RealmBackedResourceStore : ResourceStore<byte[]>
    {
        private readonly Dictionary<string, string> fileToStoragePathMapping = new Dictionary<string, string>();

        public RealmBackedResourceStore(IHasRealmFiles source, IResourceStore<byte[]> underlyingStore, string[] extensions = null)
            : base(underlyingStore)
        {
            // Must be initialised before the file cache.
            if (extensions != null)
            {
                foreach (string extension in extensions)
                    AddExtension(extension);
            }

            initialiseFileCache(source);
        }

        private void initialiseFileCache(IHasRealmFiles source)
        {
            fileToStoragePathMapping.Clear();
            foreach (var f in source.Files)
                fileToStoragePathMapping[f.Filename.ToLowerInvariant()] = f.File.GetStoragePath();
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
            fileToStoragePathMapping.TryGetValue(filename.ToLower(), out string path) ? path : null;

        public override IEnumerable<string> GetAvailableResources() => fileToStoragePathMapping.Keys;
    }
}
