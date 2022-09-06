// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for any mod that should only be selectable on specific platforms.
    /// </summary>
    public interface IPlatformExclusiveMod
    {
        bool AcceptPlatform();
    }
}
