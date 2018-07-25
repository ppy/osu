// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledSliderBar : CompositeDrawable
    {
        private readonly Box box;
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;
        private readonly SetupTickSliderBar sliderBar;

        private const float label_container_width = 150;
        private const float corner_radius = 15;
        private const float default_label_text_size = 16;
        private const float default_bottom_label_text_size = 12;
        private const float normal_height = 60;
        private const float default_label_padding = 15;
        private const float default_top_padding = 10;
        private const float default_bottom_padding = 12;
        private const float default_left_slider_bar_padding = 20;
        private const float default_right_slider_bar_padding = 35;

        public event Action<float> SliderBarValueChanged;

        public float CurrentValue
        {
            get => sliderBar.Current.Value;
            set => sliderBar.Current.Value = value;
        }

        public float SliderMinValue
        {
            get => sliderBar.MinValue;
            set => sliderBar.MinValue = value;
        }

        public float SliderMaxValue
        {
            get => sliderBar.MaxValue;
            set => sliderBar.MaxValue = value;
        }

        public float SliderNormalPrecision
        {
            get => sliderBar.NormalPrecision;
            set => sliderBar.NormalPrecision = value;
        }

        public float SliderAlternatePrecision
        {
            get => sliderBar.AlternatePrecision;
            set => sliderBar.AlternatePrecision = value;
        }

        private string tooltipTextSuffix = "";
        public string TooltipTextSuffix
        {
            get => tooltipTextSuffix;
            set
            {
                tooltipTextSuffix = value;
                sliderBar.TooltipTextSuffix = value;
            }
        }

        public string LabelText
        {
            get => label.Text;
            set => label.Text = value;
        }

        public string BottomLabelText
        {
            get => bottomText.Text;
            set
            {
                bottomText.Text = value;
                Height = normal_height + (value != "" ? 20 : 0);
            }
        }

        public string LeftTickCaption
        {
            get => sliderBar.LeftTickCaption;
            set => sliderBar.LeftTickCaption = value;
        }
        public string MiddleTickCaption
        {
            get => sliderBar.MiddleTickCaption;
            set => sliderBar.MiddleTickCaption = value;
        }
        public string RightTickCaption
        {
            get => sliderBar.RightTickCaption;
            set => sliderBar.RightTickCaption = value;
        }

        public float LabelTextSize
        {
            get => label.TextSize;
            set => label.TextSize = value;
        }

        public Color4 LabelTextColour
        {
            get => label.Colour;
            set => label.Colour = value;
        }

        public Color4 BackgroundColour
        {
            get => box.Colour;
            set => box.Colour = value;
        }

        public LabelledSliderBar()
        {
            RelativeSizeAxes = Axes.X;
            Height = normal_height;
            Masking = true;
            CornerRadius = corner_radius;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("1c2125"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    label = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Padding = new MarginPadding { Left = default_label_padding, Top = default_top_padding },
                                        Colour = Color4.White,
                                        TextSize = default_label_text_size,
                                        Font = @"Exo2.0-Bold",
                                    },
                                    sliderBar = new SetupTickSliderBar(0, 10, 1, 1)
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Y = default_top_padding,
                                        Padding = new MarginPadding { Left = default_left_slider_bar_padding, Right = default_right_slider_bar_padding },
                                        Colour = Color4.White,
                                        TooltipTextSuffix = tooltipTextSuffix
                                    },
                                },
                            },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Absolute, label_container_width),
                                new Dimension()
                            }
                        },
                        bottomText = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding { Left = default_label_padding, Bottom = default_bottom_padding },
                            TextSize = default_bottom_label_text_size,
                            Font = @"Exo2.0-BoldItalic",
                        },
                    }
                }
            };

            sliderBar.Current.ValueChanged += SliderBarValueChanged;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            bottomText.Colour = osuColour.Yellow;
        }
    }
}
