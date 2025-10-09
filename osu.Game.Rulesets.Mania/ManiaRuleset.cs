// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Filter;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.Edit.Setup;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Skinning.Argon;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Mania.Skinning.Legacy;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaRuleset : Ruleset, ILegacyRuleset
    {
        /// <summary>
        /// The maximum number of supported keys in a single stage.
        /// </summary>
        public const int MAX_STAGE_KEYS = 10;

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => new DrawableManiaRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor();

        public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new ManiaHealthProcessor(drainStartTime);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new ManiaBeatmapConverter(beatmap, this);

        public override PerformanceCalculator CreatePerformanceCalculator() => new ManiaPerformanceCalculator();

        public const string SHORT_NAME = "mania";

        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;

        public override HitObjectComposer CreateHitObjectComposer() => new ManiaHitObjectComposer(this);

        public override IBeatmapVerifier CreateBeatmapVerifier() => new ManiaBeatmapVerifier();

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case TrianglesSkin:
                    return new ManiaTrianglesSkinTransformer(skin, beatmap);

                case ArgonSkin:
                    return new ManiaArgonSkinTransformer(skin, beatmap);

                case DefaultLegacySkin:
                case RetroSkin:
                    return new ManiaClassicSkinTransformer(skin, beatmap);

                case LegacySkin:
                    return new ManiaLegacySkinTransformer(skin, beatmap);
            }

            return null;
        }

        public override IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlag(LegacyMods.Nightcore))
                yield return new ManiaModNightcore();
            else if (mods.HasFlag(LegacyMods.DoubleTime))
                yield return new ManiaModDoubleTime();

            if (mods.HasFlag(LegacyMods.Perfect))
                yield return new ManiaModPerfect();
            else if (mods.HasFlag(LegacyMods.SuddenDeath))
                yield return new ManiaModSuddenDeath();

            if (mods.HasFlag(LegacyMods.Cinema))
                yield return new ManiaModCinema();
            else if (mods.HasFlag(LegacyMods.Autoplay))
                yield return new ManiaModAutoplay();

            if (mods.HasFlag(LegacyMods.Easy))
                yield return new ManiaModEasy();

            if (mods.HasFlag(LegacyMods.FadeIn))
                yield return new ManiaModFadeIn();

            if (mods.HasFlag(LegacyMods.Flashlight))
                yield return new ManiaModFlashlight();

            if (mods.HasFlag(LegacyMods.HalfTime))
                yield return new ManiaModHalfTime();

            if (mods.HasFlag(LegacyMods.HardRock))
                yield return new ManiaModHardRock();

            if (mods.HasFlag(LegacyMods.Hidden))
                yield return new ManiaModHidden();

            if (mods.HasFlag(LegacyMods.Key1))
                yield return new ManiaModKey1();

            if (mods.HasFlag(LegacyMods.Key2))
                yield return new ManiaModKey2();

            if (mods.HasFlag(LegacyMods.Key3))
                yield return new ManiaModKey3();

            if (mods.HasFlag(LegacyMods.Key4))
                yield return new ManiaModKey4();

            if (mods.HasFlag(LegacyMods.Key5))
                yield return new ManiaModKey5();

            if (mods.HasFlag(LegacyMods.Key6))
                yield return new ManiaModKey6();

            if (mods.HasFlag(LegacyMods.Key7))
                yield return new ManiaModKey7();

            if (mods.HasFlag(LegacyMods.Key8))
                yield return new ManiaModKey8();

            if (mods.HasFlag(LegacyMods.Key9))
                yield return new ManiaModKey9();

            if (mods.HasFlag(LegacyMods.KeyCoop))
                yield return new ManiaModDualStages();

            if (mods.HasFlag(LegacyMods.NoFail))
                yield return new ManiaModNoFail();

            if (mods.HasFlag(LegacyMods.Random))
                yield return new ManiaModRandom();

            if (mods.HasFlag(LegacyMods.Mirror))
                yield return new ManiaModMirror();

            if (mods.HasFlag(LegacyMods.ScoreV2))
                yield return new ManiaModScoreV2();
        }

        public override LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            var value = base.ConvertToLegacyMods(mods);

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case ManiaModKey1:
                        value |= LegacyMods.Key1;
                        break;

                    case ManiaModKey2:
                        value |= LegacyMods.Key2;
                        break;

                    case ManiaModKey3:
                        value |= LegacyMods.Key3;
                        break;

                    case ManiaModKey4:
                        value |= LegacyMods.Key4;
                        break;

                    case ManiaModKey5:
                        value |= LegacyMods.Key5;
                        break;

                    case ManiaModKey6:
                        value |= LegacyMods.Key6;
                        break;

                    case ManiaModKey7:
                        value |= LegacyMods.Key7;
                        break;

                    case ManiaModKey8:
                        value |= LegacyMods.Key8;
                        break;

                    case ManiaModKey9:
                        value |= LegacyMods.Key9;
                        break;

                    case ManiaModDualStages:
                        value |= LegacyMods.KeyCoop;
                        break;

                    case ManiaModFadeIn:
                        value |= LegacyMods.FadeIn;
                        value &= ~LegacyMods.Hidden; // this is toggled on in the base call due to inheritance, but we don't want that.
                        break;

                    case ManiaModMirror:
                        value |= LegacyMods.Mirror;
                        break;

                    case ManiaModRandom:
                        value |= LegacyMods.Random;
                        break;
                }
            }

            return value;
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new ManiaModEasy(),
                        new ManiaModNoFail(),
                        new MultiMod(new ManiaModHalfTime(), new ManiaModDaycore()),
                        new ManiaModNoRelease(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new ManiaModHardRock(),
                        new MultiMod(new ManiaModSuddenDeath(), new ManiaModPerfect()),
                        new MultiMod(new ManiaModDoubleTime(), new ManiaModNightcore()),
                        new MultiMod(new ManiaModFadeIn(), new ManiaModHidden(), new ManiaModCover()),
                        new ManiaModFlashlight(),
                        new ModAccuracyChallenge(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new ManiaModRandom(),
                        new ManiaModDualStages(),
                        new ManiaModMirror(),
                        new ManiaModDifficultyAdjust(),
                        new ManiaModClassic(),
                        new ManiaModInvert(),
                        new ManiaModConstantSpeed(),
                        new ManiaModHoldOff(),
                        new MultiMod(
                            new ManiaModKey1(),
                            new ManiaModKey2(),
                            new ManiaModKey3(),
                            new ManiaModKey4(),
                            new ManiaModKey5(),
                            new ManiaModKey6(),
                            new ManiaModKey7(),
                            new ManiaModKey8(),
                            new ManiaModKey9(),
                            new ManiaModKey10()
                        ),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new ManiaModAutoplay(), new ManiaModCinema()),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new ManiaModMuted(),
                        new ModAdaptiveSpeed()
                    };

                case ModType.System:
                    return new Mod[]
                    {
                        new ManiaModScoreV2(),
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override string Description => "osu!mania";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Smashing keys";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetMania };

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new ManiaDifficultyCalculator(RulesetInfo, beatmap);

        public int LegacyID => 3;

        public ILegacyScoreSimulator CreateLegacyScoreSimulator() => new ManiaLegacyScoreSimulator();

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new ManiaReplayFrame();

        public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new ManiaRulesetConfigManager(settings, RulesetInfo);

        public override RulesetSettingsSubsection CreateSettings() => new ManiaSettingsSubsection(this);

        public override IEnumerable<int> AvailableVariants
        {
            get
            {
                for (int i = 1; i <= MAX_STAGE_KEYS; i++)
                    yield return (int)PlayfieldType.Single + i;
                for (int i = 2; i <= MAX_STAGE_KEYS * 2; i += 2)
                    yield return (int)PlayfieldType.Dual + i;
            }
        }

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
        {
            switch (getPlayfieldType(variant))
            {
                case PlayfieldType.Single:
                    return new SingleStageVariantGenerator(variant).GenerateMappings();

                case PlayfieldType.Dual:
                    return new DualStageVariantGenerator(getDualStageKeyCount(variant)).GenerateMappings();
            }

            return Array.Empty<KeyBinding>();
        }

        public override LocalisableString GetVariantName(int variant)
        {
            switch (getPlayfieldType(variant))
            {
                default:
                    return $"{variant}K";

                case PlayfieldType.Dual:
                {
                    int keys = getDualStageKeyCount(variant);
                    return $"{keys}K + {keys}K";
                }
            }
        }

        /// <summary>
        /// Finds the number of keys for each stage in a <see cref="PlayfieldType.Dual"/> variant.
        /// </summary>
        /// <param name="variant">The variant.</param>
        private int getDualStageKeyCount(int variant) => (variant - (int)PlayfieldType.Dual) / 2;

        /// <summary>
        /// Finds the <see cref="PlayfieldType"/> that corresponds to a variant value.
        /// </summary>
        /// <param name="variant">The variant value.</param>
        /// <returns>The <see cref="PlayfieldType"/> that corresponds to <paramref name="variant"/>.</returns>
        private PlayfieldType getPlayfieldType(int variant)
        {
            return (PlayfieldType)Enum.GetValues(typeof(PlayfieldType)).Cast<int>().OrderDescending().First(v => variant >= v);
        }

        protected override IEnumerable<HitResult> GetValidHitResults()
        {
            return new[]
            {
                HitResult.Perfect,
                HitResult.Great,
                HitResult.Good,
                HitResult.Ok,
                HitResult.Meh,

                // HitResult.SmallBonus is used for awarding perfect bonus score but is not included here as
                // it would be a bit redundant to show this to the user.
            };
        }

        public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap) => new[]
        {
            new StatisticItem("Performance Breakdown", () => new PerformanceBreakdownChart(score, playableBeatmap)
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y
            }),
            new StatisticItem("Timing Distribution", () => new HitEventTimingDistributionGraph(score.HitEvents)
            {
                RelativeSizeAxes = Axes.X,
                Height = 250
            }, true),
            new StatisticItem("Statistics", () => new SimpleStatisticTable(2, new SimpleStatisticItem[]
            {
                new AverageHitError(score.HitEvents),
                new UnstableRate(score.HitEvents)
            }), true)
        };

        /// <seealso cref="ManiaHitWindows"/>
        public override BeatmapDifficulty GetAdjustedDisplayDifficulty(IBeatmapInfo beatmapInfo, IReadOnlyCollection<Mod> mods)
        {
            BeatmapDifficulty adjustedDifficulty = base.GetAdjustedDisplayDifficulty(beatmapInfo, mods);

            // notably, in mania, hit windows are designed to be independent of track playback rate (see `ManiaHitWindows.SpeedMultiplier`).
            // *however*, to not make matters *too* simple, mania Hard Rock and Easy differ from all other rulesets
            // in that they apply multipliers *to hit window durations directly* rather than to the Overall Difficulty attribute itself.
            // because the duration of hit window durations as a function of OD is not a linear function,
            // this means that multiplying the OD is *not* the same thing as multiplying the hit window duration.
            // in fact, the second operation is *much* harsher and will produce values much farther outside of normal operating range
            // (even negative in the case of Easy).
            // stable handles this wrong on song select and just assumes that it can handle mania EZ / HR the same way as all other rulesets.

            double perfectHitWindow = IBeatmapDifficultyInfo.DifficultyRange(adjustedDifficulty.OverallDifficulty, ManiaHitWindows.PERFECT_WINDOW_RANGE);

            if (mods.Any(m => m is ManiaModHardRock))
                perfectHitWindow /= ManiaModHardRock.HIT_WINDOW_DIFFICULTY_MULTIPLIER;
            else if (mods.Any(m => m is ManiaModEasy))
                perfectHitWindow /= ManiaModEasy.HIT_WINDOW_DIFFICULTY_MULTIPLIER;

            adjustedDifficulty.OverallDifficulty = (float)IBeatmapDifficultyInfo.InverseDifficultyRange(perfectHitWindow, ManiaHitWindows.PERFECT_WINDOW_RANGE);
            adjustedDifficulty.CircleSize = ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo), mods);

            return adjustedDifficulty;
        }

        public override IEnumerable<RulesetBeatmapAttribute> GetBeatmapAttributesForDisplay(IBeatmapInfo beatmapInfo, IReadOnlyCollection<Mod> mods)
        {
            // a special touch-up of key count is required to the original difficulty, since key conversion mods are not `IApplicableToDifficulty`
            var originalDifficulty = new BeatmapDifficulty(beatmapInfo.Difficulty)
            {
                CircleSize = ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo), [])
            };
            var adjustedDifficulty = GetAdjustedDisplayDifficulty(beatmapInfo, mods);
            var colours = new OsuColour();

            yield return new RulesetBeatmapAttribute(SongSelectStrings.KeyCount, @"KC", originalDifficulty.CircleSize, adjustedDifficulty.CircleSize, 18)
            {
                Description = "Affects the number of key columns on the playfield."
            };

            var hitWindows = new ManiaHitWindows();
            hitWindows.SetDifficulty(adjustedDifficulty.OverallDifficulty);
            hitWindows.IsConvert = !beatmapInfo.Ruleset.Equals(RulesetInfo);
            hitWindows.ClassicModActive = mods.Any(m => m is ManiaModClassic);
            yield return new RulesetBeatmapAttribute(SongSelectStrings.Accuracy, @"OD", originalDifficulty.OverallDifficulty, adjustedDifficulty.OverallDifficulty, 10)
            {
                Description = "Affects timing requirements for notes.",
                AdditionalMetrics = hitWindows.GetAllAvailableWindows()
                                              .Reverse()
                                              .Select(window => new RulesetBeatmapAttribute.AdditionalMetric(
                                                  $"{window.result.GetDescription().ToUpperInvariant()} hit window",
                                                  LocalisableString.Interpolate($@"±{hitWindows.WindowFor(window.result):0.##} ms"),
                                                  colours.ForHitResult(window.result)
                                              )).ToArray()
            };

            yield return new RulesetBeatmapAttribute(SongSelectStrings.HPDrain, @"HP", originalDifficulty.DrainRate, adjustedDifficulty.DrainRate, 10)
            {
                Description = "Affects the harshness of health drain and the health penalties for missing."
            };
        }

        public override IRulesetFilterCriteria CreateRulesetFilterCriteria()
        {
            return new ManiaFilterCriteria();
        }

        public override IEnumerable<Drawable> CreateEditorSetupSections() =>
        [
            new MetadataSection(),
            new ManiaDifficultySection(),
            new ResourcesSection(),
            new DesignSection(),
        ];

        public int GetKeyCount(IBeatmapInfo beatmapInfo, IReadOnlyList<Mod>? mods = null)
            => ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo), mods);
    }

    public enum PlayfieldType
    {
        /// <summary>
        /// Columns are grouped into a single stage.
        /// Number of columns in this stage lies at (item - Single).
        /// </summary>
        Single = 0,

        /// <summary>
        /// Columns are grouped into two stages.
        /// Overall number of columns lies at (item - Dual), further computation is required for
        /// number of columns in each individual stage.
        /// </summary>
        Dual = 1000,
    }
}
