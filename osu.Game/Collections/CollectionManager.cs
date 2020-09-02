// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.IO.Legacy;

namespace osu.Game.Collections
{
    public class CollectionManager : CompositeDrawable
    {
        /// <summary>
        /// Database version in YYYYMMDD format (matching stable).
        /// </summary>
        private const int database_version = 30000000;

        private const string database_name = "collection.db";

        [Resolved]
        private GameHost host { get; set; }

        public IBindableList<BeatmapCollection> Collections => collections;
        private readonly BindableList<BeatmapCollection> collections = new BindableList<BeatmapCollection>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (host.Storage.Exists(database_name))
            {
                using (var stream = host.Storage.GetStream(database_name))
                    collections.AddRange(readCollection(stream));
            }

            foreach (var c in collections)
                c.Changed += backgroundSave;
            collections.CollectionChanged += (_, __) => backgroundSave();
        }

        /// <summary>
        /// Set a storage with access to an osu-stable install for import purposes.
        /// </summary>
        public Func<Storage> GetStableStorage { private get; set; }

        /// <summary>
        /// This is a temporary method and will likely be replaced by a full-fledged (and more correctly placed) migration process in the future.
        /// </summary>
        // public Task ImportFromStableAsync()
        // {
        //     var stable = GetStableStorage?.Invoke();
        //
        //     if (stable == null)
        //     {
        //         Logger.Log("No osu!stable installation available!", LoggingTarget.Information, LogLevel.Error);
        //         return Task.CompletedTask;
        //     }
        //
        //     if (!stable.ExistsDirectory(database_name))
        //     {
        //         // This handles situations like when the user does not have a Skins folder
        //         Logger.Log($"No {database_name} folder available in osu!stable installation", LoggingTarget.Information, LogLevel.Error);
        //         return Task.CompletedTask;
        //     }
        //
        //     return Task.Run(async () => await Import(GetStableImportPaths(GetStableStorage()).Select(f => stable.GetFullPath(f)).ToArray()));
        // }
        private List<BeatmapCollection> readCollection(Stream stream)
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
                        var collection = new BeatmapCollection { Name = sr.ReadString() };
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
                        sw.Write(collections.Count);

                        foreach (var c in collections)
                        {
                            sw.Write(c.Name);
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

        public string Name;

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
