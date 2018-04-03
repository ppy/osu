// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

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
    }
}
