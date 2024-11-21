// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK.Graphics;
using osuTK.Input;
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
            Add(notificationOverlay);
            Add(metadataClient);

            // add button to observe for daily challenge changes and perform its logic.
            Add(new DailyChallengeButton(@"button-default-select", new Color4(102, 68, 204, 255), (_, _) => { }, 0, Key.D));
        }

        [Test]
        public void TestDailyChallenge()
        {
            startChallenge(1234);
            AddStep("push screen", () => LoadScreen(new DailyChallengeIntro(room)));
        }

        [Test]
        public void TestPlayIntroOnceFlag()
        {
            startChallenge(1234);
            AddStep("set intro played flag", () => Dependencies.Get<SessionStatics>().SetValue(Static.DailyChallengeIntroPlayed, true));

            startChallenge(1235);

            AddAssert("intro played flag reset", () => Dependencies.Get<SessionStatics>().Get<bool>(Static.DailyChallengeIntroPlayed), () => Is.False);

            AddStep("push screen", () => LoadScreen(new DailyChallengeIntro(room)));
            AddUntilStep("intro played flag set", () => Dependencies.Get<SessionStatics>().Get<bool>(Static.DailyChallengeIntroPlayed), () => Is.True);
        }

        private void startChallenge(int roomId)
        {
            AddStep("add room", () =>
            {
                API.Perform(new CreateRoomRequest(room = new Room
                {
                    RoomID = roomId,
                    Name = "Daily Challenge: June 4, 2024",
                    Playlist =
                    [
                        new PlaylistItem(CreateAPIBeatmap(new OsuRuleset().RulesetInfo))
                        {
                            RequiredMods = [new APIMod(new OsuModTraceable())],
                            AllowedMods = [new APIMod(new OsuModDoubleTime())]
                        }
                    ],
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now.AddHours(24),
                    Category = RoomCategory.DailyChallenge
                }));
            });
            AddStep("signal client", () => metadataClient.DailyChallengeUpdated(new DailyChallengeInfo { RoomID = roomId }));
        }
    }
}
