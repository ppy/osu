// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Mania.Edit.Checks;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaBeatmapVerifier : IBeatmapVerifier
    {
        private readonly List<ICheck> checks = new List<ICheck>
        {
            // Settings
            new CheckKeyCount(),
            new CheckManiaAbnormalDifficultySettings(),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            return checks.SelectMany(check => check.Run(context));
        }
    }
}
