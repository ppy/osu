// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// Used as a nested hitobject to provide <see cref="TaikoStrongJudgement"/>s for <see cref="DrawableTaikoHitObject"/>s.
    /// </summary>
    public abstract class DrawableStrongNestedHit : DrawableTaikoHitObject
    {
        public readonly DrawableHitObject MainObject;

        protected DrawableStrongNestedHit(StrongHitObject strong, DrawableHitObject mainObject)
            : base(strong)
        {
            MainObject = mainObject;
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
