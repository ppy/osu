// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        /// Shake the contents of this container.
        /// </summary>
        /// <param name="maximumLength">The maximum length the shake should last.</param>
        public void Shake(double maximumLength)
        {
            const float shake_amount = 8;
            const float shake_duration = 30;

            // if we don't have enough time, don't bother shaking.
            if (maximumLength < shake_duration * 2)
                return;

            var sequence = this.MoveToX(shake_amount, shake_duration / 2, Easing.OutSine).Then()
                               .MoveToX(-shake_amount, shake_duration, Easing.InOutSine).Then();

            // if we don't have enough time for the second shake, skip it.
            if (maximumLength > shake_duration * 4)
                sequence = sequence
                           .MoveToX(shake_amount, shake_duration, Easing.InOutSine).Then()
                           .MoveToX(-shake_amount, shake_duration, Easing.InOutSine).Then();

            sequence.MoveToX(0, shake_duration / 2, Easing.InSine);
        }
    }
}
