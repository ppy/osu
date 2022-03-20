// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// The usage of this mod to determine its playability.
    /// </summary>
    public enum ModUsage
    {
        /// <summary>
        /// In a solo gameplay session.
        /// </summary>
        User,

        /// <summary>
        /// In a multiplayer match, as a required mod.
        /// </summary>
        MultiplayerRequired,

        /// <summary>
        /// In a multiplayer match, as a "free" mod.
        /// </summary>
        MultiplayerFree,
    }
}
