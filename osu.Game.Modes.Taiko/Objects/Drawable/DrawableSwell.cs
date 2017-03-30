// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using System;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableSwell : DrawableTaikoHitObject
    {
        /// <summary>
        /// The amount of times the user has hit this swell.
        /// </summary>
        private int userHits;

        private readonly Swell swell;

        public DrawableSwell(Swell swell)
            : base(swell)
        {
            this.swell = swell;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (userTriggered)
            {
                if (Time.Current < HitObject.StartTime)
                    return;

                userHits++;

                if (userHits == swell.RequiredHits)
                {
                    Judgement.Result = HitResult.Hit;
                    Judgement.TaikoResult = TaikoHitResult.Great;
                }
            }
            else
            {
                if (Judgement.TimeOffset < 0)
                    return;

                if (userHits > swell.RequiredHits / 2)
                {
                    Judgement.Result = HitResult.Hit;
                    Judgement.TaikoResult = TaikoHitResult.Good;
                }
                else
                    Judgement.Result = HitResult.Miss;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
        }

        protected override void UpdateScrollPosition(double time)
        {
            base.UpdateScrollPosition(Math.Min(time, HitObject.StartTime));
        }

        protected override bool HandleKeyPress(Key key)
        {
            if (Judgement.Result.HasValue)
                return false;

            UpdateJudgement(true);

            return true;
        }
    }
}
