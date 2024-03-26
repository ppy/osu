// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A ruleset-agnostic beatmap verifier that identifies issues in common metadata or mapping standards.
    /// </summary>
    public class BeatmapVerifier : IBeatmapVerifier
    {
        private readonly List<ICheck> checks = new List<ICheck>
        {
            // Resources
            new CheckBackgroundPresence(),
            new CheckBackgroundQuality(),
            new CheckVideoResolution(),

            // Audio
            new CheckAudioPresence(),
            new CheckAudioQuality(),
            new CheckMutedObjects(),
            new CheckFewHitsounds(),
            new CheckTooShortAudioFiles(),
            new CheckAudioInVideo(),
            new CheckDelayedHitsounds(),

            // Files
            new CheckZeroByteFiles(),

            // Compose
            new CheckUnsnappedObjects(),
            new CheckConcurrentObjects(),
            new CheckZeroLengthObjects(),
            new CheckDrainLength(),
            new CheckUnusedAudioAtEnd(),

            // Timing
            new CheckPreviewTime(),

            // Events
            new CheckBreaks(),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            return checks.SelectMany(check => check.Run(context));
        }
    }
}
