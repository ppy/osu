// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class ContextMenuItem : ClickableContainer
    {
        private readonly Box background;
        private readonly OsuSpriteText text;

        public ContextMenuItem(string title)
        {
            Width = 150;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Colour = Color4.White,
                    TextSize = 20,
                    Text = title,
                    Font = @"Exo2.0",
                    Margin = new MarginPadding{ Left = 10 },
                }
            };
        }

        protected override bool OnHover(InputState state)
        {
            background.Colour = Color4.Blue;
            text.Colour = Color4.Yellow;
            text.Font = @"Exo2.0-Bold";
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            background.Colour = Color4.Black;
            text.Colour = Color4.White;
            text.Font = @"Exo2.0";
            base.OnHoverLost(state);
        }
    }
}
