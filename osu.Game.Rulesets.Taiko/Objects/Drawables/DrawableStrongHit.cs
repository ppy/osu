// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableStrongHit : DrawableStrongHitObject
    {
        /// <summary>
        /// The lenience for the second key press.
        /// This does not adjust by map difficulty in ScoreV2 yet.
        /// </summary>
        private const double second_hit_window = 30;

        public DrawableHit MainObject => (DrawableHit)base.MainObject;

        public DrawableStrongHit(StrongHitObject strong, DrawableHit hit)
            : base(strong, hit)
        {
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!MainObject.Result.HasResult)
            {
                base.CheckForJudgements(userTriggered, timeOffset);
                return;
            }

            if (!MainObject.Result.IsHit)
            {
                ApplyResult(r => r.Type = HitResult.Miss);
                return;
            }

            if (!userTriggered)
            {
                if (timeOffset > second_hit_window)
                    ApplyResult(r => r.Type = HitResult.Miss);
                return;
            }

            if (Math.Abs(MainObject.Result.TimeOffset - timeOffset) < second_hit_window)
                ApplyResult(r => r.Type = HitResult.Great);
        }

        public override bool OnPressed(TaikoAction action)
        {
            // Don't process actions until the main hitobject is hit
            if (!MainObject.IsHit)
                return false;

            // Don't process actions if the pressed button was released
            if (MainObject.HitAction == null)
                return false;

            // Don't handle invalid hit action presses
            if (!MainObject.HitActions.Contains(action))
                return false;

            return UpdateJudgement(true);
        }
    }
}
