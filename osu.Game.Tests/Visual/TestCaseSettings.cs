// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseSettings : OsuTestCase
    {
        private readonly SettingsOverlay settings;

        public TestCaseSettings()
        {
            Children = new[] { settings = new MainSettings() };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            settings.ToggleVisibility();
        }
    }
}
