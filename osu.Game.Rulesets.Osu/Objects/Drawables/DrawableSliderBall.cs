// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSliderBall : CircularContainer, ISliderProgress
    {
        public const float FOLLOW_AREA = 2.4f;

        private DrawableSlider drawableSlider;
        private Drawable ball;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableSlider)
        {
            this.drawableSlider = (DrawableSlider)drawableSlider;

            Origin = Anchor.Centre;

            Size = OsuHitObject.OBJECT_DIMENSIONS;

            Children = new[]
            {
                new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SliderFollowCircle), _ => new DefaultFollowCircle())
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                ball = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SliderBall), _ => new DefaultSliderBall())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null)
        {
            // Consider the case of rewinding - children's transforms are handled internally, so propagating down
            // any further will cause weirdness with the Tracking bool below. Let's not propagate further at this point.
            base.ClearTransformsAfter(time, false, targetMember);
        }

        public override void ApplyTransformsAt(double time, bool propagateChildren = false)
        {
            // For the same reasons as above w.r.t rewinding, we shouldn't propagate to children here either.

            // ReSharper disable once RedundantArgumentDefaultValue
            base.ApplyTransformsAt(time, false);
        }

        public void UpdateProgress(double completionProgress)
        {
            Slider slider = drawableSlider.HitObject;
            Position = slider.CurvePositionAt(completionProgress);

            // 0.1 / slider.Path.Distance is the additional progress needed to ensure the diff length is 0.1
            double checkDistance = 0.1 / slider.Path.Distance;
            var diff = slider.CurvePositionAt(Math.Min(1 - checkDistance, completionProgress)) - slider.CurvePositionAt(Math.Min(1, completionProgress + checkDistance));

            // Ensure the value is substantially high enough to allow for Atan2 to get a valid angle.
            // Needed for when near completion, or in case of a very short slider.
            if (diff.LengthFast < 0.01f)
                return;

            ball.Rotation = -90 + (float)(-Math.Atan2(diff.X, diff.Y) * 180 / Math.PI);
        }
    }
}
