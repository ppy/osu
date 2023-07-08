// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Game.Database;
using osu.Game.Extensions;
using Realms;

namespace osu.Game.Skinning
{
    public class RealmBackedResourceStore<T> : ResourceStore<byte[]>
        where T : RealmObject, IHasRealmFiles, IHasGuidPrimaryKey
    {
        private Lazy<Dictionary<string, string>> fileToStoragePathMapping;

        private readonly Live<T> liveSource;
        private readonly IDisposable? realmSubscription;

        public RealmBackedResourceStore(Live<T> source, IResourceStore<byte[]> underlyingStore, RealmAccess? realm)
            : base(underlyingStore)
        {
            liveSource = source;

            invalidateCache();
            Debug.Assert(fileToStoragePathMapping != null);

            realmSubscription = realm?.RegisterForNotifications(r => r.All<T>().Where(s => s.ID == source.ID), skinChanged);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            realmSubscription?.Dispose();
        }

        private void skinChanged(IRealmCollection<T> sender, ChangeSet? changes) => invalidateCache();

        protected override IEnumerable<string> GetFilenames(string name)
        {
            foreach (string filename in base.GetFilenames(name))
            {
                string? path = getPathForFile(filename.ToStandardisedPath());
                if (path != null)
                    yield return path;
            }
        }

        private string? getPathForFile(string filename)
        {
            if (fileToStoragePathMapping.Value.TryGetValue(filename.ToLowerInvariant(), out string? path))
                return path;

            return null;
        }

        private void invalidateCache() => fileToStoragePathMapping = new Lazy<Dictionary<string, string>>(initialiseFileCache);

        private Dictionary<string, string> initialiseFileCache() => liveSource.PerformRead(source =>
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Clear();
            foreach (var f in source.Files)
                dictionary[f.Filename.ToLowerInvariant()] = f.File.GetStoragePath();

            return dictionary;
        });

        public override IEnumerable<string> GetAvailableResources() => fileToStoragePathMapping.Value.Keys;
    }
}
