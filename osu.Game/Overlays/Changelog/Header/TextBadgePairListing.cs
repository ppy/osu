// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input;
using System;

namespace osu.Game.Overlays.Changelog.Header
{
    public class TextBadgePairListing : TextBadgePair
    {
        private TextBadgePairRelease releaseBadge;
        private ColourInfo badgeColour;

        public TextBadgePairListing(ColourInfo badgeColour) : base(badgeColour, "Listing")
        {
            this.releaseBadge = releaseBadge;
            this.badgeColour = badgeColour;
            text.Font = "Exo2.0-Bold";
        }

        public override void Activate()
        {
            lineBadge.IsCollapsed = false;
            text.Font = "Exo2.0-Bold";
            SetTextColor(Color4.White, 100);
            OnActivation?.Invoke();
        }

        public override void Deactivate()
        {
            lineBadge.IsCollapsed = true;
            //text.Font = "Exo2.0-Regular"; // commented out since it makes bad resize-jumping
            SetTextColor(badgeColour, 100);
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
    }
}
