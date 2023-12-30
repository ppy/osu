// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.IO.Stores;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Filter;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;
using osu.Game.Users;

namespace osu.Game.Rulesets
{
    public abstract class Ruleset
    {
        public RulesetInfo RulesetInfo { get; }

        private static readonly ConcurrentDictionary<string, IMod[]> mod_reference_cache = new ConcurrentDictionary<string, IMod[]>();

        /// <summary>
        /// Version history:
        /// 2022.205.0   FramedReplayInputHandler.CollectPendingInputs renamed to FramedReplayHandler.CollectReplayInputs.
        /// 2022.822.0   All strings return values have been converted to LocalisableString to allow for localisation support.
        /// </summary>
        public const string CURRENT_RULESET_API_VERSION = "2022.822.0";

        /// <summary>
        /// Define the ruleset API version supported by this ruleset.
        /// Ruleset implementations should be updated to support the latest version to ensure they can still be loaded.
        /// </summary>
        /// <remarks>
        /// Generally, all ruleset implementations should point this directly to <see cref="CURRENT_RULESET_API_VERSION"/>.
        /// This will ensure that each time you compile a new release, it will pull in the most recent version.
        /// See https://github.com/ppy/osu/wiki/Breaking-Changes for full details on required ongoing changes.
        /// </remarks>
        public virtual string RulesetAPIVersionSupported => string.Empty;

        /// <summary>
        /// A queryable source containing all available mods.
        /// Call <see cref="IMod.CreateInstance"/> for consumption purposes.
        /// </summary>
        public IEnumerable<IMod> AllMods
        {
            get
            {
                // Is the case for many test usages.
                if (string.IsNullOrEmpty(ShortName))
                    return CreateAllMods();

                if (!mod_reference_cache.TryGetValue(ShortName, out var mods))
                    mod_reference_cache[ShortName] = mods = CreateAllMods().Cast<IMod>().ToArray();

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
        public IEnumerable<Mod> CreateAllMods() => Enum.GetValues<ModType>()
                                                       // Confine all mods of each mod type into a single IEnumerable<Mod>
                                                       .SelectMany(GetModsFor)
                                                       // Filter out all null mods
                                                       // This is to handle old rulesets which were doing mods bad. Can be removed at some point we are sure nulls will not appear here.
                                                       // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                                                       .Where(mod => mod != null)
                                                       // Resolve MultiMods as their .Mods property
                                                       .SelectMany(mod => (mod as MultiMod)?.Mods ?? new[] { mod });

        /// <summary>
        /// Returns a fresh instance of the mod matching the specified acronym.
        /// </summary>
        /// <param name="acronym">The acronym to query for .</param>
        public Mod? CreateModFromAcronym(string acronym)
        {
            return AllMods.FirstOrDefault(m => m.Acronym == acronym)?.CreateInstance();
        }

        /// <summary>
        /// Returns a fresh instance of the mod matching the specified type.
        /// </summary>
        public T? CreateMod<T>()
            where T : Mod
        {
            return AllMods.FirstOrDefault(m => m is T)?.CreateInstance() as T;
        }

        /// <summary>
        /// Creates an enumerable with mods that are supported by the ruleset for the supplied <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// If there are no applicable mods from the given <paramref name="type"/> in this ruleset,
        /// then the proper behaviour is to return an empty enumerable.
        /// <see langword="null"/> mods should not be present in the returned enumerable.
        /// </remarks>
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
                    case ModNoFail:
                        value |= LegacyMods.NoFail;
                        break;

                    case ModEasy:
                        value |= LegacyMods.Easy;
                        break;

                    case ModHidden:
                        value |= LegacyMods.Hidden;
                        break;

                    case ModHardRock:
                        value |= LegacyMods.HardRock;
                        break;

                    case ModPerfect:
                        value |= LegacyMods.Perfect | LegacyMods.SuddenDeath;
                        break;

                    case ModSuddenDeath:
                        value |= LegacyMods.SuddenDeath;
                        break;

                    case ModNightcore:
                        value |= LegacyMods.Nightcore | LegacyMods.DoubleTime;
                        break;

                    case ModDoubleTime:
                        value |= LegacyMods.DoubleTime;
                        break;

                    case ModRelax:
                        value |= LegacyMods.Relax;
                        break;

                    case ModHalfTime:
                        value |= LegacyMods.HalfTime;
                        break;

                    case ModFlashlight:
                        value |= LegacyMods.Flashlight;
                        break;

                    case ModCinema:
                        value |= LegacyMods.Cinema | LegacyMods.Autoplay;
                        break;

                    case ModAutoplay:
                        value |= LegacyMods.Autoplay;
                        break;

                    case ModScoreV2:
                        value |= LegacyMods.ScoreV2;
                        break;
                }
            }

            return value;
        }

        public ModAutoplay? GetAutoplayMod() => CreateMod<ModAutoplay>();

        public ModTouchDevice? GetTouchDeviceMod() => CreateMod<ModTouchDevice>();

        /// <summary>
        /// Create a transformer which adds lookups specific to a ruleset to skin sources.
        /// </summary>
        /// <param name="skin">The source skin.</param>
        /// <param name="beatmap">The current beatmap.</param>
        /// <returns>A skin with a transformer applied, or null if no transformation is provided by this ruleset.</returns>
        public virtual ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case LegacySkin:
                    return new LegacySkinTransformer(skin);

                case ArgonSkin:
                    return new ArgonSkinTransformer(skin);
            }

            return null;
        }

        protected Ruleset()
        {
            RulesetInfo = new RulesetInfo
            {
                Name = Description,
                ShortName = ShortName,
                OnlineID = (this as ILegacyRuleset)?.LegacyID ?? -1,
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
        public abstract DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null);

        /// <summary>
        /// Creates a <see cref="ScoreProcessor"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <returns>The score processor.</returns>
        public virtual ScoreProcessor CreateScoreProcessor() => new ScoreProcessor(this);

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
        public virtual IBeatmapProcessor? CreateBeatmapProcessor(IBeatmap beatmap) => null;

        public abstract DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap);

        /// <summary>
        /// Optionally creates a <see cref="PerformanceCalculator"/> to generate performance data from the provided score.
        /// </summary>
        /// <returns>A performance calculator instance for the provided score.</returns>
        public virtual PerformanceCalculator? CreatePerformanceCalculator() => null;

        public virtual HitObjectComposer? CreateHitObjectComposer() => null;

        public virtual IBeatmapVerifier? CreateBeatmapVerifier() => null;

        public virtual Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.Solid.QuestionCircle };

        public virtual IResourceStore<byte[]> CreateResourceStore() => new NamespacedResourceStore<byte[]>(new DllResourceStore(GetType().Assembly), @"Resources");

        public abstract string Description { get; }

        public virtual RulesetSettingsSubsection? CreateSettings() => null;

        /// <summary>
        /// Creates the <see cref="IRulesetConfigManager"/> for this <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="settings">The <see cref="SettingsStore"/> to store the settings.</param>
        public virtual IRulesetConfigManager? CreateConfig(SettingsStore? settings) => null;

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
        public virtual LocalisableString GetVariantName(int variant) => string.Empty;

        /// <summary>
        /// For rulesets which support legacy (osu-stable) replay conversion, this method will create an empty replay frame
        /// for conversion use.
        /// </summary>
        /// <returns>An empty frame for the current ruleset, or null if unsupported.</returns>
        public virtual IConvertibleReplayFrame? CreateConvertibleReplayFrame() => null;

        /// <summary>
        /// Creates the statistics for a <see cref="ScoreInfo"/> to be displayed in the results screen.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to create the statistics for. The score is guaranteed to have <see cref="ScoreInfo.HitEvents"/> populated.</param>
        /// <param name="playableBeatmap">The <see cref="IBeatmap"/>, converted for this <see cref="Ruleset"/> with all relevant <see cref="Mod"/>s applied.</param>
        /// <returns>The <see cref="StatisticItem"/>s to display.</returns>
        public virtual StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap) => Array.Empty<StatisticItem>();

        /// <summary>
        /// Get all valid <see cref="HitResult"/>s for this ruleset.
        /// Generally used for results display purposes, where it can't be determined if zero-count means the user has not achieved any or the type is not used by this ruleset.
        /// </summary>
        /// <returns>
        /// All valid <see cref="HitResult"/>s along with a display-friendly name.
        /// </returns>
        public IEnumerable<(HitResult result, LocalisableString displayName)> GetHitResults()
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
        public virtual LocalisableString GetDisplayNameForHitResult(HitResult result) => result.GetLocalisableDescription();

        /// <summary>
        /// Applies changes to difficulty attributes for presenting to a user a rough estimate of how rate adjust mods affect difficulty.
        /// Importantly, this should NOT BE USED FOR ANY CALCULATIONS.
        ///
        /// It is also not always correct, and arguably is never correct depending on your frame of mind.
        /// </summary>
        /// <param name="difficulty">>The <see cref="IBeatmapDifficultyInfo"/> that will be adjusted.</param>
        /// <param name="rate">The rate adjustment multiplier from mods. For example 1.5 for DT.</param>
        /// <returns>The adjusted difficulty attributes.</returns>
        public virtual BeatmapDifficulty GetRateAdjustedDisplayDifficulty(IBeatmapDifficultyInfo difficulty, double rate) => new BeatmapDifficulty(difficulty);

        /// <summary>
        /// Creates ruleset-specific beatmap filter criteria to be used on the song select screen.
        /// </summary>
        public virtual IRulesetFilterCriteria? CreateRulesetFilterCriteria() => null;

        /// <summary>
        /// Can be overridden to add a ruleset-specific section to the editor beatmap setup screen.
        /// </summary>
        public virtual RulesetSetupSection? CreateEditorSetupSection() => null;

        /// <summary>
        /// Can be overridden to alter the difficulty section to the editor beatmap setup screen.
        /// </summary>
        public virtual DifficultySection? CreateEditorDifficultySection() => null;
    }
}
