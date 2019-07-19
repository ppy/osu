// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Speed : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => strainDecay;

        private const int max_pattern_length = 15;
        private const int min_mono_nerf = 8;
        private const int max_mono_nerf = 20;

        private double strainDecay = 0.3;

        private readonly double[][] previousDeltas = { new double[max_pattern_length], new double[max_pattern_length], new double[max_pattern_length] };

        private int sameColourCount = 1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // Sliders and spinners are optional to hit and thus are ignored
            if (!(current.BaseObject is Hit) || current.DeltaTime <= 0.0)
            {
                strainDecay = float.Epsilon;
                sameColourCount = 0;
                return 0.0;
            }

            int noteType = current.BaseObject is RimHit ? 1 : 0;
            var taikoCurrent = (TaikoDifficultyHitObject)current;

            if (taikoCurrent.HasTypeChange)
                sameColourCount = 0;
            else
                sameColourCount++;

            double deltaSum = 0;
            double deltaCount = 0;
            deltaSum += current.DeltaTime;
            deltaCount += 1.0;

            for (var i = 1; i < max_pattern_length; i++)
            {
                for(var j = 0; j < 2; ++j)
                {
                    if (previousDeltas[j == 1 ? noteType : 2][i - 1] != 0.0)
                    {
                        double weight = Math.Pow(0.9, i) * (j == 1 ? 1 : 0.5);
                        deltaCount += weight;
                        deltaSum += previousDeltas[j == 1 ? noteType : 2][i - 1] * weight;
                    }
                    previousDeltas[noteType][i] = previousDeltas[noteType][i - 1];
                }
            }

            previousDeltas[noteType][0] = current.DeltaTime;
            previousDeltas[2][0] = current.DeltaTime;

            // Use last N notes instead of last 1 note for determining pattern speed. Especially affects 1/8 doubles.
            // Limit speed to 333bpm monocolor streams. Usually patterns are mixed, shouldnt be a huge problem.
            double normalizedDelta = Math.Max(deltaSum / deltaCount, 45);

            // Overwrite current.DeltaTime with normalizedDelta in Skill's strainDecay function
            strainDecay = Math.Pow(Math.Pow(0.5, normalizedDelta / 1000.0), 1000.0 / Math.Max(current.DeltaTime, 1.0)) * 0.3;

            var monoNerf = 1.0;

            if(sameColourCount > min_mono_nerf)
                monoNerf = Math.Pow(0.98, Math.Min(sameColourCount, max_mono_nerf) - min_mono_nerf);

            return 71.0 / normalizedDelta * monoNerf;
        }
    }
}
