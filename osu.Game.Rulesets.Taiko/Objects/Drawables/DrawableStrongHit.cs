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

        private readonly DrawableHit hit;

        public DrawableStrongHit(StrongHitObject strong, DrawableHit hit)
            : base(strong)
        {
            this.hit = hit;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!hit.Result.HasResult)
            {
                base.CheckForJudgements(userTriggered, timeOffset);
                return;
            }

            if (!hit.Result.IsHit)
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

            if (Math.Abs(hit.Result.TimeOffset - timeOffset) < second_hit_window)
                ApplyResult(r => r.Type = HitResult.Great);
        }

        public override bool OnPressed(TaikoAction action)
        {
            // Don't process actions until the main hitobject is hit
            if (!hit.IsHit)
                return false;

            // Don't process actions if the pressed button was released
            if (hit.HitAction == null)
                return false;

            // Don't handle invalid hit action presses
            if (!hit.HitActions.Contains(action))
                return false;

            return UpdateJudgement(true);
        }
    }
}
