// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// The usage of this mod to determine whether it's playable in such context.
    /// </summary>
    public enum ModUsage
    {
        /// <summary>
        /// Used for a per-user gameplay session.
        /// </summary>
        User,

        /// <summary>
        /// Used in multiplayer but must be applied to all users.
        /// This is generally the case for mods which affect the length of gameplay.
        /// </summary>
        MultiplayerRoomWide,

        /// <summary>
        /// Used in multiplayer either at a room or per-player level (i.e. "free mod").
        /// </summary>
        MultiplayerPerPlayer,
    }
}
