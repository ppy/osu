// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that adds the ability to shake its contents.
    /// </summary>
    public class ShakeContainer : Container
    {
        /// <summary>
        /// The length of a single shake.
        /// </summary>
        public float ShakeDuration = 80;

        /// <summary>
        /// Total number of shakes. May be shortened if possible.
        /// </summary>
        public float TotalShakes = 4;

        /// <summary>
        /// Pixels of displacement per shake.
        /// </summary>
        public float ShakeMagnitude = 8;

        /// <summary>
        /// Shake the contents of this container.
        /// </summary>
        /// <param name="maximumLength">The maximum length the shake should last.</param>
        public void Shake(double? maximumLength = null)
        {
            const float shake_amount = 8;

            // if we don't have enough time, don't bother shaking.
            if (maximumLength < ShakeDuration * 2)
                return;

            var sequence = this.MoveToX(shake_amount, ShakeDuration / 2, Easing.OutSine).Then()
                               .MoveToX(-shake_amount, ShakeDuration, Easing.InOutSine).Then();

            // if we don't have enough time for the second shake, skip it.
            if (!maximumLength.HasValue || maximumLength >= ShakeDuration * 4)
                sequence = sequence
                           .MoveToX(shake_amount, ShakeDuration, Easing.InOutSine).Then()
                           .MoveToX(-shake_amount, ShakeDuration, Easing.InOutSine).Then();

            sequence.MoveToX(0, ShakeDuration / 2, Easing.InSine);
        }
    }
}
