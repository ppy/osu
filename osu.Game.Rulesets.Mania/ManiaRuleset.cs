// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Graphics;
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

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case TrianglesSkin:
                    return new ManiaTrianglesSkinTransformer(skin, beatmap);

                case ArgonSkin:
                    return new ManiaArgonSkinTransformer(skin, beatmap);

                case DefaultLegacySkin:
                    return new ManiaClassicSkinTransformer(skin, beatmap);

                case LegacySkin:
                    return new ManiaLegacySkinTransformer(skin, beatmap);
            }

            return null;
        }

        public override IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlagFast(LegacyMods.Nightcore))
                yield return new ManiaModNightcore();
            else if (mods.HasFlagFast(LegacyMods.DoubleTime))
                yield return new ManiaModDoubleTime();

            if (mods.HasFlagFast(LegacyMods.Perfect))
                yield return new ManiaModPerfect();
            else if (mods.HasFlagFast(LegacyMods.SuddenDeath))
                yield return new ManiaModSuddenDeath();

            if (mods.HasFlagFast(LegacyMods.Cinema))
                yield return new ManiaModCinema();
            else if (mods.HasFlagFast(LegacyMods.Autoplay))
                yield return new ManiaModAutoplay();

            if (mods.HasFlagFast(LegacyMods.Easy))
                yield return new ManiaModEasy();

            if (mods.HasFlagFast(LegacyMods.FadeIn))
                yield return new ManiaModFadeIn();

            if (mods.HasFlagFast(LegacyMods.Flashlight))
                yield return new ManiaModFlashlight();

            if (mods.HasFlagFast(LegacyMods.HalfTime))
                yield return new ManiaModHalfTime();

            if (mods.HasFlagFast(LegacyMods.HardRock))
                yield return new ManiaModHardRock();

            if (mods.HasFlagFast(LegacyMods.Hidden))
                yield return new ManiaModHidden();

            if (mods.HasFlagFast(LegacyMods.Key1))
                yield return new ManiaModKeyCount { KeyCount = { Value = 1 } };

            if (mods.HasFlagFast(LegacyMods.Key2))
                yield return new ManiaModKeyCount { KeyCount = { Value = 2 } };

            if (mods.HasFlagFast(LegacyMods.Key3))
                yield return new ManiaModKeyCount { KeyCount = { Value = 3 } };

            if (mods.HasFlagFast(LegacyMods.Key4))
                yield return new ManiaModKeyCount { KeyCount = { Value = 4 } };

            if (mods.HasFlagFast(LegacyMods.Key5))
                yield return new ManiaModKeyCount { KeyCount = { Value = 5 } };

            if (mods.HasFlagFast(LegacyMods.Key6))
                yield return new ManiaModKeyCount { KeyCount = { Value = 6 } };

            if (mods.HasFlagFast(LegacyMods.Key7))
                yield return new ManiaModKeyCount { KeyCount = { Value = 7 } };

            if (mods.HasFlagFast(LegacyMods.Key8))
                yield return new ManiaModKeyCount { KeyCount = { Value = 8 } };

            if (mods.HasFlagFast(LegacyMods.Key9))
                yield return new ManiaModKeyCount { KeyCount = { Value = 9 } };

            if (mods.HasFlagFast(LegacyMods.KeyCoop))
                yield return new ManiaModDualStages();

            if (mods.HasFlagFast(LegacyMods.NoFail))
                yield return new ManiaModNoFail();

            if (mods.HasFlagFast(LegacyMods.Random))
                yield return new ManiaModRandom();

            if (mods.HasFlagFast(LegacyMods.Mirror))
                yield return new ManiaModMirror();

            if (mods.HasFlagFast(LegacyMods.ScoreV2))
                yield return new ModScoreV2();
        }

        public override LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            var value = base.ConvertToLegacyMods(mods);

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case ManiaModKeyCount keyCountMod:
                        value |= keyCountMod.KeyCount.Value switch
                        {
                            1 => LegacyMods.Key1,
                            2 => LegacyMods.Key2,
                            3 => LegacyMods.Key3,
                            4 => LegacyMods.Key4,
                            5 => LegacyMods.Key5,
                            6 => LegacyMods.Key6,
                            7 => LegacyMods.Key7,
                            8 => LegacyMods.Key8,
                            9 => LegacyMods.Key9,
                            _ => 0,
                        };
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
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new ManiaModHardRock(),
                        new MultiMod(new ManiaModSuddenDeath(), new ManiaModPerfect()),
                        new MultiMod(new ManiaModDoubleTime(), new ManiaModNightcore()),
                        new MultiMod(new ManiaModFadeIn(), new ManiaModHidden()),
                        new ManiaModFlashlight(),
                        new ModAccuracyChallenge(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new ManiaModRandom(),
                        new ManiaModKeyCount(),
                        new ManiaModDualStages(),
                        new ManiaModMirror(),
                        new ManiaModDifficultyAdjust(),
                        new ManiaModClassic(),
                        new ManiaModInvert(),
                        new ManiaModConstantSpeed(),
                        new ManiaModHoldOff(),
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
                        new ModScoreV2(),
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
            return (PlayfieldType)Enum.GetValues(typeof(PlayfieldType)).Cast<int>().OrderByDescending(i => i).First(v => variant >= v);
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

        public override IRulesetFilterCriteria CreateRulesetFilterCriteria()
        {
            return new ManiaFilterCriteria();
        }

        public override RulesetSetupSection CreateEditorSetupSection() => new ManiaSetupSection();

        public override DifficultySection CreateEditorDifficultySection() => new ManiaDifficultySection();

        public int GetKeyCount(IBeatmapInfo beatmapInfo)
            => ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo));
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
