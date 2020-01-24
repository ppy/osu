// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Replays
{
    public abstract class AutoGenerator : IAutoGenerator
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
        protected IBeatmap Beatmap;

        #endregion

        protected AutoGenerator(IBeatmap beatmap)
        {
            Beatmap = beatmap;
        }

        #region Constants

        // Shared amongst all modes
        protected const double KEY_UP_DELAY = 50;

        #endregion

        protected virtual HitObject GetNextObject(int currentIndex)
        {
            if (currentIndex >= Beatmap.HitObjects.Count - 1)
                return null;

            return Beatmap.HitObjects[currentIndex + 1];
        }
    }
}
