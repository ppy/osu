// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.VisualTests.Tests
{
    class TestCaseSpriteText : TestCase
    {
        public override string Name => @"SpriteText";

        public override string Description => @"Test all sizes of text rendering";

        public override void Reset()
        {
            base.Reset();

            FlowContainer flow;

            Children = new Drawable[]
            {
                new ScrollContainer
                {
                    Children = new[]
                    {
                        flow = new FlowContainer
                        {
                            Anchor = Anchor.TopLeft,
                            Direction = FlowDirection.VerticalOnly,
                        }
                    }
                }
            };

            for (int i = 1; i <= 200; i++)
            {
                SpriteText text = new SpriteText
                {
                    Text = $@"Font testy at size {i}",
                    TextSize = i
                };

                flow.Add(text);
            }
        }
    }
}
