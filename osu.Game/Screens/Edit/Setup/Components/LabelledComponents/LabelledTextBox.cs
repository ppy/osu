// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Setup.Components.LabelledComponents
{
    public class LabelledTextBox : CompositeDrawable
    {
        private const float label_container_width = 150;
        private const float corner_radius = 15;
        private const float default_height = 40;
        private const float default_label_left_padding = 15;
        private const float default_label_top_padding = 12;
        private const float default_label_text_size = 16;

        public event TextBox.OnCommitHandler OnCommit;

        public bool ReadOnly
        {
            get => textBox.ReadOnly;
            set => textBox.ReadOnly = value;
        }

        public string LabelText
        {
            get => label.Text;
            set => label.Text = value;
        }

        public float LabelTextSize
        {
            get => label.Font.Size;
            set => label.Font = label.Font.With(size: value);
        }

        public string PlaceholderText
        {
            get => textBox.PlaceholderText;
            set => textBox.PlaceholderText = value;
        }

        public string Text
        {
            get => textBox.Text;
            set => textBox.Text = value;
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

        private readonly OsuTextBox textBox;
        private readonly Container content;
        private readonly OsuSpriteText label;

        public LabelledTextBox()
        {
            RelativeSizeAxes = Axes.X;
            Height = default_height;
            CornerRadius = corner_radius;
            Masking = true;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                CornerRadius = corner_radius,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex("1c2125"),
                    },
                    new GridContainer
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
                                    Font = OsuFont.GetFont(size: default_label_text_size, weight: FontWeight.Bold),
                                },
                                textBox = new OsuTextBox
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 1,
                                    CornerRadius = corner_radius,
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
            };

            textBox.OnCommit += OnCommit;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            textBox.BorderColour = colours.Blue;
        }
    }
}
