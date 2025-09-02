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
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens;
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
                var room = CreateDefaultRoom();
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
                Child = new MatchmakingScreenStack
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
        public void TestStatus()
        {
            AddWaitStep("wait for scroll", 5);
            AddStep("pick", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
            {
                Stage = MatchmakingStage.UserBeatmapSelect
            }).WaitSafely());

            AddWaitStep("wait for scroll", 5);
            AddStep("selection", () =>
            {
                MultiplayerPlaylistItem[] beatmaps = Enumerable.Range(1, 50).Select(i => new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                }).ToArray();

                beatmaps = Random.Shared.GetItems(beatmaps, 8);

                MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
                {
                    Stage = MatchmakingStage.ServerBeatmapFinalised,
                    CandidateItems = beatmaps.Select(b => b.ID).ToArray(),
                    CandidateItem = beatmaps[0].ID
                }).WaitSafely();
            });

            AddWaitStep("wait for scroll", 35);
            AddStep("room end", () =>
            {
                var state = new MatchmakingRoomState
                {
                    CurrentRound = 1,
                    Stage = MatchmakingStage.Ended
                };

                int localUserId = API.LocalUser.Value.OnlineID;

                state.Users[localUserId].Placement = 1;
                state.Users[localUserId].Rounds[1].Placement = 1;
                state.Users[localUserId].Rounds[1].TotalScore = 1;
                state.Users[localUserId].Rounds[1].Statistics[HitResult.LargeBonus] = 1;

                state.Users[1].Placement = 2;
                state.Users[1].Rounds[1].Placement = 2;

                MultiplayerClient.ChangeMatchRoomState(state).WaitSafely();
            });
        }
    }
}
