// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Marker interface for any mod which requires the approach circles to be visible.
    /// Used for incompatibility with <see cref="IHidesApproachCircles"/>.
    /// </summary>
    /// <remarks>
    /// Note that this is only a marker interface for incompatibility purposes, it does not change any gameplay behaviour.
    /// </remarks>
    public interface IRequiresApproachCircles
    {
    }
}
