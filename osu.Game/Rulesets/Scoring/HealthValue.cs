// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// Represents a point-in-time health value.
    /// In use for HP graphs which are persisted to replays.
    /// </summary>
    public struct HealthValue
    {
        /// <summary>
        /// Time instant at which the health value was recorded.
        /// </summary>
        public double Time { get; }

        /// <summary>
        /// The health at <see cref="Time"/>, relative to max (range [0, 1]).
        /// </summary>
        public double Health { get; }

        public HealthValue(double time, double health)
        {
            Time = time;
            Health = health;
        }
    }
}
