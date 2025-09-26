// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneUserPanelOverlay : MultiplayerTestScene
    {
        private UserPanelOverlay list = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.Matchmaking)));
            WaitForJoined();

            AddStep("add list", () => Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.8f),
                Child = list = new UserPanelOverlay()
            });
        }

        [Test]
        public void TestChangeDisplayMode()
        {
            AddStep("join users", () =>
            {
                for (int i = 0; i < 7; i++)
                {
                    MultiplayerClient.AddUser(new MultiplayerRoomUser(i)
                    {
                        User = new APIUser
                        {
                            Username = $"User {i}"
                        }
                    });
                }
            });

            AddStep("change to split mode", () => list.DisplayStyle = PanelDisplayStyle.Split);
            AddStep("change to grid mode", () => list.DisplayStyle = PanelDisplayStyle.Grid);
            AddStep("change to hidden mode", () => list.DisplayStyle = PanelDisplayStyle.Hidden);
        }

        [Test]
        public void AddPanelsGrid()
        {
            AddStep("change to grid mode", () => list.DisplayStyle = PanelDisplayStyle.Grid);

            int userId = 0;

            AddRepeatStep("join user", () =>
            {
                MultiplayerClient.AddUser(new MultiplayerRoomUser(userId)
                {
                    User = new APIUser
                    {
                        Username = $"User {userId}"
                    }
                });

                userId++;
            }, 8);
        }

        [Test]
        public void AddPanelsSplit()
        {
            AddStep("change to split mode", () => list.DisplayStyle = PanelDisplayStyle.Split);

            int userId = 0;

            AddRepeatStep("join user", () =>
            {
                MultiplayerClient.AddUser(new MultiplayerRoomUser(userId)
                {
                    User = new APIUser
                    {
                        Username = $"User {userId}"
                    }
                });

                userId++;
            }, 8);
        }

        [Test]
        public void ChangeRankings()
        {
            AddStep("join users", () =>
            {
                for (int i = 0; i < 7; i++)
                {
                    MultiplayerClient.AddUser(new MultiplayerRoomUser(i)
                    {
                        User = new APIUser
                        {
                            Username = $"User {i}"
                        }
                    });
                }
            });

            AddStep("set random placements", () =>
            {
                MultiplayerRoom room = MultiplayerClient.ServerRoom!;

                int[] placements = Enumerable.Range(1, room.Users.Count).ToArray();
                Random.Shared.Shuffle(placements);

                MatchmakingRoomState state = new MatchmakingRoomState();

                for (int i = 0; i < room.Users.Count; i++)
                    state.Users[room.Users[i].UserID].Placement = placements[i];

                MultiplayerClient.ChangeMatchRoomState(state).WaitSafely();
            });
        }
    }
}
