// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using System;

namespace osu.Game.Screens.SelectV2
{
    public partial class SearchFilterButton : ClickableContainer
    {
        public Drawable? HoverTarget { get; set; }

        public Func<bool>? IsPopoverVisible { get; set; }
        public Action<bool>? SetPopoverVisible { get; set; }

        private SpriteIcon? icon;
        private Vector2 pendingShear;

        private bool popoverVisibleOnMouseDown;

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
            popoverVisibleOnMouseDown = IsPopoverVisible?.Invoke() == true;
            return base.OnMouseDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (SetPopoverVisible is Action<bool> setPopoverVisible)
            {
                setPopoverVisible(!popoverVisibleOnMouseDown);
                return true;
            }

            return base.OnClick(e);
        }

        public void SetIconShear(Vector2 shear)
        {
            pendingShear = shear;
            if (icon != null)
                icon.Shear = shear;
        }
    }
}
