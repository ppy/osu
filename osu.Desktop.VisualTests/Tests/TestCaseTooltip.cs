// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Configuration;
using OpenTK;
using osu.Game.Graphics;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTooltip : TestCase
    {
        public override string Description => "tests tooltips on various elements";

        public override void Reset()
        {
            base.Reset();
            OsuSliderBar<int> slider;
            OsuSliderBar<double> sliderDouble;

            const float width = 400;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        new TooltipTextContainer("text with a tooltip"),
                        new TooltipTextContainer("more text with another tooltip"),
                        new TooltipTextbox
                        {
                            Text = "a textbox with a tooltip",
                            Size = new Vector2(width,30),
                        },
                        slider = new OsuSliderBar<int>
                        {
                            Width = width,
                        },
                        sliderDouble = new OsuSliderBar<double>
                        {
                            Width = width,
                        },
                    },
                },
            };

            slider.Current.BindTo(new BindableInt(5)
            {
                MaxValue = 10,
                MinValue = 0
            });

            sliderDouble.Current.BindTo(new BindableDouble(0.5)
            {
                MaxValue = 1,
                MinValue = 0
            });
        }

        private class TooltipTextContainer : Container, IHasTooltip
        {
            private readonly OsuSpriteText text;

            public string TooltipText => text.Text;

            public TooltipTextContainer(string tooltipText)
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
            public string TooltipText => Text;
        }
    }
}
