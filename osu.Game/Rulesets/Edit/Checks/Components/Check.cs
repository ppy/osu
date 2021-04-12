// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public abstract class Check
    {
        /// <summary>
        /// The metadata for this check.
        /// </summary>
        public abstract CheckMetadata Metadata { get; }

        /// <summary>
        /// All possible templates for issues that this check may return.
        /// </summary>
        public abstract IEnumerable<IssueTemplate> PossibleTemplates { get; }

        /// <summary>
        /// Runs this check and returns any issues detected for the provided beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to run the check on.</param>
        public abstract IEnumerable<Issue> Run(IBeatmap beatmap);

        protected Check()
        {
            foreach (var template in PossibleTemplates)
                template.Origin = this;
        }
    }
}
