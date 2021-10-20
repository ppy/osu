// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using Realms;

#nullable enable

namespace osu.Game.Models
{
    [ExcludeFromDynamicCompile]
    [MapTo("BeatmapSet")]
    public class RealmBeatmapSet : RealmObject, IHasGuidPrimaryKey, IHasRealmFiles, ISoftDelete, IEquatable<RealmBeatmapSet>, IBeatmapSetInfo
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public DateTimeOffset DateAdded { get; set; }

        public IBeatmapMetadataInfo? Metadata => Beatmaps.FirstOrDefault()?.Metadata;

        public IList<RealmBeatmap> Beatmaps { get; } = null!;

        public IList<RealmNamedFileUsage> Files { get; } = null!;

        public bool DeletePending { get; set; }

        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Whether deleting this beatmap set should be prohibited (due to it being a system requirement to be present).
        /// </summary>
        public bool Protected { get; set; }

        public double MaxStarDifficulty => Beatmaps.Max(b => b.StarRating);

        public double MaxLength => Beatmaps.Max(b => b.Length);

        public double MaxBPM => Beatmaps.Max(b => b.BPM);

        /// <summary>
        /// Returns the storage path for the file in this beatmapset with the given filename, if any exists, otherwise null.
        /// The path returned is relative to the user file storage.
        /// </summary>
        /// <param name="filename">The name of the file to get the storage path of.</param>
        public string? GetPathForFile(string filename) => Files.SingleOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase))?.File.StoragePath;

        public override string ToString() => Metadata?.ToString() ?? base.ToString();

        public bool Equals(RealmBeatmapSet? other)
        {
            if (other == null)
                return false;

            if (IsManaged && other.IsManaged)
                return ID == other.ID;

            if (OnlineID > 0 && other.OnlineID > 0)
                return OnlineID == other.OnlineID;

            if (!string.IsNullOrEmpty(Hash) && !string.IsNullOrEmpty(other.Hash))
                return Hash == other.Hash;

            return ReferenceEquals(this, other);
        }

        IEnumerable<IBeatmapInfo> IBeatmapSetInfo.Beatmaps => Beatmaps;

        IEnumerable<INamedFileUsage> IBeatmapSetInfo.Files => Files;
    }
}
