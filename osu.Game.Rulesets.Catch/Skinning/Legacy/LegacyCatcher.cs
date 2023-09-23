// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public abstract partial class LegacyCatcher : CompositeDrawable
    {
        protected LegacyCatcher()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.Both;
        }

        protected override void Update()
        {
            base.Update();

            // stable sets the Y origin position of the catcher to 16px in order for the catching range and OD scaling to align with the top of the catcher's plate in the default skin.
            OriginPosition = new Vector2(DrawWidth / 2, 16f);
        }
    }
}
