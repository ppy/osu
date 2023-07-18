// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A class which can run against a beatmap and surface issues to the user which could go against known criteria or hinder gameplay.
    /// </summary>
    public interface IBeatmapVerifier
    {
        public IEnumerable<Issue> Run(BeatmapVerifierContext context);
    }
}
