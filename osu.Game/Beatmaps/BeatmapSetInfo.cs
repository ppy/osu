﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Models;
using Realms;

#nullable enable

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    [MapTo("BeatmapSet")]
    public class BeatmapSetInfo : RealmObject, IHasGuidPrimaryKey, IHasRealmFiles, ISoftDelete, IEquatable<BeatmapSetInfo>, IBeatmapSetInfo
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public DateTimeOffset DateAdded { get; set; }

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

        /// <summary>
        /// Returns the storage path for the file in this beatmapset with the given filename, if any exists, otherwise null.
        /// The path returned is relative to the user file storage.
        /// </summary>
        /// <param name="filename">The name of the file to get the storage path of.</param>
        public string? GetPathForFile(string filename) => Files.SingleOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase))?.File.GetStoragePath();

        public bool Equals(BeatmapSetInfo? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ID == other.ID;
        }

        public override string ToString() => Metadata.GetDisplayString();

        public bool Equals(IBeatmapSetInfo? other) => other is BeatmapSetInfo b && Equals(b);

        IEnumerable<IBeatmapInfo> IBeatmapSetInfo.Beatmaps => Beatmaps;

        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => Files;
    }
}
