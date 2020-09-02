// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.IO.Legacy;

namespace osu.Game.Collections
{
    public class CollectionManager : CompositeDrawable
    {
        private const string database_name = "collection.db";

        public IBindableList<BeatmapCollection> Collections => collections;
        private readonly BindableList<BeatmapCollection> collections = new BindableList<BeatmapCollection>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            if (host.Storage.Exists(database_name))
            {
                using (var stream = host.Storage.GetStream(database_name))
                    collections.AddRange(readCollection(stream));
            }
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

            using (var reader = new SerializationReader(stream))
            {
                reader.ReadInt32(); // Version

                int collectionCount = reader.ReadInt32();
                result.Capacity = collectionCount;

                for (int i = 0; i < collectionCount; i++)
                {
                    var collection = new BeatmapCollection { Name = reader.ReadString() };
                    int mapCount = reader.ReadInt32();

                    for (int j = 0; j < mapCount; j++)
                    {
                        string checksum = reader.ReadString();

                        var beatmap = beatmaps.QueryBeatmap(b => b.MD5Hash == checksum);
                        if (beatmap != null)
                            collection.Beatmaps.Add(beatmap);
                    }

                    result.Add(collection);
                }
            }

            return result;
        }
    }

    public class BeatmapCollection
    {
        public string Name;

        public readonly BindableList<BeatmapInfo> Beatmaps = new BindableList<BeatmapInfo>();

        public DateTimeOffset LastModifyTime { get; private set; }

        public BeatmapCollection()
        {
            LastModifyTime = DateTimeOffset.UtcNow;

            Beatmaps.CollectionChanged += (_, __) => LastModifyTime = DateTimeOffset.Now;
        }
    }
}
