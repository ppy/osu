// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input;

namespace osu.Game.Overlays.Changelog.Header
{
    public class TextBadgePairListing : TextBadgePair
    {
        private ColourInfo badgeColour;

        public TextBadgePairListing(ColourInfo badgeColour) : base(badgeColour, "Listing", false)
        {
            this.badgeColour = badgeColour;
            text.Font = "Exo2.0-Bold";
            text.Anchor = Anchor.TopCentre;
            text.Origin = Anchor.TopCentre;

            // I'm using this for constant badge width here, so that the whole
            // thing doesn't jump left/right when listing's size changes
            // due to different font weight (and thus width)
            lineBadge.RelativeSizeAxes = Axes.None;

            // this doesn't work without the scheduler
            // (because the text isn't yet fully drawn when it's loaded?)
            text.OnLoadComplete = d => Scheduler.Add(UpdateBadgeWidth);
        }

        public override void Activate()
        {
            lineBadge.IsCollapsed = false;
            text.Font = "Exo2.0-Bold";
            SetTextColour(Color4.White, 100);
            OnActivation?.Invoke();
        }

        public override void Deactivate()
        {
            lineBadge.IsCollapsed = true;
            text.Font = "Exo2.0-Regular"; // commented out since it makes bad resize-jumping
            SetTextColour(badgeColour, 100);
            OnDeactivation?.Invoke();
        }

        protected override bool OnClick(InputState state)
        {
            Activate();
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            lineBadge.ResizeHeightTo(lineBadge.UncollapsedHeight, lineBadge.TransitionDuration);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (lineBadge.IsCollapsed) lineBadge.ResizeHeightTo(1, lineBadge.TransitionDuration);
            base.OnHoverLost(state);
        }

        public void UpdateBadgeWidth() => lineBadge.ResizeWidthTo(text.DrawWidth);
    }
}
