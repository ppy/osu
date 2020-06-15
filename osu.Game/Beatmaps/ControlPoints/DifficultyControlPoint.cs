// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class DifficultyControlPoint : ControlPoint
    {
        /// <summary>
        /// The speed multiplier at this control point.
        /// </summary>
        public readonly BindableDouble SpeedMultiplierBindable = new BindableDouble(1)
        {
            Precision = 0.1,
            Default = 1,
            MinValue = 0.1,
            MaxValue = 10
        };

        /// <summary>
        /// The speed multiplier at this control point.
        /// </summary>
        public double SpeedMultiplier
        {
            get => SpeedMultiplierBindable.Value;
            set => SpeedMultiplierBindable.Value = value;
        }

        public override bool IsRedundant(ControlPoint existing)
            => existing is DifficultyControlPoint existingDifficulty
               && SpeedMultiplier == existingDifficulty.SpeedMultiplier;
    }
}
