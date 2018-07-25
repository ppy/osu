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

        private const float corner_radius = 15;
        private const float default_label_text_size = 16;
        private const float default_bottom_label_text_size = 12;
        private const float normal_height = 40;
        private const float default_label_padding = 15;
        private const float default_top_padding = 12;
        private const float default_bottom_padding = 12;

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
                changeHeight(normal_height + (value != "" ? 20 : 0));
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
            CornerRadius = corner_radius;
            RelativeSizeAxes = Axes.X;
            Height = normal_height;

            InternalChildren = new Drawable[]
            {
                outerContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = normal_height,
                    CornerRadius = corner_radius,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        box = new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = normal_height,
                            Colour = OsuColour.FromHex("1c2125"),
                        },
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = normal_height,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = normal_height,
                                    Children = new Drawable[]
                                    {
                                        label = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Padding = new MarginPadding { Left = default_label_padding, Top = default_top_padding },
                                            Colour = Color4.White,
                                            TextSize = default_label_text_size,
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
                                    Padding = new MarginPadding { Left = default_label_padding, Bottom = default_bottom_padding },
                                    TextSize = default_bottom_label_text_size,
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
