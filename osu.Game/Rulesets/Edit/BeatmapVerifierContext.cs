// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        public BeatmapVerifierContext(IBeatmap beatmap, IWorkingBeatmap workingBeatmap, DifficultyRating difficultyRating = DifficultyRating.ExpertPlus)
        {
            Beatmap = beatmap;
            WorkingBeatmap = workingBeatmap;
            InterpretedDifficulty = difficultyRating;
        }
    }
}
