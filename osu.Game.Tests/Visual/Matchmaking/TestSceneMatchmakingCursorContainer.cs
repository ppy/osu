// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking.Events;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingCursorContainer : MultiplayerTestScene
    {
        private readonly IReadOnlyList<APIUser> users = new[]
        {
            new APIUser
            {
                Id = 2,
                Username = "peppy",
            },
            new APIUser
            {
                Id = 1040328,
                Username = "smoogipoo",
            },
            new APIUser
            {
                Id = 6573093,
                Username = "OliBomby",
            },
            new APIUser
            {
                Id = 7782553,
                Username = "aesth",
            },
            new APIUser
            {
                Id = 6411631,
                Username = "Maarvin",
            }
        };

        private readonly PlaylistItem[] items = Enumerable.Range(1, 50).Select(i => new PlaylistItem(new MultiplayerPlaylistItem
        {
            ID = i,
            BeatmapID = i,
            StarRating = i / 10.0,
        })).ToArray();

        private MatchmakingCursorContainer? cursorContainer;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom(MatchType.Matchmaking);
                room.Playlist = items;

                JoinRoom(room);
            });

            WaitForJoined();

            AddStep("add users", () =>
            {
                foreach (var user in users)
                    MultiplayerClient.AddUser(user);
            });

            AddStep("add screen", () =>
            {
                Child = new ScreenStack(new SubScreenBeatmapSelect());
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var inputManager = GetContainingInputManager()!;

            Scheduler.AddDelayed(() =>
            {
                cursorContainer ??= this.ChildrenOfType<MatchmakingCursorContainer>().FirstOrDefault();

                if (cursorContainer == null)
                    return;

                var position = cursorContainer.ToLocalSpace(inputManager.CurrentState.Mouse.Position);

                var request = new MatchmakingCursorPositionRequest
                {
                    X = position.X,
                    Y = position.Y,
                };

                Scheduler.AddDelayed(() =>
                {
                    MultiplayerClient.SendUserMatchRequest(users[0].Id, request).FireAndForget();
                }, 500);

                Scheduler.AddDelayed(() =>
                {
                    MultiplayerClient.SendUserMatchRequest(users[1].Id, request).FireAndForget();
                }, 1000);

                Scheduler.AddDelayed(() =>
                {
                    MultiplayerClient.SendUserMatchRequest(users[2].Id, request).FireAndForget();
                }, 1500);
            }, 50, true);
        }
    }
}
