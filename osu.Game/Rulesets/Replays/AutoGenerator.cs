// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Replays
{
    public abstract class AutoGenerator<T> : IAutoGenerator
        where T : HitObject
    {
        /// <summary>
        /// Creates the auto replay and returns it.
        /// Every subclass of OsuAutoGeneratorBase should implement this!
        /// </summary>
        public abstract Replay Generate();

        #region Parameters

        /// <summary>
        /// The beatmap we're making.
        /// </summary>
        protected Beatmap<T> Beatmap;

        #endregion

        protected AutoGenerator(Beatmap<T> beatmap)
        {
            Beatmap = beatmap;
        }

        #region Constants

        // Shared amongst all modes
        protected const double KEY_UP_DELAY = 50;

        #endregion
    }
}
