// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseTextAwesome : OsuTestCase
    {
        public TestCaseTextAwesome()
        {
            FillFlowContainer flow;

            Add(new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = flow = new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                },
            });

            foreach (FontAwesome fa in Enum.GetValues(typeof(FontAwesome)))
                flow.Add(new Icon(fa));
        }

        private class Icon : Container, IHasTooltip
        {
            public string TooltipText { get; }

            public Icon(FontAwesome fa)
            {
                TooltipText = fa.ToString();

                AutoSizeAxes = Axes.Both;
                Child = new SpriteIcon
                {
                    Icon = fa,
                    Size = new Vector2(60),
                };
            }
        }
    }
}
