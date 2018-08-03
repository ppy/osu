// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableStrongDrumRollTick : DrawableStrongHitObject
    {
        private readonly DrawableDrumRollTick tick;

        public DrawableStrongDrumRollTick(StrongHitObject strong, DrawableDrumRollTick tick)
            : base(strong)
        {
            this.tick = tick;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!tick.Judged)
                return;

            ApplyResult(r => r.Type = tick.IsHit ? HitResult.Great : HitResult.Miss);
        }

        public override bool OnPressed(TaikoAction action) => false;
    }
}
