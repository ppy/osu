using osu.Game.Scoring;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Online.Leaderboards
{
    public interface ILeaderboard
    {
        IEnumerable<ScoreInfo> Scores { get; }

        void RefreshScores();
    }
}
