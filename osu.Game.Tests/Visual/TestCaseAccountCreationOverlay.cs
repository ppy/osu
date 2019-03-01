// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.AccountCreation;

namespace osu.Game.Tests.Visual
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

        public TestCaseAccountCreationOverlay()
        {
            var accountCreation = new AccountCreationOverlay();
            Child = accountCreation;

            accountCreation.State = Visibility.Visible;
        }
    }
}
