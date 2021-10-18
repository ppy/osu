// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    /// <remarks>
    /// Note that going forward, this control point type should always be assigned directly to HitObjects.
    /// </remarks>
    public class DifficultyControlPoint : ControlPoint
    {
        public static readonly DifficultyControlPoint DEFAULT = new DifficultyControlPoint
        {
            SliderVelocityBindable = { Disabled = true },
        };

        /// <summary>
        /// The slider velocity at this control point.
        /// </summary>
        public readonly BindableDouble SliderVelocityBindable = new BindableDouble(1)
        {
            Precision = 0.01,
            Default = 1,
            MinValue = 0.1,
            MaxValue = 10
        };

        public override Color4 GetRepresentingColour(OsuColour colours) => colours.Lime1;

        /// <summary>
        /// The slider velocity at this control point.
        /// </summary>
        public double SliderVelocity
        {
            get => SliderVelocityBindable.Value;
            set => SliderVelocityBindable.Value = value;
        }

        public override bool IsRedundant(ControlPoint existing)
            => existing is DifficultyControlPoint existingDifficulty
               && SliderVelocity == existingDifficulty.SliderVelocity;

        public override void CopyFrom(ControlPoint other)
        {
            SliderVelocity = ((DifficultyControlPoint)other).SliderVelocity;

            base.CopyFrom(other);
        }
    }
}
