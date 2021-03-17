// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a positional length.
    /// </summary>
    public interface IHasDistance : IHasDuration
    {
        /// <summary>
        /// The positional length of the HitObject.
        /// </summary>
        double Distance { get; }
    }
}
