// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class NewComboColourButton : CircularContainer
    {
        private readonly Box fill;
        private readonly CircularContainer button;

        public const float SIZE_X = 75;
        public const float SIZE_Y = 75;
        public const float BOTTOM_LABEL_TEXT_SIZE = 14;
        public const float COLOUR_LABEL_TEXT_SIZE = 11;

        public event Action ButtonClicked;

        public NewComboColourButton()
        {
            Size = new Vector2(SIZE_X, SIZE_Y);

            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = SIZE_Y + 5 },
                    Colour = Color4.White,
                    Text = "New",
                    TextSize = BOTTOM_LABEL_TEXT_SIZE
                },
                button = new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    BorderThickness = 3.5f,
                    Alpha = 0.5f,
                    Child = fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        Alpha = 0,
                        AlwaysPresent = true
                    },
                },
                new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_plus,
                    Size = new Vector2(10)
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            button.BorderColour = osuColour.Blue;
        }

        protected override bool OnClick(InputState state)
        {
            ButtonClicked?.Invoke();
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            fill.FadeTo(0.2f, 500, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            fill.FadeTo(0, 500, Easing.OutQuint);
            base.OnHoverLost(state);
        }
    }
}
