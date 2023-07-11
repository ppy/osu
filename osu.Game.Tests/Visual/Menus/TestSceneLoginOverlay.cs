// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
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

        [Resolved]
        private Clipboard clipboard { get; set; } = null!;

        private OsuPasswordTextBox passwordTextBox => loginOverlay.ChildrenOfType<OsuPasswordTextBox>().First();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create login overlay", () =>
            {
                Child = loginOverlay = new LoginOverlay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });
            AddStep("show login overlay", () => loginOverlay.Show());
        }

        [Test]
        public void TestLoginSuccess()
        {
            AddStep("logout", () => API.Logout());

            AddStep("enter password", () => passwordTextBox.Text = "password");
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

            AddStep("enter password", () => passwordTextBox.Text = "password");
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

            AddStep("enter password", () => passwordTextBox.Text = "password");
            AddStep("submit", () => loginOverlay.ChildrenOfType<OsuButton>().First(b => b.Text.ToString() == "Sign in").TriggerClick());
        }

        [Test]
        public void TestClickingOnFlagClosesOverlay()
        {
            AddStep("logout", () => API.Logout());
            AddStep("enter password", () => passwordTextBox.Text = "password");
            AddStep("submit", () => loginOverlay.ChildrenOfType<OsuButton>().First(b => b.Text.ToString() == "Sign in").TriggerClick());

            AddStep("click on flag", () =>
            {
                InputManager.MoveMouseTo(loginOverlay.ChildrenOfType<UpdateableFlag>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("login overlay is hidden", () => loginOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestPastePasswordWithTouch()
        {
            const string sample_password = "hunter2";

            AddStep("logout", () => API.Logout());
            AddStep("set clipboard text", () => clipboard.SetText(sample_password));

            AddStep("begin touch for right click", () => InputManager.BeginTouch(getCenteredTouch(passwordTextBox)));
            AddUntilStep("wait for context menu to show", () => loginOverlay.ChildrenOfType<OsuContextMenu>().First().IsPresent);
            AddStep("end touch", () => InputManager.EndTouch(getCenteredTouch(passwordTextBox)));

            AddStep("touch 'Paste'", () =>
            {
                var pasteMenuItem = loginOverlay.ChildrenOfType<DrawableOsuMenuItem>().First(d => d.Item.Text.Value == CommonStrings.Paste);
                InputManager.BeginTouch(getCenteredTouch(pasteMenuItem));
                InputManager.EndTouch(getCenteredTouch(pasteMenuItem));
            });

            AddAssert("pasted from clipboard", () => passwordTextBox.Text, () => Is.EqualTo(sample_password));
        }

        private static Touch getCenteredTouch(Drawable drawable) => new Touch(TouchSource.Touch1, drawable.ToScreenSpace(drawable.LayoutRectangle.Centre));
    }
}
