// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    internal class TaikoScoreV1Processor
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

        private readonly double modMultiplier;
        private readonly int difficultyPeppyStars;
        private readonly IBeatmap playableBeatmap;
        private readonly IReadOnlyList<Mod> mods;

        public TaikoScoreV1Processor(IBeatmap baseBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
        {
            this.playableBeatmap = playableBeatmap;
            this.mods = mods;

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

            difficultyPeppyStars = (int)Math.Round(
                (baseBeatmap.Difficulty.DrainRate
                 + baseBeatmap.Difficulty.OverallDifficulty
                 + baseBeatmap.Difficulty.CircleSize
                 + Math.Clamp(objectCount / baseBeatmap.Difficulty.DrainRate * 8, 0, 16)) / 38 * 5);

            modMultiplier = mods.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier);

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
                case SwellTick:
                    scoreIncrease = 300;
                    increaseCombo = false;
                    break;

                case DrumRollTick:
                    scoreIncrease = 300;
                    increaseCombo = false;
                    isBonus = true;
                    break;

                case Swell swell:
                    // The taiko swell generally does not match the osu-stable implementation in any way.
                    // We'll redo the calculations to match osu-stable here...
                    double minimumRotationsPerSecond = IBeatmapDifficultyInfo.DifficultyRange(playableBeatmap.Difficulty.OverallDifficulty, 3, 5, 7.5);
                    double secondsDuration = swell.Duration / 1000;

                    // The amount of half spins that are required to successfully complete the spinner (i.e. get a 300).
                    int halfSpinsRequiredForCompletion = (int)(secondsDuration * minimumRotationsPerSecond);

                    halfSpinsRequiredForCompletion = (int)Math.Max(1, halfSpinsRequiredForCompletion * 1.65f);

                    if (mods.Any(m => m is ModDoubleTime))
                        halfSpinsRequiredForCompletion = Math.Max(1, (int)(halfSpinsRequiredForCompletion * 0.75f));
                    if (mods.Any(m => m is ModHalfTime))
                        halfSpinsRequiredForCompletion = Math.Max(1, (int)(halfSpinsRequiredForCompletion * 1.5f));

                    for (int i = 0; i <= halfSpinsRequiredForCompletion; i++)
                        simulateHit(new SwellTick());

                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    increaseCombo = false;
                    isBonus = true;
                    break;

                case Hit:
                    scoreIncrease = 300;
                    addScoreComboMultiplier = true;
                    break;

                case DrumRoll:
                    foreach (var nested in hitObject.NestedHitObjects)
                        simulateHit(nested);
                    return;
            }

            if (hitObject is DrumRollTick tick)
            {
                if (playableBeatmap.ControlPointInfo.EffectPointAt(tick.Parent.StartTime).KiaiMode)
                    scoreIncrease = (int)(scoreIncrease * 1.2f);

                if (tick.IsStrong)
                    scoreIncrease += scoreIncrease / 5;
            }

            // The score increase directly contributed to by the combo-multiplied portion.
            int comboScoreIncrease = 0;

            if (addScoreComboMultiplier)
            {
                int oldScoreIncrease = scoreIncrease;

                // ReSharper disable once PossibleLossOfFraction (intentional to match osu-stable...)
                scoreIncrease += (int)(scoreIncrease / 35 * 2 * (difficultyPeppyStars + 1) * modMultiplier) * (Math.Min(100, combo) / 10);

                if (hitObject is Swell)
                {
                    if (playableBeatmap.ControlPointInfo.EffectPointAt(hitObject.GetEndTime()).KiaiMode)
                        scoreIncrease = (int)(scoreIncrease * 1.2f);
                }
                else
                {
                    if (playableBeatmap.ControlPointInfo.EffectPointAt(hitObject.StartTime).KiaiMode)
                        scoreIncrease = (int)(scoreIncrease * 1.2f);
                }

                comboScoreIncrease = scoreIncrease - oldScoreIncrease;
            }

            if (hitObject is Swell || (hitObject is TaikoStrongableHitObject strongable && strongable.IsStrong))
            {
                scoreIncrease *= 2;
                comboScoreIncrease *= 2;
            }

            scoreIncrease -= comboScoreIncrease;

            if (addScoreComboMultiplier)
                ComboScore += comboScoreIncrease;

            if (isBonus)
                BonusScore += scoreIncrease;
            else
                BaseScore += scoreIncrease;

            if (increaseCombo)
                combo++;

            if (hitObject is Swell)
            {
            }
        }
    }
}
