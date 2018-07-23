// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input.States;

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
            IsActivated = true;
            LineBadge.Uncollapse();
            Text.Font = "Exo2.0-Bold";
            SetTextColour(Color4.White, 100);
            SampleActivate?.Play();
            OnActivation?.Invoke();
        }

        public override void Deactivate()
        {
            IsActivated = false;
            LineBadge.Collapse();
            Text.Font = "Exo2.0-Regular"; // commented out since it makes bad resize-jumping
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
            LineBadge.Uncollapse();
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (!IsActivated)
                LineBadge.Collapse();
            base.OnHoverLost(state);
        }

        public void UpdateBadgeWidth() => LineBadge.ResizeWidthTo(Text.DrawWidth);
    }
}
