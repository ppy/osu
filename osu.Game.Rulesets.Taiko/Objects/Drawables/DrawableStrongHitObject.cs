// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableStrongHitObject : DrawableTaikoHitObject
    {
        protected DrawableStrongHitObject(StrongHitObject strong)
            : base(strong)
        {
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
