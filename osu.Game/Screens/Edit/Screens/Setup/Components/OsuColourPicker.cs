// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuColourPicker : Container, IHasCurrentValue<Color4>
    {
        private readonly OsuCircularButton copyButton;
        private readonly OsuCircularButton pasteButton;

        private bool isColourChangedFromGradient;

        public const float SIZE_X = 200;
        public const float SIZE_Y = 350;
        public const float COLOUR_INFO_HEIGHT = 30;
        public const float DEFAULT_PADDING = 10;

        private float leftPadding => OsuColourButton.SIZE_Y / 2 + DEFAULT_PADDING * 2;

        public event Action<Color4> ColourChanged;

        public OsuColourPicker()
        {
            SetupOsuTextBox colourText;
            Box colourPreviewFill;
            OsuColourPickerGradient colourPickerGradient;
            OsuColourPickerHue colourPickerHue;

            Size = new Vector2(0, OsuColourButton.SIZE_Y);
            CornerRadius = 10;
            Masking = true;
            
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("232e34"),
                },
                new Box
                {
                    Size = new Vector2(SIZE_X, 75),
                    Colour = OsuColour.FromHex("1c2125"),
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Size = new Vector2(SIZE_X, COLOUR_INFO_HEIGHT + DEFAULT_PADDING),
                            Children = new Drawable[]
                            {
                                colourText = new SetupOsuTextBox
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Size = new Vector2(SIZE_X - leftPadding, COLOUR_INFO_HEIGHT),
                                    Position = new Vector2(-DEFAULT_PADDING, DEFAULT_PADDING),
                                    CornerRadius = COLOUR_INFO_HEIGHT / 2,
                                    Text = "#ffffff",
                                },
                                new CircularContainer
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Size = new Vector2(COLOUR_INFO_HEIGHT),
                                    Position = new Vector2(-DEFAULT_PADDING, DEFAULT_PADDING),
                                    Masking = true,
                                    Child = colourPreviewFill = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.White,
                                        AlwaysPresent = true,
                                    },
                                },
                            }
                        },
                        new Container // TODO: Fix the spacing here
                        {
                            Size = new Vector2(SIZE_X, SIZE_Y - COLOUR_INFO_HEIGHT - DEFAULT_PADDING),
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10),
                                    Padding = new MarginPadding { Right = 15 },
                                    Children = new[]
                                    {
                                        copyButton = new OsuCircularButton
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Size = new Vector2((SIZE_X - leftPadding - 10) / 2, 20),
                                            Margin = new MarginPadding { Left = leftPadding - 10 },
                                            CornerRadius = 10,
                                            LabelText = "Copy",
                                        },
                                        pasteButton = new OsuCircularButton
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Size = new Vector2((SIZE_X - leftPadding - 10) / 2, 20),
                                            CornerRadius = 10,
                                            LabelText = "Paste",
                                        }
                                    }
                                }
                            }
                        },
                    }
                },
                colourPickerGradient = new OsuColourPickerGradient
                {
                    Position = new Vector2(10, 85),
                    ActiveColour = Color4.Red
                },
                colourPickerHue = new OsuColourPickerHue
                {
                    Position = new Vector2(10, 275),
                }
            };

            Current.Value = Color4.White;
            Current.ValueChanged += newValue =>
            {
                if (!isColourChangedFromGradient)
                {
                    colourPickerGradient.Current.Value = newValue;
                    colourPickerHue.Hue = Color4.ToHsv(newValue).X;
                }
                colourPreviewFill.FadeColour(newValue, 200, Easing.OutQuint);
                colourText.Text = toHexRGBString(newValue);
                TriggerColourChanged(newValue);
            };

            colourPickerHue.HueChanged += a => colourPickerGradient.ActiveColour = a;
            colourPickerGradient.SelectedColourChanged += a =>
            {
                isColourChangedFromGradient = true;
                Current.Value = a;
                isColourChangedFromGradient = false;
            };
            
            colourText.OnCommit += delegate
            {
                try
                {
                    Current.Value = OsuColour.FromHex(colourText.Text.Substring(1, 6));
                }
                catch
                {
                    colourText.Text = toHexRGBString(Current.Value);
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            copyButton.DefaultColour = osuColour.Blue;
            pasteButton.DefaultColour = osuColour.Blue;
        }

        public void Expand()
        {
            this.ResizeWidthTo(SIZE_X, 500, Easing.OutQuint);
            this.ResizeHeightTo(SIZE_Y, 500, Easing.OutQuint);
        }

        public void Collapse()
        {
            this.ResizeHeightTo(OsuColourButton.SIZE_Y, 500, Easing.OutQuint);
            this.ResizeWidthTo(0, 500, Easing.OutQuint);
        }

        public void TriggerColourChanged(Color4 newValue)
        {
            ColourChanged?.Invoke(newValue);
        }

        public Bindable<Color4> Current { get; } = new Bindable<Color4>();

        private string toHexRGBString(Color4 colour) => $"#{((byte)(colour.R * 255)).ToString("X2").ToLower()}{((byte)(colour.G * 255)).ToString("X2").ToLower()}{((byte)(colour.B * 255)).ToString("X2").ToLower()}";
    }
}
