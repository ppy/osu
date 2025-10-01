// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestScenePickScreen : MultiplayerTestScene
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
        }

        [Test]
        public void TestScreen()
        {
            var selectedItems = new List<long>();

            SubScreenBeatmapSelect screen = null!;

            AddStep("add screen", () => Child = new ScreenStack(screen = new SubScreenBeatmapSelect()));

            AddStep("select maps", () =>
            {
                selectedItems.Clear();

                foreach (var user in users)
                {
                    var item = items[Random.Shared.Next(items.Length)];
                    selectedItems.Add(item.ID);

                    Scheduler.AddDelayed(() =>
                    {
                        MultiplayerClient.MatchmakingToggleUserSelection(user.Id, item.ID).FireAndForget();
                    }, RNG.NextDouble(10, 1000));
                }
            });

            AddStep("show final map", () =>
            {
                long[] candidateItems = selectedItems.ToArray();
                long finalItem = candidateItems[Random.Shared.Next(candidateItems.Length)];

                screen.RollFinalBeatmap(candidateItems, finalItem);
            });
        }
    }
}
