// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingScreen : MultiplayerTestScene
    {
        private const int user_count = 8;
        private const int beatmap_count = 50;

        private MultiplayerRoomUser[] users = null!;
        private MatchmakingScreen screen = null!;

        public TestSceneMatchmakingScreen()
        {
            Add(new BackButton
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                State = { Value = Visibility.Visible }
            });
        }

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

            AddStep("load match", () =>
            {
                users = Enumerable.Range(1, user_count).Select(i => new MultiplayerRoomUser(i)
                {
                    User = new APIUser
                    {
                        Username = $"Player {i}"
                    }
                }).ToArray();

                var beatmaps = Enumerable.Range(1, beatmap_count).Select(i => new MultiplayerPlaylistItem
                {
                    BeatmapID = i,
                    StarRating = i / 10.0
                }).ToArray();

                LoadScreen(screen = new MatchmakingScreen(new MultiplayerRoom(0)
                {
                    Users = users,
                    Playlist = beatmaps
                }));
            });
            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestGameplayFlow()
        {
            // Initial "ready" status of the room".
            AddWaitStep("wait", 5);

            AddStep("round start", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
            {
                RoomStatus = MatchmakingRoomStatus.RoundStart
            }).WaitSafely());

            // Next round starts with picks.
            AddWaitStep("wait", 5);

            AddStep("pick", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
            {
                RoomStatus = MatchmakingRoomStatus.UserPicks
            }).WaitSafely());

            // Make some selections
            AddWaitStep("wait", 5);

            for (int i = 0; i < 3; i++)
            {
                int j = i * 2;
                AddStep("click a beatmap", () =>
                {
                    Quad panelQuad = this.ChildrenOfType<BeatmapPanel>().ElementAt(j).ScreenSpaceDrawQuad;

                    InputManager.MoveMouseTo(new Vector2(panelQuad.Centre.X, panelQuad.TopLeft.Y + 5));
                    InputManager.Click(MouseButton.Left);
                });

                AddWaitStep("wait", 2);
            }

            // Lock in the gameplay beatmap

            AddStep("selection", () =>
            {
                MultiplayerPlaylistItem[] beatmaps = Enumerable.Range(1, 50).Select(i => new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                }).ToArray();

                MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
                {
                    RoomStatus = MatchmakingRoomStatus.SelectBeatmap,
                    CandidateItems = beatmaps.Select(b => b.ID).ToArray(),
                    CandidateItem = beatmaps[0].ID
                }).WaitSafely();
            });

            // Prepare gameplay.
            AddWaitStep("wait", 25);

            AddStep("prepare gameplay", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
            {
                RoomStatus = MatchmakingRoomStatus.PrepareGameplay
            }).WaitSafely());

            // Start gameplay.
            AddWaitStep("wait", 5);

            AddStep("gameplay", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
            {
                RoomStatus = MatchmakingRoomStatus.Gameplay
            }).WaitSafely());

            AddStep("start gameplay", () => MultiplayerClient.StartMatch().WaitSafely());
            // AddUntilStep("wait for player", () => (Stack.CurrentScreen as Player)?.IsLoaded == true);

            // Finish gameplay.
            AddWaitStep("wait", 5);

            AddStep("room end", () =>
            {
                MatchmakingRoomState state = new MatchmakingRoomState
                {
                    Round = 1,
                    RoomStatus = MatchmakingRoomStatus.RoomEnd
                };

                int localUserId = API.LocalUser.Value.OnlineID;

                state.Users[localUserId].Placement = 1;
                state.Users[localUserId].Rounds[1].Placement = 1;
                state.Users[localUserId].Rounds[1].TotalScore = 1;
                state.Users[localUserId].Rounds[1].Statistics[HitResult.LargeBonus] = 1;

                MultiplayerClient.ChangeMatchRoomState(state).WaitSafely();
            });
        }
    }
}
