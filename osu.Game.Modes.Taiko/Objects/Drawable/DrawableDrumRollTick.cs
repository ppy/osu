// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Modes.Taiko.Judgements;
using System;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject
    {
        private readonly DrumRollTick tick;

        public DrawableDrumRollTick(DrumRollTick tick)
            : base(tick)
        {
            this.tick = tick;
        }

        protected override TaikoJudgement CreateJudgement() => new TaikoDrumRollTickJudgement();

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > tick.HitWindow)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            if (Math.Abs(Judgement.TimeOffset) < tick.HitWindow)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.TaikoResult = TaikoHitResult.Great;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
        }

        protected override void UpdateScrollPosition(double time)
        {
            // Drum roll ticks shouldn't move
        }

        protected override bool HandleKeyPress(Key key)
        {
            return Judgement.Result == HitResult.None && UpdateJudgement(true);
        }
    }
}
