// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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
    public class LabelledTextBox : CompositeDrawable
    {
        private readonly OsuTextBox textBox;
        private readonly Container content;
        private readonly OsuSpriteText label;

        public const float DEFAULT_LABEL_TEXT_SIZE = 18;
        public const float DEFAULT_HEIGHT = 50;
        public const float DEFAULT_LABEL_PADDING = 15;

        public Action<string> TextBoxTextChangedAction;

        public void TriggerTextBoxTextChanged(string newText)
        {
            TextBoxTextChangedAction?.Invoke(newText);
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
            set => base.Padding = value;
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

        public Color4 TextColour
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
            CornerRadius = 15;
            //RelativeSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            base.Height = DEFAULT_HEIGHT;

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
                                            Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING, Top = DEFAULT_LABEL_PADDING },
                                            Colour = Color4.White,
                                            TextSize = DEFAULT_LABEL_TEXT_SIZE,
                                            Text = LabelText,
                                            Font = @"Exo2.0-Bold",
                                        },
                                        textBox = new OsuTextBox
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            RelativeSizeAxes = Axes.X,
                                            Height = DEFAULT_HEIGHT,
                                            CornerRadius = 15,
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
            textBox.OnCommit += delegate { TriggerTextBoxTextChanged(textBox.Text); };
        }
    }
}
