// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class DifficultyControlPoint : ControlPoint
    {
        /// <summary>
        /// The speed multiplier at this control point.
        /// </summary>
        public double SpeedMultiplier
        {
            get => speedMultiplier;
            set => speedMultiplier = MathHelper.Clamp(value, 0.1, 10);
        }

        private double speedMultiplier = 1;

        public override bool EquivalentTo(ControlPoint other)
            => base.EquivalentTo(other)
               && other is DifficultyControlPoint difficulty
               && SpeedMultiplier.Equals(difficulty.SpeedMultiplier);
    }
}
