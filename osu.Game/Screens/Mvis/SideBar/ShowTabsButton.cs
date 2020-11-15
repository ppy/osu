// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar
{
    internal class ShowTabsButton : OsuClickableContainer
    {
        private readonly Box hoverBox;

        public ShowTabsButton()
        {
            Size = new Vector2(50);
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            TooltipText = "切换Header";
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(0.4f)
                },
                new SpriteIcon
                {
                    Icon = FontAwesome.Solid.Bars,
                    Size = new Vector2(20),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                hoverBox = new Box
                {
                    Colour = Colour4.White.Opacity(0.2f),
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Masking = true;
            Content.CornerRadius = 12.5f;
            Content.Anchor = Content.Origin = Anchor.Centre;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverBox.FadeIn(300);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverBox.FadeOut(300);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.ScaleTo(0.9f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }
    }
}
