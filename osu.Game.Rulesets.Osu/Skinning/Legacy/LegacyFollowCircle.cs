// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacyFollowCircle : FollowCircle
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

        protected override void OnSliderPress()
        {
            Debug.Assert(ParentObject != null);

            double remainingTime = Math.Max(0, ParentObject.HitStateUpdateTime - Time.Current);

            // Note that the scale adjust here is 2 instead of DrawableSliderBall.FOLLOW_AREA to match legacy behaviour.
            // This means the actual tracking area for gameplay purposes is larger than the sprite (but skins may be accounting for this).
            this.ScaleTo(1f).ScaleTo(2f, Math.Min(180f, remainingTime), Easing.Out)
                .FadeTo(0).FadeTo(1f, Math.Min(60f, remainingTime));
        }

        protected override void OnSliderRelease()
        {
        }

        protected override void OnSliderEnd()
        {
            this.ScaleTo(1.6f, 200, Easing.Out)
                .FadeOut(200, Easing.In);
        }

        protected override void OnSliderTick()
        {
            if (Scale.X >= 2f)
            {
                this.ScaleTo(2.2f)
                    .ScaleTo(2f, 200);
            }
        }

        protected override void OnSliderBreak()
        {
            this.ScaleTo(4f, 100)
                .FadeTo(0f, 100);
        }
    }
}
