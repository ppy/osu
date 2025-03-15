// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.SelectV2.Leaderboards;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallenge : OnlinePlayTestScene
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
        public void TestDailyChallenge()
        {
            var room = new Room
            {
                RoomID = 1234,
                Name = "Daily Challenge: June 4, 2024",
                Playlist =
                [
                    new PlaylistItem(TestResources.CreateTestBeatmapSetInfo().Beatmaps.First())
                    {
                        RequiredMods = [new APIMod(new OsuModTraceable())],
                        AllowedMods = [new APIMod(new OsuModDoubleTime())]
                    }
                ],
                EndDate = DateTimeOffset.Now.AddHours(12),
                Category = RoomCategory.DailyChallenge
            };

            AddStep("add room", () => API.Perform(new CreateRoomRequest(room)));
            AddStep("push screen", () => LoadScreen(new Screens.OnlinePlay.DailyChallenge.DailyChallenge(room)));
        }

        [Test]
        public void TestUseTheseModsUnavailableIfNoFreeMods()
        {
            var room = new Room
            {
                RoomID = 1234,
                Name = "Daily Challenge: June 4, 2024",
                Playlist =
                [
                    new PlaylistItem(TestResources.CreateTestBeatmapSetInfo().Beatmaps.First())
                    {
                        RequiredMods = [new APIMod(new OsuModTraceable())],
                        AllowedMods = []
                    }
                ],
                EndDate = DateTimeOffset.Now.AddHours(12),
                Category = RoomCategory.DailyChallenge
            };

            AddStep("add room", () => API.Perform(new CreateRoomRequest(room)));
            Screens.OnlinePlay.DailyChallenge.DailyChallenge screen = null!;
            AddStep("push screen", () => LoadScreen(screen = new Screens.OnlinePlay.DailyChallenge.DailyChallenge(room)));
            AddUntilStep("wait for pushed", () => screen.IsCurrentScreen());
            AddStep("force transforms to finish", () => FinishTransforms(true));
            AddStep("right click second score", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<LeaderboardScoreV2>().ElementAt(1));
                InputManager.Click(MouseButton.Right);
            });
            AddAssert("use these mods not present",
                () => this.ChildrenOfType<OsuContextMenu>().All(m => m.Items.All(item => item.Text.Value != "Use these mods")));
        }

        [Test]
        public void TestNotifications()
        {
            var room = new Room
            {
                RoomID = 1234,
                Name = "Daily Challenge: June 4, 2024",
                Playlist =
                [
                    new PlaylistItem(TestResources.CreateTestBeatmapSetInfo().Beatmaps.First())
                    {
                        RequiredMods = [new APIMod(new OsuModTraceable())],
                        AllowedMods = [new APIMod(new OsuModDoubleTime())]
                    }
                ],
                EndDate = DateTimeOffset.Now.AddHours(12),
                Category = RoomCategory.DailyChallenge
            };

            AddStep("add room", () => API.Perform(new CreateRoomRequest(room)));
            AddStep("set daily challenge info", () => metadataClient.DailyChallengeInfo.Value = new DailyChallengeInfo { RoomID = 1234 });

            Screens.OnlinePlay.DailyChallenge.DailyChallenge screen = null!;
            AddStep("push screen", () => LoadScreen(screen = new Screens.OnlinePlay.DailyChallenge.DailyChallenge(room)));
            AddUntilStep("wait for screen", () => screen.IsCurrentScreen());
            AddStep("daily challenge ended", () => metadataClient.DailyChallengeInfo.Value = null);
            AddAssert("notification posted", () => notificationOverlay.AllNotifications.OfType<SimpleNotification>().Any(n => n.Text == DailyChallengeStrings.ChallengeEndedNotification));
        }

        [Test]
        public void TestConclusionNotificationDoesNotFireOnDisconnect()
        {
            var room = new Room
            {
                RoomID = 1234,
                Name = "Daily Challenge: June 4, 2024",
                Playlist =
                [
                    new PlaylistItem(TestResources.CreateTestBeatmapSetInfo().Beatmaps.First())
                    {
                        RequiredMods = [new APIMod(new OsuModTraceable())],
                        AllowedMods = [new APIMod(new OsuModDoubleTime())]
                    }
                ],
                EndDate = DateTimeOffset.Now.AddHours(12),
                Category = RoomCategory.DailyChallenge
            };

            AddStep("add room", () => API.Perform(new CreateRoomRequest(room)));
            AddStep("set daily challenge info", () => metadataClient.DailyChallengeInfo.Value = new DailyChallengeInfo { RoomID = 1234 });

            Screens.OnlinePlay.DailyChallenge.DailyChallenge screen = null!;
            AddStep("push screen", () => LoadScreen(screen = new Screens.OnlinePlay.DailyChallenge.DailyChallenge(room)));
            AddUntilStep("wait for screen", () => screen.IsCurrentScreen());
            AddStep("disconnect from metadata server", () => metadataClient.Disconnect());
            AddUntilStep("wait for disconnection", () => metadataClient.DailyChallengeInfo.Value, () => Is.Null);
            AddAssert("no notification posted", () => notificationOverlay.AllNotifications, () => Is.Empty);
            AddStep("reconnect to metadata server", () => metadataClient.Reconnect());
        }
    }
}
