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
            AutoSizeAxes = Axes.Both;

            // stable applies a 0.5x factor to all gamefield sprites (assuming game field is 512x384),
            // and also a constant 0.7x factor for catcher sprites specifically.
            Scale = new Vector2(0.5f * 0.7f);
        }
    }
}
