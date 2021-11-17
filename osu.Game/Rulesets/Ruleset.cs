// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Users;
using JetBrains.Annotations;
using osu.Framework.Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Testing;
using osu.Game.Extensions;
using osu.Game.Rulesets.Filter;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Rulesets
{
    [ExcludeFromDynamicCompile]
    public abstract class Ruleset
    {
        public RulesetInfo RulesetInfo { get; internal set; }

        private static readonly ConcurrentDictionary<int, IMod[]> mod_reference_cache = new ConcurrentDictionary<int, IMod[]>();

        /// <summary>
        /// A queryable source containing all available mods.
        /// Call <see cref="IMod.CreateInstance"/> for consumption purposes.
        /// </summary>
        public IEnumerable<IMod> AllMods
        {
            get
            {
                if (!(RulesetInfo.ID is int id))
                    return CreateAllMods();

                if (!mod_reference_cache.TryGetValue(id, out var mods))
                    mod_reference_cache[id] = mods = CreateAllMods().Cast<IMod>().ToArray();

                return mods;
            }
        }

        /// <summary>
        /// Returns fresh instances of all mods.
        /// </summary>
        /// <remarks>
        /// This comes with considerable allocation overhead. If only accessing for reference purposes (ie. not changing bindables / settings)
        /// use <see cref="AllMods"/> instead.
        /// </remarks>
        public IEnumerable<Mod> CreateAllMods() => Enum.GetValues(typeof(ModType)).Cast<ModType>()
                                                       // Confine all mods of each mod type into a single IEnumerable<Mod>
                                                       .SelectMany(GetModsFor)
                                                       // Filter out all null mods
                                                       .Where(mod => mod != null)
                                                       // Resolve MultiMods as their .Mods property
                                                       .SelectMany(mod => (mod as MultiMod)?.Mods ?? new[] { mod });

        /// <summary>
        /// Returns a fresh instance of the mod matching the specified acronym.
        /// </summary>
        /// <param name="acronym">The acronym to query for .</param>
        public Mod CreateModFromAcronym(string acronym)
        {
            return AllMods.FirstOrDefault(m => m.Acronym == acronym)?.CreateInstance();
        }

        /// <summary>
        /// Returns a fresh instance of the mod matching the specified type.
        /// </summary>
        public T CreateMod<T>()
            where T : Mod
        {
            return AllMods.FirstOrDefault(m => m is T)?.CreateInstance() as T;
        }

        public abstract IEnumerable<Mod> GetModsFor(ModType type);

        /// <summary>
        /// Converts mods from legacy enum values. Do not override if you're not a legacy ruleset.
        /// </summary>
        /// <param name="mods">The legacy enum which will be converted.</param>
        /// <returns>An enumerable of constructed <see cref="Mod"/>s.</returns>
        public virtual IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods) => Array.Empty<Mod>();

        /// <summary>
        /// Converts mods to legacy enum values. Do not override if you're not a legacy ruleset.
        /// </summary>
        /// <param name="mods">The mods which will be converted.</param>
        /// <returns>A single bitwise enumerable value representing (to the best of our ability) the mods.</returns>
        public virtual LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            var value = LegacyMods.None;

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case ModNoFail _:
                        value |= LegacyMods.NoFail;
                        break;

                    case ModEasy _:
                        value |= LegacyMods.Easy;
                        break;

                    case ModHidden _:
                        value |= LegacyMods.Hidden;
                        break;

                    case ModHardRock _:
                        value |= LegacyMods.HardRock;
                        break;

                    case ModPerfect _:
                        value |= LegacyMods.Perfect;
                        break;

                    case ModSuddenDeath _:
                        value |= LegacyMods.SuddenDeath;
                        break;

                    case ModNightcore _:
                        value |= LegacyMods.Nightcore;
                        break;

                    case ModDoubleTime _:
                        value |= LegacyMods.DoubleTime;
                        break;

                    case ModRelax _:
                        value |= LegacyMods.Relax;
                        break;

                    case ModHalfTime _:
                        value |= LegacyMods.HalfTime;
                        break;

                    case ModFlashlight _:
                        value |= LegacyMods.Flashlight;
                        break;

                    case ModCinema _:
                        value |= LegacyMods.Cinema;
                        break;

                    case ModAutoplay _:
                        value |= LegacyMods.Autoplay;
                        break;
                }
            }

            return value;
        }

        [CanBeNull]
        public ModAutoplay GetAutoplayMod() => CreateMod<ModAutoplay>();

        public virtual ISkin CreateLegacySkinProvider([NotNull] ISkin skin, IBeatmap beatmap) => null;

        protected Ruleset()
        {
            RulesetInfo = new RulesetInfo
            {
                Name = Description,
                ShortName = ShortName,
                ID = (this as ILegacyRuleset)?.LegacyID,
                InstantiationInfo = GetType().GetInvariantInstantiationInfo(),
                Available = true,
            };
        }

        /// <summary>
        /// Attempt to create a hit renderer for a beatmap
        /// </summary>
        /// <param name="beatmap">The beatmap to create the hit renderer for.</param>
        /// <param name="mods">The <see cref="Mod"/>s to apply.</param>
        /// <exception cref="BeatmapInvalidForRulesetException">Unable to successfully load the beatmap to be usable with this ruleset.</exception>
        public abstract DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null);

        /// <summary>
        /// Creates a <see cref="ScoreProcessor"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <returns>The score processor.</returns>
        public virtual ScoreProcessor CreateScoreProcessor() => new ScoreProcessor();

        /// <summary>
        /// Creates a <see cref="HealthProcessor"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <returns>The health processor.</returns>
        public virtual HealthProcessor CreateHealthProcessor(double drainStartTime) => new DrainingHealthProcessor(drainStartTime);

        /// <summary>
        /// Creates a <see cref="IBeatmapConverter"/> to convert a <see cref="IBeatmap"/> to one that is applicable for this <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to be converted.</param>
        /// <returns>The <see cref="IBeatmapConverter"/>.</returns>
        public abstract IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap);

        /// <summary>
        /// Optionally creates a <see cref="IBeatmapProcessor"/> to alter a <see cref="IBeatmap"/> after it has been converted.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to be processed.</param>
        /// <returns>The <see cref="IBeatmapProcessor"/>.</returns>
        public virtual IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => null;

        public abstract DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap);

        /// <summary>
        /// Optionally creates a <see cref="PerformanceCalculator"/> to generate performance data from the provided score.
        /// </summary>
        /// <param name="attributes">Difficulty attributes for the beatmap related to the provided score.</param>
        /// <param name="score">The score to be processed.</param>
        /// <returns>A performance calculator instance for the provided score.</returns>
        [CanBeNull]
        public virtual PerformanceCalculator CreatePerformanceCalculator(DifficultyAttributes attributes, ScoreInfo score) => null;

        /// <summary>
        /// Optionally creates a <see cref="PerformanceCalculator"/> to generate performance data from the provided score.
        /// </summary>
        /// <param name="beatmap">The beatmap to use as a source for generating <see cref="DifficultyAttributes"/>.</param>
        /// <param name="score">The score to be processed.</param>
        /// <returns>A performance calculator instance for the provided score.</returns>
        [CanBeNull]
        public PerformanceCalculator CreatePerformanceCalculator(IWorkingBeatmap beatmap, ScoreInfo score)
        {
            var difficultyCalculator = CreateDifficultyCalculator(beatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(score.Mods);
            return CreatePerformanceCalculator(difficultyAttributes, score);
        }

        public virtual HitObjectComposer CreateHitObjectComposer() => null;

        public virtual IBeatmapVerifier CreateBeatmapVerifier() => null;

        public virtual Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.Solid.QuestionCircle };

        public virtual IResourceStore<byte[]> CreateResourceStore() => new NamespacedResourceStore<byte[]>(new DllResourceStore(GetType().Assembly), @"Resources");

        public abstract string Description { get; }

        public virtual RulesetSettingsSubsection CreateSettings() => null;

        /// <summary>
        /// Creates the <see cref="IRulesetConfigManager"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="settings">The <see cref="SettingsStore"/> to store the settings.</param>
        public virtual IRulesetConfigManager CreateConfig(SettingsStore settings) => null;

        /// <summary>
        /// A unique short name to reference this ruleset in online requests.
        /// </summary>
        public abstract string ShortName { get; }

        /// <summary>
        /// The playing verb to be shown in the <see cref="UserActivity.InGame"/> activities.
        /// </summary>
        public virtual string PlayingVerb => "Playing";

        /// <summary>
        /// A list of available variant ids.
        /// </summary>
        public virtual IEnumerable<int> AvailableVariants => new[] { 0 };

        /// <summary>
        /// Get a list of default keys for the specified variant.
        /// </summary>
        /// <param name="variant">A variant.</param>
        /// <returns>A list of valid <see cref="KeyBinding"/>s.</returns>
        public virtual IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => Array.Empty<KeyBinding>();

        /// <summary>
        /// Gets the name for a key binding variant. This is used for display in the settings overlay.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <returns>A descriptive name of the variant.</returns>
        public virtual string GetVariantName(int variant) => string.Empty;

        /// <summary>
        /// For rulesets which support legacy (osu-stable) replay conversion, this method will create an empty replay frame
        /// for conversion use.
        /// </summary>
        /// <returns>An empty frame for the current ruleset, or null if unsupported.</returns>
        public virtual IConvertibleReplayFrame CreateConvertibleReplayFrame() => null;

        /// <summary>
        /// Creates the statistics for a <see cref="ScoreInfo"/> to be displayed in the results screen.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to create the statistics for. The score is guaranteed to have <see cref="ScoreInfo.HitEvents"/> populated.</param>
        /// <param name="playableBeatmap">The <see cref="IBeatmap"/>, converted for this <see cref="Ruleset"/> with all relevant <see cref="Mod"/>s applied.</param>
        /// <returns>The <see cref="StatisticRow"/>s to display. Each <see cref="StatisticRow"/> may contain 0 or more <see cref="StatisticItem"/>.</returns>
        [NotNull]
        public virtual StatisticRow[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap) => Array.Empty<StatisticRow>();

        /// <summary>
        /// Get all valid <see cref="HitResult"/>s for this ruleset.
        /// Generally used for results display purposes, where it can't be determined if zero-count means the user has not achieved any or the type is not used by this ruleset.
        /// </summary>
        /// <returns>
        /// All valid <see cref="HitResult"/>s along with a display-friendly name.
        /// </returns>
        public IEnumerable<(HitResult result, string displayName)> GetHitResults()
        {
            var validResults = GetValidHitResults();

            // enumerate over ordered list to guarantee return order is stable.
            foreach (var result in EnumExtensions.GetValuesInOrder<HitResult>())
            {
                switch (result)
                {
                    // hard blocked types, should never be displayed even if the ruleset tells us to.
                    case HitResult.None:
                    case HitResult.IgnoreHit:
                    case HitResult.IgnoreMiss:
                    // display is handled as a completion count with corresponding "hit" type.
                    case HitResult.LargeTickMiss:
                    case HitResult.SmallTickMiss:
                        continue;
                }

                if (result == HitResult.Miss || validResults.Contains(result))
                    yield return (result, GetDisplayNameForHitResult(result));
            }
        }

        /// <summary>
        /// Get all valid <see cref="HitResult"/>s for this ruleset.
        /// Generally used for results display purposes, where it can't be determined if zero-count means the user has not achieved any or the type is not used by this ruleset.
        /// </summary>
        /// <remarks>
        /// <see cref="HitResult.Miss"/> is implicitly included. Special types like <see cref="HitResult.IgnoreHit"/> are ignored even when specified.
        /// </remarks>
        protected virtual IEnumerable<HitResult> GetValidHitResults() => EnumExtensions.GetValuesInOrder<HitResult>();

        /// <summary>
        /// Get a display friendly name for the specified result type.
        /// </summary>
        /// <param name="result">The result type to get the name for.</param>
        /// <returns>The display name.</returns>
        public virtual string GetDisplayNameForHitResult(HitResult result) => result.GetDescription();

        /// <summary>
        /// Creates ruleset-specific beatmap filter criteria to be used on the song select screen.
        /// </summary>
        [CanBeNull]
        public virtual IRulesetFilterCriteria CreateRulesetFilterCriteria() => null;

        /// <summary>
        /// Can be overridden to add a ruleset-specific section to the editor beatmap setup screen.
        /// </summary>
        [CanBeNull]
        public virtual RulesetSetupSection CreateEditorSetupSection() => null;
    }
}
