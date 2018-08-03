// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableStrongDrumRoll : DrawableStrongHitObject
    {
        private readonly DrawableDrumRoll drumRoll;

        public DrawableStrongDrumRoll(StrongHitObject strong, DrawableDrumRoll drumRoll)
            : base(strong)
        {
            this.drumRoll = drumRoll;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!drumRoll.Judged)
                return;

            ApplyResult(r => r.Type = drumRoll.IsHit ? HitResult.Great : HitResult.Miss);
        }

        public override bool OnPressed(TaikoAction action) => false;
    }
}
