//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Modes.Catch;
using osu.Game.Modes.Catch.UI;
using osu.Game.Modes.Mania;
using osu.Game.Modes.Mania.UI;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.Taiko;
using osu.Game.Modes.Taiko.UI;
using OpenTK;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseGamefield : TestCase
    {
        public override string Name => @"Gamefield";

        public override string Description => @"Showing hitobjects and what not.";

        public override void Reset()
        {
            base.Reset();

            //ensure we are at offset 0
            Clock = new FramedClock();

            List<HitObject> objects = new List<HitObject>();

            int time = 500;
            for (int i = 0; i < 100; i++)
            {
                objects.Add(new HitCircle()
                {
                    StartTime = time,
                    Position = new Vector2(RNG.Next(0, 512), RNG.Next(0, 384)),
                    Scale = RNG.NextSingle(0.5f, 1.0f),
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
    }
}
