// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public const float DEFAULT_LABEL_TEXT_SIZE = 20;
        public const float DEFAULT_BOTTOM_LABEL_TEXT_SIZE = 14;
        public const float NORMAL_HEIGHT = 75;
        public const float DEFAULT_LABEL_PADDING = 15;
        public const float DEFAULT_TOP_PADDING = 15;
        public const float DEFAULT_BOTTOM_PADDING = 15;
        public const float DEFAULT_SLIDER_BAR_PADDING = 300;

        public Action<float> SliderBarValueChangedAction;

        public void TriggerSliderBarValueChanged(float newValue)
        {
            SliderBarValueChangedAction?.Invoke(newValue);
        }

        private float sliderMinValue = 0;
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
                changeHeight(NORMAL_HEIGHT + (value != "" ? 25 : 0));
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
                base.Height = Height + base.Padding.Top;
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
            CornerRadius = 15;
            RelativeSizeAxes = Axes.X;
            base.Height = NORMAL_HEIGHT + Padding.Top;

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
                                                Padding = new MarginPadding { Left = DEFAULT_SLIDER_BAR_PADDING, Top = DEFAULT_TOP_PADDING },
                                                Colour = Color4.White,
                                            },
                                        },
                                    },
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.Absolute, 180),
                                        new Dimension()
                                    }
                                },
                                bottomText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Colour = Color4.Yellow,
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
            sliderBar.Current.ValueChanged += delegate { TriggerSliderBarValueChanged(sliderBar.Current.Value); };
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
