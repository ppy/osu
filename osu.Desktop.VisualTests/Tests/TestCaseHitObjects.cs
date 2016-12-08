//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using OpenTK;
using osu.Framework.Allocation;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseHitObjects : TestCase
    {
        public override string Name => @"Hit Objects";

        public TestCaseHitObjects()
        {
            var swClock = new StopwatchClock(true) { Rate = 0.2f };
            Clock = new FramedClock(swClock);
        }

        public override void Reset()
        {
            base.Reset();

            Clock.ProcessFrame();

            Container approachContainer = new Container { Depth = float.MinValue, };

            Add(approachContainer);

            const int count = 10;

            for (int i = 0; i < count; i++)
            {
                var h = new HitCircle
                {
                    StartTime = Clock.CurrentTime + 1000 + i * 80,
                    Position = new Vector2((i - count / 2) * 14),
                };

                DrawableHitCircle d = new DrawableHitCircle(h)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Depth = i,
                    State = ArmedState.Hit,
                    Judgement = new OsuJudgementInfo { Result = HitResult.Hit }
                };


                approachContainer.Add(d.ApproachCircle.CreateProxy());
                Add(d);
            }
        }
    }
}
