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
using osu.Game.Graphics;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Rulesets.Taiko.Edit;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Argon;
using osu.Game.Rulesets.Taiko.Skinning.Legacy;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Overlays.Settings;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;
using osu.Game.Rulesets.Configuration;
using osu.Game.Configuration;
using osu.Game.Rulesets.Taiko.Configuration;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoRuleset : Ruleset, ILegacyRuleset
    {
        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => new DrawableTaikoRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new TaikoScoreProcessor();

        public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new TaikoHealthProcessor();

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new TaikoBeatmapConverter(beatmap, this);

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case ArgonSkin:
                    return new TaikoArgonSkinTransformer(skin);

                case LegacySkin:
                    return new TaikoLegacySkinTransformer(skin);
            }

            return null;
        }

        public const string SHORT_NAME = "taiko";

        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.MouseLeft, TaikoAction.LeftCentre),
            new KeyBinding(InputKey.MouseRight, TaikoAction.LeftRim),
            new KeyBinding(InputKey.D, TaikoAction.LeftRim),
            new KeyBinding(InputKey.F, TaikoAction.LeftCentre),
            new KeyBinding(InputKey.J, TaikoAction.RightCentre),
            new KeyBinding(InputKey.K, TaikoAction.RightRim),
        };

        public override IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlagFast(LegacyMods.Nightcore))
                yield return new TaikoModNightcore();
            else if (mods.HasFlagFast(LegacyMods.DoubleTime))
                yield return new TaikoModDoubleTime();

            if (mods.HasFlagFast(LegacyMods.Perfect))
                yield return new TaikoModPerfect();
            else if (mods.HasFlagFast(LegacyMods.SuddenDeath))
                yield return new TaikoModSuddenDeath();

            if (mods.HasFlagFast(LegacyMods.Cinema))
                yield return new TaikoModCinema();
            else if (mods.HasFlagFast(LegacyMods.Autoplay))
                yield return new TaikoModAutoplay();

            if (mods.HasFlagFast(LegacyMods.Easy))
                yield return new TaikoModEasy();

            if (mods.HasFlagFast(LegacyMods.Flashlight))
                yield return new TaikoModFlashlight();

            if (mods.HasFlagFast(LegacyMods.HalfTime))
                yield return new TaikoModHalfTime();

            if (mods.HasFlagFast(LegacyMods.HardRock))
                yield return new TaikoModHardRock();

            if (mods.HasFlagFast(LegacyMods.Hidden))
                yield return new TaikoModHidden();

            if (mods.HasFlagFast(LegacyMods.NoFail))
                yield return new TaikoModNoFail();

            if (mods.HasFlagFast(LegacyMods.Relax))
                yield return new TaikoModRelax();

            if (mods.HasFlagFast(LegacyMods.Random))
                yield return new TaikoModRandom();

            if (mods.HasFlagFast(LegacyMods.ScoreV2))
                yield return new ModScoreV2();
        }

        public override LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            var value = base.ConvertToLegacyMods(mods);

            if (mods.OfType<TaikoModRandom>().Any())
                value |= LegacyMods.Random;

            return value;
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new TaikoModEasy(),
                        new TaikoModNoFail(),
                        new MultiMod(new TaikoModHalfTime(), new TaikoModDaycore()),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new TaikoModHardRock(),
                        new MultiMod(new TaikoModSuddenDeath(), new TaikoModPerfect()),
                        new MultiMod(new TaikoModDoubleTime(), new TaikoModNightcore()),
                        new TaikoModHidden(),
                        new TaikoModFlashlight(),
                        new ModAccuracyChallenge(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new TaikoModRandom(),
                        new TaikoModDifficultyAdjust(),
                        new TaikoModClassic(),
                        new TaikoModSwap(),
                        new TaikoModSingleTap(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new TaikoModAutoplay(), new TaikoModCinema()),
                        new TaikoModRelax(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new TaikoModMuted(),
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

        public override string Description => "osu!taiko";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Bashing drums";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetTaiko };

        public override HitObjectComposer CreateHitObjectComposer() => new TaikoHitObjectComposer(this);

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new TaikoDifficultyCalculator(RulesetInfo, beatmap);

        public override PerformanceCalculator CreatePerformanceCalculator() => new TaikoPerformanceCalculator();

        public int LegacyID => 1;

        public ILegacyScoreSimulator CreateLegacyScoreSimulator() => new TaikoLegacyScoreSimulator();

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new TaikoReplayFrame();

        public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new TaikoRulesetConfigManager(settings, RulesetInfo);

        public override RulesetSettingsSubsection CreateSettings() => new TaikoSettingsSubsection(this);

        protected override IEnumerable<HitResult> GetValidHitResults()
        {
            return new[]
            {
                HitResult.Great,
                HitResult.Ok,

                HitResult.SmallBonus,
                HitResult.LargeBonus,
            };
        }

        public override LocalisableString GetDisplayNameForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.SmallBonus:
                    return "drum tick";

                case HitResult.LargeBonus:
                    return "bonus";
            }

            return base.GetDisplayNameForHitResult(result);
        }

        public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap)
        {
            var timedHitEvents = score.HitEvents.Where(e => e.HitObject is Hit).ToList();

            return new[]
            {
                new StatisticItem("Performance Breakdown", () => new PerformanceBreakdownChart(score, playableBeatmap)
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }),
                new StatisticItem("Timing Distribution", () => new HitEventTimingDistributionGraph(timedHitEvents)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 250
                }, true),
                new StatisticItem("Statistics", () => new SimpleStatisticTable(2, new SimpleStatisticItem[]
                {
                    new AverageHitError(timedHitEvents),
                    new UnstableRate(timedHitEvents)
                }), true)
            };
        }

        public override BeatmapDifficulty GetRateAdjustedDifficulty(IBeatmapDifficultyInfo baseDifficulty, double rate)
        {
            BeatmapDifficulty adjustedDifficulty = new BeatmapDifficulty(baseDifficulty);

            double hitwindow = 35.0 - 15.0 * (adjustedDifficulty.OverallDifficulty - 5) / 5;
            hitwindow /= rate;
            adjustedDifficulty.OverallDifficulty = (float)(5 * (35 - hitwindow) / 15 + 5);

            return adjustedDifficulty ?? (BeatmapDifficulty)baseDifficulty;
        }
    }
}
