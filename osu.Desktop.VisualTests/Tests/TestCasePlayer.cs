//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.GameModes.Testing;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.GameModes.Play;
using OpenTK;

namespace osu.Desktop.VisualTests.Tests
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

            int time = 1500;
            for (int i = 0; i < 50; i++)
            {
                objects.Add(new Circle()
                {
                    StartTime = time,
                    Position = new Vector2(RNG.Next(100, 400), RNG.Next(100, 200))
                });

                time += 500;
            }

            Add(new Player
            {
                Beatmap = new WorkingBeatmap(new Beatmap
                {
                    HitObjects = objects
                })
            });
        }

        protected override void Update()
        {
            base.Update();
            localClock.ProcessFrame();
        }
    }
}
