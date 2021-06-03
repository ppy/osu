// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    /// <summary>
    /// A specific check that can be run on a beatmap to verify or find issues.
    /// </summary>
    public interface ICheck
    {
        /// <summary>
        /// The metadata for this check.
        /// </summary>
        public CheckMetadata Metadata { get; }

        /// <summary>
        /// All possible templates for issues that this check may return.
        /// </summary>
        public IEnumerable<IssueTemplate> PossibleTemplates { get; }

        /// <summary>
        /// Runs this check and returns any issues detected for the provided beatmap.
        /// </summary>
        /// <param name="context">The beatmap verifier context associated with the beatmap.</param>
        public IEnumerable<Issue> Run(BeatmapVerifierContext context);
    }
}
