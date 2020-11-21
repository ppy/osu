﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaRuleset : Ruleset, ILegacyRuleset
    {
        /// <summary>
        /// The maximum number of supported keys in a single stage.
        /// </summary>
        public const int MAX_STAGE_KEYS = 10;

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => new DrawableManiaRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor();

        public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new DrainingHealthProcessor(drainStartTime, 0.2);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new ManiaBeatmapConverter(beatmap, this);

        public override PerformanceCalculator CreatePerformanceCalculator(DifficultyAttributes attributes, ScoreInfo score) => new ManiaPerformanceCalculator(this, attributes, score);

        public const string SHORT_NAME = "mania";

        public override HitObjectComposer CreateHitObjectComposer() => new ManiaHitObjectComposer(this);

        public override ISkin CreateLegacySkinProvider(ISkinSource source, IBeatmap beatmap) => new ManiaLegacySkinTransformer(source, beatmap);

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
        }

        public override LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            var value = base.ConvertToLegacyMods(mods);

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case ManiaModKey1 _:
                        value |= LegacyMods.Key1;
                        break;

                    case ManiaModKey2 _:
                        value |= LegacyMods.Key2;
                        break;

                    case ManiaModKey3 _:
                        value |= LegacyMods.Key3;
                        break;

                    case ManiaModKey4 _:
                        value |= LegacyMods.Key4;
                        break;

                    case ManiaModKey5 _:
                        value |= LegacyMods.Key5;
                        break;

                    case ManiaModKey6 _:
                        value |= LegacyMods.Key6;
                        break;

                    case ManiaModKey7 _:
                        value |= LegacyMods.Key7;
                        break;

                    case ManiaModKey8 _:
                        value |= LegacyMods.Key8;
                        break;

                    case ManiaModKey9 _:
                        value |= LegacyMods.Key9;
                        break;

                    case ManiaModDualStages _:
                        value |= LegacyMods.KeyCoop;
                        break;

                    case ManiaModFadeIn _:
                        value |= LegacyMods.FadeIn;
                        value &= ~LegacyMods.Hidden; // this is toggled on in the base call due to inheritance, but we don't want that.
                        break;

                    case ManiaModMirror _:
                        value |= LegacyMods.Mirror;
                        break;

                    case ManiaModRandom _:
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
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new MultiMod(new ManiaModKey4(),
                            new ManiaModKey5(),
                            new ManiaModKey6(),
                            new ManiaModKey7(),
                            new ManiaModKey8(),
                            new ManiaModKey9(),
                            new ManiaModKey10(),
                            new ManiaModKey1(),
                            new ManiaModKey2(),
                            new ManiaModKey3()),
                        new ManiaModRandom(),
                        new ManiaModDualStages(),
                        new ManiaModMirror(),
                        new ManiaModDifficultyAdjust(),
                        new ManiaModInvert(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new ManiaModAutoplay(), new ManiaModCinema()),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown())
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override string Description => "osu!mania";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Smashing keys";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetMania };

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => new ManiaDifficultyCalculator(this, beatmap);

        public int LegacyID => 3;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new ManiaReplayFrame();

        public override IRulesetConfigManager CreateConfig(SettingsStore settings) => new ManiaRulesetConfigManager(settings, RulesetInfo);

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

        public override string GetVariantName(int variant)
        {
            switch (getPlayfieldType(variant))
            {
                default:
                    return $"{variant}K";

                case PlayfieldType.Dual:
                {
                    var keys = getDualStageKeyCount(variant);
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

                HitResult.LargeTickHit,
            };
        }

        public override string GetDisplayNameForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.LargeTickHit:
                    return "hold tick";
            }

            return base.GetDisplayNameForHitResult(result);
        }

        public override StatisticRow[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap) => new[]
        {
            new StatisticRow
            {
                Columns = new[]
                {
                    new StatisticItem("Timing Distribution", new HitEventTimingDistributionGraph(score.HitEvents)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 250
                    }),
                }
            },
            new StatisticRow
            {
                Columns = new[]
                {
                    new StatisticItem(string.Empty, new SimpleStatisticTable(3, new SimpleStatisticItem[]
                    {
                        new UnstableRate(score.HitEvents)
                    }))
                }
            }
        };
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
