//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.GameModes.Testing;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using System.Collections.Generic;
using osu.Framework.Timing;
using osu.Game.GameModes.Play;

namespace osu.Desktop.Tests
{
    class TestCasePlayer : TestCase
    {
        public override string Name => @"Player";

        public override string Description => @"Showing everything to play the game.";

        FramedClock localClock;

        protected override IFrameBasedClock Clock => localClock;

        public override void Reset()
        {
            base.Reset();

            //ensure we are at offset 0
            localClock = new FramedClock();

            var objects = new List<HitObject>();

            int time = 500;
            for (int i = 0; i < 1; i++)
            {
                objects.Add(new Circle()
                {
                    StartTime = time,
                    Position = new Vector2(RNG.Next(0, 512), RNG.Next(0, 384))
                });

                time += 500;
            }

            Add(new Player()
            {
                Beatmap = new Beatmap
                {
                    HitObjects = objects
                }
            });
        }

        protected override void Update()
        {
            base.Update();
            localClock.ProcessFrame();
        }
    }
}
