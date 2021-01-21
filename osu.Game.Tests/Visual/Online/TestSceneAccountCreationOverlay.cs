// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneAccountCreationOverlay : OsuTestScene
    {
        private readonly Container userPanelArea;
        private readonly AccountCreationOverlay accountCreation;

        private IBindable<User> localUser;

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
            API.Logout();

            localUser = API.LocalUser.GetBoundCopy();
            localUser.BindValueChanged(user => { userPanelArea.Child = new UserGridPanel(user.NewValue) { Width = 200 }; }, true);
        }

        [Test]
        public void TestOverlayVisibility()
        {
            AddStep("start hidden", () => accountCreation.Hide());
            AddStep("log out", API.Logout);

            AddStep("show manually", () => accountCreation.Show());
            AddUntilStep("overlay is visible", () => accountCreation.State.Value == Visibility.Visible);

            AddStep("log back in", () => API.Login("dummy", "password"));
            AddUntilStep("overlay is hidden", () => accountCreation.State.Value == Visibility.Hidden);
        }
    }
}
