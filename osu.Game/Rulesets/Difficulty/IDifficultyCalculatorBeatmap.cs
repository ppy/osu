// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// Extended <see cref="IBeatmap"/> with functions needed in dififculty calculation.
    /// </summary>
    public interface IDifficultyCalculatorBeatmap : IBeatmap
    {
        /// <summary>
        /// Finds the maximum achievable combo by hitting all <see cref="HitObject"/>s in a beatmap.
        /// </summary>
        int GetMaxCombo();

        /// <summary>
        /// Finds amount of <see cref="HitObject"/>s that have given type. This doesn't include nested hit objects.
        /// </summary>
        int GetHitObjectCountOf(Type type);
    }
}
