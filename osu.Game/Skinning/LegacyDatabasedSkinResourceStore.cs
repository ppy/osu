// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Development;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Game.Extensions;
using Realms;

namespace osu.Game.Skinning
{
    public class LegacyDatabasedSkinResourceStore : ResourceStore<byte[]>
    {
        private readonly Dictionary<string, string> fileToStoragePathMapping = new Dictionary<string, string>();

        private readonly IDisposable subscription;

        public LegacyDatabasedSkinResourceStore(SkinInfo source, IResourceStore<byte[]> underlyingStore)
            : base(underlyingStore)
        {
            // Subscribing to non-managed instances doesn't work.
            // In this usage, the skin may be non-managed in tests.
            if (source.IsManaged)
            {
                // Subscriptions can only work on the main thread.
                Debug.Assert(ThreadSafety.IsUpdateThread);

                subscription = source.Files
                                     .AsRealmCollection().SubscribeForNotifications((sender, changes, error) =>
                                     {
                                         if (changes == null)
                                             return;

                                         // If a large number of changes are made on skin files, this may be better suited to being cleared here
                                         // and reinitialised on next usage.
                                         initialiseFileCache(source);
                                     });
            }

            initialiseFileCache(source);
        }

        ~LegacyDatabasedSkinResourceStore()
        {
            Dispose(false);
        }

        private void initialiseFileCache(SkinInfo source)
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            subscription?.Dispose();
        }

        private string getPathForFile(string filename) =>
            fileToStoragePathMapping.TryGetValue(filename.ToLower(), out string path) ? path : null;

        public override IEnumerable<string> GetAvailableResources() => fileToStoragePathMapping.Keys;
    }
}
