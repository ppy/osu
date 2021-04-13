// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A ruleset-agnostic beatmap converter that identifies issues in common metadata or mapping standards.
    /// </summary>
    public class BeatmapVerifier : IBeatmapVerifier
    {
        private readonly List<ICheck> checks = new List<ICheck>
        {
            new CheckBackground(),
        };

        public IEnumerable<Issue> Run(IBeatmap beatmap) => checks.SelectMany(check => check.Run(beatmap));
    }
}
