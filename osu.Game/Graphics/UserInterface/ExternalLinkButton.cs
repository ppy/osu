// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Platform;
using OpenTK;
using OpenTK.Graphics;

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
                Icon = FontAwesome.fa_external_link,
                RelativeSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, GameHost host)
        {
            hoverColour = colours.Yellow;
            this.host = host;
        }

        protected override bool OnHover(InputState state)
        {
            InternalChild.FadeColour(hoverColour, 500, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            InternalChild.FadeColour(Color4.White, 500, Easing.OutQuint);
            base.OnHoverLost(state);
        }

        protected override bool OnClick(InputState state)
        {
            if(Link != null)
                host.OpenUrlExternally(Link);
            return true;
        }

        public string TooltipText => "View in browser";
    }
}
