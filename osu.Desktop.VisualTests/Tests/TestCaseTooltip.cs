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
            TooltipSlider slider;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0,10),
                    Children = new Drawable[]
                    {
                        new TooltipContainer("Text with some tooltip"),
                        new TooltipContainer("and another one"),
                        new TooltipTextbox
                        {
                            Text = "a box with a tooltip",
                            Size = new Vector2(300,30),
                        },
                        slider = new TooltipSlider
                        {
                            Width = 300,
                        },
                    },
                },
            };


            slider.Current.BindTo(new BindableInt(5)
            {
                MaxValue = 10,
                MinValue = 0
            });
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

        private class TooltipSlider : OsuSliderBar<int>, IHasDelayedTooltip, IHasOverhangingTooltip
        {
            public string Tooltip => Current.Value.ToString();

            int IHasDelayedTooltip.Delay => Overhanging ? 0 : 250;

            public bool Overhanging { get; set; }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                Overhanging = true;
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                Overhanging = false;
                return base.OnMouseUp(state, args);
            }
        }
    }
}
