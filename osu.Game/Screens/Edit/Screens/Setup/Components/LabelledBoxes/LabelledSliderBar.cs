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

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes
{
    public class LabelledSliderBar : CompositeDrawable
    {
        private readonly Container content;
        private readonly Container outerContainer;
        private readonly Box box;
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;
        private readonly OsuTickSliderBar sliderBar;

        public const float LABEL_CONTAINER_WIDTH = 150;
        public const float OUTER_CORNER_RADIUS = 15;
        public const float INNER_CORNER_RADIUS = 10;
        public const float DEFAULT_LABEL_TEXT_SIZE = 16;
        public const float DEFAULT_BOTTOM_LABEL_TEXT_SIZE = 12;
        public const float NORMAL_HEIGHT = 60;
        public const float DEFAULT_LABEL_PADDING = 15;
        public const float DEFAULT_TOP_PADDING = 10;
        public const float DEFAULT_BOTTOM_PADDING = 12;
        public const float DEFAULT_LEFT_SLIDER_BAR_PADDING = 20;
        public const float DEFAULT_RIGHT_SLIDER_BAR_PADDING = 35;

        public event Action<float> SliderBarValueChanged;

        public void TriggerSliderBarValueChanged(float newValue)
        {
            SliderBarValueChanged?.Invoke(newValue);
        }

        public float CurrentValue
        {
            get => sliderBar.Current.Value;
            set
            {
                sliderBar.Current.Value = value;
                TriggerSliderBarValueChanged(value);
            }
        }

        private float sliderMinValue;
        public float SliderMinValue
        {
            get => sliderMinValue;
            set
            {
                sliderMinValue = value;
                sliderBar.MinValue = value;
            }
        }

        private float sliderMaxValue = 10;
        public float SliderMaxValue
        {
            get => sliderMaxValue;
            set
            {
                sliderMaxValue = value;
                sliderBar.MaxValue = value;
            }
        }

        private float sliderNormalPrecision = 1;
        public float SliderNormalPrecision
        {
            get => sliderNormalPrecision;
            set
            {
                sliderNormalPrecision = value;
                sliderBar.NormalPrecision = value;
            }
        }

        private float sliderAlternatePrecision = 1;
        public float SliderAlternatePrecision
        {
            get => sliderAlternatePrecision;
            set
            {
                sliderAlternatePrecision = value;
                sliderBar.AlternatePrecision = value;
            }
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

        private string labelText;
        public string LabelText
        {
            get => labelText;
            set
            {
                labelText = value;
                label.Text = value;
            }
        }

        private string bottomLabelText;
        public string BottomLabelText
        {
            get => bottomLabelText;
            set
            {
                bottomLabelText = value;
                bottomText.Text = value;
                changeHeight(NORMAL_HEIGHT + (value != "" ? 20 : 0));
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

        private float labelTextSize;
        public float LabelTextSize
        {
            get => labelTextSize;
            set
            {
                labelTextSize = value;
                label.TextSize = value;
            }
        }

        public MarginPadding Padding
        {
            get => base.Padding;
            set
            {
                base.Padding = value;
                Height = NORMAL_HEIGHT + base.Padding.Top;
            }
        }

        public MarginPadding LabelPadding
        {
            get => label.Padding;
            set => label.Padding = value;
        }

        public MarginPadding SliderBarPadding
        {
            get => sliderBar.Padding;
            set => sliderBar.Padding = value;
        }

        public Color4 LabelTextColour
        {
            get => label.Colour;
            set => label.Colour = value;
        }

        public Color4 BackgroundColour
        {
            get => content.Colour;
            set => content.Colour = value;
        }

        public LabelledSliderBar()
        {
            Masking = true;
            CornerRadius = OUTER_CORNER_RADIUS;
            RelativeSizeAxes = Axes.X;
            Height = NORMAL_HEIGHT + Padding.Top;

            InternalChildren = new Drawable[]
            {
                outerContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = NORMAL_HEIGHT,
                    CornerRadius = 15,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        box = new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = NORMAL_HEIGHT,
                            Colour = OsuColour.FromHex("1c2125"),
                        },
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = NORMAL_HEIGHT,
                            Children = new Drawable[]
                            {
                                new GridContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = NORMAL_HEIGHT,
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            label = new OsuSpriteText
                                            {
                                                Anchor = Anchor.TopLeft,
                                                Origin = Anchor.TopLeft,
                                                Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING, Top = DEFAULT_TOP_PADDING },
                                                Colour = Color4.White,
                                                TextSize = DEFAULT_LABEL_TEXT_SIZE,
                                                Text = LabelText,
                                                Font = @"Exo2.0-Bold",
                                            },
                                            sliderBar = new OsuTickSliderBar(sliderMinValue, sliderMaxValue, sliderNormalPrecision, sliderAlternatePrecision)
                                            {
                                                Anchor = Anchor.TopLeft,
                                                Origin = Anchor.TopLeft,
                                                Y = DEFAULT_TOP_PADDING,
                                                Padding = new MarginPadding { Left = DEFAULT_LEFT_SLIDER_BAR_PADDING, Right = DEFAULT_RIGHT_SLIDER_BAR_PADDING },
                                                Colour = Color4.White,
                                                TooltipTextSuffix = tooltipTextSuffix
                                            },
                                        },
                                    },
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.Absolute, LABEL_CONTAINER_WIDTH),
                                        new Dimension()
                                    }
                                },
                                bottomText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING, Bottom = DEFAULT_BOTTOM_PADDING },
                                    TextSize = DEFAULT_BOTTOM_LABEL_TEXT_SIZE,
                                    Font = @"Exo2.0-BoldItalic",
                                    Text = BottomLabelText
                                },
                            }
                        }
                    }
                }
            };

            sliderBar.Current.ValueChanged += TriggerSliderBarValueChanged;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            bottomText.Colour = osuColour.Yellow;
        }

        private void changeHeight(float newHeight)
        {
            Height = newHeight + Padding.Top;
            content.Height = newHeight;
            box.Height = newHeight;
            outerContainer.Height = newHeight;
        }
    }
}
