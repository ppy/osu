// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Scoring
{
    public interface IScoreInfo : IHasOnlineID<long>
    {
        APIUser User { get; }

        long TotalScore { get; }

        int MaxCombo { get; }

        double Accuracy { get; }

        bool HasReplay { get; }

        DateTimeOffset Date { get; }

        double? PP { get; }

        IBeatmapInfo Beatmap { get; }

        IRulesetInfo Ruleset { get; }

        ScoreRank Rank { get; }

        // Mods is currently missing from this interface as the `IMod` class has properties which can't be fulfilled by `APIMod`,
        // but also doesn't expose `Settings`. We can consider how to implement this in the future if required.

        // Statistics is also missing. This can be reconsidered once changes in serialisation have been completed.
    }
}
