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
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;

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

        private Vector2? lastPosition;

        public void UpdateProgress(double completionProgress)
        {
            Position = drawableSlider.HitObject.CurvePositionAt(completionProgress);

            var diff = lastPosition.HasValue ? lastPosition.Value - Position : Position - drawableSlider.HitObject.CurvePositionAt(completionProgress + 0.01f);

            bool rewinding = (Clock as IGameplayClock)?.IsRewinding == true;

            // Ensure the value is substantially high enough to allow for Atan2 to get a valid angle.
            if (diff.LengthFast < 0.01f)
                return;

            ball.Rotation = -90 + (float)(-Math.Atan2(diff.X, diff.Y) * 180 / Math.PI) + (rewinding ? 180 : 0);
            lastPosition = Position;
        }
    }
}
