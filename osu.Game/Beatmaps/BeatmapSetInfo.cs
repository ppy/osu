// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetInfo : IHasPrimaryKey, IHasFiles<BeatmapSetFileInfo>, ISoftDelete, IEquatable<BeatmapSetInfo>
    {
        public int ID { get; set; }

        private int? onlineBeatmapSetID;

        public int? OnlineBeatmapSetID
        {
            get => onlineBeatmapSetID;
            set => onlineBeatmapSetID = value > 0 ? value : null;
        }

        public DateTimeOffset DateAdded { get; set; }

        public BeatmapSetOnlineStatus Status { get; set; } = BeatmapSetOnlineStatus.None;

        public BeatmapMetadata Metadata { get; set; }

        public List<BeatmapInfo> Beatmaps { get; set; }

        [NotMapped]
        public BeatmapSetOnlineInfo OnlineInfo { get; set; }

        [NotMapped]
        public BeatmapSetMetrics Metrics { get; set; }

        /// <summary>
        /// The maximum star difficulty of all beatmaps in this set.
        /// </summary>
        public double MaxStarDifficulty => Beatmaps?.Max(b => b.StarDifficulty) ?? 0;

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

        public string StoryboardFile => Files?.Find(f => f.Filename.EndsWith(".osb"))?.Filename;

        public List<BeatmapSetFileInfo> Files { get; set; }

        public override string ToString() => Metadata?.ToString() ?? base.ToString();

        public bool Protected { get; set; }

        public bool Equals(BeatmapSetInfo other)
        {
            if (other == null)
                return false;

            if (ID != 0 && other.ID != 0)
                return ID == other.ID;

            if (OnlineBeatmapSetID.HasValue && other.OnlineBeatmapSetID.HasValue)
                return OnlineBeatmapSetID == other.OnlineBeatmapSetID;

            if (!string.IsNullOrEmpty(Hash) && !string.IsNullOrEmpty(other.Hash))
                return Hash == other.Hash;

            return ReferenceEquals(this, other);
        }
    }
}
