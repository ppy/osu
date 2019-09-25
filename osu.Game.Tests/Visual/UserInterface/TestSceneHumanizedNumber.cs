// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Utils;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneHumanizedNumber : OsuTestScene
    {
        public TestSceneHumanizedNumber()
        {
            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new DrawableNumber(0),
                    new DrawableNumber(1001),
                    new DrawableNumber(845006456),
                    new DrawableNumber(9875454545),
                    new DrawableNumber(987545454554545),
                    new DrawableNumber(987545454554545455),
                    new DrawableNumber(long.MaxValue),
                }
            };
        }

        private class DrawableNumber : SpriteText, IHasTooltip
        {
            public string TooltipText => value.ToString("F0");

            private readonly long value;

            public DrawableNumber(long value)
            {
                this.value = value;
                Text = HumanizerUtils.Humanize(value);
            }
        }
    }
}
