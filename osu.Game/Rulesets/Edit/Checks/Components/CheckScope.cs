// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public enum CheckScope
    {
        /// <summary>
        /// Run checks that apply to the current difficulty.
        /// </summary>
        Difficulty,

        /// <summary>
        /// Run checks that apply to the beatmapset as a whole.
        /// </summary>
        Beatmapset,
    }
}
