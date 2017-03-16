// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Converts a Beatmap for another mode.
    /// </summary>
    /// <typeparam name="T">The type of HitObject stored in the Beatmap.</typeparam>
    public interface IBeatmapConverter<T> where T : HitObject
    {
        /// <summary>
        /// Converts a Beatmap to another mode.
        /// </summary>
        /// <param name="original">The original Beatmap.</param>
        /// <returns>The converted Beatmap.</returns>
        Beatmap<T> Convert(Beatmap original);
    }
}
