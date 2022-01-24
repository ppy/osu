﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Extensions;

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    [Serializable]
    [Table(@"BeatmapSetInfo")]
    public class EFBeatmapSetInfo : IHasPrimaryKey, IHasFiles<BeatmapSetFileInfo>, ISoftDelete, IEquatable<EFBeatmapSetInfo>, IBeatmapSetInfo
    {
        public int ID { get; set; }

        public bool IsManaged => ID > 0;

        private int? onlineID;

        [Column("OnlineBeatmapSetID")]
        public int? OnlineID
        {
            get => onlineID;
            set => onlineID = value > 0 ? value : null;
        }

        public DateTimeOffset DateAdded { get; set; }

        public EFBeatmapMetadata Metadata { get; set; }

        [NotNull]
        public List<EFBeatmapInfo> Beatmaps { get; } = new List<EFBeatmapInfo>();

        public BeatmapOnlineStatus Status { get; set; } = BeatmapOnlineStatus.None;

        public List<BeatmapSetFileInfo> Files { get; } = new List<BeatmapSetFileInfo>();

        /// <summary>
        /// The maximum star difficulty of all beatmaps in this set.
        /// </summary>
        [JsonIgnore]
        public double MaxStarDifficulty => Beatmaps.Count == 0 ? 0 : Beatmaps.Max(b => b.StarRating);

        /// <summary>
        /// The maximum playable length in milliseconds of all beatmaps in this set.
        /// </summary>
        [JsonIgnore]
        public double MaxLength => Beatmaps.Count == 0 ? 0 : Beatmaps.Max(b => b.Length);

        /// <summary>
        /// The maximum BPM of all beatmaps in this set.
        /// </summary>
        [JsonIgnore]
        public double MaxBPM => Beatmaps.Count == 0 ? 0 : Beatmaps.Max(b => b.BPM);

        [NotMapped]
        public bool DeletePending { get; set; }

        public string Hash { get; set; }

        /// <summary>
        /// Returns the storage path for the file in this beatmapset with the given filename, if any exists, otherwise null.
        /// The path returned is relative to the user file storage.
        /// </summary>
        /// <param name="filename">The name of the file to get the storage path of.</param>
        public string GetPathForFile(string filename) => Files.SingleOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase))?.FileInfo.GetStoragePath();

        public override string ToString() => Metadata?.ToString() ?? base.ToString();

        public bool Protected { get; set; }

        public bool Equals(EFBeatmapSetInfo other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            if (ID != 0 && other.ID != 0)
                return ID == other.ID;

            return false;
        }

        public bool Equals(IBeatmapSetInfo other) => other is EFBeatmapSetInfo b && Equals(b);

        #region Implementation of IHasOnlineID

        int IHasOnlineID<int>.OnlineID => OnlineID ?? -1;

        #endregion

        #region Implementation of IBeatmapSetInfo

        IBeatmapMetadataInfo IBeatmapSetInfo.Metadata => Metadata ?? Beatmaps.FirstOrDefault()?.Metadata ?? new EFBeatmapMetadata();
        IEnumerable<IBeatmapInfo> IBeatmapSetInfo.Beatmaps => Beatmaps;
        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => Files;

        #endregion
    }
}
