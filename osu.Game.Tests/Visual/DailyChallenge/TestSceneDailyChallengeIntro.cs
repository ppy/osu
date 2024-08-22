// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
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

        private Room room = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            base.Content.Add(notificationOverlay);
            base.Content.Add(metadataClient);
        }

        [Test]
        public void TestDailyChallenge()
        {
            startChallenge();
            AddStep("push screen", () => LoadScreen(new DailyChallengeIntro(room)));
        }

        [Test]
        public void TestPlayIntroOnceFlag()
        {
            AddStep("set intro played flag", () => Dependencies.Get<SessionStatics>().SetValue(Static.DailyChallengeIntroPlayed, true));

            startChallenge();

            AddAssert("intro played flag reset", () => Dependencies.Get<SessionStatics>().Get<bool>(Static.DailyChallengeIntroPlayed), () => Is.False);

            AddStep("push screen", () => LoadScreen(new DailyChallengeIntro(room)));
            AddUntilStep("intro played flag set", () => Dependencies.Get<SessionStatics>().Get<bool>(Static.DailyChallengeIntroPlayed), () => Is.True);
        }

        private void startChallenge()
        {
            room = new Room
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
        }
    }
}
