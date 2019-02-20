// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Strain : Skill
    {
        private const double rhythm_change_base_threshold = 0.2;
        private const double rhythm_change_base = 2.0;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.3;

        private ColourSwitch lastColourSwitch = ColourSwitch.None;

        private int sameColourCount = 1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double addition = 1;

            // We get an extra addition if we are not a slider or spinner
            if (current.LastObject is Hit && current.BaseObject is Hit && current.DeltaTime < 1000)
            {
                if (hasColourChange(current))
                    addition += 0.75;

                if (hasRhythmChange(current))
                    addition += 1;
            }
            else
            {
                lastColourSwitch = ColourSwitch.None;
                sameColourCount = 1;
            }

            double additionFactor = 1;

            // Scale the addition factor linearly from 0.4 to 1 for DeltaTime from 0 to 50
            if (current.DeltaTime < 50)
                additionFactor = 0.4 + 0.6 * current.DeltaTime / 50;

            return additionFactor * addition;
        }

        private bool hasRhythmChange(DifficultyHitObject current)
        {
            // We don't want a division by zero if some random mapper decides to put two HitObjects at the same time.
            if (current.DeltaTime == 0 || Previous.Count == 0 || Previous[0].DeltaTime == 0)
                return false;

            double timeElapsedRatio = Math.Max(Previous[0].DeltaTime / current.DeltaTime, current.DeltaTime / Previous[0].DeltaTime);

            if (timeElapsedRatio >= 8)
                return false;

            double difference = Math.Log(timeElapsedRatio, rhythm_change_base) % 1.0;

            return difference > rhythm_change_base_threshold && difference < 1 - rhythm_change_base_threshold;
        }

        private bool hasColourChange(DifficultyHitObject current)
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
