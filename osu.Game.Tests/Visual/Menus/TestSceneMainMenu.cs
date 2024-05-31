// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
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
