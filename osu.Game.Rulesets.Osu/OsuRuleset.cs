// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Setup;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.Skinning.Argon;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Rulesets.Osu.Statistics;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu
{
    public class OsuRuleset : Ruleset, ILegacyRuleset
    {
        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => new DrawableOsuRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor();

        public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new OsuHealthProcessor(drainStartTime);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new OsuBeatmapConverter(beatmap, this);

        public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => new OsuBeatmapProcessor(beatmap);

        public const string SHORT_NAME = "osu";

        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, OsuAction.LeftButton),
            new KeyBinding(InputKey.X, OsuAction.RightButton),
            new KeyBinding(InputKey.C, OsuAction.Smoke),
            new KeyBinding(InputKey.MouseLeft, OsuAction.LeftButton),
            new KeyBinding(InputKey.MouseRight, OsuAction.RightButton),
        };

        public override IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlag(LegacyMods.Nightcore))
                yield return new OsuModNightcore();
            else if (mods.HasFlag(LegacyMods.DoubleTime))
                yield return new OsuModDoubleTime();

            if (mods.HasFlag(LegacyMods.Perfect))
                yield return new OsuModPerfect();
            else if (mods.HasFlag(LegacyMods.SuddenDeath))
                yield return new OsuModSuddenDeath();

            if (mods.HasFlag(LegacyMods.Autopilot))
                yield return new OsuModAutopilot();

            if (mods.HasFlag(LegacyMods.Cinema))
                yield return new OsuModCinema();
            else if (mods.HasFlag(LegacyMods.Autoplay))
                yield return new OsuModAutoplay();

            if (mods.HasFlag(LegacyMods.Easy))
                yield return new OsuModEasy();

            if (mods.HasFlag(LegacyMods.Flashlight))
                yield return new OsuModFlashlight();

            if (mods.HasFlag(LegacyMods.HalfTime))
                yield return new OsuModHalfTime();

            if (mods.HasFlag(LegacyMods.HardRock))
                yield return new OsuModHardRock();

            if (mods.HasFlag(LegacyMods.Hidden))
                yield return new OsuModHidden();

            if (mods.HasFlag(LegacyMods.NoFail))
                yield return new OsuModNoFail();

            if (mods.HasFlag(LegacyMods.Relax))
                yield return new OsuModRelax();

            if (mods.HasFlag(LegacyMods.SpunOut))
                yield return new OsuModSpunOut();

            if (mods.HasFlag(LegacyMods.Target))
                yield return new OsuModTargetPractice();

            if (mods.HasFlag(LegacyMods.TouchDevice))
                yield return new OsuModTouchDevice();

            if (mods.HasFlag(LegacyMods.ScoreV2))
                yield return new ModScoreV2();
        }

        public override LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            var value = base.ConvertToLegacyMods(mods);

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case OsuModAutopilot:
                        value |= LegacyMods.Autopilot;
                        break;

                    case OsuModSpunOut:
                        value |= LegacyMods.SpunOut;
                        break;

                    case OsuModTargetPractice:
                        value |= LegacyMods.Target;
                        break;

                    case OsuModTouchDevice:
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
                        new OsuModStrictTracking(),
                        new OsuModAccuracyChallenge(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new OsuModTargetPractice(),
                        new OsuModDifficultyAdjust(),
                        new OsuModClassic(),
                        new OsuModRandom(),
                        new OsuModMirror(),
                        new MultiMod(new OsuModAlternate(), new OsuModSingleTap())
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
                        new OsuModNoScope(),
                        new MultiMod(new OsuModMagnetised(), new OsuModRepel()),
                        new ModAdaptiveSpeed(),
                        new OsuModFreezeFrame(),
                        new OsuModBubbles(),
                        new OsuModSynesthesia(),
                        new OsuModDepth(),
                        new OsuModBloom()
                    };

                case ModType.System:
                    return new Mod[]
                    {
                        new OsuModTouchDevice(),
                        new ModScoreV2(),
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.RulesetOsu };

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new OsuDifficultyCalculator(RulesetInfo, beatmap);

        public override PerformanceCalculator CreatePerformanceCalculator() => new OsuPerformanceCalculator();

        public override HitObjectComposer CreateHitObjectComposer() => new OsuHitObjectComposer(this);

        public override IBeatmapVerifier CreateBeatmapVerifier() => new OsuBeatmapVerifier();

        public override string Description => "osu!";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Clicking circles";

        public override RulesetSettingsSubsection CreateSettings() => new OsuSettingsSubsection(this);

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case LegacySkin:
                    return new OsuLegacySkinTransformer(skin);

                case ArgonSkin:
                    return new OsuArgonSkinTransformer(skin);

                case TrianglesSkin:
                    return new OsuTrianglesSkinTransformer(skin);
            }

            return null;
        }

        public int LegacyID => 0;

        public ILegacyScoreSimulator CreateLegacyScoreSimulator() => new OsuLegacyScoreSimulator();

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new OsuReplayFrame();

        public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new OsuRulesetConfigManager(settings, RulesetInfo);

        protected override IEnumerable<HitResult> GetValidHitResults()
        {
            return new[]
            {
                HitResult.Great,
                HitResult.Ok,
                HitResult.Meh,

                HitResult.LargeTickHit,
                HitResult.SmallTickHit,
                HitResult.SliderTailHit,
                HitResult.SmallBonus,
                HitResult.LargeBonus,
            };
        }

        public override LocalisableString GetDisplayNameForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.LargeTickHit:
                    return "slider tick";

                case HitResult.SliderTailHit:
                case HitResult.SmallTickHit:
                    return "slider end";

                case HitResult.SmallBonus:
                    return "spinner spin";

                case HitResult.LargeBonus:
                    return "spinner bonus";
            }

            return base.GetDisplayNameForHitResult(result);
        }

        public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap)
        {
            var timedHitEvents = score.HitEvents.Where(e => e.HitObject is HitCircle && !(e.HitObject is SliderTailCircle)).ToList();

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
                new StatisticItem("Accuracy Heatmap", () => new AccuracyHeatmap(score, playableBeatmap)
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

        public override IEnumerable<Drawable> CreateEditorSetupSections() =>
        [
            new MetadataSection(),
            new OsuDifficultySection(),
            new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(SetupScreen.SPACING),
                Children = new Drawable[]
                {
                    new ResourcesSection
                    {
                        RelativeSizeAxes = Axes.X,
                    },
                    new ColoursSection
                    {
                        RelativeSizeAxes = Axes.X,
                    }
                }
            },
            new DesignSection(),
        ];

        /// <seealso cref="OsuHitObject.ApplyDefaultsToSelf"/>
        /// <seealso cref="OsuHitWindows"/>
        public override BeatmapDifficulty GetAdjustedDisplayDifficulty(IBeatmapInfo difficulty, IReadOnlyCollection<Mod> mods)
        {
            BeatmapDifficulty adjustedDifficulty = base.GetAdjustedDisplayDifficulty(difficulty, mods);
            double rate = ModUtils.CalculateRateWithMods(mods);

            double preempt = IBeatmapDifficultyInfo.DifficultyRange(adjustedDifficulty.ApproachRate, OsuHitObject.PREEMPT_MAX, OsuHitObject.PREEMPT_MID, OsuHitObject.PREEMPT_MIN);
            preempt /= rate;
            adjustedDifficulty.ApproachRate = (float)IBeatmapDifficultyInfo.InverseDifficultyRange(preempt, OsuHitObject.PREEMPT_MAX, OsuHitObject.PREEMPT_MID, OsuHitObject.PREEMPT_MIN);

            double greatHitWindow = IBeatmapDifficultyInfo.DifficultyRange(adjustedDifficulty.OverallDifficulty, OsuHitWindows.GREAT_WINDOW_RANGE);
            greatHitWindow /= rate;
            adjustedDifficulty.OverallDifficulty = (float)IBeatmapDifficultyInfo.InverseDifficultyRange(greatHitWindow, OsuHitWindows.GREAT_WINDOW_RANGE);

            return adjustedDifficulty;
        }

        public override IEnumerable<RulesetBeatmapAttribute> GetBeatmapAttributesForDisplay(IBeatmapInfo beatmapInfo, IReadOnlyCollection<Mod> mods)
        {
            var originalDifficulty = beatmapInfo.Difficulty;
            // `modAdjustedDifficulty` contains only the direct effect of mods.
            // `effectiveDifficulty` contains the "perceived" effect of rate-adjusting mods on OD and AR.
            // we make a distinction here, because some of the calculations below will require very careful maneuvering between the two for correct results.
            var modAdjustedDifficulty = base.GetAdjustedDisplayDifficulty(beatmapInfo, mods);
            var effectiveDifficulty = GetAdjustedDisplayDifficulty(beatmapInfo, mods);
            var colours = new OsuColour();

            // for circle size, we can use `effectiveDifficulty` directly
            yield return new RulesetBeatmapAttribute(SongSelectStrings.CircleSize, @"CS", originalDifficulty.CircleSize, effectiveDifficulty.CircleSize, 10)
            {
                Description = "Affects the size of hit circles and sliders.",
                AdditionalMetrics =
                [
                    new RulesetBeatmapAttribute.AdditionalMetric("Hit circle radius", (OsuHitObject.OBJECT_RADIUS * LegacyRulesetExtensions.CalculateScaleFromCircleSize(effectiveDifficulty.CircleSize, applyFudge: true)).ToLocalisableString("0.#"))
                ]
            };

            // for approach rate, we can use `effectiveDifficulty` directly, and it is even convenient to do so (it correctly handles rate-changing mods like DT/HT)
            yield return new RulesetBeatmapAttribute(SongSelectStrings.ApproachRate, @"AR", originalDifficulty.ApproachRate, effectiveDifficulty.ApproachRate, 10)
            {
                Description = "Affects how early objects appear on screen relative to their hit time.",
                AdditionalMetrics =
                [
                    new RulesetBeatmapAttribute.AdditionalMetric("Approach time", LocalisableString.Interpolate($@"{IBeatmapDifficultyInfo.DifficultyRange(effectiveDifficulty.ApproachRate, OsuHitObject.PREEMPT_RANGE):#,0.##} ms"))
                ]
            };

            // for OD is where it gets difficult.
            // when displaying hit window ranges with rate-changing mods active, we will want to adjust for rate ourselves, as `effectiveDifficulty` may not be accurate
            // because `OsuHitWindows` applies a floor-and-round operation that will result in inaccurate results
            // (the floor-and-round needs to happen *before* rate is taken into account, not after).
            // for spinner RPM requirements, we do not want to involve rate-changing mods *at all*,
            // because rate-adjusting mods do not change the spin requirement (see `SpinnerRotationTracker.AddRotation()`).
            var hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(modAdjustedDifficulty.OverallDifficulty);
            double rate = ModUtils.CalculateRateWithMods(mods);
            yield return new RulesetBeatmapAttribute(SongSelectStrings.Accuracy, @"OD", originalDifficulty.OverallDifficulty, effectiveDifficulty.OverallDifficulty, 10)
            {
                Description = "Affects timing requirements for hit circles and spin speed requirements for spinners.",
                AdditionalMetrics = hitWindows.GetAllAvailableWindows()
                                              .Reverse()
                                              .Select(window => new RulesetBeatmapAttribute.AdditionalMetric(
                                                  $"{window.result.GetDescription().ToUpperInvariant()} hit window",
                                                  LocalisableString.Interpolate($@"±{hitWindows.WindowFor(window.result) / rate:0.##} ms"),
                                                  colours.ForHitResult(window.result)
                                              )).Concat([
                                                  new RulesetBeatmapAttribute.AdditionalMetric("RPM required to clear spinners", LocalisableString.Interpolate($@"{IBeatmapDifficultyInfo.DifficultyRange(modAdjustedDifficulty.OverallDifficulty, Spinner.CLEAR_RPM_RANGE):N0} RPM")),
                                                  new RulesetBeatmapAttribute.AdditionalMetric("RPM required to get full spinner bonus", LocalisableString.Interpolate($@"{IBeatmapDifficultyInfo.DifficultyRange(modAdjustedDifficulty.OverallDifficulty, Spinner.COMPLETE_RPM_RANGE):N0} RPM")),
                                              ]).ToArray()
            };

            // HP drain is thankfully simple enough.
            yield return new RulesetBeatmapAttribute(SongSelectStrings.HPDrain, @"HP", originalDifficulty.DrainRate, effectiveDifficulty.DrainRate, 10)
            {
                Description = "Affects the harshness of health drain and the health penalties for missing."
            };
        }

        public override bool EditorShowScrollSpeed => false;
    }
}
