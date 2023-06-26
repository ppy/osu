// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerGameplayLeaderboardTeams : MultiplayerGameplayLeaderboardTestScene
    {
        private int team;

        protected override MultiplayerRoomUser CreateUser(int userId)
        {
            var user = base.CreateUser(userId);
            user.MatchState = new TeamVersusUserState
            {
                TeamID = team++ % 2
            };
            return user;
        }

        protected override MultiplayerGameplayLeaderboard CreateLeaderboard() =>
            new MultiplayerGameplayLeaderboard(MultiplayerUsers.ToArray())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("Add external display components", () =>
            {
                LoadComponentAsync(new MatchScoreDisplay
                {
                    Team1Score = { BindTarget = Leaderboard.TeamScores[0] },
                    Team2Score = { BindTarget = Leaderboard.TeamScores[1] }
                }, Add);

                LoadComponentAsync(new GameplayMatchScoreDisplay
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Team1Score = { BindTarget = Leaderboard.TeamScores[0] },
                    Team2Score = { BindTarget = Leaderboard.TeamScores[1] },
                    Expanded = { BindTarget = Leaderboard.Expanded },
                }, Add);
            });
        }
    }
}
