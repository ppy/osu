// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledTextBox : CompositeDrawable
    {
        private readonly OsuSetupTextBox textBox;
        private readonly Container content;
        private readonly OsuSpriteText label;

        public const float LABEL_CONTAINER_WIDTH = 150;
        public const float OUTER_CORNER_RADIUS = 15;
        public const float INNER_CORNER_RADIUS = 10;
        public const float DEFAULT_HEIGHT = 40;
        public const float DEFAULT_LABEL_LEFT_PADDING = 15;
        public const float DEFAULT_LABEL_TOP_PADDING = 12;
        public const float DEFAULT_LABEL_TEXT_SIZE = 16;

        public event Action<string> TextBoxTextChanged;

        public void TriggerTextBoxTextChanged(string newText)
        {
            TextBoxTextChanged?.Invoke(newText);
        }

        private bool readOnly;
        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                textBox.ReadOnly = value;
                readOnly = value;
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

        private string textBoxPlaceholderText;
        public string TextBoxPlaceholderText
        {
            get => textBoxPlaceholderText;
            set
            {
                textBoxPlaceholderText = value;
                textBox.PlaceholderText = value;
            }
        }

        private string textBoxText;
        public string TextBoxText
        {
            get => textBoxText;
            set
            {
                textBoxText = value;
                textBox.Text = value;
                TextBoxTextChanged?.Invoke(value);
            }
        }

        private float height = DEFAULT_HEIGHT;
        public float Height
        {
            get => height;
            private set
            {
                height = value;
                textBox.Height = value;
                content.Height = value;
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

        public MarginPadding TextBoxPadding
        {
            get => textBox.Padding;
            set => textBox.Padding = value;
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

        public LabelledTextBox()
        {
            Masking = true;
            CornerRadius = OUTER_CORNER_RADIUS;
            RelativeSizeAxes = Axes.X;
            base.Height = DEFAULT_HEIGHT + Padding.Top;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = DEFAULT_HEIGHT,
                    CornerRadius = OUTER_CORNER_RADIUS,
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
                                            Padding = new MarginPadding { Left = DEFAULT_LABEL_LEFT_PADDING, Top = DEFAULT_LABEL_TOP_PADDING },
                                            Colour = Color4.White,
                                            TextSize = DEFAULT_LABEL_TEXT_SIZE,
                                            Text = LabelText,
                                            Font = @"Exo2.0-Bold",
                                        },
                                        textBox = new OsuSetupTextBox
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            RelativeSizeAxes = Axes.X,
                                            Height = DEFAULT_HEIGHT,
                                            ReadOnly = ReadOnly,
                                            CornerRadius = INNER_CORNER_RADIUS,
                                        },
                                    },
                                },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, LABEL_CONTAINER_WIDTH),
                                    new Dimension()
                                }
                            }
                        }
                    }
                }
            };

            textBox.OnCommit += delegate { TriggerTextBoxTextChanged(textBox.Text); };
        }
    }
}
