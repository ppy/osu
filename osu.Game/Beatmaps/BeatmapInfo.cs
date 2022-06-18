﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A single beatmap difficulty.
    /// </summary>
    [ExcludeFromDynamicCompile]
    [Serializable]
    [MapTo("Beatmap")]
    public class BeatmapInfo : RealmObject, IHasGuidPrimaryKey, IBeatmapInfo, IEquatable<BeatmapInfo>
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public string DifficultyName { get; set; } = string.Empty;

        public RulesetInfo Ruleset { get; set; } = null!;

        public BeatmapDifficulty Difficulty { get; set; } = null!;

        public BeatmapMetadata Metadata { get; set; } = null!;

        [JsonIgnore]
        [Backlink(nameof(ScoreInfo.BeatmapInfo))]
        public IQueryable<ScoreInfo> Scores { get; } = null!;

        public BeatmapUserSettings UserSettings { get; set; } = null!;

        public BeatmapInfo(RulesetInfo? ruleset = null, BeatmapDifficulty? difficulty = null, BeatmapMetadata? metadata = null)
        {
            ID = Guid.NewGuid();
            Ruleset = ruleset ?? new RulesetInfo
            {
                OnlineID = 0,
                ShortName = @"osu",
                Name = @"null placeholder ruleset"
            };
            Difficulty = difficulty ?? new BeatmapDifficulty();
            Metadata = metadata ?? new BeatmapMetadata();
            UserSettings = new BeatmapUserSettings();
        }

        [UsedImplicitly]
        private BeatmapInfo()
        {
        }

        public BeatmapSetInfo? BeatmapSet { get; set; }

        [Ignored]
        public RealmNamedFileUsage? File => BeatmapSet?.Files.FirstOrDefault(f => f.File.Hash == Hash);

        [Ignored]
        public BeatmapOnlineStatus Status
        {
            get => (BeatmapOnlineStatus)StatusInt;
            set => StatusInt = (int)value;
        }

        [MapTo(nameof(Status))]
        public int StatusInt { get; set; } = (int)BeatmapOnlineStatus.None;

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public double Length { get; set; }

        public double BPM { get; set; }

        public string Hash { get; set; } = string.Empty;

        public double StarRating { get; set; }

        [Indexed]
        public string MD5Hash { get; set; } = string.Empty;

        [JsonIgnore]
        public bool Hidden { get; set; }

        #region Properties we may not want persisted (but also maybe no harm?)

        public double AudioLeadIn { get; set; }

        public float StackLeniency { get; set; } = 0.7f;

        public bool SpecialStyle { get; set; }

        public bool LetterboxInBreaks { get; set; }

        public bool WidescreenStoryboard { get; set; } = true;

        public bool EpilepsyWarning { get; set; }

        public bool SamplesMatchPlaybackRate { get; set; } = true;

        /// <summary>
        /// The ratio of distance travelled per time unit.
        /// Generally used to decouple the spacing between hit objects from the enforced "velocity" of the beatmap (see <see cref="DifficultyControlPoint.SliderVelocity"/>).
        /// </summary>
        /// <remarks>
        /// The most common method of understanding is that at a default value of 1.0, the time-to-distance ratio will match the slider velocity of the beatmap
        /// at the current point in time. Increasing this value will make hit objects more spaced apart when compared to the cursor movement required to track a slider.
        ///
        /// This is only a hint property, used by the editor in <see cref="IDistanceSnapProvider"/> implementations. It does not directly affect the beatmap or gameplay.
        /// </remarks>
        public double DistanceSpacing { get; set; } = 1.0;

        public int BeatDivisor { get; set; }

        public int GridSize { get; set; }

        public double TimelineZoom { get; set; } = 1.0;

        [Ignored]
        public CountdownType Countdown { get; set; } = CountdownType.Normal;

        /// <summary>
        /// The number of beats to move the countdown backwards (compared to its default location).
        /// </summary>
        public int CountdownOffset { get; set; }

        #endregion

        public bool Equals(BeatmapInfo? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ID == other.ID;
        }

        public bool Equals(IBeatmapInfo? other) => other is BeatmapInfo b && Equals(b);

        public bool AudioEquals(BeatmapInfo? other) => other != null
                                                       && BeatmapSet != null
                                                       && other.BeatmapSet != null
                                                       && BeatmapSet.Hash == other.BeatmapSet.Hash
                                                       && Metadata.AudioFile == other.Metadata.AudioFile;

        public bool BackgroundEquals(BeatmapInfo? other) => other != null
                                                            && BeatmapSet != null
                                                            && other.BeatmapSet != null
                                                            && BeatmapSet.Hash == other.BeatmapSet.Hash
                                                            && Metadata.BackgroundFile == other.Metadata.BackgroundFile;

        IBeatmapMetadataInfo IBeatmapInfo.Metadata => Metadata;
        IBeatmapSetInfo? IBeatmapInfo.BeatmapSet => BeatmapSet;
        IRulesetInfo IBeatmapInfo.Ruleset => Ruleset;
        IBeatmapDifficultyInfo IBeatmapInfo.Difficulty => Difficulty;

        #region Compatibility properties

        [Ignored]
        [Obsolete("Use BeatmapInfo.Difficulty instead.")] // can be removed 20220719
        public BeatmapDifficulty BaseDifficulty
        {
            get => Difficulty;
            set => Difficulty = value;
        }

        [Ignored]
        public string? Path => File?.Filename;

        [Ignored]
        public APIBeatmap? OnlineInfo { get; set; }

        /// <summary>
        /// The maximum achievable combo on this beatmap, populated for online info purposes only.
        /// Todo: This should never be used nor exist, but is still relied on in <see cref="ScoresContainer.Scores"/> since <see cref="IBeatmapInfo"/> can't be used yet. For now this is obsoleted until it is removed.
        /// </summary>
        [Ignored]
        [Obsolete("Use ScoreManager.GetMaximumAchievableComboAsync instead.")]
        public int? MaxCombo { get; set; }

        [Ignored]
        public int[] Bookmarks { get; set; } = Array.Empty<int>();

        public int BeatmapVersion;

        public BeatmapInfo Clone() => (BeatmapInfo)this.Detach().MemberwiseClone();

        public override string ToString() => this.GetDisplayTitle();

        #endregion
    }
}
