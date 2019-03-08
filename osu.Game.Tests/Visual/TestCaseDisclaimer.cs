// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual
{
    public class TestCaseDisclaimer : ScreenTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            LoadScreen(new Disclaimer());
        }
    }
}
