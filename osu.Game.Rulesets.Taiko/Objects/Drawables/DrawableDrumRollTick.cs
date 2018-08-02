// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject<DrumRollTick>
    {
        private readonly JudgementResult result;
        private readonly JudgementResult strongResult;

        public DrawableDrumRollTick(DrumRollTick tick)
            : base(tick)
        {
            FillMode = FillMode.Fit;

            result = Results.Single(r => !(r.Judgement is TaikoStrongHitJudgement));
            strongResult = Results.SingleOrDefault(r => r.Judgement is TaikoStrongHitJudgement);
        }

        public override bool DisplayJudgement => false;

        protected override TaikoPiece CreateMainPiece() => new TickPiece
        {
            Filled = HitObject.FirstTick
        };

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (timeOffset > HitObject.HitWindow)
                {
                    ApplyResult(result, r => r.Type = HitResult.Miss);
                    if (HitObject.IsStrong)
                        ApplyResult(strongResult, r => r.Type = HitResult.Miss);
                }

                return;
            }

            if (Math.Abs(timeOffset) > HitObject.HitWindow)
                return;

            ApplyResult(result, r => r.Type = HitResult.Great);
            if (HitObject.IsStrong)
                ApplyResult(strongResult, r => r.Type = HitResult.Great);
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(0, 100, Easing.OutQuint).Expire();
                    break;
            }
        }

        public override bool OnPressed(TaikoAction action) => UpdateJudgement(true);
    }
}
