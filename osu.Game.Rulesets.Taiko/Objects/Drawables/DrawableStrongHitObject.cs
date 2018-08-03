// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableStrongHitObject : DrawableTaikoHitObject
    {
        public override bool DisplayJudgement => false;

        public readonly DrawableHitObject MainObject;

        protected DrawableStrongHitObject(StrongHitObject strong, DrawableHitObject mainObject)
            : base(strong)
        {
            MainObject = mainObject;

            AlwaysPresent = true;
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
