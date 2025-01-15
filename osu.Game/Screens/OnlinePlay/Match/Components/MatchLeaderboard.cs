// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public partial class MatchLeaderboard : Leaderboard<MatchLeaderboardScope, APIUserScoreAggregate>
    {
        public MatchLeaderboard(MatchLeaderboardScoresProvider leaderboardScoresProvider)
            : base(leaderboardScoresProvider)
        {
        }

        protected override LeaderboardScore CreateDrawableScore(APIUserScoreAggregate model, int index) => new MatchLeaderboardScore(model, index);

        protected override LeaderboardScore CreateDrawableTopScore(APIUserScoreAggregate model) => new MatchLeaderboardScore(model, model.Position, false);
    }

    public enum MatchLeaderboardScope
    {
        Overall
    }
}
