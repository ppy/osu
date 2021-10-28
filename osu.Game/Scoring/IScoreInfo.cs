// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Users;

namespace osu.Game.Scoring
{
    public interface IScoreInfo : IHasOnlineID<long>
    {
        int TotalScore { get; set; }

        int MaxCombo { get; set; }

        User User { get; set; }

        long OnlineScoreID { get; set; }

        bool Replay { get; set; }

        DateTimeOffset Date { get; set; }

        BeatmapInfo BeatmapInfo { get; set; }

        double Accuracy { get; set; }

        double? PP { get; set; }

        BeatmapMetadata Metadata { get; set; }

        Dictionary<string, int> Statistics { get; set; }

        int OnlineRulesetID { get; set; }

        string[] Mods { get; set; }

        public ScoreRank Rank { get; set; }
    }
}
