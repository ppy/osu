// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class SkinTimingColourLookup
    {
        /// <summary>
        /// The divisor to get a color for.
        /// </summary>
        public readonly int SnapDivisor;

        public SkinTimingColourLookup(int snapDivisor)
        {
            SnapDivisor = snapDivisor;
        }
    }
}
