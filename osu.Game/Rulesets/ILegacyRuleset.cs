// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring.Legacy;

namespace osu.Game.Rulesets
{
    public interface ILegacyRuleset
    {
        const int MAX_LEGACY_RULESET_ID = 3;

        /// <summary>
        /// Identifies the server-side ID of a legacy ruleset.
        /// </summary>
        int LegacyID { get; }

        /// <summary>
        /// Retrieves the number of mania keys required to play the beatmap.
        /// </summary>
        int GetKeyCount(IBeatmapInfo beatmapInfo, IReadOnlyList<Mod>? mods = null) => 0;

        ILegacyScoreSimulator CreateLegacyScoreSimulator();
    }
}
