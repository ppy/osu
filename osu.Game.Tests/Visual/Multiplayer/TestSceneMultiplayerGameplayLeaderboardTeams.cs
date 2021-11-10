// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerGameplayLeaderboardTeams : MultiplayerTestScene
    {
        private static IEnumerable<int> users => Enumerable.Range(0, 16);

        public new TestSceneMultiplayerGameplayLeaderboard.TestMultiplayerSpectatorClient SpectatorClient =>
            (TestSceneMultiplayerGameplayLeaderboard.TestMultiplayerSpectatorClient)OnlinePlayDependencies?.SpectatorClient;

        protected override OnlinePlayTestSceneDependencies CreateOnlinePlayDependencies() => new TestDependencies();

        protected class TestDependencies : MultiplayerTestSceneDependencies
        {
            protected override TestSpectatorClient CreateSpectatorClient() => new TestSceneMultiplayerGameplayLeaderboard.TestMultiplayerSpectatorClient();
        }

        private MultiplayerGameplayLeaderboard leaderboard;
        private GameplayMatchScoreDisplay gameplayScoreDisplay;

        protected override Room CreateRoom()
        {
            var room = base.CreateRoom();
            room.Type.Value = MatchType.TeamVersus;
            return room;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set local user", () => ((DummyAPIAccess)API).LocalUser.Value = LookupCache.GetUserAsync(1).Result);

            AddStep("create leaderboard", () =>
            {
                leaderboard?.Expire();

                OsuScoreProcessor scoreProcessor;
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);

                var playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
                var multiplayerUsers = new List<MultiplayerRoomUser>();

                foreach (int user in users)
                {
                    SpectatorClient.StartPlay(user, Beatmap.Value.BeatmapInfo.OnlineBeatmapID ?? 0);
                    var roomUser = OnlinePlayDependencies.Client.AddUser(new APIUser { Id = user }, true);

                    roomUser.MatchState = new TeamVersusUserState
                    {
                        TeamID = RNG.Next(0, 2)
                    };

                    multiplayerUsers.Add(roomUser);
                }

                Children = new Drawable[]
                {
                    scoreProcessor = new OsuScoreProcessor(),
                };

                scoreProcessor.ApplyBeatmap(playableBeatmap);

                LoadComponentAsync(leaderboard = new MultiplayerGameplayLeaderboard(scoreProcessor, multiplayerUsers.ToArray())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }, gameplayLeaderboard =>
                {
                    LoadComponentAsync(new MatchScoreDisplay
                    {
                        Team1Score = { BindTarget = leaderboard.TeamScores[0] },
                        Team2Score = { BindTarget = leaderboard.TeamScores[1] }
                    }, Add);

                    LoadComponentAsync(gameplayScoreDisplay = new GameplayMatchScoreDisplay
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Team1Score = { BindTarget = leaderboard.TeamScores[0] },
                        Team2Score = { BindTarget = leaderboard.TeamScores[1] }
                    }, Add);

                    Add(gameplayLeaderboard);
                });
            });

            AddUntilStep("wait for load", () => leaderboard.IsLoaded);
            AddUntilStep("wait for user population", () => Client.CurrentMatchPlayingUserIds.Count > 0);
        }

        [Test]
        public void TestScoreUpdates()
        {
            AddRepeatStep("update state", () => SpectatorClient.RandomlyUpdateState(), 100);
            AddToggleStep("switch compact mode", expanded =>
            {
                leaderboard.Expanded.Value = expanded;
                gameplayScoreDisplay.Expanded.Value = expanded;
            });
        }
    }
}
