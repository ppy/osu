// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Net;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Login;
using osu.Game.Overlays.Settings;
using osu.Game.Users.Drawables;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneLoginOverlay : OsuManualInputManagerTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private LoginOverlay loginOverlay = null!;

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

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
            assertAPIState(APIState.Offline);

            AddStep("enter password", () => loginOverlay.ChildrenOfType<OsuPasswordTextBox>().First().Text = "password");
            AddStep("submit", () => loginOverlay.ChildrenOfType<OsuButton>().First(b => b.Text.ToString() == "Sign in").TriggerClick());

            assertAPIState(APIState.RequiresSecondFactorAuth);
            AddUntilStep("wait for second factor auth form", () => loginOverlay.ChildrenOfType<SecondFactorAuthForm>().SingleOrDefault(), () => Is.Not.Null);

            AddStep("set up verification handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case VerifySessionRequest verifySessionRequest:
                        if (verifySessionRequest.VerificationKey == "88800088")
                            verifySessionRequest.TriggerSuccess();
                        else
                            verifySessionRequest.TriggerFailure(new WebException());
                        return true;
                }

                return false;
            });
            AddStep("enter code", () => loginOverlay.ChildrenOfType<OsuTextBox>().First().Text = "88800088");
            assertAPIState(APIState.Online);
            AddStep("clear handler", () => dummyAPI.HandleRequest = null);
        }

        private void assertAPIState(APIState expected) =>
            AddUntilStep($"login state is {expected}", () => API.State.Value, () => Is.EqualTo(expected));

        [Test]
        public void TestVerificationFailure()
        {
            bool verificationHandled = false;
            AddStep("reset flag", () => verificationHandled = false);
            AddStep("logout", () => API.Logout());
            assertAPIState(APIState.Offline);

            AddStep("enter password", () => loginOverlay.ChildrenOfType<OsuPasswordTextBox>().First().Text = "password");
            AddStep("submit", () => loginOverlay.ChildrenOfType<OsuButton>().First(b => b.Text.ToString() == "Sign in").TriggerClick());

            assertAPIState(APIState.RequiresSecondFactorAuth);
            AddUntilStep("wait for second factor auth form", () => loginOverlay.ChildrenOfType<SecondFactorAuthForm>().SingleOrDefault(), () => Is.Not.Null);

            AddStep("set up verification handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case VerifySessionRequest verifySessionRequest:
                        if (verifySessionRequest.VerificationKey == "88800088")
                            verifySessionRequest.TriggerSuccess();
                        else
                            verifySessionRequest.TriggerFailure(new WebException());
                        verificationHandled = true;
                        return true;
                }

                return false;
            });
            AddStep("enter code", () => loginOverlay.ChildrenOfType<OsuTextBox>().First().Text = "abcdefgh");
            AddUntilStep("wait for verification handled", () => verificationHandled);
            assertAPIState(APIState.RequiresSecondFactorAuth);
            AddStep("clear handler", () => dummyAPI.HandleRequest = null);
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

            assertAPIState(APIState.RequiresSecondFactorAuth);
            AddUntilStep("wait for second factor auth form", () => loginOverlay.ChildrenOfType<SecondFactorAuthForm>().SingleOrDefault(), () => Is.Not.Null);

            AddStep("enter code", () => loginOverlay.ChildrenOfType<OsuTextBox>().First().Text = "88800088");
            assertAPIState(APIState.Online);

            AddStep("click on flag", () =>
            {
                InputManager.MoveMouseTo(loginOverlay.ChildrenOfType<UpdateableFlag>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("login overlay is hidden", () => loginOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestUncheckingRememberUsernameClearsIt()
        {
            AddStep("logout", () => API.Logout());
            AddStep("set username", () => configManager.SetValue(OsuSetting.Username, "test_user"));
            AddStep("set remember password", () => configManager.SetValue(OsuSetting.SavePassword, true));
            AddStep("uncheck remember username", () =>
            {
                InputManager.MoveMouseTo(loginOverlay.ChildrenOfType<SettingsCheckbox>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("remember username off", () => configManager.Get<bool>(OsuSetting.SaveUsername), () => Is.False);
            AddAssert("remember password off", () => configManager.Get<bool>(OsuSetting.SavePassword), () => Is.False);
            AddAssert("username cleared", () => configManager.Get<string>(OsuSetting.Username), () => Is.Empty);
        }

        [Test]
        public void TestUncheckingRememberPasswordClearsToken()
        {
            AddStep("logout", () => API.Logout());
            AddStep("set token", () => configManager.SetValue(OsuSetting.Token, "test_token"));
            AddStep("set remember password", () => configManager.SetValue(OsuSetting.SavePassword, true));
            AddStep("uncheck remember token", () =>
            {
                InputManager.MoveMouseTo(loginOverlay.ChildrenOfType<SettingsCheckbox>().Last());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("remember password off", () => configManager.Get<bool>(OsuSetting.SavePassword), () => Is.False);
            AddAssert("token cleared", () => configManager.Get<string>(OsuSetting.Token), () => Is.Empty);
        }
    }
}
