// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Catch.Edit;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Catch.Skinning.Argon;
using osu.Game.Rulesets.Catch.Skinning.Legacy;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch
{
    public class CatchRuleset : Ruleset, ILegacyRuleset
    {
        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => new DrawableCatchRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor();

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new CatchBeatmapConverter(beatmap, this);

        public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => new CatchBeatmapProcessor(beatmap);

        public const string SHORT_NAME = "fruits";

        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, CatchAction.MoveLeft),
            new KeyBinding(InputKey.Left, CatchAction.MoveLeft),
            new KeyBinding(InputKey.X, CatchAction.MoveRight),
            new KeyBinding(InputKey.Right, CatchAction.MoveRight),
            new KeyBinding(InputKey.Shift, CatchAction.Dash),
            new KeyBinding(InputKey.MouseLeft, CatchAction.Dash),
        };

        public override IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlagFast(LegacyMods.Nightcore))
                yield return new CatchModNightcore();
            else if (mods.HasFlagFast(LegacyMods.DoubleTime))
                yield return new CatchModDoubleTime();

            if (mods.HasFlagFast(LegacyMods.Perfect))
                yield return new CatchModPerfect();
            else if (mods.HasFlagFast(LegacyMods.SuddenDeath))
                yield return new CatchModSuddenDeath();

            if (mods.HasFlagFast(LegacyMods.Cinema))
                yield return new CatchModCinema();
            else if (mods.HasFlagFast(LegacyMods.Autoplay))
                yield return new CatchModAutoplay();

            if (mods.HasFlagFast(LegacyMods.Easy))
                yield return new CatchModEasy();

            if (mods.HasFlagFast(LegacyMods.Flashlight))
                yield return new CatchModFlashlight();

            if (mods.HasFlagFast(LegacyMods.HalfTime))
                yield return new CatchModHalfTime();

            if (mods.HasFlagFast(LegacyMods.HardRock))
                yield return new CatchModHardRock();

            if (mods.HasFlagFast(LegacyMods.Hidden))
                yield return new CatchModHidden();

            if (mods.HasFlagFast(LegacyMods.NoFail))
                yield return new CatchModNoFail();

            if (mods.HasFlagFast(LegacyMods.Relax))
                yield return new CatchModRelax();

            if (mods.HasFlagFast(LegacyMods.ScoreV2))
                yield return new ModScoreV2();
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new CatchModEasy(),
                        new CatchModNoFail(),
                        new MultiMod(new CatchModHalfTime(), new CatchModDaycore())
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new CatchModHardRock(),
                        new MultiMod(new CatchModSuddenDeath(), new CatchModPerfect()),
                        new MultiMod(new CatchModDoubleTime(), new CatchModNightcore()),
                        new CatchModHidden(),
                        new CatchModFlashlight(),
                        new ModAccuracyChallenge(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new CatchModDifficultyAdjust(),
                        new CatchModClassic(),
                        new CatchModMirror(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new CatchModAutoplay(), new CatchModCinema()),
                        new CatchModRelax(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new CatchModFloatingFruits(),
                        new CatchModMuted(),
                        new CatchModNoScope(),
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

        public override string Description => "osu!catch";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Catching fruit";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetCatch };

        protected override IEnumerable<HitResult> GetValidHitResults()
        {
            return new[]
            {
                HitResult.Great,

                HitResult.LargeTickHit,
                HitResult.SmallTickHit,
                HitResult.LargeBonus,
            };
        }

        public override LocalisableString GetDisplayNameForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.LargeTickHit:
                    return "Large droplet";

                case HitResult.SmallTickHit:
                    return "Small droplet";

                case HitResult.LargeBonus:
                    return "Banana";
            }

            return base.GetDisplayNameForHitResult(result);
        }

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new CatchDifficultyCalculator(RulesetInfo, beatmap);

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case LegacySkin:
                    return new CatchLegacySkinTransformer(skin);

                case ArgonSkin:
                    return new CatchArgonSkinTransformer(skin);
            }

            return null;
        }

        public override PerformanceCalculator CreatePerformanceCalculator() => new CatchPerformanceCalculator();

        public int LegacyID => 2;

        public ILegacyScoreSimulator CreateLegacyScoreSimulator() => new CatchLegacyScoreSimulator();

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new CatchReplayFrame();

        public override HitObjectComposer CreateHitObjectComposer() => new CatchHitObjectComposer(this);

        public override IBeatmapVerifier CreateBeatmapVerifier() => new CatchBeatmapVerifier();

        public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap)
        {
            return new[]
            {
                new StatisticItem("Performance Breakdown", () => new PerformanceBreakdownChart(score, playableBeatmap)
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }),
            };
        }

        public override BeatmapDifficulty GetRateAdjustedDifficulty(IBeatmapDifficultyInfo baseDifficulty, double rate)
        {
            BeatmapDifficulty adjustedDifficulty = new BeatmapDifficulty(baseDifficulty);

            double preempt = adjustedDifficulty.ApproachRate < 5 ? (1200.0 + 600.0 * (5 - adjustedDifficulty.ApproachRate) / 5) : (1200.0 - 750.0 * (adjustedDifficulty.ApproachRate - 5) / 5);

            preempt /= rate;
            adjustedDifficulty.ApproachRate = (float)(preempt > 1200 ? ((1800 - preempt) / 120) : ((1200 - preempt) / 150 + 5));

            return adjustedDifficulty ?? (BeatmapDifficulty)baseDifficulty;
        }
    }
}
