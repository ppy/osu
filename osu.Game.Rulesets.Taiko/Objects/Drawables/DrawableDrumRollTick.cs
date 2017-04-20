// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using OpenTK.Input;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject<DrumRollTick>
    {
        public DrawableDrumRollTick(DrumRollTick tick)
            : base(tick)
        {
        }

        protected override TaikoPiece CreateMainPiece() => new TickPiece
        {
            Filled = HitObject.FirstTick
        };

        protected override TaikoJudgement CreateJudgement() => new TaikoDrumRollTickJudgement { SecondHit = HitObject.IsStrong };

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
                return;

            if (Math.Abs(Judgement.TimeOffset) < HitObject.HitWindow)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.TaikoResult = TaikoHitResult.Great;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    Content.ScaleTo(0, 100, EasingTypes.OutQuint);
                    break;
            }
        }

        protected override void UpdateScrollPosition(double time)
        {
            // Ticks don't move
        }

        protected override bool HandleKeyPress(Key key)
        {
            return Judgement.Result == HitResult.None && UpdateJudgement(true);
        }
    }
}
