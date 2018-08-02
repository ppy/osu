// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableHitStrong : DrawableHit
    {
        /// <summary>
        /// The lenience for the second key press.
        /// This does not adjust by map difficulty in ScoreV2 yet.
        /// </summary>
        private const double second_hit_window = 30;

        private readonly JudgementResult strongResult;

        private double firstHitTime;
        private bool firstKeyHeld;
        private TaikoAction firstHitAction;

        protected DrawableHitStrong(Hit hit)
            : base(hit)
        {
            strongResult = Results.SingleOrDefault(r => r.Judgement is TaikoStrongHitJudgement);
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!Result.HasResult)
            {
                base.CheckForJudgements(userTriggered, timeOffset);
                return;
            }

            if (!Result.IsHit)
            {
                ApplyResult(strongResult, r => r.Type = HitResult.Miss);
                return;
            }

            if (!userTriggered)
            {
                if (timeOffset > second_hit_window)
                    ApplyResult(strongResult, r => r.Type = HitResult.Miss);
                return;
            }

            if (Math.Abs(firstHitTime - Time.Current) < second_hit_window)
                ApplyResult(strongResult, r => r.Type = HitResult.Great);
        }

        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            switch (state)
            {
                case ArmedState.Idle:
                    firstHitTime = 0;
                    firstKeyHeld = false;
                    break;
            }
        }

        public override bool OnReleased(TaikoAction action)
        {
            if (action == firstHitAction)
                firstKeyHeld = false;
            return base.OnReleased(action);
        }

        public override bool OnPressed(TaikoAction action)
        {
            if (AllJudged)
                return false;

            // Check if we've handled the first key
            if (!Result.HasResult)
            {
                // First key hasn't been handled yet, attempt to handle it
                bool handled = base.OnPressed(action);

                if (handled)
                {
                    firstHitTime = Time.Current;
                    firstHitAction = action;
                    firstKeyHeld = true;
                }

                return handled;
            }

            // Don't handle represses of the first key
            if (firstHitAction == action)
                return false;

            // Don't handle invalid hit action presses
            if (!HitActions.Contains(action))
                return false;

            // Assume the intention was to hit the strong hit with both keys only if the first key is still being held down
            return firstKeyHeld && UpdateJudgement(true);
        }
    }
}
