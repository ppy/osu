//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.GameModes.Testing;
using osu.Game.Overlays;
using osu.Framework.Timing;
using osu.Framework;

namespace osu.Desktop.Tests
{
    class TestCaseMusicController : TestCase
    {
        public override string Name => @"Music Controller";
        public override string Description => @"Tests music controller ui.";

        IFrameBasedClock ourClock;
        protected override IFrameBasedClock Clock => ourClock;

        public override void Load(BaseGame game)
        {
            base.Load(game);
            ourClock = new FramedClock();
        }

        public override void Reset()
        {
            base.Reset();
            ourClock.ProcessFrame();
            MusicController mc = new MusicController
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
