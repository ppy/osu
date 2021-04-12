// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit
{
    public abstract class BeatmapVerifier
    {
        /// <summary>
        /// Checks which are performed regardless of ruleset.
        /// These handle things like beatmap metadata, timing, and other ruleset agnostic elements.
        /// </summary>
        private readonly IReadOnlyList<ICheck> generalChecks = new List<ICheck>
        {
            new CheckBackground()
        };

        public virtual IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            return generalChecks.SelectMany(check => check.Run(beatmap));
        }
    }
}
