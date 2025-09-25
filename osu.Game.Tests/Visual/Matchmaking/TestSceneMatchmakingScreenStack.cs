// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingScreenStack : MultiplayerTestScene
    {
        private const int user_count = 8;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom(MatchType.Matchmaking);
                room.Playlist = Enumerable.Range(1, 50).Select(i => new PlaylistItem(new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                })).ToArray();

                JoinRoom(room);
            });

            WaitForJoined();

            AddStep("add carousel", () =>
            {
                Child = new ScreenMatchmaking.ScreenStack
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                };
            });

            AddStep("join users", () =>
            {
                var users = Enumerable.Range(1, user_count).Select(i => new MultiplayerRoomUser(i)
                {
                    User = new APIUser
                    {
                        Username = $"Player {i}"
                    }
                }).ToArray();

                foreach (var user in users)
                    MultiplayerClient.AddUser(user);
            });
        }

        [Test]
        public void TestChangeStage()
        {
            for (int round = 1; round <= 2; round++)
            {
                AddLabel($"Round {round}");

                int r = round;
                changeStage(MatchmakingStage.RoundWarmupTime, state => state.CurrentRound = r);
                changeStage(MatchmakingStage.UserBeatmapSelect);
                changeStage(MatchmakingStage.ServerBeatmapFinalised, state =>
                {
                    MultiplayerPlaylistItem[] beatmaps = Enumerable.Range(1, 8).Select(i => new MultiplayerPlaylistItem
                    {
                        ID = i,
                        BeatmapID = i,
                        StarRating = i / 10.0,
                    }).ToArray();

                    state.CandidateItems = beatmaps.Select(b => b.ID).ToArray();
                    state.CandidateItem = beatmaps[0].ID;
                }, waitTime: 35);

                changeStage(MatchmakingStage.WaitingForClientsBeatmapDownload);
                changeStage(MatchmakingStage.GameplayWarmupTime);
                changeStage(MatchmakingStage.Gameplay);
                changeStage(MatchmakingStage.ResultsDisplaying);
            }

            changeStage(MatchmakingStage.Ended, state =>
            {
                int localUserId = API.LocalUser.Value.OnlineID;

                state.Users[localUserId].Placement = 1;
                state.Users[localUserId].Rounds[1].Placement = 1;
                state.Users[localUserId].Rounds[1].TotalScore = 1;
                state.Users[localUserId].Rounds[1].Statistics[HitResult.LargeBonus] = 1;
            });
        }

        private void changeStage(MatchmakingStage stage, Action<MatchmakingRoomState>? prepare = null, int waitTime = 5)
        {
            AddStep($"stage: {stage}", () => MultiplayerClient.MatchmakingChangeStage(stage, prepare).WaitSafely());
            AddWaitStep("wait", waitTime);
        }
    }
}
