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
        private const int height = 30;
        private const int width = 200;

        private Color4 backgroundHoveredColour => OsuColour.FromHex(@"172023");
        private Color4 backgroundColour => OsuColour.FromHex(@"223034");

        protected Color4 TextColour { set { text.Colour = value; } }

        private readonly OsuSpriteText text;
        private readonly Box background;

        public ContextMenuItem(string title)
        {
            Width = width;
            Height = height;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = backgroundColour,
                },
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = 18,
                    Text = title,
                    Font = @"Exo2.0",
                    Margin = new MarginPadding{ Left = 20 },
                }
            };
        }

        protected override bool OnHover(InputState state)
        {
            background.Colour = backgroundHoveredColour;
            text.Font = @"Exo2.0-Bold";
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            background.Colour = backgroundColour;
            text.Font = @"Exo2.0";
            base.OnHoverLost(state);
        }
    }
}
