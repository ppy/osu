// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Framework.Threading;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.GameModes.Testing;

namespace osu.Framework.VisualTests.Tests
{
    class TestCaseScrollableFlow : TestCase
    {
        private ScheduledDelegate boxCreator;

        public override string Name => @"Scrollable Flow";
        public override string Description => @"A flow container in a scroll container";

        Scheduler scheduler = new Scheduler();

        public override void Reset()
        {
            base.Reset();

            FlowContainer flow = new FlowContainer
            {
                LayoutDuration = 100,
                LayoutEasing = EasingTypes.Out,
                Padding = new Vector2(1, 1),
                SizeMode = InheritMode.X
            };

            boxCreator?.Cancel();

            boxCreator = scheduler.AddDelayed(delegate
            {
                if (Parent == null) return;

                Box box = new Box
                {
                    Size = new Vector2(80, 80),
                    Colour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1)
                };

                flow.Add(box);

                box.FadeInFromZero(1000);
                box.Delay(RNG.Next(0, 20000));
                box.FadeOutFromOne(4000);
                box.Expire();
            }, 100, true);

            scheduler.Add(boxCreator);

            ScrollContainer scrolling = new ScrollContainer();
            scrolling.Add(flow);
            Add(scrolling);
        }

        protected override void Update()
        {
            scheduler.Update();
            base.Update();
        }
    }
}
