// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Tests.Visual.OnlinePlay;
using CreateRoomRequest = osu.Game.Online.Rooms.CreateRoomRequest;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallengeIntro : OnlinePlayTestScene
    {
        [Cached(typeof(MetadataClient))]
        private TestMetadataClient metadataClient = new TestMetadataClient();

        [Cached(typeof(INotificationOverlay))]
        private NotificationOverlay notificationOverlay = new NotificationOverlay();

        [BackgroundDependencyLoader]
        private void load()
        {
            base.Content.Add(notificationOverlay);
            base.Content.Add(metadataClient);
        }

        [Test]
        [Solo]
        public void TestDailyChallenge()
        {
            var room = new Room
            {
                RoomID = { Value = 1234 },
                Name = { Value = "Daily Challenge: June 4, 2024" },
                Playlist =
                {
                    new PlaylistItem(CreateAPIBeatmapSet().Beatmaps.First())
                    {
                        RequiredMods = [new APIMod(new OsuModTraceable())],
                        AllowedMods = [new APIMod(new OsuModDoubleTime())]
                    }
                },
                EndDate = { Value = DateTimeOffset.Now.AddHours(12) },
                Category = { Value = RoomCategory.DailyChallenge }
            };

            AddStep("add room", () => API.Perform(new CreateRoomRequest(room)));
            AddStep("push screen", () => LoadScreen(new Screens.OnlinePlay.DailyChallenge.DailyChallengeIntro(room)));
        }

        [Test]
        public void TestNotifications()
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
            AddStep("set daily challenge info", () => metadataClient.DailyChallengeInfo.Value = new DailyChallengeInfo { RoomID = 1234 });

            Screens.OnlinePlay.DailyChallenge.DailyChallenge screen = null!;
            AddStep("push screen", () => LoadScreen(screen = new Screens.OnlinePlay.DailyChallenge.DailyChallenge(room)));
            AddUntilStep("wait for screen", () => screen.IsCurrentScreen());
            AddStep("daily challenge ended", () => metadataClient.DailyChallengeInfo.Value = null);
        }
    }
}
