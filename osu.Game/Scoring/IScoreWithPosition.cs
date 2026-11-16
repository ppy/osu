// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;

namespace osu.Game.Scoring
{
    /// <summary>
    /// Extension of <see cref="IScoreInfo"/> by supplying position information.
    /// Used in leaderboard contexts.
    /// </summary>
    public interface IScoreWithPosition : IScoreInfo
    {
        /// <summary>
        /// The numerical one-based position of this score on a leaderboard or other ordered list of scores.
        /// </summary>
        int? Position { get; }
    }

    /// <summary>
    /// Helper class to augment an arbitrary <see cref="IScoreInfo"/> instance with a position.
    /// </summary>
    public record ScoreWithPosition(IScoreInfo Score, int? Position) : IScoreWithPosition
    {
        public long OnlineID => Score.OnlineID;
        public IUser User => Score.User;
        public long TotalScore => Score.TotalScore;
        public int MaxCombo => Score.MaxCombo;
        public double Accuracy => Score.Accuracy;
        public long LegacyOnlineID => Score.LegacyOnlineID;
        public DateTimeOffset Date => Score.Date;
        public double? PP => Score.PP;
        public IBeatmapInfo? Beatmap => Score.Beatmap;
        public IRulesetInfo Ruleset => Score.Ruleset;
        public ScoreRank Rank => Score.Rank;
        public IEnumerable<IConfiguredMod> Mods => Score.Mods;
        public IReadOnlyDictionary<HitResult, int> Statistics => Score.Statistics;
        public IReadOnlyDictionary<HitResult, int> MaximumStatistics => Score.MaximumStatistics;
    }
}
