// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A lookup type intended for use for skinnable gameplay components (not HUD level components).
    /// </summary>
    /// <remarks>
    /// The most common usage of this class is for ruleset-specific skinning implementations, but it can also be used directly
    /// (see <see cref="DrawableJudgement"/>'s usage for <see cref="HitResult"/>) where ruleset-agnostic elements are required.
    /// </remarks>
    /// <typeparam name="T">An enum lookup type.</typeparam>
    public class GameplaySkinComponentLookup<T> : ISkinComponentLookup, IEquatable<GameplaySkinComponentLookup<T>>
        where T : Enum
    {
        public readonly T Component;

        public GameplaySkinComponentLookup(T component)
        {
            Component = component;
        }

        public virtual RulesetInfo? Ruleset => null;

        public override string ToString()
        {
            if (Ruleset == null)
                return Component.ToString();

            return $"{Component.ToString()} (\"{Ruleset.Name}\" only)";
        }

        public bool Equals(GameplaySkinComponentLookup<T>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return EqualityComparer<T>.Default.Equals(Component, other.Component);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((GameplaySkinComponentLookup<T>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Component);
        }

        bool IEquatable<ISkinComponentLookup>.Equals(ISkinComponentLookup? other)
            => other is GameplaySkinComponentLookup<T> lookup && Equals(lookup);

        object ISkinComponentLookup.Target => Component;
    }
}
