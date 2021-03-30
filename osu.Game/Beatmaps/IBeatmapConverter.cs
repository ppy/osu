// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Provides functionality to convert a <see cref="IBeatmap"/> for a <see cref="Ruleset"/>.
    /// </summary>
    public interface IBeatmapConverter
    {
        /// <summary>
        /// Invoked when a <see cref="HitObject"/> has been converted.
        /// The first argument contains the <see cref="HitObject"/> that was converted.
        /// The second argument contains the <see cref="HitObject"/>s that were output from the conversion process.
        /// </summary>
        event Action<HitObject, IEnumerable<HitObject>> ObjectConverted;

        IBeatmap Beatmap { get; }

        /// <summary>
        /// Whether <see cref="Beatmap"/> can be converted by this <see cref="IBeatmapConverter"/>.
        /// </summary>
        bool CanConvert();

        /// <summary>
        /// Converts <see cref="Beatmap"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The converted Beatmap.</returns>
        IBeatmap Convert(CancellationToken cancellationToken = default);
    }
}
