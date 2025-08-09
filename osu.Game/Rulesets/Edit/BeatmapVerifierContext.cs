// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        /// All playable beatmap difficulties in the same beatmapset, including the current beatmap.
        /// </summary>
        public readonly IReadOnlyList<IBeatmap> BeatmapsetDifficulties;

        /// <summary>
        /// The working beatmapset difficulties, including the current working beatmap.
        /// </summary>
        public readonly IReadOnlyList<IWorkingBeatmap> WorkingBeatmapsetDifficulties;

        public BeatmapVerifierContext(IBeatmap beatmap, IWorkingBeatmap workingBeatmap, DifficultyRating difficultyRating = DifficultyRating.ExpertPlus, IReadOnlyList<IBeatmap>? beatmapsetDifficulties = null, IReadOnlyList<IWorkingBeatmap>? workingBeatmapsetDifficulties = null)
        {
            Beatmap = beatmap;
            WorkingBeatmap = workingBeatmap;
            InterpretedDifficulty = difficultyRating;
            BeatmapsetDifficulties = beatmapsetDifficulties ?? new List<IBeatmap> { beatmap };
            WorkingBeatmapsetDifficulties = workingBeatmapsetDifficulties ?? new List<IWorkingBeatmap> { workingBeatmap };
        }

        public static BeatmapVerifierContext Create(IBeatmap beatmap, IWorkingBeatmap workingBeatmap, DifficultyRating difficultyRating = DifficultyRating.ExpertPlus, BeatmapManager? beatmapManager = null)
        {
            var beatmapSet = beatmap.BeatmapInfo.BeatmapSet;

            if (beatmapSet?.Beatmaps == null)
            {
                return new BeatmapVerifierContext(beatmap, workingBeatmap, difficultyRating, new[] { beatmap });
            }

            var difficulties = new List<IBeatmap>();
            var workingDifficulties = new List<IWorkingBeatmap>();

            foreach (var beatmapInfo in beatmapSet.Beatmaps)
            {
                // Use the current beatmap if it matches this BeatmapInfo
                if (beatmapInfo.Equals(beatmap.BeatmapInfo))
                {
                    difficulties.Add(beatmap);
                    workingDifficulties.Add(workingBeatmap);
                    continue;
                }

                // Resolve other difficulties using BeatmapManager if available
                var working = beatmapManager?.GetWorkingBeatmap(beatmapInfo);
                if (working != null)
                    workingDifficulties.Add(working);

                var playable = working?.GetPlayableBeatmap(beatmapInfo.Ruleset);
                if (playable != null)
                    difficulties.Add(playable);
            }

            return new BeatmapVerifierContext(beatmap, workingBeatmap, difficultyRating, difficulties, workingDifficulties);
        }
    }
}
