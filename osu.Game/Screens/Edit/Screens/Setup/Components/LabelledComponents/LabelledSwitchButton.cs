// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
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
    public class LabelledSwitchButton : CompositeDrawable
    {
        private readonly Container content;
        private readonly Container outerContainer;
        private readonly Box box;
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;
        private readonly OsuSetupSwitchButton switchButton;

        public const float OUTER_CORNER_RADIUS = 15;
        public const float DEFAULT_LABEL_TEXT_SIZE = 16;
        public const float DEFAULT_BOTTOM_LABEL_TEXT_SIZE = 12;
        public const float NORMAL_HEIGHT = 40;
        public const float DEFAULT_LABEL_PADDING = 15;
        public const float DEFAULT_TOP_PADDING = 12;
        public const float DEFAULT_BOTTOM_PADDING = 12;

        public event Action<bool> SwitchButtonValueChanged;

        public void TriggerSwitchButtonValueChanged(bool newValue)
        {
            SwitchButtonValueChanged?.Invoke(newValue);
        }

        public bool CurrentValue
        {
            get => switchButton.Current.Value;
            set => switchButton.Current.Value = value;
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
                Height = Height + base.Padding.Top;
            }
        }

        public MarginPadding LabelPadding
        {
            get => label.Padding;
            set => label.Padding = value;
        }

        public MarginPadding RadioButtonPadding
        {
            get => switchButton.Padding;
            set => switchButton.Padding = value;
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

        public LabelledSwitchButton()
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
                    CornerRadius = OUTER_CORNER_RADIUS,
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
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = NORMAL_HEIGHT,
                                    Children = new Drawable[]
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
                                        switchButton = new OsuSetupSwitchButton
                                        {
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            Position = new Vector2(-15, 10),
                                        },
                                    },
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

            switchButton.Current.ValueChanged += TriggerSwitchButtonValueChanged;
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
