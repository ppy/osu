//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Input;
using osu.Game.Overlays;
using osu.Framework.Graphics.Containers;

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
