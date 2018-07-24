// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using static osu.Framework.Graphics.UserInterface.TextBox;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledTextBox : CompositeDrawable
    {
        private readonly OsuTextBox textBox;
        private readonly Container content;
        private readonly OsuSpriteText label;

        private const float label_container_width = 150;
        private const float outer_corner_radius = 15;
        private const float inner_corner_radius = 10;
        private const float default_height = 40;
        private const float default_label_left_padding = 15;
        private const float default_label_top_padding = 12;
        private const float default_label_text_size = 16;

        public event OnCommitHandler OnCommit;

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

        private string placeholderText;
        public string PlaceholderText
        {
            get => placeholderText;
            set
            {
                placeholderText = value;
                textBox.PlaceholderText = value;
            }
        }

        private string text;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                textBox.Text = value;
            }
        }

        private float height = default_height;
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
            RelativeSizeAxes = Axes.X;
            base.Height = default_height + Padding.Top;
            CornerRadius = outer_corner_radius;
            Masking = true;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                CornerRadius = outer_corner_radius,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex("1c2125"),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = default_height,
                        Child = new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = default_height,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    label = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Padding = new MarginPadding { Left = default_label_left_padding, Top = default_label_top_padding },
                                        Colour = Color4.White,
                                        TextSize = default_label_text_size,
                                        Font = @"Exo2.0-Bold",
                                    },
                                    textBox = new OsuTextBox
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        RelativeSizeAxes = Axes.X,
                                        Height = default_height,
                                        CornerRadius = inner_corner_radius,
                                    },
                                },
                            },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Absolute, label_container_width),
                                new Dimension()
                            }
                        }
                    }
                }
            };

            textBox.OnCommit += OnCommit;
        }
    }
}
