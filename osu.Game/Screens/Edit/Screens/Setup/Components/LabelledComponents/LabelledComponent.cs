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

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledComponent : CompositeDrawable
    {
        private readonly Box background;
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;

        private const float corner_radius = 15;
        private const float label_text_size = 16;
        private const float bottom_label_text_size = 12;
        private const float base_height = 40;
        private const float label_horizontal_offset = 15;
        private const float label_vertical_offset = 12;
        private const float switch_horizontal_offset = 15;
        private const float switch_vertical_offset = 10;

        protected bool HasBottomText => true;

        public string LabelText
        {
            get => label.Text;
            set => label.Text = value;
        }

        public string BottomLabelText
        {
            get => bottomText.Text;
            set
            {
                bottomText.Text = value;
                Height = base_height + (value != "" ? 20 : 0);
            }
        }

        public float LabelTextSize
        {
            get => label.TextSize;
            set => label.TextSize = value;
        }

        public Color4 LabelTextColour
        {
            get => label.Colour;
            set => label.Colour = value;
        }

        public Color4 BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }

        public LabelledComponent()
        {
            RelativeSizeAxes = Axes.X;
            Height = base_height;
            CornerRadius = corner_radius;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("1c2125"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                label = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Position = new Vector2(label_horizontal_offset, label_vertical_offset),
                                    Colour = Color4.White,
                                    TextSize = label_text_size,
                                    Font = @"Exo2.0-Bold",
                                },
                                CreateComponent(),
                            },
                        },
                        bottomText = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Position = new Vector2(label_horizontal_offset, -label_vertical_offset),
                            TextSize = bottom_label_text_size,
                            Font = @"Exo2.0-BoldItalic",
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            bottomText.Colour = osuColour.Yellow;
        }

        protected virtual Drawable CreateComponent() => null;
    }
}
