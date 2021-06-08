// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
}

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherSprite : SkinnableDrawable
    {
        protected override bool ApplySizeRestrictionsToDefault => true;

        public CatcherSprite(CatcherAnimationState state)
            : base(new CatchSkinComponent(componentFromState(state)), _ =>
                new DefaultCatcherSprite(state), confineMode: ConfineMode.ScaleToFit)
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(CatcherArea.CATCHER_SIZE);

            // Sets the origin roughly to the centre of the catcher's plate to allow for correct scaling.
            OriginPosition = new Vector2(0.5f, 0.06f) * CatcherArea.CATCHER_SIZE;
        }

        private static CatchSkinComponents componentFromState(CatcherAnimationState state)
        {
            switch (state)
            {
                case CatcherAnimationState.Fail:
                    return CatchSkinComponents.CatcherFail;

                case CatcherAnimationState.Kiai:
                    return CatchSkinComponents.CatcherKiai;

                default:
                    return CatchSkinComponents.CatcherIdle;
            }
        }
    }
}
