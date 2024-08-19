// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.Menu;
using osuTK.Input;
using Color4 = osuTK.Graphics.Color4;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneMainMenuButton : OsuTestScene
    {
        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Test]
        public void TestStandardButton()
        {
            AddStep("add button", () => Child = new MainMenuButton(
                ButtonSystemStrings.Solo, @"button-default-select", OsuIcon.Player, new Color4(102, 68, 204, 255), _ => { }, 0, Key.P)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                ButtonSystemState = ButtonSystemState.TopLevel,
            });
        }

        [Test]
        public void TestDailyChallengeButton()
        {
            AddStep("set up API", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetRoomRequest getRoomRequest:
                        if (getRoomRequest.RoomId != 1234)
                            return false;

                        var beatmap = CreateAPIBeatmap();
                        beatmap.OnlineID = 1001;
                        getRoomRequest.TriggerSuccess(new Room
                        {
                            RoomID = { Value = 1234 },
                            Playlist =
                            {
                                new PlaylistItem(beatmap)
                            },
                            StartDate = { Value = DateTimeOffset.Now.AddMinutes(-5) },
                            EndDate = { Value = DateTimeOffset.Now.AddSeconds(30) }
                        });
                        return true;

                    default:
                        return false;
                }
            });

            NotificationOverlay notificationOverlay = null!;
            DependencyProvidingContainer buttonContainer = null!;

            AddStep("beatmap of the day active", () => metadataClient.DailyChallengeUpdated(new DailyChallengeInfo
            {
                RoomID = 1234,
            }));
            AddStep("add content", () =>
            {
                notificationOverlay = new NotificationOverlay();
                Children = new Drawable[]
                {
                    notificationOverlay,
                    buttonContainer = new DependencyProvidingContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        CachedDependencies = [(typeof(INotificationOverlay), notificationOverlay)],
                        Child = new DailyChallengeButton(@"button-default-select", new Color4(102, 68, 204, 255), _ => { }, 0, Key.D)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            ButtonSystemState = ButtonSystemState.TopLevel,
                        },
                    },
                };
            });
            AddAssert("notification posted", () => notificationOverlay.AllNotifications.Count(), () => Is.EqualTo(1));

            AddStep("clear notifications", () =>
            {
                foreach (var notification in notificationOverlay.AllNotifications)
                    notification.Close(runFlingAnimation: false);
            });
            AddStep("beatmap of the day not active", () => metadataClient.DailyChallengeUpdated(null));
            AddAssert("no notification posted", () => notificationOverlay.AllNotifications.Count(), () => Is.Zero);

            AddStep("hide button's parent", () => buttonContainer.Hide());
            AddStep("beatmap of the day active", () => metadataClient.DailyChallengeUpdated(new DailyChallengeInfo
            {
                RoomID = 1234,
            }));
            AddAssert("no notification posted", () => notificationOverlay.AllNotifications.Count(), () => Is.Zero);
        }
    }
}
