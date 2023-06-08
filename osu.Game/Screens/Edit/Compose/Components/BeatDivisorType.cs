// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Compose.Components
{
    public enum BeatDivisorType
    {
        /// <summary>
        /// Most common divisors, all with denominators being powers of two.
        /// </summary>
        Common,

        /// <summary>
        /// Divisors with denominators divisible by 3.
        /// </summary>
        Triplets,

        /// <summary>
        /// Fully arbitrary/custom beat divisors.
        /// </summary>
        Custom,
    }
}
