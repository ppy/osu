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

namespace osu.Desktop.Tests
{
    class TestCaseMusicController : TestCase
    {
        public override string Name => @"Music Controller";
        public override string Description => @"Tests music controller ui.";

        public override void Reset()
        {
            base.Reset();
            MusicController mc = new MusicController
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre
            };
            Add(mc);
            AddToggle(@"Show", mc.ToggleVisibility);
        }
    }
}
