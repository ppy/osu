// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Platform;
using osuTK;

namespace osu.Game.Extensions
{
    public static class DrawableExtensions
    {
        /// <summary>
        /// Shakes this drawable.
        /// </summary>
        /// <param name="target">The target to shake.</param>
        /// <param name="shakeDuration">The length of a single shake.</param>
        /// <param name="shakeMagnitude">Pixels of displacement per shake.</param>
        /// <param name="maximumLength">The maximum length the shake should last.</param>
        public static void Shake(this Drawable target, double shakeDuration = 80, float shakeMagnitude = 8, double? maximumLength = null)
        {
            // if we don't have enough time, don't bother shaking.
            if (maximumLength < shakeDuration * 2)
                return;

            var sequence = target.MoveToX(shakeMagnitude, shakeDuration / 2, Easing.OutSine).Then()
                                 .MoveToX(-shakeMagnitude, shakeDuration, Easing.InOutSine).Then();

            // if we don't have enough time for the second shake, skip it.
            if (!maximumLength.HasValue || maximumLength >= shakeDuration * 4)
            {
                sequence = sequence
                           .MoveToX(shakeMagnitude, shakeDuration, Easing.InOutSine).Then()
                           .MoveToX(-shakeMagnitude, shakeDuration, Easing.InOutSine).Then();
            }

            sequence.MoveToX(0, shakeDuration / 2, Easing.InSine);
        }

        /// <summary>
        /// Accepts a delta vector in screen-space coordinates and converts it to one which can be applied to this drawable's position.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        /// <param name="delta">A delta in screen-space coordinates.</param>
        /// <returns>The delta vector in Parent's coordinates.</returns>
        public static Vector2 ScreenSpaceDeltaToParentSpace(this Drawable drawable, Vector2 delta) =>
            drawable.Parent!.ToLocalSpace(drawable.Parent!.ToScreenSpace(Vector2.Zero) + delta);

        /// <summary>
        /// Some elements don't handle rewind correctly and fixing them is non-trivial.
        /// In the future we need a better solution to this, but as a temporary work-around, give these components the game-wide
        /// clock so they don't need to worry about rewind.
        ///
        /// This only works if input handling components handle OnPressed/OnReleased which results in a correct state while rewinding.
        ///
        /// This is kinda dodgy (and will cause weirdness when pausing gameplay) but is better than completely broken rewind.
        /// </summary>
        public static void ApplyGameWideClock(this Drawable drawable, GameHost host)
        {
            drawable.Clock = host.UpdateThread.Clock;
            drawable.ProcessCustomClock = false;
        }
    }
}
