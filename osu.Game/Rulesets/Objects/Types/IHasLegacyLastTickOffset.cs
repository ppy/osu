// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A type of <see cref="HitObject"/> which may require the last tick to be offset.
    /// This is specific to osu!stable conversion, and should not be used elsewhere.
    /// </summary>
    public interface IHasLegacyLastTickOffset
    {
        double LegacyLastTickOffset { get; }
    }
}
