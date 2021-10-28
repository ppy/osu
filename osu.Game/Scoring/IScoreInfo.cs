// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;

namespace osu.Game.Scoring
{
    public interface IScoreInfo : IHasOnlineID<long>
    {
        User User { get; }

        long TotalScore { get; }

        int MaxCombo { get; }

        double Accuracy { get; }

        bool HasReplay { get; }

        DateTimeOffset Date { get; }

        double? PP { get; }

        IBeatmapInfo Beatmap { get; }

        Dictionary<HitResult, int> Statistics { get; }

        IRulesetInfo Ruleset { get; }

        public ScoreRank Rank { get; }

        // Mods is currently missing from this interface as the `IMod` class has properties which can't be fulfilled by `APIMod`,
        // but also doesn't expose `Settings`. We can consider how to implement this in the future if required.
    }
}
