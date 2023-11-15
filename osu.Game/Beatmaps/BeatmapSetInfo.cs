// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A realm model containing metadata for a beatmap set (containing multiple <see cref="BeatmapInfo"/>s).
    /// </summary>
    [MapTo("BeatmapSet")]
    public class BeatmapSetInfo : RealmObject, IHasGuidPrimaryKey, IHasRealmFiles, ISoftDelete, IEquatable<BeatmapSetInfo>, IBeatmapSetInfo, IDeepCloneable<BeatmapSetInfo>
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public DateTimeOffset DateAdded { get; set; }

        /// <summary>
        /// The date this beatmap set was first submitted.
        /// </summary>
        public DateTimeOffset? DateSubmitted { get; set; }

        /// <summary>
        /// The date this beatmap set was ranked.
        /// </summary>
        public DateTimeOffset? DateRanked { get; set; }

        [JsonIgnore]
        public IBeatmapMetadataInfo Metadata => Beatmaps.FirstOrDefault()?.Metadata ?? new BeatmapMetadata();

        public IList<BeatmapInfo> Beatmaps { get; } = null!;

        public IList<RealmNamedFileUsage> Files { get; } = null!;

        [Ignored]
        public BeatmapOnlineStatus Status
        {
            get => (BeatmapOnlineStatus)StatusInt;
            set => StatusInt = (int)value;
        }

        [MapTo(nameof(Status))]
        public int StatusInt { get; set; } = (int)BeatmapOnlineStatus.None;

        public bool DeletePending { get; set; }

        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Whether deleting this beatmap set should be prohibited (due to it being a system requirement to be present).
        /// </summary>
        public bool Protected { get; set; }

        public double MaxStarDifficulty => Beatmaps.Count == 0 ? 0 : Beatmaps.Max(b => b.StarRating);

        public double MaxLength => Beatmaps.Count == 0 ? 0 : Beatmaps.Max(b => b.Length);

        public double MaxBPM => Beatmaps.Count == 0 ? 0 : Beatmaps.Max(b => b.BPM);

        public BeatmapSetInfo(IEnumerable<BeatmapInfo>? beatmaps = null)
            : this()
        {
            ID = Guid.NewGuid();
            if (beatmaps != null)
                Beatmaps.AddRange(beatmaps);
        }

        [UsedImplicitly] // Realm
        private BeatmapSetInfo()
        {
        }

        public bool Equals(BeatmapSetInfo? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ID == other.ID;
        }

        public override string ToString() => Metadata.GetDisplayString();

        public bool Equals(IBeatmapSetInfo? other) => other is BeatmapSetInfo b && Equals(b);

        public BeatmapSetInfo DeepClone(IDictionary<object, object> referenceLookup)
        {
            if (referenceLookup.TryGetValue(this, out object? existing))
                return (BeatmapSetInfo)existing;

            var clone = this.Detach();

            if (ReferenceEquals(clone, this))
            {
                clone = new BeatmapSetInfo
                {
                    ID = ID,
                    OnlineID = OnlineID,
                    DateAdded = DateAdded,
                    DateSubmitted = DateSubmitted,
                    DateRanked = DateRanked,
                    StatusInt = StatusInt,
                    DeletePending = DeletePending,
                    Hash = Hash,
                    Protected = Protected
                };
            }

            referenceLookup[this] = clone;

            clone.Beatmaps.AddRange(Beatmaps.Select(b => b.DeepClone(referenceLookup)));
            clone.Files.AddRange(Files.Select(f => f.DeepClone(referenceLookup)));

            return clone;
        }

        IEnumerable<IBeatmapInfo> IBeatmapSetInfo.Beatmaps => Beatmaps;

        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => Files;

        public bool AllBeatmapsUpToDate => Beatmaps.All(b => b.MatchesOnlineVersion);
    }
}
