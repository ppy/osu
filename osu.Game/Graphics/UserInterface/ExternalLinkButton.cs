// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ExternalLinkButton : CompositeDrawable, IHasTooltip, IHasContextMenu
    {
        public string? Link { get; set; }

        private Color4 hoverColour;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private Clipboard clipboard { get; set; } = null!;

        [Resolved]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        private readonly SpriteIcon linkIcon;

        public ExternalLinkButton(string? link = null)
        {
            Link = link;
            Size = new Vector2(12);
            InternalChildren = new Drawable[]
            {
                linkIcon = new SpriteIcon
                {
                    Icon = FontAwesome.Solid.ExternalLinkAlt,
                    RelativeSizeAxes = Axes.Both
                },
                new HoverClickSounds()
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoverColour = colours.Yellow;
        }

        protected override bool OnHover(HoverEvent e)
        {
            linkIcon.FadeColour(hoverColour, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            linkIcon.FadeColour(Color4.White, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Link != null)
                host.OpenUrlExternally(Link);
            return true;
        }

        public LocalisableString TooltipText => "view in browser";

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (Link != null)
                {
                    items.Add(new OsuMenuItem("Open", MenuItemType.Highlighted, () => host.OpenUrlExternally(Link)));
                    items.Add(new OsuMenuItem("Copy URL", MenuItemType.Standard, copyUrl));
                }

                return items.ToArray();
            }
        }

        private void copyUrl()
        {
            if (Link != null)
            {
                clipboard.SetText(Link);
                onScreenDisplay?.Display(new CopyUrlToast());
            }
        }
    }
}
