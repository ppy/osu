// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Colour : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.3;

        private ColourSwitch lastColourSwitch = ColourSwitch.None;
        private int sameColourCount = 1;

        private int[] previousDonLengths = {0, 0}, previousKatLengths = {0, 0};
        private int sameTypeCount = 1;
        // TODO: make this smarter (dont initialise with "Don")
        private bool previousIsKat = false;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            return StrainValueOfNew(current);
        }

        protected double StrainValueOfNew(DifficultyHitObject current)
        {

            double returnVal = 0.0;
            double returnMultiplier = 1.0;

            if (previousIsKat != ((TaikoDifficultyHitObject) current).IsKat)
            {
                returnVal = 1.5 - (1.75 / (sameTypeCount + 0.65));

                if (previousIsKat)
                {
                    if (sameTypeCount % 2 == previousDonLengths[0] % 2)
                    {
                        returnMultiplier *= 0.8;
                    }

                    if (previousKatLengths[0] == sameTypeCount)
                    {
                        returnMultiplier *= 0.525;
                    }

                    if (previousKatLengths[1] == sameTypeCount)
                    {
                        returnMultiplier *= 0.75;
                    }

                    previousKatLengths[1] = previousKatLengths[0];
                    previousKatLengths[0] = sameTypeCount;
                }
                else
                {
                    if (sameTypeCount % 2 == previousKatLengths[0] % 2)
                    {
                        returnMultiplier *= 0.8;
                    }

                    if (previousDonLengths[0] == sameTypeCount)
                    {
                        returnMultiplier *= 0.525;
                    }

                    if (previousDonLengths[1] == sameTypeCount)
                    {
                        returnMultiplier *= 0.75;
                    }

                    previousDonLengths[1] = previousDonLengths[0];
                    previousDonLengths[0] = sameTypeCount;
                }


                sameTypeCount = 1;
                previousIsKat = ((TaikoDifficultyHitObject) current).IsKat;

            }

            else
            {
                sameTypeCount += 1;
            }

            return Math.Min(1.25, returnVal) * returnMultiplier;
        }

        protected double StrainValueOfOld(DifficultyHitObject current)
        {

            double addition = 0;

            // We get an extra addition if we are not a slider or spinner
            if (current.LastObject is Hit && current.BaseObject is Hit && current.DeltaTime < 1000)
            {
                if (hasColourChange(current))
                    addition = 0.75;
            }
            else
            {
                lastColourSwitch = ColourSwitch.None;
                sameColourCount = 1;
            }

            return addition;
        }


        private bool hasColourChange(DifficultyHitObject current)
        {
            var taikoCurrent = (TaikoDifficultyHitObject) current;

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
