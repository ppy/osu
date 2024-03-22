// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// The base lookup type to be used with <see cref="ISkin.GetDrawableComponent"/>.
    /// Should be implemented as necessary to add further criteria to lookups, which are usually consumed by ruleset transformers or legacy lookup cases.
    /// </summary>
    /// <remarks>
    /// Implementations of <see cref="ISkin.GetDrawableComponent"/> should match on types implementing this interface
    /// to scope particular lookup variations. Using this, a ruleset or skin implementation could make its own lookup
    /// type to scope away from more global contexts.
    ///
    /// More commonly, a ruleset could make use of <see cref="GameplaySkinComponentLookup{T}"/> to do a simple lookup based on
    /// a provided enum.
    /// </remarks>
    public interface ISkinComponentLookup : IEquatable<ISkinComponentLookup>
    {
        object Target { get; }

        RulesetInfo? Ruleset { get; }
    }
}
