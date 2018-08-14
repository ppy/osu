// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

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
