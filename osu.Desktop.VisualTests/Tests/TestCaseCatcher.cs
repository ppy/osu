//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Input;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.Gameplay.Catch;
using osu.Framework.Timing;

namespace osu.Desktop.Tests
{
    class TestCaseCatcher : TestCase
    {
        FramedOffsetClock localClock;

        protected override IFrameBasedClock Clock => localClock;

        public override string Name => @"Catcher";

        public override string Description => @"Tests osu!catch catcher";

        public override void Reset()
        {
            base.Reset();

            //ensure we are at offset 0
            if (localClock == null)
                localClock = new FramedOffsetClock(base.Clock);
            localClock.Offset = -base.Clock.CurrentTime;

            Add(new Catcher()
            {
                Position = (new Vector2(240, 400)),
                //Clock
            });
        }

        protected override void Update()
        {
            base.Update();
            localClock.ProcessFrame();
        }
    }
}