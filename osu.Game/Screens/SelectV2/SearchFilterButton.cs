// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class SearchFilterButton : ClickableContainer
    {
        public Drawable? HoverTarget { get; set; }

        public IHasPopover? PopoverTarget { get; set; }

        private SpriteIcon? icon;
        private Vector2 pendingShear;

        public SearchFilterButton()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = icon = new SpriteIcon
            {
                Icon = FontAwesome.Solid.Search,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(16),
                Shear = pendingShear
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            HoverTarget?.FadeIn(200);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            HoverTarget?.FadeOut(200);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (PopoverTarget is Drawable drawable)
                drawable.HidePopover();
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (IsHovered && PopoverTarget is IHasPopover hasPopover)
                hasPopover.ShowPopover();

            base.OnMouseUp(e);
        }

        public void SetIconShear(Vector2 shear)
        {
            pendingShear = shear;
            if (icon != null)
                icon.Shear = shear;
        }
    }
}
