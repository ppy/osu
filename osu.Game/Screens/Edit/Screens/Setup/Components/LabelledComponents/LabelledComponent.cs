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
    public abstract class LabelledComponent : CompositeDrawable
    {
        protected const float CONTENT_PADDING = 10;
        protected const float CORNER_RADIUS = 15;

        private readonly Box background;
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;

        public string LabelText
        {
            get => label.Text;
            set => label.Text = value;
        }

        public string BottomLabelText
        {
            get => bottomText.Text;
            set => bottomText.Text = value;
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

        protected LabelledComponent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            CornerRadius = CORNER_RADIUS;
            Masking = true;

            Drawable component;
            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("1c2125"),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(CONTENT_PADDING),
                    Spacing = new Vector2(0, 12),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new[]
                            {
                                label = new OsuSpriteText
                                {
                                    Colour = Color4.White,
                                    Font = @"Exo2.0-Bold",
                                },
                                component = CreateComponent(),
                            },
                        },
                        bottomText = new OsuSpriteText
                        {
                            TextSize = 12,
                            Font = @"Exo2.0-BoldItalic",
                        },
                    }
                }
            };

            component.Anchor = Anchor.TopRight;
            component.Origin = Anchor.TopRight;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            bottomText.Colour = osuColour.Yellow;
        }

        protected abstract Drawable CreateComponent();
    }
}
