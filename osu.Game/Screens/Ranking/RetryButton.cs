// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class RetryButton : OsuAnimatedButton
    {
        private readonly Box background;

        [Resolved]
        private Player? player { get; set; }

        public RetryButton()
        {
            Size = new Vector2(50, 30);

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Icon = FontAwesome.Solid.Redo,
                },
            };

            TooltipText = "retry";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Green;

            if (player != null)
                Action = () => player.Restart();
        }
    }
}
