// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class GrayButton : OsuAnimatedButton
    {
        protected SpriteIcon Icon { get; private set; }
        protected Box Background { get; private set; }

        private readonly IconUsage icon;

        [Resolved]
        private OsuColour colours { get; set; }

        public GrayButton(IconUsage icon)
        {
            this.icon = icon;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(new Drawable[]
            {
                Background = new Box
                {
                    Colour = colours.Gray4,
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                Icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Icon = icon,
                },
            });
        }
    }
}
