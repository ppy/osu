// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Utils;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneMetricNumbers : OsuTestScene
    {
        public TestSceneMetricNumbers()
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
                    new DrawableNumber(999_999),
                    new DrawableNumber(1_000_000),
                    new DrawableNumber(845_006_456),
                    new DrawableNumber(999_999_999),
                    new DrawableNumber(1_000_000_000),
                    new DrawableNumber(7_875_454_545),
                    new DrawableNumber(999_999_999_999),
                    new DrawableNumber(1_000_000_000_000),
                    new DrawableNumber(687_545_454_554_545),
                    new DrawableNumber(999_999_999_999_999),
                    new DrawableNumber(1_000_000_000_000_000),
                    new DrawableNumber(587_545_454_554_545_455),
                    new DrawableNumber(999_999_999_999_999_999),
                    new DrawableNumber(1_000_000_000_000_000_000),
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
                Text = HumanizerUtils.ToReadableString(value);
            }
        }
    }
}
