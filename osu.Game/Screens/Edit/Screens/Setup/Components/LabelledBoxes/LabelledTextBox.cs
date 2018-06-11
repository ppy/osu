// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes
{
    public class LabelledTextBox : Drawable
    {
        // Determine the accessibility of the following
        private readonly OsuTextBox textBox;
        private readonly Container content;
        private readonly OsuSpriteText label;

        public const float DEFAULT_LABEL_TEXT_SIZE = 16;
        public const float DEFAULT_HEIGHT = 50;
        public const float DEFAULT_LABEL_PADDING = 20;
        public const float DEFAULT_TEXT_BOX_PADDING = 300;

        public string LabelText
        {
            get => label.Text;
            set => label.Text = value;
        }

        public float LabelTextSize
        {
            get => label.TextSize;
            set => label.TextSize = value;
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
            content = new Container
            {
                Colour = OsuColour.FromHex("1c2125"),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Height = DEFAULT_HEIGHT,
                Children = new Drawable[]
                {
                    textBox = new OsuTextBox
                    {
                        RelativeSizeAxes = Axes.Y,
                        Height = DEFAULT_HEIGHT,
                        Padding = new MarginPadding { Left = DEFAULT_TEXT_BOX_PADDING },
                    },
                    label = new OsuSpriteText
                    {
                        Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING },
                        Colour = Color4.White,
                        TextSize = DEFAULT_LABEL_TEXT_SIZE,
                        Font = @"Exo2.0-Bold",
                    },
                }
            };
        }
    }
}
