// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge.Events
{
    public class NewScoreEvent
    {
        public NewScoreEvent(long scoreID, APIUser user, long totalScore, int? newRank)
        {
            ScoreID = scoreID;
            User = user;
            TotalScore = totalScore;
            NewRank = newRank;
        }

        public long ScoreID { get; }
        public APIUser User { get; }
        public long TotalScore { get; }
        public int? NewRank { get; }
    }
}
