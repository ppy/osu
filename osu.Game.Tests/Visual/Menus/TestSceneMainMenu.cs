// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.Menu;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneMainMenu : OsuGameTestScene
    {
        private OnlineMenuBanner onlineMenuBanner => Game.ChildrenOfType<OnlineMenuBanner>().Single();

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("don't fetch online content", () => onlineMenuBanner.FetchOnlineContent = false);
            AddStep("disable return to top on idle", () => Game.ChildrenOfType<ButtonSystem>().Single().ReturnToTopOnIdle = false);
        }

        [Test]
        public void TestDailyChallenge()
        {
            AddStep("set up API", () => ((DummyAPIAccess)API).HandleRequest = req =>
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
                            Name = { Value = "Aug 8, 2024" },
                            Playlist =
                            {
                                new PlaylistItem(beatmap)
                            },
                            StartDate = { Value = DateTimeOffset.Now.AddMinutes(-30) },
                            EndDate = { Value = DateTimeOffset.Now.AddSeconds(60) }
                        });
                        return true;

                    default:
                        return false;
                }
            });

            AddStep("beatmap of the day active", () => Game.ChildrenOfType<IMetadataClient>().Single().DailyChallengeUpdated(new DailyChallengeInfo
            {
                RoomID = 1234,
            }));

            AddStep("enter menu", () => InputManager.Key(Key.P));
            AddStep("enter submenu", () => InputManager.Key(Key.P));
            AddStep("enter daily challenge", () => InputManager.Key(Key.D));

            AddUntilStep("wait for daily challenge screen", () => Game.ScreenStack.CurrentScreen, Is.TypeOf<Screens.OnlinePlay.DailyChallenge.DailyChallenge>);
        }

        [Test]
        public void TestOnlineMenuBannerTrusted()
        {
            AddStep("set online content", () => onlineMenuBanner.Current.Value = new APIMenuContent
            {
                Images = new[]
                {
                    new APIMenuImage
                    {
                        Image = @"https://assets.ppy.sh/main-menu/project-loved-2@2x.png",
                        Url = $@"{API.WebsiteRootUrl}/home/news/2023-12-21-project-loved-december-2023",
                    }
                }
            });
            AddAssert("system title not visible", () => onlineMenuBanner.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddStep("enter menu", () => InputManager.Key(Key.Enter));
            AddUntilStep("system title visible", () => onlineMenuBanner.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddUntilStep("image loaded", () => onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>().FirstOrDefault()?.IsLoaded, () => Is.True);

            AddStep("click banner", () =>
            {
                InputManager.MoveMouseTo(onlineMenuBanner);
                InputManager.Click(MouseButton.Left);
            });

            // Might not catch every occurrence due to async nature, but works in manual testing and saves annoying test setup.
            AddAssert("no dialog", () => Game.ChildrenOfType<DialogOverlay>().SingleOrDefault()?.CurrentDialog == null);
        }

        [Test]
        public void TestOnlineMenuBannerUntrustedDomain()
        {
            AddStep("set online content", () => onlineMenuBanner.Current.Value = new APIMenuContent
            {
                Images = new[]
                {
                    new APIMenuImage
                    {
                        Image = @"https://assets.ppy.sh/main-menu/project-loved-2@2x.png",
                        Url = @"https://google.com",
                    }
                }
            });
            AddAssert("system title not visible", () => onlineMenuBanner.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddStep("enter menu", () => InputManager.Key(Key.Enter));
            AddUntilStep("system title visible", () => onlineMenuBanner.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddUntilStep("image loaded", () => onlineMenuBanner.ChildrenOfType<OnlineMenuBanner.MenuImage>().FirstOrDefault()?.IsLoaded, () => Is.True);

            AddStep("click banner", () =>
            {
                InputManager.MoveMouseTo(onlineMenuBanner);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for dialog", () => Game.ChildrenOfType<DialogOverlay>().SingleOrDefault()?.CurrentDialog != null);
        }
    }
}
