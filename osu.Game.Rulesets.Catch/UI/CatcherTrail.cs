// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects.Pooling;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// A trail of the catcher.
    /// It also represents a hyper dash afterimage.
    /// </summary>
    public partial class CatcherTrail : PoolableDrawableWithLifetime<CatcherTrailEntry>
    {
        private readonly SkinnableCatcher body;

        public CatcherTrail()
        {
            Size = new Vector2(Catcher.BASE_SIZE);
            Origin = Anchor.TopCentre;
            Blending = BlendingParameters.Additive;
            InternalChild = body = new SkinnableCatcher
            {
                // Using a frozen clock because trails should not be animated when the skin has an animated catcher.
                // TODO: The animation should be frozen at the animation frame at the time of the trail generation.
                Clock = new FramedClock(new ManualClock()),
            };
        }

        protected override void OnApply(CatcherTrailEntry entry)
        {
            Position = new Vector2(entry.Position, 0);
            Scale = entry.Scale;

            body.AnimationState.Value = entry.CatcherState;

            using (BeginAbsoluteSequence(entry.LifetimeStart, false))
                applyTransforms(entry.Animation);
        }

        protected override void OnFree(CatcherTrailEntry entry)
        {
            ApplyTransformsAt(double.MinValue);
            ClearTransforms();
        }

        private void applyTransforms(CatcherTrailAnimation animation)
        {
            switch (animation)
            {
                case CatcherTrailAnimation.Dashing:
                case CatcherTrailAnimation.HyperDashing:
                    this.FadeTo(0.4f).FadeOut(800, Easing.OutQuint);
                    break;

                case CatcherTrailAnimation.HyperDashAfterImage:
                    this.MoveToOffset(new Vector2(0, -10), 1200, Easing.In);
                    this.ScaleTo(Scale * 0.95f).ScaleTo(Scale * 1.2f, 1200, Easing.In);
                    this.FadeOut(1200);
                    break;
            }

            Expire();
        }
    }
}
