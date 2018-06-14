// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using System;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHardRock : ModHardRock, IApplicableToHitObject
    {
        public override double ScoreMultiplier => 1.12;
        public override bool Ranked => true;

        private float lastStartX;
        private int lastStartTime;

        public void ApplyToHitObject(HitObject hitObject)
        {
            var catchObject = (CatchHitObject)hitObject;

            float position = catchObject.X;
            int startTime = (int)hitObject.StartTime;

            if (lastStartX == 0)
            {
                lastStartX = position;
                lastStartTime = startTime;
                return;
            }

            float diff = lastStartX - position;
            int timeDiff = startTime - lastStartTime;

            if (timeDiff > 1000)
            {
                lastStartX = position;
                lastStartTime = startTime;
                return;
            }

            if (diff == 0)
            {
                bool right = RNG.NextBool();

                float rand = Math.Min(20, (float)RNG.NextDouble(0, timeDiff / 4d)) / CatchPlayfield.BASE_WIDTH;

                if (right)
                {
                    if (position + rand <= 1)
                        position += rand;
                    else
                        position -= rand;
                }
                else
                {
                    if (position - rand >= 0)
                        position -= rand;
                    else
                        position += rand;
                }

                catchObject.X = position;

                return;
            }

            if (Math.Abs(diff) < timeDiff / 3d)
            {
                if (diff > 0)
                {
                    if (position - diff > 0)
                        position -= diff;
                }
                else
                {
                    if (position - diff < 1)
                        position -= diff;
                }
            }

            catchObject.X = position;

            lastStartX = position;
            lastStartTime = startTime;
        }
    }
}
