// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallenge : OnlinePlayTestScene
    {
        [Test]
        public void TestDailyChallenge()
        {
            var room = new Room
            {
                RoomID = { Value = 1234 },
                Name = { Value = "Daily Challenge: June 4, 2024" },
                Playlist =
                {
                    new PlaylistItem(TestResources.CreateTestBeatmapSetInfo().Beatmaps.First())
                    {
                        RequiredMods = [new APIMod(new OsuModTraceable())],
                        AllowedMods = [new APIMod(new OsuModDoubleTime())]
                    }
                },
                EndDate = { Value = DateTimeOffset.Now.AddHours(12) },
                Category = { Value = RoomCategory.DailyChallenge }
            };

            AddStep("add room", () => API.Perform(new CreateRoomRequest(room)));
            AddStep("push screen", () => LoadScreen(new Screens.OnlinePlay.DailyChallenge.DailyChallenge(room)));
        }
    }
}
