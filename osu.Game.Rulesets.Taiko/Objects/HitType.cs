// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Objects
{
    /// <summary>
    /// The type of a <see cref="Hit"/>.
    /// </summary>
    public enum HitType
    {
        /// <summary>
        /// A <see cref="Hit"/> that can be hit by the centre portion of the drum.
        /// </summary>
        Centre,

        /// <summary>
        /// A <see cref="Hit"/> that can be hit by the rim portion of the drum.
        /// </summary>
        Rim
    }
}
