// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private IBindable<User> localUser;

        public TestSceneAccountCreationOverlay()
        {
            AccountCreationOverlay accountCreation;

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

            AddStep("show", () => accountCreation.Show());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            API.Logout();

            localUser = API.LocalUser.GetBoundCopy();
            localUser.BindValueChanged(user => { userPanelArea.Child = new UserGridPanel(user.NewValue) { Width = 200 }; }, true);

            AddStep("logout", API.Logout);
        }
    }
}
