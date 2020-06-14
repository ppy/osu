// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.PurePlayer.Components
{
    public class MusicPanelButton : OsuAnimatedButton
    {
        protected FillFlowContainer contentFillFlow;
        private Box flashBox;
        public SpriteIcon spriteIcon;
        public IconUsage ButtonIcon;
        public Drawable ExtraDrawable;
        public bool NoIcon;

        public MusicPanelButton()
        {
            content.FadeEdgeEffectTo(Colour4.Black.Opacity(0));
            Children = new Drawable[]
            {
                flashBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(0.5f),
                    Alpha = 0,
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
                            Size = new Vector2(30),
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

        protected override bool OnMouseDown(Framework.Input.Events.MouseDownEvent e)
        {
            flashBox.FadeTo(0.8f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(Framework.Input.Events.MouseUpEvent e)
        {
            flashBox.FadeOut(300, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        protected override bool OnClick(Framework.Input.Events.ClickEvent e)
        {
            flashBox.FadeTo(1).Then().FadeOut(300); // `this.FlashColour(Color4.White, 300)` 不管用
            return base.OnClick(e);
        }
    }
}
