// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Denotes a mod which removes timed inputs from a ruleset which would usually have them.
    /// </summary>
    /// <remarks>
    /// This will be used, for instance, to omit showing offset calibration UI post-gameplay.
    /// </remarks>
    public interface IHasNoTimedInputs
    {
    }
}
