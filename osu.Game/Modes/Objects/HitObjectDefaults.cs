// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;

namespace osu.Game.Modes.Objects
{
    /// <summary>
    /// A set of default Beatmap values for HitObjects to consume.
    /// </summary>
    public class HitObjectDefaults
    {
        /// <summary>
        /// The Beatmap timing.
        /// </summary>
        public TimingInfo Timing;

        /// <summary>
        /// The Beatmap difficulty.
        /// </summary>
        public BaseDifficulty Difficulty;
    }
}
