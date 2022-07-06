// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyFollowCircle : CompositeDrawable
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
    }
}
