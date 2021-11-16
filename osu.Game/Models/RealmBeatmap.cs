// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using Realms;

#nullable enable

namespace osu.Game.Models
{
    /// <summary>
    /// A single beatmap difficulty.
    /// </summary>
    [ExcludeFromDynamicCompile]
    [Serializable]
    [MapTo("Beatmap")]
    public class RealmBeatmap : RealmObject, IHasGuidPrimaryKey, IBeatmapInfo, IEquatable<RealmBeatmap>
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string DifficultyName { get; set; } = string.Empty;

        public RealmRuleset Ruleset { get; set; } = null!;

        public RealmBeatmapDifficulty Difficulty { get; set; } = null!;

        public RealmBeatmapMetadata Metadata { get; set; } = null!;

        public RealmBeatmapSet? BeatmapSet { get; set; }

        public BeatmapSetOnlineStatus Status
        {
            get => (BeatmapSetOnlineStatus)StatusInt;
            set => StatusInt = (int)value;
        }

        [MapTo(nameof(Status))]
        public int StatusInt { get; set; }

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public double Length { get; set; }

        public double BPM { get; set; }

        public string Hash { get; set; } = string.Empty;

        public double StarRating { get; set; }

        public string MD5Hash { get; set; } = string.Empty;

        [JsonIgnore]
        public bool Hidden { get; set; }

        public RealmBeatmap(RealmRuleset ruleset, RealmBeatmapDifficulty difficulty, RealmBeatmapMetadata metadata)
        {
            Ruleset = ruleset;
            Difficulty = difficulty;
            Metadata = metadata;
        }

        [UsedImplicitly]
        private RealmBeatmap()
        {
        }

        #region Properties we may not want persisted (but also maybe no harm?)

        public double AudioLeadIn { get; set; }

        public float StackLeniency { get; set; } = 0.7f;

        public bool SpecialStyle { get; set; }

        public bool LetterboxInBreaks { get; set; }

        public bool WidescreenStoryboard { get; set; }

        public bool EpilepsyWarning { get; set; }

        public bool SamplesMatchPlaybackRate { get; set; }

        public double DistanceSpacing { get; set; }

        public int BeatDivisor { get; set; }

        public int GridSize { get; set; }

        public double TimelineZoom { get; set; }

        #endregion

        public bool Equals(RealmBeatmap? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ID == other.ID;
        }

        public bool Equals(IBeatmapInfo? other) => other is RealmBeatmap b && Equals(b);

        public bool AudioEquals(RealmBeatmap? other) => other != null
                                                        && BeatmapSet != null
                                                        && other.BeatmapSet != null
                                                        && BeatmapSet.Hash == other.BeatmapSet.Hash
                                                        && Metadata.AudioFile == other.Metadata.AudioFile;

        public bool BackgroundEquals(RealmBeatmap? other) => other != null
                                                             && BeatmapSet != null
                                                             && other.BeatmapSet != null
                                                             && BeatmapSet.Hash == other.BeatmapSet.Hash
                                                             && Metadata.BackgroundFile == other.Metadata.BackgroundFile;

        IBeatmapMetadataInfo IBeatmapInfo.Metadata => Metadata;
        IBeatmapSetInfo? IBeatmapInfo.BeatmapSet => BeatmapSet;
        IRulesetInfo IBeatmapInfo.Ruleset => Ruleset;
        IBeatmapDifficultyInfo IBeatmapInfo.Difficulty => Difficulty;
    }
}
