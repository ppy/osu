// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Testing;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    public class BeatmapSetInfo : IHasPrimaryKey, IHasFiles<BeatmapSetFileInfo>, ISoftDelete, IEquatable<BeatmapSetInfo>, IBeatmapSetInfo
    {
        public int ID { get; set; }

        private int? onlineID;

        [Column("OnlineBeatmapSetID")]
        public int? OnlineID
        {
            get => onlineID;
            set => onlineID = value > 0 ? value : null;
        }

        public DateTimeOffset DateAdded { get; set; }

        public BeatmapMetadata Metadata { get; set; }

        public List<BeatmapInfo> Beatmaps { get; set; }

        public BeatmapSetOnlineStatus Status { get; set; } = BeatmapSetOnlineStatus.None;

        [NotNull]
        public List<BeatmapSetFileInfo> Files { get; set; } = new List<BeatmapSetFileInfo>();

        /// <summary>
        /// The maximum star difficulty of all beatmaps in this set.
        /// </summary>
        public double MaxStarDifficulty => Beatmaps?.Max(b => b.StarRating) ?? 0;

        /// <summary>
        /// The maximum playable length in milliseconds of all beatmaps in this set.
        /// </summary>
        public double MaxLength => Beatmaps?.Max(b => b.Length) ?? 0;

        /// <summary>
        /// The maximum BPM of all beatmaps in this set.
        /// </summary>
        public double MaxBPM => Beatmaps?.Max(b => b.BPM) ?? 0;

        [NotMapped]
        public bool DeletePending { get; set; }

        public string Hash { get; set; }

        /// <summary>
        /// Returns the storage path for the file in this beatmapset with the given filename, if any exists, otherwise null.
        /// The path returned is relative to the user file storage.
        /// </summary>
        /// <param name="filename">The name of the file to get the storage path of.</param>
        public string GetPathForFile(string filename) => Files.SingleOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase))?.FileInfo.StoragePath;

        public override string ToString() => Metadata?.ToString() ?? base.ToString();

        public bool Protected { get; set; }

        public bool Equals(BeatmapSetInfo other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            if (ID != 0 && other.ID != 0)
                return ID == other.ID;

            return false;
        }

        public bool Equals(IBeatmapSetInfo other) => other is BeatmapSetInfo b && Equals(b);

        #region Implementation of IHasOnlineID

        int IHasOnlineID<int>.OnlineID => OnlineID ?? -1;

        #endregion

        #region Implementation of IBeatmapSetInfo

        IBeatmapMetadataInfo IBeatmapSetInfo.Metadata => Metadata;
        IEnumerable<IBeatmapInfo> IBeatmapSetInfo.Beatmaps => Beatmaps;
        IEnumerable<INamedFileUsage> IBeatmapSetInfo.Files => Files;

        #endregion
    }
}
