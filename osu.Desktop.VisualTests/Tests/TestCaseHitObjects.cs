//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.Beatmaps.Objects.Osu.Drawable;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseHitObjects : TestCase
    {
        public override string Name => @"Hit Objects";

        IFrameBasedClock ourClock;

        protected override IFrameBasedClock Clock => ourClock;

        protected override void Load(BaseGame game)
        {
            base.Load(game);

            var swClock = new StopwatchClock(true) { Rate = 1 };
            ourClock = new FramedClock(swClock);
        }

        public override void Reset()
        {
            base.Reset();

            ourClock.ProcessFrame();

            for (int i = 0; i < 20; i++)
            {
                var h = new Circle
                {
                    StartTime = ourClock.CurrentTime + 1000 + i * 80,
                    Position = new OpenTK.Vector2(i * 14),
                };

                Add(new DrawableCircle(h)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Depth = -i,
                    State = ArmedState.Armed,
                });
            }
        }

        protected override void Update()
        {
            base.Update();
            ourClock.ProcessFrame();
        }
    }
}
