// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Localisation;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A ruleset-agnostic beatmap verifier that identifies issues in common metadata or mapping standards.
    /// </summary>
    public class BeatmapVerifier : IBeatmapVerifier
    {
        /// <summary>
        /// An <see cref="Issue"/> annotated with the difficulty or beatmapset scope it was found under.
        /// </summary>
        public record ScopedIssue(Issue Issue, LocalisableString Scope);

        private readonly List<ICheck> checks = new List<ICheck>
        {
            // Resources
            new CheckBackgroundPresence(),
            new CheckBackgroundQuality(),
            new CheckVideoResolution(),
            new CheckVideoUsage(),

            // Audio
            new CheckAudioPresence(),
            new CheckAudioQuality(),
            new CheckMutedObjects(),
            new CheckTooShortAudioFiles(),
            new CheckAudioInVideo(),
            new CheckDelayedHitsounds(),
            new CheckSongFormat(),
            new CheckHitsoundsFormat(),
            new CheckInconsistentAudio(),

            // Files
            new CheckZeroByteFiles(),

            // Compose
            new CheckUnsnappedObjects(),
            new CheckZeroLengthObjects(),
            new CheckDrainLength(),
            new CheckUnusedAudioAtEnd(),

            // Timing
            new CheckPreviewTime(),
            new CheckInconsistentTimingControlPoints(),

            // Events
            new CheckBreaks(),

            // Metadata
            new CheckTitleMarkers(),
            new CheckInconsistentMetadata(),
            new CheckMissingGenreLanguage(),

            // Settings
            new CheckInconsistentSettings(),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            return checks.SelectMany(check => check.Run(context));
        }

        /// <summary>
        /// Runs checks across an entire beatmapset: set-scoped checks once, difficulty-scoped checks per difficulty.
        /// </summary>
        public static IEnumerable<ScopedIssue> RunForBeatmapSet(IWorkingBeatmap currentWorking, BeatmapManager? beatmapManager)
        {
            var playable = currentWorking.GetPlayableBeatmap(currentWorking.BeatmapInfo.Ruleset);
            var context = BeatmapVerifierContext.Create(
                playable,
                currentWorking,
                StarDifficulty.GetDifficultyRating(currentWorking.BeatmapInfo.StarRating),
                beatmapManager);

            return RunForBeatmapSet(context.AllDifficulties.ToList(), context.CurrentDifficulty);
        }

        /// <summary>
        /// Runs checks across the provided difficulties.
        /// Set-scoped checks use <paramref name="preferredCurrent"/> as the current difficulty.
        /// </summary>
        public static IEnumerable<ScopedIssue> RunForBeatmapSet(
            IReadOnlyList<BeatmapVerifierContext.VerifiedBeatmap> allDifficulties,
            BeatmapVerifierContext.VerifiedBeatmap preferredCurrent)
        {
            var generalVerifier = new BeatmapVerifier();
            var loadedTracks = ensureTracksAvailable(allDifficulties, preferredCurrent);

            try
            {
                var results = new List<ScopedIssue>();

                results.AddRange(collect(generalVerifier, allDifficulties, preferredCurrent, CheckScope.BeatmapSet, EditorStrings.CheckEntireBeatmapSet));

                foreach (var difficulty in allDifficulties)
                {
                    results.AddRange(collect(
                        generalVerifier,
                        allDifficulties,
                        difficulty,
                        CheckScope.Difficulty,
                        difficulty.Playable.BeatmapInfo.DifficultyName));
                }

                return results;
            }
            finally
            {
                foreach (var track in loadedTracks)
                    track.Dispose();
            }
        }

        /// <summary>
        /// Runs general and ruleset verifiers for <paramref name="current"/>, keeping only issues of <paramref name="scope"/>.
        /// </summary>
        private static IEnumerable<ScopedIssue> collect(
            BeatmapVerifier generalVerifier,
            IReadOnlyList<BeatmapVerifierContext.VerifiedBeatmap> allDifficulties,
            BeatmapVerifierContext.VerifiedBeatmap current,
            CheckScope scope,
            LocalisableString scopeLabel)
        {
            var context = new BeatmapVerifierContext(
                current,
                allDifficulties.Where(d => d != current).ToList(),
                StarDifficulty.GetDifficultyRating(current.Playable.BeatmapInfo.StarRating));

            var issues = generalVerifier.Run(context);

            var rulesetVerifier = current.Playable.BeatmapInfo.Ruleset.CreateInstance().CreateBeatmapVerifier();
            if (rulesetVerifier != null)
                issues = issues.Concat(rulesetVerifier.Run(context));

            foreach (var issue in issues.Where(i => i.Check.Metadata.Scope == scope))
                yield return new ScopedIssue(issue, scopeLabel);
        }

        /// <summary>
        /// Shares the editor track across same-audio difficulties.
        /// </summary>
        private static List<Track> ensureTracksAvailable(
            IReadOnlyList<BeatmapVerifierContext.VerifiedBeatmap> allDifficulties,
            BeatmapVerifierContext.VerifiedBeatmap preferredCurrent)
        {
            var loadedTracks = new List<Track>();

            if (preferredCurrent.Working is not WorkingBeatmap referenceWorking)
                return loadedTracks;

            foreach (var difficulty in allDifficulties)
            {
                if (difficulty == preferredCurrent)
                    continue;

                if (difficulty.Working is not WorkingBeatmap working || working.TrackLoaded)
                    continue;

                if (referenceWorking.TrackLoaded && referenceWorking.TryTransferTrack(working))
                    continue;

                loadedTracks.Add(working.LoadTrack());
            }

            return loadedTracks;
        }
    }
}
