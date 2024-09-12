// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Represents a lookup of a collection of elements that make up a particular skinnable <see cref="GlobalSkinnableContainers"/> of the game.
    /// </summary>
    public class GlobalSkinnableContainerLookup : ISkinComponentLookup, IEquatable<GlobalSkinnableContainerLookup>
    {
        /// <summary>
        /// The target area / layer of the game for which skin components will be returned.
        /// </summary>
        public readonly GlobalSkinnableContainers Lookup;

        /// <summary>
        /// The ruleset for which skin components should be returned.
        /// A <see langword="null"/> value means that returned components are global and should be applied for all rulesets.
        /// </summary>
        public readonly RulesetInfo? Ruleset;

        public GlobalSkinnableContainerLookup(GlobalSkinnableContainers lookup, RulesetInfo? ruleset = null)
        {
            Lookup = lookup;
            Ruleset = ruleset;
        }

        public override string ToString()
        {
            if (Ruleset == null) return Lookup.GetDescription();

            return $"{Lookup.GetDescription()} (\"{Ruleset.Name}\" only)";
        }

        public bool Equals(GlobalSkinnableContainerLookup? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Lookup == other.Lookup && (ReferenceEquals(Ruleset, other.Ruleset) || Ruleset?.Equals(other.Ruleset) == true);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((GlobalSkinnableContainerLookup)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Lookup, Ruleset);
        }
    }
}
