// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public class GameplaySkinComponentLookup<T> : ISkinComponentLookup
        where T : Enum
    {
        public readonly T Component;

        public GameplaySkinComponentLookup(T component)
        {
            Component = component;
        }

        protected virtual string RulesetPrefix => string.Empty;
        protected virtual string ComponentName => Component.ToString();
    }
}
