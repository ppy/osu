// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyFollowCircle : FollowCircle
    {
        public LegacyFollowCircle(Drawable animationContent)
        {
            // follow circles are 2x the hitcircle resolution in legacy skins (since they are scaled down from >1x
            animationContent.Scale *= 0.5f;
            animationContent.Anchor = Anchor.Centre;
            animationContent.Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            InternalChild = animationContent;
        }

        protected override void OnTrackingChanged(ValueChangedEvent<bool> tracking)
        {
            Debug.Assert(ParentObject != null);

            if (ParentObject.Judged)
                return;

            double remainingTime = Math.Max(0, ParentObject.HitStateUpdateTime - Time.Current);

            // Note that the scale adjust here is 2 instead of DrawableSliderBall.FOLLOW_AREA to match legacy behaviour.
            // This means the actual tracking area for gameplay purposes is larger than the sprite (but skins may be accounting for this).
            if (tracking.NewValue)
            {
                // TODO: Follow circle should bounce on each slider tick.
                this.ScaleTo(0.5f).ScaleTo(2f, Math.Min(180f, remainingTime), Easing.Out)
                    .FadeTo(0).FadeTo(1f, Math.Min(60f, remainingTime));
            }
            else
            {
                // TODO: Should animate only at the next slider tick if we want to match stable perfectly.
                this.ScaleTo(4f, 100)
                    .FadeTo(0f, 100);
            }
        }

        protected override void OnSliderEnd()
        {
            this.ScaleTo(1.6f, 200, Easing.Out)
                .FadeOut(200, Easing.In);
        }
    }
}
