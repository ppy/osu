// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Scoring
{
    public enum HitDetail
    {
        /// <summary>
        /// Indicates that the object was hit early
        /// </summary>
        Fast,

        /// <summary>
        /// Indicates that the object was hit late
        /// </summary>
        Slow,

        /// <summary>
        /// Indicates that the object was right on time
        /// </summary>
        Flawless,
    }
}
