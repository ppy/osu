// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.AccountCreation;
using osu.Game.Overlays.Settings;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneAccountCreationOverlay : OsuTestScene
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

            AddStep("log back in", () => API.Login("dummy", "password"));
            AddUntilStep("overlay is hidden", () => accountCreation.State.Value == Visibility.Hidden);
        }
    }
}
