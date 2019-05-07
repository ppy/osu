// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class ExternalLinkButton : CompositeDrawable, IHasTooltip
    {
        public string Link { get; set; }

        private Color4 hoverColour;
        private GameHost host;

        public ExternalLinkButton(string link = null)
        {
            Link = link;
            Size = new Vector2(12);
            InternalChild = new SpriteIcon
            {
                Icon = FontAwesome.Solid.ExternalLinkAlt,
                RelativeSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, GameHost host)
        {
            hoverColour = colours.Yellow;
            this.host = host;
        }

        protected override bool OnHover(HoverEvent e)
        {
            InternalChild.FadeColour(hoverColour, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            InternalChild.FadeColour(Color4.White, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Link != null)
                host.OpenUrlExternally(Link);
            return true;
        }

        public string TooltipText => "View in browser";
    }
}
