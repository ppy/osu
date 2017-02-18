﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMusicController : TestCase
    {
        public override string Name => @"Music Controller";
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
            AddToggle(@"Show", mc.ToggleVisibility);
        }
    }
}
