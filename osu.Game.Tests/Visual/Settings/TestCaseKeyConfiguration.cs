// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Settings
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
