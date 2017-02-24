// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens.Testing;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseOptions : TestCase
    {
        public override string Name => @"Options";

        public override string Description => @"Tests the options overlay";

        private OptionsOverlay options;

        public override void Reset()
        {
            base.Reset();

            Children = new[] { options = new OptionsOverlay() };
            options.ToggleVisibility();
        }
    }
}
