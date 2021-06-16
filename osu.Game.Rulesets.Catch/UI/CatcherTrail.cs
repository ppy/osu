// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Timing;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// A trail of the catcher.
    /// It also represents a hyper dash afterimage.
    /// </summary>
    public class CatcherTrail : PoolableDrawable
    {
        public CatcherAnimationState AnimationState
        {
            set => body.AnimationState.Value = value;
        }

        private readonly SkinnableCatcher body;

        public CatcherTrail()
        {
            Size = new Vector2(CatcherArea.CATCHER_SIZE);
            Origin = Anchor.TopCentre;
            Blending = BlendingParameters.Additive;
            InternalChild = body = new SkinnableCatcher
            {
                // Using a frozen clock because trails should not be animated when the skin has an animated catcher.
                // TODO: The animation should be frozen at the animation frame at the time of the trail generation.
                Clock = new FramedClock(new ManualClock()),
            };
        }

        protected override void FreeAfterUse()
        {
            ClearTransforms();
            base.FreeAfterUse();
        }
    }
}
