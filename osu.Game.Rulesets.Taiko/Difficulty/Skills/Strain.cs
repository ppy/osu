// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Strain : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => strainDecay * 0.3;

        private const int max_pattern_length = 15;

        private double strainDecay = 1.0;

        private string lastNotes = "";

        // Pattern occurences. An optimization is possible using bitarrays instead of strings.
        private readonly Dictionary<string, int>[] patternOccur = new Dictionary<string, int>[] { new Dictionary<string, int>(), new Dictionary<string, int>() };

        private readonly double[] previousDeltas = new double[max_pattern_length];
        private int noteNum;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // Sliders and spinners are optional to hit and thus are ignored
            if (!(current.LastObject is Hit))
                return 0.0;

            // Decay known patterns
            noteNum++;

            // Arbitrary values found by analyzing top plays of different people
            double addition = 1.9 + 2.5 * colourChangeDifficulty(current);

            double deltaSum = current.DeltaTime;
            int deltaCount = 1;

            for (var i = 1; i < max_pattern_length; i++)
            {
                if (previousDeltas[i - 1] != 0.0)
                {
                    deltaCount++;
                    deltaSum += previousDeltas[i - 1];
                }

                previousDeltas[i] = previousDeltas[i - 1];
            }

            previousDeltas[0] = current.DeltaTime;

            // Use last N notes instead of last 1 note for determining pattern speed. Especially affects 1/8 doubles. TODO account more for recent notes?
            var normalizedDelta = deltaSum / deltaCount;
            // Overwrite current.DeltaTime with normalizedDelta in Skill's strainDecay function
            strainDecay = Math.Pow(Math.Pow(0.3, normalizedDelta / 1000.0), 1000.0 / Math.Max(current.DeltaTime, 1.0));

            // Remember patterns
            bool isRim = current.LastObject is RimHit;
            var patternsEndingWithSameNoteType = patternOccur[isRim ? 1 : 0];
            for (var i = 1; i < lastNotes.Length; i++)
                patternsEndingWithSameNoteType[lastNotes.Substring(lastNotes.Length - i)] = noteNum;

            // Forget oldest note
            if (lastNotes.Length == max_pattern_length)
                lastNotes = lastNotes.Substring(1);
            // Remember latest note
            lastNotes += isRim ? 'k' : 'd';

            return addition;
        }

        private double colourChangeDifficulty(DifficultyHitObject current)
        {
            double chanceError = 0.0, chanceTotal = 0.0;
            bool isRim = current.LastObject is RimHit;

            for (var i = 1; i < lastNotes.Length; i++)
            {
                var pattern = lastNotes.Substring(lastNotes.Length - i);

                // How well is the pattern remembered
                double[] memory = new double[2];

                for (var j = 0; j < 2; j++)
                {
                    int n = 0;
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
            }

            // If we don't remember any patterns, chances are 50/50
            if (chanceTotal == 0.0)
            {
                chanceTotal = 1.0;
                chanceError = 0.5;
            }

            return Math.Max(0, Math.Pow(chanceError / chanceTotal, 1.4));
        }
    }
}
