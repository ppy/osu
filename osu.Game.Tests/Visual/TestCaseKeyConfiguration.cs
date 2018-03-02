// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseKeyConfiguration : OsuTestCase
    {
        private readonly KeyBindingOverlay overlay;

        public TestCaseKeyConfiguration()
        {
            Child = overlay = new KeyBindingOverlay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            overlay.Show();
        }
    }
}
