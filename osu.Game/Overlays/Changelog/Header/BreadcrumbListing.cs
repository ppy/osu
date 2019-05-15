// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog.Header
{
    public class BreadcrumbListing : Breadcrumb
    {
        private readonly ColourInfo badgeColour;

        public BreadcrumbListing(ColourInfo badgeColour)
            : base(badgeColour, "Listing", false)
        {
            this.badgeColour = badgeColour;
            Text.Font = Text.Font.With(weight: FontWeight.Bold);
            Text.Anchor = Anchor.TopCentre;
            Text.Origin = Anchor.TopCentre;

            AutoSizeAxes = Axes.None;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Activate();
            Width = Text.DrawWidth;
        }

        public override void Activate()
        {
            if (IsActivated)
                return;

            base.Activate();
            SetTextColour(Color4.White, 100);
        }

        public override void Deactivate()
        {
            if (!IsActivated)
                return;

            base.Deactivate();
            SetTextColour(badgeColour, 100);
        }

        protected override bool OnHover(HoverEvent e)
        {
            LineBadge.Uncollapse();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!IsActivated)
                LineBadge.Collapse();
            base.OnHoverLost(e);
        }
    }
}
