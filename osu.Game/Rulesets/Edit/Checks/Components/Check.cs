// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public abstract class Check
    {
        /// <summary>
        /// Returns the <see cref="CheckMetadata"/> for this check.
        /// Basically, its information.
        /// </summary>
        /// <returns></returns>
        public abstract CheckMetadata Metadata();

        /// <summary>
        /// The templates for issues that this check may use.
        /// Basically, what issues this check can detect.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<IssueTemplate> Templates();

        /// <summary>
        /// Returns zero, one, or several issues detected by this
        /// check on the given beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to run the check on.</param>
        /// <returns></returns>
        public abstract IEnumerable<Issue> Run(IBeatmap beatmap);

        protected Check()
        {
            foreach (var template in Templates())
                template.Origin = this;
        }
    }
}
