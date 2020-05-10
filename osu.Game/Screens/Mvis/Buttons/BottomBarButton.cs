// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Mvis.Buttons
{
    public class BottomBarButton : OsuAnimatedButton
    {
        protected FillFlowContainer contentFillFlow;
        protected readonly Box bgBox;
        public SpriteIcon spriteIcon;
        public IconUsage ButtonIcon;
        public Drawable ExtraDrawable;
        public bool NoIcon;

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
                contentFillFlow = new FillFlowContainer
                {
                    Margin = new MarginPadding{ Left = 15, Right = 15 },
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        spriteIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(13),
                            Icon = ButtonIcon,
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            this.spriteIcon.Icon = ButtonIcon;

            if ( ExtraDrawable != null )
            {
                this.contentFillFlow.Add(ExtraDrawable);
                this.spriteIcon.Size = new Vector2(18);
            }

            if ( NoIcon == true )
                this.contentFillFlow.Remove(spriteIcon);
        }
    }
}
