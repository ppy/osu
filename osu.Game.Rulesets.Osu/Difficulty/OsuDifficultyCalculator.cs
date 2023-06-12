// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.0675;

        public override int Version => 20220902;

        private readonly IWorkingBeatmap workingBeatmap;

        public OsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            workingBeatmap = beatmap;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods };

            double aimRating = Math.Sqrt(skills[0].DifficultyValue()) * difficulty_multiplier;
            double aimRatingNoSliders = Math.Sqrt(skills[1].DifficultyValue()) * difficulty_multiplier;
            double speedRating = Math.Sqrt(skills[2].DifficultyValue()) * difficulty_multiplier;
            double speedNotes = ((Speed)skills[2]).RelevantNoteCount();
            double flashlightRating = Math.Sqrt(skills[3].DifficultyValue()) * difficulty_multiplier;

            double sliderFactor = aimRating > 0 ? aimRatingNoSliders / aimRating : 1;

            if (mods.Any(m => m is OsuModTouchDevice))
            {
                aimRating = Math.Pow(aimRating, 0.8);
                flashlightRating = Math.Pow(flashlightRating, 0.8);
            }

            if (mods.Any(h => h is OsuModRelax))
            {
                aimRating *= 0.9;
                speedRating = 0.0;
                flashlightRating *= 0.7;
            }

            double baseAimPerformance = Math.Pow(5 * Math.Max(1, aimRating / 0.0675) - 4, 3) / 100000;
            double baseSpeedPerformance = Math.Pow(5 * Math.Max(1, speedRating / 0.0675) - 4, 3) / 100000;
            double baseFlashlightPerformance = 0.0;

            if (mods.Any(h => h is OsuModFlashlight))
                baseFlashlightPerformance = Math.Pow(flashlightRating, 2.0) * 25.0;

            double basePerformance =
                Math.Pow(
                    Math.Pow(baseAimPerformance, 1.1) +
                    Math.Pow(baseSpeedPerformance, 1.1) +
                    Math.Pow(baseFlashlightPerformance, 1.1), 1.0 / 1.1
                );

            double starRating = basePerformance > 0.00001
                ? Math.Cbrt(OsuPerformanceCalculator.PERFORMANCE_BASE_MULTIPLIER) * 0.027 * (Math.Cbrt(100000 / Math.Pow(2, 1 / 1.1) * basePerformance) + 4)
                : 0;

            double preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / clockRate;
            double drainRate = beatmap.Difficulty.DrainRate;
            int maxCombo = beatmap.GetMaxCombo();

            int hitCirclesCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int sliderCount = beatmap.HitObjects.Count(h => h is Slider);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            double hitWindowGreat = hitWindows.WindowFor(HitResult.Great) / clockRate;

            OsuScoreV1Processor sv1Processor = new OsuScoreV1Processor(workingBeatmap.Beatmap, beatmap, mods);

            return new OsuDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                AimDifficulty = aimRating,
                SpeedDifficulty = speedRating,
                SpeedNoteCount = speedNotes,
                FlashlightDifficulty = flashlightRating,
                SliderFactor = sliderFactor,
                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                DrainRate = drainRate,
                MaxCombo = maxCombo,
                HitCircleCount = hitCirclesCount,
                SliderCount = sliderCount,
                SpinnerCount = spinnerCount,
                LegacyTotalScore = sv1Processor.TotalScore,
                LegacyComboScore = sv1Processor.ComboScore,
                LegacyBonusScore = sv1Processor.BonusScore
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

            // The first jump is formed by the first two hitobjects of the map.
            // If the map has less than two OsuHitObjects, the enumerator will not return anything.
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                var lastLast = i > 1 ? beatmap.HitObjects[i - 2] : null;
                objects.Add(new OsuDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], lastLast, clockRate, objects, objects.Count));
            }

            return objects;
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            return new Skill[]
            {
                new Aim(mods, true),
                new Aim(mods, false),
                new Speed(mods),
                new Flashlight(mods)
            };
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModTouchDevice(),
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
            new OsuModFlashlight(),
            new MultiMod(new OsuModFlashlight(), new OsuModHidden())
        };
    }

    public abstract class ScoreV1Processor
    {
        protected readonly int DifficultyPeppyStars;
        protected readonly double ScoreMultiplier;

        protected readonly IBeatmap PlayableBeatmap;

        protected ScoreV1Processor(IBeatmap baseBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
        {
            PlayableBeatmap = playableBeatmap;

            int countNormal = 0;
            int countSlider = 0;
            int countSpinner = 0;

            foreach (HitObject obj in baseBeatmap.HitObjects)
            {
                switch (obj)
                {
                    case IHasPath:
                        countSlider++;
                        break;

                    case IHasDuration:
                        countSpinner++;
                        break;

                    default:
                        countNormal++;
                        break;
                }
            }

            int objectCount = countNormal + countSlider + countSpinner;

            DifficultyPeppyStars = (int)Math.Round(
                (baseBeatmap.Difficulty.DrainRate
                 + baseBeatmap.Difficulty.OverallDifficulty
                 + baseBeatmap.Difficulty.CircleSize
                 + Math.Clamp(objectCount / baseBeatmap.Difficulty.DrainRate * 8, 0, 16)) / 38 * 5);

            ScoreMultiplier = DifficultyPeppyStars * mods.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier);
        }
    }

    public class OsuScoreV1Processor : ScoreV1Processor
    {
        public int TotalScore => BaseScore + ComboScore + BonusScore;

        /// <summary>
        /// Amount of score that is combo-and-difficulty-multiplied, excluding mod multipliers.
        /// </summary>
        public int ComboScore { get; private set; }

        /// <summary>
        /// Amount of score that is NOT combo-and-difficulty-multiplied.
        /// </summary>
        public int BaseScore { get; private set; }

        /// <summary>
        /// Amount of score whose judgements would be treated as "bonus" in ScoreV2.
        /// </summary>
        public int BonusScore { get; private set; }

        private int combo;

        public OsuScoreV1Processor(IBeatmap baseBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
            : base(baseBeatmap, playableBeatmap, mods)
        {
            foreach (var obj in playableBeatmap.HitObjects)
                simulateHit(obj);
        }

        private void simulateHit(HitObject hitObject)
        {
            bool increaseCombo = true;
            bool addScoreComboMultiplier = false;
            bool isBonus = false;

            int scoreIncrease = 0;

            switch (hitObject)
            {
                case SliderHeadCircle:
                case SliderTailCircle:
                case SliderRepeat:
                    scoreIncrease = 30;
                    break;

                case SliderTick:
                    scoreIncrease = 10;
                    break;

                case SpinnerBonusTick:
                    scoreIncrease = 1100;
                    increaseCombo = false;
                    isBonus = true;
                    break;

                case SpinnerTick:
                    scoreIncrease = 100;
                    increaseCombo = false;
                    isBonus = true;
                    break;

                case HitCircle:
                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    break;

                case Slider:
                    foreach (var nested in hitObject.NestedHitObjects)
                        simulateHit(nested);

                    scoreIncrease = 300;
                    increaseCombo = false;
                    addScoreComboMultiplier = true;
                    break;

                case Spinner spinner:
                    // The spinner object applies a lenience because gameplay mechanics differ from osu-stable.
                    // We'll redo the calculations to match osu-stable here...
                    const double maximum_rotations_per_second = 477.0 / 60;
                    double minimumRotationsPerSecond = IBeatmapDifficultyInfo.DifficultyRange(PlayableBeatmap.Difficulty.OverallDifficulty, 3, 5, 7.5);
                    double secondsDuration = spinner.Duration / 1000;

                    // The total amount of half spins possible for the entire spinner.
                    int totalHalfSpinsPossible = (int)(secondsDuration * maximum_rotations_per_second * 2);
                    // The amount of half spins that are required to successfully complete the spinner (i.e. get a 300).
                    int halfSpinsRequiredForCompletion = (int)(secondsDuration * minimumRotationsPerSecond);
                    // To be able to receive bonus points, the spinner must be rotated another 1.5 times.
                    int halfSpinsRequiredBeforeBonus = halfSpinsRequiredForCompletion + 3;

                    for (int i = 0; i <= totalHalfSpinsPossible; i++)
                    {
                        if (i > halfSpinsRequiredBeforeBonus && (i - halfSpinsRequiredBeforeBonus) % 2 == 0)
                            simulateHit(new SpinnerBonusTick());
                        else if (i > 1 && i % 2 == 0)
                            simulateHit(new SpinnerTick());
                    }

                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    break;
            }

            if (addScoreComboMultiplier)
            {
                // ReSharper disable once PossibleLossOfFraction (intentional to match osu-stable...)
                ComboScore += (int)(Math.Max(0, combo - 1) * (scoreIncrease / 25 * ScoreMultiplier));
            }

            if (isBonus)
                BonusScore += scoreIncrease;
            else
                BaseScore += scoreIncrease;

            if (increaseCombo)
                combo++;
        }
    }
}
