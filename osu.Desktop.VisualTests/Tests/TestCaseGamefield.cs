//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.GameModes.Play.Catch;
using osu.Game.GameModes.Play.Mania;
using osu.Game.GameModes.Play.Osu;
using osu.Game.GameModes.Play.Taiko;
using System.Collections.Generic;
using osu.Framework.Timing;

namespace osu.Desktop.Tests
{
    class TestCaseGamefield : TestCase
    {
        public override string Name => @"Gamefield";

        public override string Description => @"Showing hitobjects and what not.";

        FramedClock localClock;

        protected override IFrameBasedClock Clock => localClock;

        public override void Reset()
        {
            base.Reset();

            //ensure we are at offset 0
            localClock = new FramedClock();

            List<HitObject> objects = new List<HitObject>();

            int time = 500;
            for (int i = 0; i < 100; i++)
            {
                objects.Add(new Circle()
                {
                    StartTime = time,
                    Position = new Vector2(RNG.Next(0, 512), RNG.Next(0, 384))
                });

                time += RNG.Next(50, 500);
            }

            Beatmap beatmap = new Beatmap
            {
                HitObjects = objects
            };

            Add(new Drawable[]
            {
                new OsuHitRenderer
                {
                    Objects = beatmap.HitObjects,
                    Scale = new Vector2(0.5f),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft
                },
                new TaikoHitRenderer
                {
                    Objects = beatmap.HitObjects,
                    Scale = new Vector2(0.5f),
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight
                },
                new CatchHitRenderer
                {
                    Objects = beatmap.HitObjects,
                    Scale = new Vector2(0.5f),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                },
                new ManiaHitRenderer
                {
                    Objects = beatmap.HitObjects,
                    Scale = new Vector2(0.5f),
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight
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
