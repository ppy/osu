// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using System.Net;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.AccountCreation;
using osu.Game.Overlays.Settings;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneAccountCreationOverlay : OsuTestScene
    {
        private readonly Container userPanelArea;
        private readonly AccountCreationOverlay accountCreation;

        private IBindable<APIUser> localUser;

        public TestSceneAccountCreationOverlay()
        {
            Children = new Drawable[]
            {
                accountCreation = new AccountCreationOverlay(),
                userPanelArea = new Container
                {
                    Padding = new MarginPadding(10),
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            localUser = API.LocalUser.GetBoundCopy();
            localUser.BindValueChanged(user => { userPanelArea.Child = new UserGridPanel(user.NewValue) { Width = 200 }; }, true);
        }

        [Test]
        public void TestOverlayVisibility()
        {
            AddStep("start hidden", () => accountCreation.Hide());
            AddStep("log out", () => API.Logout());

            AddStep("show manually", () => accountCreation.Show());
            AddUntilStep("overlay is visible", () => accountCreation.State.Value == Visibility.Visible);

            AddStep("click button", () => accountCreation.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddUntilStep("warning screen is present", () => accountCreation.ChildrenOfType<ScreenWarning>().SingleOrDefault()?.IsPresent == true);

            AddStep("log back in", () =>
            {
                API.Login("dummy", "password");
                ((DummyAPIAccess)API).AuthenticateSecondFactor("abcdefgh");
            });
            AddUntilStep("overlay is hidden", () => accountCreation.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestFullFlow()
        {
            AddStep("log out", () => API.Logout());

            AddStep("show manually", () => accountCreation.Show());
            AddUntilStep("overlay is visible", () => accountCreation.State.Value == Visibility.Visible);

            AddStep("click button", () => accountCreation.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddUntilStep("warning screen is present", () => accountCreation.ChildrenOfType<ScreenWarning>().SingleOrDefault()?.IsPresent == true);

            AddStep("proceed", () => accountCreation.ChildrenOfType<DangerousSettingsButton>().Single().TriggerClick());
            AddUntilStep("entry screen is present", () => accountCreation.ChildrenOfType<ScreenEntry>().SingleOrDefault()?.IsPresent == true);

            AddStep("input details", () =>
            {
                var entryScreen = accountCreation.ChildrenOfType<ScreenEntry>().Single();
                entryScreen.ChildrenOfType<OsuTextBox>().ElementAt(0).Text = "new_user";
                entryScreen.ChildrenOfType<OsuTextBox>().ElementAt(1).Text = "new.user@fake.mail";
                entryScreen.ChildrenOfType<OsuTextBox>().ElementAt(2).Text = "password";
            });
            AddStep("click button", () => accountCreation.ChildrenOfType<ScreenEntry>().Single()
                                                         .ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddUntilStep("verification screen is present", () => accountCreation.ChildrenOfType<ScreenEmailVerification>().SingleOrDefault()?.IsPresent == true);

            AddStep("verify", () =>
                accountCreation.ChildrenOfType<ScreenEmailVerification>().Single().ChildrenOfType<TextBox>().Single().Current.Value = "abcdefgh");
            AddUntilStep("overlay is hidden", () => accountCreation.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestFailedVerification()
        {
            AddStep("set up API", () => ((DummyAPIAccess)API).HandleRequest = req =>
            {
                switch (req)
                {
                    case VerifySessionRequest verifySessionRequest:
                        if (verifySessionRequest.VerificationKey == "abcdefgh")
                            verifySessionRequest.TriggerSuccess();
                        else
                        {
                            Scheduler.AddDelayed(
                                () => verifySessionRequest.TriggerFailure(new APIException("Incorrect verification code.", null, HttpStatusCode.UnprocessableEntity)),
                                1000);
                        }

                        return true;

                    default:
                        return false;
                }
            });

            AddStep("log out", () => API.Logout());

            AddStep("show manually", () => accountCreation.Show());
            AddUntilStep("overlay is visible", () => accountCreation.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("click button", () => accountCreation.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddUntilStep("warning screen is present", () => accountCreation.ChildrenOfType<ScreenWarning>().SingleOrDefault()?.IsPresent, () => Is.True);

            AddStep("proceed", () => accountCreation.ChildrenOfType<DangerousSettingsButton>().Single().TriggerClick());
            AddUntilStep("entry screen is present", () => accountCreation.ChildrenOfType<ScreenEntry>().SingleOrDefault()?.IsPresent, () => Is.True);

            AddStep("input details", () =>
            {
                var entryScreen = accountCreation.ChildrenOfType<ScreenEntry>().Single();
                entryScreen.ChildrenOfType<OsuTextBox>().ElementAt(0).Text = "new_user";
                entryScreen.ChildrenOfType<OsuTextBox>().ElementAt(1).Text = "new.user@fake.mail";
                entryScreen.ChildrenOfType<OsuTextBox>().ElementAt(2).Text = "password";
            });
            AddStep("click button", () => accountCreation.ChildrenOfType<ScreenEntry>().Single()
                                                         .ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddUntilStep("verification screen is present", () => accountCreation.ChildrenOfType<ScreenEmailVerification>().SingleOrDefault()?.IsPresent, () => Is.True);

            AddStep("fail to verify", () =>
                accountCreation.ChildrenOfType<ScreenEmailVerification>().Single().ChildrenOfType<TextBox>().Single().Current.Value = "abcdefff");
            AddUntilStep("overlay is still visible", () => accountCreation.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddUntilStep("text box back to enabled",
                () => accountCreation.ChildrenOfType<ScreenEmailVerification>().Single().ChildrenOfType<TextBox>().Single().Current.Disabled, () => Is.False);

            AddStep("verify", () =>
                accountCreation.ChildrenOfType<ScreenEmailVerification>().Single().ChildrenOfType<TextBox>().Single().Current.Value = "abcdefgh");
            AddUntilStep("overlay is hidden", () => accountCreation.State.Value, () => Is.EqualTo(Visibility.Hidden));
        }
    }
}
