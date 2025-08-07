// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// Represents the context provided by the beatmap verifier to the checks it runs.
    /// Contains information about what is being checked and how it should be checked.
    /// </summary>
    public class BeatmapVerifierContext
    {
        /// <summary>
        /// The playable beatmap instance of the current beatmap.
        /// </summary>
        public readonly IBeatmap Beatmap;

        /// <summary>
        /// The working beatmap instance of the current beatmap.
        /// </summary>
        public readonly IWorkingBeatmap WorkingBeatmap;

        /// <summary>
        /// The difficulty level which the current beatmap is considered to be.
        /// </summary>
        public DifficultyRating InterpretedDifficulty;

        /// <summary>
        /// All beatmap difficulties in the same beatmapset, including the current beatmap.
        /// </summary>
        public readonly IReadOnlyList<IBeatmap> BeatmapsetDifficulties;

        /// <summary>
        /// Creates a new <see cref="BeatmapVerifierContext"/> with the specified data.
        /// </summary>
        /// <param name="beatmap">The playable beatmap instance.</param>
        /// <param name="workingBeatmap">The working beatmap instance.</param>
        /// <param name="difficultyRating">The difficulty level of the beatmap.</param>
        /// <param name="beatmapsetDifficulties">All beatmap difficulties in the same beatmapset.</param>
        public BeatmapVerifierContext(IBeatmap beatmap, IWorkingBeatmap workingBeatmap, DifficultyRating difficultyRating = DifficultyRating.ExpertPlus, IReadOnlyList<IBeatmap>? beatmapsetDifficulties = null)
        {
            Beatmap = beatmap;
            WorkingBeatmap = workingBeatmap;
            InterpretedDifficulty = difficultyRating;
            BeatmapsetDifficulties = beatmapsetDifficulties ?? new List<IBeatmap> { beatmap };
        }

        /// <summary>
        /// Creates a new <see cref="BeatmapVerifierContext"/> with beatmap resolution.
        /// </summary>
        /// <param name="beatmap">The playable beatmap instance.</param>
        /// <param name="workingBeatmap">The working beatmap instance.</param>
        /// <param name="difficultyRating">The difficulty level of the beatmap.</param>
        /// <param name="beatmapResolver">Resolver function to resolve other difficulties in the beatmapset.</param>
        /// <returns>A new <see cref="BeatmapVerifierContext"/> with resolved beatmapset difficulties.</returns>
        public static BeatmapVerifierContext CreateWithBeatmapResolver(IBeatmap beatmap, IWorkingBeatmap workingBeatmap, DifficultyRating difficultyRating = DifficultyRating.ExpertPlus, Func<BeatmapInfo, IBeatmap?>? beatmapResolver = null)
        {
            var beatmapSet = beatmap.BeatmapInfo.BeatmapSet;

            if (beatmapSet?.Beatmaps == null)
            {
                return new BeatmapVerifierContext(beatmap, workingBeatmap, difficultyRating, new[] { beatmap });
            }

            var difficulties = new List<IBeatmap>();

            foreach (var beatmapInfo in beatmapSet.Beatmaps)
            {
                // Use the current beatmap if it matches this BeatmapInfo
                if (beatmapInfo.Equals(beatmap.BeatmapInfo))
                {
                    difficulties.Add(beatmap);
                    continue;
                }

                // Try to resolve other difficulties using the provided resolver
                var resolvedBeatmap = beatmapResolver?.Invoke(beatmapInfo);
                if (resolvedBeatmap != null)
                    difficulties.Add(resolvedBeatmap);
            }

            return new BeatmapVerifierContext(beatmap, workingBeatmap, difficultyRating, difficulties);
        }
    }
}
