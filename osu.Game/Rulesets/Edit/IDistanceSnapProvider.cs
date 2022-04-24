// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Edit
{
    public interface IDistanceSnapProvider : IPositionSnapProvider
    {
        /// <summary>
        /// The spacing multiplier applied to beat snap distances.
        /// </summary>
        /// <seealso cref="BeatmapInfo.DistanceSpacing"/>
        IBindable<double> DistanceSpacingMultiplier { get; }
    }
}
