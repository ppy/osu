// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.SelectV2.Leaderboards;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallenge : OnlinePlayTestScene
    {
        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            Room room = null!;

            AddStep("add room", () => API.Perform(new CreateRoomRequest(room = new Room
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
            })));

            AddStep("push screen", () => LoadScreen(new Screens.OnlinePlay.DailyChallenge.DailyChallenge(room)));
        }

        [Test]
        public void TestDailyChallenge()
        {
        }

        [Test]
        public void TestScoreNavigation()
        {
            AddStep("click on score", () => this.ChildrenOfType<LeaderboardScoreV2>().First().TriggerClick());
            AddUntilStep("wait for load", () => Stack.CurrentScreen is ResultsScreen results && results.IsLoaded);
            AddAssert("replay download button exists", () => this.ChildrenOfType<ReplayDownloadButton>().Count(), () => Is.EqualTo(1));
        }
    }
}
