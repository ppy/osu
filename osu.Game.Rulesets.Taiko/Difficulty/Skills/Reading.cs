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
        protected override double SkillMultiplier => 1.0;
        protected override double StrainDecayBase => 0.3;

        private const int max_pattern_length = 15;

        private string lastNotes = "";

        // Pattern occurences. An optimization is possible using bitarrays instead of strings.
        private readonly Dictionary<string, int>[] patternOccur = { new Dictionary<string, int>(), new Dictionary<string, int>() };
        private readonly Dictionary<string, int>[] patternCount = { new Dictionary<string, int>(), new Dictionary<string, int>() };

        private int noteNum;

        private const double slow_note_delta_min = 800.0;
        private const double slow_note_delta_max = 2000.0;

        private double repetitionNerf = 1.0;

        private ColourSwitch lastColourSwitch = ColourSwitch.None;

        private int lastSameColourCount;
        private int sameColourCount = 1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // Sliders and spinners are optional to hit and thus are ignored
            if (!(current.BaseObject is Hit) || current.DeltaTime <= 0.0)
            {
                lastSameColourCount = sameColourCount;
                sameColourCount = 0;
                return 0.0;
            }

            // Decay known patterns
            noteNum++;
            bool isRim = current.BaseObject is RimHit;

            var colorDiff = colourChangeDifficulty(current);

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

            var taikoCurrent = (TaikoDifficultyHitObject)current;
            repetitionNerf = Math.Min(Math.Max(repetitionNerf * 25 * colorDiff, 1.0 / 25), 1.0);
            taikoCurrent.RepetitionNerf = Math.Pow(repetitionNerf, 0.5);

            double addition = 1.2375 + 24.75 * colorDiff * taikoCurrent.RepetitionNerf;

            if (current.DeltaTime > slow_note_delta_min)
                addition *= 0.9 - 0.9 * Math.Pow((Math.Min(current.DeltaTime, slow_note_delta_max) - slow_note_delta_min) / (slow_note_delta_max - slow_note_delta_min), 0.25) + 0.1;

            return addition * 1.15;
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

            return Math.Max(0, Math.Pow(chanceError / chanceTotal, hasColourChangeLegacy(current) ? (1.4 / (1.99 + Math.Pow(-0.99, Math.Min(10, lastSameColourCount)))) : 1.7) * (0.5 + 0.5 * Math.Pow(Math.Max(0, freqRating), 1.5)));
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
            lastSameColourCount = sameColourCount;
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
