// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Collections;
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
    /// A realm model containing metadata for a single beatmap difficulty.
    /// This should generally include anything which is required to be filtered on at song select, or anything pertaining to storage of beatmaps in the client.
    /// </summary>
    /// <remarks>
    /// There are some legacy fields in this model which are not persisted to realm. These are isolated in a code region within the class and should eventually be migrated to `Beatmap`.
    /// </remarks>
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

        /// <summary>
        /// Defaults to -1 (meaning not-yet-calculated).
        /// Will likely be superseded with a better storage considering ruleset/mods.
        /// </summary>
        public double StarRating { get; set; } = -1;

        [Indexed]
        public string MD5Hash { get; set; } = string.Empty;

        public string OnlineMD5Hash { get; set; } = string.Empty;

        /// <summary>
        /// The last time of a local modification (via the editor).
        /// </summary>
        public DateTimeOffset? LastLocalUpdate { get; set; }

        /// <summary>
        /// The last time online metadata was applied to this beatmap.
        /// </summary>
        public DateTimeOffset? LastOnlineUpdate { get; set; }

        /// <summary>
        /// Whether this beatmap matches the online version, based on fetched online metadata.
        /// Will return <c>true</c> if no online metadata is available.
        /// </summary>
        public bool MatchesOnlineVersion => LastOnlineUpdate == null || MD5Hash == OnlineMD5Hash;

        [JsonIgnore]
        public bool Hidden { get; set; }

        /// <summary>
        /// Reset any fetched online linking information (and history).
        /// </summary>
        public void ResetOnlineInfo()
        {
            OnlineID = -1;
            LastOnlineUpdate = null;
            OnlineMD5Hash = string.Empty;
            if (Status != BeatmapOnlineStatus.LocallyModified)
                Status = BeatmapOnlineStatus.None;
        }

        #region Properties we may not want persisted (but also maybe no harm?)

        public double AudioLeadIn { get; set; }

        public float StackLeniency { get; set; } = 0.7f;

        public bool SpecialStyle { get; set; }

        public bool LetterboxInBreaks { get; set; }

        public bool WidescreenStoryboard { get; set; } = true;

        public bool EpilepsyWarning { get; set; }

        public bool SamplesMatchPlaybackRate { get; set; } = true;

        /// <summary>
        /// The time at which this beatmap was last played by the local user.
        /// </summary>
        public DateTimeOffset? LastPlayed { get; set; }

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

        public int BeatDivisor { get; set; } = 4;

        public int GridSize { get; set; }

        public double TimelineZoom { get; set; } = 1.0;

        /// <summary>
        /// The time in milliseconds when last exiting the editor with this beatmap loaded.
        /// </summary>
        public double? EditorTimestamp { get; set; }

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
                                                       && compareFiles(this, other, m => m.AudioFile);

        public bool BackgroundEquals(BeatmapInfo? other) => other != null
                                                            && BeatmapSet != null
                                                            && other.BeatmapSet != null
                                                            && compareFiles(this, other, m => m.BackgroundFile);

        private static bool compareFiles(BeatmapInfo x, BeatmapInfo y, Func<IBeatmapMetadataInfo, string> getFilename)
        {
            Debug.Assert(x.BeatmapSet != null);
            Debug.Assert(y.BeatmapSet != null);

            string? fileHashX = x.BeatmapSet.GetFile(getFilename(x.Metadata))?.File.Hash;
            string? fileHashY = y.BeatmapSet.GetFile(getFilename(y.Metadata))?.File.Hash;

            return fileHashX == fileHashY;
        }

        /// <summary>
        /// When updating a beatmap, its hashes will change. Collections currently track beatmaps by hash, so they need to be updated.
        /// This method will handle updating
        /// </summary>
        /// <param name="realm">A realm instance in an active write transaction.</param>
        /// <param name="previousMD5Hash">The previous MD5 hash of the beatmap before update.</param>
        public void TransferCollectionReferences(Realm realm, string previousMD5Hash)
        {
            var collections = realm.All<BeatmapCollection>().AsEnumerable().Where(c => c.BeatmapMD5Hashes.Contains(previousMD5Hash));

            foreach (var c in collections)
            {
                c.BeatmapMD5Hashes.Remove(previousMD5Hash);
                c.BeatmapMD5Hashes.Add(MD5Hash);
            }
        }

        /// <summary>
        /// Local scores are retained separate from a beatmap's lifetime, matched via <see cref="ScoreInfo.BeatmapHash"/>.
        /// Therefore we need to detach / reattach scores when a beatmap is edited or imported.
        /// </summary>
        /// <param name="realm">A realm instance in an active write transaction.</param>
        public void UpdateLocalScores(Realm realm)
        {
            // first disassociate any scores which are already attached and no longer valid.
            foreach (var score in Scores)
                score.BeatmapInfo = null;

            // then attach any scores which match the new hash.
            foreach (var score in realm.All<ScoreInfo>().Where(s => s.BeatmapHash == Hash))
                score.BeatmapInfo = this;
        }

        IBeatmapMetadataInfo IBeatmapInfo.Metadata => Metadata;
        IBeatmapSetInfo? IBeatmapInfo.BeatmapSet => BeatmapSet;
        IRulesetInfo IBeatmapInfo.Ruleset => Ruleset;
        IBeatmapDifficultyInfo IBeatmapInfo.Difficulty => Difficulty;

        #region Compatibility properties

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
