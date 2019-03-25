// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.AccountCreation;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestCaseAccountCreationOverlay : OsuTestCase
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

        [Cached(typeof(IAPIProvider))]
        private DummyAPIAccess api = new DummyAPIAccess();

        public TestCaseAccountCreationOverlay()
        {
            Container userPanelArea;
            AccountCreationOverlay accountCreation;

            Children = new Drawable[]
            {
                api,
                accountCreation = new AccountCreationOverlay(),
                userPanelArea = new Container
                {
                    Padding = new MarginPadding(10),
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
            };

            api.Logout();
            api.LocalUser.BindValueChanged(user => { userPanelArea.Child = new UserPanel(user.NewValue) { Width = 200 }; }, true);

            AddStep("show", () => accountCreation.State = Visibility.Visible);
            AddStep("logout", () => api.Logout());
        }
    }
}
