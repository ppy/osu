// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Overlays;
using osu.Framework.Graphics.Containers;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseMusicController : TestCase
    {
        public override string Description => @"Tests music controller ui.";

        private MusicController mc;

        public TestCaseMusicController()
        {
            Clock = new FramedClock();
        }

        public override void Reset()
        {
            base.Reset();
            Clock.ProcessFrame();
            mc = new MusicController
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre
            };
            Add(mc);

            AddToggleStep(@"toggle visibility", state => mc.State = state ? Visibility.Visible : Visibility.Hidden);
            AddStep(@"show", () => mc.State = Visibility.Visible);
        }
    }
}
