// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using System;
using System.Linq;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Rulesets.Osu.Edit.Setup;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Rulesets.Osu.Statistics;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Rulesets.Osu
{
    public class OsuRuleset : Ruleset, ILegacyRuleset
    {
        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => new DrawableOsuRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor();

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new OsuBeatmapConverter(beatmap, this);

        public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => new OsuBeatmapProcessor(beatmap);

        public const string SHORT_NAME = "osu";

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, OsuAction.LeftButton),
            new KeyBinding(InputKey.X, OsuAction.RightButton),
            new KeyBinding(InputKey.MouseLeft, OsuAction.LeftButton),
            new KeyBinding(InputKey.MouseRight, OsuAction.RightButton),
        };

        public override IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlagFast(LegacyMods.Nightcore))
                yield return new OsuModNightcore();
            else if (mods.HasFlagFast(LegacyMods.DoubleTime))
                yield return new OsuModDoubleTime();

            if (mods.HasFlagFast(LegacyMods.Perfect))
                yield return new OsuModPerfect();
            else if (mods.HasFlagFast(LegacyMods.SuddenDeath))
                yield return new OsuModSuddenDeath();

            if (mods.HasFlagFast(LegacyMods.Autopilot))
                yield return new OsuModAutopilot();

            if (mods.HasFlagFast(LegacyMods.Cinema))
                yield return new OsuModCinema();
            else if (mods.HasFlagFast(LegacyMods.Autoplay))
                yield return new OsuModAutoplay();

            if (mods.HasFlagFast(LegacyMods.Easy))
                yield return new OsuModEasy();

            if (mods.HasFlagFast(LegacyMods.Flashlight))
                yield return new OsuModFlashlight();

            if (mods.HasFlagFast(LegacyMods.HalfTime))
                yield return new OsuModHalfTime();

            if (mods.HasFlagFast(LegacyMods.HardRock))
                yield return new OsuModHardRock();

            if (mods.HasFlagFast(LegacyMods.Hidden))
                yield return new OsuModHidden();

            if (mods.HasFlagFast(LegacyMods.NoFail))
                yield return new OsuModNoFail();

            if (mods.HasFlagFast(LegacyMods.Relax))
                yield return new OsuModRelax();

            if (mods.HasFlagFast(LegacyMods.SpunOut))
                yield return new OsuModSpunOut();

            if (mods.HasFlagFast(LegacyMods.Target))
                yield return new OsuModTarget();

            if (mods.HasFlagFast(LegacyMods.TouchDevice))
                yield return new OsuModTouchDevice();
        }

        public override LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            var value = base.ConvertToLegacyMods(mods);

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case OsuModAutopilot _:
                        value |= LegacyMods.Autopilot;
                        break;

                    case OsuModSpunOut _:
                        value |= LegacyMods.SpunOut;
                        break;

                    case OsuModTarget _:
                        value |= LegacyMods.Target;
                        break;

                    case OsuModTouchDevice _:
                        value |= LegacyMods.TouchDevice;
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
                        new OsuModEasy(),
                        new OsuModNoFail(),
                        new MultiMod(new OsuModHalfTime(), new OsuModDaycore()),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new OsuModHardRock(),
                        new MultiMod(new OsuModSuddenDeath(), new OsuModPerfect()),
                        new MultiMod(new OsuModDoubleTime(), new OsuModNightcore()),
                        new OsuModHidden(),
                        new MultiMod(new OsuModFlashlight(), new OsuModBlinds()),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new OsuModTarget(),
                        new OsuModDifficultyAdjust(),
                        new OsuModClassic(),
                        new OsuModRandom(),
                        new OsuModMirror(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new OsuModAutoplay(), new OsuModCinema()),
                        new OsuModRelax(),
                        new OsuModAutopilot(),
                        new OsuModSpunOut(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new OsuModTransform(),
                        new OsuModWiggle(),
                        new OsuModSpinIn(),
                        new MultiMod(new OsuModGrow(), new OsuModDeflate()),
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new OsuModTraceable(),
                        new OsuModBarrelRoll(),
                        new OsuModApproachDifferent(),
                        new OsuModMuted(),
                    };

                case ModType.System:
                    return new Mod[]
                    {
                        new OsuModTouchDevice(),
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetOsu };

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => new OsuDifficultyCalculator(this, beatmap);

        public override PerformanceCalculator CreatePerformanceCalculator(DifficultyAttributes attributes, ScoreInfo score) => new OsuPerformanceCalculator(this, attributes, score);

        public override HitObjectComposer CreateHitObjectComposer() => new OsuHitObjectComposer(this);

        public override IBeatmapVerifier CreateBeatmapVerifier() => new OsuBeatmapVerifier();

        public override string Description => "osu!";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Clicking circles";

        public override RulesetSettingsSubsection CreateSettings() => new OsuSettingsSubsection(this);

        public override ISkin CreateLegacySkinProvider(ISkin skin, IBeatmap beatmap) => new OsuLegacySkinTransformer(skin);

        public int LegacyID => 0;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new OsuReplayFrame();

        public override IRulesetConfigManager CreateConfig(SettingsStore settings) => new OsuRulesetConfigManager(settings, RulesetInfo);

        protected override IEnumerable<HitResult> GetValidHitResults()
        {
            return new[]
            {
                HitResult.Great,
                HitResult.Ok,
                HitResult.Meh,

                HitResult.LargeTickHit,
                HitResult.SmallTickHit,
                HitResult.SmallBonus,
                HitResult.LargeBonus,
            };
        }

        public override string GetDisplayNameForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.LargeTickHit:
                    return "slider tick";

                case HitResult.SmallTickHit:
                    return "slider end";

                case HitResult.SmallBonus:
                    return "spinner spin";

                case HitResult.LargeBonus:
                    return "spinner bonus";
            }

            return base.GetDisplayNameForHitResult(result);
        }

        public override StatisticRow[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap)
        {
            var timedHitEvents = score.HitEvents.Where(e => e.HitObject is HitCircle && !(e.HitObject is SliderTailCircle)).ToList();

            return new[]
            {
                new StatisticRow
                {
                    Columns = new[]
                    {
                        new StatisticItem("Timing Distribution",
                            new HitEventTimingDistributionGraph(timedHitEvents)
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
                        new StatisticItem("Accuracy Heatmap", new AccuracyHeatmap(score, playableBeatmap)
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
                            new UnstableRate(timedHitEvents)
                        }))
                    }
                }
            };
        }

        public override RulesetSetupSection CreateEditorSetupSection() => new OsuSetupSection();
    }
}
