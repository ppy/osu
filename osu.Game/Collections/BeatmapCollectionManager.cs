// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.IO.Legacy;

namespace osu.Game.Collections
{
    public class BeatmapCollectionManager : CompositeDrawable
    {
        /// <summary>
        /// Database version in YYYYMMDD format (matching stable).
        /// </summary>
        private const int database_version = 30000000;

        private const string database_name = "collection.db";

        public readonly BindableList<BeatmapCollection> Collections = new BindableList<BeatmapCollection>();

        public bool SupportsImportFromStable => RuntimeInfo.IsDesktop;

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (host.Storage.Exists(database_name))
            {
                using (var stream = host.Storage.GetStream(database_name))
                    importCollections(readCollections(stream));
            }

            foreach (var c in Collections)
                c.Changed += backgroundSave;
            Collections.CollectionChanged += (_, __) => backgroundSave();
        }

        /// <summary>
        /// Set a storage with access to an osu-stable install for import purposes.
        /// </summary>
        public Func<Storage> GetStableStorage { private get; set; }

        /// <summary>
        /// This is a temporary method and will likely be replaced by a full-fledged (and more correctly placed) migration process in the future.
        /// </summary>
        public Task ImportFromStableAsync()
        {
            var stable = GetStableStorage?.Invoke();

            if (stable == null)
            {
                Logger.Log("No osu!stable installation available!", LoggingTarget.Information, LogLevel.Error);
                return Task.CompletedTask;
            }

            if (!stable.Exists(database_name))
            {
                // This handles situations like when the user does not have a collections.db file
                Logger.Log($"No {database_name} available in osu!stable installation", LoggingTarget.Information, LogLevel.Error);
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                var storage = GetStableStorage();

                if (storage.Exists(database_name))
                {
                    using (var stream = storage.GetStream(database_name))
                    {
                        var collection = readCollections(stream);
                        Schedule(() => importCollections(collection));
                    }
                }
            });
        }

        private void importCollections(List<BeatmapCollection> newCollections)
        {
            foreach (var newCol in newCollections)
            {
                var existing = Collections.FirstOrDefault(c => c.Name == newCol.Name);
                if (existing == null)
                    Collections.Add(existing = new BeatmapCollection { Name = { Value = newCol.Name.Value } });

                foreach (var newBeatmap in newCol.Beatmaps)
                {
                    if (!existing.Beatmaps.Contains(newBeatmap))
                        existing.Beatmaps.Add(newBeatmap);
                }
            }
        }

        private List<BeatmapCollection> readCollections(Stream stream)
        {
            var result = new List<BeatmapCollection>();

            try
            {
                using (var sr = new SerializationReader(stream))
                {
                    sr.ReadInt32(); // Version

                    int collectionCount = sr.ReadInt32();
                    result.Capacity = collectionCount;

                    for (int i = 0; i < collectionCount; i++)
                    {
                        var collection = new BeatmapCollection { Name = { Value = sr.ReadString() } };
                        int mapCount = sr.ReadInt32();

                        for (int j = 0; j < mapCount; j++)
                        {
                            string checksum = sr.ReadString();

                            var beatmap = beatmaps.QueryBeatmap(b => b.MD5Hash == checksum);
                            if (beatmap != null)
                                collection.Beatmaps.Add(beatmap);
                        }

                        result.Add(collection);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to read collection database.");
            }

            return result;
        }

        private readonly object saveLock = new object();
        private int lastSave;
        private int saveFailures;

        /// <summary>
        /// Perform a save with debounce.
        /// </summary>
        private void backgroundSave()
        {
            var current = Interlocked.Increment(ref lastSave);
            Task.Delay(100).ContinueWith(task =>
            {
                if (current != lastSave)
                    return;

                if (!save())
                    backgroundSave();
            });
        }

        private bool save()
        {
            lock (saveLock)
            {
                Interlocked.Increment(ref lastSave);

                try
                {
                    // This is NOT thread-safe!!

                    using (var sw = new SerializationWriter(host.Storage.GetStream(database_name, FileAccess.Write)))
                    {
                        sw.Write(database_version);
                        sw.Write(Collections.Count);

                        foreach (var c in Collections)
                        {
                            sw.Write(c.Name.Value);
                            sw.Write(c.Beatmaps.Count);

                            foreach (var b in c.Beatmaps)
                                sw.Write(b.MD5Hash);
                        }
                    }

                    if (saveFailures < 10)
                        saveFailures = 0;
                    return true;
                }
                catch (Exception e)
                {
                    // Since this code is not thread-safe, we may run into random exceptions (such as collection enumeration or out of range indexing).
                    // Failures are thus only alerted if they exceed a threshold (once) to indicate "actual" errors having occurred.
                    if (++saveFailures == 10)
                        Logger.Error(e, "Failed to save collection database!");
                }

                return false;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            save();
        }
    }

    public class BeatmapCollection
    {
        /// <summary>
        /// Invoked whenever any change occurs on this <see cref="BeatmapCollection"/>.
        /// </summary>
        public event Action Changed;

        public readonly Bindable<string> Name = new Bindable<string>();

        public readonly BindableList<BeatmapInfo> Beatmaps = new BindableList<BeatmapInfo>();

        public DateTimeOffset LastModifyTime { get; private set; }

        public BeatmapCollection()
        {
            LastModifyTime = DateTimeOffset.UtcNow;

            Beatmaps.CollectionChanged += (_, __) =>
            {
                LastModifyTime = DateTimeOffset.Now;
                Changed?.Invoke();
            };
        }
    }
}
