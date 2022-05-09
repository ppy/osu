// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that make adjustments to the track.
    /// </summary>
    public interface IApplicableToTrack : IApplicableMod
    {
        /// <summary>
        /// Use this method to apply any adjustments applicable for this mod to the supplied <paramref name="track"/>.
        /// </summary>
        /// <remarks>
        /// This method is the only valid point at which adjustments can be added safely.
        /// Storing the supplied <paramref name="track"/> reference and adding adjustments at a later date is not supported and will lead to incorrect operation.
        /// </remarks>
        void ApplyToTrack(ITrack track);
    }
}
