// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a curve.
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public interface IHasPathWithRepeats : IHasPath, IHasRepeats
    {
    }

    public static class HasPathWithRepeatsExtensions
    {
        /// <summary>
        /// Computes the position on the curve relative to how much of the <see cref="HitObject"/> has been completed.
        /// </summary>
        /// <param name="obj">The curve.</param>
        /// <param name="progress">[0, 1] where 0 is the start time of the <see cref="HitObject"/> and 1 is the end time of the <see cref="HitObject"/>.</param>
        /// <returns>The position on the curve.</returns>
        public static Vector2 CurvePositionAt(this IHasPathWithRepeats obj, double progress)
            => obj.Path.PositionAt(obj.ProgressAt(progress));

        /// <summary>
        /// Computes the progress along the curve relative to how much of the <see cref="HitObject"/> has been completed.
        /// </summary>
        /// <param name="obj">The curve.</param>
        /// <param name="progress">[0, 1] where 0 is the start time of the <see cref="HitObject"/> and 1 is the end time of the <see cref="HitObject"/>.</param>
        /// <returns>[0, 1] where 0 is the beginning of the curve and 1 is the end of the curve.</returns>
        public static double ProgressAt(this IHasPathWithRepeats obj, double progress)
        {
            double p = progress * obj.SpanCount() % 1;
            if (obj.SpanAt(progress) % 2 == 1)
                p = 1 - p;
            return p;
        }

        /// <summary>
        /// Determines which span of the curve the progress point is on.
        /// </summary>
        /// <param name="obj">The curve.</param>
        /// <param name="progress">[0, 1] where 0 is the beginning of the curve and 1 is the end of the curve.</param>
        /// <returns>[0, SpanCount) where 0 is the first run.</returns>
        public static int SpanAt(this IHasPathWithRepeats obj, double progress)
            => (int)(progress * obj.SpanCount());
    }
}
