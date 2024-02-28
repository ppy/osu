// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Provides various data sources which allow for synchronising visuals to a known beat.
    /// Primarily intended for use with <see cref="BeatSyncedContainer"/>.
    /// </summary>
    [Cached]
    public interface IBeatSyncProvider : IHasAmplitudes
    {
        /// <summary>
        /// Access any available control points from a beatmap providing beat sync. If <c>null</c>, no current provider is available.
        /// </summary>
        ControlPointInfo? ControlPoints { get; }

        /// <summary>
        /// Access a clock currently responsible for providing beat sync.
        /// </summary>
        IClock Clock { get; }
    }
}
