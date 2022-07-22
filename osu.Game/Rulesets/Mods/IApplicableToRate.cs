// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface that should be implemented by mods that affect the track playback speed,
    /// and in turn, values of the track rate.
    /// </summary>
    public interface IApplicableToRate : IApplicableToAudio
    {
        /// <summary>
        /// Returns the playback rate at <paramref name="time"/> after this mod is applied.
        /// </summary>
        /// <param name="time">The time instant at which the playback rate is queried.</param>
        /// <param name="rate">The playback rate before applying this mod.</param>
        /// <returns>The playback rate after applying this mod.</returns>
        double ApplyToRate(double time, double rate = 1);
    }
}
