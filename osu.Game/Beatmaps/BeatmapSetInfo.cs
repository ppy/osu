// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        public DateTimeOffset Submitted
        {
            get => OnlineInfo.Submitted;
            set => OnlineInfo.Submitted = value;
        }

        [NotMapped]
        [JsonIgnore]
        public DateTimeOffset? Ranked
        {
            get => OnlineInfo.Ranked;
            set => OnlineInfo.Ranked = value;
        }

        [NotMapped]
        [JsonIgnore]
        public DateTimeOffset? LastUpdated
        {
            get => OnlineInfo.LastUpdated;
            set => OnlineInfo.LastUpdated = value;
        }

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineStatus Status { get; set; } = BeatmapSetOnlineStatus.None;

        [NotMapped]
        [JsonIgnore]
        public bool HasExplicitContent
        {
            get => OnlineInfo.HasExplicitContent;
            set => OnlineInfo.HasExplicitContent = value;
        }

        [NotMapped]
        [JsonIgnore]
        public bool HasVideo
        {
            get => OnlineInfo.HasVideo;
            set => OnlineInfo.HasVideo = value;
        }

        [NotMapped]
        [JsonIgnore]
        public bool HasStoryboard
        {
            get => OnlineInfo.HasStoryboard;
            set => OnlineInfo.HasStoryboard = value;
        }

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineCovers Covers
        {
            get => OnlineInfo.Covers;
            set => OnlineInfo.Covers = value;
        }

        [NotMapped]
        [JsonIgnore]
        public string Preview
        {
            get => OnlineInfo.Preview;
            set => OnlineInfo.Preview = value;
        }

        [NotMapped]
        [JsonIgnore]
        public double BPM
        {
            get => OnlineInfo.BPM;
            set => OnlineInfo.BPM = value;
        }

        [NotMapped]
        [JsonIgnore]
        public int PlayCount
        {
            get => OnlineInfo.PlayCount;
            set => OnlineInfo.PlayCount = value;
        }

        [NotMapped]
        [JsonIgnore]
        public int FavouriteCount
        {
            get => OnlineInfo.FavouriteCount;
            set => OnlineInfo.FavouriteCount = value;
        }

        [NotMapped]
        [JsonIgnore]
        public bool HasFavourited
        {
            get => OnlineInfo.HasFavourited;
            set => OnlineInfo.HasFavourited = value;
        }

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineAvailability Availability
        {
            get => OnlineInfo.Availability;
            set => OnlineInfo.Availability = value;
        }

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineGenre Genre
        {
            get => OnlineInfo.Genre;
            set => OnlineInfo.Genre = value;
        }

        [NotMapped]
        [JsonIgnore]
        public BeatmapSetOnlineLanguage Language
        {
            get => OnlineInfo.Language;
            set => OnlineInfo.Language = value;
        }

        [NotMapped]
        [JsonIgnore]
        public int? TrackId
        {
            get => OnlineInfo.TrackId;
            set => OnlineInfo.TrackId = value;
        }

        #endregion
    }
}
