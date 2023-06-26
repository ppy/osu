// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Users.Drawables;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneLoginOverlay : OsuManualInputManagerTestScene
    {
        private LoginOverlay loginOverlay = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = loginOverlay = new LoginOverlay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("show login overlay", () => loginOverlay.Show());
        }

        [Test]
        public void TestLoginSuccess()
        {
            AddStep("logout", () => API.Logout());

            AddStep("enter password", () => loginOverlay.ChildrenOfType<OsuPasswordTextBox>().First().Text = "password");
            AddStep("submit", () => loginOverlay.ChildrenOfType<OsuButton>().First(b => b.Text.ToString() == "Sign in").TriggerClick());
        }

        [Test]
        public void TestLoginFailure()
        {
            AddStep("logout", () =>
            {
                API.Logout();
                ((DummyAPIAccess)API).FailNextLogin();
            });

            AddStep("enter password", () => loginOverlay.ChildrenOfType<OsuPasswordTextBox>().First().Text = "password");
            AddStep("submit", () => loginOverlay.ChildrenOfType<OsuButton>().First(b => b.Text.ToString() == "Sign in").TriggerClick());
        }

        [Test]
        public void TestLoginConnecting()
        {
            AddStep("logout", () =>
            {
                API.Logout();
                ((DummyAPIAccess)API).PauseOnConnectingNextLogin();
            });

            AddStep("enter password", () => loginOverlay.ChildrenOfType<OsuPasswordTextBox>().First().Text = "password");
            AddStep("submit", () => loginOverlay.ChildrenOfType<OsuButton>().First(b => b.Text.ToString() == "Sign in").TriggerClick());
        }

        [Test]
        public void TestClickingOnFlagClosesOverlay()
        {
            AddStep("logout", () => API.Logout());
            AddStep("enter password", () => loginOverlay.ChildrenOfType<OsuPasswordTextBox>().First().Text = "password");
            AddStep("submit", () => loginOverlay.ChildrenOfType<OsuButton>().First(b => b.Text.ToString() == "Sign in").TriggerClick());

            AddStep("click on flag", () =>
            {
                InputManager.MoveMouseTo(loginOverlay.ChildrenOfType<UpdateableFlag>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("login overlay is hidden", () => loginOverlay.State.Value == Visibility.Hidden);
        }
    }
}
