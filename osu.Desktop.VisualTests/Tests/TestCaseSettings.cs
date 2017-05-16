// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseSettings : TestCase
    {
        public override string Description => @"Tests the settings overlay";

        private SettingsOverlay settings;

        public override void Reset()
        {
            base.Reset();

            Children = new[] { settings = new SettingsOverlay() };
            settings.ToggleVisibility();
        }
    }
}
