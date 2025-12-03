// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
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
        /// Collects the constituent parts of a beatmap being verified.
        /// </summary>
        /// <param name="Working">
        /// Use this to access beatmap resources like its track, storyboard, waveform, or similar.
        /// </param>
        /// <param name="Playable">
        /// The <see cref="IBeatmap"/> in its actual playable state after beatmap conversion.
        /// Use this to inspect the actual beatmap contents, like its hitobjects, timing points, breaks, etc.
        /// </param>
        public record VerifiedBeatmap(IWorkingBeatmap Working, IBeatmap Playable);

        /// <summary>
        /// The difficulty level which the current beatmap is considered to be.
        /// </summary>
        public DifficultyRating InterpretedDifficulty;

        /// <summary>
        /// The current beatmap being checked.
        /// </summary>
        public readonly VerifiedBeatmap CurrentDifficulty;

        /// <summary>
        /// Other beatmaps in the same beatmapset.
        /// </summary>
        public readonly IReadOnlyList<VerifiedBeatmap> OtherDifficulties;

        /// <summary>
        /// All beatmaps in the same beatmapset.
        /// </summary>
        public IEnumerable<VerifiedBeatmap> AllDifficulties => OtherDifficulties.Prepend(CurrentDifficulty);

        public BeatmapVerifierContext(VerifiedBeatmap currentDifficulty, IReadOnlyList<VerifiedBeatmap> otherDifficulties, DifficultyRating difficultyRating)
        {
            CurrentDifficulty = currentDifficulty;
            InterpretedDifficulty = difficultyRating;
            OtherDifficulties = otherDifficulties;
        }

        /// <summary>
        /// Backwards-compatible constructor that allows creating a context from a single playable and working beatmap.
        /// </summary>
        public BeatmapVerifierContext(IBeatmap beatmap, IWorkingBeatmap workingBeatmap, DifficultyRating difficultyRating = DifficultyRating.ExpertPlus)
            : this(new VerifiedBeatmap(workingBeatmap, beatmap), [], difficultyRating)
        {
        }

        public static BeatmapVerifierContext Create(IBeatmap beatmap, IWorkingBeatmap workingBeatmap, DifficultyRating difficultyRating = DifficultyRating.ExpertPlus, BeatmapManager? beatmapManager = null)
        {
            var beatmapSet = beatmap.BeatmapInfo.BeatmapSet;

            var current = new VerifiedBeatmap(workingBeatmap, beatmap);

            if (beatmapSet?.Beatmaps == null || beatmapSet.Beatmaps.Count == 1)
                return new BeatmapVerifierContext(current, [], difficultyRating);

            var others = new List<VerifiedBeatmap>();

            foreach (var info in beatmapSet.Beatmaps)
            {
                if (info.Equals(beatmap.BeatmapInfo))
                    continue;

                var otherWorking = beatmapManager?.GetWorkingBeatmap(info);
                var otherPlayable = otherWorking?.GetPlayableBeatmap(info.Ruleset);

                if (otherWorking != null && otherPlayable != null)
                    others.Add(new VerifiedBeatmap(otherWorking, otherPlayable));
            }

            return new BeatmapVerifierContext(current, others, difficultyRating);
        }
    }
}
