// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.IO.Legacy;

namespace osu.Game.Collections
{
    public class CollectionManager
    {
        private const string import_from_stable_path = "collection.db";

        private readonly BeatmapManager beatmaps;

        public CollectionManager(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;
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

            if (!stable.ExistsDirectory(import_from_stable_path))
            {
                // This handles situations like when the user does not have a Skins folder
                Logger.Log($"No {import_from_stable_path} folder available in osu!stable installation", LoggingTarget.Information, LogLevel.Error);
                return Task.CompletedTask;
            }

            return Task.Run(async () => await Import(GetStableImportPaths(GetStableStorage()).Select(f => stable.GetFullPath(f)).ToArray()));
        }

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
                    collection.Beatmaps.Capacity = mapCount;

                    for (int j = 0; j < mapCount; j++)
                    {
                        string checksum = reader.ReadString();

                        var beatmap = beatmaps.QueryBeatmap(b => b.MD5Hash == checksum);
                        if (beatmap != null)
                            collection.Beatmaps.Add(beatmap);
                    }
                }
            }

            return result;
        }
    }

    public class BeatmapCollection
    {
        public string Name;

        public readonly List<BeatmapInfo> Beatmaps = new List<BeatmapInfo>();
    }
}
