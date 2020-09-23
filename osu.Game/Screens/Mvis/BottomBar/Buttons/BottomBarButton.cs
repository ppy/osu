// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Graphics.Containers;
using osuTK.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Effects;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class BottomBarButton : OsuClickableContainer
    {
        protected FillFlowContainer contentFillFlow;
        protected readonly Box bgBox;
        private Box flashBox;
        private Container content;
        public SpriteIcon spriteIcon;
        public IconUsage ButtonIcon;
        public Drawable ExtraDrawable;
        public bool NoIcon;

        public BottomBarButton()
        {
            Size = new Vector2(30, 30);

            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    CornerRadius = 5,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 3f,
                        Colour = Color4.Black.Opacity(0.6f),
                    },
                    Children = new Drawable[]
                    {
                        bgBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
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
                        flashBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Alpha = 0,
                        }
                    }
                }
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

            // From OsuAnimatedButton
            if (AutoSizeAxes != Axes.None)
            {
                content.RelativeSizeAxes = (Axes.Both & ~AutoSizeAxes);
                content.AutoSizeAxes = AutoSizeAxes;
            }
        }

        protected override bool OnMouseDown(Framework.Input.Events.MouseDownEvent e)
        {
            content.ScaleTo(0.8f, 2000, Easing.OutQuint);
            flashBox.FadeTo(0.8f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(Framework.Input.Events.MouseUpEvent e)
        {
            content.ScaleTo(1f, 1000, Easing.OutElastic);
            flashBox.FadeOut(1000, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        protected override bool OnClick(Framework.Input.Events.ClickEvent e)
        {
            flashBox.FadeTo(1).Then().FadeOut(300); // `this.FlashColour(Color4.White, 300)` 不管用
            return base.OnClick(e);
        }
    }
}
