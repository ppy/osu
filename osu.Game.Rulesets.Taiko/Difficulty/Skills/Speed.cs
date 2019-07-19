// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Speed : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => strainDecay;

        private const int max_pattern_length = 15;

        private double strainDecay = 0.3;

        private readonly double[] strain = new double[2];
        private readonly double[][] previousDeltas = { new double[max_pattern_length], new double[max_pattern_length] };

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // Sliders and spinners are optional to hit and thus are ignored
            if (!(current.BaseObject is Hit) || current.DeltaTime <= 0.0)
            {
                strainDecay = float.Epsilon;
                return 0.0;
            }

            int noteType = current.BaseObject is RimHit ? 1 : 0;

            double deltaSum = 0;
            double deltaCount = 0;
            deltaSum += current.DeltaTime;
            deltaCount += 1.0;

            for (var i = 1; i < max_pattern_length; i++)
            {
                if (previousDeltas[noteType][i - 1] != 0.0)
                {
                    double weight = Math.Pow(0.9, i);
                    deltaCount += weight;
                    deltaSum += previousDeltas[noteType][i - 1] * weight;
                }

                previousDeltas[noteType][i] = previousDeltas[noteType][i - 1];
            }

            previousDeltas[noteType][0] = current.DeltaTime;

            // Use last N notes instead of last 1 note for determining pattern speed. Especially affects 1/8 doubles.
            // Limit speed to 333bpm monocolor streams. Usually patterns are mixed, shouldnt be a huge problem.
            double normalizedDelta = Math.Max(deltaSum / deltaCount, 45);

            // Overwrite current.DeltaTime with normalizedDelta in Skill's strainDecay function
            strainDecay = Math.Pow(Math.Pow(0.5, normalizedDelta / 1000.0), 1000.0 / Math.Max(current.DeltaTime, 1.0)) * 0.3;

            return 70.0 / normalizedDelta;
        }
    }
}
