// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherTrailSprite : PoolableDrawable
    {
        public CatcherAnimationState AnimationState
        {
            set => body.AnimationState.Value = value;
        }

        private readonly SkinnableCatcher body;

        public CatcherTrailSprite()
        {
            Size = new Vector2(CatcherArea.CATCHER_SIZE);
            Origin = Anchor.TopCentre;
            Blending = BlendingParameters.Additive;
            InternalChild = body = new SkinnableCatcher();
        }

        protected override void FreeAfterUse()
        {
            ClearTransforms();
            base.FreeAfterUse();
        }
    }
}
