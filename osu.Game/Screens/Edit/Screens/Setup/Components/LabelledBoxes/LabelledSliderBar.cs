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
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;
        private readonly OsuTickSliderBar sliderBar;

        public const float DEFAULT_LABEL_TEXT_SIZE = 20;
        public const float DEFAULT_BOTTOM_LABEL_TEXT_SIZE = 14;
        public const float DEFAULT_HEIGHT = 75;
        public const float DEFAULT_LABEL_PADDING = 15;
        public const float DEFAULT_TOP_PADDING = 15;
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

        private float sliderValueInterval = 1;
        public float SliderValueInterval
        {
            get => sliderValueInterval;
            set
            {
                sliderValueInterval = value;
                sliderBar.ValueInterval = value;
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
            }
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
            base.Height = DEFAULT_HEIGHT + Padding.Top;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = DEFAULT_HEIGHT,
                    CornerRadius = 15,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = DEFAULT_HEIGHT,
                            Colour = OsuColour.FromHex("1c2125"),
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = DEFAULT_HEIGHT,
                            Child = new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = DEFAULT_HEIGHT,
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
                                        sliderBar = new OsuTickSliderBar(sliderMinValue, sliderMaxValue, sliderValueInterval)
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Padding = new MarginPadding { Left = DEFAULT_SLIDER_BAR_PADDING, Top = DEFAULT_TOP_PADDING },
                                            //Width = 0.7f,
                                            Colour = Color4.White,
                                        },
                                        bottomText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Colour = Color4.Yellow,
                                            TextSize = DEFAULT_BOTTOM_LABEL_TEXT_SIZE,
                                            Font = @"Exo2.0-BoldItalic",
                                            Text = BottomLabelText
                                        },
                                    },
                                },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, 180),
                                    new Dimension()
                                }
                            }
                        }
                    }
                }
            };
            sliderBar.Current.ValueChanged += delegate { TriggerSliderBarValueChanged(sliderBar.Current.Value); };
        }
    }
}
