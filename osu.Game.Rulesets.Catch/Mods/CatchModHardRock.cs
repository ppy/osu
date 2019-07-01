// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private float? lastPosition;
        private double lastStartTime;

        public void ApplyToHitObject(HitObject hitObject)
        {
            if (hitObject is JuiceStream stream)
            {
                lastPosition = stream.EndX;
                lastStartTime = stream.EndTime;
                return;
            }

            if (!(hitObject is Fruit))
                return;

            var catchObject = (CatchHitObject)hitObject;

            float position = catchObject.X;
            double startTime = hitObject.StartTime;

            if (lastPosition == null)
            {
                lastPosition = position;
                lastStartTime = startTime;

                return;
            }

            float positionDiff = position - lastPosition.Value;
            double timeDiff = startTime - lastStartTime;

            if (timeDiff > 1000)
            {
                lastPosition = position;
                lastStartTime = startTime;
                return;
            }

            if (positionDiff == 0)
            {
                applyRandomOffset(ref position, timeDiff / 4d);
                catchObject.X = position;
                return;
            }

            if (Math.Abs(positionDiff * CatchPlayfield.BASE_WIDTH) < timeDiff / 3d)
                applyOffset(ref position, positionDiff);

            catchObject.X = position;

            lastPosition = position;
            lastStartTime = startTime;
        }

        /// <summary>
        /// Applies a random offset in a random direction to a position, ensuring that the final position remains within the boundary of the playfield.
        /// </summary>
        /// <param name="position">The position which the offset should be applied to.</param>
        /// <param name="maxOffset">The maximum offset, cannot exceed 20px.</param>
        private void applyRandomOffset(ref float position, double maxOffset)
        {
            bool right = RNG.NextBool();
            float rand = Math.Min(20, (float)RNG.NextDouble(0, Math.Max(0, maxOffset))) / CatchPlayfield.BASE_WIDTH;

            if (right)
            {
                // Clamp to the right bound
                if (position + rand <= 1)
                    position += rand;
                else
                    position -= rand;
            }
            else
            {
                // Clamp to the left bound
                if (position - rand >= 0)
                    position -= rand;
                else
                    position += rand;
            }
        }

        /// <summary>
        /// Applies an offset to a position, ensuring that the final position remains within the boundary of the playfield.
        /// </summary>
        /// <param name="position">The position which the offset should be applied to.</param>
        /// <param name="amount">The amount to offset by.</param>
        private void applyOffset(ref float position, float amount)
        {
            if (amount > 0)
            {
                // Clamp to the right bound
                if (position + amount < 1)
                    position += amount;
            }
            else
            {
                // Clamp to the left bound
                if (position + amount > 0)
                    position += amount;
            }
        }
    }
}
