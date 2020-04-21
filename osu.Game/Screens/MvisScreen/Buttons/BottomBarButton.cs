// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Mvis.Buttons
{
    public class BottomBarButton : OsuAnimatedButton
    {
        private SpriteIcon spriteIcon;
        protected readonly Box bgBox;
        public IconUsage ButtonIcon;

        public BottomBarButton()
        {
            Size = new Vector2(30, 30);

            Children = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = Color4Extensions.FromHex("#5a5a5a"),
                },
                spriteIcon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Icon = ButtonIcon,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            this.spriteIcon.Icon = ButtonIcon;
        }
    }
}
