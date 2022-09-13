// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public class SoloGameplayLeaderboard : GameplayLeaderboard
    {
        [BackgroundDependencyLoader]
        private void load(Player player, ScoreProcessor processor, ILeaderboard leaderboard)
        {
            ILeaderboardScore local = Add(player.Score.ScoreInfo.User, true);

            local.TotalScore.BindTarget = processor.TotalScore;
            local.Accuracy.BindTarget = processor.Accuracy;
            local.Combo.BindTarget = processor.Combo;

            foreach (var s in leaderboard.Scores)
            {
                // todo: APIUser is pain for IScoreInfo.
                var score = Add(new APIUser
                {
                    Id = s.User.OnlineID,
                    Username = s.User.Username,
                }, false);

                score.TotalScore.Value = s.TotalScore;
                score.Accuracy.Value = s.Accuracy;
                score.Combo.Value = s.MaxCombo;
            }
        }
    }
}
