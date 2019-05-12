// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableTinyDroplet : DrawableDroplet
    {
        public DrawableTinyDroplet(TinyDroplet h)
            : base(h)
        {
            Size = new Vector2((float)CatchHitObject.OBJECT_RADIUS) / 8;
        }
    }
}
