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
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    public class BeatmapSetInfo : IHasPrimaryKey, IHasFiles<BeatmapSetFileInfo>, ISoftDelete, IEquatable<BeatmapSetInfo>, IBeatmapSetInfo, IBeatmapSetOnlineInfo
    {
        public int ID { get; set; }

        private int? onlineBeatmapSetID;

        public int? OnlineBeatmapSetID
        {
            get => onlineBeatmapSetID;
            set => onlineBeatmapSetID = value > 0 ? value : null;
        }

        public DateTimeOffset DateAdded { get; set; }

        public BeatmapMetadata Metadata { get; set; }

        public List<BeatmapInfo> Beatmaps { get; set; }

        [NotNull]
        public List<BeatmapSetFileInfo> Files { get; set; } = new List<BeatmapSetFileInfo>();

        [NotMapped]
        public APIBeatmapSet OnlineInfo { get; set; }

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

        #region Implementation of IHasOnlineID

        public int OnlineID => OnlineBeatmapSetID ?? -1;

        #endregion

        #region Implementation of IBeatmapSetInfo

        IBeatmapMetadataInfo IBeatmapSetInfo.Metadata => Metadata;
        IEnumerable<IBeatmapInfo> IBeatmapSetInfo.Beatmaps => Beatmaps;
        IEnumerable<INamedFileUsage> IBeatmapSetInfo.Files => Files;

        #endregion

        #region Delegation for IBeatmapSetOnlineInfo

        [NotMapped]
        [JsonIgnore]
        public DateTimeOffset Submitted => OnlineInfo.Submitted;

        [NotMapped]
        [JsonIgnore]
        public DateTimeOffset? Ranked => OnlineInfo.Ranked;

        [NotMapped]
        [JsonIgnore]
        public DateTimeOffset? LastUpdated => OnlineInfo.LastUpdated;

        [JsonIgnore]
        public BeatmapSetOnlineStatus Status { get; set; } = BeatmapSetOnlineStatus.None;

        [NotMapped]
        [JsonIgnore]
        public bool HasExplicitContent => OnlineInfo.HasExplicitContent;

        [NotMapped]
        [JsonIgnore]
        public bool HasVideo => OnlineInfo.HasVideo;

        [NotMapped]
        [JsonIgnore]
        public bool HasStoryboard => OnlineInfo.HasStoryboard;

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineCovers Covers => OnlineInfo.Covers;

        [NotMapped]
        [JsonIgnore]
        public string Preview => OnlineInfo.Preview;

        [NotMapped]
        [JsonIgnore]
        public double BPM => OnlineInfo.BPM;

        [NotMapped]
        [JsonIgnore]
        public int PlayCount => OnlineInfo.PlayCount;

        [NotMapped]
        [JsonIgnore]
        public int FavouriteCount => OnlineInfo.FavouriteCount;

        [NotMapped]
        [JsonIgnore]
        public bool HasFavourited => OnlineInfo.HasFavourited;

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineAvailability Availability => OnlineInfo.Availability;

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineGenre Genre => OnlineInfo.Genre;

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineLanguage Language => OnlineInfo.Language;

        [NotMapped]
        [JsonIgnore]
        public int? TrackId => OnlineInfo?.TrackId;

        [NotMapped]
        [JsonIgnore]
        public int[] Ratings => OnlineInfo?.Ratings;

        #endregion
    }
}
