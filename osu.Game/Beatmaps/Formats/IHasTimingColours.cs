// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public interface IHasTimingColours
    {
        /// <summary>
        /// A list of non-default timing colours.
        /// null in this context means "unspecified".
        ///
        /// If index 0 is specified, that should be used as the default
        /// for unspecified indexes.
        /// </summary>
        List<Color4?> CustomTimingColours { get; set; }
        /// <summary>
        /// Get the colour corresponding to the specified beat divisor.
        /// </summary>
        Color4 GetTimingColourFor(int beatDivisor);
    }
}
