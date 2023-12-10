// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public interface IHasSnapColours
    {
        /// <summary>
        /// Retrieves the list of snap colours for presentation only.
        /// </summary>
        IReadOnlyList<Color4>? SnapColours { get; }

        /// <summary>
        /// The list of custom snap colours.
        /// If non-empty, <see cref="SnapColours"/> will return these colours;
        /// if empty, <see cref="SnapColours"/> will fall back to default snap colours.
        /// </summary>
        List<Color4> CustomSnapColours { get; }
    }
}
