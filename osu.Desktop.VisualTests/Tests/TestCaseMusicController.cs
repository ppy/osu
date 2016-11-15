//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMusicController : TestCase
    {
        public override string Name => @"Music Controller";
        public override string Description => @"Tests music controller ui.";

        IFrameBasedClock ourClock;
        protected override IFrameBasedClock Clock => ourClock;

        protected MusicController mc;

        public TestCaseMusicController()
        {
            ourClock = new FramedClock();
        }

        public override void Reset()
        {
            base.Reset();
            ourClock.ProcessFrame();
            mc = new MusicController
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre
            };
            Add(mc);
            AddToggle(@"Show", mc.ToggleVisibility);
        }

        protected override void Update()
        {
            base.Update();
            ourClock.ProcessFrame();
        }
    }
}
