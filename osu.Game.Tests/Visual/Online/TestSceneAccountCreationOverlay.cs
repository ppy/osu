// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.AccountCreation;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneAccountCreationOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ErrorTextFlowContainer),
            typeof(AccountCreationBackground),
            typeof(ScreenEntry),
            typeof(ScreenWarning),
            typeof(ScreenWelcome),
            typeof(AccountCreationScreen),
        };

        private readonly Container userPanelArea;

        private Bindable<User> localUser;

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
