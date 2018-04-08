// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHardRock : ModHardRock, IApplicableToHitObject<CatchHitObject>
    {
        public override double ScoreMultiplier => 1.12;
        public override bool Ranked => true;
        
        private float lastStartX;
        private int lastStartTime;

        public void ApplyToHitObject(CatchHitObject hitObject)
        {
            // Code from Stable, we keep calculation on a scale of 0 to 512
            float position = hitObject.X * 512;
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

                float rand = Math.Min(20, (float)RNG.NextDouble(0, timeDiff / 4));

                if (right)
                {
                    if (position + rand <= 512)
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

                hitObject.X = position / 512;

                return;
            }

            if (Math.Abs(diff) < timeDiff / 3)
            {
                if (diff > 0)
                {
                    if (position - diff > 0)
                        position -= diff;
                }
                else
                {
                    if (position - diff < 512)
                        position -= diff;
                }
            }

            hitObject.X = position / 512;

            lastStartX = position;
            lastStartTime = startTime;
        }
    }
}
