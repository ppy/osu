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
        /// Used for a per-user gameplay session. Determines whether the mod is playable by an end user.
        /// </summary>
        User,

        /// <summary>
        /// Used as a "required mod" for a multiplayer match.
        /// </summary>
        MultiplayerRequired,

        /// <summary>
        /// Used as a "free mod" for a multiplayer match.
        /// </summary>
        MultiplayerFree,
    }
}
