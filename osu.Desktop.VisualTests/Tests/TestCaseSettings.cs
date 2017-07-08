// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseSettings : TestCase
    {
        public override string Description => @"Tests the settings overlay";

        private readonly SettingsOverlay settings;

        public TestCaseSettings()
        {
            Children = new[] { settings = new SettingsOverlay() };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            settings.ToggleVisibility();
        }
    }
}
