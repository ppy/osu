// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// The context which the beatmap verifier provides to checks it runs.
    /// Contains information about what is being verified and how that should be done.
    /// </summary>
    public class BeatmapVerifierContext
    {
        /// <summary>
        /// The working beatmap instance of the current beatmap.
        /// </summary>
        public readonly IWorkingBeatmap WorkingBeatmap;

        /// <summary>
        /// The difficulty level which the current beatmap is considered to be.
        /// </summary>
        public readonly Bindable<DifficultyRating> InterpretedDifficulty;

        public BeatmapVerifierContext(IWorkingBeatmap workingBeatmap, DifficultyRating difficultyRating = DifficultyRating.ExpertPlus)
        {
            WorkingBeatmap = workingBeatmap;
            InterpretedDifficulty = new Bindable<DifficultyRating>(difficultyRating);
        }
    }
}
