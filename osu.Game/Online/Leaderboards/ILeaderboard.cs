// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using System.Collections.Generic;

namespace osu.Game.Online.Leaderboards
{
    public interface ILeaderboard
    {
        IEnumerable<ScoreInfo> Scores { get; }

        void RefreshScores();
    }
}
