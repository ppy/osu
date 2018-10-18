// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Changelog.Header
{
    public class TextBadgePairListing : TextBadgePair
    {
        private readonly ColourInfo badgeColour;

        public TextBadgePairListing(ColourInfo badgeColour) : base(badgeColour, "Listing", false)
        {
            IsActivated = true;
            this.badgeColour = badgeColour;
            Text.Font = "Exo2.0-Bold";
            Text.Anchor = Anchor.TopCentre;
            Text.Origin = Anchor.TopCentre;

            // I'm using this for constant badge width here, so that the whole
            // thing doesn't jump left/right when listing's size changes
            // due to different font weight (and thus width)
            LineBadge.RelativeSizeAxes = Axes.None;

            // this doesn't work without the scheduler
            // (because the text isn't yet fully drawn when it's loaded?)
            Text.OnLoadComplete = d => Scheduler.Add(UpdateBadgeWidth);
        }

        public override void Activate()
        {
            if (IsActivated)
                return;
            IsActivated = true;
            LineBadge.Uncollapse();
            Text.Font = "Exo2.0-Bold";
            SetTextColour(Color4.White, 100);
        }

        public override void Deactivate()
        {
            IsActivated = false;
            LineBadge.Collapse();
            Text.Font = "Exo2.0-Regular";
            SetTextColour(badgeColour, 100);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Activate();
            return base.OnClick(e);
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

        public void UpdateBadgeWidth() => LineBadge.ResizeWidthTo(Text.DrawWidth);
    }
}
