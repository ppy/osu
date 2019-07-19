// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Reading : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => strainDecay;

        private const int max_pattern_length = 15;

        private double strainDecay = 1.0;

        private string lastNotes = "";

        // Pattern occurences. An optimization is possible using bitarrays instead of strings.
        private readonly Dictionary<string, int>[] patternOccur = { new Dictionary<string, int>(), new Dictionary<string, int>() };
        private readonly Dictionary<string, int>[] patternCount = { new Dictionary<string, int>(), new Dictionary<string, int>() };

        private readonly double[] previousDeltas = new double[max_pattern_length];
        private int noteNum;

        // Only nerfs super repeating patterns
        private double repetitionNerf = 1.0;

        private const double rhythm_change_base_threshold = 0.2;
        private const double rhythm_change_base = 2.0;

        private ColourSwitch lastColourSwitch = ColourSwitch.None;

        private int sameColourCount = 1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // Sliders and spinners are optional to hit and thus are ignored
            if (!(current.BaseObject is Hit) || current.DeltaTime <= 0.0)
            {
                strainDecay = 0.1;
                sameColourCount = 0;
                return 0.0;
            }

            // Decay known patterns
            noteNum++;
            bool isRim = current.BaseObject is RimHit;

            // Arbitrary values found by analyzing top plays of different people
            var colorDiff = colourChangeDifficulty(current);

            double deltaSum = current.DeltaTime;
            double deltaCount = 1;

            for (var i = 1; i < max_pattern_length; i++)
            {
                if (previousDeltas[i - 1] != 0.0)
                {
                    double weight = Math.Pow(0.9, i);
                    deltaCount += weight;
                    deltaSum += previousDeltas[i - 1] * weight;
                }

                previousDeltas[i] = previousDeltas[i - 1];
            }

            previousDeltas[0] = current.DeltaTime;

            // Use last N notes instead of last 1 note for determining pattern speed. Especially affects 1/8 doubles.
            var normalizedDelta = deltaSum / deltaCount;

            // Overwrite current.DeltaTime with normalizedDelta in Skill's strainDecay function
            strainDecay = Math.Pow(Math.Pow(0.3, normalizedDelta / 1000.0), 1000.0 / Math.Max(current.DeltaTime, 1.0));

            // Remember patterns
            var occurForThisType = patternOccur[isRim ? 1 : 0];
            var countForThisType = patternCount[isRim ? 1 : 0];

            for (var i = 1; i < lastNotes.Length; i++)
            {
                var pattern = lastNotes.Substring(lastNotes.Length - i);
                occurForThisType[pattern] = noteNum;
                int count;
                countForThisType.TryGetValue(pattern, out count);
                countForThisType[pattern] = count + 1;
            }

            // Forget oldest note
            if (lastNotes.Length == max_pattern_length)
                lastNotes = lastNotes.Substring(1);

            // Remember latest note
            lastNotes += isRim ? 'k' : 'd';
            repetitionNerf = Math.Min(Math.Max(repetitionNerf * 25.0 * colorDiff, 1.0 / 25.0), 1.0);
            double addition = 0.272 + 5.44 * colorDiff * repetitionNerf;

            return Math.Pow(addition, 1.1);
        }

        private double colourChangeDifficulty(DifficultyHitObject current)
        {
            double chanceError = 0.0, chanceTotal = 0.0;
            double freqRating = 1.0;
            bool isRim = current.BaseObject is RimHit;

            for (var i = 1; i < lastNotes.Length; i++)
            {
                var pattern = lastNotes.Substring(lastNotes.Length - i);

                // How well is the pattern remembered
                double[] memory = new double[2];
                int[] count = new int[2];

                for (var j = 0; j < 2; j++)
                {
                    int n;
                    patternCount[j].TryGetValue(pattern, out count[j]);
                    patternOccur[j].TryGetValue(pattern, out n);
                    memory[j] = Math.Pow(0.99, noteNum - n);
                }

                double[] weight = new double[2];
                for (var j = 0; j < 2; j++)
                    weight[j] = Math.Pow(1.1, i) * Math.Pow(memory[j], 5);

                // Only account for this if we remember something
                if (memory[0] + memory[1] != 0.0)
                {
                    chanceError += weight[isRim ? 0 : 1] * memory[isRim ? 0 : 1] / (memory[0] + memory[1]);
                    chanceTotal += weight[0] + weight[1];
                }

                // If a pattern is frequent, nerf it even if it's unpredictable in the current circumstances
                freqRating *= 1.1 - Math.Pow(1.0 + Math.Pow(1.6, i) / 16000, Math.Min(34, count[isRim ? 1 : 0])) / 10;
            }

            // If we don't remember anything, chances are 50/50
            if (chanceTotal == 0.0)
            {
                chanceTotal = 1.0;
                chanceError = 0.5;
            }

            return Math.Max(0, Math.Pow(chanceError / chanceTotal, hasColourChangeLegacy(current) ? 1.4 : 1.7) * (0.5 + 0.5 * Math.Pow(Math.Max(0, freqRating), 1.5)));
        }

        // Keep this, because it determines general predictability
        private bool hasColourChangeLegacy(DifficultyHitObject current)
        {
            var taikoCurrent = (TaikoDifficultyHitObject)current;

            if (!taikoCurrent.HasTypeChange)
            {
                sameColourCount++;
                return false;
            }

            var oldColourSwitch = lastColourSwitch;
            var newColourSwitch = sameColourCount % 2 == 0 ? ColourSwitch.Even : ColourSwitch.Odd;

            lastColourSwitch = newColourSwitch;
            sameColourCount = 1;

            // We only want a bonus if the parity of the color switch changes
            return oldColourSwitch != ColourSwitch.None && oldColourSwitch != newColourSwitch;
        }

        private enum ColourSwitch
        {
            None,
            Even,
            Odd
        }
    }
}
