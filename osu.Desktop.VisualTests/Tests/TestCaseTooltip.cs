// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Game.Graphics.Cursor;
using OpenTK;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTooltip : TestCase
    {
        public override string Description => "tests tooltips on various elements";

        public override void Reset()
        {
            base.Reset();

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new TooltipContainer("Text with some tooltip"),
                        new TooltipContainer("and another one"),
                        new TooltipTextbox
                        {
                            Text = "a box with a tooltip",
                            Width = 300,
                        },
                        new TooltipSlider
                        {
                            Bindable = new BindableInt(5)
                            {
                                MaxValue = 10,
                                MinValue = 0,
                            },
                            Size = new Vector2(300,16),
                        },
                    },
                },
            };
        }

        private class TooltipContainer : Container, IHasTooltip
        {
            private readonly OsuSpriteText text;

            public string Tooltip => text.Text;

            public TooltipContainer(string tooltipText)
            {
                AutoSizeAxes = Axes.Both;
                Children = new[]
                {
                    text = new OsuSpriteText
                    {
                        Text = tooltipText,
                    }
                };
            }

        }

        private class TooltipTextbox : OsuTextBox, IHasTooltip
        {
            public string Tooltip => Text;
        }

        private class TooltipSlider : OsuSliderBar<int>, IHasDelayedTooltip
        {
            public string Tooltip => Bindable.Value.ToString();

            int IHasDelayedTooltip.Delay => mousePressed ? 0 : 250;

            private bool mousePressed;

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                mousePressed = true;
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                mousePressed = false;
                return base.OnMouseUp(state, args);
            }
        }
    }
}
