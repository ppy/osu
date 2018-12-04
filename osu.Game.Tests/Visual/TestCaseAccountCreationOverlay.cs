// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    public class TestCaseAccountCreationOverlay : OsuTestCase
    {
        public TestCaseAccountCreationOverlay()
        {
            var accountCreation = new AccountCreationOverlay();
            Child = accountCreation;

            accountCreation.State = Visibility.Visible;
        }
    }
}
