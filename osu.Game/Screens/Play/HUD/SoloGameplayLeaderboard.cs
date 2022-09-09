// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public class SoloGameplayLeaderboard : GameplayLeaderboard
    {
        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, ScoreProcessor processor, ILeaderboard leaderboard)
        {
            var local = Add(api.LocalUser.Value, true);
            local.TotalScore.BindTarget = processor.TotalScore;
            local.Accuracy.BindTarget = processor.Accuracy;
            local.Combo.BindTarget = processor.Combo;

            foreach (var player in leaderboard.Scores)
            {
                // todo: APIUser is pain for IScoreInfo.
                var score = Add(new APIUser
                {
                    Id = player.User.OnlineID,
                    Username = player.User.Username,
                }, false);

                score.TotalScore.Value = player.TotalScore;
                score.Accuracy.Value = player.Accuracy;
                score.Combo.Value = player.MaxCombo;
            }
        }
    }
}
