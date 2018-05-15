// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaPerformanceCalculator : PerformanceCalculator
    {
        private Mod[] mods;
        private int countGeki;
        private int countKatu;
        private int count300;
        private int count100;
        private int count50;
        private int countMiss;

        public ManiaPerformanceCalculator(Ruleset ruleset, IBeatmap beatmap, Score score)
            : base(ruleset, beatmap, score)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            mods = Score.Mods;
            countGeki = Convert.ToInt32(Score.Statistics[HitResult.Perfect]);
            countKatu = Convert.ToInt32(Score.Statistics[HitResult.Ok]);
            count300 = Convert.ToInt32(Score.Statistics[HitResult.Great]);
            count100 = Convert.ToInt32(Score.Statistics[HitResult.Good]);
            count50 = Convert.ToInt32(Score.Statistics[HitResult.Meh]);
            countMiss = Convert.ToInt32(Score.Statistics[HitResult.Miss]);

            if (mods.Any(m => !m.Ranked))
                return 0;

            // Custom multipliers for NoFail
            double multiplier = 1.1; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is ModNoFail))
                multiplier *= 0.9;
            if (mods.Any(m => m is ModEasy))
                multiplier *= 0.5;

            double strainValue = computeStrainValue();
            double accValue = computeAccuracyValue();
            double totalValue =
                Math.Pow(
                    Math.Pow(strainValue, 1.1) +
                    Math.Pow(accValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            if (categoryDifficulty != null)
            {
                categoryDifficulty["Strain"] = strainValue;
                categoryDifficulty["Accuracy"] = accValue;

            }

            return totalValue;
        }

        private double computeStrainValue()
        {
            IEnumerable<Mod> scoreIncreaseMods = new ManiaRuleset().GetModsFor(ModType.DifficultyIncrease);

            double scoreMultiplier = 1.0;
            foreach (var m in mods.Where(m => !scoreIncreaseMods.Contains(m)))
                scoreMultiplier *= m.ScoreMultiplier;

            // Score after being scaled by non-difficulty-increasing mods
            double scaledScore = Score.TotalScore;

            // Scale score up, so it's comparable to other keymods
            scaledScore *= 1.0 / scoreMultiplier;

            // Obtain strain difficulty
            double strainValue = Math.Pow(5 * Math.Max(1, Attributes["Strain"] / 0.0825) - 4.0, 3.0) / 110000.0;

            // Longer maps are worth more
            strainValue *= 1.0 + 0.1 * Math.Min(1.0, totalHits / 1500.0);

            if (scaledScore <= 500000)
                strainValue = 0;
            else if (scaledScore <= 600000)
                strainValue *= (scaledScore - 500000) / 100000 * 0.3;
            else if (scaledScore <= 700000)
                strainValue *= 0.3 + (scaledScore - 600000) / 100000 * 0.35;
            else if (scaledScore <= 800000)
                strainValue *= 0.65 + (scaledScore - 700000) / 100000 * 0.20;
            else if (scaledScore <= 900000)
                strainValue *= 0.85 + (scaledScore - 800000) / 100000 * 0.1;
            else
                strainValue *= 0.95 + (scaledScore - 900000) / 100000 * 0.05;

            return strainValue;
        }

        private double computeAccuracyValue()
        {
            double hitWindow300 = (Beatmap.HitObjects.First().HitWindows.Great / 2 - 0.5) / TimeRate;
            if (hitWindow300 <= 0)
                return 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accuracyValue = Math.Pow(150.0 / hitWindow300 * Math.Pow(Score.Accuracy, 16), 1.8) * 2.5;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            accuracyValue *= Math.Min(1.15, Math.Pow(totalHits / 1500.0, 0.3));

            return accuracyValue;
        }

        private double totalHits => countGeki + countKatu + count300 + count100 + count50 + countMiss;
    }
}
