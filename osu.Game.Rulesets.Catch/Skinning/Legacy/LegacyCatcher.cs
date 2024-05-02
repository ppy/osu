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

            // in stable, catcher sprites are displayed in their raw size. stable also has catcher sprites displayed with the following scale factors applied:
            //  1. 0.5x, affecting all sprites in the playfield, computed here based on lazer's catch playfield dimensions (see WIDTH/HEIGHT constants in CatchPlayfield),
            //           source: https://github.com/peppy/osu-stable-reference/blob/1531237b63392e82c003c712faa028406073aa8f/osu!/GameplayElements/HitObjectManager.cs#L483-L494
            //  2. 0.7x, a constant scale applied to all catcher sprites on construction.
            AutoSizeAxes = Axes.Both;
            Scale = new Vector2(0.5f * 0.7f);
        }

        protected override void Update()
        {
            base.Update();

            // stable sets the Y origin position of the catcher to 16px in order for the catching range and OD scaling to align with the top of the catcher's plate in the default skin.
            OriginPosition = new Vector2(DrawWidth / 2, 16f);
        }
    }
}
